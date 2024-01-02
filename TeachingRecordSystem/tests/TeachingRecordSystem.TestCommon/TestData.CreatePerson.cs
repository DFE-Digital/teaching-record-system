using System.Collections.Immutable;
using Microsoft.Xrm.Sdk.Messages;
using Optional;
using Optional.Unsafe;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Models;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public Task<CreatePersonResult> CreatePerson(Action<CreatePersonBuilder>? configure = null)
    {
        var builder = new CreatePersonBuilder();
        configure?.Invoke(builder);
        return builder.Execute(this);
    }

    public class CreatePersonBuilder
    {
        private const string TeacherStatusQualifiedTeacherTrained = "71";
        private const string EaryYearsStatusProfessionalStatus = "222";

        private bool? _syncEnabledOverride;
        private DateOnly? _dateOfBirth;
        private bool? _hasTrn;
        private string? _firstName;
        private string? _middleName;
        private string? _lastName;
        private string? _email;
        private string? _mobileNumber;
        private Contact_GenderCode? _gender;
        private bool? _hasNationalInsuranceNumber;
        private DateOnly? _qtsDate;
        private string? _teacherStatus;
        private DateOnly? _eytsDate;
        private string? _earlyYearsStatus;
        private readonly List<Sanction> _sanctions = [];
        private readonly List<CreatePersonMandatoryQualificationBuilder> _mqBuilders = [];

        public Guid PersonId { get; } = Guid.NewGuid();

        public CreatePersonBuilder WithSyncOverride(bool enabled)
        {
            _syncEnabledOverride = enabled;
            return this;
        }

        public CreatePersonBuilder WithDateOfBirth(DateOnly dateOfBirth)
        {
            if (_dateOfBirth is not null && _dateOfBirth != dateOfBirth)
            {
                throw new InvalidOperationException("WithDateOfBirth cannot be changed after it's set.");
            }

            _dateOfBirth = dateOfBirth;
            return this;
        }

        public CreatePersonBuilder WithFirstName(string firstName)
        {
            if (_firstName is not null && _firstName != firstName)
            {
                throw new InvalidOperationException("WithFirstName cannot be changed after it's set.");
            }

            _firstName = firstName;
            return this;
        }

        public CreatePersonBuilder WithMiddleName(string middleName)
        {
            if (_middleName is not null && _middleName != middleName)
            {
                throw new InvalidOperationException("WithMiddleName cannot be changed after it's set.");
            }

            _middleName = middleName;
            return this;
        }

        public CreatePersonBuilder WithLastName(string lastName)
        {
            if (_lastName is not null && _lastName != lastName)
            {
                throw new InvalidOperationException("WithLastName cannot be changed after it's set.");
            }

            _lastName = lastName;
            return this;
        }

        public CreatePersonBuilder WithEmail(string email)
        {
            if (_email is not null && _email != email)
            {
                throw new InvalidOperationException("WithEmail cannot be changed after it's set.");
            }

            _email = email;
            return this;
        }

        public CreatePersonBuilder WithMobileNumber(string mobileNumber)
        {
            if (_mobileNumber is not null && _mobileNumber != mobileNumber)
            {
                throw new InvalidOperationException("WithMobileNumber cannot be changed after it's set.");
            }

            _mobileNumber = mobileNumber;
            return this;
        }

        public CreatePersonBuilder WithSanction(
            string sanctionCode,
            DateOnly? startDate = null,
            DateOnly? endDate = null,
            DateOnly? reviewDate = null,
            bool spent = false,
            string? details = null,
            string? detailsLink = null,
            bool isActive = true)
        {
            _sanctions.Add(new(Guid.NewGuid(), sanctionCode, startDate, endDate, reviewDate, spent, details, detailsLink, isActive));
            return this;
        }

        public CreatePersonBuilder WithMandatoryQualification(Action<CreatePersonMandatoryQualificationBuilder>? configure = null)
        {
            var mqBuilder = new CreatePersonMandatoryQualificationBuilder();
            configure?.Invoke(mqBuilder);
            _mqBuilders.Add(mqBuilder);
            return this;
        }

        public CreatePersonBuilder WithTrn(bool hasTrn = true)
        {
            if (_hasTrn is not null && _hasTrn != hasTrn)
            {
                throw new InvalidOperationException("WithTrn cannot be changed after it's set.");
            }

            _hasTrn = hasTrn;
            return this;
        }

        public CreatePersonBuilder WithGender(Contact_GenderCode gender)
        {
            if (_gender is not null && _gender != gender)
            {
                throw new InvalidOperationException("WithGender cannot be changed after it's set.");
            }

            _gender = gender;
            return this;
        }

        public CreatePersonBuilder WithNationalInsuranceNumber(bool? hasNationalInsuranceNumber = true)
        {
            if (_hasNationalInsuranceNumber is not null && _hasNationalInsuranceNumber != hasNationalInsuranceNumber)
            {
                throw new InvalidOperationException("WithNationalInsuranceNumber cannot be changed after it's set.");
            }

            _hasNationalInsuranceNumber = hasNationalInsuranceNumber;
            return this;
        }

        public CreatePersonBuilder WithQts(DateOnly qtsDate, string teacherStatus = TeacherStatusQualifiedTeacherTrained)
        {
            if ((_qtsDate is not null && _qtsDate != qtsDate) || (_teacherStatus is not null && _teacherStatus != teacherStatus))
            {
                throw new InvalidOperationException("WithQts cannot be changed after it's set.");
            }

            _qtsDate = qtsDate;
            _teacherStatus = teacherStatus;
            return this;
        }

        public CreatePersonBuilder WithEyts(DateOnly eytsDate, string earlyYearsStatus = EaryYearsStatusProfessionalStatus)
        {
            if ((_eytsDate is not null && _eytsDate != eytsDate) || (_earlyYearsStatus is not null && _earlyYearsStatus != earlyYearsStatus))
            {
                throw new InvalidOperationException("WithEyts cannot be changed after it's set.");
            }

            _eytsDate = eytsDate;
            _earlyYearsStatus = earlyYearsStatus;
            return this;
        }

        internal async Task<CreatePersonResult> Execute(TestData testData)
        {
            var hasTrn = _hasTrn ?? true;
            var trn = hasTrn ? await testData.GenerateTrn() : null;
            var statedFirstName = _firstName ?? testData.GenerateFirstName();
            var statedMiddleName = _middleName ?? testData.GenerateMiddleName();
            var firstAndMiddleNames = $"{statedFirstName} {statedMiddleName}".Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var firstName = firstAndMiddleNames.First();
            var middleName = string.Join(" ", firstAndMiddleNames.Skip(1));
            var lastName = _lastName ?? testData.GenerateLastName();
            var dateOfBirth = _dateOfBirth ?? testData.GenerateDateOfBirth();
            var gender = _gender ?? testData.GenerateGender();

            var contact = new Contact()
            {
                Id = PersonId,
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                dfeta_StatedFirstName = statedFirstName,
                dfeta_StatedMiddleName = statedMiddleName,
                dfeta_StatedLastName = lastName,
                BirthDate = dateOfBirth.ToDateTime(new TimeOnly()),
                dfeta_TRN = trn,
                GenderCode = gender
            };

            if (_email is not null)
            {
                contact.EMailAddress1 = _email;
            }

            if (_mobileNumber is not null)
            {
                contact.MobilePhone = _mobileNumber;
            }

            if (_hasNationalInsuranceNumber ?? false)
            {
                contact.dfeta_NINumber = testData.GenerateNationalInsuranceNumber();
            }

            var txnRequestBuilder = RequestBuilder.CreateTransaction(testData.OrganizationService);
            txnRequestBuilder.AddRequest(new CreateRequest() { Target = contact });

            if (_qtsDate is not null && _teacherStatus is not null)
            {
                var teacherStatus = await testData.ReferenceDataCache.GetTeacherStatusByValue(_teacherStatus);
                var qtsRegistrationId = Guid.NewGuid();
                txnRequestBuilder.AddRequest(new CreateRequest()
                {
                    Target = new dfeta_qtsregistration()
                    {
                        Id = qtsRegistrationId,
                        dfeta_PersonId = PersonId.ToEntityReference(Contact.EntityLogicalName)
                    }
                });

                txnRequestBuilder.AddRequest(new CreateRequest()
                {
                    Target = new dfeta_initialteachertraining()
                    {
                        dfeta_PersonId = PersonId.ToEntityReference(Contact.EntityLogicalName),
                        dfeta_Result = dfeta_ITTResult.Pass,
                    }
                });

                // Plugin which updates Contact with QTS Date only fires on Update or Delete
                txnRequestBuilder.AddRequest(new UpdateRequest()
                {
                    Target = new dfeta_qtsregistration()
                    {
                        Id = qtsRegistrationId,
                        dfeta_QTSDate = _qtsDate.Value.ToDateTimeWithDqtBstFix(isLocalTime: true),
                        dfeta_TeacherStatusId = teacherStatus.Id.ToEntityReference(dfeta_teacherstatus.EntityLogicalName),
                    }
                });
            }

            if (_eytsDate is not null && _earlyYearsStatus is not null)
            {
                var earlyYearsStatus = await testData.ReferenceDataCache.GetEarlyYearsStatusByValue(_earlyYearsStatus);
                var eytsRegistrationId = Guid.NewGuid();
                txnRequestBuilder.AddRequest(new CreateRequest()
                {
                    Target = new dfeta_qtsregistration()
                    {
                        Id = eytsRegistrationId,
                        dfeta_PersonId = PersonId.ToEntityReference(Contact.EntityLogicalName)
                    }
                });

                // Plugin which updates Contact with EYTS Date only fires on Update or Delete
                txnRequestBuilder.AddRequest(new UpdateRequest()
                {
                    Target = new dfeta_qtsregistration()
                    {
                        Id = eytsRegistrationId,
                        dfeta_EYTSDate = _eytsDate.Value.ToDateTimeWithDqtBstFix(isLocalTime: true),
                        dfeta_EarlyYearsStatusId = earlyYearsStatus.Id.ToEntityReference(dfeta_earlyyearsstatus.EntityLogicalName),
                    }
                });
            }

            foreach (var sanction in _sanctions)
            {
                var sanctionCode = await testData.ReferenceDataCache.GetSanctionCodeByValue(sanction.SanctionCode);
                var crmSanction = new dfeta_sanction()
                {
                    Id = sanction.SanctionId,
                    dfeta_PersonId = PersonId.ToEntityReference(Contact.EntityLogicalName),
                    dfeta_SanctionCodeId = sanctionCode.Id.ToEntityReference(dfeta_sanctioncode.EntityLogicalName),
                    dfeta_StartDate = sanction.StartDate?.ToDateTimeWithDqtBstFix(isLocalTime: true),
                    dfeta_EndDate = sanction.EndDate?.ToDateTimeWithDqtBstFix(isLocalTime: true),
                    dfeta_NoReAppuntildate = sanction.ReviewDate?.ToDateTimeWithDqtBstFix(isLocalTime: true),
                    dfeta_Spent = sanction.Spent,
                    dfeta_SanctionDetails = sanction.Details
                };

                if (!string.IsNullOrWhiteSpace(sanction.DetailsLink))
                {
                    crmSanction.dfeta_DetailsLink = sanction.DetailsLink;
                }

                txnRequestBuilder.AddRequest(new CreateRequest()
                {
                    Target = crmSanction
                });

                if (!sanction.IsActive)
                {
                    txnRequestBuilder.AddRequest(new UpdateRequest()
                    {
                        Target = new dfeta_sanction()
                        {
                            Id = sanction.SanctionId,
                            StateCode = dfeta_sanctionState.Inactive
                        }
                    });
                }
            }

            var mqs = new List<MandatoryQualificationInfo>();
            foreach (var mqBuilder in _mqBuilders)
            {
                mqs.Add(await mqBuilder.AppendRequests(this, testData, txnRequestBuilder));
            }

            var retrieveContactHandle = txnRequestBuilder.AddRequest<RetrieveResponse>(new RetrieveRequest()
            {
                ColumnSet = new(allColumns: true),
                Target = PersonId.ToEntityReference(Contact.EntityLogicalName)
            });

            await txnRequestBuilder.Execute();

            // Read the contact record back (plugins may have added/amended data so our original record will be stale)
            contact = retrieveContactHandle.GetResponse().Entity.ToEntity<Contact>();

            await testData.SyncConfiguration.SyncIfEnabled(
                helper => helper.SyncPerson(contact, ignoreInvalid: false, CancellationToken.None),
                _syncEnabledOverride);

            return new CreatePersonResult()
            {
                PersonId = PersonId,
                Contact = contact,
                Trn = trn,
                DateOfBirth = dateOfBirth,
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                StatedFirstName = statedFirstName,
                StatedMiddleName = statedMiddleName,
                StatedLastName = lastName,
                Email = _email,
                MobileNumber = _mobileNumber,
                Gender = gender.ToString(),
                NationalInsuranceNumber = contact.dfeta_NINumber,
                QtsDate = _qtsDate,
                EytsDate = _eytsDate,
                Sanctions = [.. _sanctions],
                MandatoryQualifications = [.. mqs]
            };
        }
    }

    public class CreatePersonMandatoryQualificationBuilder
    {
        private Option<string?> _dqtMqEstablishmentValue;
        private Option<MandatoryQualificationSpecialism?> _specialism;
        private Option<MandatoryQualificationStatus?> _status;
        private Option<DateOnly?> _startDate;
        private Option<DateOnly?> _endDate;

        public CreatePersonMandatoryQualificationBuilder WithDqtMqEstablishmentValue(string? value)
        {
            _dqtMqEstablishmentValue = Option.Some(value);
            return this;
        }

        public CreatePersonMandatoryQualificationBuilder WithSpecialism(MandatoryQualificationSpecialism? specialism)
        {
            _specialism = Option.Some(specialism);
            return this;
        }

        public CreatePersonMandatoryQualificationBuilder WithStatus(MandatoryQualificationStatus? status)
        {
            _status = Option.Some(status);
            return this;
        }

        public CreatePersonMandatoryQualificationBuilder WithStartDate(DateOnly? startDate)
        {
            _startDate = Option.Some(startDate);
            return this;
        }

        public CreatePersonMandatoryQualificationBuilder WithEndDate(DateOnly? endDate)
        {
            _endDate = Option.Some(endDate);
            return this;
        }

        internal async Task<MandatoryQualificationInfo> AppendRequests(CreatePersonBuilder createPersonBuilder, TestData testData, RequestBuilder requestBuilder)
        {
            var mqEstablishments = await testData.ReferenceDataCache.GetMqEstablishments();

            var qualificationId = Guid.NewGuid();
            var personId = createPersonBuilder.PersonId;

            var dqtMqEstablishmentValue = _dqtMqEstablishmentValue.ValueOr(mqEstablishments.RandomOne().dfeta_Value);
            var specialism = _specialism.ValueOr(MandatoryQualificationSpecialismRegistry.All.Where(s => s.Title != "Deaf education").RandomOne().Value);  // Build env is missing Deaf education
            var status = _status.ValueOr(_endDate.ValueOrDefault() is DateOnly ? MandatoryQualificationStatus.Passed : MandatoryQualificationStatusRegistry.All.RandomOne().Value);
            var startDate = _startDate.ValueOr(testData.GenerateDate(min: new DateOnly(2000, 1, 1)));
            var endDate = _endDate.ValueOr(status == MandatoryQualificationStatus.Passed ? testData.GenerateDate(min: (startDate ?? new DateOnly(2000, 1, 1)).AddYears(1)) : null);

            var mqEstablishment = dqtMqEstablishmentValue is not null ?
                await testData.ReferenceDataCache.GetMqEstablishmentByValue(dqtMqEstablishmentValue) :
                null;

            var dqtSpecialism = specialism is not null ?
                await testData.ReferenceDataCache.GetMqSpecialismByValue(specialism.Value.GetDqtValue()) :
                null;

            var qualification = new dfeta_qualification()
            {
                Id = qualificationId,
                dfeta_PersonId = personId.ToEntityReference(Contact.EntityLogicalName),
                dfeta_Type = dfeta_qualification_dfeta_Type.MandatoryQualification,
                dfeta_MQ_MQEstablishmentId = mqEstablishment?.Id.ToEntityReference(dfeta_mqestablishment.EntityLogicalName),
                dfeta_MQ_SpecialismId = dqtSpecialism?.Id.ToEntityReference(dfeta_specialism.EntityLogicalName),
                dfeta_MQStartDate = startDate.ToDateTimeWithDqtBstFix(isLocalTime: true),
                dfeta_MQ_Date = endDate?.ToDateTimeWithDqtBstFix(isLocalTime: true),
                dfeta_MQ_Status = status?.GetDqtStatus()
            };

            requestBuilder.AddRequest(new CreateRequest()
            {
                Target = qualification
            });

            return new MandatoryQualificationInfo(
                qualificationId,
                dqtMqEstablishmentValue,
                specialism,
                status,
                startDate,
                endDate
            );
        }
    }

    public record CreatePersonResult
    {
        public required Guid PersonId { get; init; }
        public Guid ContactId => PersonId;
        public required Contact Contact { get; init; }
        public required string? Trn { get; init; }
        public required DateOnly DateOfBirth { get; init; }
        public required string FirstName { get; init; }
        public required string MiddleName { get; init; }
        public required string LastName { get; init; }
        public required string StatedFirstName { get; init; }
        public required string StatedMiddleName { get; init; }
        public required string StatedLastName { get; init; }
        public required string? Email { get; init; }
        public required string? MobileNumber { get; init; }
        public required string Gender { get; init; }
        public required string? NationalInsuranceNumber { get; init; }
        public required DateOnly? QtsDate { get; init; }
        public required DateOnly? EytsDate { get; init; }
        public required ImmutableArray<Sanction> Sanctions { get; init; }
        public required ImmutableArray<MandatoryQualificationInfo> MandatoryQualifications { get; init; }
    }

    public record Sanction(Guid SanctionId, string SanctionCode, DateOnly? StartDate, DateOnly? EndDate, DateOnly? ReviewDate, bool Spent, string? Details, string? DetailsLink, bool IsActive);

    public record MandatoryQualificationInfo(
        Guid QualificationId,
        string? DqtMqEstablishmentValue,
        MandatoryQualificationSpecialism? Specialism,
        MandatoryQualificationStatus? Status,
        DateOnly? StartDate,
        DateOnly? EndDate);
}
