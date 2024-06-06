using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public Task<Account> CreateAccount(Action<CreateAccountBuilder>? configure)
    {
        var builder = new CreateAccountBuilder();
        configure?.Invoke(builder);
        return builder.Execute(this);
    }

    public class CreateAccountBuilder
    {
        private string? _name = null;

        public CreateAccountBuilder WithName(string name)
        {
            if (_name is not null && _name != name)
            {
                throw new InvalidOperationException("WithName cannot be changed after it's set.");
            }

            _name = name;
            return this;
        }

        public async Task<Account> Execute(TestData testData)
        {
            var name = _name ?? Faker.Company.Name();
            var accountId = Guid.NewGuid();
            var account = new Account()
            {
                Id = accountId,
                Name = name
            };
            await testData.OrganizationService.ExecuteAsync(new CreateRequest()
            {
                Target = account
            });
            return account;
        }
    }
}
