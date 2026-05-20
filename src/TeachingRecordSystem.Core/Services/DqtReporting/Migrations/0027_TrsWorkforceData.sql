create table trs_tps_employments (
    tps_employment_id uniqueidentifier not null primary key,
    person_id uniqueidentifier,
    establishment_id uniqueidentifier,
    start_date date,
    end_date date,
    last_known_tps_employed_date date,
    last_extract_date date,
    employment_type integer,
    created_on datetime,
    updated_on datetime,
    [key] varchar(50),
    national_insurance_number char(9),
    person_postcode varchar(10),
    withdrawal_confirmed bit,
    [__Inserted] datetime,
    [__Updated] datetime
)

create table trs_tps_establishments (
    tps_establishment_id uniqueidentifier not null primary key,
    la_code char(3),
    establishment_code char(4),
    employers_name varchar(200),
    school_gias_name varchar(200),
    school_closed_date date,
    [__Inserted] datetime,
    [__Updated] datetime
)
