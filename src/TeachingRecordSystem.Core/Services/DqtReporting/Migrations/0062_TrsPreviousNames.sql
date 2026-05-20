create table trs_previous_names (
    previous_name_id uniqueidentifier not null primary key,
    person_id uniqueidentifier not null,
    created_on datetime,
    updated_on datetime,
    deleted_on datetime,
    first_name varchar(100) not null,
    middle_name varchar(100) not null,
    last_name varchar(100) not null,
    [__Inserted] datetime,
    [__Updated] datetime
)

CREATE INDEX ix_previous_names_person_id ON trs_previous_names (person_id)
