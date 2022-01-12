using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DqtApi.DataStore.Crm;
using DqtApi.DataStore.Crm.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DqtApi.Tests.V1.UnitTests
{
    public class TeachersController
    {
        private readonly Mock<IDataverseAdapter> _adapter;
        private readonly string trn = "1111111";
        private readonly DateTime birthDate = new DateTime(2000, 1, 1);

        public TeachersController()
        {
            _adapter = new Mock<IDataverseAdapter>();            
        }

        [Fact]
        public async Task Given_there_is_no_match_return_not_found()
        {
            _adapter.Setup(a => a.GetMatchingTeachersAsync(It.IsAny<GetTeacherRequest>())).ReturnsAsync(new List<Contact>());

            var result = await new DqtApi.V1.Controllers.TeachersController(_adapter.Object).GetTeacher(new GetTeacherRequest { TRN = trn, BirthDate = birthDate });

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Given_there_is_a_match_return_ok()
        {
            _adapter.Setup(a => a.GetMatchingTeachersAsync(It.IsAny<GetTeacherRequest>())).ReturnsAsync(new[] { new Contact{ dfeta_TRN = trn } });

            var result = await new DqtApi.V1.Controllers.TeachersController(_adapter.Object).GetTeacher(new GetTeacherRequest { TRN = trn, BirthDate = birthDate });

            Assert.IsType<OkObjectResult>(result);
        }
    }
}
