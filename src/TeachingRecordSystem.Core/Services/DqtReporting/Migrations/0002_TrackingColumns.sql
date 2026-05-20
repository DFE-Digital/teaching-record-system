if not exists (select 1 from information_schema.tables where table_name = 'annotation')
begin
create table [annotation] (
	[Id] uniqueidentifier not null primary key,
	[objecttypecode] nvarchar(4000),
	[owninguser] uniqueidentifier,
	[owninguser_entitytype] nvarchar(128),
	[objectid] uniqueidentifier,
	[objectid_entitytype] nvarchar(128),
	[owningbusinessunit] uniqueidentifier,
	[owningbusinessunit_entitytype] nvarchar(128),
	[subject] nvarchar(500),
	[isdocument] bit,
	[notetext] nvarchar(max),
	[mimetype] nvarchar(256),
	[langid] nvarchar(2),
	[documentbody] nvarchar(max),
	[createdon] datetime,
	[filesize] int,
	[filename] nvarchar(255),
	[createdby] uniqueidentifier,
	[createdby_entitytype] nvarchar(128),
	[isprivate] bit,
	[modifiedby] uniqueidentifier,
	[modifiedby_entitytype] nvarchar(128),
	[modifiedon] datetime,
	[versionnumber] bigint,
	[createdbyname] nvarchar(100),
	[modifiedbyname] nvarchar(100),
	[ownerid] uniqueidentifier,
	[ownerid_entitytype] nvarchar(128),
	[owneridname] nvarchar(100),
	[owneridtype] nvarchar(4000),
	[stepid] nvarchar(32),
	[overriddencreatedon] datetime,
	[importsequencenumber] int,
	[createdbyyominame] nvarchar(100),
	[modifiedbyyominame] nvarchar(100),
	[owneridyominame] nvarchar(100),
	[objectidtypecode] nvarchar(4000),
	[createdonbehalfby] uniqueidentifier,
	[createdonbehalfby_entitytype] nvarchar(128),
	[createdonbehalfbyname] nvarchar(100),
	[createdonbehalfbyyominame] nvarchar(100),
	[modifiedonbehalfby] uniqueidentifier,
	[modifiedonbehalfby_entitytype] nvarchar(128),
	[modifiedonbehalfbyname] nvarchar(100),
	[modifiedonbehalfbyyominame] nvarchar(100),
	[owningteam] uniqueidentifier,
	[owningteam_entitytype] nvarchar(128),
	[prefix] nvarchar(10),
	[storagepointer] nvarchar(10),
	[filepointer] nvarchar(255),
	[dummyfilename] nvarchar(500),
	[dummyregarding] nvarchar(500),
	[owningbusinessunitname] nvarchar(160)
)
end



create table [__DeleteLog] (
    EntityId uniqueidentifier not null,
    EntityType nvarchar(128) not null,
    Deleted datetime not null,
    primary key(EntityId, EntityType)
)

alter table [dfeta_mrtraining] add [__Inserted] datetime
alter table [dfeta_optionsetmapping] add [__Inserted] datetime
alter table [dfeta_organisationcategory] add [__Inserted] datetime
alter table [dfeta_prdeclinestatusreason] add [__Inserted] datetime
alter table [dfeta_previousname] add [__Inserted] datetime
alter table [dfeta_profileamendrequest] add [__Inserted] datetime
alter table [dfeta_qtsregistration] add [__Inserted] datetime
alter table [dfeta_qualification] add [__Inserted] datetime
alter table [dfeta_sanction] add [__Inserted] datetime
alter table [dfeta_sanctioncode] add [__Inserted] datetime
alter table [dfeta_schooldirectinitiative] add [__Inserted] datetime
alter table [dfeta_serviceannouncement] add [__Inserted] datetime
alter table [dfeta_specialism] add [__Inserted] datetime
alter table [dfeta_teacherstatus] add [__Inserted] datetime
alter table [dfeta_temporaryqueries] add [__Inserted] datetime
alter table [dfeta_tsstcohorts] add [__Inserted] datetime
alter table [dfeta_tsstsubjects] add [__Inserted] datetime
alter table [dfeta_webactivitylog] add [__Inserted] datetime
alter table [incident] add [__Inserted] datetime
alter table [incidentresolution] add [__Inserted] datetime
alter table [lead] add [__Inserted] datetime
alter table [subject] add [__Inserted] datetime
alter table [systemuser] add [__Inserted] datetime
alter table [transactioncurrency] add [__Inserted] datetime
alter table [account] add [__Inserted] datetime
alter table [accountleads] add [__Inserted] datetime
alter table [annotation] add [__Inserted] datetime
alter table [campaign] add [__Inserted] datetime
alter table [contact] add [__Inserted] datetime
alter table [contactleads] add [__Inserted] datetime
alter table [dfeta_autonumber] add [__Inserted] datetime
alter table [dfeta_businesseventaudit] add [__Inserted] datetime
alter table [dfeta_confirmationletter] add [__Inserted] datetime
alter table [dfeta_country] add [__Inserted] datetime
alter table [dfeta_countrystate] add [__Inserted] datetime
alter table [dfeta_disability] add [__Inserted] datetime
alter table [dfeta_document] add [__Inserted] datetime
alter table [dfeta_earlyyearsstatus] add [__Inserted] datetime
alter table [dfeta_employment] add [__Inserted] datetime
alter table [dfeta_ethnicity] add [__Inserted] datetime
alter table [dfeta_hequalification] add [__Inserted] datetime
alter table [dfeta_hesaukprnmapping] add [__Inserted] datetime
alter table [dfeta_hesubject] add [__Inserted] datetime
alter table [dfeta_induction] add [__Inserted] datetime
alter table [dfeta_inductionperiod] add [__Inserted] datetime
alter table [dfeta_initialteachertraining] add [__Inserted] datetime
alter table [dfeta_initiativecode] add [__Inserted] datetime
alter table [dfeta_integrationtransaction] add [__Inserted] datetime
alter table [dfeta_integrationtransactionrecord] add [__Inserted] datetime
alter table [dfeta_ittqualification] add [__Inserted] datetime
alter table [dfeta_ittsubject] add [__Inserted] datetime
alter table [dfeta_legacyaudit] add [__Inserted] datetime
alter table [dfeta_mqestablishment] add [__Inserted] datetime
alter table [dfeta_mrapplication] add [__Inserted] datetime
alter table [dfeta_mrcourse] add [__Inserted] datetime
alter table [dfeta_mremployment] add [__Inserted] datetime
alter table [dfeta_mrtraining] add [__Updated] datetime
alter table [dfeta_optionsetmapping] add [__Updated] datetime
alter table [dfeta_organisationcategory] add [__Updated] datetime
alter table [dfeta_prdeclinestatusreason] add [__Updated] datetime
alter table [dfeta_previousname] add [__Updated] datetime
alter table [dfeta_profileamendrequest] add [__Updated] datetime
alter table [dfeta_qtsregistration] add [__Updated] datetime
alter table [dfeta_qualification] add [__Updated] datetime
alter table [dfeta_sanction] add [__Updated] datetime
alter table [dfeta_sanctioncode] add [__Updated] datetime
alter table [dfeta_schooldirectinitiative] add [__Updated] datetime
alter table [dfeta_serviceannouncement] add [__Updated] datetime
alter table [dfeta_specialism] add [__Updated] datetime
alter table [dfeta_teacherstatus] add [__Updated] datetime
alter table [dfeta_temporaryqueries] add [__Updated] datetime
alter table [dfeta_tsstcohorts] add [__Updated] datetime
alter table [dfeta_tsstsubjects] add [__Updated] datetime
alter table [dfeta_webactivitylog] add [__Updated] datetime
alter table [incident] add [__Updated] datetime
alter table [incidentresolution] add [__Updated] datetime
alter table [lead] add [__Updated] datetime
alter table [subject] add [__Updated] datetime
alter table [systemuser] add [__Updated] datetime
alter table [transactioncurrency] add [__Updated] datetime
alter table [account] add [__Updated] datetime
alter table [accountleads] add [__Updated] datetime
alter table [annotation] add [__Updated] datetime
alter table [campaign] add [__Updated] datetime
alter table [contact] add [__Updated] datetime
alter table [contactleads] add [__Updated] datetime
alter table [dfeta_autonumber] add [__Updated] datetime
alter table [dfeta_businesseventaudit] add [__Updated] datetime
alter table [dfeta_confirmationletter] add [__Updated] datetime
alter table [dfeta_country] add [__Updated] datetime
alter table [dfeta_countrystate] add [__Updated] datetime
alter table [dfeta_disability] add [__Updated] datetime
alter table [dfeta_document] add [__Updated] datetime
alter table [dfeta_earlyyearsstatus] add [__Updated] datetime
alter table [dfeta_employment] add [__Updated] datetime
alter table [dfeta_ethnicity] add [__Updated] datetime
alter table [dfeta_hequalification] add [__Updated] datetime
alter table [dfeta_hesaukprnmapping] add [__Updated] datetime
alter table [dfeta_hesubject] add [__Updated] datetime
alter table [dfeta_induction] add [__Updated] datetime
alter table [dfeta_inductionperiod] add [__Updated] datetime
alter table [dfeta_initialteachertraining] add [__Updated] datetime
alter table [dfeta_initiativecode] add [__Updated] datetime
alter table [dfeta_integrationtransaction] add [__Updated] datetime
alter table [dfeta_integrationtransactionrecord] add [__Updated] datetime
alter table [dfeta_ittqualification] add [__Updated] datetime
alter table [dfeta_ittsubject] add [__Updated] datetime
alter table [dfeta_legacyaudit] add [__Updated] datetime
alter table [dfeta_mqestablishment] add [__Updated] datetime
alter table [dfeta_mrapplication] add [__Updated] datetime
alter table [dfeta_mrcourse] add [__Updated] datetime
alter table [dfeta_mremployment] add [__Updated] datetime
