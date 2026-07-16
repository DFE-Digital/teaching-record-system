alter table trs_support_tasks add assigned_to_user_id uniqueidentifier
alter table trs_support_tasks add completed_by_user_id uniqueidentifier
alter table trs_support_tasks add completed_on datetime
alter table trs_support_tasks add outcome_label nvarchar(200)
alter table trs_support_tasks add subject_email_address nvarchar(200)
alter table trs_support_tasks add subject_name nvarchar(max)
alter table trs_support_tasks add subject_names nvarchar(max)
