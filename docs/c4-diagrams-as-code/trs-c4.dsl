workspace {

    !identifiers hierarchical

    model {
        citizen = person "User (Citizen)"
        support-user = person "Support User"

        softwareSystem = softwareSystem "Teaching Record System"{

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

            aytq = group "Access Your Teaching Qualifications" {

                aytq-web-app = container "AYTQ Web App" {
                    tags "Access Your Teaching Qualifications" "AYTQ Web App"
                }

                container "AYTQ Database" {
                    tags "Access Your Teaching Qualifications" "Database"
                    aytq-web-app -> this "Reads from and writes to"
                }

            }

            gov-one-login = group "One Login" {
                gov-one-login-api = container "GOVUK.OneLogin API" {
                    tags "One Login" "GOVUK.OneLogin API"
                }

            }

            citizen -> aytq-web-app "Visits Service"
            support-user -> trs-web-app "Uses Support Application"
            trs-web-app -> active-directory "Uses AD API to Authenticate User"
            active-directory -> trs-web-app "Return OAUTH claim"
            trs-auth-server -> trs-web-app "Is part of"
            aytq-web-app -> trs-auth-server "OAuth"
            trs-auth-server -> gov-one-login-api "OAuth"
            gov-one-login-api -> trs-auth-server "OAuth"
            trs-auth-server -> dqt-crm-data-layer "Grants Access To"
            trs-auth-server -> aytq-web-app "OAuth"
            gov-one-login-api -> trs-auth-server
            aytq-web-app -> trsApi "Uses"

        }

    }

    views {
        container softwareSystem "Containers_All" {
            include *
            autolayout
        }

        container softwareSystem "Containers_Service1" {
            include ->softwareSystem.teaching-record-system->
            autolayout
        }

        container softwareSystem "Containers_Service2" {
            include ->softwareSystem.active-directory->
            autolayout
        }

        container softwareSystem "Containers_Service3" {
            include ->softwareSystem.aytq-web-app->
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
            element "Service 6" {
                background #DD8BFE
            }
            element "Service 7" {
                background #89ACFF
            }
            element "Service 8" {
                background #FDA9F4
            }

        }

    }

}