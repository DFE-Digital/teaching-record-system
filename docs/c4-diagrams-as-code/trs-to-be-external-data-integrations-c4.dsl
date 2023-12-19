workspace {

  !identifiers hierarchical

  model {

    support-user = person "Support User"
    citizen = person "User (Citizen)"

    softwareSystem = softwareSystem "To Be TRS External Data-Integrations"{

      teaching-record-system = group "Teaching Record System" {
        trsApi = container "TRS API" {
          tags "Teaching Record System" "TRS API"
        }

      }

      tps = group "Teacher Pensions" {
        tps-bancs-api = container "TPS Bancs Pension Administration System API" {
          tags "Teacher Pensions" "TPS"
        }
      }

      ewc = group "EWC" {
        ewc-api = container "EWC Welsh Teaching Council API" {
          tags "Teacher Pensions" "EWC"
        }
      }

      ewc-api -> trsApi "Get a TRN for a EWC registered teacher (to-be)"
      ewc-api -> trsApi "Send EWC teaching training data (to-be)"
      ewc-api -> trsApi "Send EWC prohibition data (to-be)"

      tps-bancs-api -> trsApi "Get a TRN for a teaching pension member (to-be)"
      tps-bancs-api -> trsApi "Send deceased indicator (to-be)"
      tps-bancs-api -> trsApi "Send workforce data (to-be)"
      tps-bancs-api -> trsApi "Send teaching claims payment checks data (to-be)"

      trsApi -> tps-bancs-api "Return a TRN for a teaching pension member (to-be)"
      trsApi -> tps-bancs-api "Return prohibition outcomes (to-be)"

    }

  }

  views {
    container softwareSystem "containers-external-all" {
      include *
      autolayout
    }

    container softwareSystem "containers-trs-tcs-data-integrations" {
      include ->softwareSystem.tps-bancs-api->
      autolayout
    }

     container softwareSystem "containers-trs-ewc-data-integrations" {
      include ->softwareSystem.ewc-api->
      autolayout
    }

    styles {
      element "Person" {
        shape Person
        background #89ACFF
      }
      element "Service API" {
        shape hexagon
      }
      element "Database" {
        shape cylinder
      }
      element "DQT" {
        shape cylinder
        background #F08CA4
      }
      element "Teaching Record System" {
        background #91F0AE
      }
      element "Corporate Sysytems" {
        background #EDF08C
      }
      element "Access Your Teaching Qualifications" {
        background #8CD0F0
      }
      element "One Login" {
        background #FFAC33
      }
      element "EWC" {
        background #DD8BFE
      }
      element "TPS" {
        background #89ACFF
      }
      element "Service 8" {
        background #FDA9F4
      }

    }

  }

}
