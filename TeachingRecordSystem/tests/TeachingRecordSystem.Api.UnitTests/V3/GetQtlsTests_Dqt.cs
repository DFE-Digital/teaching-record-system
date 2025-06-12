using System.Diagnostics;
using TeachingRecordSystem.Api.V3.Implementation.Operations;

namespace TeachingRecordSystem.Api.UnitTests.V3;

public partial class GetQtlsTests
{
    [Fact]
    public Task Dqt_HandleAsync_ContactDoesNotExist_ReturnsNotFoundError() =>
        WithHandler<GetQtlsHandler>(async handler =>
        {
            // Arrange
            FeatureProvider.Features.Remove(FeatureNames.RoutesToProfessionalStatus);

            var command = new GetQtlsCommand("0000000");

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            AssertError(result, ApiError.ErrorCodes.PersonNotFound);
        });

    [Fact]
    public Task Dqt_HandleAsync_ContactHasNullQtlsDate_ReturnsNull() =>
        WithHandler<GetQtlsHandler>(async handler =>
        {
            // Arrange
            FeatureProvider.Features.Remove(FeatureNames.RoutesToProfessionalStatus);

            var person = await TestData.CreatePersonAsync(p => p.WithTrn());
            Debug.Assert(person.Contact.dfeta_qtlsdate is null);

            var command = new GetQtlsCommand(person.Trn!);

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            var success = AssertSuccess(result);
            Assert.Equal(person.Trn, success.Trn);
            Assert.Null(success.QtsDate);
        });

    [Fact]
    public Task Dqt_HandleAsync_ContactHasNonNullQtlsDate_ReturnsDate() =>
        WithHandler<GetQtlsHandler>(async handler =>
        {
            // Arrange
            FeatureProvider.Features.Remove(FeatureNames.RoutesToProfessionalStatus);

            var qtlsDate = new DateOnly(2025, 5, 1);

            var person = await TestData.CreatePersonAsync(p => p.WithTrn().WithQtls(qtlsDate));

            var command = new GetQtlsCommand(person.Trn!);

            // Act
            var result = await handler.HandleAsync(command);

            // Assert
            var success = AssertSuccess(result);
            Assert.Equal(person.Trn, success.Trn);
            Assert.Equal(qtlsDate, success.QtsDate);
        });
}

