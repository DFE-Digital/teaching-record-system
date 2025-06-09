alter table trs_qualifications add route_to_professional_status_type_id uniqueidentifier

create table trs_route_to_professional_status_types (
    route_to_professional_status_type_id uniqueidentifier primary key,
    name nvarchar(max),
    professional_status_type int,
    is_active bit,
    training_start_date_required int,
    training_end_date_required int,
    award_date_required int,
    induction_exemption_required int,
    training_provider_required int,
    degree_type_required int,
    training_country_required int,
    training_age_specialism_type_required int,
    training_subjects_required int,
    induction_exemption_reason_id uniqueidentifier
)
