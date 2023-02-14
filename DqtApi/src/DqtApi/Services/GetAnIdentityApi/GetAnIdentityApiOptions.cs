using System.ComponentModel.DataAnnotations;

namespace DqtApi.Services.GetAnIdentityApi
{
    public class GetAnIdentityApiOptions
    {
        [Required]
        public string TokenEndpoint { get; set; }

        [Required]
        public string ClientSecret { get; init; }

        [Required]
        public string ClientId { get; init; }

        [Required]
        public string BaseAddress { get; init; }
    }
}
