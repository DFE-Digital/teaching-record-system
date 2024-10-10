using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public Task<Account> CreateAccountAsync(Action<CreateAccountBuilder>? configure)
    {
        var builder = new CreateAccountBuilder();
        configure?.Invoke(builder);
        return builder.ExecuteAsync(this);
    }

    public class CreateAccountBuilder
    {
        private string? _name = null;
        private string? _accountnumber = null;

        public CreateAccountBuilder WithName(string name)
        {
            if (_name is not null && _name != name)
            {
                throw new InvalidOperationException("WithName cannot be changed after it's set.");
            }

            _name = name;
            return this;
        }

        public CreateAccountBuilder WithAccountNumber(string accountnumber)
        {
            if (_accountnumber is not null && _accountnumber != accountnumber)
            {
                throw new InvalidOperationException("WithAccountNumber cannot be changed after it's set.");
            }

            _accountnumber = accountnumber;
            return this;
        }

        public async Task<Account> ExecuteAsync(TestData testData)
        {
            var name = _name ?? Faker.Company.Name();
            var accountId = Guid.NewGuid();
            var account = new Account()
            {
                Id = accountId,
                Name = name,
                AccountNumber = _accountnumber
            };
            await testData.OrganizationService.ExecuteAsync(new CreateRequest()
            {
                Target = account
            });
            return account;
        }
    }
}
