using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DqtApi.Models;
using FluentValidation;

namespace DqtApi.Validators
{
    public class GetTeacherValidator : AbstractValidator<GetTeacherRequest>
    {
        public GetTeacherValidator()
        {
            RuleFor(x => x.TRN).Matches(@"^\d{7}$");
            RuleFor(x => x.BirthDate);
        }
    }
}
