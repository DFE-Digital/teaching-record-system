create table trs_training_providers (
    training_provider_id uniqueidentifier primary key,
    [name] varchar(200),
    ukprn char(8),
    is_active bit,
    [__Inserted] datetime,
    [__Updated] datetime
)

