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
        private string _name = string.Empty;

        public CreateAccountBuilder WithName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new InvalidOperationException("Name has not been set");
            }
            _name = name;
            return this;
        }

        public async Task<Account> Execute(TestData testData)
        {
            var accountId = Guid.NewGuid();
            var account = new Account()
            {
                Id = accountId,
                Name = _name
            };
            await testData.WithDbContext(async dbContext =>
            {
                await testData.OrganizationService.ExecuteAsync(new CreateRequest()
                {
                    Target = account
                });
            });
            return account;
        }
    }
}
