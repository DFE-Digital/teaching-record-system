using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.Api.UnitTests.V3;

public partial class SetQtlsTests
{
    [Fact]
    public Task Dqt_HandleAsync_PersonDoesNotExist_ReturnsError() =>
        WithHandler<SetQtlsHandler>(async handler =>
        {
            // Arrange
            FeatureProvider.Features.Remove(FeatureNames.RoutesToProfessionalStatus);

            var command = new SetQtlsCommand("0000000", QtsDate: null);

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            AssertError(result, ApiError.ErrorCodes.PersonNotFound);
        });

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public Task Dqt_HandleAsync_UpdatesQtlsDate(bool hasExistingDate, bool hasNewDate) =>
        WithHandler<SetQtlsHandler>(async handler =>
        {
            // Arrange
            FeatureProvider.Features.Remove(FeatureNames.RoutesToProfessionalStatus);

            DateOnly? existingQtlsDate = hasExistingDate ? new DateOnly(2025, 4, 1) : null;
            DateOnly? newQtlsDate = hasNewDate ? new DateOnly(2025, 4, 10) : null;

            var person = await TestData.CreatePersonAsync(p =>
            {
                p.WithTrn();

                if (existingQtlsDate is DateOnly qtlsDate)
                {
                    p.WithQtls(qtlsDate);
                }
            });

            var command = new SetQtlsCommand(person.Trn!, QtsDate: newQtlsDate);

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            var success = AssertSuccess(result);
            Assert.Equal(newQtlsDate, success.QtsDate);

            var contact = XrmFakedContext.CreateQuery<Contact>().Single(c => c.Id == person.ContactId);
            Assert.Equal(newQtlsDate.ToDateTimeWithDqtBstFix(isLocalTime: false), contact.dfeta_qtlsdate);
        });

    [Fact]
    public Task Dqt_HandleAsync_ContactHasActiveAlert_CreatesTask() =>
        WithHandler<SetQtlsHandler>(async handler =>
        {
            // Arrange
            FeatureProvider.Features.Remove(FeatureNames.RoutesToProfessionalStatus);

            var newQtlsDate = new DateOnly(2025, 4, 10);

            var person = await TestData.CreatePersonAsync(p => p
                .WithTrn()
                .WithAlert());

            var command = new SetQtlsCommand(person.Trn!, QtsDate: newQtlsDate);

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            var success = AssertSuccess(result);
            Assert.Equal(newQtlsDate, success.QtsDate);

            var task = XrmFakedContext.CreateQuery<Core.Dqt.Models.Task>().SingleOrDefault(c => c.RegardingObjectId.Id == person.ContactId);
            Assert.NotNull(task);
        });
}
