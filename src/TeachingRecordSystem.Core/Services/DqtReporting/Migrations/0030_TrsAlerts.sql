create table trs_alert_categories (
    alert_category_id uniqueidentifier not null,
    [name] varchar(200),
    display_order integer,
    [__Inserted] datetime,
    [__Updated] datetime
)

create table trs_alert_types (
    alert_type_id uniqueidentifier not null,
    alert_category_id uniqueidentifier,
    [name] varchar(200),
    dqt_sanction_code varchar(5),
    prohibition_level integer,
    internal_only bit,
    is_active bit,
    display_order integer,
    [__Inserted] datetime,
    [__Updated] datetime
)

create table trs_alerts (
    alert_id uniqueidentifier not null,
    alert_type_id uniqueidentifier,
    person_id uniqueidentifier,
    details text,
    external_link text,
    start_date date,
    end_date date,
    created_on datetime,
    updated_on datetime,
    deleted_on datetime,
    dqt_created_on datetime,
    dqt_modified_on datetime,
    dqt_sanction_id uniqueidentifier,
    dqt_state integer,
    [__Inserted] datetime,
    [__Updated] datetime
)

create index ix_trs_alerts_person on trs_alerts (person_id)
