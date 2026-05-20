CREATE TABLE trs_notes
(
    note_id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    person_id UNIQUEIDENTIFIER NOT NULL,
    content_html TEXT NOT NULL,
    updated_on datetime NULL,
    created_on datetime  NOT NULL,
    created_by_dqt_user_id UNIQUEIDENTIFIER NULL,
    created_by_dqt_user_name NVARCHAR(MAX) NULL,
    updated_by_dqt_user_id UNIQUEIDENTIFIER NULL,
    updated_by_dqt_user_name NVARCHAR(MAX) NULL,
    [file_name] NVARCHAR(MAX) NULL,
    original_file_name NVARCHAR(MAX) NULL
);