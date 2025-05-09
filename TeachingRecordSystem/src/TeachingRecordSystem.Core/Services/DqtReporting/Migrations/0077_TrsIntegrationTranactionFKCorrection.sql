IF OBJECT_ID('fk_trs_integration_transaction_records_persons_person_id', 'F') IS NOT NULL
    ALTER TABLE trs_integration_transaction_records
    DROP CONSTRAINT fk_trs_integration_transaction_records_persons_person_id;

IF OBJECT_ID('fk_trs_integrationtransactionrecord_integrationtransaction', 'F') IS NOT NULL
    ALTER TABLE trs_integration_transaction_records
    DROP CONSTRAINT fk_trs_integrationtransactionrecord_integrationtransaction;

IF OBJECT_ID('trs_integration_transaction_records', 'U') IS NOT NULL
    DROP TABLE trs_integration_transaction_records;

IF OBJECT_ID('trs_integration_transactions', 'U') IS NOT NULL
    DROP TABLE trs_integration_transactions;


CREATE TABLE trs_integration_transactions
(
    integration_transaction_id BIGINT NOT NULL,
    interface_type INT NOT NULL,
    import_status INT NOT NULL,
    total_count INT NOT NULL,
    success_count INT NOT NULL,
    failure_count INT NOT NULL,
    duplicate_count INT NOT NULL,
    file_name NVARCHAR(MAX) NOT NULL,
    created_date DATETIME NOT NULL,
    [__Inserted] [datetime] NULL,
	[__Updated] [datetime] NULL,
    CONSTRAINT pk_trs_integration_transactions PRIMARY KEY (integration_transaction_id)
);

CREATE TABLE trs_integration_transaction_records
(
    integration_transaction_record_id BIGINT NOT NULL,
    row_data NVARCHAR(3000),
    failure_message NVARCHAR(3000),
    person_id UNIQUEIDENTIFIER,
    duplicate BIT,
    [status] INT NOT NULL,
    created_date DATETIME NOT NULL,
    integration_transaction_id BIGINT,
    has_active_alert BIT,
    [__Inserted] [datetime] NULL,
	[__Updated] [datetime] NULL,
    CONSTRAINT pk_trs_integration_transaction_records PRIMARY KEY (integration_transaction_record_id),
    CONSTRAINT fk_trs_integration_transaction_records_persons_person_id FOREIGN KEY (person_id)
        REFERENCES trs_persons (person_id),
    CONSTRAINT fk_trs_integrationtransactionrecord_integrationtransaction FOREIGN KEY (integration_transaction_id)
        REFERENCES trs_integration_transactions (integration_transaction_id)
);
