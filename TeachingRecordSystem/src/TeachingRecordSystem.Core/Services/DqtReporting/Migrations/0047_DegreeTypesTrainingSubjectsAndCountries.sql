create table trs_degree_types (
    degree_type_id uniqueidentifier primary key,
    [name] varchar(200),
    is_active bit,
    [__Inserted] datetime,
    [__Updated] datetime
)

create table trs_training_subjects (
    training_subject_id uniqueidentifier primary key,
    [name] varchar(200),
    [reference] varchar(10),
    is_active bit,
    [__Inserted] datetime,
    [__Updated] datetime
)

create table trs_countries (
    country_id varchar(10) primary key,
    [name] varchar(200),
    official_name varchar(200),
    citizen_names varchar(200),    
    [__Inserted] datetime,
    [__Updated] datetime
)
