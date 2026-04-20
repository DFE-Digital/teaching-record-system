using Riok.Mapperly.Abstractions;
using TeachingRecordSystem.Api.V3.Implementation.Operations;
using TeachingRecordSystem.Api.V3.V20240814.Responses;
using TeachingRecordSystem.Core.ApiSchema.V3.V20240101.Dtos;
using V20240814Dtos = TeachingRecordSystem.Core.ApiSchema.V3.V20240814.Dtos;

namespace TeachingRecordSystem.Api.V3.V20240814;

[Mapper]
public partial class ApiMapper
{
    public FindPersonsResponse MapFindPersonsResponse(FindPersonsResult source) =>
        new()
        {
            Total = source.Total,
            Results = source.Items.Select(MapFindPersonsResponseResult).AsReadOnly()
        };

    public FindPersonsResponseResult MapFindPersonsResponseResult(FindPersonsResultItem source) =>
        new()
        {
            Trn = source.Trn,
            DateOfBirth = source.DateOfBirth,
            FirstName = source.FirstName,
            MiddleName = source.MiddleName,
            LastName = source.LastName,
            Sanctions = source.Sanctions.Select(MapSanctionInfo).AsReadOnly(),
            PreviousNames = source.PreviousNames.Select(MapNameInfo).AsReadOnly(),
            InductionStatus = source.DqtInductionStatus is { } dqt ? MapDqtInductionStatusInfo(dqt) : null!,
            Qts = source.Qts is { } qts ? MapQts(qts) : null,
            Eyts = source.Eyts is { } eyts ? MapEyts(eyts) : null
        };

    private V20240814Dtos.DqtInductionStatusInfo MapDqtInductionStatusInfo(Implementation.Dtos.DqtInductionStatusInfo source) =>
        new() { Status = MapDqtInductionStatus(source.Status), StatusDescription = source.StatusDescription };

    private V20240814Dtos.QtsInfo MapQts(Implementation.Dtos.QtsInfo source) =>
        new() { Awarded = source.HoldsFrom, StatusDescription = source.StatusDescription };

    private V20240814Dtos.EytsInfo MapEyts(Implementation.Dtos.EytsInfo source) =>
        new() { Awarded = source.HoldsFrom, StatusDescription = source.StatusDescription };

    private SanctionInfo MapSanctionInfo(Implementation.Dtos.SanctionInfo source) =>
        new() { Code = source.Code, StartDate = source.StartDate };

    private NameInfo MapNameInfo(Implementation.Dtos.NameInfo source) =>
        new() { FirstName = source.FirstName, MiddleName = source.MiddleName, LastName = source.LastName };

    public FindPersonResponseResult MapFindPersonResponseResult(FindPersonsResultItem source) =>
        new()
        {
            Trn = source.Trn,
            DateOfBirth = source.DateOfBirth,
            FirstName = source.FirstName,
            MiddleName = source.MiddleName,
            LastName = source.LastName,
            Sanctions = source.Sanctions.Select(MapSanctionInfo).AsReadOnly(),
            PreviousNames = source.PreviousNames.Select(MapNameInfo).AsReadOnly(),
            InductionStatus = source.DqtInductionStatus is { } dqt ? MapDqtInductionStatusInfo(dqt) : null!,
            Qts = source.Qts is { } qts ? MapQts(qts) : null,
            Eyts = source.Eyts is { } eyts ? MapEyts(eyts) : null
        };

    private static Core.ApiSchema.V3.V20240101.Dtos.DqtInductionStatus MapDqtInductionStatus(Implementation.Dtos.DqtInductionStatus source) =>
        source switch
        {
            Implementation.Dtos.DqtInductionStatus.Exempt => Core.ApiSchema.V3.V20240101.Dtos.DqtInductionStatus.Exempt,
            Implementation.Dtos.DqtInductionStatus.Fail => Core.ApiSchema.V3.V20240101.Dtos.DqtInductionStatus.Fail,
            Implementation.Dtos.DqtInductionStatus.FailedInWales => Core.ApiSchema.V3.V20240101.Dtos.DqtInductionStatus.FailedinWales,
            Implementation.Dtos.DqtInductionStatus.InductionExtended => Core.ApiSchema.V3.V20240101.Dtos.DqtInductionStatus.InductionExtended,
            Implementation.Dtos.DqtInductionStatus.InProgress => Core.ApiSchema.V3.V20240101.Dtos.DqtInductionStatus.InProgress,
            Implementation.Dtos.DqtInductionStatus.NotYetCompleted => Core.ApiSchema.V3.V20240101.Dtos.DqtInductionStatus.NotYetCompleted,
            Implementation.Dtos.DqtInductionStatus.Pass => Core.ApiSchema.V3.V20240101.Dtos.DqtInductionStatus.Pass,
            Implementation.Dtos.DqtInductionStatus.PassedInWales => Core.ApiSchema.V3.V20240101.Dtos.DqtInductionStatus.PassedinWales,
            Implementation.Dtos.DqtInductionStatus.RequiredToComplete => Core.ApiSchema.V3.V20240101.Dtos.DqtInductionStatus.RequiredtoComplete,
            _ => throw new ArgumentOutOfRangeException(nameof(source))
        };
}
