namespace TeachingRecordSystem.Core.DataStore.Postgres;

public static class QueryFilterNames
{
    // Filter names are global; this one excludes soft deleted rows from every entity with a DeletedOn column
    public const string Deleted = "Deleted";

    public static class Person
    {
        // Excludes persons that are not active i.e. those that have been deactivated or merged into another record
        public const string Deactivated = "Deactivated";
    }
}
