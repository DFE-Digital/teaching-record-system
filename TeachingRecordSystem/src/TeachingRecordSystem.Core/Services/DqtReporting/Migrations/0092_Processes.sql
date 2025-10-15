create table trs_processes
(
    process_id uniqueidentifier primary key,
    process_type int,
    created datetime,
    user_id uniqueidentifier,
    dqt_user_id uniqueidentifier,
    dqt_user_name nvarchar(200),
    person_ids nvarchar(max),
    [__Inserted] datetime,
    [__Updated] datetime
)

create table trs_process_events
(
    process_event_id uniqueidentifier primary key,
    process_id uniqueidentifier,
    event_name nvarchar(200),
    payload nvarchar(max),
    person_ids nvarchar(max),
    [__Inserted] datetime,
    [__Updated] datetime
)
