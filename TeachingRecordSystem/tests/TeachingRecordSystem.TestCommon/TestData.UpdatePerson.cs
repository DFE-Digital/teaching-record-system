using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public Task UpdatePerson(Action<UpdatePersonBuilder>? configure)
    {
        var builder = new UpdatePersonBuilder();
        configure?.Invoke(builder);
        return builder.Execute(this);
    }

    public class UpdatePersonBuilder
    {
        private Guid? _personId = null;
        private (string FirstName, string? MiddleName, string LastName)? _updatedName = null;

        public UpdatePersonBuilder WithPersonId(Guid personId)
        {
            if (_personId is not null)
            {
                throw new InvalidOperationException("WithPersonId has already been set");
            }

            _personId = personId;
            return this;
        }

        public UpdatePersonBuilder WithUpdatedName(string firstName, string? middleName, string lastName)
        {
            if (_updatedName is not null)
            {
                throw new InvalidOperationException("WithUpdatedName has already been set");
            }

            _updatedName = (firstName, middleName, lastName);
            return this;
        }

        public async Task Execute(TestData testData)
        {
            if (_personId is null)
            {
                throw new InvalidOperationException("WithPersonId has not been set");
            }

            if (_updatedName is not null)
            {
                await testData.OrganizationService.ExecuteAsync(new UpdateRequest()
                {
                    Target = new Contact()
                    {
                        Id = _personId!.Value,
                        FirstName = _updatedName.Value.FirstName,
                        MiddleName = _updatedName.Value.MiddleName,
                        LastName = _updatedName.Value.LastName
                    }
                });
            }
        }
    }
}
