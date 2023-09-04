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
        private string? _lastName;
        private string? _previousLastName;
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

        public CreatePersonBuilder WithLastName(string lastName)
        {
            if (_lastName is not null && _lastName != lastName)
            {
                throw new InvalidOperationException("WithLastName cannot be changed after it's set.");
            }

            _lastName = lastName;
            return this;
        }

        public CreatePersonBuilder WithPreviousLastName(string previousLastName)
        {
            if (_previousLastName is not null && _previousLastName != previousLastName)
            {
                throw new InvalidOperationException("WithPreviousLastName cannot be changed after it's set.");
            }

            _previousLastName = previousLastName;
            return this;
        }

        public CreatePersonBuilder WithSanction(
            string sanctionCode,
            DateOnly? startDate = null,
            DateOnly? endDate = null,
            DateOnly? reviewDate = null,
            bool spent = false)
        {
            _sanctions.Add(new(sanctionCode, startDate, endDate, reviewDate, spent));
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

        public async Task<CreatePersonResult> Execute(CrmTestData testData)
        {
            var hasTrn = _hasTrn ?? true;
            var trn = hasTrn ? await testData.GenerateTrn() : null;

            var firstName = testData.GenerateFirstName();
            var middleName = testData.GenerateMiddleName();
            var lastName = _lastName ?? testData.GenerateLastName();
            var dateOfBirth = _dateOfBirth ?? testData.GenerateDateOfBirth();

            var personId = Guid.NewGuid();

            var contact = new Contact()
            {
                Id = personId,
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                BirthDate = dateOfBirth.ToDateTime(new TimeOnly()),
                dfeta_TRN = trn
            };

            if (_previousLastName is not null)
            {
                contact.dfeta_PreviousLastName = _previousLastName;
            }

            var txnRequestBuilder = RequestBuilder.CreateTransaction(testData.OrganizationService);
            txnRequestBuilder.AddRequest(new CreateRequest() { Target = contact });

            foreach (var sanction in _sanctions)
            {
                var sanctionCode = await testData.ReferenceDataCache.GetSanctionCodeByValue(sanction.SanctionCode);

                txnRequestBuilder.AddRequest(new CreateRequest()
                {
                    Target = new dfeta_sanction()
                    {
                        dfeta_PersonId = personId.ToEntityReference(Contact.EntityLogicalName),
                        dfeta_SanctionCodeId = sanctionCode.Id.ToEntityReference(dfeta_sanctioncode.EntityLogicalName),
                        dfeta_StartDate = sanction.StartDate?.FromDateOnlyWithDqtBstFix(isLocalTime: true),
                        dfeta_EndDate = sanction.EndDate?.FromDateOnlyWithDqtBstFix(isLocalTime: true),
                        dfeta_NoReAppuntildate = sanction.ReviewDate?.FromDateOnlyWithDqtBstFix(isLocalTime: true),
                        dfeta_Spent = sanction.Spent
                    }
                });
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
                PreviousLastName = _previousLastName,
                Sanctions = _sanctions
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
        public required string? PreviousLastName { get; init; }
        public required IReadOnlyCollection<Sanction> Sanctions { get; init; }

        public Contact ToContact() => new()
        {
            Id = PersonId,
            FirstName = FirstName,
            MiddleName = MiddleName,
            LastName = LastName,
            dfeta_StatedFirstName = FirstName,
            dfeta_StatedMiddleName = MiddleName,
            dfeta_StatedLastName = LastName,
            BirthDate = DateOfBirth.FromDateOnlyWithDqtBstFix(isLocalTime: false),
            dfeta_TRN = Trn
        };
    }

    public record Sanction(string SanctionCode, DateOnly? StartDate, DateOnly? EndDate, DateOnly? ReviewDate, bool Spent);
}
