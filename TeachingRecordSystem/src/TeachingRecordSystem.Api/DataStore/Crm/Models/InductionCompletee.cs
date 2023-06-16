﻿namespace TeachingRecordSystem.Api.DataStore.Crm.Models;

public record InductionCompletee
{
    public required Guid TeacherId { get; set; }
    public required string Trn { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string EmailAddress { get; set; }
}
