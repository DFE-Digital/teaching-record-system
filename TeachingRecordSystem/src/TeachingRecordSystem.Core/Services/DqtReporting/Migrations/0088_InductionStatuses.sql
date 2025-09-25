create table trs_induction_statuses
(
    induction_status int primary key,
    name             nvarchar(200) not null,
    [__Created]      datetime,
    [__Updated]      datetime
)
