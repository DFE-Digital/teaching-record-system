create table trs_establishments (
    establishment_id uniqueidentifier primary key,
    urn int,
    la_code character(3),
    la_name varchar(50),
    establishment_number character(4),
    establishment_name varchar(120),
    establishment_type_code varchar(3),
    establishment_type_name varchar(100),
    establishment_type_group_code int,
    establishment_type_group_name varchar(50),
    establishment_status_code int,
    establishment_status_name varchar(50),
    street varchar(100),
    locality varchar(100),
    address3 varchar(100),
    town varchar(100),
    county varchar(100),
    postcode varchar(10),
    establishment_source_id int,
    [__Inserted] datetime,
    [__Updated] datetime
)