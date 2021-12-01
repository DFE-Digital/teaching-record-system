using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DqtApi.DAL;
using DqtApi.Models;
using DqtApi.Responses;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DqtApi.Tests.UnitTests
{
    public class TeachersController
    {
        private readonly Mock<IDataverseAdaptor> _adaptor;
        private readonly string trn = "1111111";
        private readonly string birthDate = new DateTime(2000, 1, 1).ToString("yyyy-MM-dd");

        public TeachersController()
        {
            _adaptor = new Mock<IDataverseAdaptor>();            
        }

        [Fact]
        public async Task Given_there_is_no_match_return_not_found()
        {
            _adaptor.Setup(a => a.GetMatchingTeachersAsync(It.IsAny<Models.GetTeacherRequest>())).ReturnsAsync(new List<Contact>());

            var result = await new DqtApi.TeachersController(_adaptor.Object).GetTeacher(trn, birthDate, null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Given_there_is_a_match_return_ok()
        {
            _adaptor.Setup(a => a.GetMatchingTeachersAsync(It.IsAny<Models.GetTeacherRequest>())).ReturnsAsync(new[] { new Contact{ dfeta_TRN = trn } });

            var result = await new DqtApi.TeachersController(_adaptor.Object).GetTeacher(trn, birthDate, null);

            Assert.IsType<OkObjectResult>(result);
        }

        //public static GetTeacherRequest MatchesFields(string trn, DateTime birthdate, string nationalInsuranceNumber)
        //    => Match.Create<GetTeacherRequest>(request => request.TRN == trn && request.BirthDate == birthdate && request.NationalInsuranceNumber == nationalInsuranceNumber);
    }
}
