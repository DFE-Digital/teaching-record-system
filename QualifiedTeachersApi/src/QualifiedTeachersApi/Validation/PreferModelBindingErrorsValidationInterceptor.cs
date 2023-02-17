using System.Linq;
using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace QualifiedTeachersApi.Validation
{
    public class PreferModelBindingErrorsValidationInterceptor : IValidatorInterceptor
    {
        public ValidationResult AfterAspNetValidation(ActionContext actionContext, IValidationContext validationContext, ValidationResult result)
        {
            // If there are model binding errors, ignore additional errors from our validators.
            // This prevents getting both 'Thing is invalid' and 'Thing is required' messages.

            var failures = result.Errors.ToList();

            foreach (var error in result.Errors)
            {
                if (actionContext.ModelState.TryGetValue(error.PropertyName, out var modelStateEntry) &&
                    modelStateEntry.Errors.Count > 0)
                {
                    failures.Remove(error);
                }
            }

            return new ValidationResult(failures);
        }

        public IValidationContext BeforeAspNetValidation(ActionContext actionContext, IValidationContext commonContext)
        {
            return commonContext;
        }
    }
}
