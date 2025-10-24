using System.Globalization;
using System.Transactions;
using CsvHelper;
using CsvHelper.Configuration;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.PersonMatching;
using TeachingRecordSystem.Core.Services.TrnGeneration;
using PersonCreatedEvent = TeachingRecordSystem.Core.Events.Legacy.PersonCreatedEvent;

namespace TeachingRecordSystem.Core.Jobs;

public class AllocateTrnsToOverseasNpqApplicantsJob(
    TrsDbContext dbContext,
    IPersonMatchingService personMatchingService,
    IBackgroundJobScheduler backgroundJobScheduler,
    ITrnGenerator trnGenerator,
    IImportFileStorageService fileStorageService,
    IClock clock)
{
    public const string ContainerName = "overseas-npq-applicants";
    public const string PendingFolderName = "pending";
    public const string ImportedFolderName = "imported";
    public const string OutputFolderName = "output";
    public const string OutputFileNamePrefix = "overseas-npq-applicants-allocated-trns-";
    public const string FirstNamePersonalisationKey = "first name";
    public const string LastNamePersonalisationKey = "last name";
    public const string TrnPersonalisationKey = "trn";

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        dbContext.Database.SetCommandTimeout(30);

        var pendingImportFileNames = await fileStorageService.GetFileNamesAsync(ContainerName, PendingFolderName, cancellationToken);
        if (pendingImportFileNames.Length == 0)
        {
            return;
        }

        var importId = Guid.NewGuid();
        using var txn = new TransactionScope(
            TransactionScopeOption.RequiresNew,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        var fileName = pendingImportFileNames[0];
        var fileNameParts = fileName.Split("/");
        var fileNameWithoutFolder = fileNameParts.Last();

        using var inputStream = await fileStorageService.GetFileAsync(ContainerName, fileName, cancellationToken);
        using var inputStreamReader = new StreamReader(inputStream);

        using var csvReader = new CsvReader(inputStreamReader, new CsvConfiguration(CultureInfo.CurrentCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
        });

        var now = clock.UtcNow;

        var inputRows = await csvReader.GetRecordsAsync<AllocateTrnsToOverseasNpqApplicantsInputRowRaw>().ToArrayAsync();
        var outputRows = new List<AllocateTrnsToOverseasNpqApplicantsOutputRowRaw>();
        foreach (var inputRow in inputRows)
        {
            List<string> errors = [];

            if (string.IsNullOrWhiteSpace(inputRow.FirstName))
            {
                errors.Add("First Name is required");
            }

            if (string.IsNullOrWhiteSpace(inputRow.LastName))
            {
                errors.Add("Last Name is required");
            }

            DateOnly? dateOfBirth = null;
            if (string.IsNullOrWhiteSpace(inputRow.DateOfBirth))
            {
                errors.Add("Date of Birth is required");
            }
            else if (DateOnly.TryParseExact(inputRow.DateOfBirth.Trim(), "dd/MM/yyyy", out var dob))
            {
                dateOfBirth = dob;
            }
            else
            {
                errors.Add("Date of Birth is in an incorrect format");
            }

            EmailAddress? email = null;
            if (string.IsNullOrWhiteSpace(inputRow.EmailAddress))
            {
                errors.Add("Email Address is required");
            }
            else if (!EmailAddress.TryParse(inputRow.EmailAddress.Trim(), out email))
            {
                errors.Add("Email Address is in an incorrect format");
            }

            NationalInsuranceNumber? nino = null;
            if (!string.IsNullOrWhiteSpace(inputRow.NationalInsuranceNumber) && !NationalInsuranceNumber.TryParse(inputRow.NationalInsuranceNumber.Trim(), out nino))
            {
                errors.Add("NI Number is in an incorrect format");
            }

            Gender? gender = null;
            var validGenderValues = new List<string>() { "Male", "Female", "Other" };
            if (string.IsNullOrWhiteSpace(inputRow.Gender))
            {
            }
            else if (validGenderValues.Contains(inputRow.Gender.Trim()) && Enum.TryParse<Gender>(inputRow.Gender, out var g))
            {
                gender = g;
            }
            else
            {
                errors.Add("Gender is in an incorrect format");
            }

            var outputRow = new AllocateTrnsToOverseasNpqApplicantsOutputRowRaw
            {
                FirstName = inputRow.FirstName,
                MiddleName = inputRow.MiddleName,
                LastName = inputRow.LastName,
                DateOfBirth = inputRow.DateOfBirth,
                EmailAddress = inputRow.EmailAddress,
                NationalInsuranceNumber = inputRow.NationalInsuranceNumber,
                Nationality = inputRow.Nationality,
                Gender = inputRow.Gender,
                ConfirmedStartedOrDueToStartNpq = inputRow.ConfirmedStartedOrDueToStartNpq
            };
            outputRows.Add(outputRow);

            outputRow.Errors = string.Join(",", errors);

            if (errors.Count > 0)
            {
                outputRow.Result = "Validation errors";
            }
            else
            {
                var firstName = inputRow.FirstName!.Trim();
                var middleName = inputRow.MiddleName?.Trim();
                var lastName = inputRow.LastName!.Trim();
                var applicationUserId = ApplicationUser.NpqApplicationUserGuid;
                var requestId = Guid.NewGuid().ToString();

                var trnRequestMetadata = new TrnRequestMetadata
                {
                    ApplicationUserId = applicationUserId,
                    RequestId = requestId,
                    CreatedOn = now,
                    IdentityVerified = null,
                    OneLoginUserSubject = null,
                    Name = new[] { firstName, middleName, lastName }.GetNonEmptyValues(),
                    FirstName = inputRow.FirstName,
                    MiddleName = inputRow.MiddleName,
                    LastName = inputRow.LastName,
                    DateOfBirth = dateOfBirth!.Value,
                    EmailAddress = email?.ToString(),
                    NationalInsuranceNumber = nino?.ToString(),
                    Gender = gender
                };

                var personMatchingResult = await personMatchingService.MatchFromTrnRequestAsync(trnRequestMetadata);

                switch (personMatchingResult.Outcome)
                {
                    case TrnRequestMatchResultOutcome.NoMatches:
                        var newTrn = await trnGenerator.GenerateTrnAsync();
                        outputRow.Result = "TRN allocated";
                        outputRow.AllocatedTrn = newTrn;

                        var person = Person.Create(
                            newTrn,
                            firstName!,
                            middleName ?? "",
                            lastName!,
                            dateOfBirth,
                            email,
                            nino,
                            gender,
                            now,
                            sourceTrnRequest: (applicationUserId, requestId));

                        dbContext.Persons.Add(person.Person);

                        dbContext.TrnRequestMetadata.Add(trnRequestMetadata);

                        dbContext.AddEventWithoutBroadcast(new PersonCreatedEvent
                        {
                            EventId = Guid.NewGuid(),
                            CreatedUtc = now,
                            RaisedBy = SystemUser.SystemUserId,
                            PersonId = person.Person.PersonId,
                            PersonAttributes = person.PersonAttributes,
                            CreateReason = null,
                            CreateReasonDetail = null,
                            EvidenceFile = null,
                            TrnRequestMetadata = EventModels.TrnRequestMetadata.FromModel(trnRequestMetadata),

                        });

                        var emailId = Guid.NewGuid();

                        dbContext.Emails.Add(new Email
                        {
                            EmailId = emailId,
                            TemplateId = EmailTemplateIds.TrnGeneratedForNpq,
                            EmailAddress = email!.ToString(),
                            Personalization = new Dictionary<string, string>()
                            {
                                [FirstNamePersonalisationKey] = firstName,
                                [LastNamePersonalisationKey] = lastName,
                                [TrnPersonalisationKey] = newTrn!,
                            },
                        });

                        await backgroundJobScheduler.EnqueueAsync<SendEmailJob>(x => x.ExecuteAsync(emailId));

                        break;

                    default:
                        outputRow.Result = "TRN not allocated due to potential duplicates";

                        var trns = await dbContext.Persons
                            .Where(p => personMatchingResult.PotentialMatchesPersonIds.Contains(p.PersonId))
                            .Select(p => p.Trn)
                            .OrderBy(trn => trn)
                            .ToArrayAsync();

                        outputRow.PotentialDuplicateTrns = string.Join(",", trns);
                        break;
                }
            }
        }

        await dbContext.SaveChangesAsync();
        txn.Complete();

        await fileStorageService.MoveFileAsync(ContainerName, fileName, ImportedFolderName, cancellationToken);

        using var outputStream = await fileStorageService.WriteFileAsync(ContainerName, $"{OutputFolderName}/{OutputFileNamePrefix}{now:yyyyMMddHHmmss}.csv", cancellationToken);
        using var outputStreamWriter = new StreamWriter(outputStream);
        using var csvWriter = new CsvWriter(outputStreamWriter, new CsvConfiguration(CultureInfo.CurrentCulture) { HasHeaderRecord = true });

        csvWriter.WriteHeader<AllocateTrnsToOverseasNpqApplicantsOutputRowRaw>();
        await csvWriter.NextRecordAsync();
        foreach (var row in outputRows)
        {
            csvWriter.WriteRecord(row);
            await csvWriter.NextRecordAsync();
            await csvWriter.FlushAsync();
        }
    }
}
