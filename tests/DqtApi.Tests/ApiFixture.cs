using Microsoft.AspNetCore.Mvc.Testing;

namespace DqtApi.Tests
{
    public class ApiFixture : WebApplicationFactory<DqtApi.Startup>
    {
        public void ResetMocks()
        {
        }
    }
}