create table trs_events (
    event_id uniqueidentifier primary key,
    event_name varchar(200),
    created datetime,
    payload text,
    published bit,
    inserted datetime,
    [key] varchar(200),
    person_id uniqueidentifier,
    alert_id uniqueidentifier,
    qualification_id uniqueidentifier
)

create index ix_trs_events_person_id on trs_events (person_id)

create index ix_trs_events_alert_id on trs_events (alert_id)

create index ix_trs_events_qualification_id on trs_events (qualification_id)
