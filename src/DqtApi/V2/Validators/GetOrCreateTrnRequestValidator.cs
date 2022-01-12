using DqtApi.DataStore.Sql.Models;
using DqtApi.V2.Requests;
using FluentValidation;

namespace DqtApi.V2.Validators
{
    public class GetOrCreateTrnRequestValidator : AbstractValidator<GetOrCreateTrnRequest>
    {
        public GetOrCreateTrnRequestValidator()
        {
            RuleFor(r => r.RequestId)
                .Matches(TrnRequest.ValidRequestIdPattern)
                    .WithMessage(Properties.StringResources.ErrorMessages_RequestIdCanOnlyContainCharactersDigitsUnderscoresAndDashes)
                .MaximumLength(TrnRequest.RequestIdMaxLength)
                    .WithMessage(Properties.StringResources.ErrorMessages_RequestIdMustBe100CharactersOrFewer);
        }
    }
}
