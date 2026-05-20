create table trs_persons (
    person_id uniqueidentifier not null primary key,
    trn char(7),
    first_name varchar(100),
    middle_name varchar(100),
    last_name varchar(100),
    date_of_birth date,
    email_address varchar(100),
    national_insurance_number char(9),
    dqt_contact_id uniqueidentifier,
    dqt_state integer,
    dqt_first_name varchar(100),
    dqt_middle_name varchar(100),
    dqt_last_name varchar(100),
    dqt_first_sync datetime,
    dqt_last_sync datetime,
    dqt_created_on datetime,
    dqt_modified_on datetime,
    created_on datetime,
    deleted_on datetime,
    updated_on datetime,
    [__Inserted] datetime,
    [__Updated] datetime
)

create index ix_trs_persons_trn on trs_persons (trn)
