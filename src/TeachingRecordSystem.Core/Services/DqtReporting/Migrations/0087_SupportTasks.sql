create table trs_support_tasks
(
    support_task_reference varchar(16),
    support_task_type int,
    status int,
    data nvarchar(max),
    one_login_user_subject varchar(255),
    person_id uniqueidentifier,
    created_on datetime,
    updated_on datetime,
    trn_request_application_user_id uniqueidentifier,
    trn_request_id varchar(100),
    [__Inserted] datetime,
    [__Updated] datetime,
)
