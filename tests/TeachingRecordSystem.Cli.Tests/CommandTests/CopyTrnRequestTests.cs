using System.CommandLine;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Cli.Tests.CommandTests;

public class CopyTrnRequestTests(IServiceProvider services) : CommandTestBase(services)
{
    [Fact]
    public async Task CopyTrnRequest_TrnRequestDoesNotExist_ReturnsError()
    {
        // Arrange
        var sourceApplicationUser = await TestData.CreateApplicationUserAsync();
        var targetApplicationUser = await TestData.CreateApplicationUserAsync();
        var error = new StringWriter();

        // Act
        var result = await InvokeAsync(sourceApplicationUser.UserId, Guid.NewGuid().ToString(), targetApplicationUser.UserId, error: error);

        // Assert
        Assert.Equal(1, result);
        Assert.Contains("TRN request was not found", error.ToString());
    }

    [Fact]
    public async Task CopyTrnRequest_TrnRequestDoesNotHaveVerifiedOneLoginUser_ReturnsError()
    {
        // Arrange
        var sourceApplicationUser = await TestData.CreateApplicationUserAsync();
        var targetApplicationUser = await TestData.CreateApplicationUserAsync();
        var trnRequest = await CreateTrnRequestAsync(sourceApplicationUser.UserId, identityVerified: false);
        var error = new StringWriter();

        // Act
        var result = await InvokeAsync(sourceApplicationUser.UserId, trnRequest.RequestId, targetApplicationUser.UserId, error: error);

        // Assert
        Assert.Equal(1, result);
        Assert.Contains("TRN request does not have a verified One Login user", error.ToString());
    }

    [Fact]
    public async Task CopyTrnRequest_TargetApplicationUserDoesNotExist_ReturnsError()
    {
        // Arrange
        var sourceApplicationUser = await TestData.CreateApplicationUserAsync();
        var trnRequest = await CreateTrnRequestAsync(sourceApplicationUser.UserId);
        var error = new StringWriter();

        // Act
        var result = await InvokeAsync(sourceApplicationUser.UserId, trnRequest.RequestId, Guid.NewGuid(), error: error);

        // Assert
        Assert.Equal(1, result);
        Assert.Contains("Target application user was not found", error.ToString());
    }

    [Fact]
    public async Task CopyTrnRequest_TargetApplicationUserAlreadyHasTrnRequestWithId_ReturnsError()
    {
        // Arrange
        var sourceApplicationUser = await TestData.CreateApplicationUserAsync();
        var targetApplicationUser = await TestData.CreateApplicationUserAsync();
        var trnRequest = await CreateTrnRequestAsync(sourceApplicationUser.UserId);
        await CreateTrnRequestAsync(targetApplicationUser.UserId, requestId: trnRequest.RequestId);
        var error = new StringWriter();

        // Act
        var result = await InvokeAsync(sourceApplicationUser.UserId, trnRequest.RequestId, targetApplicationUser.UserId, error: error);

        // Assert
        Assert.Equal(1, result);
        Assert.Contains("Target application user already has a TRN request with this ID", error.ToString());
    }

    [Fact]
    public async Task CopyTrnRequest_ValidInvocation_CreatesDormantCopyForTargetApplicationUser()
    {
        // Arrange
        var sourceApplicationUser = await TestData.CreateApplicationUserAsync();
        var targetApplicationUser = await TestData.CreateApplicationUserAsync();
        var trnRequest = await CreateTrnRequestAsync(sourceApplicationUser.UserId);
        var output = new StringWriter();

        // Act
        var result = await InvokeAsync(sourceApplicationUser.UserId, trnRequest.RequestId, targetApplicationUser.UserId, output: output);

        // Assert
        Assert.Equal(0, result);
        Assert.Contains($"Created dormant TRN request {trnRequest.RequestId} for application user {targetApplicationUser.UserId}", output.ToString());

        await WithDbContextAsync(async dbContext =>
        {
            var copy = await dbContext.TrnRequestMetadata
                .SingleAsync(r => r.ApplicationUserId == targetApplicationUser.UserId && r.RequestId == trnRequest.RequestId);

            Assert.Equal(TrnRequestStatus.Dormant, copy.Status);
            Assert.Equal(trnRequest.OneLoginUserSubject, copy.OneLoginUserSubject);
            Assert.True(copy.IdentityVerified);
            Assert.Equal(trnRequest.EmailAddress, copy.EmailAddress);
            Assert.Equal(trnRequest.FirstName, copy.FirstName);
            Assert.Equal(trnRequest.LastName, copy.LastName);
            Assert.Equal(trnRequest.DateOfBirth, copy.DateOfBirth);
            Assert.Equal(trnRequest.NationalInsuranceNumber, copy.NationalInsuranceNumber);
            Assert.Null(copy.ResolvedPersonId);

            var original = await dbContext.TrnRequestMetadata
                .SingleAsync(r => r.ApplicationUserId == sourceApplicationUser.UserId && r.RequestId == trnRequest.RequestId);
            Assert.Equal(TrnRequestStatus.Dormant, original.Status);
        });
    }

    private async Task<TrnRequestMetadata> CreateTrnRequestAsync(Guid applicationUserId, bool identityVerified = true, string? requestId = null)
    {
        var trnRequest = new TrnRequestMetadata
        {
            RequestId = requestId ?? Guid.NewGuid().ToString(),
            ApplicationUserId = applicationUserId,
            OneLoginUserSubject = TestData.CreateOneLoginUserSubject(),
            CreatedOn = TimeProvider.UtcNow,
            EmailAddress = TestData.GenerateUniqueEmail(),
            FirstName = TestData.GenerateFirstName(),
            MiddleName = null,
            LastName = TestData.GenerateLastName(),
            Name = [],
            DateOfBirth = TestData.GenerateDateOfBirth(),
            NationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber(),
            IdentityVerified = identityVerified,
            Status = TrnRequestStatus.Dormant
        };

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.TrnRequestMetadata.Add(trnRequest);
            await dbContext.SaveChangesAsync();
        });

        return trnRequest;
    }

    private Task<int> InvokeAsync(
        Guid sourceApplicationUserId,
        string trnRequestId,
        Guid targetApplicationUserId,
        TextWriter? output = null,
        TextWriter? error = null)
    {
        var command = Commands.CreateCopyTrnRequestCommand(Configuration);

        var parseResult = command.Parse([
            "--trn-request-id", trnRequestId,
            "--source-application-user-id", sourceApplicationUserId.ToString(),
            "--target-application-user-id", targetApplicationUserId.ToString()
        ]);

        return parseResult.InvokeAsync(
            new InvocationConfiguration
            {
                Output = output ?? TextWriter.Null,
                Error = error ?? TextWriter.Null
            });
    }
}
