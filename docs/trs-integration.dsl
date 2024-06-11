workspace {

  !identifiers hierarchical


  model {

    support-user = person "Support User"
    citizen = person "User (Citizen)"

    softwareSystem = softwareSystem "TRS/DQT Data-Integrations"{

      teaching-record-system = group "Teaching Record System / DQT" {
        trsApi = container "TRS API" {
          tags "Teaching Record System" "TRS API" "New"
          }
        dqtApi = container "DQT Data APis (sftp, file uploads, manual SQL)" {
          tags  "Legacy-To-Be-Decom."
        }
        dqt-legacy-http-Api = container "DQT HTTP API" {
          tags "Legacy-To-Be-Decom."
        }

        tq-secure-email = container "TQ unit secure email" {

        }
      }

       ssis-random = group "Adhoc data jobs" {
        contact-data-retention = container "Contact Data Retention" {
         tags "Legacy-To-Be-Decom."
        }

        ftp-file-retention = container "FTP file retention" {
          tags "Legacy-To-Be-Decom."
        }

        gias-recon = container "GIAS reconciliation" {
          tags "Legacy-To-Be-Decom."
        }

        failed-qts-retention = container "Failed QTS retention" {
          tags "Legacy-To-Be-Decom."
        }
        
        gias-recon-new = container "GIAS reconciliation New" {
          tags "New"
        }
        
        trs-data-retention-new = container "TRS data retention new" {
          tags "New"
        }
        
      }

      tps = group "External Data Share - DfE is data controller" {
        tps-bancs-api = container "TPS Bancs Pension Administration System API" {
          tags "Teacher Pensions TCS" "TPS TCS" "New"
        }
      }

      tpsCapita = group "Capita decomission: Various"  {
        tps-capita-api = container "TPS Capita Pension Administration System File Portal" "legacy" {
          tags "Teacher Pensions capita" "TPS Capita" "Legacy-To-Be-Decom."
        }
      }
      
      dbs = group "Disclosure and Barring Service"  {
        dbs-api = container "DBS" {
          tags "DBS"
        }
      }
      
      qualsequiv = group "External Data Share - DfE not data controller" {
        set-api = container "Society For Education Training" {
          tags "Society For Education Training" "SET" "New"
        }

        scotland-api = container "Education Scotland" {
          tags "Scotland"
        }

        ni-api = container "Education N.I" {
          tags "Northern Ireland"
        }

        ewcnew-api = container "EWC Welsh Teaching Council HTTP API/email" {
          tags "EWC" "New"
        }
        ewclegacy-api = container "EWC Welsh Teaching Council SFTP API" {
          tags "Legacy-To-Be-Decom."
        }
      }

      quals = group "Qualifications & Induction England" {
        register-api = container "Register For ITT" {
          tags "Register"
        }
        afqts = container "Apply For QTS" {
          tags "AFQTS"
        }

        npq-legacy-api = container "CPD: Register for an NPQ" {
          tags "National Professional Qualification" "Legacy-To-Be-Decom."
        }
        ecf-legacy-api = container "CPD: ECF Legacy" {
          tags "ECF Legacy" "Legacy-To-Be-Decom."
        }
        ecf-new-api = container "CPD: Early Careers Framework" {
          tags "ECF New" "New"
        }
        dqt-ab-portal-legacy = container "DQT AB Portal" {
          tags "Appropriate Body Portal" "Legacy-To-Be-Decom."
        }
      }

    set-api -> trsAPI "http: Get TRN"
    set-api -> trsAPI "http: Award QTLS"
    trsAPI ->  set-api "http: Prohibition outcomes"
    

      ewcnew-api -> trsApi "http: Get a TRN "
      ewcnew-api -> trsApi "http: ITT"
      ewcnew-api -> trsApi  "http: Prohibition outcomes"

      ewclegacy-api -> tps-capita-api "http: New TRNs"
      tps-capita-api -> dqtApi "http:  Prohibitions, New TRNs)"
      ewclegacy-api -> tq-secure-email "email: Teaching alerts / prohibitions

      trsApi -> tps-bancs-api  "http: Return a TRN"
      trsApi -> tps-bancs-api "http: Prohibition outcomes"

      tps-bancs-api -> trsApi "http: Request a TRN"
      tps-bancs-api -> trsApi "http: Deceased active and inactive service"
      tps-bancs-api -> trsApi "sftp: Workforce data"

      tps-capita-api -> dqtApi "http: Claim Paymnets Check Return"
      dqtApi -> tps-capita-api "http: Claim Paymnets Check Request"
      tps-capita-api -> dqtApi "http: EWC New Teachers'
      tps-capita-api -> dqtApi "http: Deceased Teachers file"

      dqtApi -> tps-capita-api "http: New records in DQT"
      dqtApi -> tps-capita-api "http: Amend records in DQT"

      dqtApi -> tps-capita-api "http: TRN monthly reconciliation"
      dqtApi -> tps-capita-api "http: Prohib monthly recon"
      dqtApi -> tps-capita-api "http: TRN dupe monthly recon"
      tps-capita-api -> dqtApi "http: TRN dupe monthly recon"

      register-api -> trsApi "http: Create/Update TR (ITT)"
      register-api -> trsApi "http: Award QTS/EYTS instruction post ITT"


      npq-legacy-api -> ecf-legacy-api "http: synch"
      ecf-legacy-api -> dqt-legacy-http-Api "http: Award NPQ"

      ecf-legacy-api -> dqt-legacy-http-Api "http: Read Induction (poling)"
      dqt-ab-portal-legacy -> dqtApi "sftp: File upload from ABs to DQT"

      npq-legacy-api -> trsApi "http: Read TR post One-Login"
      ecf-new-api -> trsApi "http: Complete mandatory induction"

     scotland-api -> tq-secure-email "email: Teaching alerts / prohibs"
     
     tq-secure-email -> ni-api "email: Teaching alerts / prohibs"
     tq-secure-email -> scotland-api "email: Teaching alerts / prohibs"
     //N.I not currently receiving prohibs
     //tq-secure-email ->  ni-api "email: Teaching alerts / prohibs"

     contact-data-retention -> dqtApi "ssis:data recon job"
     ftp-file-retention -> dqtApi "ssis:data recon job"
     gias-recon -> dqtApi "ssis:data recon job"
     failed-qts-retention -> dqtApi "ssis:data recon job"

    trsApi -> ewcnew-api "http: prohibition outcomes"
    afqts -> trsApi "http: Get TRN"
    afqts -> trsApi "http: Award QTS (England, Scotland, NI, Rest of world)"
    dbs-api -> tq-secure-email "email:7 per week + weekly rec + monthly rec
    gias-recon-new -> trsApi "New GIAS reconciliation job"
    trs-data-retention-new -> trsApi "Data retention jobs"

    }

  }

  views {
    container softwareSystem "containers-all-integrations-trs-dqt" {
      include *
      autolayout
    }

     container softwareSystem "containers-teacher-pensions-as-is-and-to-be" {
      include ->softwareSystem.ewcnew-api-> ->softwareSystem.ewclegacy-api-> ->softwareSystem.tps-capita-api-> ->softwareSystem.contact-data-retention-> ->softwareSystem.ftp-file-retention-> ->softwareSystem.gias-recon-> ->softwareSystem.tps-bancs-api->

      autolayout
      }
      container softwareSystem "containers-teacher-pensions-as-is" {
      include ->softwareSystem.ewclegacy-api-> ->softwareSystem.tps-capita-api-> ->softwareSystem.contact-data-retention-> ->softwareSystem.ftp-file-retention-> ->softwareSystem.gias-recon->

      autolayout
      }

     container softwareSystem "containers-teacher-pensions-to-be" {
      include ->softwareSystem.ewcnew-api-> ->softwareSystem.tps-bancs-api->

      autolayout
      }

    styles {


     element "Legacy-To-Be-Decom." {
        background #ec8212
      }
      element "New" {
        background #22D017
      }


    }

  }

}
