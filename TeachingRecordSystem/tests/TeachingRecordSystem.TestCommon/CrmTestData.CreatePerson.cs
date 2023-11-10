using System.Collections.Immutable;
using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.TestCommon;

public partial class CrmTestData
{
    public Task<CreatePersonResult> CreatePerson(Action<CreatePersonBuilder>? configure = null)
    {
        var builder = new CreatePersonBuilder();
        configure?.Invoke(builder);
        return builder.Execute(this);
    }

    public class CreatePersonBuilder
    {
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
        private DateOnly? _eytsDate;
        private readonly List<Sanction> _sanctions = new();

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
            string details = "lorem ipsum",
            string? detailsLink = null,
            bool isActive = true)
        {
            _sanctions.Add(new(Guid.NewGuid(), sanctionCode, startDate, endDate, reviewDate, spent, details, detailsLink, isActive));
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

        public CreatePersonBuilder WithQtsDate(DateOnly qtsDate)
        {
            if (_qtsDate is not null && _qtsDate != qtsDate)
            {
                throw new InvalidOperationException("WithQtsDate cannot be changed after it's set.");
            }

            _qtsDate = qtsDate;
            return this;
        }

        public CreatePersonBuilder WithEytsDate(DateOnly eytsDate)
        {
            if (_eytsDate is not null && _eytsDate != eytsDate)
            {
                throw new InvalidOperationException("WithEytsDate cannot be changed after it's set.");
            }

            _eytsDate = eytsDate;
            return this;
        }

        public async Task<CreatePersonResult> Execute(CrmTestData testData)
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

            var personId = Guid.NewGuid();

            var contact = new Contact()
            {
                Id = personId,
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

            if (_qtsDate is not null)
            {
                contact.dfeta_QTSDate = _qtsDate.Value.FromDateOnlyWithDqtBstFix(isLocalTime: true);
            }

            if (_eytsDate is not null)
            {
                contact.dfeta_EYTSDate = _eytsDate.Value.FromDateOnlyWithDqtBstFix(isLocalTime: true);
            }

            var txnRequestBuilder = RequestBuilder.CreateTransaction(testData.OrganizationService);
            txnRequestBuilder.AddRequest(new CreateRequest() { Target = contact });

            foreach (var sanction in _sanctions)
            {
                var sanctionCode = await testData.ReferenceDataCache.GetSanctionCodeByValue(sanction.SanctionCode);
                var crmSanction = new dfeta_sanction()
                {
                    Id = sanction.SanctionId,
                    dfeta_PersonId = personId.ToEntityReference(Contact.EntityLogicalName),
                    dfeta_SanctionCodeId = sanctionCode.Id.ToEntityReference(dfeta_sanctioncode.EntityLogicalName),
                    dfeta_StartDate = sanction.StartDate?.FromDateOnlyWithDqtBstFix(isLocalTime: true),
                    dfeta_EndDate = sanction.EndDate?.FromDateOnlyWithDqtBstFix(isLocalTime: true),
                    dfeta_NoReAppuntildate = sanction.ReviewDate?.FromDateOnlyWithDqtBstFix(isLocalTime: true),
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

            await txnRequestBuilder.Execute();

            return new CreatePersonResult()
            {
                PersonId = personId,
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
                Sanctions = _sanctions.ToImmutableArray()
            };
        }
    }

    public record CreatePersonResult
    {
        public required Guid PersonId { get; init; }
        public Guid ContactId => PersonId;
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

        public Contact ToContact() => new()
        {
            Id = PersonId,
            FirstName = FirstName,
            MiddleName = MiddleName,
            LastName = LastName,
            dfeta_StatedFirstName = StatedFirstName,
            dfeta_StatedMiddleName = StatedMiddleName,
            dfeta_StatedLastName = StatedLastName,
            BirthDate = DateOfBirth.FromDateOnlyWithDqtBstFix(isLocalTime: false),
            dfeta_TRN = Trn,
            EMailAddress1 = Email,
            MobilePhone = MobileNumber,
            dfeta_NINumber = NationalInsuranceNumber,
            dfeta_QTSDate = QtsDate?.FromDateOnlyWithDqtBstFix(isLocalTime: true),
            dfeta_EYTSDate = EytsDate?.FromDateOnlyWithDqtBstFix(isLocalTime: true),
            GenderCode = Enum.Parse<Contact_GenderCode>(Gender)
        };
    }

    public record Sanction(Guid SanctionId, string SanctionCode, DateOnly? StartDate, DateOnly? EndDate, DateOnly? ReviewDate, bool Spent, string Details, string? DetailsLink, bool IsActive);
}
