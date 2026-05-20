--Contact
--adx_createdbyipaddress
if not exists (select 1 from information_schema.columns where table_name = 'contact' and column_name = 'adx_createdbyipaddress')
    alter table contact add adx_createdbyipaddress nvarchar(100)

--adx_createdbyusername
if not exists (select 1 from information_schema.columns where table_name = 'contact' and column_name = 'adx_createdbyusername')
    alter table contact add adx_createdbyusername nvarchar(100)

--adx_modifiedbyipaddress
if not exists (select 1 from information_schema.columns where table_name = 'contact' and column_name = 'adx_modifiedbyipaddress')
    alter table contact add adx_modifiedbyipaddress nvarchar(100)

--adx_modifiedbyusername
if not exists (select 1 from information_schema.columns where table_name = 'contact' and column_name = 'adx_modifiedbyusername')
    alter table contact add adx_modifiedbyusername nvarchar(100)

--adx_organizationname
if not exists (select 1 from information_schema.columns where table_name = 'contact' and column_name = 'adx_organizationname')
    alter table contact add adx_organizationname nvarchar(250)

--adx_timezone
if not exists (select 1 from information_schema.columns where table_name = 'contact' and column_name = 'adx_timezone')
    alter table contact add adx_timezone int

--msa_managingpartnerid
if not exists (select 1 from information_schema.columns where table_name = 'contact' and column_name = 'msa_managingpartnerid')
    alter table contact add msa_managingpartnerid uniqueidentifier

--msa_managingpartnerid_entitytype
if not exists (select 1 from information_schema.columns where table_name = 'contact' and column_name = 'msa_managingpartnerid_entitytype')
    alter table contact add msa_managingpartnerid_entitytype nvarchar(128)

--msa_managingpartneridname
if not exists (select 1 from information_schema.columns where table_name = 'contact' and column_name = 'msa_managingpartneridname')
    alter table contact add msa_managingpartneridname nvarchar(160)

--msa_managingpartneridyominame
if not exists (select 1 from information_schema.columns where table_name = 'contact' and column_name = 'msa_managingpartneridyominame')
    alter table contact add msa_managingpartneridyominame nvarchar(160)

--msdyn_disablewebtracking
if not exists (select 1 from information_schema.columns where table_name = 'contact' and column_name = 'msdyn_disablewebtracking')
    alter table contact add msdyn_disablewebtracking bit

--msdyn_isminor
if not exists (select 1 from information_schema.columns where table_name = 'contact' and column_name = 'msdyn_isminor')
    alter table contact add msdyn_isminor bit

--msdyn_isminorwithparentalconsent
if not exists (select 1 from information_schema.columns where table_name = 'contact' and column_name = 'msdyn_isminorwithparentalconsent')
    alter table contact add msdyn_isminorwithparentalconsent bit

--msdyn_portaltermsagreementdate
if not exists (select 1 from information_schema.columns where table_name = 'contact' and column_name = 'msdyn_portaltermsagreementdate')
    alter table contact add msdyn_portaltermsagreementdate datetime

--adx_confirmremovepassword
if not exists (select 1 from information_schema.columns where table_name = 'contact' and column_name = 'adx_confirmremovepassword')
    alter table contact add adx_confirmremovepassword bit

--adx_identity_accessfailedcount
if not exists (select 1 from information_schema.columns where table_name = 'contact' and column_name = 'adx_identity_accessfailedcount')
    alter table contact add adx_identity_accessfailedcount int

--adx_identity_emailaddress1confirmed
if not exists (select 1 from information_schema.columns where table_name = 'contact' and column_name = 'adx_identity_emailaddress1confirmed')
    alter table contact add adx_identity_emailaddress1confirmed bit

--adx_identity_lastsuccessfullogin
if not exists (select 1 from information_schema.columns where table_name = 'contact' and column_name = 'adx_identity_lastsuccessfullogin')
    alter table contact add adx_identity_lastsuccessfullogin datetime


--adx_identity_lockoutenabled
if not exists (select 1 from information_schema.columns where table_name = 'contact' and column_name = 'adx_identity_lockoutenabled')
    alter table contact add adx_identity_lockoutenabled bit

--adx_identity_lockoutenddate
if not exists (select 1 from information_schema.columns where table_name = 'contact' and column_name = 'adx_identity_lockoutenddate')
    alter table contact add adx_identity_lockoutenddate datetime

--adx_identity_logonenabled
if not exists (select 1 from information_schema.columns where table_name = 'contact' and column_name = 'adx_identity_logonenabled')
    alter table contact add adx_identity_logonenabled bit

--adx_identity_mobilephoneconfirmed
if not exists (select 1 from information_schema.columns where table_name = 'contact' and column_name = 'adx_identity_mobilephoneconfirmed')
    alter table contact add adx_identity_mobilephoneconfirmed bit

--adx_identity_newpassword
if not exists (select 1 from information_schema.columns where table_name = 'contact' and column_name = 'adx_identity_newpassword')
    alter table contact add adx_identity_newpassword nvarchar(100)

--adx_identity_passwordhash
if not exists (select 1 from information_schema.columns where table_name = 'contact' and column_name = 'adx_identity_passwordhash')
    alter table contact add adx_identity_passwordhash nvarchar(128)

--adx_identity_securitystamp
if not exists (select 1 from information_schema.columns where table_name = 'contact' and column_name = 'adx_identity_securitystamp')
    alter table contact add adx_identity_securitystamp nvarchar(100)

--adx_identity_twofactorenabled
if not exists (select 1 from information_schema.columns where table_name = 'contact' and column_name = 'adx_identity_twofactorenabled')
    alter table contact add adx_identity_twofactorenabled bit

--adx_identity_username
if not exists (select 1 from information_schema.columns where table_name = 'contact' and column_name = 'adx_identity_username')
    alter table contact add adx_identity_username nvarchar(100)

--adx_preferredlcid
if not exists (select 1 from information_schema.columns where table_name = 'contact' and column_name = 'adx_preferredlcid')
    alter table contact add adx_preferredlcid int

--adx_profilealert
if not exists (select 1 from information_schema.columns where table_name = 'contact' and column_name = 'adx_profilealert')
    alter table contact add adx_profilealert bit

--adx_profilealertdate
if not exists (select 1 from information_schema.columns where table_name = 'contact' and column_name = 'adx_profilealertdate')
    alter table contact add adx_profilealertdate datetime

--adx_profilealertinstructions
if not exists (select 1 from information_schema.columns where table_name = 'contact' and column_name = 'adx_profilealertinstructions')
    alter table contact add adx_profilealertinstructions nvarchar(max)

--adx_profileisanonymous
if not exists (select 1 from information_schema.columns where table_name = 'contact' and column_name = 'adx_profileisanonymous')
    alter table contact add adx_profileisanonymous bit

--adx_profilelastactivity
if not exists (select 1 from information_schema.columns where table_name = 'contact' and column_name = 'adx_profilelastactivity')
    alter table contact add adx_profilelastactivity datetime

--adx_profilemodifiedon
if not exists (select 1 from information_schema.columns where table_name = 'contact' and column_name = 'adx_profilemodifiedon')
    alter table contact add adx_profilemodifiedon datetime

--adx_publicprofilecopy
if not exists (select 1 from information_schema.columns where table_name = 'contact' and column_name = 'adx_publicprofilecopy')
    alter table contact add adx_publicprofilecopy nvarchar(max)

--mspp_userpreferredlcid
if not exists (select 1 from information_schema.columns where table_name = 'contact' and column_name = 'mspp_userpreferredlcid')
    alter table contact add mspp_userpreferredlcid int

--adx_identity_locallogindisabled
if not exists (select 1 from information_schema.columns where table_name = 'contact' and column_name = 'adx_identity_locallogindisabled')
    alter table contact add adx_identity_locallogindisabled bit

