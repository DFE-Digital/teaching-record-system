IF OBJECT_ID('fk_trs_integration_transaction_records_persons_person_id', 'F') IS NOT NULL
begin
    ALTER TABLE trs_integration_transaction_records
    DROP CONSTRAINT fk_trs_integration_transaction_records_persons_person_id;
end
