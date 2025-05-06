namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

// seems like I have to define a query to the handler
public record MyDummyQuery : ICrmQuery<Account[]>;
