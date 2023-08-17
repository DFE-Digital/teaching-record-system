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
        private bool? _hasTrn;

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
            var lastName = testData.GenerateLastName();
            var dateOfBirth = testData.GenerateDateOfBirth();

            var personId = Guid.NewGuid();

            var contact = new Contact()
            {
                Id = personId,
                FirstName = firstName,
                LastName = lastName,
                BirthDate = dateOfBirth.ToDateTime(new TimeOnly()),
                dfeta_TRN = trn
            };

            await testData.OrganizationService.CreateAsync(contact);

            return new CreatePersonResult()
            {
                PersonId = personId,
                Trn = trn,
                DateOfBirth = dateOfBirth,
                FirstName = firstName,
                LastName = lastName
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
        public required string LastName { get; init; }
    }
}
