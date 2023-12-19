workspace {

  !identifiers hierarchical

  model {

    support-user = person "Support User"
    citizen = person "User (Citizen)"

    softwareSystem = softwareSystem "Teaching Record System Core Containers"{

      teaching-record-system = group "Teaching Record System" {
        trsApi = container "TRS API (formerly known as Qualified Teachers API)" {
          tags "Teaching Record System" "TRS API"
        }
        trs-web-app = container "TRS Web App" {
          tags "Teaching Record System" "TRS Web App"
          trsApi -> this "Is part of"
        }
        trs-database = container "TRS Database" {
          tags "Teaching Record System" "Database"
          trsApi -> this "Reads from and writes to"
        }
        dqt-crm-data-layer = container "DQT D365 Data Layer (Legacy)" {
          tags "Teaching Record System" "DQT"
          trsApi -> this "Reads from and writes to"
        }
        trs-auth-server = container "TRS Auth Server" {
          tags "Teaching Record System" "TRS Auth Server"
        }
      }

      corporate-systems = group "Corporate Sysytems" {
        active-directory = container "DfE Active Directory" {
          tags "Corporate Sysytems" "AD"
        }
      }

      aytq = group "A Teacher Service e.g.Access Your Teaching Qualifications" {

                aytq-web-app = container "AYTQ Web App" {
                    tags "Access Your Teaching Qualifications" "AYTQ Web App"
                }

                container "AYTQ Database" {
                    tags "Access Your Teaching Qualifications" "Database"
                    aytq-web-app -> this "Reads from and writes to"
                }

            }

      support-user -> trs-web-app "Uses Support Application"
      citizen -> aytq-web-app "Visits Service"

      trs-web-app -> active-directory "Uses AD API to Authenticate User"

      active-directory -> trs-web-app "Return OAUTH claim"

      trs-auth-server -> trs-web-app "Is part of"
      trs-auth-server -> dqt-crm-data-layer "Grants Access To"
      trs-auth-server -> trs-database "Reads from and writes to & Grants Access To"

      aytq-web-app -> trs-auth-server "OAuth"
      trs-auth-server -> aytq-web-app "OAuth"
      aytq-web-app -> trsApi "Uses"



    }


  }

  views {
    container softwareSystem "Containers-All" {
      include *
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
