EXEC sys.sp_rename
    @objname = N'dbo.trs_persons.induction_exemption_without_reason',
    @newname = 'induction_exempt_without_reason',
    @objtype = 'COLUMN'
