namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetAllSanctionCodesQuery(bool ActiveOnly) : ICrmQuery<dfeta_sanctioncode[]>;
