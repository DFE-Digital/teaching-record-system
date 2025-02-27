using System;
using Microsoft.EntityFrameworkCore.Migrations;
using TeachingRecordSystem.Core.Services.DqtReporting;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class DegreeTypesTrainingSubjectsAndCountriesPopulation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "UK");

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("02d718fb-2686-41ee-8819-79266b139ec7"));

            migrationBuilder.AddColumn<string>(
                name: "reference",
                table: "training_subjects",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "training_country_id",
                table: "qualifications",
                type: "character varying(10)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "country_id",
                table: "countries",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(4)",
                oldMaxLength: 4);

            migrationBuilder.AddColumn<string>(
                name: "citizen_names",
                table: "countries",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "official_name",
                table: "countries",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "degree_types",
                columns: table => new
                {
                    degree_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_degree_types", x => x.degree_type_id);
                });

            migrationBuilder.Sql($"ALTER PUBLICATION {DqtReportingService.TrsDbPublicationName} SET TABLE qualifications, tps_establishments, tps_employments, establishments, persons, alerts, alert_types, alert_categories, events, training_providers, countries, training_subjects, degree_types;");

            migrationBuilder.InsertData(
                table: "countries",
                columns: new[] { "country_id", "citizen_names", "name", "official_name" },
                values: new object[,]
                {
                    { "AD", "Andorran", "Andorra", "The Principality of Andorra" },
                    { "AE", "Citizen of the United Arab Emirates", "United Arab Emirates", "The United Arab Emirates" },
                    { "AF", "Afghan", "Afghanistan", "The Islamic Republic of Afghanistan" },
                    { "AG", "Citizen of Antigua and Barbuda", "Antigua and Barbuda", "Antigua and Barbuda" },
                    { "AI", "Anguillan", "Anguilla", "Anguilla" },
                    { "AL", "Albanian", "Albania", "The Republic of Albania" },
                    { "AM", "Armenian", "Armenia", "The Republic of Armenia" },
                    { "AO", "Angolan", "Angola", "The Republic of Angola" },
                    { "AR", "Argentine", "Argentina", "The Argentine Republic" },
                    { "AT", "Austrian", "Austria", "The Republic of Austria" },
                    { "AU", "Australian", "Australia", "The Commonwealth of Australia" },
                    { "AZ", "Azerbaijani", "Azerbaijan", "The Republic of Azerbaijan" },
                    { "BA", "Citizen of Bosnia and Herzegovina", "Bosnia and Herzegovina", "Bosnia and Herzegovina" },
                    { "BAT", "Not applicable", "British Antarctic Territory", "British Antarctic Territory" },
                    { "BB", "Barbadian", "Barbados", "Barbados" },
                    { "BD", "Bangladeshi", "Bangladesh", "The People's Republic of Bangladesh" },
                    { "BE", "Belgian", "Belgium", "The Kingdom of Belgium" },
                    { "BF", "Burkinan", "Burkina Faso", "Burkina Faso" },
                    { "BG", "Bulgarian", "Bulgaria", "The Republic of Bulgaria" },
                    { "BH", "Bahraini", "Bahrain", "The Kingdom of Bahrain" },
                    { "BI", "Burundian", "Burundi", "The Republic of Burundi" },
                    { "BJ", "Beninese", "Benin", "The Republic of Benin" },
                    { "BM", "Bermudan", "Bermuda", "Bermuda" },
                    { "BN", "Bruneian", "Brunei", "Brunei Darussalam" },
                    { "BO", "Bolivian", "Bolivia", "The Plurinational State of Bolivia" },
                    { "BR", "Brazilian", "Brazil", "The Federative Republic of Brazil" },
                    { "BS", "Bahamian", "The Bahamas", "The Commonwealth of The Bahamas" },
                    { "BT", "Bhutanese", "Bhutan", "The Kingdom of Bhutan" },
                    { "BW", "Botswanan", "Botswana", "The Republic of Botswana" },
                    { "BY", "Belarusian", "Belarus", "The Republic of Belarus" },
                    { "BZ", "Belizean", "Belize", "Belize" },
                    { "CA", "Canadian", "Canada", "Canada" },
                    { "CD", "Congolese (DRC)", "Congo (Democratic Republic)", "The Democratic Republic of the Congo" },
                    { "CF", "Central African", "Central African Republic", "The Central African Republic" },
                    { "CG", "Congolese (Republic of the Congo)", "Congo", "The Republic of the Congo" },
                    { "CH", "Swiss", "Switzerland", "The Swiss Confederation" },
                    { "CI", "Ivorian", "Ivory Coast", "The Republic of Côte D’Ivoire" },
                    { "CL", "Chilean", "Chile", "The Republic of Chile" },
                    { "CM", "Cameroonian", "Cameroon", "The Republic of Cameroon" },
                    { "CN", "Chinese", "China", "The People's Republic of China" },
                    { "CO", "Colombian", "Colombia", "The Republic of Colombia" },
                    { "CR", "Costa Rican", "Costa Rica", "The Republic of Costa Rica" },
                    { "CU", "Cuban", "Cuba", "The Republic of Cuba" },
                    { "CV", "Cape Verdean", "Cape Verde", "The Republic of Cabo Verde" },
                    { "CY", "Cypriot", "Cyprus", "The Republic of Cyprus" },
                    { "CZ", "Czech", "Czechia", "The Czech Republic" },
                    { "DE", "German", "Germany", "The Federal Republic of Germany" },
                    { "DJ", "Djiboutian", "Djibouti", "The Republic of Djibouti" },
                    { "DK", "Danish", "Denmark", "The Kingdom of Denmark" },
                    { "DM", "Dominican", "Dominica", "The Commonwealth of Dominica" },
                    { "DO", "Citizen of the Dominican Republic", "Dominican Republic", "The Dominican Republic" },
                    { "DZ", "Algerian", "Algeria", "The People's Democratic Republic of Algeria" },
                    { "EC", "Ecuadorean", "Ecuador", "The Republic of Ecuador" },
                    { "EE", "Estonian", "Estonia", "The Republic of Estonia" },
                    { "EG", "Egyptian", "Egypt", "The Arab Republic of Egypt" },
                    { "ER", "Eritrean", "Eritrea", "The State of Eritrea" },
                    { "ES", "Spanish", "Spain", "The Kingdom of Spain" },
                    { "ET", "Ethiopian", "Ethiopia", "The Federal Democratic Republic of Ethiopia" },
                    { "FI", "Finnish", "Finland", "The Republic of Finland" },
                    { "FJ", "Fijian", "Fiji", "The Republic of Fiji" },
                    { "FK", "Falkland Islander", "Falkland Islands", "Falkland Islands" },
                    { "FM", "Micronesian", "Federated States of Micronesia", "Federated States of Micronesia" },
                    { "FR", "French", "France", "The French Republic" },
                    { "GA", "Gabonese", "Gabon", "The Gabonese Republic" },
                    { "GB", "Briton, British", "United Kingdom", "The United Kingdom of Great Britain and Northern Ireland" },
                    { "GB-CYM", "Briton, British", "Wales", "Wales" },
                    { "GB-ENG", "Briton, British", "England", "England" },
                    { "GB-NIR", "Briton, British", "Northen Ireland", "Northen Ireland" },
                    { "GB-SCT", "Briton, British", "Scotland", "Scotland" },
                    { "GB-WLS", "Briton, British", "Wales", "Wales" },
                    { "GD", "Grenadian", "Grenada", "Grenada" },
                    { "GE", "Georgian", "Georgia", "Georgia" },
                    { "GG", "Guernseyman/Guernseywoman or Giernési, Ridunian, Sarkee as appropriate", "Guernsey, Alderney, Sark", "Bailiwick of Guernsey" },
                    { "GH", "Ghanaian", "Ghana", "The Republic of Ghana" },
                    { "GI", "Gibraltarian", "Gibraltar", "Gibraltar" },
                    { "GM", "Gambian", "The Gambia", "The Republic of The Gambia" },
                    { "GN", "Guinean", "Guinea", "The Republic of Guinea" },
                    { "GQ", "Equatorial Guinean", "Equatorial Guinea", "The Republic of Equatorial Guinea" },
                    { "GR", "Greek", "Greece", "The Hellenic Republic" },
                    { "GS", "Not applicable", "South Georgia and South Sandwich Islands", "South Georgia and the South Sandwich Islands" },
                    { "GT", "Guatemalan", "Guatemala", "The Republic of Guatemala" },
                    { "GW", "Citizen of Guinea-Bissau", "Guinea-Bissau", "The Republic of Guinea-Bissau" },
                    { "GY", "Guyanese", "Guyana", "The Co-operative Republic of Guyana" },
                    { "HN", "Honduran", "Honduras", "The Republic of Honduras" },
                    { "HR", "Croatian", "Croatia", "The Republic of Croatia" },
                    { "HT", "Haitian", "Haiti", "The Republic of Haiti" },
                    { "HU", "Hungarian", "Hungary", "Hungary" },
                    { "ID", "Indonesian", "Indonesia", "The Republic of Indonesia" },
                    { "IE", "Irish", "Ireland", "Ireland" },
                    { "IL", "Israeli", "Israel", "The State of Israel" },
                    { "IM", "Manxman/Manxwoman or Manx", "Isle of Man", "Isle of Man" },
                    { "IN", "Indian", "India", "The Republic of India" },
                    { "IO", "Not applicable", "British Indian Ocean Territory", "The British Indian Ocean Territory" },
                    { "IQ", "Iraqi", "Iraq", "The Republic of Iraq" },
                    { "IR", "Iranian", "Iran", "The Islamic Republic of Iran" },
                    { "IS", "Icelandic", "Iceland", "Iceland" },
                    { "IT", "Italian", "Italy", "The Italian Republic" },
                    { "JE", "Jerseyman/Jerseywoman", "Jersey", "Bailiwick of Jersey" },
                    { "JM", "Jamaican", "Jamaica", "Jamaica" },
                    { "JO", "Jordanian", "Jordan", "The Hashemite Kingdom of Jordan" },
                    { "JP", "Japanese", "Japan", "Japan" },
                    { "KE", "Kenyan", "Kenya", "The Republic of Kenya" },
                    { "KG", "Kyrgyz", "Kyrgyzstan", "The Kyrgyz Republic" },
                    { "KH", "Cambodian", "Cambodia", "The Kingdom of Cambodia" },
                    { "KI", "Citizen of Kiribati", "Kiribati", "The Republic of Kiribati" },
                    { "KM", "Comoran", "Comoros", "The Union of the Comoros" },
                    { "KN", "Citizen of St Christopher (St Kitts) and Nevis", "St Kitts and Nevis", "The Federation of Saint Christopher and Nevis" },
                    { "KP", "North Korean", "North Korea", "The Democratic People's Republic of Korea" },
                    { "KR", "South Korean", "South Korea", "The Republic of Korea" },
                    { "KW", "Kuwaiti", "Kuwait", "The State of Kuwait" },
                    { "KY", "Cayman Islander, Caymanian", "Cayman Islands", "Cayman Islands" },
                    { "KZ", "Kazakh", "Kazakhstan", "The Republic of Kazakhstan" },
                    { "LA", "Lao", "Laos", "The Lao People's Democratic Republic" },
                    { "LB", "Lebanese", "Lebanon", "The Lebanese Republic" },
                    { "LC", "St Lucian", "St Lucia", "Saint Lucia" },
                    { "LI", "Liechtenstein citizen", "Liechtenstein", "The Principality of Liechtenstein" },
                    { "LK", "Sri Lankan", "Sri Lanka", "The Democratic Socialist Republic of Sri Lanka" },
                    { "LR", "Liberian", "Liberia", "The Republic of Liberia" },
                    { "LS", "Citizen of Lesotho", "Lesotho", "The Kingdom of Lesotho" },
                    { "LT", "Lithuanian", "Lithuania", "The Republic of Lithuania" },
                    { "LU", "Luxembourger", "Luxembourg", "The Grand Duchy of Luxembourg" },
                    { "LV", "Latvian", "Latvia", "The Republic of Latvia" },
                    { "LY", "Libyan", "Libya", "State of Libya" },
                    { "MA", "Moroccan", "Morocco", "The Kingdom of Morocco" },
                    { "MC", "Monegasque", "Monaco", "The Principality of Monaco" },
                    { "MD", "Moldovan", "Moldova", "The Republic of Moldova" },
                    { "ME", "Montenegrin", "Montenegro", "Montenegro" },
                    { "MG", "Citizen of Madagascar", "Madagascar", "The Republic of Madagascar" },
                    { "MH", "Marshallese", "Marshall Islands", "The Republic of the Marshall Islands" },
                    { "MK", "Macedonian", "North Macedonia", "Republic of North Macedonia" },
                    { "ML", "Malian", "Mali", "The Republic of Mali" },
                    { "MM", "Citizen of Myanmar", "Myanmar (Burma)", "The Republic of the Union of Myanmar" },
                    { "MN", "Mongolian", "Mongolia", "Mongolia" },
                    { "MR", "Mauritanian", "Mauritania", "The Islamic Republic of Mauritania" },
                    { "MS", "Montserratian", "Montserrat", "Montserrat" },
                    { "MT", "Maltese", "Malta", "The Republic of Malta" },
                    { "MU", "Mauritian", "Mauritius", "The Republic of Mauritius" },
                    { "MV", "Maldivian", "Maldives", "The Republic of Maldives" },
                    { "MW", "Malawian", "Malawi", "The Republic of Malawi" },
                    { "MX", "Mexican", "Mexico", "The United Mexican States" },
                    { "MY", "Malaysian", "Malaysia", "Malaysia" },
                    { "MZ", "Mozambican", "Mozambique", "The Republic of Mozambique" },
                    { "NA", "Namibian", "Namibia", "The Republic of Namibia" },
                    { "NE", "Nigerien", "Niger", "The Republic of Niger" },
                    { "NG", "Nigerian", "Nigeria", "The Federal Republic of Nigeria" },
                    { "NI", "Nicaraguan", "Nicaragua", "The Republic of Nicaragua" },
                    { "NL", "Dutch", "Netherlands", "The Kingdom of the Netherlands" },
                    { "NO", "Norwegian", "Norway", "The Kingdom of Norway" },
                    { "NP", "Nepalese", "Nepal", "Nepal" },
                    { "NR", "Nauruan", "Nauru", "The Republic of Nauru" },
                    { "NZ", "New Zealander", "New Zealand", "New Zealand" },
                    { "OM", "Omani", "Oman", "The Sultanate of Oman" },
                    { "PA", "Panamanian", "Panama", "The Republic of Panama" },
                    { "PE", "Peruvian", "Peru", "The Republic of Peru" },
                    { "PG", "Papua New Guinean", "Papua New Guinea", "The Independent State of Papua New Guinea" },
                    { "PH", "Filipino", "Philippines", "The Republic of the Philippines" },
                    { "PK", "Pakistani", "Pakistan", "The Islamic Republic of Pakistan" },
                    { "PL", "Polish", "Poland", "The Republic of Poland" },
                    { "PN", "Pitcairn Islander or Pitcairner", "Pitcairn, Henderson, Ducie and Oeno Islands", "Pitcairn, Henderson, Ducie and Oeno Islands" },
                    { "PT", "Portuguese", "Portugal", "The Portuguese Republic" },
                    { "PW", "Palauan", "Palau", "The Republic of Palau" },
                    { "PY", "Paraguayan", "Paraguay", "The Republic of Paraguay" },
                    { "QA", "Qatari", "Qatar", "The State of Qatar" },
                    { "RO", "Romanian", "Romania", "Romania" },
                    { "RS", "Serbian", "Serbia", "The Republic of Serbia" },
                    { "RU", "Russian", "Russia", "The Russian Federation" },
                    { "RW", "Rwandan", "Rwanda", "The Republic of Rwanda" },
                    { "SA", "Saudi Arabian", "Saudi Arabia", "The Kingdom of Saudi Arabia" },
                    { "SB", "Solomon Islander", "Solomon Islands", "Solomon Islands" },
                    { "SC", "Citizen of Seychelles", "Seychelles", "The Republic of Seychelles" },
                    { "SD", "Sudanese", "Sudan", "The Republic of the Sudan" },
                    { "SE", "Swedish", "Sweden", "The Kingdom of Sweden" },
                    { "SG", "Singaporean", "Singapore", "The Republic of Singapore" },
                    { "SH", "St Helenian or Tristanian as appropriate. Ascension has no indigenous population", "St Helena, Ascension and Tristan da Cunha", "St Helena, Ascension and Tristan da Cunha" },
                    { "SI", "Slovenian", "Slovenia", "The Republic of Slovenia" },
                    { "SK", "Slovak", "Slovakia", "The Slovak Republic" },
                    { "SL", "Sierra Leonean", "Sierra Leone", "The Republic of Sierra Leone" },
                    { "SM", "San Marinese", "San Marino", "The Republic of San Marino" },
                    { "SN", "Senegalese", "Senegal", "The Republic of Senegal" },
                    { "SO", "Somali", "Somalia", "Federal Republic of Somalia" },
                    { "SR", "Surinamese", "Suriname", "The Republic of Suriname" },
                    { "SS", "South Sudanese", "South Sudan", "The Republic of South Sudan" },
                    { "ST", "Sao Tomean", "Sao Tome and Principe", "The Democratic Republic of Sao Tome and Principe" },
                    { "SV", "Salvadorean", "El Salvador", "The Republic of El Salvador" },
                    { "SY", "Syrian", "Syria", "The Syrian Arab Republic" },
                    { "SZ", "Swazi", "Eswatini", "Kingdom of Eswatini" },
                    { "TC", "Turks and Caicos Islander", "Turks and Caicos Islands", "Turks and Caicos Islands" },
                    { "TD", "Chadian", "Chad", "The Republic of Chad" },
                    { "TG", "Togolese", "Togo", "The Togolese Republic" },
                    { "TH", "Thai", "Thailand", "The Kingdom of Thailand" },
                    { "TJ", "Tajik", "Tajikistan", "The Republic of Tajikistan" },
                    { "TL", "East Timorese", "East Timor", "The Democratic Republic of Timor-Leste" },
                    { "TM", "Turkmen", "Turkmenistan", "Turkmenistan" },
                    { "TN", "Tunisian", "Tunisia", "Republic of Tunisia" },
                    { "TO", "Tongan", "Tonga", "The Kingdom of Tonga" },
                    { "TR", "Turk, Turkish", "Turkey", "Republic of Türkiye" },
                    { "TT", "Trinidad and Tobago citizen", "Trinidad and Tobago", "The Republic of Trinidad and Tobago" },
                    { "TV", "Tuvaluan", "Tuvalu", "Tuvalu" },
                    { "TZ", "Tanzanian", "Tanzania", "The United Republic of Tanzania" },
                    { "UA", "Ukrainian", "Ukraine", "Ukraine" },
                    { "UG", "Ugandan", "Uganda", "The Republic of Uganda" },
                    { "US", "American", "United States", "The United States of America" },
                    { "UY", "Uruguayan", "Uruguay", "The Oriental Republic of Uruguay" },
                    { "UZ", "Uzbek", "Uzbekistan", "The Republic of Uzbekistan" },
                    { "VA", "Vatican citizen", "Vatican City", "Vatican City State" },
                    { "VC", "Vincentian", "St Vincent", "Saint Vincent and the Grenadines" },
                    { "VE", "Venezuelan", "Venezuela", "The Bolivarian Republic of Venezuela" },
                    { "VG", "British Virgin Islander", "British Virgin Islands", "The Virgin Islands" },
                    { "VN", "Vietnamese", "Vietnam", "The Socialist Republic of Viet Nam" },
                    { "VU", "Citizen of Vanuatu", "Vanuatu", "The Republic of Vanuatu" },
                    { "WS", "Samoan", "Samoa", "The Independent State of Samoa" },
                    { "XK", "Kosovan", "Kosovo", "The Republic of Kosovo" },
                    { "XQZ", "Not applicable", "Akrotiri", "Akrotiri" },
                    { "XXD", "Not applicable", "Dhekelia", "Dhekelia" },
                    { "YE", "Yemeni", "Yemen", "The Republic of Yemen" },
                    { "ZA", "South African", "South Africa", "The Republic of South Africa" },
                    { "ZM", "Zambian", "Zambia", "The Republic of Zambia" },
                    { "ZW", "Zimbabwean", "Zimbabwe", "The Republic of Zimbabwe" }
                });

            migrationBuilder.InsertData(
                table: "degree_types",
                columns: new[] { "degree_type_id", "is_active", "name" },
                values: new object[,]
                {
                    { new Guid("02e4f052-bd3b-490c-bea0-bd390bc5b227"), true, "BEng (Hons)/Education" },
                    { new Guid("1fcd0543-14d1-4866-b961-2812239eec06"), true, "BA (Hons) Combined Studies/Education of the Deaf" },
                    { new Guid("2f7a914f-f95f-421a-a55e-60ed88074cf2"), true, "Postgraduate Art Teachers Certificate" },
                    { new Guid("311ef3a9-6aba-4314-acf8-4bba46aebe9e"), true, "Graduate Certificate in Education" },
                    { new Guid("35d04fbb-c19b-4cd9-8fa6-39d90883a52a"), true, "BSc" },
                    { new Guid("40a85dd0-8512-438e-8040-649d7d677d07"), true, "Postgraduate Certificate in Education" },
                    { new Guid("4c0578b6-e9af-4c98-a3bc-038343b1436a"), true, "Certificate in Education (FE)" },
                    { new Guid("4ec0a016-07eb-47b4-8cdd-e276945d401e"), true, "Qualification gained in Europe" },
                    { new Guid("54f72259-23b2-4d79-a6ca-c185084c903f"), true, "PGCE (Articled Teachers Scheme)" },
                    { new Guid("63d80489-ee3d-43af-8c4a-1d6ae0d65f68"), true, "Postgraduate Diploma in Education" },
                    { new Guid("6d07695e-5b5b-4dd4-997c-420e4424255c"), true, "Graduate Diploma" },
                    { new Guid("72dbd225-6a7e-42af-b918-cf284bccaeef"), true, "BSc/Education" },
                    { new Guid("7330e2f5-dd02-4498-9b7c-5cf99d7d0cab"), true, "BSc/Certificate in Education" },
                    { new Guid("7471551d-132e-4c5d-82cc-a41190f01245"), true, "Teachers Certificate FE" },
                    { new Guid("78a8d033-06c8-4beb-b415-5907f5f39207"), true, "Postgraduate Certificate in Education" },
                    { new Guid("7c703efb-a5d3-41d3-b243-ee8974695dd8"), true, "Professional Graduate Diploma in Education" },
                    { new Guid("826f6cc9-e5f8-4ce7-a5ee-6194d19f7e22"), true, "BA with Intercalated PGCE" },
                    { new Guid("84e541d5-d55a-4d44-bc52-983322c1453f"), true, "BA Education" },
                    { new Guid("85ab05c8-be3a-4a72-9d04-9efc30d87289"), true, "BTech/Education" },
                    { new Guid("8d0440f2-f731-4ac2-b49c-927af903bf59"), true, "Postgraduate Art Teachers Diploma" },
                    { new Guid("969c89e7-35b8-43d8-be07-17ef76c3b4bf"), true, "BA" },
                    { new Guid("984af9ff-bb42-48ac-a634-f2c4954c8158"), true, "BTech (Hons)/Education" },
                    { new Guid("9959e914-f4f4-44cd-909f-e170a0f1ac42"), true, "BSc (Hons)" },
                    { new Guid("9b35bdfa-cbd5-44fd-a45a-6167e7559de7"), true, "BEd (Hons)" },
                    { new Guid("9cf31754-5ac5-46a1-99e5-5c98cba1b881"), true, "Unknown" },
                    { new Guid("9f4af7a8-14a5-4b34-af72-dc04c5245fc7"), true, "BSc (Hons) with Intercalated PGCE" },
                    { new Guid("ae28704f-cfa3-4c6e-a47d-c4a048383018"), true, "Professional PGCE" },
                    { new Guid("b02914fe-3a30-4f7c-94ec-0cd87a75834d"), true, "Teachers Certificate" },
                    { new Guid("b44e02b1-7257-4609-a9e5-46ed72c91b98"), true, "Certificate in Education" },
                    { new Guid("b7b0635a-22c3-41e3-a420-77b9b58c51cd"), true, "BEd" },
                    { new Guid("b96d4ad9-6da0-4dad-a9e4-e35b2a0838eb"), true, "BA Combined Studies/Education of the Deaf" },
                    { new Guid("b9ef569f-fb23-4f31-842e-a0d940d911be"), true, "Graduate Certificate in Science and Education" },
                    { new Guid("bc6c1f17-26a5-4987-9d50-2615e138e281"), true, "Degree Equivalent (this will include foreign qualifications)" },
                    { new Guid("c06660d3-8964-40d0-985f-80b25eced995"), true, "BA (Hons) with Intercalated PGCE" },
                    { new Guid("c584eb2f-1419-4870-a230-5d81ae9b5f77"), true, "Postgraduate Certificate in Education (Further Education)" },
                    { new Guid("d82637a0-33ed-4181-b00b-9d53e7853552"), true, "Graduate Certificate in Mathematics and Education" },
                    { new Guid("d8e267d2-ed85-4eee-8119-45d0c6cc5f6b"), true, "Professional Graduate Certificate in Education" },
                    { new Guid("dba69141-4101-4e05-80e0-524e3967d589"), true, "Undergraduate Master of Teaching" },
                    { new Guid("dbb7c27b-8a27-4a94-908d-4b4404acebd5"), true, "BA (Hons)" },
                    { new Guid("e0b22ab0-fa25-4c31-aa61-cab56a4e6a2b"), true, "PGCE" },
                    { new Guid("eb04bde4-9a7b-4c68-b7e1-a9254e0e7467"), true, "BA Education Certificate" },
                    { new Guid("fc85c7e2-7fd7-4585-8c37-c29852e6027f"), true, "Degree" }
                });

            migrationBuilder.InsertData(
                table: "training_subjects",
                columns: new[] { "training_subject_id", "is_active", "name", "reference" },
                values: new object[,]
                {
                    { new Guid("002bf98f-fd1e-422d-a951-1cd4dd29d4ce"), false, "German Lang, Lit & Cult", "R8820" },
                    { new Guid("00447627-36a3-42c1-9336-5cc4d24e46d3"), true, "geomorphology", "101064" },
                    { new Guid("005ceb13-0881-4ed0-bb19-08d49c3763a0"), true, "musicology", "100667" },
                    { new Guid("006fa254-d985-4f43-82bf-54d49c4fa91c"), true, "rehabilitation studies", "101289" },
                    { new Guid("008ff140-767e-46c0-ac32-85292e33da8f"), true, "medieval history", "100309" },
                    { new Guid("00996bd5-f2f5-4423-bb44-162efb24acb8"), true, "ship design", "100568" },
                    { new Guid("00b2d1d0-628a-4a1d-943a-3e317e9cf45c"), true, "learning disabilities nursing", "100286" },
                    { new Guid("00bd6592-db8b-4eea-89b7-be0922576aa0"), false, "Italian Language & Studies", "R3101" },
                    { new Guid("01561d4b-5dd5-45ae-9989-5a0b713251da"), false, "Business Management Studies", "N1001" },
                    { new Guid("015d862e-2aed-49df-9e5f-d17b0d426972"), true, "food and beverage production", "100526" },
                    { new Guid("0177cdf3-d1e8-4db5-8c44-6421a0f013ce"), true, "James Joyce studies", "101479" },
                    { new Guid("01a3070e-8d75-43ed-a6c7-14bbe0a8b42b"), true, "pest management", "100884" },
                    { new Guid("01fdac1e-d370-4a0e-a390-4df05436c839"), true, "stochastic processes", "101033" },
                    { new Guid("02b41511-fa57-4c46-9597-6b2d3a8b74d3"), true, "health and safety management", "100866" },
                    { new Guid("02e05cd6-1962-46fb-8f5e-8ef2ac23d162"), true, "electrical engineering", "100164" },
                    { new Guid("03358e6a-b8af-4fb0-b83c-dcffb5578e31"), true, "colonial and post-colonial literature", "101108" },
                    { new Guid("033d952b-4f47-47f6-a4c8-f11b30d8b763"), false, "Commerce", "N1206" },
                    { new Guid("03497a81-2f36-4eb2-aaab-5dbe501b6d98"), true, "popular music composition", "101451" },
                    { new Guid("0367b82f-8cfd-420a-bf8a-71dbf878b72e"), true, "Hungarian studies", "101311" },
                    { new Guid("036aedb3-4173-44f3-97ab-0eaba86e03b7"), true, "work-based learning", "101277" },
                    { new Guid("0416a4f2-4a3a-40ee-847a-bdaa5a0727f2"), false, "Arts Administration", "W9900" },
                    { new Guid("041e55de-d9de-45aa-8710-7bec63db7e13"), true, "biometry", "100865" },
                    { new Guid("042590b0-ce6e-4024-981c-e0bc85af3ea7"), true, "health informatics", "100994" },
                    { new Guid("04284cc5-681f-4545-9918-5e3e67196b4a"), true, "microelectronic engineering", "100168" },
                    { new Guid("0477d8a5-ccbb-47bd-a86f-e1405128fc08"), true, "psychometrics", "101383" },
                    { new Guid("049a7a5d-d3ac-415b-85df-58e82b2dcb5f"), false, "Science and Technology", "F9605" },
                    { new Guid("04e8008c-0e2e-4888-80ef-2ffb5f1e5ab4"), true, "Chinese medical techniques", "100236" },
                    { new Guid("053003f2-8ce0-4943-b0b1-99baf4fd0239"), false, "Mathematics & Computer Studies", "G5004" },
                    { new Guid("05352933-c094-4582-8015-91019dae260e"), true, "chemical physics", "100416" },
                    { new Guid("05389dd4-79f3-484c-ad6e-2b09a3c80947"), false, "Horticultural Science", "D2501" },
                    { new Guid("058782be-f887-4851-bb16-18d3620eacdd"), true, "public relations", "100076" },
                    { new Guid("0636913d-783f-4d6d-89cf-f8c3f6f2e0f7"), true, "sculpture", "100592" },
                    { new Guid("0670dc56-acf9-4531-b3e9-fa9472833586"), true, "climate change", "101070" },
                    { new Guid("06fa8277-7809-421d-8a24-f3ab8130149b"), true, "marine sciences", "100418" },
                    { new Guid("07b4718e-18be-4d1d-beed-731630fa5c27"), true, "applied psychology", "100493" },
                    { new Guid("07b68d5f-8182-4a3c-8392-d6387875be1c"), false, "French Lang and Literature", "R1103" },
                    { new Guid("07d698d1-4ee3-47ce-8b25-70a1684e0abd"), false, "Drama With English", "W9918" },
                    { new Guid("07d95576-5741-46c8-b3dd-8dd3877e07fe"), true, "computer forensics", "100385" },
                    { new Guid("07e8e170-5b8d-4efc-9416-f32300bce270"), false, "Economics and Business Ed", "N9702" },
                    { new Guid("080ae67b-901a-4fa4-a36c-d911ee2a581e"), false, "Design & Tech : Home Economics", "W9927" },
                    { new Guid("08711a13-26f5-4c60-8de8-8aec7b9691b5"), false, "Rural Studies", "F9010" },
                    { new Guid("08736a66-ecbb-4679-9f0e-93cdfae37fb0"), true, "Breton language", "101419" },
                    { new Guid("08966417-8e15-4ac3-8ae5-182a516b38c7"), false, "Intergrated Studies", "F9629" },
                    { new Guid("08a39573-be7d-465c-9fb7-de4436dbb393"), true, "modern Middle Eastern literature", "101196" },
                    { new Guid("08b5901c-2b50-45d3-8503-b10259f77f13"), false, "History and Geography", "Z0101" },
                    { new Guid("09483d19-dfe8-4e08-abe7-e9de379efb42"), true, "natural language processing", "100961" },
                    { new Guid("097175c6-113d-4ffa-b25c-2f68e46f993b"), true, "Italian studies", "100327" },
                    { new Guid("097620e0-eb23-4fe8-be42-97c4aad5bc08"), false, "Special Education Studies", "X9014" },
                    { new Guid("09819c9d-9588-4f69-b71e-020dfebbd0fa"), true, "international development", "100488" },
                    { new Guid("09ab681b-966a-4c0d-9bc4-e97a0811312d"), true, "financial mathematics", "100401" },
                    { new Guid("09b16d6c-03aa-4c5c-9303-79c270420bf5"), true, "biochemistry", "100344" },
                    { new Guid("09d47234-0d31-4356-881f-b3f4db01af7f"), true, "high performance computing", "100741" },
                    { new Guid("09db8708-b306-4495-b183-7161d5efd5ec"), true, "health sciences", "100246" },
                    { new Guid("0a08ee0b-2bf7-44e1-8fff-72ca8619a3c1"), false, "Studies In Art", "W9922" },
                    { new Guid("0a4a7bcb-7f61-4ba8-af8c-b4df02898d29"), false, "Drama and Contextual Studies", "W4009" },
                    { new Guid("0a5e8fa8-507e-4434-8004-b221a5adcf0e"), true, "Welsh literature", "101163" },
                    { new Guid("0b174977-18ba-4d6e-ae85-4601cf297b8a"), true, "requirements engineering", "100821" },
                    { new Guid("0b34aa64-241b-45dd-bd91-664f66dbd0f3"), false, "Computing and Technology", "G9007" },
                    { new Guid("0b353b77-d02d-4e5e-b1d0-f963f27d4ed4"), true, "popular music", "100841" },
                    { new Guid("0b609275-68df-4077-8a03-04031603d6a1"), true, "African literature", "101188" },
                    { new Guid("0b765f82-8223-49e5-9f69-ed89b7adba68"), true, "plant biochemistry", "100932" },
                    { new Guid("0ba8308c-fc2e-4f05-97f1-dc99d34f6c31"), true, "occupational health", "100248" },
                    { new Guid("0bae8523-c607-4345-9cbb-d7284d0e0d14"), false, "Physics/Engineering Science", "F6006" },
                    { new Guid("0bc8b844-e96c-4fca-b73b-3c0d3615fb3f"), false, "Education Of The Part. Hearing", "X6006" },
                    { new Guid("0bcaf443-de1d-460a-ba5c-f11baff3bc79"), true, "electrical power generation", "101353" },
                    { new Guid("0bce3dad-3452-4e8b-8232-158a71544698"), true, "polymer science and technology", "100145" },
                    { new Guid("0c1d6a94-ca55-4b23-91e3-a4f64f8e2115"), false, "Norwegian", "R7500" },
                    { new Guid("0c267114-e372-437d-9933-446b3cd4fd02"), true, "Italian language", "100326" },
                    { new Guid("0c9fa4b9-5a50-4842-bde4-3d13f0a103a1"), true, "ecology", "100347" },
                    { new Guid("0d29d5fc-242b-4f0b-b16c-6c69bbd94bff"), true, "human biology", "100350" },
                    { new Guid("0d813255-b8ae-4686-b694-35ff085a4d7c"), false, "General Topics In Education", "X8890" },
                    { new Guid("0dada6be-3bf3-49b7-8c97-d00f486371c8"), true, "Portuguese language", "101142" },
                    { new Guid("0e21bc65-ceac-4af3-88d7-88798cac7c5b"), true, "railway engineering", "100157" },
                    { new Guid("0e3352a8-44b7-4963-8ffc-03daeb9223a2"), true, "computer games", "101267" },
                    { new Guid("0f11491d-d1d3-4aa4-9ff9-e905ae6825ae"), false, "Geography (As A Science)", "F8000" },
                    { new Guid("0f3c8673-8bf6-4774-8eea-7063899f6dbb"), false, "General Studies In Humanities", "Y3200" },
                    { new Guid("0f68664e-ba97-454d-ab56-0349579cf647"), true, "political sociology", "100629" },
                    { new Guid("0fa6cf72-549d-424e-affa-52cb6114ffb3"), false, "Creative Studies (Art)", "W1008" },
                    { new Guid("0fbdda8a-cb18-4a37-af1b-ca651de0d1dc"), true, "Margaret Atwood studies", "101480" },
                    { new Guid("0fbec27a-048e-41fd-a14f-da97ef67626f"), false, "Hispanic Studies", "R4003" },
                    { new Guid("100d2adc-c1da-4ea3-a04b-700551d9e587"), true, "Vietnamese language", "101369" },
                    { new Guid("100fb49b-1b22-4973-8547-7eafc0014715"), true, "economic systems", "100606" },
                    { new Guid("101d29db-2e4b-4a8d-9dbe-3ba97886713a"), true, "music technology", "100221" },
                    { new Guid("102e59ef-4574-4185-916a-a42b60cd6fef"), true, "history of architecture", "100782" },
                    { new Guid("10624120-7343-4f49-9a04-57a25bca422f"), true, "community theatre", "100710" },
                    { new Guid("1137e97c-4f30-401c-a32b-946ff6e77813"), true, "crop production", "100947" },
                    { new Guid("115600e6-88cf-420d-b084-6d11531e1a2e"), true, "engineering physics", "101061" },
                    { new Guid("11b1bafe-d26c-466c-b058-420aaf45490f"), false, "Victorian Studies", "V1201" },
                    { new Guid("11f926e8-e3bc-43c1-bc8e-e029d3711f81"), true, "emergency nursing", "100284" },
                    { new Guid("122a823b-810d-4f62-9143-6a1b8a0f2275"), true, "maritime archaeology", "101261" },
                    { new Guid("12828dec-1ebc-4310-a2b3-3e1d622e79ea"), true, "epilepsy care", "101333" },
                    { new Guid("128d8774-b7ad-486a-9ff5-4a91c295d2b5"), true, "astrophysics", "100415" },
                    { new Guid("130113b9-4100-44da-a17e-3c67158685e1"), true, "pure mathematics", "100405" },
                    { new Guid("132a4b3f-69c7-41cd-9527-b8e7c6d39ec9"), false, "English As A Foreign Language", "Q3700" },
                    { new Guid("1349b1af-8631-4477-ac30-900675ca7688"), false, "Further Ed. Teacher Training", "X5001" },
                    { new Guid("134f93b0-463f-447e-8336-36fab9ef2834"), true, "popular music performance", "100657" },
                    { new Guid("1377b304-0103-4f4e-ad71-6ff55c7cae46"), false, "Fellow Royal Photographic Society", "10519" },
                    { new Guid("1389bdcc-6bba-45e2-bac9-a1e954239124"), false, "Broad Balanced Science", "F9608" },
                    { new Guid("139ca859-dc3e-4d74-a404-eaa27fcc7bdf"), false, "Bed (UK)", "Z0080" },
                    { new Guid("13bd3303-d7d6-49e4-aad7-e38469e1d598"), true, "market research", "100846" },
                    { new Guid("13de25f8-fc36-4154-b751-d0ab535c662a"), true, "forensic psychology", "100387" },
                    { new Guid("1418a886-7be4-4de9-b58c-213f7a8017b1"), false, "Modern Studies", "Y3201" },
                    { new Guid("146aeb34-7a39-449f-abc3-61a4b4cdd4f2"), false, "Icelandic", "ZZ9005" },
                    { new Guid("149c7347-0ea9-4424-aacc-21b54e68479e"), false, "History and The Environment", "V9003" },
                    { new Guid("14d25849-7c42-42ff-8447-16e940cda458"), true, "film and sound recording", "100890" },
                    { new Guid("14fb4100-daad-43a3-bf9d-7cc177f5727c"), true, "cellular pathology", "100540" },
                    { new Guid("15048d02-d96c-44a5-be08-d2d1254ccf35"), false, "Language Studies", "Q1400" },
                    { new Guid("15a03936-e4ec-4942-8e1e-40675ba4c10a"), true, "pathology", "100274" },
                    { new Guid("15b11065-d6cf-4fd5-a4ee-8f30adefbf0f"), true, "mapping science", "101058" },
                    { new Guid("15ba6775-9cd7-4213-986b-42bda9de0f37"), true, "Thomas Pynchon studies", "101474" },
                    { new Guid("15c8a0ef-b13d-4bf4-8e81-1e25b232d99d"), true, "broadcast journalism", "100439" },
                    { new Guid("15d6e452-fe60-4306-b477-bbc523e5ffd7"), true, "marine physics", "101390" },
                    { new Guid("163a9ae9-28d0-44ad-a1c3-2fad5b884069"), true, "operating department practice", "100273" },
                    { new Guid("1692ab8b-1e2c-4526-a784-c7565d0452da"), true, "secondary education", "100465" },
                    { new Guid("16a152b8-214b-4f46-91f2-fbfe04a9dc97"), true, "veterinary medicine", "100531" },
                    { new Guid("1708895b-2a60-4a86-8ead-be84dce2ab42"), false, "Tech (Home Economics/Textiles)", "W9906" },
                    { new Guid("1717c366-c869-444d-a1db-e387b564432a"), false, "Expressive Arts (Music)", "W3003" },
                    { new Guid("174f8921-6ce5-4c97-b154-05478409b7c0"), true, "Urdu language", "101175" },
                    { new Guid("176547b7-970b-4320-b3a1-50f500ecbeea"), true, "psychology of ageing", "100958" },
                    { new Guid("176f4491-75d6-40db-a57d-d6ad8179506c"), true, "South East Asian history", "100773" },
                    { new Guid("178c0d7f-5583-4f40-a8f6-e5f3d1533b06"), true, "Coptic language", "101414" },
                    { new Guid("179c9c1b-b059-4ba8-992b-f434f19fe368"), true, "musical theatre", "100035" },
                    { new Guid("17a83eeb-f022-4cdf-9142-3e818b02cb7d"), true, "engineering and industrial mathematics", "101028" },
                    { new Guid("180fd8ff-a74f-4c37-b200-3775a0098601"), true, "social history", "100312" },
                    { new Guid("18897954-105e-4ff7-8c10-831ee70b7072"), true, "broadcast engineering", "100539" },
                    { new Guid("18b3d2e8-755a-40aa-ac72-279660dbb60f"), false, "Art & Crafts", "W9000" },
                    { new Guid("18be8c05-cdbc-4cb9-aebe-62b4ac4d6a77"), false, "Education of special education needs children", "X6005" },
                    { new Guid("18c20e91-b116-47db-8003-2d446a368119"), true, "herbal medicine", "100237" },
                    { new Guid("18cdb3e7-3e4f-40bb-8bd5-91708e465eac"), true, "music education and teaching", "100642" },
                    { new Guid("18d039ca-7602-40d0-9e73-4a9d3a082b4e"), true, "physiology", "100262" },
                    { new Guid("18e14dbe-0d5f-480f-8d14-a852e7cac8a5"), true, "environmental sciences", "100381" },
                    { new Guid("18e55e1e-b040-4f85-8d7b-e9bb246852bb"), false, "English For Non-Native Speakrs", "Q3004" },
                    { new Guid("192027d5-8de0-479d-aa6f-4660fa668dba"), true, "Japanese studies", "101168" },
                    { new Guid("192c1cc6-9dd9-44a4-b3fa-13857c2bdd39"), false, "Literature (Anglo-Irish)", "P4602" },
                    { new Guid("19428eee-01f0-43ac-9eea-2e5363befe81"), true, "biophysical science", "100949" },
                    { new Guid("198d6afd-b83b-4ef2-b7f2-6cabcd53fce0"), true, "community music", "100854" },
                    { new Guid("199589d3-6196-4c4b-a70e-eba0d37ce656"), true, "international studies", "101288" },
                    { new Guid("19b439e5-b05f-4d02-8b91-2af1f5cdd112"), false, "General Language Studies", "T8880" },
                    { new Guid("19b578fb-58e4-4251-b08e-acb5d28cd00b"), false, "Punjabi", "ZZ9009" },
                    { new Guid("19b95194-9ced-40c1-a2cb-77f8e84a044f"), true, "international relations", "100490" },
                    { new Guid("19e2cb94-2215-4a66-8dde-d7c2e812ccbd"), false, "Science & The Environment", "F9012" },
                    { new Guid("1a2bffd0-afac-43b6-99e2-826b4405c02d"), true, "population biology", "100850" },
                    { new Guid("1a386301-427f-4644-b38c-178c442ea10f"), false, "Modern Hebrew", "ZZ9006" },
                    { new Guid("1a4b33dc-7741-4d94-a8f0-125802434990"), true, "Persian languages", "101193" },
                    { new Guid("1a7283b8-f30f-4a3c-af42-37fad6698e4a"), false, "Religious & Moral Education", "V8008" },
                    { new Guid("1a9917a9-ca2b-4a9f-9036-58fe19d3a82e"), true, "computer games design", "101268" },
                    { new Guid("1ad52ba0-46a2-4435-8fa8-9b750c5e2f9e"), false, "Drama, Music, Movement, Lit", "W4012" },
                    { new Guid("1b14b772-1562-48c1-a65e-62d4bf8f9033"), true, "business information systems", "100361" },
                    { new Guid("1b28203f-061b-4bc1-8e77-13ce64f663b8"), true, "history", "100302" },
                    { new Guid("1bbe2c87-e3b4-444e-a4b8-375cf0d8aa2b"), true, "cell zoology", "100881" },
                    { new Guid("1bbea5b6-97ce-4d20-9ab1-75d271e8c093"), true, "architectural engineering", "100120" },
                    { new Guid("1bcd45c8-003a-424a-960e-e554f7970882"), true, "archaeological sciences", "100384" },
                    { new Guid("1bd7237d-6f15-4624-99d2-768a3a19fe79"), true, "land management for recreation", "100990" },
                    { new Guid("1be1bdfa-31a6-45cf-b79b-7be417611777"), true, "environmental and public health", "101317" },
                    { new Guid("1be8bb34-4ebd-4dea-be6e-7777bc122632"), true, "office administration", "100868" },
                    { new Guid("1c295d1e-d9b4-42ac-99b4-9c66c292acf1"), true, "materials engineering", "100203" },
                    { new Guid("1c3682e5-9249-4419-b4c5-22dda62ed042"), true, "information services", "100916" },
                    { new Guid("1c3831cf-3676-46fd-b754-ef9d02d2134c"), true, "exploration geology", "101093" },
                    { new Guid("1c6c703c-9587-443f-9395-411221cc105b"), false, "Dyeing", "J4603" },
                    { new Guid("1c81fe60-8cd8-4a11-ab9c-27968bf29a0a"), true, "Turkish literature", "101434" },
                    { new Guid("1caaeaad-b11b-47e4-99ba-0c64035a4785"), true, "Aramaic language", "101417" },
                    { new Guid("1d0d6c47-a8af-490c-a497-1d52de98e693"), true, "history of music", "100664" },
                    { new Guid("1d13074c-2bac-471d-89ac-d8036701b081"), true, "finance", "100107" },
                    { new Guid("1d158b01-1f96-4748-a7b1-1d37f7384581"), true, "biochemical engineering", "100141" },
                    { new Guid("1d169844-189b-4f79-9bef-6f226ee3be71"), true, "cognitive modelling", "100989" },
                    { new Guid("1d18a3f5-35cf-4ff1-8fd2-7c9788155052"), false, "Engineering Science", "H1001" },
                    { new Guid("1d24c528-17be-4133-83b3-a09745452c7e"), true, "musical instrument manufacture", "100560" },
                    { new Guid("1d25bcc9-252b-46d8-b9c0-6423a4114604"), false, "Modern and Community Langs", "T2005" },
                    { new Guid("1d28a7a9-eb0b-42f2-aac0-2fa19a2945b3"), true, "crime history", "101436" },
                    { new Guid("1d3de640-4bc9-4209-a4a0-810f7a4ba525"), false, "Food & Nutrition", "B4003" },
                    { new Guid("1d5bf739-15e3-41c8-b79a-d34a9b33286e"), false, "Building Construction", "K2500" },
                    { new Guid("1d6b39fc-78ad-4225-b3e8-81a7e90c1411"), true, "space science", "101102" },
                    { new Guid("1da79a08-8ff9-4b45-8cfa-b9920181de29"), true, "theology", "100340" },
                    { new Guid("1dcbec92-e99d-4c7f-95ce-03e5b9a9eabc"), false, "History and Cultural Studies", "V9004" },
                    { new Guid("1df31e53-28fe-4958-8a96-6695295aec2d"), true, "intelligent systems", "100757" },
                    { new Guid("1e1bfca4-e469-47f5-99f7-2abfc80b5204"), true, "veterinary pharmacology", "100939" },
                    { new Guid("1e454430-1007-4e1f-b63a-0d9f81eea52f"), true, "computer systems engineering", "100162" },
                    { new Guid("1e48812c-f676-424d-b221-759bfeb59df5"), false, "Social Administration", "L4000" },
                    { new Guid("1f51429e-aea1-49ef-b2a1-8ab0d32de4fd"), true, "meteorology", "100382" },
                    { new Guid("1f599880-87cd-4198-9ee7-e38c93aa7791"), false, "Integrated Science", "F9601" },
                    { new Guid("1fa54f3d-066b-46cd-9e49-cad7a87551df"), true, "modern Middle Eastern studies", "101190" },
                    { new Guid("1fb57fe9-6d2f-44e7-a5e5-ca46744800c9"), true, "international economics", "100452" },
                    { new Guid("1fd99ccf-1bc7-43b1-aa0d-dfa5a857c57f"), true, "J. M. Coetzee studies", "101486" },
                    { new Guid("1febf25d-e304-4bbf-af6d-90ae5496e118"), true, "emergency and disaster management", "100823" },
                    { new Guid("1ff6d080-a941-4bcb-bc4c-9b2066bd3354"), true, "laser physics", "101076" },
                    { new Guid("1ffe77b3-028f-46c0-a3e5-7b67b94265de"), true, "computer animation and visual effects", "100363" },
                    { new Guid("200588a9-01bc-4ce2-918a-10243041ce05"), false, "Writing", "W4007" },
                    { new Guid("207e1c41-80b2-47d1-8e71-4125f81f7ce4"), true, "printmaking", "100595" },
                    { new Guid("20974aa7-f47a-450c-8758-255012f981ee"), true, "French society and culture", "101133" },
                    { new Guid("20c2b60c-c0cc-4fc7-9f4d-cccfb2c09c6a"), true, "John Donne studies", "101468" },
                    { new Guid("210baa89-bb8a-46bb-8930-dbccee4648f6"), false, "Ancient Oriental Studies", "Q9700" },
                    { new Guid("21230c26-2e89-4e55-97fb-627c9f5df4fe"), true, "fine art", "100059" },
                    { new Guid("2135e569-c745-40b0-a2b3-e0a97a87f2f7"), true, "programming", "100956" },
                    { new Guid("2157c540-7f1f-49ee-a306-5811e2d2c1de"), true, "bioprocessing", "100135" },
                    { new Guid("217a0cf0-5de0-430b-8521-44e1b410d5cc"), true, "landscape studies", "100588" },
                    { new Guid("21cf21a0-b58a-41c2-ae13-3ff7fef9031b"), false, "English and Cummunications", "Q3009" },
                    { new Guid("21ebe2d3-a365-4aec-a9f1-1ef35695fc94"), true, "electrical power distribution", "101354" },
                    { new Guid("21ee43a3-3128-49b6-b0b0-2acc0df4c36b"), true, "affective neuroscience", "101382" },
                    { new Guid("220c233a-b047-46ed-b2d8-13b982d04796"), false, "Art, Craft & Design", "W2401" },
                    { new Guid("2215bf19-10e0-47fc-aa90-117fc2d977aa"), true, "Thai language", "101258" },
                    { new Guid("22373981-7b6d-4256-b092-d30e08689b0f"), false, "Science With Technology", "F9619" },
                    { new Guid("22431326-ab5d-4b4b-9b83-905b7ae19fad"), true, "history of mathematics", "100784" },
                    { new Guid("224bc083-b345-48ef-bb96-bf150cfeeab9"), true, "Latin language", "101420" },
                    { new Guid("227674ea-f3df-4b41-9c5f-30a599105527"), true, "publishing", "100925" },
                    { new Guid("228002ca-78e1-4669-bdae-b4fa2e9c7a6b"), true, "media production", "100443" },
                    { new Guid("2284a258-fe2b-4c3d-b808-639559f0fd5c"), true, "music therapy", "101241" },
                    { new Guid("229e90fb-51ec-44ba-8043-e55d49b65fd3"), true, "plant cell science", "100873" },
                    { new Guid("22a47345-9a09-4fca-a421-b5ad4427e2d6"), true, "British Sign Language studies", "100317" },
                    { new Guid("22ac0094-fbe3-44ef-a07e-cb55470932d6"), true, "agricultural sciences", "100516" },
                    { new Guid("22f75afc-f4f1-4304-85e5-118a8c26edad"), true, "international agriculture", "101001" },
                    { new Guid("234301a0-132a-4590-92f2-6bcec408ed09"), true, "digital circuit engineering", "100546" },
                    { new Guid("234a27eb-0b3a-451e-8184-ae61d6ed85c4"), true, "data management", "100755" },
                    { new Guid("23567693-5534-4e72-b409-e87e895e45eb"), true, "Russian and East European studies", "100331" },
                    { new Guid("239efa09-2bb7-4082-b5a6-fa88a44b4cc1"), true, "event management", "100083" },
                    { new Guid("23a85c45-da70-4694-99aa-2c1842e27afe"), false, "Social Science", "L3200" },
                    { new Guid("24337f10-3ace-4d64-9c03-dc833d5060f1"), true, "nanotechnology", "101234" },
                    { new Guid("2450d889-9e85-4ace-a600-d3fe738c7df5"), true, "Turkish studies", "101195" },
                    { new Guid("2462fb70-abb7-469f-9044-d877fe67ee88"), true, "production and manufacturing engineering", "100209" },
                    { new Guid("249f7719-a095-4f06-9cd3-f40a750e7895"), false, "Textiles & Dress", "W2207" },
                    { new Guid("24afc417-713b-4fcc-93a4-9d6d115bae9d"), false, "C D T Incorp Tech & Info Tech", "W9926" },
                    { new Guid("24b884c4-e71c-4918-9cab-a0c958b3f698"), true, "nuclear and particle physics", "101077" },
                    { new Guid("24b8dd7c-dc76-4bb4-a698-a085cb55e208"), false, "Rural Environmental Science", "F9007" },
                    { new Guid("24be80d3-5dcb-46ac-aa2a-fa128a135084"), false, "Folklife Studies", "V3201" },
                    { new Guid("24bf7977-91bc-41ee-b714-9927fb755e18"), true, "Polish studies", "101152" },
                    { new Guid("24ee14c9-c5d6-4bee-a1d7-33d2178571af"), true, "dentistry", "100268" },
                    { new Guid("2504524a-1939-42af-81d2-aa38a697b3b9"), true, "information modelling", "100751" },
                    { new Guid("253ecffe-40f6-4bdb-8b12-9db37be36d97"), true, "engineering surveying", "100548" },
                    { new Guid("25486642-ed99-49ef-af00-4f3884a3873f"), true, "mathematics", "100403" },
                    { new Guid("2570cafb-7723-4f79-92f2-3749fdea7042"), true, "Chinese literature", "101166" },
                    { new Guid("25c61157-6b92-4bbf-a4ed-a7ea668f6a4c"), true, "development in Africa", "101358" },
                    { new Guid("25c67747-998b-4298-9ad3-c9de4e2250d4"), true, "applied statistics", "101030" },
                    { new Guid("25d3aa62-c0e8-40df-a301-7a68b2800fa2"), true, "history of photography", "100714" },
                    { new Guid("263ad86f-e308-4694-889b-37fbd201c381"), false, "Office Studies", "N7601" },
                    { new Guid("26993aae-c4c2-452a-858c-4becd04ef568"), true, "qualitative psychology", "101463" },
                    { new Guid("26c6261a-8d84-462e-867c-840a3d254c90"), true, "social sciences", "100471" },
                    { new Guid("26dc3264-29c7-47fc-9cbe-bcd920851dd3"), false, "Science With Chemistry", "F9626" },
                    { new Guid("26e9d312-2b21-4088-a89d-ab88fad7a000"), true, "ethnicity", "100624" },
                    { new Guid("2734f3b1-0704-4f2d-8728-756b4e2cd211"), true, "probability", "101032" },
                    { new Guid("274a3014-f40a-4aaa-b83e-2d7206044d6f"), false, "Bengali", "ZZ9001" },
                    { new Guid("278e9486-f2a5-41b3-ae3f-2dd87116ae96"), true, "systems thinking", "100743" },
                    { new Guid("279f4e4f-a9c9-4715-9d03-92d94f077db6"), false, "Business Education (Tech)", "N9701" },
                    { new Guid("27b8aa4b-f440-4ee1-9f67-3922c874bb68"), true, "engineering geology", "101106" },
                    { new Guid("27bfb349-fd7d-449d-be97-d5be5ff136b2"), false, "Geography Studies As A Science", "F8880" },
                    { new Guid("27e29434-3179-464d-a202-c00c44ae9af3"), true, "men's studies", "100623" },
                    { new Guid("27ee2e03-b3cb-4626-8fd1-55d86b05303c"), true, "geographical information systems", "100369" },
                    { new Guid("28188a3f-0a2c-44f2-abe2-e673d76e9e90"), false, "Studies In History and Soc", "V5004" },
                    { new Guid("28310ae2-c47e-496a-a76e-8c2892f663ae"), true, "economics", "100450" },
                    { new Guid("28b0ba22-90c9-46d8-bd0d-b053053af3ac"), false, "Games and Sports", "X9020" },
                    { new Guid("28b80ac2-1abf-4ae1-8331-f943be0b5604"), true, "mentorship", "101322" },
                    { new Guid("28c5407c-1378-4de0-ba60-cdc8c54b2850"), true, "ballet", "100885" },
                    { new Guid("28c563cd-2af9-443c-a56e-aa2c0a131953"), false, "Technology Of Education", "X7000" },
                    { new Guid("28fcd605-8929-4ea9-9ddc-1fee09487e64"), true, "democracy", "100609" },
                    { new Guid("2907cf28-9074-4036-a61e-34820a95fda6"), false, "Technology and Mathematics", "G9004" },
                    { new Guid("2925db2f-c371-4829-8590-4326bc8efa44"), false, "Studies In Geog and Society", "L8004" },
                    { new Guid("29a80bbd-7c38-45d7-a0fb-e66dc6533f1d"), true, "Czech society and culture", "101501" },
                    { new Guid("29acfe2e-0518-4dd4-8292-aec30c66aab6"), true, "Russian society and culture", "101499" },
                    { new Guid("29b26c51-b494-4f61-b3d0-04a32837a5cc"), true, "podiatry", "100253" },
                    { new Guid("29babaa8-fe64-46e9-90de-6ca0e87ccc7d"), false, "Science: Environmental Science", "F9620" },
                    { new Guid("29c237c6-cde2-4036-b535-2c6a9566fb17"), true, "corrosion technology", "100545" },
                    { new Guid("29c7ee52-35b7-4283-acc9-daaf0628e596"), true, "rural estate management", "100977" },
                    { new Guid("29fbaaaf-3324-4d98-9294-156e47e89a29"), true, "crop protection", "100945" },
                    { new Guid("2a12c052-e652-4f5b-82c5-54ed9e338bc6"), true, "post compulsory education and training", "100508" },
                    { new Guid("2a42adb4-6937-4b4b-a5f7-94539c1dcb2c"), true, "welfare policy", "100649" },
                    { new Guid("2a4f0c02-0752-4eb3-a930-23a2ba23a597"), true, "Russian literature", "101157" },
                    { new Guid("2a5cd404-d58e-4dd4-b8b2-bc94d62f7c22"), true, "community ecology", "101457" },
                    { new Guid("2a62ac14-5ca9-4994-875f-cfb8cccc86e8"), true, "palliative care nursing", "100292" },
                    { new Guid("2b179d38-e596-4794-b67b-3dc3bb2669f7"), true, "community dance", "101454" },
                    { new Guid("2b26be9b-0cb9-4a90-ad98-6f03c238cdaf"), true, "dispute resolution", "101323" },
                    { new Guid("2b3281d0-600d-47e9-a839-39bae90b88c1"), true, "marine zoology", "100883" },
                    { new Guid("2b3aa4f9-c961-43d0-aa4c-e905c6a65ed4"), false, "Hindi", "ZZ9004" },
                    { new Guid("2b8070c7-2068-4934-b818-555d5ea6d1e1"), false, "Behavioural Sciences", "L7300" },
                    { new Guid("2b92b588-f93c-43c9-a8a7-b59a8d0763d5"), true, "maritime geography", "101065" },
                    { new Guid("2b9bbd90-4d71-4a01-a48a-4135fdb81434"), false, "Drama and Spoken English", "W4010" },
                    { new Guid("2b9be9ad-20ad-450d-8679-fc5927e5ec4f"), true, "theatre production", "100700" },
                    { new Guid("2bb0e5eb-feba-499a-81ef-da38a601f722"), true, "population ecology", "101458" },
                    { new Guid("2bbadac6-8ba7-45a0-9f04-531402cfc0c4"), false, "French Language & Studies", "R1101" },
                    { new Guid("2bc97e50-d388-42bf-a7c6-9141391a52f6"), true, "medical nursing", "100747" },
                    { new Guid("2bd3f2f1-d575-48ca-bc1f-76f931206344"), true, "veterinary pharmacy", "100941" },
                    { new Guid("2be7776d-3807-4411-8aa6-b2c2a12445a5"), true, "book-keeping", "100838" },
                    { new Guid("2bf3754c-289b-457f-b6e2-00360b77266d"), true, "hydrogeology", "101089" },
                    { new Guid("2bff5596-4179-432c-8957-e603ae8c05c5"), true, "music marketing", "100644" },
                    { new Guid("2c360bba-7777-4c41-a427-9b18c6018379"), true, "socialism", "101508" },
                    { new Guid("2c4a75db-5561-4c40-8412-1097db5a398f"), true, "Irish studies", "101315" },
                    { new Guid("2c531d1d-5245-463e-acfe-02f61d117000"), true, "general science", "100390" },
                    { new Guid("2c5f8ac1-b53f-4934-bc9d-3f99f7eef7cc"), true, "quantitative psychology", "101462" },
                    { new Guid("2caa82a3-839f-417a-b8c5-0585f27ef9c2"), true, "Byzantine studies", "100774" },
                    { new Guid("2cb6598f-d5e9-4ae2-9956-fa4a9b2c4144"), true, "operational research", "100404" },
                    { new Guid("2cd80a11-888a-4753-ab2b-9dfbb22c660c"), true, "scriptwriting", "100729" },
                    { new Guid("2cd9a0dd-0170-4599-93e6-2fde2b6d67a2"), false, "Sport", "X2007" },
                    { new Guid("2d4ff954-31ff-4fd8-9f34-2031db3e2257"), true, "German studies", "100324" },
                    { new Guid("2d5afbb0-e482-4096-835c-c6792e9e69aa"), true, "geotechnical engineering", "100551" },
                    { new Guid("2d79a348-30e2-45bd-aecc-5e26632a059b"), false, "Moral Education", "V7608" },
                    { new Guid("2d980b0a-5952-4721-ab83-16616d5c973e"), true, "blood sciences", "100912" },
                    { new Guid("2dbb43fa-74eb-4e15-95bc-ef8b5c2821dc"), true, "telecommunications engineering", "100159" },
                    { new Guid("2dc59122-9f7d-480a-8dd5-ded3f1159ca0"), false, "Wood Metal and Management", "W6101" },
                    { new Guid("2dd5b4ac-9916-4926-8815-e92a291b171b"), true, "creative management", "100811" },
                    { new Guid("2de6b266-34d9-494a-b791-7783ccc30af4"), false, "Ling,lit & Cult Herit-Welsh", "Q5207" },
                    { new Guid("2e15b175-5b8a-4962-90fa-7fc0f1cabcf6"), true, "modern languages", "100329" },
                    { new Guid("2e30b36e-31b7-4076-888e-1aaba600f20c"), true, "British history", "100758" },
                    { new Guid("2e345641-bc2b-486b-9445-12369e3c1614"), true, "stage design", "100708" },
                    { new Guid("2e52f992-1f2f-47c3-a7c2-a306ede8912d"), true, "Irish history", "100759" },
                    { new Guid("2e603ced-1727-4f90-97c8-02fae338438d"), false, "Science-Physics-Bath Ude", "F3012" },
                    { new Guid("2e646bcd-a058-4f3b-b365-bdfbed04c26a"), true, "applied linguistics", "100970" },
                    { new Guid("2e646f90-5d75-4e60-9488-bf92cf2bf71a"), true, "forensic science", "100388" },
                    { new Guid("2e9608ae-1f1e-4ed8-a112-1d4a7735b988"), false, "Chemistry With Science", "F9615" },
                    { new Guid("2e9a1a96-3e0a-4b81-81c7-f6e352d04093"), true, "graphic design", "100061" },
                    { new Guid("2ed9c4eb-3c98-4c43-baa4-3820b0ad1b92"), true, "technical theatre studies", "100702" },
                    { new Guid("2f3e4ce8-2f13-477c-8926-da703aee5a2f"), true, "real estate", "100218" },
                    { new Guid("2f5a7879-d9e4-4eb9-8b25-42b0fb956799"), true, "African studies", "101184" },
                    { new Guid("2f711074-58b6-4adf-9476-d20e65fb3b10"), true, "web and multimedia design", "100375" },
                    { new Guid("2fc5da28-052b-4024-8a6e-5b4437061039"), false, "Geography (Unspecified)", "L8001" },
                    { new Guid("2fca5663-efce-441e-a27b-7965ca027e83"), false, "Greek Civilisation", "V1006" },
                    { new Guid("3005b86b-422d-4754-a2ef-e9cc23726c0f"), true, "project management", "100812" },
                    { new Guid("302c3189-a93a-4e1e-bc7f-9b0132ea1e6d"), true, "animal physiology", "100937" },
                    { new Guid("30d48719-a814-4754-8af6-1a774ecde2db"), false, "Urdu", "T5002" },
                    { new Guid("32267037-19d9-4bd9-8819-6caa93531139"), true, "health psychology", "100985" },
                    { new Guid("32771ba9-0091-4d11-afc8-3f9302c41c74"), true, "German history", "100763" },
                    { new Guid("32d38308-1410-462b-a54e-8864bab2afce"), false, "Human Sciences", "L3405" },
                    { new Guid("32daed5f-4d3e-4258-a995-2f4feabd3223"), true, "radio studies", "100921" },
                    { new Guid("3339922b-63a6-4b9e-90bd-8c6dd581ec1f"), true, "systems engineering", "100188" },
                    { new Guid("3347ecf7-5cdf-44d2-b54d-f7a9e423e635"), false, "Literature and Communications", "P4601" },
                    { new Guid("3354f48b-2324-47f2-a222-21c1eba171f8"), false, "Personnel and Social Education", "L8203" },
                    { new Guid("336c0a90-dcc2-4812-aabb-b4ba0fb08d0d"), false, "Literature & Communic Studies", "P4603" },
                    { new Guid("336c1b58-f68a-465b-a4bf-fcdf28e56f91"), false, "Swedish", "R7200" },
                    { new Guid("3377f679-bf3d-43a1-a87c-e7a894410869"), true, "molecular biology", "100354" },
                    { new Guid("33cd2078-bad8-4bbf-8e11-732f3f557f8e"), true, "Russian languages", "100330" },
                    { new Guid("33edfadf-4ca1-47fa-a4bc-d61a8ebd6caa"), false, "Foreign & Community Languages", "Q1301" },
                    { new Guid("33fcf7a4-a2a3-47c4-aad2-8c4a8f5a4c15"), true, "molecular genetics", "100900" },
                    { new Guid("3413aa59-0a14-4aea-9464-51af33fadcba"), true, "study skills", "101090" },
                    { new Guid("3453f5cc-a4c9-4888-8cd5-26fcf5e9b729"), true, "Egyptology", "100787" },
                    { new Guid("3462b0b8-edf2-4902-96dc-315ea21356e0"), true, "Japanese languages", "101169" },
                    { new Guid("346759df-c8d8-43e4-a1e1-4c1314743ff8"), false, "Czech", "T1400" },
                    { new Guid("34c02db5-bd41-44eb-a17e-75fca7c678aa"), false, "Recreational Management", "X2001" },
                    { new Guid("34e627ec-864b-43cc-a955-e38e3c84db2d"), false, "Geography (Not As Physical Science)", "L8000" },
                    { new Guid("34eacf01-b9c1-49c4-ad05-c9d49e21edb6"), false, "Visual Studies", "W1504" },
                    { new Guid("34f89dce-cfd0-4e70-bbff-beec08b16549"), false, "Music In Education", "W9919" },
                    { new Guid("34fa8d5b-d5ba-4f4e-963b-f10e09446509"), false, "Political Science", "M1005" },
                    { new Guid("35015519-1f04-4102-b909-7e61de13e4b3"), true, "publicity studies", "100919" },
                    { new Guid("3542a8ce-5347-41e7-9173-319615d198b6"), true, "information systems", "100371" },
                    { new Guid("35ddee6c-ef6c-414d-b482-e8781360e086"), false, "English and Drama", "Q3011" },
                    { new Guid("35fe2cfa-df05-4da3-ab16-aa60c437dfe1"), true, "psychology of memory and learning", "101342" },
                    { new Guid("36682f4e-def2-4193-b69f-9e79ad830b92"), true, "Turkish society and culture studies", "101504" },
                    { new Guid("369b957b-d9a7-46f4-8d65-402612c5904c"), true, "environmentalism", "101510" },
                    { new Guid("36b2d76b-3818-4add-b19b-1ee5a780d79a"), true, "Stone Age", "101437" },
                    { new Guid("36cade99-c6e0-48ba-bf3c-ebe32940c1b9"), true, "petroleum geology", "101105" },
                    { new Guid("37a477a0-a241-49f6-9a4f-c814c2cbfe53"), false, "Visual Art", "W1500" },
                    { new Guid("37da781a-3e55-4a57-b022-e45b18a1fffd"), false, "Russian With German", "R8206" },
                    { new Guid("382164d8-d363-475f-b664-f2667988ce06"), false, "Personal,social and Careers Ed", "X9010" },
                    { new Guid("385427b7-ac55-490a-a79c-59a0e49cc296"), false, "Handicraft Teachers Diploma", "10584" },
                    { new Guid("386aadb1-aaa1-4032-8358-a1eb15271dcb"), false, "Home Economics (Design & Tech)", "W9912" },
                    { new Guid("388f81a8-47e4-40f1-9cc6-064979e119ff"), true, "translation studies", "101130" },
                    { new Guid("38cd7540-feff-4444-a796-c23299c976c7"), true, "fashion", "100054" },
                    { new Guid("38dab944-d3cf-4bfa-b9e4-9cba848bb05b"), true, "optometry", "100036" },
                    { new Guid("38e8fe1a-2432-4769-b16e-0c6220f35439"), true, "primary teaching", "100511" },
                    { new Guid("38ea9ff7-58f6-4628-92a4-90da306a8e6b"), true, "phonetics and phonology", "100971" },
                    { new Guid("3917bcb4-5821-4158-a7be-7e652535c9d3"), true, "music production", "100223" },
                    { new Guid("391a7b71-6d81-46d7-b84f-a1e6e4b1b59f"), true, "satellite engineering", "100118" },
                    { new Guid("391adbf7-6c0e-4812-b74f-5b4e0ea14554"), true, "research methods in psychology", "100959" },
                    { new Guid("393a9694-85de-4e95-9480-4d5f50a2069a"), true, "environmental geoscience", "100380" },
                    { new Guid("39631f6c-67d4-4cb4-b020-be6eafd75d17"), true, "human genetics", "100898" },
                    { new Guid("39b9a251-fdb6-47de-94f8-eccf786fafc6"), true, "public policy", "100647" },
                    { new Guid("39c345fe-34a2-4d20-b0e0-eafdb8271909"), true, "safety engineering", "100185" },
                    { new Guid("3a21e625-4ad8-4507-b4d2-fff1c8813232"), true, "children's nursing", "100280" },
                    { new Guid("3a3d37bd-826b-41d3-a40b-41f86449227c"), true, "environmental biotechnology", "100136" },
                    { new Guid("3a45a810-a5af-447f-b3d2-2c424ddef3f7"), false, "Metalwork", "W6100" },
                    { new Guid("3a4f748c-e038-4d47-994c-d375200f2c46"), true, "biblical studies", "100801" },
                    { new Guid("3a574eb1-219d-47e3-8a54-5bf4e3f1b60b"), true, "orthoptics", "100037" },
                    { new Guid("3a638c16-b253-4f12-9dfe-905af7cc3221"), true, "ultrasound", "101330" },
                    { new Guid("3aa251bc-cb76-4f3e-8549-5d6141fd2b15"), true, "environmental risk", "101048" },
                    { new Guid("3adaffc3-1ef7-4309-9c46-1b7d9c1e52ff"), true, "biomedical engineering", "100127" },
                    { new Guid("3adb78ae-7d6b-4b27-80e7-bb5700a0d65d"), true, "transcriptomics", "101377" },
                    { new Guid("3af3868f-4737-49d4-8f7b-964a4e791c16"), false, "General Art and Design", "W8890" },
                    { new Guid("3b25b54f-f2f5-495b-975e-a3ab76b84004"), true, "Scandinavian literature", "101425" },
                    { new Guid("3b34ef4b-48de-4598-b189-c96e49c28c3c"), false, "Environ Science & Outdoor Stud", "F9628" },
                    { new Guid("3b41e62a-6fe2-49cd-9d29-c6871fa239f6"), true, "sociology of science and technology", "100631" },
                    { new Guid("3b8f636e-56e5-4319-be11-0df6847c84d0"), false, "Science In The Enviroment", "F9022" },
                    { new Guid("3bda657a-4133-49f2-b4aa-866bc1c232df"), true, "motorcycle engineering", "100205" },
                    { new Guid("3be9f0cd-eef3-44fd-b242-0a1afd3e014d"), false, "General Biological Sciences", "C8890" },
                    { new Guid("3c07a450-006c-4979-8ea7-92644a20a363"), true, "South Asian studies", "101172" },
                    { new Guid("3c22b1c3-7895-4d00-ad61-ceb7afebe075"), false, "Physics and Science", "F9632" },
                    { new Guid("3c414f92-18e1-4a5e-b83b-c95887e4269d"), false, "Domestic Science", "N7501" },
                    { new Guid("3c67fd37-c855-4f18-8f31-8325f9b8e132"), false, "Jewish Studies", "V1409" },
                    { new Guid("3c7ad07f-2035-4471-b839-4152d1667efb"), true, "computational mathematics", "101029" },
                    { new Guid("3c8f4f25-7219-4092-97ef-0a069944eca8"), true, "Iberian studies", "100765" },
                    { new Guid("3cb2d0b2-7c70-47b8-a651-09dc8cb2de38"), true, "hair services", "101374" },
                    { new Guid("3cd9b0c1-3bc0-4661-ae9a-28f0052fac05"), true, "cultural studies", "101233" },
                    { new Guid("3cec73b4-4a12-4476-8b02-e5bb6da83d9d"), true, "alternative medicines and therapies", "100234" },
                    { new Guid("3d025e1e-1322-4598-97a0-7bbf58f7e49c"), true, "sports coaching", "100095" },
                    { new Guid("3d09c78c-5a80-4eff-9bf6-ce99f49da31e"), true, "agricultural technology", "101006" },
                    { new Guid("3d10c47d-0d40-4ecd-8613-2e25073a42be"), false, "Afrikaans", "T7007" },
                    { new Guid("3d661752-70e8-4798-a671-674c8c25ad8c"), true, "geology", "100395" },
                    { new Guid("3d85e3e9-45b7-4115-8e34-28bc9f49e669"), false, "Modern Greek", "T2400" },
                    { new Guid("3dd1508e-4cfe-4d07-b2c9-2a05abfe43d2"), true, "transport engineering", "100154" },
                    { new Guid("3e41820f-d187-4bc1-8b8e-3f39c9cc871f"), true, "Russian studies", "101151" },
                    { new Guid("3e605bdf-3fd6-4a0b-aadb-9babcd1b2815"), false, "Studies In Humanities", "V9005" },
                    { new Guid("3ed0000f-80f8-4463-840c-50079ca5a3c0"), true, "volcanology", "101081" },
                    { new Guid("3ee501aa-8691-4f43-afc0-873215dfdb25"), true, "biomolecular science", "100948" },
                    { new Guid("3ee588ff-9e30-4021-ae11-b38ac59481e7"), false, "Recreational Studies", "X2006" },
                    { new Guid("3eee0122-51d4-4b42-83be-5eefdb8206e3"), false, "City and Guilds Farm Machinery", "10218" },
                    { new Guid("3efbe302-6f37-4bf2-a531-1ac9fca9b6a8"), false, "Handicraft: Overseas Quals. In Handicraf", "289" },
                    { new Guid("3f0f94f2-66ae-44f2-abfb-4f47f624da60"), true, "health studies", "100473" },
                    { new Guid("3f14daf8-a77c-476e-908b-c76015904356"), true, "planning", "100197" },
                    { new Guid("3f1d3ac4-f2d9-458d-9781-f48264fac94c"), false, "Music and Drama", "W3300" },
                    { new Guid("3f5b01de-4f79-40bb-bcb7-4329439bc87d"), true, "environmental engineering", "100180" },
                    { new Guid("3fcd12f3-07b3-4a93-b816-6fa84725ce82"), true, "agricultural geography", "101407" },
                    { new Guid("4014b436-25ed-46d4-9acd-58392e6483d3"), true, "manufacturing engineering", "100202" },
                    { new Guid("403432e9-9cf3-44a1-b0d3-0b8e9ed11324"), false, "Combustion Science", "H8601" },
                    { new Guid("4043db20-6edf-46ac-bc5e-6ff66f832ea4"), false, "Expressive Arts (Drama)", "W4004" },
                    { new Guid("404e89a5-86c5-4094-929f-d71a200696a0"), true, "negotiated studies", "101275" },
                    { new Guid("405dc7fc-e885-4395-b528-134fce190ee4"), true, "property development", "100586" },
                    { new Guid("40e061ea-ac5e-4d4c-be7f-7899809163e5"), false, "Rural Science", "F9008" },
                    { new Guid("4126e118-da61-4823-915d-6ea71075c0f7"), false, "Russian Language & Studies", "R8101" },
                    { new Guid("4141853b-ba3c-4103-b66f-791e53a3478c"), false, "Classical Languages", "Q8101" },
                    { new Guid("414f44ef-d513-4bdc-9a09-7114854c7b57"), true, "social psychology", "100498" },
                    { new Guid("41506fe7-2976-42de-a3c3-0a152036c2c8"), true, "aquatic biology", "100848" },
                    { new Guid("41acb1d6-0c23-4c4a-a5bc-a9f5a90070fd"), true, "diagnostic imaging", "100129" },
                    { new Guid("41bc9dc1-ab51-48d3-97f5-1af495b6e739"), true, "moving image techniques", "100887" },
                    { new Guid("41bd0fb3-1463-4142-ba02-c09b1b79c989"), true, "dance and culture", "101453" },
                    { new Guid("41d32719-868c-42d2-95df-a243210f9a0a"), true, "medical microbiology", "100907" },
                    { new Guid("41fe4f2e-ef27-4feb-b8ea-8edd9b5fd109"), true, "Latin American studies", "101199" },
                    { new Guid("4228b95a-5d26-48b6-9885-c8f13638acbe"), true, "aerodynamics", "100428" },
                    { new Guid("42435b6e-bc8c-4e40-b570-bb8c3d23a9a7"), true, "operating systems", "100735" },
                    { new Guid("425cbd91-96fe-4ad0-9a38-b0642b6714a8"), false, "Communication Studies", "P8830" },
                    { new Guid("428611ab-c8ca-47ac-ba2b-0c520d49225d"), true, "law", "100485" },
                    { new Guid("42a0a43b-202d-43e7-a4c2-e1188668c5ca"), true, "earth sciences", "100394" },
                    { new Guid("42dae18e-f586-46b4-8ed0-881860446113"), true, "cardiology", "100748" },
                    { new Guid("42e392da-9099-4f04-a53e-3a7c485f328c"), true, "computer vision", "100968" },
                    { new Guid("431dd705-e42c-4d7c-8d7d-eee22d445e9f"), true, "Latin American literature", "101201" },
                    { new Guid("432dd487-fb3a-4cc8-ab58-ea3798215396"), true, "Sanskrit studies", "101115" },
                    { new Guid("433b27c1-6427-4bdd-81f8-a29195820f3b"), true, "acting", "100067" },
                    { new Guid("4356579d-443b-4c92-84f8-f482d7dde009"), true, "European Union law", "100680" },
                    { new Guid("435e6461-eead-403b-a7ca-a4b044efe05e"), true, "product design", "100050" },
                    { new Guid("438ca274-2324-478a-8af0-f190c118f445"), true, "avionics", "100117" },
                    { new Guid("43b4797b-cc46-48a9-bd88-c1a6915560fa"), true, "online publishing", "100927" },
                    { new Guid("43fc6a84-a915-4b95-84fe-968b01d6887a"), true, "early years education", "100463" },
                    { new Guid("440dcd61-0e38-4e47-947d-4ffc31e894ad"), true, "Spanish society and culture", "101138" },
                    { new Guid("44363260-f784-4960-ad48-b77ed364c90f"), false, "Engineering (Tech: Science)", "H8701" },
                    { new Guid("444c8dca-dd6a-4576-a563-6df66d831391"), true, "psychopharmacology", "101464" },
                    { new Guid("44692e69-7ed2-44b9-b647-50b1863f824f"), false, "Outdoor Activities", "X2018" },
                    { new Guid("446e32dd-82a8-4913-9b01-9acce8bf79f6"), false, "Language and Communications", "Q1409" },
                    { new Guid("44ad6021-e2b0-457f-8e96-53bcc09514ab"), true, "marketing", "100075" },
                    { new Guid("44b618a8-cf12-4283-9ced-6449b530f44b"), true, "photography", "100063" },
                    { new Guid("44d8f33b-fd00-4837-a910-b70bf283d7db"), false, "Operational Rsearch Techniques", "G4500" },
                    { new Guid("453e5c45-f45c-427d-8288-9eb383885706"), true, "staff development", "100861" },
                    { new Guid("45553b52-4e17-4c22-bd41-898292a13d6f"), true, "fascism", "101509" },
                    { new Guid("45738b76-2673-44da-81f5-d083cfc62d2a"), true, "meat science", "101387" },
                    { new Guid("45a38648-6141-417a-baa0-83eee909ec92"), true, "clinical practice nursing", "100746" },
                    { new Guid("4663e668-63d8-4f19-92b1-05d8d8be11a8"), true, "metal work", "100721" },
                    { new Guid("46d3ea6a-fe49-48b7-8872-9d6fb138375a"), true, "clinical physiology", "100258" },
                    { new Guid("475abd69-35ca-48b6-b760-1f33ed40c5c6"), true, "French language", "100321" },
                    { new Guid("476dc800-47ca-4193-83f8-d303a143945f"), true, "Scottish history", "100311" },
                    { new Guid("476e1968-f1cf-48fe-8750-d243185de639"), true, "economic history", "100301" },
                    { new Guid("478a448e-c3a4-41ea-84ab-e37db1132b55"), true, "Canadian studies", "101205" },
                    { new Guid("47c69434-0b15-4ce9-81dc-958a18eae221"), true, "Ukrainian language", "101429" },
                    { new Guid("47d53963-a316-459b-be64-45fda5a11a67"), true, "game keeping management", "100979" },
                    { new Guid("47f9bb11-9b64-424e-8684-54f1f762dee2"), true, "body awareness", "101452" },
                    { new Guid("48285645-14b8-44bb-83f2-ff5aa017d4b5"), true, "multimedia computing science", "100737" },
                    { new Guid("4853d8e8-7dcf-46ae-9faa-ebf05254e9f7"), false, "Design & Tech-Food & Textiles", "W9908" },
                    { new Guid("485c837a-1cf7-4aa4-9f98-71cd1982a07c"), true, "philosophy of science", "100338" },
                    { new Guid("48a093d9-c4b9-4816-bd00-50624b5b680b"), false, "General Technologies", "J8890" },
                    { new Guid("48bdcabb-54c9-42e9-87c8-d3a2b0e05cfc"), true, "work placement experience (personal learning)", "101276" },
                    { new Guid("48f58d75-2aa2-4f3d-8a31-9ca06c443a32"), true, "microeconomics", "101401" },
                    { new Guid("48f697fe-dea1-4327-a218-824316b553ee"), false, "Bilingual Education", "Q1411" },
                    { new Guid("491b0a6f-af41-4971-b576-03e3c78e03bb"), true, "medical genetics", "100899" },
                    { new Guid("493bb547-cb22-42e4-839b-e37bac538ec6"), false, "Design (General)", "W2004" },
                    { new Guid("49a92360-d5c1-4aa2-a874-29b02a5f45d7"), false, "Other", "Z000" },
                    { new Guid("49aa7c26-5773-413c-b671-bfb6c638cf75"), true, "Portuguese studies", "101141" },
                    { new Guid("49baaed7-fdba-4cc8-aa6a-5d332c0bb9db"), false, "Speech Therapy", "B9503" },
                    { new Guid("49bc87b2-3e2d-47a2-af40-ab998ab7968e"), true, "East Asian studies", "101271" },
                    { new Guid("4a35a92c-64cd-48f5-aa96-49afafd4d826"), true, "social anthropology", "100437" },
                    { new Guid("4a5a0451-3233-43b8-be80-62ce9cf31317"), false, "Other Sciences", "ZZ9007" },
                    { new Guid("4a88f169-3c72-4f44-a9fc-3f52257e2649"), true, "business law", "100482" },
                    { new Guid("4a9b0a77-b3f3-4628-833e-73f60205b55e"), true, "natural sciences", "100391" },
                    { new Guid("4b444a5b-ba54-4bae-927b-e04d681115f9"), true, "paediatrics", "101325" },
                    { new Guid("4b574f13-25c8-4d72-9bcb-1b36dca347e3"), true, "food and beverage studies", "101017" },
                    { new Guid("4b954b11-8c87-4047-9ef3-f7c0c2ebde1f"), true, "electromagnetism", "101391" },
                    { new Guid("4bd2f549-1abe-48bc-a445-07637d6934fd"), true, "palaeontology", "100398" },
                    { new Guid("4be3fef8-8089-4bfa-987d-459551b48430"), true, "acoustics", "100427" },
                    { new Guid("4c7c502f-5ca6-4825-91e3-7df3f5a59faf"), true, "art psychotherapy", "101320" },
                    { new Guid("4c7fd913-9bd3-4f59-80ec-c3dba706e10f"), true, "adult education", "100454" },
                    { new Guid("4c91326d-0935-441c-acb1-93e53b02342b"), true, "theoretical physics", "100426" },
                    { new Guid("4d066777-81b5-4d37-a41b-588c56884cd8"), false, "English Linguistic Studies", "Q1002" },
                    { new Guid("4d0fda31-4099-404a-9de2-e352c65be4b4"), true, "sales management", "100851" },
                    { new Guid("4d787ede-5554-4332-afa0-ead82e8596ad"), true, "Gaelic language", "101120" },
                    { new Guid("4d8a4f2b-dae3-4108-ba6c-c7d9b2f8c818"), true, "exercise for health", "101319" },
                    { new Guid("4dbb8509-87d1-42da-8af8-f6538d3b6656"), false, "Natural Philosophy", "F3001" },
                    { new Guid("4e5eef54-e052-453d-8be3-7ae3df6440ca"), false, "Spanish Studies (In Translation)", "R4100" },
                    { new Guid("4e61c720-032c-446b-837e-942ba7fc52d0"), true, "international marketing", "100853" },
                    { new Guid("4e6731e8-11f4-40b5-986d-882d4f735828"), true, "economic geography", "100665" },
                    { new Guid("4e7756cf-1532-4386-af10-16dc79f3afc5"), true, "pollution control", "101072" },
                    { new Guid("4ea17408-32d5-4419-beaf-289c047fbc62"), false, "Modern English Studies", "Q3100" },
                    { new Guid("4f20b6db-07c7-424d-8b89-c63660a47e9c"), true, "Welsh history", "100760" },
                    { new Guid("4f4bb434-3bde-43da-b9cb-0f609baae4c8"), false, "Spanish With French", "Q9709" },
                    { new Guid("4f5c56cc-76ce-455a-b03b-fc0cc75bf7b8"), false, "Geography and The Environment", "L8202" },
                    { new Guid("4f78c615-382c-4253-889d-2d16db76ab73"), true, "neonatal nursing", "100289" },
                    { new Guid("4f84d5f2-6045-41a3-8742-a9bc970d037b"), true, "plant sciences", "100355" },
                    { new Guid("4fa48204-19b4-4ba2-b6ff-f1b8c3c1d0eb"), true, "toxicology", "100277" },
                    { new Guid("4fb480a9-4a15-48d2-bf4e-476455a78fb8"), true, "directing for theatre", "100697" },
                    { new Guid("4ff3116a-2507-4b5e-89a1-ca2fac9e050a"), true, "child care", "100654" },
                    { new Guid("5063eae9-1d19-4fdb-b79d-08bf1fb1967e"), true, "computer games graphics", "101019" },
                    { new Guid("50b6d73a-25b4-41fe-a5c2-7784f21343ab"), true, "film music and screen music", "100842" },
                    { new Guid("50c7b73d-793d-49c9-b0b9-dfc20072e558"), true, "financial risk", "100835" },
                    { new Guid("51419d04-4602-469c-8f2d-2acdb86d34e0"), true, "astronomy", "100414" },
                    { new Guid("519df0ff-b4be-4e82-b135-9be576756343"), true, "investment", "100828" },
                    { new Guid("51b1a620-1203-4b16-b645-3914aba8ca87"), true, "physician associate studies", "100750" },
                    { new Guid("51baac39-e6f1-4a78-b4f2-e1a27a59238a"), false, "Youth & Community Studies", "L5206" },
                    { new Guid("51d91e5a-c91d-4dd6-ad83-39d7d5b3c39c"), true, "applied mathematics", "100400" },
                    { new Guid("51f4fae2-ffd5-4856-99b1-8c147b1783fe"), true, "Hindi language", "101174" },
                    { new Guid("52286b44-0fe6-495a-aca0-6a45e992825a"), true, "Robert Louis Stevenson studies", "101489" },
                    { new Guid("523f9f94-d612-464f-9d67-4f1a0ee69657"), true, "mechanics", "100430" },
                    { new Guid("52798172-6ead-46f8-bdfd-643d75f1e9b5"), true, "politics", "100491" },
                    { new Guid("5327ea16-d703-4436-bae6-95ffc440ee53"), true, "theology and religious studies", "100794" },
                    { new Guid("53557635-d831-4a7e-be85-2ca829131153"), true, "furniture design and making", "100633" },
                    { new Guid("5366fe16-a2f6-412b-965a-b89facfec4df"), false, "Educational Computing", "X9011" },
                    { new Guid("5394f825-dba3-44dc-8fc7-0df0e56685db"), true, "English literature 1700 - 1900", "101095" },
                    { new Guid("53964d69-6263-47c3-979a-eb1bc87f7fbd"), false, "Creative Arts (Music)", "W3002" },
                    { new Guid("54449a21-4aca-4ac9-bd4c-26eabfd69f3b"), true, "combined studies", "101273" },
                    { new Guid("54547302-e37d-4417-a4a0-310263373eeb"), true, "food science", "100527" },
                    { new Guid("547bbce7-8493-4870-a8b7-9450ef068fa4"), false, "Outdoor and Science Education", "X9012" },
                    { new Guid("54bb102b-e6bd-4394-ad80-54d58bc7f8c7"), false, "Science In The Human Environment", "F9013" },
                    { new Guid("5502f1f7-4698-4d3b-a906-755224c9a224"), true, "mining engineering", "100204" },
                    { new Guid("5522426b-1a63-4004-8050-afa10c46d977"), false, "Economics With Business Studs", "L1005" },
                    { new Guid("552796db-2b49-4c50-a378-8687d6f58dc0"), true, "forensic anthropology", "101218" },
                    { new Guid("55302d2d-8d73-43b7-ac84-119e8b2e1616"), true, "management and organisation of education", "100817" },
                    { new Guid("553308ae-51ff-4cb8-ab88-d67a2d88253b"), false, "Teach Eng -Speakers Other Lang", "Q9706" },
                    { new Guid("556f4eb6-5cbc-487e-9af2-cd15436ef417"), true, "urban and regional planning", "100199" },
                    { new Guid("55b2e32c-9c1d-4f31-aa07-256ab91924a2"), true, "petroleum engineering", "100178" },
                    { new Guid("55b34741-a989-4114-b47a-b3c03f9436b6"), false, "Expressive Arts(Visual Arts)", "W1505" },
                    { new Guid("55b708b0-a85d-4f3c-8f7c-5db56d387feb"), false, "Physics With Core Science", "F9613" },
                    { new Guid("55ce4249-81fc-4645-80cd-9452bcd5258e"), false, "Teacher Training", "X8810" },
                    { new Guid("55dd7f60-64f9-4964-aed3-ba4eda386895"), false, "Gen Asian Lang, Lit & Cult", "T8850" },
                    { new Guid("56587ef8-0534-44eb-84b2-1804a48bd063"), true, "African society and culture studies", "101189" },
                    { new Guid("565fd87c-c69a-4c60-b54f-d55626cd44e4"), true, "international history", "100778" },
                    { new Guid("568f1cbf-7a3d-4413-ac01-d6cd9504cd29"), false, "Music Studies", "W9920" },
                    { new Guid("56b3abea-cfec-4717-84cc-3027d731fd4a"), true, "plant physiology", "101460" },
                    { new Guid("56b8afc3-545d-4089-b4b6-aa12106ebec0"), true, "paramedic science", "100749" },
                    { new Guid("56c84480-02c3-4f84-9cc7-0c99b64b167a"), true, "corporate image", "100856" },
                    { new Guid("56c866d8-38ee-4c2a-81bb-784351350695"), false, "History Of Education", "X9007" },
                    { new Guid("56da3835-805c-4fdb-b326-3e6691c37f2a"), true, "Latin American history", "100769" },
                    { new Guid("56e31208-a3cb-4bb8-ab68-2ffa20cf3683"), true, "architecture", "100122" },
                    { new Guid("57170354-f7a4-4bef-bcce-e75704ea6879"), true, "museum studies", "100918" },
                    { new Guid("5721d3c3-1806-440f-ae30-69168c4a6b7d"), false, "Maths and Info. Technology", "G9006" },
                    { new Guid("573c51f4-b452-46a8-9ef6-a30880bb7cdc"), false, "Expressive Arts (Art)", "W1005" },
                    { new Guid("574eab5a-a458-41b9-b342-25b92f86ca3c"), false, "Russian Lang, Lit & Cult", "R8880" },
                    { new Guid("576400f1-4d1a-49e2-ab25-b3d556fcda2f"), false, "Environmental & Social Studies", "L8201" },
                    { new Guid("5766a04a-416c-45ed-b595-4870617b8835"), false, "Numeracy", "G9002" },
                    { new Guid("579e416c-93b0-4644-a27e-a758e6bdc8f5"), true, "veterinary public health", "100942" },
                    { new Guid("57cfe593-fed9-4fc4-8baa-d3b4854fbd0c"), true, "forensic biology", "100386" },
                    { new Guid("583434ba-6e54-4a9a-9e6e-319cbc2462b5"), true, "feminism", "101403" },
                    { new Guid("586f80c0-a796-46dc-bd8a-2f258d80a5e2"), false, "Business Education", "N1211" },
                    { new Guid("58830e60-73ec-463d-8fc5-dd50006880cf"), true, "remote sensing", "101056" },
                    { new Guid("58917db9-66a6-4bf0-ab9b-e6f84a697a03"), true, "accountancy", "100104" },
                    { new Guid("58bc51a7-9817-4d4d-8413-162117083123"), true, "Swedish language", "101148" },
                    { new Guid("58bef565-e0f5-4e6f-963e-7fc9ef70e4fa"), true, "international hospitality management", "100087" },
                    { new Guid("58d5d357-04d8-4a43-a4b1-c92e818e0fa1"), true, "philosophy", "100337" },
                    { new Guid("58ebe09f-cb1a-422b-87e4-7af97ca1cebb"), false, "Computer Education With Maths", "G5007" },
                    { new Guid("5925db8d-306c-43a5-be80-6fe72216b504"), true, "German literature", "101134" },
                    { new Guid("598ea4ef-f7fa-4d5c-aa7c-5d6b1a3e87f0"), false, "Organisation & Methods", "N2001" },
                    { new Guid("59917cbc-5ebe-4499-bd24-33b6116efdc3"), true, "artificial intelligence", "100359" },
                    { new Guid("59a25822-d064-4bb8-be7a-0a4bb7831596"), false, "Design & Tech (Cdt/Home Econ)", "W9904" },
                    { new Guid("59d77f50-752f-4f1f-8828-c02c7f1e5681"), true, "econometrics", "100604" },
                    { new Guid("59fa7ec1-29e4-4e5f-85ef-cfa01cdf30ee"), true, "Portuguese society and culture", "101144" },
                    { new Guid("5a728384-8cf9-4872-88f1-3505b5dd6c8b"), true, "classical Greek literature", "101423" },
                    { new Guid("5a877c5b-3809-4607-a214-9db06cffad7e"), true, "sport technology", "101379" },
                    { new Guid("5aa404eb-bb50-4157-8552-c82286519c0e"), true, "calligraphy", "101362" },
                    { new Guid("5adde787-4c4e-433e-940f-3361e5dfdd32"), false, "Science Education", "F9023" },
                    { new Guid("5b0cca8e-dd95-44d6-a155-278a6c73d69b"), false, "Rural & Env Sc (With Integ Sc)", "F9622" },
                    { new Guid("5b398a15-d5cc-42c5-9ce3-206683576887"), true, "applied science", "100392" },
                    { new Guid("5b3e925e-ff48-481e-958c-ca423e2b26af"), true, "food marketing", "101215" },
                    { new Guid("5b8976d3-916f-4d7e-8f75-3f74c9a16d77"), true, "pre-clinical dentistry", "100275" },
                    { new Guid("5bb25bcb-3502-47e8-a0ff-bb1b82222d33"), true, "liberalism", "101506" },
                    { new Guid("5bc8aa6c-ef41-4b52-be3d-468d7e9e4615"), true, "facilities management", "101308" },
                    { new Guid("5bcec838-7f13-4afd-a54d-fd1c04030e54"), true, "family history", "100779" },
                    { new Guid("5bfc7bbf-9b23-47ac-93c2-11dc21761d1f"), true, "Dylan Thomas studies", "101493" },
                    { new Guid("5c11c9bc-111e-496e-bfde-375c435caa2e"), true, "history of medicine", "100785" },
                    { new Guid("5c3764a8-8dd1-48d2-8054-e3adcaf9f329"), true, "animal nutrition", "100940" },
                    { new Guid("5c43ee62-a862-4696-95e5-e29f9b31974b"), true, "veterinary nursing", "100532" },
                    { new Guid("5c63d2e6-06a9-4d4b-964b-16bb9ff09320"), false, "Handicraft: City and Guilds Of London In", "211" },
                    { new Guid("5c6671a5-7035-4848-a127-9efc1b0b0fb5"), false, "General Studies In Arts", "Y3000" },
                    { new Guid("5c72de01-669e-46b1-9c42-d3807cc9cf6e"), true, "film directing", "100888" },
                    { new Guid("5c933a7e-45bc-41eb-8750-75974b52d19c"), true, "neuroscience", "100272" },
                    { new Guid("5ce88fd5-c56e-484e-8fd7-4a9db9fe04a8"), true, "clinical engineering", "100005" },
                    { new Guid("5cfb55a2-8bc1-447f-94db-1cfcb6ef7de6"), true, "security policy", "100652" },
                    { new Guid("5d093a20-86e5-4bd4-9af8-4b7d99b2b38a"), true, "sports therapy", "100475" },
                    { new Guid("5d165020-b3d5-49ed-96c7-823d670ac327"), false, "Physics (With Science)", "F9623" },
                    { new Guid("5d3344e7-2d76-420d-9a51-6fd9f099b0c7"), true, "clinical dentistry", "100266" },
                    { new Guid("5d5202b9-5551-4a13-b9d5-e7365af72872"), false, "Post-Graduate Certificate In Education()", "10886" },
                    { new Guid("5d8c415f-889d-4c7e-89bb-8712d8e6f451"), true, "Judaism", "100797" },
                    { new Guid("5db1e31c-0b2d-4cec-8948-d807587c6ffe"), true, "hospitality", "100891" },
                    { new Guid("5def97b7-415f-4474-9dd8-6b6f6fbbae99"), true, "English language", "100318" },
                    { new Guid("5defee3b-369c-4d11-850f-e1b221c854ae"), true, "Scandinavian studies", "101145" },
                    { new Guid("5e5bf2a6-7b34-472a-a976-5d487723ca6a"), true, "public administration", "100090" },
                    { new Guid("5e780b07-bc44-40a7-b568-363c60a8cd1c"), true, "older people nursing", "100291" },
                    { new Guid("5e8e1bc5-76fe-49f8-998d-87c7703b4306"), true, "social philosophy", "100792" },
                    { new Guid("5e914ab2-6ecc-4ccc-b4ab-74fab27cb4dd"), false, "Chemical Sciences", "F1001" },
                    { new Guid("5ef92d4f-ee77-4b21-a7eb-4ca8e11fbdd6"), false, "Ed For Those With Sn", "X8860" },
                    { new Guid("5efc50ee-821f-4f93-836e-15e2e00e9615"), false, "Tech: Business Studies", "W2504" },
                    { new Guid("5f2be574-e12e-41b9-8e2b-912b3e791d73"), false, "Liberal Studies", "V9001" },
                    { new Guid("5f3fa42f-8e4f-490b-831b-5a4b377dfe99"), true, "Indian society and culture studies", "101179" },
                    { new Guid("600f089b-d010-4f0f-9b9a-e0454877cee8"), true, "Caribbean studies", "101207" },
                    { new Guid("6036321d-b9fe-4d08-bea6-cea5a74b366a"), false, "Creative Arts (Art & Design)", "W2002" },
                    { new Guid("604f7027-5e40-4b71-870e-0c0416111659"), false, "Performance Arts", "W4005" },
                    { new Guid("60634adb-265b-472d-a933-0aabf76e09ec"), true, "dietetics", "100744" },
                    { new Guid("60649a80-50cd-41c2-9331-cbd6e898363b"), true, "advice and guidance (personal learning)", "101279" },
                    { new Guid("60943501-351b-445b-9e9a-0b41908e10e1"), false, "Science With Mathematics", "G1501" },
                    { new Guid("60fabe1f-e034-43a6-b78b-04b88ed80ef2"), false, "City and Guilds Engineering Planning, Es", "10216" },
                    { new Guid("61c7104e-62db-4afd-80ee-82b3beb80e51"), false, "Ancient Greek Lang & Lit", "Q8870" },
                    { new Guid("61de39af-ea4c-440f-b067-d141803da8ee"), false, "Teaching Diploma In Speech and Drama", "612" },
                    { new Guid("6227e2db-a8aa-4ef2-9dc4-50e8613abd9f"), true, "probation/after-care", "100662" },
                    { new Guid("623d6139-f6b8-4195-ba23-77532df794e3"), true, "dance", "100068" },
                    { new Guid("6252ed64-2767-40af-9958-a5b384a26eed"), true, "architectural technology", "100121" },
                    { new Guid("6272c51d-9606-4a26-8006-e06b80cbd23d"), true, "biological sciences", "100345" },
                    { new Guid("6273dbf8-6d1a-451c-94a2-1ef2aa187284"), true, "Latin American society and culture studies", "101202" },
                    { new Guid("6286ef89-fa59-4722-829b-6b6942d31b65"), false, "Mathematical Education", "G1401" },
                    { new Guid("62900ade-8d65-4998-ab0b-fd1e60137b8f"), true, "French studies", "100322" },
                    { new Guid("62a1db9e-7f37-4b0c-acc9-fd449405613b"), false, "English Lang and Literature", "Q9702" },
                    { new Guid("62aabba7-cf0b-4af1-8ecf-f390dbd19d6e"), true, "tourism", "100875" },
                    { new Guid("62ac6698-ef06-4e8b-a5cc-38c707fa1c8f"), true, "applied biology", "100343" },
                    { new Guid("632d41b6-c3e1-4bf0-ad45-852188e1c0e1"), true, "geological hazards", "101082" },
                    { new Guid("636969e1-02fe-4364-a66a-5536c03966bb"), true, "social policy", "100502" },
                    { new Guid("63a2dac9-cd6b-400f-a733-7800490ea4be"), true, "climate science", "100379" },
                    { new Guid("63cb6b6a-5a73-4a90-9a75-7fbe9efc45b6"), false, "Expressive Arts (Dance)", "W4502" },
                    { new Guid("644e6c3b-ad86-4765-b7f9-5ef50b8e7d17"), true, "history of art", "100306" },
                    { new Guid("64b0b917-5a26-4de6-a31f-4560da248ade"), true, "comparative literary studies", "101037" },
                    { new Guid("65221774-02a5-47e6-9871-3d2649a947e9"), true, "classical Arabic", "101114" },
                    { new Guid("656b86dd-4490-48a1-ad11-d7ad00f86e66"), true, "sport and exercise sciences", "100433" },
                    { new Guid("658fb64e-95f3-4cc7-9b02-044ea2842ef3"), false, "Applied ICT", "G510" },
                    { new Guid("65b5a0c6-9899-44e1-b4b3-8001f86cab5a"), true, "Celtic studies", "101118" },
                    { new Guid("65d29394-9eb2-4353-9952-2dec7a624468"), true, "applied microbiology", "100906" },
                    { new Guid("65d7207f-4e8b-4fd4-a96d-61a1a1b3ed58"), true, "Spanish language", "100332" },
                    { new Guid("660f24a6-b4c9-4fb7-8280-2786c00f1832"), true, "analytical chemistry", "100413" },
                    { new Guid("660f7262-c9e0-4d2e-a7a3-819cd06d85f3"), true, "marine biology", "100351" },
                    { new Guid("663896c2-2b03-46a4-858b-edf0051e168d"), true, "research skills", "100962" },
                    { new Guid("663fbab6-5647-42c2-8526-92e56ec7e95a"), true, "human demography", "101408" },
                    { new Guid("668094a7-ce45-442b-94ea-627f429be5a9"), true, "civil engineering", "100148" },
                    { new Guid("6688e046-7a19-4b6b-906e-ceff1c3ef85e"), true, "Chaucer studies", "101472" },
                    { new Guid("6697f9e5-03f6-4d7f-8286-0a352a720b91"), false, "Food Science and Nutrition", "D4201" },
                    { new Guid("66c1d9da-76cb-4989-952e-08c5af70a880"), false, "Ecological Studies", "C9003" },
                    { new Guid("678e3fda-78fe-4967-81a0-3025f1b6cde2"), true, "clinical psychology", "100494" },
                    { new Guid("67d15e48-3ed4-4ad5-a753-4d03009ea056"), true, "offshore engineering", "100152" },
                    { new Guid("681077c5-e4f1-44d7-9904-fed8c3b61aec"), true, "pharmacy", "100251" },
                    { new Guid("682c5a3c-c854-4492-8f93-ceb19a011c6f"), true, "audio technology", "100222" },
                    { new Guid("68402b74-8a2b-4c7c-ae6f-61930e1bfe6c"), true, "phonology", "100973" },
                    { new Guid("686e15bb-1f60-4c73-a966-7861d016467b"), false, "Welsh and Welsh Studies", "Q5205" },
                    { new Guid("688ed1a2-b598-420a-a0a5-10607ccf558e"), true, "political geography", "100668" },
                    { new Guid("689509dc-d929-47a7-b132-2e6b48016ba4"), false, "Religious & Moral Studies", "V8004" },
                    { new Guid("689b4033-c5de-463c-a34d-53dcf993d93d"), true, "Iron Age", "101439" },
                    { new Guid("68a48811-bbd4-4139-983c-5fb30054c8d0"), true, "theatre nursing", "100294" },
                    { new Guid("68d4e0e5-7154-41fd-91a2-143bb9b648d4"), true, "structural engineering", "100153" },
                    { new Guid("68dfd22c-ea5c-4a20-86c3-ce61c13e9ef2"), false, "Classical Studies (In Translation)", "Q8100" },
                    { new Guid("68edb66d-a78d-43ad-b363-21ec60687f96"), true, "social care", "100501" },
                    { new Guid("690b74e9-3e35-41a5-a03d-7c8801125552"), true, "economic policy", "100601" },
                    { new Guid("693acc49-a649-4027-8735-847f4c22783b"), true, "electrical and electronic engineering", "100163" },
                    { new Guid("69853a4b-ff4f-45f1-907a-a8cdb442d3fc"), true, "inorganic chemistry", "101043" },
                    { new Guid("69f63eca-7315-49ef-9517-9b5edc7147d1"), true, "world history", "100777" },
                    { new Guid("69fd837e-1eaa-4cf8-9ca2-7386684989f8"), true, "medieval Latin language", "101421" },
                    { new Guid("6a3b4ecc-437f-48c0-95d0-ae49f2d3c57f"), true, "higher education", "100461" },
                    { new Guid("6a760f22-f973-4392-a874-49c8a1c008aa"), true, "international business", "100080" },
                    { new Guid("6b068c45-7422-42fd-9b46-8b3a2e032da3"), false, "Gen Modern Languages", "T8890" },
                    { new Guid("6b281757-4ed5-4873-b192-fe39ccca593b"), false, "Social Education", "L3403" },
                    { new Guid("6b5d6637-b315-4fb4-ad6d-9df9ebff903b"), true, "primary education", "100464" },
                    { new Guid("6b62af01-bd46-4772-8797-695bfe62717b"), true, "geography", "100409" },
                    { new Guid("6b7ec776-9580-4a54-9538-415f884a0801"), true, "geophysics", "100396" },
                    { new Guid("6b8b6116-0214-4d31-b3dc-1bf905a1dc15"), true, "Israeli studies", "101194" },
                    { new Guid("6b8d5921-caa9-43fc-b9fd-a5c429134dc4"), true, "local history", "100308" },
                    { new Guid("6bc8c948-9445-441c-afaf-e8da559e79cc"), false, "Design Related Activities", "W2011" },
                    { new Guid("6bd9a303-5764-4fb9-991c-6b305a3c662b"), true, "further education", "100460" },
                    { new Guid("6bebc908-fdca-40c6-a11d-5779dc0e08ea"), false, "Geography & Environmental Stds", "L8002" },
                    { new Guid("6c18b6e6-97c6-410a-befe-6d010a8aef7c"), false, "Greek (Classical)", "Q7000" },
                    { new Guid("6c49180a-4b10-46f3-a3d8-2ebd75a4aa21"), true, "Charles Dickens studies", "101477" },
                    { new Guid("6c5c4b8d-4775-40fd-bdd8-025bedc16fed"), false, "Science-Biology-Bath Ude", "C1003" },
                    { new Guid("6c7ece7a-95a8-4c72-ad9c-636e099e947d"), false, "B/Tec Nat Cert In Business & Finance", "11200" },
                    { new Guid("6c93c012-8f63-4b40-8380-4509b98952ff"), true, "heritage management", "100807" },
                    { new Guid("6cae025c-9b7b-4035-b057-0147b7f31790"), true, "veterinary microbiology", "100908" },
                    { new Guid("6cdf11f1-0d6f-4220-b932-d8d2cb00d8ef"), false, "Physical Science", "F9602" },
                    { new Guid("6d0a531d-9f59-4d66-ba3d-7962207a2158"), true, "water quality control", "100573" },
                    { new Guid("6d378de3-b16e-46bd-a901-981266aa3ecb"), true, "women's studies", "100622" },
                    { new Guid("6da74eef-b0e3-4f04-be14-fda298b4ed80"), true, "Bengali language", "101177" },
                    { new Guid("6e9e7eea-e05e-45c5-85dc-563f78512d1d"), false, "Management Home Hotel & Institutio", "Z0066" },
                    { new Guid("6ead1d96-794a-49da-85a7-43d33e51d0d9"), true, "education policy", "100651" },
                    { new Guid("6ed905bd-9a9b-4f58-aa84-2c0921e58409"), false, "Combined Arts", "W9914" },
                    { new Guid("6ef4c7e7-0b72-4c09-bef8-80ab05eb0623"), true, "conservatism", "101507" },
                    { new Guid("6f233e09-5ccb-4f7d-a9ea-a572a81c89cd"), false, "Psychology (Solely As Social Study)", "L7001" },
                    { new Guid("6f2f390e-f2bd-4c8f-bd27-3aa650eb7480"), true, "applied social science", "101307" },
                    { new Guid("6f883bc9-01e3-4e13-a709-c157767747b4"), true, "cognitive psychology", "100993" },
                    { new Guid("6f8f8394-0a78-4696-abd1-2289e5ccceff"), true, "colour chemistry", "101042" },
                    { new Guid("702e1c9a-5965-4556-a408-763e6450dc07"), true, "television studies", "100920" },
                    { new Guid("7046d1ea-ad8c-4f70-b47d-a58e59f0254c"), true, "allergy", "101334" },
                    { new Guid("7048cb16-f1e4-4fdc-99ed-806fb7131ef3"), false, "Electronics", "H6000" },
                    { new Guid("7062b144-ca9e-41d7-b763-884820e9f390"), true, "Bob Dylan studies", "101492" },
                    { new Guid("707207b4-3be8-424c-86d8-7d014abba57e"), true, "applied zoology", "100880" },
                    { new Guid("70771493-508f-4927-8aa6-cb509ced6989"), false, "Literature and Drama", "Q2005" },
                    { new Guid("70ff558b-ba72-4407-8e3f-4c4de913867a"), true, "metallurgy", "100033" },
                    { new Guid("71753775-70d5-4e0b-81b8-2dee013c8b9f"), true, "folk music", "101447" },
                    { new Guid("717fb02a-d703-46f4-8872-c87cded72010"), true, "behavioural biology", "100829" },
                    { new Guid("71d2189c-ad28-4a2e-9b5e-a9bc9e21877d"), true, "engineering design", "100182" },
                    { new Guid("725b7561-ecff-4877-bf67-03ff8c2b401d"), true, "urban studies", "100594" },
                    { new Guid("72f6c7a4-c596-4b5a-8b82-6e6ce30b12f5"), false, "Behavioural Studies", "L7301" },
                    { new Guid("7308bdb2-cc93-4583-a359-8551b8ec1cdf"), true, "mathematical modelling", "100402" },
                    { new Guid("731277ac-272b-4131-b6cc-7a8db657774d"), true, "speech and language therapy", "100255" },
                    { new Guid("732d9f0a-5d01-40ba-ad14-d26896d0ee88"), true, "history of religions", "100780" },
                    { new Guid("7330f94e-adc4-45f7-af24-9b0f26647cdd"), false, "Commercial Management", "N1207" },
                    { new Guid("734d465d-bc38-463f-9478-71b5b85d9e78"), true, "applied sociology", "100619" },
                    { new Guid("735c5632-3a83-4666-b47c-d59d71af41de"), true, "cardiovascular rehabilitation", "101291" },
                    { new Guid("738d4a8d-a2b1-4eb7-b5fb-faf4caa8652a"), false, "History and Social Studies", "V5003" },
                    { new Guid("73a3a509-a5ea-4652-b1a5-613bd35243b9"), false, "Latin American Languages", "R6001" },
                    { new Guid("73adcec9-11ea-4a37-a828-a383accdb329"), true, "Bronze Age", "101438" },
                    { new Guid("73cce602-e78f-437e-95d6-0054c03306d8"), true, "radiology", "100131" },
                    { new Guid("73ed8e44-7b34-4271-880a-087e89ca8045"), true, "crop nutrition", "100946" },
                    { new Guid("740922bd-503a-4666-bd26-caef2c61ddbc"), true, "digital media", "100440" },
                    { new Guid("741188bf-6d81-4e28-91ab-d25e335c04be"), false, "Community Studies", "L5202" },
                    { new Guid("742cabc6-092e-4966-947b-3d367b9b2af1"), false, "Applied Mechanics", "H3001" },
                    { new Guid("743fe59f-1324-40fe-882c-50a96f691118"), true, "electronic engineering", "100165" },
                    { new Guid("7477f9b2-9702-45b5-a34c-65c01add0fac"), true, "Finnish language", "101150" },
                    { new Guid("748bb4da-be4c-4d4b-98c0-b9b8f9517075"), true, "Oscar Wilde studies", "101471" },
                    { new Guid("74e0eb3c-6ceb-491f-89f2-a4e347132120"), true, "recreation and leisure studies", "100893" },
                    { new Guid("74e163bf-4f1c-4ac2-b58c-e703a7f6d857"), true, "modern Middle Eastern society and culture studies", "101197" },
                    { new Guid("750530a1-0439-42be-980d-540be3148a60"), false, "Computing and Science", "G5402" },
                    { new Guid("7518c8b2-be38-42c2-8ae3-aff25afb05b0"), false, "Danish", "R7300" },
                    { new Guid("752e92ba-3447-444f-aa0a-c75cb4edf765"), true, "war and peace studies", "100617" },
                    { new Guid("75381907-195d-4a4c-9c53-ceca2bde5a22"), true, "medical law", "100693" },
                    { new Guid("75566116-9a45-41c8-9040-d5701a21a4e6"), true, "retail management", "100092" },
                    { new Guid("7588b46a-c049-47d7-8967-590e4c687d8e"), true, "aromatherapy", "100235" },
                    { new Guid("75a4b9f3-8347-4567-bf7b-d5d017649e4b"), true, "livestock", "100974" },
                    { new Guid("75ecd4d7-fb5e-4c07-b787-e06b300a1653"), false, "Biological Studies", "C1005" },
                    { new Guid("75f08a2d-0f26-4585-8d61-a4bc56870f22"), false, "Dress & Textiles", "W2201" },
                    { new Guid("75f20edd-1664-4d50-bb6a-c779ad87a773"), true, "logistics", "100093" },
                    { new Guid("7603ab8c-968f-439b-bdbe-b782c073d7f4"), true, "oral history", "101435" },
                    { new Guid("7622a564-c52b-43b4-8114-84f8d9aaa78d"), true, "general practice nursing", "100285" },
                    { new Guid("7644effc-d801-4a02-bf5c-4559d4f9cada"), true, "management accountancy", "100836" },
                    { new Guid("7651639a-0aaa-489e-b09b-081123ae839d"), true, "psychology of religion", "101003" },
                    { new Guid("76967cd3-398c-4adb-a14c-df4356f45f8f"), false, "Creative Arts (Art)", "W1004" },
                    { new Guid("76a18376-e4c7-4373-932b-c46c6d722dc4"), true, "travel and tourism", "100101" },
                    { new Guid("76adae69-7eeb-4f10-8107-fe1836af820e"), true, "dermatology", "101339" },
                    { new Guid("76b13bdc-e0ee-4593-99d6-653ca2085ee2"), true, "enterprise and entrepreneurship", "101221" },
                    { new Guid("76f5b3ef-4e87-4eac-b7d1-290270544791"), true, "scholastic philosophy", "101443" },
                    { new Guid("76fa89ac-b753-4126-a0e1-e03c5562a60c"), true, "curatorial studies", "100914" },
                    { new Guid("7752124e-3ade-47de-a2fc-cd54f3600cf9"), false, "Chemical Technology", "F1600" },
                    { new Guid("77722912-7585-4bef-8116-dcec4718c98f"), true, "film production", "100441" },
                    { new Guid("7772800b-c730-4c55-8a0d-52f40908d7e0"), false, "Sport and Physical Activity", "X9015" },
                    { new Guid("77a4e585-aba4-44c7-93df-41cf9019acd6"), true, "Indonesian language", "101368" },
                    { new Guid("77a75042-cfb4-4789-82e8-6065b4909997"), false, "Metalwork Engineering", "J2002" },
                    { new Guid("77b5b603-a87d-420d-9568-c26cd991ab03"), true, "health visiting", "100295" },
                    { new Guid("7804d247-6417-4b4d-b0b0-ea4e9a3bd153"), true, "food safety", "101018" },
                    { new Guid("782256da-8eb7-48cf-a568-b9e438257fcf"), true, "Shakespeare studies", "101107" },
                    { new Guid("7848abf4-eabf-411e-9072-b206391c7d02"), true, "turbine technology", "101397" },
                    { new Guid("78a5f339-40e0-4798-85e1-a6ea8a3b2e8b"), true, "software engineering", "100374" },
                    { new Guid("791e90b4-c278-456d-880c-a53b06c4eed4"), true, "health and social care", "100476" },
                    { new Guid("79524ec2-3dbf-4d8a-af63-be18e915b953"), true, "statistical modelling", "101034" },
                    { new Guid("79989c94-096a-42e5-86e9-54496e2255bd"), true, "human geography", "100478" },
                    { new Guid("79a5cadc-4c8a-48d5-9589-c0f1f89b0cab"), true, "transport policy", "101406" },
                    { new Guid("79d58a88-8be7-433b-b0f9-b56cd59a6a14"), true, "Danish language", "101424" },
                    { new Guid("79ea2fd5-e049-470c-9299-a56b3cdf29a8"), true, "hydrography", "101073" },
                    { new Guid("7a7941ba-09f0-465b-870d-68c07a510b98"), true, "glaciology and cryospheric systems", "101394" },
                    { new Guid("7a7e8fd1-cf47-46e8-af43-33b4981bd0d5"), true, "radiation physics", "101074" },
                    { new Guid("7a8399e5-0d3d-4be5-9f32-764f8f417c2e"), true, "socio-economics", "100627" },
                    { new Guid("7a8eac2a-8349-43ea-9cca-71a8653b763a"), false, "Technical Graphics", "W9913" },
                    { new Guid("7aa3cf06-18d7-4399-8618-1efca6b071b5"), true, "highways engineering", "100156" },
                    { new Guid("7aec7644-23c9-4373-aabf-f41b39306c29"), true, "dental technology", "100128" },
                    { new Guid("7b382d3c-c525-47f6-b3bd-a52112b9c3ab"), false, "Scandinavian Lang, Lit & Cult", "R8870" },
                    { new Guid("7b63b14f-2467-4c72-930b-50d2918a2d18"), false, "Geography With Info Tech", "H8704" },
                    { new Guid("7bbdcb33-fedf-49f5-a0d8-fdb3ebf21efe"), true, "employability skills (personal learning)", "101278" },
                    { new Guid("7bda4429-a989-4aa1-8e0b-633e0cb35475"), true, "rural planning", "100593" },
                    { new Guid("7c1e887a-1a6b-4d24-9505-6a646d0aad1c"), true, "pathobiology", "100038" },
                    { new Guid("7c8452d9-b030-42cb-bef7-6ea0d7c3962d"), true, "environmental geography", "100408" },
                    { new Guid("7ca4ed44-ab25-4f35-9ff7-a2a9a5b5c179"), true, "Persian literature studies", "101433" },
                    { new Guid("7cb1ffcd-fadb-4971-9e70-5190f5bad7e9"), true, "quarrying", "100566" },
                    { new Guid("7cd7db73-e8fc-4c6c-93fa-a23003807f1b"), true, "creative arts and design", "101361" },
                    { new Guid("7cfe171e-221a-4490-bee3-d8fd833d0d76"), true, "drug and alcohol studies", "101332" },
                    { new Guid("7d3ad107-fd7e-47b4-b166-ec00fb93ff71"), true, "Welsh language", "100333" },
                    { new Guid("7d44306f-9b5c-4c22-a59d-e29233d645a0"), false, "Resource ManagementF9020Rural & Environmental ScienceF9007", "N1103" },
                    { new Guid("7d8f2a27-71d5-4b8e-8920-894e548b6c44"), true, "electronic publishing", "100926" },
                    { new Guid("7dafcdaf-24bd-4f17-a82a-fbcf8f251ab9"), false, "Social Science/Studies", "ZZ9010" },
                    { new Guid("7de02baa-59ce-4935-9c5c-c211f34ec4a6"), true, "military history", "100786" },
                    { new Guid("7de34469-0e70-4dac-b332-064d2c37673d"), false, "Business Policy", "N1205" },
                    { new Guid("7ded373a-9c34-43df-8e77-6c002e9a3d5f"), true, "management studies", "100089" },
                    { new Guid("7ec9a7f3-761c-4a71-82a1-6092801f47ab"), true, "sonic arts", "100862" },
                    { new Guid("7ecab0ee-c41d-444d-9520-462f7ed202d8"), false, "Art and Design", "W2001" },
                    { new Guid("7ecba2a4-e38d-4696-8eae-d0472cc88846"), true, "computer and information security", "100376" },
                    { new Guid("7ef73381-837c-4ef2-821f-42829f02b129"), true, "modern Middle Eastern languages", "101191" },
                    { new Guid("7f309218-5591-474f-b04d-c2fd3895fb3f"), true, "financial economics", "100451" },
                    { new Guid("7f648961-a63d-4bde-b7f3-32f6b5861373"), false, "Craft, Design & Technology", "W2403" },
                    { new Guid("7f97b210-c796-40e1-8834-c363c6da9470"), true, "macroeconomics", "101402" },
                    { new Guid("7fb9a4f0-fb88-41d5-ba47-0155cc0ac060"), true, "evolution", "100858" },
                    { new Guid("8001cf73-50a1-4861-b14c-5c4d171879a0"), true, "community justice", "100659" },
                    { new Guid("802952d9-3e1c-4679-b39f-2af7a4fa5b98"), false, "History, Geog & Relig Studies", "Q9711" },
                    { new Guid("80852e3c-d1fd-47c9-a315-064394689c60"), true, "building services engineering", "100147" },
                    { new Guid("80862308-4c4a-4d36-9b9d-587794551d1b"), false, "Greek and Roman Civilisation", "V1016" },
                    { new Guid("8094d9b4-6b2d-4614-815e-0304d9c7f05b"), true, "torts", "100690" },
                    { new Guid("80a05f33-8225-44ea-8011-819dd9ca2daa"), true, "minerals technology", "100155" },
                    { new Guid("80cc809d-2d3c-44f4-ab8c-e244844ca236"), true, "stage management", "100703" },
                    { new Guid("80ce5893-9a79-4492-b2d2-944f0be923b9"), false, "Combined Science", "Y1001" },
                    { new Guid("81110b31-8b05-4d6b-8689-e15046947374"), true, "crystallography", "101044" },
                    { new Guid("812a7beb-a791-44bd-a183-80ab09aedac9"), true, "fluid mechanics", "100577" },
                    { new Guid("81713596-a4d8-426d-9531-7397709e4134"), true, "pre-clinical medicine", "100276" },
                    { new Guid("8176f6cf-87b2-4bd9-90b2-dc717d16defd"), true, "animal science", "100523" },
                    { new Guid("8186d2bd-9804-4cdd-a05b-ffa7222efa07"), false, "Spanish Lang, Lit & Cult", "R8840" },
                    { new Guid("81c20efd-aef9-4987-9113-31b30af14084"), true, "American history", "100767" },
                    { new Guid("8206fd68-d394-4255-a26f-91dfb5c07b51"), true, "veterinary dentistry", "101347" },
                    { new Guid("822ac69e-48e6-4543-9a6c-b55e23f75d41"), true, "dental nursing", "100283" },
                    { new Guid("825e8662-bc93-41b3-b994-0003aed3952d"), true, "interior design and architecture", "101316" },
                    { new Guid("8261de11-3fd0-4dd1-8397-6107b84499e7"), false, "Language Literacy & Literature", "Q1407" },
                    { new Guid("82621ad6-d432-425b-9aba-1b4ba9eb59c3"), true, "geological oceanography", "101086" },
                    { new Guid("8270e249-6f8a-45fe-b40c-8cd1cc55e264"), true, "veterinary epidemiology", "101220" },
                    { new Guid("82a6ad3f-3646-4c12-881a-de38bc893bbd"), false, "Design Technology", "W2505" },
                    { new Guid("82afc3cd-ffc4-4faf-af47-b3839d12190d"), true, "nuclear engineering", "100172" },
                    { new Guid("82baac60-977e-4778-9c92-6338b47a480a"), true, "physics", "100425" },
                    { new Guid("82be3d5f-af3e-402f-b613-b9be2dbb3c91"), true, "criminal justice", "100483" },
                    { new Guid("82c8090f-0073-467f-97c2-c4639253b50d"), true, "gastroenterology", "101331" },
                    { new Guid("8300f384-ae4c-4dc2-8d11-8a31412adfc7"), false, "Literacy", "Q1405" },
                    { new Guid("8316caa5-6b1b-4e4b-a622-13c562ab6010"), true, "atmosphere-ocean interactions", "101351" },
                    { new Guid("8357f3c4-bd0d-41f1-af1c-f6e90888fac5"), false, "Science-Geology-Bath Ude", "F6005" },
                    { new Guid("8367b6fa-78f0-4e4e-b417-c7721212adf8"), true, "environmental biology", "100348" },
                    { new Guid("8375c130-9cb0-4d5e-b86a-7f6ade623b3b"), false, "Finnish", "T1300" },
                    { new Guid("8392b1a8-95bb-4947-b705-f9bc1268162b"), true, "cereal science", "101388" },
                    { new Guid("839464c3-199b-48e7-8156-e715abb971de"), false, "German Language & Studies", "R2103" },
                    { new Guid("840b0679-9168-4c39-8cdd-b53de203b53b"), true, "private law", "100686" },
                    { new Guid("843111ab-fcbf-45f1-92c8-2ee8dbfd5da8"), true, "theatre studies", "100698" },
                    { new Guid("843f3167-f954-4dcd-86e1-ea696892c90c"), true, "developmental psychology", "100952" },
                    { new Guid("8449bb92-7f10-45c2-8825-7f846ba37011"), true, "psychotherapy", "100254" },
                    { new Guid("84992cbd-0ee6-4166-b666-5d25f3a9e827"), false, "Engineering Technology", "H1002" },
                    { new Guid("84bbaa1c-524c-4925-a6b8-1bfbfc7fe861"), false, "Maths.Science and Technology", "G1502" },
                    { new Guid("84e5bc67-40d6-49cd-9b16-f24015626571"), true, "construction and the built environment", "100150" },
                    { new Guid("8516bb0f-fee7-492a-872c-e93ed391ddb0"), true, "radio production", "100924" },
                    { new Guid("851b3ab2-2748-40a0-9d99-ea1b8a9f2c30"), false, "Organisation Studies", "N1202" },
                    { new Guid("85466ca4-c7ac-49d4-92ea-4dc2ab330852"), true, "theatrical wardrobe design", "100705" },
                    { new Guid("85626e72-57fc-4ee9-a84c-781fa7f2ae14"), true, "sustainable agriculture and landscape development", "100998" },
                    { new Guid("8574e03a-3f1d-4415-851a-887f1b298cb7"), false, "Design For Technology", "W9925" },
                    { new Guid("85b46862-d64d-4eec-a4fa-3120b6d9b60c"), true, "Russian and East European society and culture", "101158" },
                    { new Guid("85f6f283-6622-4f82-ae37-9aa499989e24"), true, "Milton studies", "101485" },
                    { new Guid("863d333f-9174-48a8-b054-6edb167144c1"), true, "automotive engineering", "100201" },
                    { new Guid("86c56323-86e3-44eb-92d1-50b061a15076"), false, "Printed Textile Design", "W2205" },
                    { new Guid("86d68f4d-a885-434b-970a-87751cae1070"), true, "secondary teaching", "100512" },
                    { new Guid("872c7132-4b7f-4443-b06e-4e7dc9ac8a41"), true, "ecosystem ecology and land use", "100864" },
                    { new Guid("877d6f14-19ea-4baf-8a81-f6c38b5ca4f8"), false, "Product Design and Technology", "W9910" },
                    { new Guid("8788eb1a-7148-459e-b190-138969009a87"), false, "Human Ecology", "C9005" },
                    { new Guid("878d634a-2cee-439f-a3c9-3f88b4a4e61e"), true, "librarianship", "100913" },
                    { new Guid("878e10ac-cffa-4b3a-bb5c-cc0d59ae8221"), false, "Drama and Media Studies", "W9924" },
                    { new Guid("87d68682-c0e8-4500-93c5-3f7ff456164e"), true, "linguistics", "100328" },
                    { new Guid("88007c7a-3457-4dd3-90e3-86922919bcd6"), true, "motorsport engineering", "100206" },
                    { new Guid("8811f5d8-179f-4cc7-897c-973ef91e337e"), false, "Secretarial Studies", "N9700" },
                    { new Guid("8835ae44-434f-4134-af1c-eb0e6c5fb817"), true, "soil science", "101067" },
                    { new Guid("888bd3d5-86e6-41bb-83c4-ec3d31629ee4"), true, "physical geography", "100410" },
                    { new Guid("88bc0af5-32bd-40cd-9a66-8aa683487d62"), true, "human-computer interaction", "100736" },
                    { new Guid("89132be1-8ec1-4a69-bfc4-13d0a819c10b"), true, "organic chemistry", "100422" },
                    { new Guid("894e7409-da7a-4ef8-b374-ed20d67e1c44"), true, "planetary science", "101103" },
                    { new Guid("898e9e44-ba66-46b1-b5b4-7f01ea926292"), true, "anthropology", "100436" },
                    { new Guid("899aa4dd-74b5-4159-b722-48fdd4822a4b"), true, "footwear production", "100110" },
                    { new Guid("89f96a83-60af-4e38-8f22-bff721d7ccda"), true, "community work", "100655" },
                    { new Guid("8a1675e3-881b-4c2d-a98f-52559e2497ac"), true, "parasitology", "100826" },
                    { new Guid("8a335abc-15e4-4ca7-ad61-01d5434f6f3a"), true, "computer games programming", "101020" },
                    { new Guid("8a9fc70c-03a6-4b93-a9ea-55352bd19273"), true, "conducting", "100650" },
                    { new Guid("8aa2df3f-91c7-4949-b5d9-85368e8ae4bc"), false, "Home & Community Studies", "L5204" },
                    { new Guid("8aefeb1c-88ae-4f29-8c38-d889998f832f"), true, "horticulture", "100529" },
                    { new Guid("8af59050-5c7d-48d9-b6a5-1e71d79a83fc"), true, "sociolinguistics", "101016" },
                    { new Guid("8afd4cc6-6ee2-45c7-bf44-86b1dc36f1d4"), true, "Northern Irish law", "100677" },
                    { new Guid("8b09f020-7f69-4471-b25b-f3db7550df60"), true, "dementia studies", "101329" },
                    { new Guid("8b310878-d485-4bb2-9a50-35217398aba8"), true, "adult nursing", "100279" },
                    { new Guid("8b4b8c8b-2c88-4cd1-801a-777d1856677f"), true, "strategic management", "100810" },
                    { new Guid("8be27f5e-e082-40d3-8bb3-0d3ae2513d1d"), false, "Medieval Studies", "V1002" },
                    { new Guid("8c250855-95d2-49d1-ba53-7b91d29fee03"), true, "applied computing", "100358" },
                    { new Guid("8c2f07dc-c6ce-4fe4-83fa-df96ada34f9b"), false, "Eng. As A 2nd Or Foreign Lang.", "Z0034" },
                    { new Guid("8c3ed0a2-2f4d-4a4c-bb1e-a06c20194d2c"), true, "equine studies", "100519" },
                    { new Guid("8c807866-feba-496e-937d-e7996c276366"), false, "French With Russian", "R8205" },
                    { new Guid("8cc05fe7-ed83-45b1-b333-ae708454d891"), true, "classical reception", "101129" },
                    { new Guid("8cda6df0-be6c-4d64-8636-7a2e3ce3d755"), true, "learning support", "100462" },
                    { new Guid("8cf23147-2506-46bd-b731-00b15527f686"), true, "naval architecture", "100207" },
                    { new Guid("8d00899b-c61a-4a25-a4d2-40a3e656d530"), true, "quantity surveying", "100217" },
                    { new Guid("8d02bfce-06d7-44ab-848d-a9db1a1bdadc"), true, "aeronautical engineering", "100114" },
                    { new Guid("8d1d9756-ef6b-416b-8d94-58077e1fd621"), true, "classical art and archaeology", "101440" },
                    { new Guid("8d30ed21-d0d0-419f-8b8b-856afbf71f2f"), true, "banking", "100827" },
                    { new Guid("8d324f1a-95e9-4478-8f71-7b59afa80edf"), true, "databases", "100754" },
                    { new Guid("8d48fd92-358f-40fe-91e0-299caee16019"), false, "Creative Arts (Dance)", "W4501" },
                    { new Guid("8d55d3aa-dd6f-42cc-8bed-0e905f398da5"), true, "healthcare science", "100260" },
                    { new Guid("8db70b38-3d87-4105-af92-1115d6e3e070"), true, "Korean studies", "101212" },
                    { new Guid("8dba7455-2fb1-4cfd-988c-e31d7d030a53"), false, "Gen Studies In Social Sciences", "Y2000" },
                    { new Guid("8dcc2dc9-e5b0-4f0e-aa2e-677c318bbd98"), false, "Expressive Arts (Physical Ed)", "W9917" },
                    { new Guid("8df64559-8b93-4c06-b12a-dbc7d9cf554b"), true, "agricultural machinery", "101010" },
                    { new Guid("8dfdcb39-7808-4a1c-94cc-91feaea89902"), true, "drawing", "100587" },
                    { new Guid("8e0eea77-c780-4dc9-a817-80195be0cda9"), false, "Outdoor & Envir Studies", "X9018" },
                    { new Guid("8e143dbd-6c89-499b-a21f-5b87feac2c06"), false, "Welsh and Drama", "Q5206" },
                    { new Guid("8e16865f-5f5b-47a8-9ea3-31f3ba670433"), true, "veterinary biochemistry", "101461" },
                    { new Guid("8e4bd930-f37b-4eb4-ac94-6996e1413374"), false, "Science-Bal Sc With Environ Sc", "F9610" },
                    { new Guid("8e9b4527-04e3-4a2d-940a-7d6fe9071e9b"), false, "Historical & Geog Studies", "V9002" },
                    { new Guid("8f07558e-ce24-4435-a266-167d8ee7c572"), true, "veterinary pathology", "100938" },
                    { new Guid("8f0862ef-c776-481d-ba62-424cbd418ced"), true, "computer science", "100366" },
                    { new Guid("8f4d2d50-f246-46af-a380-acc469e45d6f"), true, "metaphysics", "101441" },
                    { new Guid("8f64afd7-3068-467a-8a3b-792a75a2a113"), false, "Mathematical Engineering", "J9201" },
                    { new Guid("8f652efe-7f41-40bd-b854-92c228fa7130"), true, "Gaelic literature", "101497" },
                    { new Guid("8f6c70cf-d87a-4b23-9477-bbbb41b40f6d"), false, "Language Arts", "Q1410" },
                    { new Guid("8f8bd196-37f8-4886-bcab-85cd10a4b8f4"), false, "Express. Arts(Music and Drama)", "W3006" },
                    { new Guid("8fae3d5d-bbf1-464d-8f30-ab4987b0a28a"), true, "Sumerian language", "101416" },
                    { new Guid("8fbb17b7-77fa-4395-bb5a-6fde9a6a0a68"), false, "Frenchlang and Contemp Studs", "R1104" },
                    { new Guid("8fcf9358-37aa-4674-8bb6-7982fc422350"), true, "Irish language", "101121" },
                    { new Guid("8fd4ea15-20b3-4bc8-a595-4524f028a3d7"), true, "religious writings", "100800" },
                    { new Guid("8fe42b9a-1e67-41fa-a069-2b1503593bf0"), true, "solid mechanics", "101396" },
                    { new Guid("8fff0509-d2b9-4384-809a-794092904692"), false, "Biology (With Science)", "C9702" },
                    { new Guid("90298a35-1f1c-4223-993d-a445726f08ab"), true, "metabolic biochemistry", "101380" },
                    { new Guid("90332e2d-8fa1-40f0-8f2a-63b10ac4ab1f"), true, "systems auditing", "100756" },
                    { new Guid("9078fa42-43b2-4f70-a41d-9c4a4fa39850"), false, "Technology (Home Economics)", "H8702" },
                    { new Guid("90d396ae-e34c-464d-a8fd-21fa9d69f04a"), true, "professional writing", "100731" },
                    { new Guid("90dda72e-0233-4a44-944e-54b02ad22fa8"), false, "Educational Drama", "W4008" },
                    { new Guid("910a8447-ccb7-4406-8201-10a84c726e39"), false, "Gen Euro Lang, Lit & Cult", "T8820" },
                    { new Guid("91270f41-d6c6-4424-b9ae-ee87aa5b7799"), false, "Welsh As A Modern Language", "Q5201" },
                    { new Guid("91277f6b-edce-4355-a10b-a594475f481e"), true, "applied physics", "101060" },
                    { new Guid("913129c6-f6ec-4984-8889-ade735df6f1c"), true, "gerontology", "101326" },
                    { new Guid("916701ec-cf74-4862-bfed-914420c60d77"), false, "Creative Arts (Drama)", "W4001" },
                    { new Guid("919d8745-1326-460a-a3a7-e69758915fb7"), true, "cultural geography", "100671" },
                    { new Guid("91a78289-62df-4639-bded-fd2affe58447"), false, "Drama & Movement", "W4002" },
                    { new Guid("91fcfccc-53f5-483c-875b-c53bd743a36e"), true, "ecotoxicology", "101459" },
                    { new Guid("91ffd336-1220-4c87-be4d-114ac343ada5"), true, "nationalism", "101357" },
                    { new Guid("9260b0bb-a8cb-4b3a-afb1-414e39b2736b"), false, "Provisions 16 - 19", "X9017" },
                    { new Guid("92884580-8d37-4161-964b-c160133e12cb"), false, "Maths.Stats. and Computing", "G5009" },
                    { new Guid("92ded31e-06ea-4932-9639-8ceecfd05e8b"), true, "plant pathology", "100874" },
                    { new Guid("92e25534-7fc2-4563-bf6d-c4e28fff52fb"), true, "diabetes", "101338" },
                    { new Guid("92e2dc3c-3101-4b8b-8270-f7f4e7b00459"), true, "dynamics", "100429" },
                    { new Guid("92fa27a2-726c-462a-b629-1bc53013b91f"), true, "computer architectures", "100734" },
                    { new Guid("92fb1a16-ff06-4425-a2e2-1c2cc09f48d8"), true, "ceramics", "100003" },
                    { new Guid("931ed6bc-064e-4cb9-8151-bcbee30f25de"), true, "environmental management", "100469" },
                    { new Guid("936184ac-9b1a-4dea-87fd-c8fbdbeff8e8"), true, "pharmaceutical engineering", "100144" },
                    { new Guid("93c795fc-800b-4ffe-8c57-a2208764e71a"), true, "Polish language", "101153" },
                    { new Guid("93d2ad81-bcfc-4e10-b0af-c5e6ccafc1e2"), true, "Islamic studies", "100796" },
                    { new Guid("93e62708-ff41-42d8-ac7c-70136ae6480b"), false, "Agricultural Business Mngement", "N1000" },
                    { new Guid("944c021f-1019-4cb5-96ca-c052c86d1fea"), true, "music theory and analysis", "101449" },
                    { new Guid("945059d1-1bf5-4117-91f1-ee0ce9eaf694"), false, "French With Spanish", "R8204" },
                    { new Guid("9465ad42-2570-49c3-9214-7b8128a75dc1"), true, "biomechanics", "100126" },
                    { new Guid("94937de5-91fa-4a35-8a84-7944928502a5"), true, "paper-based media studies", "100922" },
                    { new Guid("949c8474-c00a-4130-bb35-c7445cb1885d"), true, "teaching English as a foreign language", "100513" },
                    { new Guid("94f8b8a1-19ff-43da-b638-f8f4fa6e2ebf"), true, "Hungarian language", "101427" },
                    { new Guid("9505b86c-8d13-4353-a37c-919dd8b67100"), true, "industrial biotechnology", "100137" },
                    { new Guid("95154f00-abd0-4330-a5dd-a9450229461b"), false, "Science (With Biology)", "C9704" },
                    { new Guid("9542afff-82f3-4e50-a9a5-57d1d6992cef"), false, "Latin Amcn Lang, Lit & Cult", "R8860" },
                    { new Guid("95be21ee-9b90-4e4c-a104-ad413b9a3712"), false, "Science With Physics", "F9625" },
                    { new Guid("95df119c-a535-4163-acc2-7a3576720cb6"), true, "mycology", "100872" },
                    { new Guid("95f0064c-6ae0-4c44-a96d-744849d14d15"), true, "European Union politics", "100612" },
                    { new Guid("95f3bd36-3c5c-42ff-9222-2fc04bb6dc84"), true, "ophthalmic dispensing", "101511" },
                    { new Guid("9604bb69-2505-4b31-82b1-1bdc87baffcc"), true, "classical Greek studies", "101126" },
                    { new Guid("9647aef3-4848-46c1-93b5-f59a8b794691"), true, "medical biotechnology", "100138" },
                    { new Guid("96b84e54-4f40-44c9-96bb-71cf795acc06"), true, "archaeology", "100299" },
                    { new Guid("96be7eeb-a178-4958-87f8-5cf9ea086bec"), false, "Gaelic", "Q5001" },
                    { new Guid("96d77074-bef1-4522-8a42-aa03da8365c2"), true, "aerospace engineering", "100115" },
                    { new Guid("96e593ee-6bb4-4c16-8276-bcd17439e309"), false, "Chemistry and Science", "F9631" },
                    { new Guid("97756563-1c87-4e5a-9fd1-42b42bf3ec86"), true, "disability studies", "100625" },
                    { new Guid("9799ad2f-f611-488d-ab8e-d79c2caea3ad"), true, "Virginia Woolf studies", "101491" },
                    { new Guid("97ac7c7c-19f4-4414-a713-73872ddfa4de"), true, "promotion and advertising", "100855" },
                    { new Guid("97fa2cd9-fc3b-4d28-881d-fe9b2915fd93"), true, "garden design", "100590" },
                    { new Guid("984c5341-1667-425f-8fba-d20df6b378bb"), true, "chemical engineering", "100143" },
                    { new Guid("987ae7ca-ab5f-46bf-870b-db3cce5b99a9"), true, "buddhism", "100798" },
                    { new Guid("98be8ccc-3ebc-4b4d-90d3-20fb5d97bb22"), true, "parallel computing", "101400" },
                    { new Guid("9918f4b4-b96d-47ff-a398-47412fe6e7a3"), false, "Education Of Children With Learning Difficulties", "X6001" },
                    { new Guid("996fbfb7-dc91-42d5-bb49-fd0ae10990da"), true, "North American literature studies", "101203" },
                    { new Guid("9984666b-9a9c-4ab3-8950-8bba1f733669"), true, "adult education teaching", "100507" },
                    { new Guid("99c1894e-8cb2-4c28-877b-db2dd835d705"), true, "Irish language literature", "101413" },
                    { new Guid("99c2f2e0-51b8-4dcc-b6bb-36a3a4ee19cb"), false, "Plant Science", "C2002" },
                    { new Guid("99cb1ea9-b4f9-44cd-a8e1-8847ac5dcde8"), true, "cell biology", "100822" },
                    { new Guid("99dc47e0-86d3-46bd-bab0-39e8b2821eab"), true, "quantum theory and applications", "101300" },
                    { new Guid("99f2ee9e-c767-4c83-99bf-9a334ea281e3"), false, "Greek History", "V1007" },
                    { new Guid("99ff97db-7ec4-42e2-a07e-f922c06a8967"), true, "complementary medicines and therapies", "100242" },
                    { new Guid("9a4a99f7-7741-4cd2-b213-0f7b5bf574bc"), false, "Pure Science", "F9005" },
                    { new Guid("9acc22c4-c625-4381-b1f7-25c1af4e7779"), true, "design", "100048" },
                    { new Guid("9af48f73-514c-43c6-8141-0ebf5c3e634a"), true, "historical linguistics", "101410" },
                    { new Guid("9b36b8cf-3848-4009-8786-dbac9ab957e0"), true, "education studies", "100459" },
                    { new Guid("9b4ed7f0-ec01-4a51-9bd8-cf528e4b90bd"), true, "Latin studies", "101124" },
                    { new Guid("9b4f0fda-5942-4bc5-bd4e-8797caa79a37"), false, "Technology (C.D.T.)", "W2407" },
                    { new Guid("9b94f081-556e-4bc9-acf2-8794b0c33c03"), true, "social work", "100503" },
                    { new Guid("9c1652bf-d3c4-43ec-9aab-bc877bd29e63"), true, "microwave engineering", "100177" },
                    { new Guid("9c7c9a16-d994-4610-9177-8259af7bc012"), false, "Engineering (General)", "H1000" },
                    { new Guid("9c9d0929-f580-4d8c-8d1a-896978445338"), false, "Irish", "Q5300" },
                    { new Guid("9cba6eae-16f8-4aa0-9d17-93bd12a8bda2"), true, "business psychology", "100954" },
                    { new Guid("9d1022ea-20f0-4fbc-9644-72fb283c54f9"), false, "Professional Studies", "N1209" },
                    { new Guid("9d325e86-1b3a-4ca3-b94a-47402533458b"), true, "neurological rehabilitation", "101290" },
                    { new Guid("9d33b786-b705-4473-9436-7d912cc0d3c0"), true, "Scots language", "101412" },
                    { new Guid("9d41db8e-6fcd-449d-869c-85d74ff0a3af"), true, "Chinese history", "100771" },
                    { new Guid("9d423006-3647-48cd-b629-698090ce4bc8"), true, "entomology", "100882" },
                    { new Guid("9d478b6e-01c4-451c-8728-11a9a6146ab8"), true, "aviation studies", "100229" },
                    { new Guid("9d4d906c-74b9-4ad8-9711-5c291ca33cac"), true, "Australasian studies", "101206" },
                    { new Guid("9d947c4c-0da8-4aa8-978c-9ae337585c0e"), true, "spa management", "100894" },
                    { new Guid("9d96fa44-b9b0-48b9-b65a-31403dc54e8e"), true, "environmental chemistry", "101045" },
                    { new Guid("9dc4fdce-4205-4eb8-a6ce-6e8e7d9e1df8"), true, "choreography", "100711" },
                    { new Guid("9e367893-cff1-4c02-b97c-890f0056618f"), false, "Technology With Science", "F9609" },
                    { new Guid("9e42f292-cee2-4b5d-a7ef-7cdd36631b47"), true, "musicianship and performance studies", "100637" },
                    { new Guid("9f38e273-14f9-4696-ba90-8f7b5dcb035d"), false, "Craft", "W2402" },
                    { new Guid("9f3cbcad-1008-40d4-9d24-09c1b7835160"), true, "pre-clinical veterinary medicine", "101384" },
                    { new Guid("9f542ac3-cc98-445e-be82-9c505c7aef6d"), false, "Psychology (Not Solely As Social S)", "C8000" },
                    { new Guid("9f62e88f-e34a-476a-b621-7142f116967c"), true, "agricultural chemistry", "101024" },
                    { new Guid("9f791e88-4732-4ec0-bedf-483439257833"), true, "medical sciences", "100270" },
                    { new Guid("a00c0994-009e-44c3-ba53-e74c3b4f5500"), true, "Italian history", "100764" },
                    { new Guid("a01b3467-d412-4845-a433-6f42e1b638f5"), true, "medical statistics", "101031" },
                    { new Guid("a0391990-0102-4bbf-8d9e-0b204d9c1716"), true, "ancient Egyptian studies", "101113" },
                    { new Guid("a0450f4d-5e80-4aaa-88fb-5cd74f9524ef"), true, "Scottish literature", "101111" },
                    { new Guid("a047ad19-8a9d-48cb-9f89-a91ced273443"), true, "music", "100070" },
                    { new Guid("a057cea6-b264-47d4-86c8-fe76260e33e1"), true, "genomics", "100901" },
                    { new Guid("a0b21914-e5aa-4c14-affd-163f7f6d5b14"), false, "Political Education", "M1004" },
                    { new Guid("a0dafbad-f44f-4a97-836d-7424a11fb281"), true, "European studies", "101159" },
                    { new Guid("a146ec86-4547-402c-a995-bb9419c29344"), true, "thermodynamics", "100431" },
                    { new Guid("a168b9b1-8eaa-46a9-9b1c-129427dbb8ca"), true, "forestry and arboriculture", "100520" },
                    { new Guid("a1e129f5-629b-4585-8c5e-694aad18b37c"), true, "divinity", "100799" },
                    { new Guid("a1ff9aea-0b69-4a21-8d5c-83850a07e04f"), false, "German With French", "Q9708" },
                    { new Guid("a23a3eb5-e230-4603-9949-0172fcaa76fb"), true, "hypnotherapy", "101340" },
                    { new Guid("a24b1afb-6d15-476c-a5b2-c9550669883e"), true, "stone crafts", "101455" },
                    { new Guid("a2916131-a665-424f-804e-36a4b2361c85"), true, "cognitive neuroscience", "101381" },
                    { new Guid("a29959a1-7c19-46a0-a1d2-392ba4934a0c"), true, "community forestry", "101014" },
                    { new Guid("a2c79ffc-c5d5-43b3-82d9-1eb540d57805"), false, "Italian Lang, Lit & Cult", "R8830" },
                    { new Guid("a2ccdaa6-bae0-48c3-9776-f49072c96dbb"), true, "policing", "100486" },
                    { new Guid("a3489136-86d4-4462-8d86-5516f2b5b4ae"), true, "Japanese society and culture studies", "101171" },
                    { new Guid("a36d1a53-a318-4965-be40-6be47c112b9b"), true, "jazz composition", "100870" },
                    { new Guid("a42733d1-4a80-4ea9-b805-9fedbdac4d2a"), true, "condensed matter physics", "101223" },
                    { new Guid("a4ab58c0-a4c6-4e8e-ba44-cacd23036d06"), true, "Czech language", "101155" },
                    { new Guid("a50f5127-f0c7-4f7d-83fe-aa9fd6946f8c"), true, "psychology of music", "101363" },
                    { new Guid("a5293b76-fac0-4b46-a48c-07cf92fa5ef1"), true, "multimedia journalism", "100445" },
                    { new Guid("a56a8c42-f1e4-4988-9950-f825a07f74bd"), true, "contemporary dance", "100886" },
                    { new Guid("a5a1ea09-44fd-4468-9f50-c3b74744781c"), true, "jazz performance", "100656" },
                    { new Guid("a5b6f114-cc46-481a-8e64-f9ff52f621a6"), true, "Polish society and culture", "101500" },
                    { new Guid("a5e0cd95-cbbb-4b8d-a8ad-99b81a50be8f"), true, "navigation", "100230" },
                    { new Guid("a622194e-c1df-4f27-a8f5-cf8c57fa5380"), true, "property management", "100820" },
                    { new Guid("a634a867-f53e-40ef-9f4a-e67d3a56af18"), false, "Creative Studies (Music)", "W3004" },
                    { new Guid("a6442ffc-d6de-4312-8d53-d657c3ab7ff9"), false, "Foreign Languages", "Q1303" },
                    { new Guid("a64778bc-d332-4d40-aac8-b8212f74880f"), false, "Civilisation", "Q8201" },
                    { new Guid("a686e272-60f9-473b-b1a8-cb5350b7fafa"), true, "Italian literature", "101137" },
                    { new Guid("a6cf9e94-88cd-4a2a-8f72-d92f517f7da2"), false, "Chemistry With Core Science", "F9616" },
                    { new Guid("a6e9bfcf-3870-496d-9cad-262d7c3c7216"), true, "health policy", "100648" },
                    { new Guid("a6ee40aa-94e6-4d61-afa5-2dfd0ee8baeb"), false, "Science:earth Science", "F9026" },
                    { new Guid("a6fab7ee-c355-49ae-aa68-19828dd7a84d"), true, "historical performance practice", "100661" },
                    { new Guid("a6fafb8b-2eb6-47ef-9d50-1aa0ff06df6b"), true, "research and study skills in education", "101088" },
                    { new Guid("a7042311-8fc8-40e9-920b-9937fb2facc0"), false, "Info Technlgy/Computing", "G5602" },
                    { new Guid("a715cc9d-04d4-46de-88fe-895967338509"), false, "Management In Education", "X8000" },
                    { new Guid("a725a810-2554-4f81-9d16-21a36b582e97"), false, "Industrial Studies", "N6100" },
                    { new Guid("a748cab6-aaf6-4388-8aa1-fd9d03d10919"), true, "South East Asian studies", "101372" },
                    { new Guid("a7ab895e-68f1-4dd2-b8f7-0cd50fc6be4f"), false, "Mathematical Studies", "G1400" },
                    { new Guid("a7c109ea-4260-4e9d-bd62-c741a4453031"), true, "D.H. Lawrence studies", "101476" },
                    { new Guid("a80875e8-f7b5-4855-ba3f-97ccaa937cfe"), true, "Swahili and other Bantu languages", "101366" },
                    { new Guid("a81f4263-4a75-4c22-b1c4-ef66a5a533d8"), false, "French Politics", "M1003" },
                    { new Guid("a8323393-5ec2-42e7-8d73-76be8aee33b3"), true, "advertising", "100074" },
                    { new Guid("a83b2c93-3809-415e-bda0-5310dcda734e"), false, "Social and Enviro Studies", "L3406" },
                    { new Guid("a856ddfa-fd83-4a10-9a00-59612905a114"), true, "Norwegian language", "101149" },
                    { new Guid("a89bfa9e-51a8-46b4-8ba7-7ad1e8d97072"), false, "Creative Design", "W2003" },
                    { new Guid("a8f56577-e884-4315-90cf-406eb15e5146"), false, "Craft & Design", "W2404" },
                    { new Guid("a9333780-bfb9-4dda-8319-bd6400d6f4ae"), true, "French history", "101248" },
                    { new Guid("a95c27b8-1a97-4a3b-a9f4-b18721063f1f"), true, "circus arts", "100707" },
                    { new Guid("a9f10176-2c1f-4dac-a0f5-24722e9797ab"), true, "comparative law", "100683" },
                    { new Guid("aa1a0627-dee0-4a2a-a770-53073d154c07"), false, "Applied Educationapplied Education", "X9004" },
                    { new Guid("aa46b694-7bfd-42d1-af4c-9125e263abe5"), true, "prosthetics and orthotics", "100130" },
                    { new Guid("aa4b190d-4376-415b-a78b-1cbd267cb923"), true, "Italian society and culture", "101136" },
                    { new Guid("aa549e15-f91e-4c4d-a12d-f3b2cd22e415"), true, "tourism management", "100100" },
                    { new Guid("aad8fe97-278a-424e-ae8f-50f0e5f3ac4e"), true, "ancient Hebrew language", "101117" },
                    { new Guid("ab2b34d0-5a97-4521-8083-301423063a6b"), false, "Vocational English", "Q1406" },
                    { new Guid("ab5520a6-1220-46bf-bbda-759395473b4e"), true, "applied geology", "101104" },
                    { new Guid("ab6818a1-2296-43d8-a925-d2f1df414a01"), true, "applied economics", "100597" },
                    { new Guid("abaaa9f1-353e-4941-90bd-0f8503f7f0c6"), false, "Modern Foreign Languages", "R8201" },
                    { new Guid("abd77df9-de8e-4fea-9630-69015aa691b9"), true, "tissue engineering and regenerative medicine", "100572" },
                    { new Guid("ac2069d5-c000-46cc-acdb-3c4a33fd87ad"), true, "paper technology", "101356" },
                    { new Guid("ac2c966c-3b76-4675-b015-af47a4c499ca"), true, "animation", "100057" },
                    { new Guid("ac8dd31d-bc56-4ed3-8311-277c0edc98a2"), true, "pharmacology", "100250" },
                    { new Guid("ac90e7af-c5f8-4d86-9f77-1f2911c24541"), false, "Computer and Information Tech", "G9009" },
                    { new Guid("acaabc4d-96fd-4b2a-9f2d-4b6224d0c3d7"), true, "aerospace propulsion systems", "100564" },
                    { new Guid("ace453a2-6786-4403-8c1e-941000b9cbe9"), false, "Creat Stud (Art, Move & Music)", "W9916" },
                    { new Guid("ad2f87ac-ee5c-40b7-a22c-153341060661"), true, "classical studies", "100300" },
                    { new Guid("ad5d3ba7-adc4-48f9-8bd0-4c51869a1789"), true, "American studies", "100316" },
                    { new Guid("ad7e9145-24d7-4f72-9a04-1ffefd3f45f0"), true, "transport planning", "100198" },
                    { new Guid("ad813942-10c1-41e8-87a0-ce14bc09e545"), true, "biomaterials", "101210" },
                    { new Guid("ad9f2bee-1b60-4f87-b0c0-e062dc5a3616"), false, "Human Physiology", "B1001" },
                    { new Guid("ade030c9-2808-40df-aa8a-3781856aa75d"), true, "careers guidance", "100658" },
                    { new Guid("ae12666d-f58b-4b99-9f75-a2ab22e3d3f2"), false, "Latin", "Q6000" },
                    { new Guid("ae3452ca-2a9e-435d-ba7d-c821dab5fd11"), false, "Time,place and Society", "F9024" },
                    { new Guid("ae815a15-5ebe-4932-b04b-2321a4a6ee45"), true, "biogeography", "101352" },
                    { new Guid("ae8c3f08-ebf0-4f68-976b-3b3f771ddaa6"), true, "integrated circuit design", "100553" },
                    { new Guid("af2645e9-c27f-4cc2-bdec-307e791a4cb4"), false, "Religion", "V8010" },
                    { new Guid("af5b01b4-c7fc-4bcf-bfd9-0ef151682040"), true, "computer aided engineering", "100160" },
                    { new Guid("af783163-5ab8-44e5-8d2a-5077fd167c10"), true, "Salman Rushdie studies", "101496" },
                    { new Guid("af8f92f3-c16a-4343-b752-f90481a0c6c6"), true, "Christian studies", "100795" },
                    { new Guid("afa5bf91-0e04-4a4e-ab0f-30507ab6ead9"), true, "space technology", "100116" },
                    { new Guid("afdf8059-99e7-47a6-b6dc-f344756ac6ce"), false, "Health Education", "B9900" },
                    { new Guid("b009b380-8868-48aa-9a43-0b64fc9490f8"), true, "French literature", "101132" },
                    { new Guid("b01a47d5-2738-466a-b5cb-1f900ae7fed3"), false, "English Politics", "M1002" },
                    { new Guid("b0369394-5dde-4ca7-8b9f-cfe8851d73d5"), true, "control systems", "100166" },
                    { new Guid("b075d0e6-13b4-40b4-95cd-226f8b2ca356"), false, "Leisure and Tourism", "N222" },
                    { new Guid("b08fa67b-bcc0-4a41-8898-32d34fdab49b"), true, "Arabic literature", "101432" },
                    { new Guid("b09200df-d8b9-4d28-a439-f525670bd8b0"), true, "drama", "100069" },
                    { new Guid("b0932dea-ee81-45fe-91f5-a6b34a59904d"), true, "modern history", "100310" },
                    { new Guid("b094c5a5-cf52-42c1-a548-84d350ffaab5"), true, "biotechnology", "100134" },
                    { new Guid("b0bad895-85bc-45c4-b7cb-7bc157384051"), true, "Estonian language", "101426" },
                    { new Guid("b0e1417a-1837-4b29-8ce5-f2a5922082cf"), true, "genetics", "100259" },
                    { new Guid("b0ee512e-c08f-4516-b544-b471fc341118"), true, "housing", "100196" },
                    { new Guid("b0f5d0a7-cc3c-4dc6-aeac-d53af7f67e12"), false, "Economics With Social Studs", "L1004" },
                    { new Guid("b10634d5-61e7-453e-b3b2-e36e9934500f"), true, "transpersonal psychology", "101343" },
                    { new Guid("b11d6292-01c0-40b5-b94e-3aea95d2e262"), false, "Communications", "P3002" },
                    { new Guid("b127fcac-23f4-40c4-a7ec-b3570e998874"), false, "Computer Education", "G5400" },
                    { new Guid("b1509c7e-0432-4d12-9f2a-f949b07dcbad"), true, "ethics", "100793" },
                    { new Guid("b1a33f92-1b9f-4fac-8a7a-ba90f9ad174b"), false, "Housing Studies", "N8004" },
                    { new Guid("b1d99dcc-9fd1-4671-919f-97cba4f4f972"), false, "Tefl/Tesl", "Q3008" },
                    { new Guid("b1ddff20-7be7-4711-a2ec-eee463b72bea"), false, "Analytical Sciences", "F9607" },
                    { new Guid("b24fa313-7804-4469-8ab3-51c9986e2803"), true, "clock and watchmaking", "100726" },
                    { new Guid("b2d8ef59-6540-4746-ae2e-1daeb9e37c62"), true, "Chinese society and culture studies", "101167" },
                    { new Guid("b3438597-463c-4630-870d-19f7dfd9a6a5"), true, "construction management", "100151" },
                    { new Guid("b359efbe-c353-4c99-a31d-a17302f90fba"), false, "Political Economy", "L1101" },
                    { new Guid("b383dab6-7c3d-485c-b025-fbec5e5c8bb0"), true, "sociology of law", "101465" },
                    { new Guid("b3912cf9-c309-491d-b672-2cbedaf825b3"), true, "chiropractic", "100241" },
                    { new Guid("b393471a-c055-462a-9a72-9e6c702f5d68"), false, "Arabic", "T6200" },
                    { new Guid("b3a2a73a-258c-4c62-a84d-b32745769072"), false, "Critical & Contextual Studies", "W9921" },
                    { new Guid("b3d1d72b-1a01-4827-ad7d-4c733b2d5f6d"), true, "bacteriology", "100909" },
                    { new Guid("b46c9ebc-b827-4316-a398-314d84a7aa04"), false, "Portuguese Lang, Lit & Cult", "R8850" },
                    { new Guid("b48370f7-b865-4092-8b36-7096bb0fc3f2"), true, "history of design", "100783" },
                    { new Guid("b4dbdf28-2dbc-4926-b949-2899fbbdb04d"), true, "child psychology", "100953" },
                    { new Guid("b4ea95a5-f688-4fc9-a7c6-5b475dbe5670"), false, "Creative,express.Arts(Gen)", "W3005" },
                    { new Guid("b4ebaf34-b0b4-438a-ad63-d497db3267db"), true, "Japanese literature", "101170" },
                    { new Guid("b4edc1a9-02f0-48e5-ad18-e67ef5529c38"), false, "Educational Studies", "X9003" },
                    { new Guid("b500b5d3-2dbe-4248-a824-28e50e93c496"), false, "Education Of The Deaf", "X6002" },
                    { new Guid("b504b26d-45e9-48b2-86d1-ae28acac3f49"), true, "Shelley studies", "101484" },
                    { new Guid("b5932136-7a3c-47fe-8e35-6e16c8bc0b38"), true, "minerals processing", "100212" },
                    { new Guid("b5c0471b-f427-43c7-8568-0a2c38aa1013"), false, "Place and Society", "F9019" },
                    { new Guid("b5d46fde-5942-4c06-ba8c-826f28d30c5b"), false, "Minerals Estate Management", "N1002" },
                    { new Guid("b5e15d02-cb71-4d0c-989d-5b788a08b4c9"), true, "Wilkie Collins studies", "101482" },
                    { new Guid("b60a4d84-170f-4641-b735-71a5cb59561e"), false, "Social and Life Skills", "G3400" },
                    { new Guid("b61b6085-1d98-444d-b716-91da84214704"), false, "Curriculm Development In Schs", "X9008" },
                    { new Guid("b6e8291e-f76a-421b-9209-cee6f06e5c6a"), true, "school nursing", "100293" },
                    { new Guid("b714a270-b444-4286-90f0-2ff14f7de344"), true, "general or integrated engineering", "100184" },
                    { new Guid("b724c63f-204b-44b6-b0be-01634768609a"), false, "Art,design and Technology", "W2409" },
                    { new Guid("b741a185-53df-41f0-ac0d-7503812daf9f"), false, "Education (Other Than Bed UK)", "Z0093" },
                    { new Guid("b7595fa6-66e9-4b41-a000-e0a2f90f26a7"), true, "ethnomusicology and world music", "100674" },
                    { new Guid("b76efc44-6678-45c1-a40a-bbf962850ae2"), false, "Language/Literature", "Q3010" },
                    { new Guid("b77324be-caf5-475b-b069-ee77c3b147ce"), true, "sociology", "100505" },
                    { new Guid("b7cd69d0-8574-4597-bdae-976128798734"), true, "performing arts", "100071" },
                    { new Guid("b7d74a9f-d676-4f58-ad82-e207060d0d7d"), true, "structural mechanics", "100579" },
                    { new Guid("b81981d7-c0ee-4b9a-b876-8c828165f6a2"), true, "comparative religious studies", "100803" },
                    { new Guid("b86a0596-84e0-40fc-a583-e5b1bb4062cc"), true, "music and arts management", "100643" },
                    { new Guid("b8938139-d10c-47e4-a773-f81621a5a4de"), true, "business information technology", "100362" },
                    { new Guid("b8a3fb30-14c7-4c33-a646-9c5daedd026a"), true, "T.S. Eliot studies", "101469" },
                    { new Guid("b8ab13b6-f46b-404b-a849-37004f4ec893"), true, "typography", "100630" },
                    { new Guid("b90bd506-41bb-4466-a00e-3b78a0e15907"), false, "Micro-Computing", "G5005" },
                    { new Guid("b9399863-afd2-404a-9cd1-54b0ca54c273"), false, "Health", "B9904" },
                    { new Guid("b995f724-0440-4702-8959-44d4c38c0cc4"), true, "legal practice", "100692" },
                    { new Guid("b9bf8b25-342d-4fda-a24c-15fac5040265"), true, "atmospheric physics", "101068" },
                    { new Guid("b9ce2aab-8c81-494b-987e-c9331f1e43be"), true, "medical biochemistry", "100352" },
                    { new Guid("ba17b076-2d26-43fc-9699-f3d465eb6822"), false, "Physical Education/Games", "X2016" },
                    { new Guid("ba1d854e-a771-40ea-990d-bddb42106084"), false, "Information Studies", "P2001" },
                    { new Guid("ba2f9096-fe30-423f-b9cb-b5fc36c04361"), false, "Handicraft: Other UK Quals. In Handicraf", "299" },
                    { new Guid("ba4aad2c-d49b-4b9b-ada9-441af75ec314"), true, "polymer chemistry", "101053" },
                    { new Guid("ba7463e2-d78b-4a49-9d11-1cef8c53576d"), true, "Asian studies", "101180" },
                    { new Guid("bb59b1cd-dc87-4a34-b058-a3dea82bd8ac"), true, "urban geography", "100666" },
                    { new Guid("bb7e410b-2a29-40b3-bd0b-a000fcf9f3d3"), true, "community nursing", "100281" },
                    { new Guid("bb7fe797-d547-4598-a706-8d868a5b2a2a"), true, "taxation", "100831" },
                    { new Guid("bb85c7f1-514e-4ccd-8878-3e69e8392fe4"), true, "environmental history", "100670" },
                    { new Guid("bb8e7b5a-f635-4e88-b96d-d10303f954ba"), true, "television production", "100923" },
                    { new Guid("bbae8113-7dfb-4846-9443-b3bd1ee6fef6"), false, "Ceramics Technology", "J3200" },
                    { new Guid("bbbed8b2-85c7-4562-8fba-fc0b283e94fe"), false, "French With German", "R8203" },
                    { new Guid("bc117c81-102f-4b79-b6f3-b5f6bf313f9b"), false, "Property Surveying", "N8003" },
                    { new Guid("bc778b4d-8f6b-4c76-be8e-f0104078c2e3"), true, "endocrinology", "101337" },
                    { new Guid("bd2f5b46-76dd-49ea-8dd8-7a0a6edf5ed3"), true, "computer networks", "100365" },
                    { new Guid("bd4284d6-881f-4fd3-b55e-e237da6276f5"), false, "French and Spanish", "Q9704" },
                    { new Guid("bd6ba6a1-dd99-4d37-a4bd-cf2a42790cf5"), true, "Akkadian language", "101415" },
                    { new Guid("bd8c3173-8bc0-477f-a4d0-9c2034191ce8"), false, "Human Movement and Health Stds", "B9901" },
                    { new Guid("bd9b557c-ccf6-4de7-9a8a-dc92e9733863"), false, "French and German", "R1001" },
                    { new Guid("bdba8802-72c0-468f-8752-538461aa0d7d"), true, "applied chemistry", "101038" },
                    { new Guid("bdc0b764-fdf2-49f8-bd42-82ce39b58e0a"), true, "animal health", "100936" },
                    { new Guid("bde24b87-1127-48e0-8781-a67790cfaa29"), false, "Rural & Environmental Science", "F9020" },
                    { new Guid("be09de32-af0f-4ab9-aef3-3ccf11ac5517"), false, "Literary Studies", "Q2002" },
                    { new Guid("be12f399-6502-4b1e-bbb0-563a6e2d826f"), true, "anaesthesia", "101336" },
                    { new Guid("be1f7bd9-45b0-449c-9d2d-9b03796a5780"), false, "Movement Studies/Science", "F9630" },
                    { new Guid("be477e90-58b9-41cf-9bad-6a8cb6f02afb"), true, "accounting", "100105" },
                    { new Guid("be95ea6e-c145-4eba-a13e-db4ce46e513e"), true, "North American society and culture studies", "101204" },
                    { new Guid("bf13c995-0a10-4a93-81c7-3205da3e27c1"), false, "Language Studies & Philology", "Q8814" },
                    { new Guid("bf237e14-eefd-4df6-862d-cfb8676fd8c6"), false, "Geography: As Physical Sce", "L8880" },
                    { new Guid("bfbb0798-8883-43bd-a939-e7cdce91bfe2"), false, "Mathematical Physics", "F3200" },
                    { new Guid("c0156b00-1ab6-4bbe-9d53-d2e5845723b9"), false, "Fine Art & Textile Design", "W2010" },
                    { new Guid("c03c244d-18fc-482e-808e-d68340e46fe2"), false, "Earth Studies", "F9201" },
                    { new Guid("c04980ab-ecec-4768-9190-352550d9d3d5"), false, "Combined Languages", "ZZ9003" },
                    { new Guid("c0b43a2a-ee16-49c8-866c-a7606303bf3b"), true, "electromechanical engineering", "100192" },
                    { new Guid("c0f9ceab-6821-4f25-a2f5-de645d8929cd"), true, "Spanish studies", "100325" },
                    { new Guid("c0fe602f-7947-4e21-a2c3-5a0f6173e8c5"), true, "public services", "100091" },
                    { new Guid("c1287c1f-f94c-4b6b-8455-6ab4844e6ed9"), false, "P E & Recreation Studies", "X9016" },
                    { new Guid("c16edb28-1e08-42cb-97f6-360d63b0e7db"), false, "Voice Production", "W4006" },
                    { new Guid("c1af3251-14cf-4a2a-9813-ee407aa5256a"), true, "general studies", "101274" },
                    { new Guid("c1d57bc5-5f95-46c1-9480-e54b47053b8e"), true, "humanities", "100314" },
                    { new Guid("c1df4012-55f0-48ba-b0f4-b7d19f78ab29"), true, "fashion design", "100055" },
                    { new Guid("c1f0aa54-34f0-4413-b2fc-de970cc22afd"), false, "Home Management", "N7502" },
                    { new Guid("c23841d6-c106-4237-b2f0-37c7948fb80a"), false, "Classical Civilisation", "Q8200" },
                    { new Guid("c2794c3c-8bb2-4770-af97-693549b5393b"), false, "Art & Media", "W2501" },
                    { new Guid("c28fae23-0941-47c5-a786-64f48c6b51de"), true, "interactive and electronic design", "100636" },
                    { new Guid("c31d8972-3acb-4561-82a9-c7802602d0a0"), true, "neural computing", "100966" },
                    { new Guid("c35f94e5-9870-479d-98be-559c6ece63d3"), true, "Scandinavian history", "101498" },
                    { new Guid("c361eeaa-9201-4463-bb9e-eb1c000d5088"), true, "applied botany", "101376" },
                    { new Guid("c389a18a-8355-4215-9b3c-598d2c24613c"), true, "Nepali language", "101371" },
                    { new Guid("c416fc0c-b976-47d1-853d-27a5600509ab"), true, "European history", "100762" },
                    { new Guid("c45b5cc2-0a39-44d9-ad2e-852e032c10bb"), true, "materials science", "100225" },
                    { new Guid("c47f33ec-bad8-4b1f-b93f-d5ff4cb97a37"), true, "clinical medicine", "100267" },
                    { new Guid("c47f90cc-2677-4c9b-9642-3d1cbaa57551"), false, "Child Developement", "X9002" },
                    { new Guid("c4985fa2-4f47-4d4b-ad04-e62c41528f69"), true, "South Asian history", "100772" },
                    { new Guid("c4e6c135-9bd1-450c-9b83-26aaa98b4f3c"), true, "jurisprudence", "100691" },
                    { new Guid("c4f93439-2c82-41c0-92b5-2b6b7837b367"), false, "Environmental Technologies", "K3400" },
                    { new Guid("c51568f3-30dc-445a-8b58-d322ebf7ec45"), true, "comparative politics", "100618" },
                    { new Guid("c5271e45-4ec0-49a1-9e38-291bee29c595"), true, "Indian literature studies", "101430" },
                    { new Guid("c53973ed-e05e-4bfa-b3b3-bc1031c481e9"), true, "optoelectronic engineering", "100169" },
                    { new Guid("c56f6875-4575-4518-bd70-fa64bbc0b0b1"), true, "sports development", "100096" },
                    { new Guid("c5b96d63-e4b1-4083-8179-7ab9eeb5cdee"), true, "modern Hebrew language", "101269" },
                    { new Guid("c5c21379-212c-4299-bd59-22b894c535f5"), false, "Studies In Technology", "W9907" },
                    { new Guid("c5d2f92b-dcbc-4688-acb5-21e0229497d4"), true, "applied music and musicianship", "101450" },
                    { new Guid("c5e49420-9dad-43d2-82d8-99a0fc1aa901"), false, "Mathematical Science", "G1500" },
                    { new Guid("c5ea4c27-88ee-4705-8001-2b24866b09b9"), true, "Thomas Hardy studies", "101470" },
                    { new Guid("c63056cf-3af9-4c5f-a121-13c0f78c312d"), false, "Biology and Science", "C9705" },
                    { new Guid("c6dc4d6d-e6e6-4da9-8dd3-aa63a23a6394"), true, "organometallic chemistry", "101389" },
                    { new Guid("c6def189-29db-4752-8a04-0665e2022c30"), true, "baking technology management", "101021" },
                    { new Guid("c6f1935a-5bcc-4623-a9fc-714a52f28b87"), true, "ophthalmology", "100261" },
                    { new Guid("c766a01a-f6a2-4072-966e-cb14dd605a15"), true, "salon management", "100896" },
                    { new Guid("c7f4b2c1-62f5-45b2-9bdb-1afe5d1e4ce7"), true, "Spanish literature", "101139" },
                    { new Guid("c7f8bae8-792d-47f4-9367-9d72db7fdb91"), true, "gemmology", "100550" },
                    { new Guid("c8012ead-438f-4d75-99f8-c1b9bc0c58ea"), true, "electrical power", "100581" },
                    { new Guid("c806d146-4e64-4d96-a054-c959d232ec47"), true, "quaternary studies", "101091" },
                    { new Guid("c84b5d7e-5b1e-4d12-b24a-6cf426bf3090"), true, "glass crafts", "100724" },
                    { new Guid("c8b2d5a7-01a8-4b29-ae53-b5ff2d4a5ac9"), true, "professional practice in education", "101246" },
                    { new Guid("c8d7188a-2b91-4344-96a5-c9656ff65bc8"), true, "chemistry", "100417" },
                    { new Guid("c8db0527-5be2-40fd-b74b-26aeae0c29be"), true, "institutional management", "100815" },
                    { new Guid("c9018d7e-91e2-4834-bccd-13ab863f978f"), true, "aquaculture", "100976" },
                    { new Guid("c93002a8-46aa-4318-86ba-cb1ace5f0d7f"), false, "Env. Stud (Geog Hist Science)", "L8205" },
                    { new Guid("c9c395c7-dbd1-4a88-bef3-fdc7517e53d6"), true, "business and management", "100078" },
                    { new Guid("c9eea828-f4f6-442b-b4ab-4c768c9fbe08"), false, "Multi-Cultural Education", "X6007" },
                    { new Guid("ca0661f7-c906-469f-9e07-03611b63a732"), true, "brewing", "101022" },
                    { new Guid("ca2c8c0d-f51e-41f8-80e3-5b71e07bff09"), false, "Science & Environ Studies", "F9614" },
                    { new Guid("ca449a7c-7512-43bc-862a-6f1d838cd8d5"), true, "environmental impact assessment", "100549" },
                    { new Guid("ca4e606c-90b3-4a61-bf57-20f828e58fe2"), true, "music composition", "100695" },
                    { new Guid("ca74f0a9-1509-4cb5-bd7e-bc8a54f8f84f"), true, "physiotherapy", "100252" },
                    { new Guid("caa10698-4485-475a-9e87-c537fd8e4cc1"), true, "medicine", "100271" },
                    { new Guid("cab1f7ab-b05c-4bd3-95a2-801ccf1a51f3"), false, "Outdoor Education", "X2004" },
                    { new Guid("cad2a545-70fd-43d2-b3c9-98e334554912"), true, "financial reporting", "100845" },
                    { new Guid("caea9ab2-4db7-4ef9-a5a5-8dc1b0be9b8e"), true, "e-business", "100738" },
                    { new Guid("caeb3f43-056a-4b56-b94f-581f0696919c"), true, "architectural design", "100583" },
                    { new Guid("cb3e50c0-40a9-4b4e-9705-6f2008f56592"), true, "public health engineering", "100565" },
                    { new Guid("cb427f97-4d24-4cb6-86b9-86601c5b695b"), true, "sports management", "100097" },
                    { new Guid("cb4505df-5542-4907-9019-038b339fbc79"), true, "beauty therapy", "100739" },
                    { new Guid("cb992416-0831-47ca-8776-41edb36464a6"), true, "critical care nursing", "100282" },
                    { new Guid("cb9c27bf-2fb1-40c6-bde0-783a65f19c75"), true, "audiology", "100257" },
                    { new Guid("cbdc1c75-88f0-46b5-93eb-aca938af5ba0"), true, "industrial chemistry", "101041" },
                    { new Guid("cbe6d563-eae4-4c9d-b891-a02295fadd3e"), true, "creative computing", "100368" },
                    { new Guid("cbfcc9d2-7d8b-4e6b-83c9-e708ff39574f"), true, "medicinal chemistry", "100420" },
                    { new Guid("cc4495cf-5ad0-45db-a54d-9f15e3e00a9d"), false, "Natural Environmental Science", "F9003" },
                    { new Guid("cce515d7-6e1b-403a-aee7-bd6a8684b891"), true, "opera", "101448" },
                    { new Guid("cd1bdfbf-0690-4dd4-93c4-17fb286344d0"), true, "evolutionary psychology", "101345" },
                    { new Guid("cd3637d6-0101-4506-8af0-148373a58bda"), false, "Land and Property Management", "N8000" },
                    { new Guid("cd3af8db-16de-4d40-9413-9edbbd632198"), true, "psychobiology", "101344" },
                    { new Guid("cdddf2a8-627c-4a16-8ce9-22a8e7771798"), true, "specialist teaching", "101085" },
                    { new Guid("ce15e970-f0e1-45b1-9628-fa5763dc19bf"), true, "Jane Austen studies", "101478" },
                    { new Guid("ce4a64c8-c238-42c1-8c59-c48ff7f46d9c"), true, "energy engineering", "100175" },
                    { new Guid("ce8302b7-508b-435b-95ed-451613e03a96"), true, "transport geography", "100669" },
                    { new Guid("cf08921b-e8f8-4b20-9a93-34a9224cc5f5"), true, "biomedical sciences", "100265" },
                    { new Guid("cf22fc43-3c60-4800-b742-d42dacccb33d"), true, "marine engineering", "100544" },
                    { new Guid("cf397d0a-66ee-40e5-a670-64156f1fb572"), true, "youth and community work", "100466" },
                    { new Guid("cf9f4ee7-1a37-48d9-8e83-4dbc75a18a0e"), true, "Persian society and culture studies", "101503" },
                    { new Guid("cfcf7ae5-3212-4ab3-a041-79d130b89112"), true, "Welsh studies", "100335" },
                    { new Guid("cfe56b8b-9460-43fb-9596-f5508d92d6f2"), false, "Mathematics and Science", "G9005" },
                    { new Guid("d005f440-b514-4fab-9143-5e21b45de47d"), true, "religion in society", "100626" },
                    { new Guid("d006549b-a1ec-4bd7-83d2-1c70bfc46c47"), true, "bioelectronics", "101216" },
                    { new Guid("d02b6e5c-5881-403f-a017-6d8ad9982d5c"), true, "textiles technology", "100214" },
                    { new Guid("d0690fdf-edbd-40bc-8eda-f320f645f05f"), true, "counselling", "100495" },
                    { new Guid("d08fddbb-996a-42de-b978-8e44b3fc3c64"), true, "travel management", "100102" },
                    { new Guid("d0b8e759-b2b5-4f51-affb-b768b2c26026"), false, "Personal and Social Education", "ZZ9008" },
                    { new Guid("d0c61f60-1c94-4c7c-a7a1-6ecb9639e516"), false, "Japanese", "T4000" },
                    { new Guid("d0cbbab4-7449-4a90-a7f6-b2caa02616f7"), true, "technical stage management", "100704" },
                    { new Guid("d0f0178e-8d9b-4fc9-9726-4ca51d2657d7"), true, "physical sciences", "100424" },
                    { new Guid("d0f6ed5c-cccf-4fd5-9c88-e4f60c42677b"), true, "business economics", "100449" },
                    { new Guid("d0f820e4-17a6-4a63-a626-b9dd0f660da8"), true, "Scots law", "100678" },
                    { new Guid("d13d8378-641b-487c-b5a2-afa8b218a655"), true, "hospitality management", "100084" },
                    { new Guid("d167eca9-1b6a-49bf-a1fb-c6b397a8823e"), false, "Biology With Core Science", "C9701" },
                    { new Guid("d188c3a7-499e-4cdd-b4ed-a34e6c0ce08f"), false, "Maths With Computer Science", "G9003" },
                    { new Guid("d1fcfb05-ebe0-471d-ae42-7ec550bc2769"), true, "developmental biology", "100834" },
                    { new Guid("d217b5ec-2e3c-4a6c-90a4-a0f9d4776da9"), true, "agriculture", "100517" },
                    { new Guid("d21cd1d1-4b97-4114-9163-d4df6ae35ee5"), true, "Brazilian studies", "101143" },
                    { new Guid("d23f7737-8ecd-4897-a8c5-c90fb6255c8d"), true, "information management", "100370" },
                    { new Guid("d2554651-ecca-4991-b4ca-62f3a5290807"), true, "osteopathy", "100243" },
                    { new Guid("d272a428-a91e-44c3-bdf2-15e2f51ef5c8"), false, "Arts-General (Where Subject Not Spec)", "Z0079" },
                    { new Guid("d2b7f2cc-3d16-4afa-9e4a-b0bd62fdc187"), true, "archives and records management", "100915" },
                    { new Guid("d2f4140b-f3dc-4eef-946b-a9b909c8917f"), false, "Drama & Theatre Studies", "W4403" },
                    { new Guid("d30bb32c-07c2-4668-b257-f58d9dacbfb8"), true, "German society and culture", "101135" },
                    { new Guid("d30f867e-3767-479a-b4b6-caa6ef587033"), true, "gas engineering", "100176" },
                    { new Guid("d313d5fb-154d-4ffc-b012-408aa374b42d"), true, "information technology", "100372" },
                    { new Guid("d31e3eb4-4ea8-4881-b7c4-dc990bc207a0"), true, "machine learning", "100992" },
                    { new Guid("d33d5ac6-fec4-4ee6-bec9-78f70f63580b"), true, "coaching psychology", "101294" },
                    { new Guid("d34e49fd-01be-4f8e-8aca-e3be7bf356d3"), false, "Spanish (And Studies)", "Z0043" },
                    { new Guid("d35f3818-6992-4d32-be2c-c42c6fff61f8"), true, "childhood studies", "100456" },
                    { new Guid("d3b0d79b-4741-428b-824d-e98dd5d8d3c5"), true, "fine art conservation", "100599" },
                    { new Guid("d403121c-79f6-4d3a-9088-149b0558e878"), true, "virology", "100910" },
                    { new Guid("d40c6393-f3a4-4282-9ec6-b912c1e56d7d"), true, "Chinese languages", "101165" },
                    { new Guid("d40d8390-8abb-4109-a025-b81e9bc2a41c"), false, "Human Development", "L7202" },
                    { new Guid("d49391e7-b1c0-4f5b-98b8-abc5513d15e8"), true, "leadership", "100088" },
                    { new Guid("d4a6eff9-fbad-4f0c-9a2b-ae8a6362c794"), false, "French Lang, Lit & Cult", "R8810" },
                    { new Guid("d4ed37cd-f314-4be1-a49a-9920389d8d20"), false, "Asian Languages", "T5016" },
                    { new Guid("d4f0b29c-59db-4948-908a-f3dd4eb5a91f"), true, "W. B. Yeats studies", "101494" },
                    { new Guid("d526c198-94dd-45b7-91b1-3c118c991072"), true, "timber engineering", "101013" },
                    { new Guid("d554c863-efe9-4aa6-9777-d3aa0d30fc76"), true, "genetic engineering", "101378" },
                    { new Guid("d56187ae-6c84-4854-bd2c-c62d47bdc185"), true, "spa and water-based therapies", "101375" },
                    { new Guid("d564a982-5196-4ad8-819d-a8ea17e85614"), false, "Science-Chemistry-Bath Ude", "F1004" },
                    { new Guid("d6612991-e472-4c52-910a-c2ff1111d326"), true, "African languages", "101185" },
                    { new Guid("d6642c9e-617d-44df-bd10-cc6f25600279"), true, "Christopher Marlowe studies", "101487" },
                    { new Guid("d6916503-d9da-4fdc-855f-b76c48dca98d"), false, "Hispanic", "R4001" },
                    { new Guid("d6c65afa-98c8-4bfe-939a-a3b2723483f9"), false, "Spanish Language & Studies", "R4101" },
                    { new Guid("d75bd09b-4bad-48c3-8080-5c4c3e98298f"), false, "Japanese Lang, Lit & Cult", "T8840" },
                    { new Guid("d7733b81-5436-44fe-9507-bf3b69789fdb"), false, "Contemporary Studies", "V9000" },
                    { new Guid("d7768f73-c0c7-4c16-a225-2a64c5575f26"), true, "population genetics", "100902" },
                    { new Guid("d7871045-0a1e-47fa-af33-ca0a252f06fe"), true, "statistics", "100406" },
                    { new Guid("d791d361-7477-498e-af46-6e1acf764418"), false, "Child Sexual Abuse child Sexu", "X9006" },
                    { new Guid("d81173c0-a593-4780-b913-9a9a75713d1c"), true, "knowledge and information systems", "100963" },
                    { new Guid("d833e4a3-d668-4a46-96a8-885faccbf38a"), true, "photonics and optical physics", "101075" },
                    { new Guid("d8936d39-17bc-4d18-984d-b9d06771755c"), false, "Physical Education With Dance", "X2005" },
                    { new Guid("d8e8516a-70cf-4f07-b8c1-fb1e0a34b4f2"), false, "Turkish", "T6800" },
                    { new Guid("d904bcf8-bd2c-499f-ab29-ed8ad11c048a"), false, "Business Studies & Info Tech", "N9703" },
                    { new Guid("d9489cd4-c99a-4619-84f5-c802546e8c2f"), false, "General Humanities", "V8890" },
                    { new Guid("d9505e72-082a-4772-97a9-4a1f060d2364"), true, "German language", "100323" },
                    { new Guid("d96d6983-7dab-44e4-8df7-ab4e90432a43"), true, "change management", "100813" },
                    { new Guid("d9c30724-4010-42dc-91e5-7f5293b57ce7"), true, "building technology", "100584" },
                    { new Guid("d9c38734-2b4d-45e2-bda4-bca3a44cf6f0"), false, "Design (C.D.T.)", "W2406" },
                    { new Guid("da5fc0bc-2ae9-4311-ad51-27d374c6b63e"), true, "Arabic languages", "101192" },
                    { new Guid("da70c3cc-2649-4d80-84f8-904aa033a478"), true, "internet technologies", "100373" },
                    { new Guid("da8723e5-e152-4255-8b17-935794b396f8"), false, "General Studies In Science", "Y1000" },
                    { new Guid("da8aaa46-2434-454d-b5a6-d052f37f85ed"), true, "psychology of communication", "101341" },
                    { new Guid("da9e69c6-bafe-43c9-ac7b-325521a17dc7"), true, "visual and audio effects", "100717" },
                    { new Guid("daaddafb-9e5b-407f-bbb8-ae64b3e77d32"), true, "freshwater biology", "100849" },
                    { new Guid("daba812b-6b8d-4985-8e44-ffe68a678c2e"), true, "Russian history", "100766" },
                    { new Guid("dac12299-4bc1-4043-8a91-829f51b2b4ec"), true, "audit management", "100840" },
                    { new Guid("dad1f2f0-26f0-40ff-9880-69869471f600"), false, "Drama and Education", "W9915" },
                    { new Guid("db700fd7-0464-4588-aed6-bd815d3abef5"), true, "English literature", "100319" },
                    { new Guid("db853097-40ad-4eec-942b-70ca70ed7a93"), true, "international social policy", "100645" },
                    { new Guid("dbe03c07-c7c4-4b2e-b799-67a62e4e9b6a"), false, "Design and Technology Ed", "H8703" },
                    { new Guid("dbf19c97-0aa5-4a00-a04a-e082b21b4da2"), false, "Analysis Of Science and Tech.", "F9604" },
                    { new Guid("dc40d307-15ac-49a5-8f83-9b0cc00cccdc"), false, "Env Studies (History & Geog)", "F9025" },
                    { new Guid("dc4f30ce-1f4c-4006-ba7c-9af5b6f1a6b7"), true, "midwifery", "100288" },
                    { new Guid("dc55af5e-cb8f-434c-bf04-7b1abbe7c084"), true, "plant biotechnology", "100139" },
                    { new Guid("dc8ab290-b5d3-4c0a-94b1-5775fc5af443"), true, "rail vehicle engineering", "101398" },
                    { new Guid("dc964b4a-3102-446b-be6d-27b6406bce49"), true, "the Qur'an and Islamic texts", "101445" },
                    { new Guid("dc977c5f-110e-4d31-a91f-8eb3a72e07c6"), true, "acoustics and vibration", "100580" },
                    { new Guid("dd303be9-b768-4f96-b0ef-62ee0ee05189"), true, "ancient history", "100298" },
                    { new Guid("dd4ab15d-8181-4745-9fef-cd40a7db2595"), true, "occupational therapy", "100249" },
                    { new Guid("dd51e366-6b2e-4df7-8a87-f6f882985cd9"), true, "phonetics", "100972" },
                    { new Guid("dd74d98d-279e-4d89-aeef-dba8242b1730"), false, "Fine Arts", "W1000" },
                    { new Guid("dd8326c9-1a45-4c8d-a324-c2c6f8ffd9e8"), true, "health risk", "101049" },
                    { new Guid("ddfc9379-0cd2-4d8f-8d60-9de421a6c5ae"), true, "English as a second language", "101109" },
                    { new Guid("de0ed51e-81ef-4d2a-b00f-a5494b320490"), true, "pastoral studies", "100802" },
                    { new Guid("de42b10d-c9d1-4a89-8e0f-64f53bbf9487"), false, "Primary Curriculum", "X9005" },
                    { new Guid("de552b6c-448e-4d7e-b158-8b087a380983"), false, "Human Movement Studies", "W4503" },
                    { new Guid("de6c0a92-1dde-45b7-8eee-4f6e9d8bc2e7"), true, "mechanical engineering", "100190" },
                    { new Guid("de87c813-c7fb-43ba-a4cc-ca0c73e75a29"), true, "UK government/parliamentary studies", "100610" },
                    { new Guid("de8a5425-60d9-43ee-a99a-1af8ea724d3d"), false, "Biology Botany", "Z0023" },
                    { new Guid("df00483a-ab33-4bb5-9ad6-f49bba14eb86"), false, "Architectural Studies", "K1003" },
                    { new Guid("df012151-2665-4b3a-b0e0-2e06b3c3ba8d"), true, "hair and make-up", "100706" },
                    { new Guid("df0ad2cc-2384-4a1e-9665-d6a5a017b9c9"), true, "graphic arts", "100060" },
                    { new Guid("df8ed884-d0a2-4ae1-8449-1c24c0134f75"), true, "media and communication studies", "100444" },
                    { new Guid("df9507ea-c998-4a39-86ff-eade84e8165f"), false, "Building Studies", "K2001" },
                    { new Guid("dfaa8799-02cc-425f-b77d-78fa16f83cbb"), true, "mental philosophy", "100791" },
                    { new Guid("dff9472f-9e0e-4194-9bfd-f1a3b47b1352"), true, "international politics", "100489" },
                    { new Guid("e03a0e07-f459-4632-8091-d476961c2924"), true, "anatomy", "100264" },
                    { new Guid("e03f20da-f100-4653-845d-5988fe9426c4"), false, "Theatre Arts", "W4402" },
                    { new Guid("e0445402-2733-4aab-aa8e-1d96a03b10b9"), false, "Information Science", "P2000" },
                    { new Guid("e07d21a3-fb20-44b6-8ad2-59dc555f3093"), false, "Computer Studies", "G5000" },
                    { new Guid("e0aea476-b488-4c6c-9dd1-83e18223b100"), true, "public accountancy", "100837" },
                    { new Guid("e0aef8be-fc21-4a5e-9304-dbe38a6e0055"), true, "ocean sciences", "100421" },
                    { new Guid("e0b8b9fa-8b02-4252-bed8-0b7f6f68df1d"), false, "Biological Science", "C1200" },
                    { new Guid("e0edc443-05cb-46ed-b3b6-02cd4ed01c48"), true, "English studies", "100320" },
                    { new Guid("e0ee6c0b-d81d-400f-8a12-9dc4eb6309a8"), false, "Computer Educ With Science", "G5401" },
                    { new Guid("e0f053bf-392b-42f6-aea9-60238c1c40f4"), true, "numerical analysis", "101027" },
                    { new Guid("e1090d4a-9c19-442c-bd8b-4f668a5346c4"), true, "illustration", "100062" },
                    { new Guid("e1148d89-8e02-495a-9d6c-aeb15c397c20"), true, "epistemology", "101442" },
                    { new Guid("e12ba69b-a002-4515-9e49-2c9b862efb88"), false, "English With Drama", "W4003" },
                    { new Guid("e15a93b7-5296-4233-8855-68d9a8a56551"), false, "Speech Training", "B9504" },
                    { new Guid("e1858c75-659d-4542-9068-aeba745fb72a"), true, "sacred music", "100844" },
                    { new Guid("e1a90632-070b-4d64-b02b-8452b667ca9f"), true, "exotic plants and crops", "101348" },
                    { new Guid("e1c6a94f-3e3e-4c7a-81b7-06e59fd0522c"), true, "construction", "100149" },
                    { new Guid("e20dbb12-143a-4f12-bef4-e60b0782ca70"), true, "public law", "100684" },
                    { new Guid("e235aa54-7c9a-4fa3-8e02-93d488b3f395"), true, "south Slavonic languages", "101428" },
                    { new Guid("e2c85faa-12d5-4bc2-814c-47a18bcef5aa"), false, "Literature & Media Studies", "P4600" },
                    { new Guid("e2d2c5a6-ea7f-47f9-aacd-37777f6eadef"), false, "Education Of The Disadvantaged", "X6004" },
                    { new Guid("e2e11be9-30ab-4674-b2a5-9e8a202003c6"), false, "Edn.Of Childn.With Sp.Needs", "X6401" },
                    { new Guid("e2eea4f0-5351-4ac0-a1fc-208944fa06bf"), true, "financial management", "100832" },
                    { new Guid("e3217a84-07a5-46d0-9f71-d88925c96a91"), true, "psycholinguistics", "101035" },
                    { new Guid("e36116d4-7dcb-4156-a262-ed5841db46fa"), false, "Language & Literacy", "Q1404" },
                    { new Guid("e3ec5822-212e-43a3-ac71-980258883a4e"), true, "anarchism", "101404" },
                    { new Guid("e3ee0872-032e-43a5-a653-050406f50d78"), true, "the Torah and Judaic texts", "101446" },
                    { new Guid("e3ef48af-7a82-4cd8-a7bf-3f15302179ad"), true, "surveying", "100219" },
                    { new Guid("e43d7f10-5ffb-4f73-9e94-e96df292c24e"), false, "Social Biology", "C1900" },
                    { new Guid("e476bedc-e9a0-4712-a9dd-fbbddcdb68e9"), false, "Physical Education and Dance", "X2017" },
                    { new Guid("e485602f-75f3-483b-a031-f98d40dc3992"), true, "therapeutic imaging", "100132" },
                    { new Guid("e4c2c604-ed14-401f-a6d6-27e54a870f8d"), false, "Business and Management Studies", "N8810" },
                    { new Guid("e4de772e-08be-4568-92b4-0c6b8c414bc6"), false, "Mfl(French, Spanish, German)", "Q9707" },
                    { new Guid("e5185420-9f8e-4c2a-b64e-075e95c74ae1"), true, "geochemistry", "101083" },
                    { new Guid("e5407292-d96b-4370-abbd-6b0cc7126904"), true, "creative writing", "100046" },
                    { new Guid("e54e98b3-21c2-45f5-b771-727162ff46b7"), true, "oil and gas chemistry", "101054" },
                    { new Guid("e5687a38-03cf-42da-b605-f1f989fe47d5"), true, "object-oriented programming", "100960" },
                    { new Guid("e5b6f308-b651-4a0f-a1f1-a0672dba004f"), true, "organic farming", "101004" },
                    { new Guid("e60edb18-4647-41bd-a999-9a8d7a27ddfc"), true, "criminology", "100484" },
                    { new Guid("e65e88af-9a57-45c0-8ed4-06c86be83b05"), true, "physical chemistry", "101050" },
                    { new Guid("e6759210-0976-48a5-9e17-7d87c77f0a4e"), false, "Combd Science With Intens Phys", "Y1003" },
                    { new Guid("e678940a-8ef9-420b-aa7e-4675f9924d7a"), true, "nursing", "100290" },
                    { new Guid("e7796247-305f-43d3-8e4f-8ed4ed5f3707"), true, "actuarial science", "100106" },
                    { new Guid("e807718b-ccf8-4591-86b5-9210b45a9cf3"), false, "Theological Studies", "V8005" },
                    { new Guid("e850fc06-9ab9-47ef-b1ac-f8cd71aee48e"), false, "French Studies (In Translation)", "R1100" },
                    { new Guid("e8573983-9da2-4de7-905c-ed6e99171156"), true, "sport and exercise psychology", "100499" },
                    { new Guid("e87b5dff-9713-4ce6-8a3b-bbda20bf36c9"), true, "Byron studies", "101483" },
                    { new Guid("e894f3fa-6f5c-44c9-8be1-b69880a91087"), false, "Health and Movement", "B9902" },
                    { new Guid("e89f6e45-73b2-4edc-a0b9-b5cc33bf03b7"), false, "Home Science", "N7503" },
                    { new Guid("e8a2e040-b79c-497d-b06a-c398d274d805"), false, "Language", "Q1403" },
                    { new Guid("e8ca0225-809f-4e97-a345-5ccb589ebccd"), false, "Tech:design and Technology", "W2506" },
                    { new Guid("e8da5a30-bd90-40a1-a08c-3924989970db"), true, "countryside management", "100468" },
                    { new Guid("e906cefa-7869-4c9d-a031-c9d0e057e330"), true, "bioinformatics", "100869" },
                    { new Guid("e949b371-bd92-41c6-8b2b-496cc2ee6f39"), true, "English history", "100761" },
                    { new Guid("e9568024-ca71-48b4-9df1-0d7d7da77b47"), true, "poetry writing", "100730" },
                    { new Guid("e9752e40-4933-45ea-94c4-d430e505eb79"), true, "psychology", "100497" },
                    { new Guid("e9bb45be-c962-4898-b85e-78e505beb3aa"), true, "clothing production", "100109" },
                    { new Guid("e9db1d2c-c092-4b96-959c-e6e49b3bf685"), false, "Technological Mathematics", "G5008" },
                    { new Guid("e9e124f8-4172-4dbc-a361-92514ce7edc7"), false, "Policy Making", "N1208" },
                    { new Guid("ea060f51-ec47-4f86-8d2b-16d0fb4f779d"), true, "early years teaching", "100510" },
                    { new Guid("ea1f1f16-b89d-4360-81de-dcdba78a10d1"), false, "Expressive Arts", "W9003" },
                    { new Guid("ea8b79de-7593-4ff8-93db-96dc03498c0e"), false, "Arts and Physical Education", "W1009" },
                    { new Guid("eab76fae-9f79-46c7-b45c-4fe045e2074d"), true, "Arab society and culture studies", "101198" },
                    { new Guid("eaeba795-c510-4905-afca-4fa71b2fda23"), true, "computing and information technology", "100367" },
                    { new Guid("eaed7f23-49c8-4a00-bf64-68b8255e49d5"), true, "acupuncture", "100233" },
                    { new Guid("eaef7076-30e4-4284-89fc-aa08b0ccf41a"), true, "journalism", "100442" },
                    { new Guid("eb162764-b8f3-4315-9d64-635d2bdc1959"), true, "African history", "101360" },
                    { new Guid("eb65cb20-1345-4c92-a318-63f394503a52"), true, "applied environmental sciences", "101078" },
                    { new Guid("eb79a44e-cd6e-4960-abda-a881e0851304"), false, "Personal, Social and Moral Ed", "L8206" },
                    { new Guid("eba29847-22e9-4e6b-8cf7-3919971aaec6"), true, "Walter Scott studies", "101488" },
                    { new Guid("ebb0c244-5a35-4a63-a3b7-d878648d774c"), false, "Co-Ordinated Sciences", "F9618" },
                    { new Guid("ebc3de0a-8a5e-4c35-8a55-53f12cd4110d"), false, "Ancient Greek", "Q7001" },
                    { new Guid("ebeadfa0-96d0-46ac-a6c6-015b2c37531c"), true, "Joseph Conrad studies", "101495" },
                    { new Guid("ec17389d-4769-418b-a580-309f63b86abe"), true, "analogue circuit engineering", "101399" },
                    { new Guid("ec394be2-d98d-469d-a775-095ada1fadfd"), true, "organisational development", "100814" },
                    { new Guid("ec58dc4b-e68c-4978-9193-d399c0bc9d7c"), true, "Philip Larkin studies", "101475" },
                    { new Guid("ec8763d0-b9b3-4105-8009-0c5a12d114d0"), true, "strategic studies", "100616" },
                    { new Guid("ecb7ec68-3a57-4174-8c33-201d7b535f45"), false, "Catering & Institutional Management", "N8870" },
                    { new Guid("ece5432a-5102-466c-b17d-1e13f3d2a6c2"), true, "farm management", "100978" },
                    { new Guid("ece6efe0-9e8f-4a24-910a-3f7de2c3d606"), true, "shorthand and shorthand transcription", "101409" },
                    { new Guid("ed0911a4-c826-4336-a02a-9b84f25496d6"), false, "Music & Instrumental Teaching", "W9928" },
                    { new Guid("ed0e81ec-819f-419a-addc-ac25fdb9b4db"), true, "painting", "100589" },
                    { new Guid("ed23419a-1083-4d82-a58e-87e370cdd556"), true, "building surveying", "100216" },
                    { new Guid("ed247101-f5e0-4d4b-a731-1c45857886de"), true, "Samuel Beckett studies", "101481" },
                    { new Guid("ed8d7fff-a9af-4fba-90e7-2f9e7ffe3a43"), true, "gender studies", "100621" },
                    { new Guid("ed969adf-13ba-4d70-99d7-fcbb28a29fe7"), false, "Art and Design Studies", "W9923" },
                    { new Guid("edf17c48-b549-441e-87e4-08507e0961f8"), false, "Metals Technology", "J2001" },
                    { new Guid("ee464a46-6110-4639-b36e-8211a642618f"), true, "cybernetics", "101355" },
                    { new Guid("ee571a97-0208-4d35-a555-799ec1bbcd4a"), true, "agricultural economics", "100600" },
                    { new Guid("ee6fb4fd-5e7b-44db-a13f-662881177d78"), false, "Modern Literature", "Q2004" },
                    { new Guid("ee82f812-1f99-4b75-85bb-ae8403ebdf60"), true, "public international law", "100681" },
                    { new Guid("eec6ab17-135c-4053-ad5a-1defbbaf98cc"), false, "Integrated Physical Sciences", "F9606" },
                    { new Guid("ef282e34-12b1-416a-960d-882038d985f9"), true, "intellectual history", "100781" },
                    { new Guid("ef43f5b9-514e-4f23-8c3b-3dcf8842d5c4"), true, "European business studies", "100808" },
                    { new Guid("ef4ab814-d8d5-4ddf-b959-28cc8ddc8c75"), true, "forensic archaeology", "101219" },
                    { new Guid("ef4ca8c1-d5b9-45d8-a0a2-5e23870472cc"), false, "Environmental Issues", "F9612" },
                    { new Guid("ef5a08f1-06e8-493e-83a4-30ff51386132"), false, "Art Education", "X9000" },
                    { new Guid("ef89f5fc-4b03-4d29-95cc-91b7d2c5245d"), true, "landscape architecture and design", "100124" },
                    { new Guid("f047ad8b-5a1d-4593-b7c1-2fd5f956d4de"), true, "social theory", "100628" },
                    { new Guid("f07c735b-5cf5-48b8-94a9-c08f66888996"), true, "business computing", "100360" },
                    { new Guid("f0cb3e5b-2e19-4831-bb17-fcf0e823e7f9"), false, "Outdoor Education Studies", "X9013" },
                    { new Guid("f0df8572-edcc-498b-9d70-f8572d9c38dd"), true, "cinematography", "100716" },
                    { new Guid("f0e90200-11d9-4d22-a754-c698193f6ef8"), true, "zoology", "100356" },
                    { new Guid("f0eab488-e363-4323-995f-66f0326a2268"), false, "Three Dimensional Design", "W2408" },
                    { new Guid("f1116e13-0874-4326-816a-511fd77054f0"), true, "criminal law", "100685" },
                    { new Guid("f11b4e59-1fe5-4456-9b40-4184719df21d"), true, "exploration geophysics", "101084" },
                    { new Guid("f124603a-2278-4ccf-a4a6-461226d5657d"), false, "Pedology", "D9002" },
                    { new Guid("f135879a-5ca7-49e8-bc17-2433ce6278ff"), true, "development in the Americas", "101359" },
                    { new Guid("f143c67f-be3b-42c6-ac46-7df141adb5c7"), true, "insurance", "100830" },
                    { new Guid("f19a9050-00c4-4367-a447-70f927c6f0c6"), false, "Practical Theology", "V8006" },
                    { new Guid("f1b68d8d-2655-4bd3-a52e-2dc688c4db9e"), true, "textile design", "100051" },
                    { new Guid("f1cc0730-3a1e-4ea3-9237-90ed6a4ebc82"), true, "Brontës studies", "101473" },
                    { new Guid("f1efb698-4cd8-4664-af7f-c0d06fef4779"), false, "Greek Studies", "V1008" },
                    { new Guid("f205833e-389b-4f4d-8aed-0417fa4004d4"), true, "visual communication", "100632" },
                    { new Guid("f2065003-0b31-4e8a-aea3-851af15e0f1d"), true, "leather technology", "100210" },
                    { new Guid("f223dd10-8ef7-4278-bbe3-dc6cd99986a7"), true, "carpentry and joinery", "101505" },
                    { new Guid("f23449d0-38c9-4c35-8af9-c8543976aed3"), true, "quality management", "100213" },
                    { new Guid("f23cb04d-8a5e-4b98-a620-ed37367e2d88"), true, "systems analysis and design", "100753" },
                    { new Guid("f24805ed-2f25-43b7-8c96-12cbd465df94"), true, "marine technology", "100194" },
                    { new Guid("f25fc247-0efa-46e3-8e70-14d5b6192322"), false, "Physics With Technology", "F6007" },
                    { new Guid("f26da82b-6236-4f3e-9812-0d4a0bdaaec5"), true, "human resource management", "100085" },
                    { new Guid("f28e0b68-7eff-4685-8e6f-d66bf242aa37"), true, "childhood and youth studies", "100455" },
                    { new Guid("f2d770c9-5854-474d-a240-5b3f633b82bb"), true, "maintenance engineering", "100193" },
                    { new Guid("f302411a-8f80-4b2c-9450-c29c0e69accb"), true, "USA history", "100768" },
                    { new Guid("f305d9a6-04af-40f5-becd-7bc5cab5bc4f"), true, "property valuation and auctioneering", "100825" },
                    { new Guid("f318d464-ee13-4b24-b3a1-c8c382ac6a6f"), true, "Chinese studies", "101164" },
                    { new Guid("f331ad21-e6b4-4925-a72d-300daf3e3f7e"), false, "Speech & Drama", "W4600" },
                    { new Guid("f3572d73-a2e8-4b7e-beb2-f1e2d1b59c69"), true, "immunology", "100911" },
                    { new Guid("f358a7ba-aade-4840-9e74-b1bc5753f570"), true, "ancient Middle Eastern languages", "101112" },
                    { new Guid("f35b6b01-4b43-481c-844d-c99d69df45f6"), false, "Craft & Technology", "W2405" },
                    { new Guid("f36b3d42-2f37-46dc-8f34-ac8e43cb83eb"), true, "nutrition", "100247" },
                    { new Guid("f3709dba-1031-42a7-939c-d8122b184c0d"), true, "agricultural irrigation and drainage", "101385" },
                    { new Guid("f385e867-e3fc-4bb9-9165-90fceb4e5c10"), true, "medical physics", "100419" },
                    { new Guid("f3921185-d2e0-4187-ab02-a80dbe5eb010"), true, "instrumental or vocal performance", "100639" },
                    { new Guid("f394e7c3-8cf4-4069-aaa1-2bc7cb4a0feb"), false, "Expressive Arts (Art & Design)", "W2006" },
                    { new Guid("f395edab-b225-4a69-a63e-2d323c8fc233"), false, "Science (Unspecified)", "F9603" },
                    { new Guid("f39b4fd5-6b5a-41f9-a376-c5b382def9a7"), false, "Drama and Spoken Language", "W4011" },
                    { new Guid("f39c191d-310f-47f5-9096-da9d404cdc01"), true, "pharmaceutical chemistry", "100423" },
                    { new Guid("f3a072d3-28f6-420a-8bfe-dd28194323ad"), true, "crime scene investigation", "101222" },
                    { new Guid("f3a70137-9dc2-427f-a3a8-8f303db6d799"), true, "victimology", "101405" },
                    { new Guid("f3d33379-798b-4805-bd47-dfdcd21b4876"), true, "risk management", "101040" },
                    { new Guid("f415b417-eb73-40dc-94a8-c6e6db1b3041"), true, "agricultural botany", "101025" },
                    { new Guid("f4261555-e189-4145-a386-d1b48bed1dce"), true, "animal behaviour", "100522" },
                    { new Guid("f42b34b9-e6e6-4da5-aadf-c7f21aa91b2f"), true, "electronic music", "100867" },
                    { new Guid("f436098c-d507-42e7-9e47-2125eb1e232f"), true, "land management", "100819" },
                    { new Guid("f4508692-a6fb-437f-a5f2-b369f4efc55f"), true, "dance performance", "100712" },
                    { new Guid("f495f25d-39c5-49fe-8382-c81ea52416d8"), false, "Movement Studies", "W4504" },
                    { new Guid("f4a62944-3672-4ea8-8a1a-e4a88079b03e"), true, "biology", "100346" },
                    { new Guid("f4b2b856-79db-4391-bef6-2c2c20064db9"), false, "Tech (Design & Info. Tech)", "W9901" },
                    { new Guid("f4c8804d-3cdd-4467-9cc3-c33e319579f4"), true, "jazz", "100843" },
                    { new Guid("f4e86f46-52fb-4029-9ea7-8f757ecb9e89"), true, "emergency and disaster technologies", "100186" },
                    { new Guid("f4e9f12b-7a7c-486a-bf1f-d9bfbcaf55c9"), true, "mental health nursing", "100287" },
                    { new Guid("f4fb8e30-75a5-478f-8562-99794366a5dd"), true, "heritage studies", "100805" },
                    { new Guid("f508fd2e-eea1-4360-ad42-3c5a29b8eb0c"), false, "Information Tech'ogy:computing", "G5604" },
                    { new Guid("f514142e-d9be-40ef-a2af-76377125c9d2"), true, "occupational psychology", "100950" },
                    { new Guid("f549136d-72e5-4597-a594-5449672c8dde"), true, "Turkish languages", "101431" },
                    { new Guid("f54b936d-a93d-46d2-9458-d7ce381707a0"), false, "Ed Of The Deaf & Part. Hearing", "X6003" },
                    { new Guid("f57675b4-6f0e-4de2-94eb-d1754d67aa59"), true, "Hausa language", "101367" },
                    { new Guid("f593283c-66d2-48dc-8e13-7f0fc76c2686"), true, "orthopaedics", "101324" },
                    { new Guid("f5b2282e-e887-49d8-a9b8-38a030e662eb"), true, "silversmithing and goldsmithing", "100725" },
                    { new Guid("f604d521-76cb-4bf5-bc6f-1c828a7ad4bd"), true, "oncology", "101327" },
                    { new Guid("f6107b8a-b2c4-43e7-9212-5bf7913bb590"), true, "Robert Burns studies", "101490" },
                    { new Guid("f629e740-71c4-4ea7-a0eb-3aa01dd911f3"), true, "business studies", "100079" },
                    { new Guid("f6462c63-b140-44ff-a1fb-ab94c55d1da5"), true, "fire safety engineering", "100183" },
                    { new Guid("f6ec2749-41f2-4d85-a3ea-c5f0fb877a49"), true, "special needs teaching", "101087" },
                    { new Guid("f7033e45-b877-4cc9-ab81-00be6d83f568"), true, "hinduism", "101444" },
                    { new Guid("f7168e43-3705-41a1-b27b-182b876f5e48"), true, "Dutch studies", "101161" },
                    { new Guid("f71d2954-affe-4a39-b3da-f9b50ab58af6"), false, "C D T With Computer Science", "W9909" },
                    { new Guid("f748042f-1d33-4f5c-b0eb-bec5f94f0925"), true, "classical church Greek", "101422" },
                    { new Guid("f7b525ce-ee6f-422d-9005-6f686c4c83d2"), false, "Remedial Education", "X6400" },
                    { new Guid("f81ff0e0-3f39-48a8-bb70-6f6cefac57d8"), true, "health and welfare", "100653" },
                    { new Guid("f82e97f6-b2e8-427a-a18c-81e781cc03b3"), true, "composite materials", "101217" },
                    { new Guid("f8415194-0d45-48cd-b194-bc950f02ee6b"), true, "liberal arts", "100065" },
                    { new Guid("f896d8e4-7cb0-467f-b6ad-62daf8a45379"), true, "bioengineering", "101243" },
                    { new Guid("f89a7c97-f5a1-4fc6-af19-9e76b9cabfa1"), true, "educational psychology", "100496" },
                    { new Guid("f8c14f34-6d49-4a63-b5c6-589a10f349f9"), true, "conservation of buildings", "100585" },
                    { new Guid("f8db3950-de0e-44bc-9ce6-2dd27a17ceb8"), true, "marine chemistry", "101046" },
                    { new Guid("f90c044f-e0f7-4d74-adf0-d49a67d20e98"), false, "English, Drama, Media Studies", "Q9715" },
                    { new Guid("f94d3fe0-7619-4c5f-9b44-3bd3a25b3d2c"), true, "epidemiology", "101335" },
                    { new Guid("f9d879dc-6308-4c4e-9d45-7bed52ba8b7f"), true, "crafts", "100895" },
                    { new Guid("f9da902c-5e21-475e-9247-d740ada178fc"), true, "hair and beauty sciences", "101373" },
                    { new Guid("f9fcb607-1012-4a2c-9c14-2542bbc435e2"), false, "Creative Arts", "W9001" },
                    { new Guid("fa2bbcc3-4020-4518-82e1-fea9b664e810"), false, "Art and Music", "W2800" },
                    { new Guid("fa51c1b0-dc30-42c5-8213-9ddaeaff3e95"), true, "hydrology", "101079" },
                    { new Guid("fa58d436-c996-4f44-b4ec-4af1c580ed8d"), true, "sports studies", "100098" },
                    { new Guid("fa7dda66-691e-455f-9cee-e269daa4b726"), true, "water resource management", "100986" },
                    { new Guid("fa804c1c-7c4d-443f-8e3c-abf263c6c62a"), true, "animal management", "100518" },
                    { new Guid("fae7e7d0-5a8a-409d-979c-869dec518f0a"), true, "higher education teaching", "100509" },
                    { new Guid("fae959a2-b028-4ba8-b05c-754e25798b2b"), true, "film studies", "100058" },
                    { new Guid("fb1419d5-bac9-462f-9317-56e41239d5f0"), false, "Technology", "J9001" },
                    { new Guid("fb609ef8-b33a-4cf1-897e-95b3b512e51a"), true, "religious studies", "100339" },
                    { new Guid("fb69e684-ca64-4d1f-941c-198c896afe14"), true, "Latin literature", "101125" },
                    { new Guid("fba00902-86ed-4574-ac73-dbae282c67e6"), true, "early childhood studies", "100457" },
                    { new Guid("fbb5e2fe-b11b-46cf-a173-1e28ba95a9ec"), true, "Ruskin studies", "101467" },
                    { new Guid("fbeb8e3d-df5e-4620-8d22-68606f8fbce2"), true, "computational physics", "101071" },
                    { new Guid("fbee8811-9c44-4e0a-8f6b-d324cc337bb0"), false, "Welsh and Other Celtic Lang", "Z0046" },
                    { new Guid("fc044c98-d459-4f9e-98e6-9d1bcad5546d"), true, "reflexology", "100239" },
                    { new Guid("fc75352d-d09e-4a31-9e2c-46681cb151e7"), true, "Czech studies", "101312" },
                    { new Guid("fc930191-3a70-4ebc-8364-b31d72960634"), false, "French With Italian", "R8202" },
                    { new Guid("fcc67ce9-d2f7-4ee9-bce4-b12f3a3b175c"), false, "Design and Craft", "W9902" },
                    { new Guid("fceb10d4-2df2-420b-bc6d-03bbbd0fa971"), true, "physical and biological anthropology", "100663" },
                    { new Guid("fd090339-2970-4c3d-bcd0-3e3f3d02c95e"), false, "Human Studies", "L3401" },
                    { new Guid("fd1477b8-5152-4800-a9ff-2dc1a0ead5bb"), true, "obstetrics and gynaecology", "101309" },
                    { new Guid("fd14d677-32d3-491e-8e9c-7d0860d3b00b"), true, "ergonomics", "100052" },
                    { new Guid("fd1b4b8b-ca2b-44d4-aa12-7f54db8cb24d"), true, "biodiversity conservation", "101318" },
                    { new Guid("fd44ffd8-b003-4fd1-ad70-b1dd58691efe"), true, "English law", "100676" },
                    { new Guid("fd45a0ac-3a7d-46c9-baf7-b1789e371c48"), true, "history of science", "100307" },
                    { new Guid("fd48f74f-41a6-460a-8334-de1ba115de4a"), true, "microbiology", "100353" },
                    { new Guid("fd64b17f-adb0-4377-b525-bfb4d1aff8cc"), false, "Cinema & Film Studio Work", "W5300" },
                    { new Guid("fd7f0d93-3fa3-4acd-938e-e182e8722909"), true, "mechatronics and robotics", "100170" },
                    { new Guid("fe0ee47e-f87b-4651-9b05-5b3611fc9b40"), true, "surface decoration", "100728" },
                    { new Guid("fe84424c-f233-414b-a0f3-4d22c72c02fa"), true, "reproductive biology", "100847" },
                    { new Guid("ff3f23f1-d237-4028-8723-29d7c1e8f489"), true, "English literature 1200 - 1700", "101094" },
                    { new Guid("ff481306-7d5b-4122-b46b-c6fee34abc43"), true, "property law", "100689" },
                    { new Guid("ff8b2f4d-fa71-48d5-a648-3e1474aabfb2"), true, "cinematics", "101214" },
                    { new Guid("ffb6663f-7034-4592-a149-adf0d5d49ee5"), true, "fabrication", "100211" }
                });            
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"ALTER PUBLICATION {DqtReportingService.TrsDbPublicationName} SET TABLE qualifications, tps_establishments, tps_employments, establishments, persons, alerts, alert_types, alert_categories, events, training_providers;");

            migrationBuilder.DropTable(
                name: "degree_types");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "AD");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "AE");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "AF");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "AG");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "AI");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "AL");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "AM");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "AO");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "AR");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "AT");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "AU");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "AZ");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "BA");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "BAT");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "BB");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "BD");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "BE");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "BF");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "BG");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "BH");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "BI");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "BJ");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "BM");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "BN");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "BO");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "BR");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "BS");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "BT");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "BW");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "BY");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "BZ");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "CA");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "CD");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "CF");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "CG");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "CH");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "CI");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "CL");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "CM");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "CN");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "CO");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "CR");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "CU");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "CV");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "CY");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "CZ");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "DE");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "DJ");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "DK");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "DM");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "DO");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "DZ");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "EC");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "EE");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "EG");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "ER");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "ES");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "ET");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "FI");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "FJ");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "FK");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "FM");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "FR");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "GA");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "GB");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "GB-CYM");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "GB-ENG");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "GB-NIR");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "GB-SCT");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "GB-WLS");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "GD");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "GE");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "GG");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "GH");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "GI");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "GM");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "GN");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "GQ");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "GR");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "GS");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "GT");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "GW");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "GY");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "HN");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "HR");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "HT");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "HU");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "ID");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "IE");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "IL");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "IM");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "IN");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "IO");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "IQ");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "IR");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "IS");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "IT");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "JE");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "JM");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "JO");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "JP");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "KE");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "KG");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "KH");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "KI");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "KM");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "KN");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "KP");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "KR");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "KW");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "KY");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "KZ");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "LA");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "LB");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "LC");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "LI");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "LK");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "LR");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "LS");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "LT");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "LU");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "LV");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "LY");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "MA");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "MC");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "MD");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "ME");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "MG");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "MH");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "MK");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "ML");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "MM");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "MN");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "MR");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "MS");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "MT");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "MU");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "MV");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "MW");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "MX");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "MY");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "MZ");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "NA");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "NE");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "NG");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "NI");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "NL");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "NO");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "NP");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "NR");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "NZ");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "OM");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "PA");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "PE");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "PG");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "PH");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "PK");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "PL");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "PN");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "PT");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "PW");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "PY");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "QA");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "RO");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "RS");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "RU");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "RW");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "SA");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "SB");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "SC");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "SD");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "SE");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "SG");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "SH");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "SI");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "SK");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "SL");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "SM");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "SN");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "SO");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "SR");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "SS");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "ST");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "SV");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "SY");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "SZ");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "TC");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "TD");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "TG");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "TH");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "TJ");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "TL");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "TM");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "TN");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "TO");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "TR");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "TT");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "TV");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "TZ");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "UA");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "UG");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "US");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "UY");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "UZ");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "VA");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "VC");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "VE");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "VG");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "VN");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "VU");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "WS");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "XK");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "XQZ");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "XXD");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "YE");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "ZA");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "ZM");

            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "ZW");

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("002bf98f-fd1e-422d-a951-1cd4dd29d4ce"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("00447627-36a3-42c1-9336-5cc4d24e46d3"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("005ceb13-0881-4ed0-bb19-08d49c3763a0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("006fa254-d985-4f43-82bf-54d49c4fa91c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("008ff140-767e-46c0-ac32-85292e33da8f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("00996bd5-f2f5-4423-bb44-162efb24acb8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("00b2d1d0-628a-4a1d-943a-3e317e9cf45c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("00bd6592-db8b-4eea-89b7-be0922576aa0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("01561d4b-5dd5-45ae-9989-5a0b713251da"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("015d862e-2aed-49df-9e5f-d17b0d426972"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("0177cdf3-d1e8-4db5-8c44-6421a0f013ce"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("01a3070e-8d75-43ed-a6c7-14bbe0a8b42b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("01fdac1e-d370-4a0e-a390-4df05436c839"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("02b41511-fa57-4c46-9597-6b2d3a8b74d3"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("02e05cd6-1962-46fb-8f5e-8ef2ac23d162"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("03358e6a-b8af-4fb0-b83c-dcffb5578e31"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("033d952b-4f47-47f6-a4c8-f11b30d8b763"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("03497a81-2f36-4eb2-aaab-5dbe501b6d98"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("0367b82f-8cfd-420a-bf8a-71dbf878b72e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("036aedb3-4173-44f3-97ab-0eaba86e03b7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("0416a4f2-4a3a-40ee-847a-bdaa5a0727f2"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("041e55de-d9de-45aa-8710-7bec63db7e13"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("042590b0-ce6e-4024-981c-e0bc85af3ea7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("04284cc5-681f-4545-9918-5e3e67196b4a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("0477d8a5-ccbb-47bd-a86f-e1405128fc08"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("049a7a5d-d3ac-415b-85df-58e82b2dcb5f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("04e8008c-0e2e-4888-80ef-2ffb5f1e5ab4"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("053003f2-8ce0-4943-b0b1-99baf4fd0239"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("05352933-c094-4582-8015-91019dae260e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("05389dd4-79f3-484c-ad6e-2b09a3c80947"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("058782be-f887-4851-bb16-18d3620eacdd"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("0636913d-783f-4d6d-89cf-f8c3f6f2e0f7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("0670dc56-acf9-4531-b3e9-fa9472833586"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("06fa8277-7809-421d-8a24-f3ab8130149b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("07b4718e-18be-4d1d-beed-731630fa5c27"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("07b68d5f-8182-4a3c-8392-d6387875be1c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("07d698d1-4ee3-47ce-8b25-70a1684e0abd"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("07d95576-5741-46c8-b3dd-8dd3877e07fe"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("07e8e170-5b8d-4efc-9416-f32300bce270"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("080ae67b-901a-4fa4-a36c-d911ee2a581e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("08711a13-26f5-4c60-8de8-8aec7b9691b5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("08736a66-ecbb-4679-9f0e-93cdfae37fb0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("08966417-8e15-4ac3-8ae5-182a516b38c7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("08a39573-be7d-465c-9fb7-de4436dbb393"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("08b5901c-2b50-45d3-8503-b10259f77f13"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("09483d19-dfe8-4e08-abe7-e9de379efb42"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("097175c6-113d-4ffa-b25c-2f68e46f993b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("097620e0-eb23-4fe8-be42-97c4aad5bc08"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("09819c9d-9588-4f69-b71e-020dfebbd0fa"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("09ab681b-966a-4c0d-9bc4-e97a0811312d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("09b16d6c-03aa-4c5c-9303-79c270420bf5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("09d47234-0d31-4356-881f-b3f4db01af7f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("09db8708-b306-4495-b183-7161d5efd5ec"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("0a08ee0b-2bf7-44e1-8fff-72ca8619a3c1"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("0a4a7bcb-7f61-4ba8-af8c-b4df02898d29"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("0a5e8fa8-507e-4434-8004-b221a5adcf0e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("0b174977-18ba-4d6e-ae85-4601cf297b8a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("0b34aa64-241b-45dd-bd91-664f66dbd0f3"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("0b353b77-d02d-4e5e-b1d0-f963f27d4ed4"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("0b609275-68df-4077-8a03-04031603d6a1"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("0b765f82-8223-49e5-9f69-ed89b7adba68"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("0ba8308c-fc2e-4f05-97f1-dc99d34f6c31"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("0bae8523-c607-4345-9cbb-d7284d0e0d14"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("0bc8b844-e96c-4fca-b73b-3c0d3615fb3f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("0bcaf443-de1d-460a-ba5c-f11baff3bc79"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("0bce3dad-3452-4e8b-8232-158a71544698"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("0c1d6a94-ca55-4b23-91e3-a4f64f8e2115"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("0c267114-e372-437d-9933-446b3cd4fd02"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("0c9fa4b9-5a50-4842-bde4-3d13f0a103a1"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("0d29d5fc-242b-4f0b-b16c-6c69bbd94bff"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("0d813255-b8ae-4686-b694-35ff085a4d7c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("0dada6be-3bf3-49b7-8c97-d00f486371c8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("0e21bc65-ceac-4af3-88d7-88798cac7c5b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("0e3352a8-44b7-4963-8ffc-03daeb9223a2"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("0f11491d-d1d3-4aa4-9ff9-e905ae6825ae"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("0f3c8673-8bf6-4774-8eea-7063899f6dbb"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("0f68664e-ba97-454d-ab56-0349579cf647"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("0fa6cf72-549d-424e-affa-52cb6114ffb3"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("0fbdda8a-cb18-4a37-af1b-ca651de0d1dc"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("0fbec27a-048e-41fd-a14f-da97ef67626f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("100d2adc-c1da-4ea3-a04b-700551d9e587"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("100fb49b-1b22-4973-8547-7eafc0014715"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("101d29db-2e4b-4a8d-9dbe-3ba97886713a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("102e59ef-4574-4185-916a-a42b60cd6fef"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("10624120-7343-4f49-9a04-57a25bca422f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1137e97c-4f30-401c-a32b-946ff6e77813"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("115600e6-88cf-420d-b084-6d11531e1a2e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("11b1bafe-d26c-466c-b058-420aaf45490f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("11f926e8-e3bc-43c1-bc8e-e029d3711f81"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("122a823b-810d-4f62-9143-6a1b8a0f2275"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("12828dec-1ebc-4310-a2b3-3e1d622e79ea"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("128d8774-b7ad-486a-9ff5-4a91c295d2b5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("130113b9-4100-44da-a17e-3c67158685e1"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("132a4b3f-69c7-41cd-9527-b8e7c6d39ec9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1349b1af-8631-4477-ac30-900675ca7688"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("134f93b0-463f-447e-8336-36fab9ef2834"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1377b304-0103-4f4e-ad71-6ff55c7cae46"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1389bdcc-6bba-45e2-bac9-a1e954239124"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("139ca859-dc3e-4d74-a404-eaa27fcc7bdf"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("13bd3303-d7d6-49e4-aad7-e38469e1d598"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("13de25f8-fc36-4154-b751-d0ab535c662a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1418a886-7be4-4de9-b58c-213f7a8017b1"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("146aeb34-7a39-449f-abc3-61a4b4cdd4f2"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("149c7347-0ea9-4424-aacc-21b54e68479e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("14d25849-7c42-42ff-8447-16e940cda458"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("14fb4100-daad-43a3-bf9d-7cc177f5727c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("15048d02-d96c-44a5-be08-d2d1254ccf35"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("15a03936-e4ec-4942-8e1e-40675ba4c10a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("15b11065-d6cf-4fd5-a4ee-8f30adefbf0f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("15ba6775-9cd7-4213-986b-42bda9de0f37"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("15c8a0ef-b13d-4bf4-8e81-1e25b232d99d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("15d6e452-fe60-4306-b477-bbc523e5ffd7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("163a9ae9-28d0-44ad-a1c3-2fad5b884069"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1692ab8b-1e2c-4526-a784-c7565d0452da"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("16a152b8-214b-4f46-91f2-fbfe04a9dc97"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1708895b-2a60-4a86-8ead-be84dce2ab42"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1717c366-c869-444d-a1db-e387b564432a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("174f8921-6ce5-4c97-b154-05478409b7c0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("176547b7-970b-4320-b3a1-50f500ecbeea"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("176f4491-75d6-40db-a57d-d6ad8179506c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("178c0d7f-5583-4f40-a8f6-e5f3d1533b06"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("179c9c1b-b059-4ba8-992b-f434f19fe368"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("17a83eeb-f022-4cdf-9142-3e818b02cb7d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("180fd8ff-a74f-4c37-b200-3775a0098601"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("18897954-105e-4ff7-8c10-831ee70b7072"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("18b3d2e8-755a-40aa-ac72-279660dbb60f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("18be8c05-cdbc-4cb9-aebe-62b4ac4d6a77"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("18c20e91-b116-47db-8003-2d446a368119"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("18cdb3e7-3e4f-40bb-8bd5-91708e465eac"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("18d039ca-7602-40d0-9e73-4a9d3a082b4e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("18e14dbe-0d5f-480f-8d14-a852e7cac8a5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("18e55e1e-b040-4f85-8d7b-e9bb246852bb"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("192027d5-8de0-479d-aa6f-4660fa668dba"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("192c1cc6-9dd9-44a4-b3fa-13857c2bdd39"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("19428eee-01f0-43ac-9eea-2e5363befe81"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("198d6afd-b83b-4ef2-b7f2-6cabcd53fce0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("199589d3-6196-4c4b-a70e-eba0d37ce656"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("19b439e5-b05f-4d02-8b91-2af1f5cdd112"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("19b578fb-58e4-4251-b08e-acb5d28cd00b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("19b95194-9ced-40c1-a2cb-77f8e84a044f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("19e2cb94-2215-4a66-8dde-d7c2e812ccbd"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1a2bffd0-afac-43b6-99e2-826b4405c02d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1a386301-427f-4644-b38c-178c442ea10f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1a4b33dc-7741-4d94-a8f0-125802434990"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1a7283b8-f30f-4a3c-af42-37fad6698e4a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1a9917a9-ca2b-4a9f-9036-58fe19d3a82e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1ad52ba0-46a2-4435-8fa8-9b750c5e2f9e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1b14b772-1562-48c1-a65e-62d4bf8f9033"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1b28203f-061b-4bc1-8e77-13ce64f663b8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1bbe2c87-e3b4-444e-a4b8-375cf0d8aa2b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1bbea5b6-97ce-4d20-9ab1-75d271e8c093"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1bcd45c8-003a-424a-960e-e554f7970882"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1bd7237d-6f15-4624-99d2-768a3a19fe79"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1be1bdfa-31a6-45cf-b79b-7be417611777"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1be8bb34-4ebd-4dea-be6e-7777bc122632"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1c295d1e-d9b4-42ac-99b4-9c66c292acf1"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1c3682e5-9249-4419-b4c5-22dda62ed042"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1c3831cf-3676-46fd-b754-ef9d02d2134c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1c6c703c-9587-443f-9395-411221cc105b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1c81fe60-8cd8-4a11-ab9c-27968bf29a0a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1caaeaad-b11b-47e4-99ba-0c64035a4785"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1d0d6c47-a8af-490c-a497-1d52de98e693"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1d13074c-2bac-471d-89ac-d8036701b081"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1d158b01-1f96-4748-a7b1-1d37f7384581"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1d169844-189b-4f79-9bef-6f226ee3be71"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1d18a3f5-35cf-4ff1-8fd2-7c9788155052"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1d24c528-17be-4133-83b3-a09745452c7e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1d25bcc9-252b-46d8-b9c0-6423a4114604"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1d28a7a9-eb0b-42f2-aac0-2fa19a2945b3"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1d3de640-4bc9-4209-a4a0-810f7a4ba525"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1d5bf739-15e3-41c8-b79a-d34a9b33286e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1d6b39fc-78ad-4225-b3e8-81a7e90c1411"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1da79a08-8ff9-4b45-8cfa-b9920181de29"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1dcbec92-e99d-4c7f-95ce-03e5b9a9eabc"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1df31e53-28fe-4958-8a96-6695295aec2d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1e1bfca4-e469-47f5-99f7-2abfc80b5204"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1e454430-1007-4e1f-b63a-0d9f81eea52f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1e48812c-f676-424d-b221-759bfeb59df5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1f51429e-aea1-49ef-b2a1-8ab0d32de4fd"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1f599880-87cd-4198-9ee7-e38c93aa7791"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1fa54f3d-066b-46cd-9e49-cad7a87551df"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1fb57fe9-6d2f-44e7-a5e5-ca46744800c9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1fd99ccf-1bc7-43b1-aa0d-dfa5a857c57f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1febf25d-e304-4bbf-af6d-90ae5496e118"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1ff6d080-a941-4bcb-bc4c-9b2066bd3354"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1ffe77b3-028f-46c0-a3e5-7b67b94265de"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("200588a9-01bc-4ce2-918a-10243041ce05"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("207e1c41-80b2-47d1-8e71-4125f81f7ce4"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("20974aa7-f47a-450c-8758-255012f981ee"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("20c2b60c-c0cc-4fc7-9f4d-cccfb2c09c6a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("210baa89-bb8a-46bb-8930-dbccee4648f6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("21230c26-2e89-4e55-97fb-627c9f5df4fe"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2135e569-c745-40b0-a2b3-e0a97a87f2f7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2157c540-7f1f-49ee-a306-5811e2d2c1de"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("217a0cf0-5de0-430b-8521-44e1b410d5cc"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("21cf21a0-b58a-41c2-ae13-3ff7fef9031b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("21ebe2d3-a365-4aec-a9f1-1ef35695fc94"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("21ee43a3-3128-49b6-b0b0-2acc0df4c36b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("220c233a-b047-46ed-b2d8-13b982d04796"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2215bf19-10e0-47fc-aa90-117fc2d977aa"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("22373981-7b6d-4256-b092-d30e08689b0f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("22431326-ab5d-4b4b-9b83-905b7ae19fad"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("224bc083-b345-48ef-bb96-bf150cfeeab9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("227674ea-f3df-4b41-9c5f-30a599105527"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("228002ca-78e1-4669-bdae-b4fa2e9c7a6b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2284a258-fe2b-4c3d-b808-639559f0fd5c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("229e90fb-51ec-44ba-8043-e55d49b65fd3"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("22a47345-9a09-4fca-a421-b5ad4427e2d6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("22ac0094-fbe3-44ef-a07e-cb55470932d6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("22f75afc-f4f1-4304-85e5-118a8c26edad"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("234301a0-132a-4590-92f2-6bcec408ed09"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("234a27eb-0b3a-451e-8184-ae61d6ed85c4"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("23567693-5534-4e72-b409-e87e895e45eb"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("239efa09-2bb7-4082-b5a6-fa88a44b4cc1"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("23a85c45-da70-4694-99aa-2c1842e27afe"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("24337f10-3ace-4d64-9c03-dc833d5060f1"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2450d889-9e85-4ace-a600-d3fe738c7df5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2462fb70-abb7-469f-9044-d877fe67ee88"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("249f7719-a095-4f06-9cd3-f40a750e7895"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("24afc417-713b-4fcc-93a4-9d6d115bae9d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("24b884c4-e71c-4918-9cab-a0c958b3f698"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("24b8dd7c-dc76-4bb4-a698-a085cb55e208"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("24be80d3-5dcb-46ac-aa2a-fa128a135084"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("24bf7977-91bc-41ee-b714-9927fb755e18"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("24ee14c9-c5d6-4bee-a1d7-33d2178571af"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2504524a-1939-42af-81d2-aa38a697b3b9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("253ecffe-40f6-4bdb-8b12-9db37be36d97"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("25486642-ed99-49ef-af00-4f3884a3873f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2570cafb-7723-4f79-92f2-3749fdea7042"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("25c61157-6b92-4bbf-a4ed-a7ea668f6a4c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("25c67747-998b-4298-9ad3-c9de4e2250d4"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("25d3aa62-c0e8-40df-a301-7a68b2800fa2"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("263ad86f-e308-4694-889b-37fbd201c381"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("26993aae-c4c2-452a-858c-4becd04ef568"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("26c6261a-8d84-462e-867c-840a3d254c90"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("26dc3264-29c7-47fc-9cbe-bcd920851dd3"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("26e9d312-2b21-4088-a89d-ab88fad7a000"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2734f3b1-0704-4f2d-8728-756b4e2cd211"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("274a3014-f40a-4aaa-b83e-2d7206044d6f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("278e9486-f2a5-41b3-ae3f-2dd87116ae96"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("279f4e4f-a9c9-4715-9d03-92d94f077db6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("27b8aa4b-f440-4ee1-9f67-3922c874bb68"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("27bfb349-fd7d-449d-be97-d5be5ff136b2"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("27e29434-3179-464d-a202-c00c44ae9af3"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("27ee2e03-b3cb-4626-8fd1-55d86b05303c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("28188a3f-0a2c-44f2-abe2-e673d76e9e90"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("28310ae2-c47e-496a-a76e-8c2892f663ae"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("28b0ba22-90c9-46d8-bd0d-b053053af3ac"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("28b80ac2-1abf-4ae1-8331-f943be0b5604"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("28c5407c-1378-4de0-ba60-cdc8c54b2850"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("28c563cd-2af9-443c-a56e-aa2c0a131953"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("28fcd605-8929-4ea9-9ddc-1fee09487e64"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2907cf28-9074-4036-a61e-34820a95fda6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2925db2f-c371-4829-8590-4326bc8efa44"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("29a80bbd-7c38-45d7-a0fb-e66dc6533f1d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("29acfe2e-0518-4dd4-8292-aec30c66aab6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("29b26c51-b494-4f61-b3d0-04a32837a5cc"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("29babaa8-fe64-46e9-90de-6ca0e87ccc7d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("29c237c6-cde2-4036-b535-2c6a9566fb17"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("29c7ee52-35b7-4283-acc9-daaf0628e596"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("29fbaaaf-3324-4d98-9294-156e47e89a29"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2a12c052-e652-4f5b-82c5-54ed9e338bc6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2a42adb4-6937-4b4b-a5f7-94539c1dcb2c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2a4f0c02-0752-4eb3-a930-23a2ba23a597"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2a5cd404-d58e-4dd4-b8b2-bc94d62f7c22"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2a62ac14-5ca9-4994-875f-cfb8cccc86e8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2b179d38-e596-4794-b67b-3dc3bb2669f7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2b26be9b-0cb9-4a90-ad98-6f03c238cdaf"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2b3281d0-600d-47e9-a839-39bae90b88c1"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2b3aa4f9-c961-43d0-aa4c-e905c6a65ed4"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2b8070c7-2068-4934-b818-555d5ea6d1e1"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2b92b588-f93c-43c9-a8a7-b59a8d0763d5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2b9bbd90-4d71-4a01-a48a-4135fdb81434"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2b9be9ad-20ad-450d-8679-fc5927e5ec4f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2bb0e5eb-feba-499a-81ef-da38a601f722"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2bbadac6-8ba7-45a0-9f04-531402cfc0c4"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2bc97e50-d388-42bf-a7c6-9141391a52f6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2bd3f2f1-d575-48ca-bc1f-76f931206344"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2be7776d-3807-4411-8aa6-b2c2a12445a5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2bf3754c-289b-457f-b6e2-00360b77266d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2bff5596-4179-432c-8957-e603ae8c05c5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2c360bba-7777-4c41-a427-9b18c6018379"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2c4a75db-5561-4c40-8412-1097db5a398f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2c531d1d-5245-463e-acfe-02f61d117000"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2c5f8ac1-b53f-4934-bc9d-3f99f7eef7cc"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2caa82a3-839f-417a-b8c5-0585f27ef9c2"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2cb6598f-d5e9-4ae2-9956-fa4a9b2c4144"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2cd80a11-888a-4753-ab2b-9dfbb22c660c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2cd9a0dd-0170-4599-93e6-2fde2b6d67a2"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2d4ff954-31ff-4fd8-9f34-2031db3e2257"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2d5afbb0-e482-4096-835c-c6792e9e69aa"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2d79a348-30e2-45bd-aecc-5e26632a059b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2d980b0a-5952-4721-ab83-16616d5c973e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2dbb43fa-74eb-4e15-95bc-ef8b5c2821dc"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2dc59122-9f7d-480a-8dd5-ded3f1159ca0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2dd5b4ac-9916-4926-8815-e92a291b171b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2de6b266-34d9-494a-b791-7783ccc30af4"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2e15b175-5b8a-4962-90fa-7fc0f1cabcf6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2e30b36e-31b7-4076-888e-1aaba600f20c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2e345641-bc2b-486b-9445-12369e3c1614"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2e52f992-1f2f-47c3-a7c2-a306ede8912d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2e603ced-1727-4f90-97c8-02fae338438d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2e646bcd-a058-4f3b-b365-bdfbed04c26a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2e646f90-5d75-4e60-9488-bf92cf2bf71a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2e9608ae-1f1e-4ed8-a112-1d4a7735b988"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2e9a1a96-3e0a-4b81-81c7-f6e352d04093"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2ed9c4eb-3c98-4c43-baa4-3820b0ad1b92"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2f3e4ce8-2f13-477c-8926-da703aee5a2f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2f5a7879-d9e4-4eb9-8b25-42b0fb956799"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2f711074-58b6-4adf-9476-d20e65fb3b10"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2fc5da28-052b-4024-8a6e-5b4437061039"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2fca5663-efce-441e-a27b-7965ca027e83"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3005b86b-422d-4754-a2ef-e9cc23726c0f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("302c3189-a93a-4e1e-bc7f-9b0132ea1e6d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("30d48719-a814-4754-8af6-1a774ecde2db"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("32267037-19d9-4bd9-8819-6caa93531139"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("32771ba9-0091-4d11-afc8-3f9302c41c74"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("32d38308-1410-462b-a54e-8864bab2afce"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("32daed5f-4d3e-4258-a995-2f4feabd3223"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3339922b-63a6-4b9e-90bd-8c6dd581ec1f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3347ecf7-5cdf-44d2-b54d-f7a9e423e635"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3354f48b-2324-47f2-a222-21c1eba171f8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("336c0a90-dcc2-4812-aabb-b4ba0fb08d0d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("336c1b58-f68a-465b-a4bf-fcdf28e56f91"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3377f679-bf3d-43a1-a87c-e7a894410869"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("33cd2078-bad8-4bbf-8e11-732f3f557f8e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("33edfadf-4ca1-47fa-a4bc-d61a8ebd6caa"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("33fcf7a4-a2a3-47c4-aad2-8c4a8f5a4c15"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3413aa59-0a14-4aea-9464-51af33fadcba"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3453f5cc-a4c9-4888-8cd5-26fcf5e9b729"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3462b0b8-edf2-4902-96dc-315ea21356e0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("346759df-c8d8-43e4-a1e1-4c1314743ff8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("34c02db5-bd41-44eb-a17e-75fca7c678aa"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("34e627ec-864b-43cc-a955-e38e3c84db2d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("34eacf01-b9c1-49c4-ad05-c9d49e21edb6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("34f89dce-cfd0-4e70-bbff-beec08b16549"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("34fa8d5b-d5ba-4f4e-963b-f10e09446509"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("35015519-1f04-4102-b909-7e61de13e4b3"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3542a8ce-5347-41e7-9173-319615d198b6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("35ddee6c-ef6c-414d-b482-e8781360e086"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("35fe2cfa-df05-4da3-ab16-aa60c437dfe1"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("36682f4e-def2-4193-b69f-9e79ad830b92"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("369b957b-d9a7-46f4-8d65-402612c5904c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("36b2d76b-3818-4add-b19b-1ee5a780d79a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("36cade99-c6e0-48ba-bf3c-ebe32940c1b9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("37a477a0-a241-49f6-9a4f-c814c2cbfe53"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("37da781a-3e55-4a57-b022-e45b18a1fffd"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("382164d8-d363-475f-b664-f2667988ce06"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("385427b7-ac55-490a-a79c-59a0e49cc296"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("386aadb1-aaa1-4032-8358-a1eb15271dcb"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("388f81a8-47e4-40f1-9cc6-064979e119ff"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("38cd7540-feff-4444-a796-c23299c976c7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("38dab944-d3cf-4bfa-b9e4-9cba848bb05b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("38e8fe1a-2432-4769-b16e-0c6220f35439"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("38ea9ff7-58f6-4628-92a4-90da306a8e6b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3917bcb4-5821-4158-a7be-7e652535c9d3"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("391a7b71-6d81-46d7-b84f-a1e6e4b1b59f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("391adbf7-6c0e-4812-b74f-5b4e0ea14554"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("393a9694-85de-4e95-9480-4d5f50a2069a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("39631f6c-67d4-4cb4-b020-be6eafd75d17"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("39b9a251-fdb6-47de-94f8-eccf786fafc6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("39c345fe-34a2-4d20-b0e0-eafdb8271909"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3a21e625-4ad8-4507-b4d2-fff1c8813232"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3a3d37bd-826b-41d3-a40b-41f86449227c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3a45a810-a5af-447f-b3d2-2c424ddef3f7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3a4f748c-e038-4d47-994c-d375200f2c46"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3a574eb1-219d-47e3-8a54-5bf4e3f1b60b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3a638c16-b253-4f12-9dfe-905af7cc3221"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3aa251bc-cb76-4f3e-8549-5d6141fd2b15"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3adaffc3-1ef7-4309-9c46-1b7d9c1e52ff"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3adb78ae-7d6b-4b27-80e7-bb5700a0d65d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3af3868f-4737-49d4-8f7b-964a4e791c16"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3b25b54f-f2f5-495b-975e-a3ab76b84004"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3b34ef4b-48de-4598-b189-c96e49c28c3c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3b41e62a-6fe2-49cd-9d29-c6871fa239f6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3b8f636e-56e5-4319-be11-0df6847c84d0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3bda657a-4133-49f2-b4aa-866bc1c232df"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3be9f0cd-eef3-44fd-b242-0a1afd3e014d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3c07a450-006c-4979-8ea7-92644a20a363"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3c22b1c3-7895-4d00-ad61-ceb7afebe075"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3c414f92-18e1-4a5e-b83b-c95887e4269d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3c67fd37-c855-4f18-8f31-8325f9b8e132"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3c7ad07f-2035-4471-b839-4152d1667efb"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3c8f4f25-7219-4092-97ef-0a069944eca8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3cb2d0b2-7c70-47b8-a651-09dc8cb2de38"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3cd9b0c1-3bc0-4661-ae9a-28f0052fac05"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3cec73b4-4a12-4476-8b02-e5bb6da83d9d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3d025e1e-1322-4598-97a0-7bbf58f7e49c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3d09c78c-5a80-4eff-9bf6-ce99f49da31e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3d10c47d-0d40-4ecd-8613-2e25073a42be"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3d661752-70e8-4798-a671-674c8c25ad8c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3d85e3e9-45b7-4115-8e34-28bc9f49e669"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3dd1508e-4cfe-4d07-b2c9-2a05abfe43d2"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3e41820f-d187-4bc1-8b8e-3f39c9cc871f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3e605bdf-3fd6-4a0b-aadb-9babcd1b2815"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3ed0000f-80f8-4463-840c-50079ca5a3c0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3ee501aa-8691-4f43-afc0-873215dfdb25"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3ee588ff-9e30-4021-ae11-b38ac59481e7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3eee0122-51d4-4b42-83be-5eefdb8206e3"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3efbe302-6f37-4bf2-a531-1ac9fca9b6a8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3f0f94f2-66ae-44f2-abfb-4f47f624da60"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3f14daf8-a77c-476e-908b-c76015904356"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3f1d3ac4-f2d9-458d-9781-f48264fac94c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3f5b01de-4f79-40bb-bcb7-4329439bc87d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3fcd12f3-07b3-4a93-b816-6fa84725ce82"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("4014b436-25ed-46d4-9acd-58392e6483d3"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("403432e9-9cf3-44a1-b0d3-0b8e9ed11324"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("4043db20-6edf-46ac-bc5e-6ff66f832ea4"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("404e89a5-86c5-4094-929f-d71a200696a0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("405dc7fc-e885-4395-b528-134fce190ee4"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("40e061ea-ac5e-4d4c-be7f-7899809163e5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("4126e118-da61-4823-915d-6ea71075c0f7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("4141853b-ba3c-4103-b66f-791e53a3478c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("414f44ef-d513-4bdc-9a09-7114854c7b57"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("41506fe7-2976-42de-a3c3-0a152036c2c8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("41acb1d6-0c23-4c4a-a5bc-a9f5a90070fd"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("41bc9dc1-ab51-48d3-97f5-1af495b6e739"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("41bd0fb3-1463-4142-ba02-c09b1b79c989"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("41d32719-868c-42d2-95df-a243210f9a0a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("41fe4f2e-ef27-4feb-b8ea-8edd9b5fd109"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("4228b95a-5d26-48b6-9885-c8f13638acbe"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("42435b6e-bc8c-4e40-b570-bb8c3d23a9a7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("425cbd91-96fe-4ad0-9a38-b0642b6714a8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("428611ab-c8ca-47ac-ba2b-0c520d49225d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("42a0a43b-202d-43e7-a4c2-e1188668c5ca"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("42dae18e-f586-46b4-8ed0-881860446113"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("42e392da-9099-4f04-a53e-3a7c485f328c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("431dd705-e42c-4d7c-8d7d-eee22d445e9f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("432dd487-fb3a-4cc8-ab58-ea3798215396"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("433b27c1-6427-4bdd-81f8-a29195820f3b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("4356579d-443b-4c92-84f8-f482d7dde009"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("435e6461-eead-403b-a7ca-a4b044efe05e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("438ca274-2324-478a-8af0-f190c118f445"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("43b4797b-cc46-48a9-bd88-c1a6915560fa"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("43fc6a84-a915-4b95-84fe-968b01d6887a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("440dcd61-0e38-4e47-947d-4ffc31e894ad"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("44363260-f784-4960-ad48-b77ed364c90f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("444c8dca-dd6a-4576-a563-6df66d831391"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("44692e69-7ed2-44b9-b647-50b1863f824f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("446e32dd-82a8-4913-9b01-9acce8bf79f6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("44ad6021-e2b0-457f-8e96-53bcc09514ab"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("44b618a8-cf12-4283-9ced-6449b530f44b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("44d8f33b-fd00-4837-a910-b70bf283d7db"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("453e5c45-f45c-427d-8288-9eb383885706"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("45553b52-4e17-4c22-bd41-898292a13d6f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("45738b76-2673-44da-81f5-d083cfc62d2a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("45a38648-6141-417a-baa0-83eee909ec92"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("4663e668-63d8-4f19-92b1-05d8d8be11a8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("46d3ea6a-fe49-48b7-8872-9d6fb138375a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("475abd69-35ca-48b6-b760-1f33ed40c5c6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("476dc800-47ca-4193-83f8-d303a143945f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("476e1968-f1cf-48fe-8750-d243185de639"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("478a448e-c3a4-41ea-84ab-e37db1132b55"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("47c69434-0b15-4ce9-81dc-958a18eae221"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("47d53963-a316-459b-be64-45fda5a11a67"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("47f9bb11-9b64-424e-8684-54f1f762dee2"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("48285645-14b8-44bb-83f2-ff5aa017d4b5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("4853d8e8-7dcf-46ae-9faa-ebf05254e9f7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("485c837a-1cf7-4aa4-9f98-71cd1982a07c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("48a093d9-c4b9-4816-bd00-50624b5b680b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("48bdcabb-54c9-42e9-87c8-d3a2b0e05cfc"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("48f58d75-2aa2-4f3d-8a31-9ca06c443a32"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("48f697fe-dea1-4327-a218-824316b553ee"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("491b0a6f-af41-4971-b576-03e3c78e03bb"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("493bb547-cb22-42e4-839b-e37bac538ec6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("49a92360-d5c1-4aa2-a874-29b02a5f45d7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("49aa7c26-5773-413c-b671-bfb6c638cf75"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("49baaed7-fdba-4cc8-aa6a-5d332c0bb9db"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("49bc87b2-3e2d-47a2-af40-ab998ab7968e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("4a35a92c-64cd-48f5-aa96-49afafd4d826"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("4a5a0451-3233-43b8-be80-62ce9cf31317"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("4a88f169-3c72-4f44-a9fc-3f52257e2649"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("4a9b0a77-b3f3-4628-833e-73f60205b55e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("4b444a5b-ba54-4bae-927b-e04d681115f9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("4b574f13-25c8-4d72-9bcb-1b36dca347e3"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("4b954b11-8c87-4047-9ef3-f7c0c2ebde1f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("4bd2f549-1abe-48bc-a445-07637d6934fd"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("4be3fef8-8089-4bfa-987d-459551b48430"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("4c7c502f-5ca6-4825-91e3-7df3f5a59faf"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("4c7fd913-9bd3-4f59-80ec-c3dba706e10f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("4c91326d-0935-441c-acb1-93e53b02342b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("4d066777-81b5-4d37-a41b-588c56884cd8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("4d0fda31-4099-404a-9de2-e352c65be4b4"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("4d787ede-5554-4332-afa0-ead82e8596ad"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("4d8a4f2b-dae3-4108-ba6c-c7d9b2f8c818"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("4dbb8509-87d1-42da-8af8-f6538d3b6656"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("4e5eef54-e052-453d-8be3-7ae3df6440ca"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("4e61c720-032c-446b-837e-942ba7fc52d0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("4e6731e8-11f4-40b5-986d-882d4f735828"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("4e7756cf-1532-4386-af10-16dc79f3afc5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("4ea17408-32d5-4419-beaf-289c047fbc62"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("4f20b6db-07c7-424d-8b89-c63660a47e9c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("4f4bb434-3bde-43da-b9cb-0f609baae4c8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("4f5c56cc-76ce-455a-b03b-fc0cc75bf7b8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("4f78c615-382c-4253-889d-2d16db76ab73"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("4f84d5f2-6045-41a3-8742-a9bc970d037b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("4fa48204-19b4-4ba2-b6ff-f1b8c3c1d0eb"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("4fb480a9-4a15-48d2-bf4e-476455a78fb8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("4ff3116a-2507-4b5e-89a1-ca2fac9e050a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5063eae9-1d19-4fdb-b79d-08bf1fb1967e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("50b6d73a-25b4-41fe-a5c2-7784f21343ab"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("50c7b73d-793d-49c9-b0b9-dfc20072e558"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("51419d04-4602-469c-8f2d-2acdb86d34e0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("519df0ff-b4be-4e82-b135-9be576756343"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("51b1a620-1203-4b16-b645-3914aba8ca87"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("51baac39-e6f1-4a78-b4f2-e1a27a59238a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("51d91e5a-c91d-4dd6-ad83-39d7d5b3c39c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("51f4fae2-ffd5-4856-99b1-8c147b1783fe"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("52286b44-0fe6-495a-aca0-6a45e992825a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("523f9f94-d612-464f-9d67-4f1a0ee69657"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("52798172-6ead-46f8-bdfd-643d75f1e9b5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5327ea16-d703-4436-bae6-95ffc440ee53"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("53557635-d831-4a7e-be85-2ca829131153"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5366fe16-a2f6-412b-965a-b89facfec4df"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5394f825-dba3-44dc-8fc7-0df0e56685db"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("53964d69-6263-47c3-979a-eb1bc87f7fbd"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("54449a21-4aca-4ac9-bd4c-26eabfd69f3b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("54547302-e37d-4417-a4a0-310263373eeb"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("547bbce7-8493-4870-a8b7-9450ef068fa4"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("54bb102b-e6bd-4394-ad80-54d58bc7f8c7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5502f1f7-4698-4d3b-a906-755224c9a224"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5522426b-1a63-4004-8050-afa10c46d977"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("552796db-2b49-4c50-a378-8687d6f58dc0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("55302d2d-8d73-43b7-ac84-119e8b2e1616"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("553308ae-51ff-4cb8-ab88-d67a2d88253b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("556f4eb6-5cbc-487e-9af2-cd15436ef417"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("55b2e32c-9c1d-4f31-aa07-256ab91924a2"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("55b34741-a989-4114-b47a-b3c03f9436b6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("55b708b0-a85d-4f3c-8f7c-5db56d387feb"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("55ce4249-81fc-4645-80cd-9452bcd5258e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("55dd7f60-64f9-4964-aed3-ba4eda386895"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("56587ef8-0534-44eb-84b2-1804a48bd063"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("565fd87c-c69a-4c60-b54f-d55626cd44e4"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("568f1cbf-7a3d-4413-ac01-d6cd9504cd29"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("56b3abea-cfec-4717-84cc-3027d731fd4a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("56b8afc3-545d-4089-b4b6-aa12106ebec0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("56c84480-02c3-4f84-9cc7-0c99b64b167a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("56c866d8-38ee-4c2a-81bb-784351350695"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("56da3835-805c-4fdb-b326-3e6691c37f2a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("56e31208-a3cb-4bb8-ab68-2ffa20cf3683"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("57170354-f7a4-4bef-bcce-e75704ea6879"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5721d3c3-1806-440f-ae30-69168c4a6b7d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("573c51f4-b452-46a8-9ef6-a30880bb7cdc"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("574eab5a-a458-41b9-b342-25b92f86ca3c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("576400f1-4d1a-49e2-ab25-b3d556fcda2f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5766a04a-416c-45ed-b595-4870617b8835"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("579e416c-93b0-4644-a27e-a758e6bdc8f5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("57cfe593-fed9-4fc4-8baa-d3b4854fbd0c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("583434ba-6e54-4a9a-9e6e-319cbc2462b5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("586f80c0-a796-46dc-bd8a-2f258d80a5e2"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("58830e60-73ec-463d-8fc5-dd50006880cf"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("58917db9-66a6-4bf0-ab9b-e6f84a697a03"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("58bc51a7-9817-4d4d-8413-162117083123"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("58bef565-e0f5-4e6f-963e-7fc9ef70e4fa"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("58d5d357-04d8-4a43-a4b1-c92e818e0fa1"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("58ebe09f-cb1a-422b-87e4-7af97ca1cebb"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5925db8d-306c-43a5-be80-6fe72216b504"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("598ea4ef-f7fa-4d5c-aa7c-5d6b1a3e87f0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("59917cbc-5ebe-4499-bd24-33b6116efdc3"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("59a25822-d064-4bb8-be7a-0a4bb7831596"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("59d77f50-752f-4f1f-8828-c02c7f1e5681"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("59fa7ec1-29e4-4e5f-85ef-cfa01cdf30ee"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5a728384-8cf9-4872-88f1-3505b5dd6c8b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5a877c5b-3809-4607-a214-9db06cffad7e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5aa404eb-bb50-4157-8552-c82286519c0e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5adde787-4c4e-433e-940f-3361e5dfdd32"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5b0cca8e-dd95-44d6-a155-278a6c73d69b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5b398a15-d5cc-42c5-9ce3-206683576887"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5b3e925e-ff48-481e-958c-ca423e2b26af"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5b8976d3-916f-4d7e-8f75-3f74c9a16d77"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5bb25bcb-3502-47e8-a0ff-bb1b82222d33"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5bc8aa6c-ef41-4b52-be3d-468d7e9e4615"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5bcec838-7f13-4afd-a54d-fd1c04030e54"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5bfc7bbf-9b23-47ac-93c2-11dc21761d1f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5c11c9bc-111e-496e-bfde-375c435caa2e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5c3764a8-8dd1-48d2-8054-e3adcaf9f329"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5c43ee62-a862-4696-95e5-e29f9b31974b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5c63d2e6-06a9-4d4b-964b-16bb9ff09320"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5c6671a5-7035-4848-a127-9efc1b0b0fb5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5c72de01-669e-46b1-9c42-d3807cc9cf6e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5c933a7e-45bc-41eb-8750-75974b52d19c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5ce88fd5-c56e-484e-8fd7-4a9db9fe04a8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5cfb55a2-8bc1-447f-94db-1cfcb6ef7de6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5d093a20-86e5-4bd4-9af8-4b7d99b2b38a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5d165020-b3d5-49ed-96c7-823d670ac327"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5d3344e7-2d76-420d-9a51-6fd9f099b0c7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5d5202b9-5551-4a13-b9d5-e7365af72872"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5d8c415f-889d-4c7e-89bb-8712d8e6f451"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5db1e31c-0b2d-4cec-8948-d807587c6ffe"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5def97b7-415f-4474-9dd8-6b6f6fbbae99"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5defee3b-369c-4d11-850f-e1b221c854ae"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5e5bf2a6-7b34-472a-a976-5d487723ca6a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5e780b07-bc44-40a7-b568-363c60a8cd1c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5e8e1bc5-76fe-49f8-998d-87c7703b4306"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5e914ab2-6ecc-4ccc-b4ab-74fab27cb4dd"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5ef92d4f-ee77-4b21-a7eb-4ca8e11fbdd6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5efc50ee-821f-4f93-836e-15e2e00e9615"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5f2be574-e12e-41b9-8e2b-912b3e791d73"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5f3fa42f-8e4f-490b-831b-5a4b377dfe99"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("600f089b-d010-4f0f-9b9a-e0454877cee8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6036321d-b9fe-4d08-bea6-cea5a74b366a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("604f7027-5e40-4b71-870e-0c0416111659"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("60634adb-265b-472d-a933-0aabf76e09ec"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("60649a80-50cd-41c2-9331-cbd6e898363b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("60943501-351b-445b-9e9a-0b41908e10e1"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("60fabe1f-e034-43a6-b78b-04b88ed80ef2"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("61c7104e-62db-4afd-80ee-82b3beb80e51"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("61de39af-ea4c-440f-b067-d141803da8ee"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6227e2db-a8aa-4ef2-9dc4-50e8613abd9f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("623d6139-f6b8-4195-ba23-77532df794e3"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6252ed64-2767-40af-9958-a5b384a26eed"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6272c51d-9606-4a26-8006-e06b80cbd23d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6273dbf8-6d1a-451c-94a2-1ef2aa187284"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6286ef89-fa59-4722-829b-6b6942d31b65"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("62900ade-8d65-4998-ab0b-fd1e60137b8f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("62a1db9e-7f37-4b0c-acc9-fd449405613b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("62aabba7-cf0b-4af1-8ecf-f390dbd19d6e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("62ac6698-ef06-4e8b-a5cc-38c707fa1c8f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("632d41b6-c3e1-4bf0-ad45-852188e1c0e1"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("636969e1-02fe-4364-a66a-5536c03966bb"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("63a2dac9-cd6b-400f-a733-7800490ea4be"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("63cb6b6a-5a73-4a90-9a75-7fbe9efc45b6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("644e6c3b-ad86-4765-b7f9-5ef50b8e7d17"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("64b0b917-5a26-4de6-a31f-4560da248ade"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("65221774-02a5-47e6-9871-3d2649a947e9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("656b86dd-4490-48a1-ad11-d7ad00f86e66"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("658fb64e-95f3-4cc7-9b02-044ea2842ef3"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("65b5a0c6-9899-44e1-b4b3-8001f86cab5a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("65d29394-9eb2-4353-9952-2dec7a624468"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("65d7207f-4e8b-4fd4-a96d-61a1a1b3ed58"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("660f24a6-b4c9-4fb7-8280-2786c00f1832"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("660f7262-c9e0-4d2e-a7a3-819cd06d85f3"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("663896c2-2b03-46a4-858b-edf0051e168d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("663fbab6-5647-42c2-8526-92e56ec7e95a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("668094a7-ce45-442b-94ea-627f429be5a9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6688e046-7a19-4b6b-906e-ceff1c3ef85e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6697f9e5-03f6-4d7f-8286-0a352a720b91"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("66c1d9da-76cb-4989-952e-08c5af70a880"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("678e3fda-78fe-4967-81a0-3025f1b6cde2"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("67d15e48-3ed4-4ad5-a753-4d03009ea056"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("681077c5-e4f1-44d7-9904-fed8c3b61aec"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("682c5a3c-c854-4492-8f93-ceb19a011c6f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("68402b74-8a2b-4c7c-ae6f-61930e1bfe6c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("686e15bb-1f60-4c73-a966-7861d016467b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("688ed1a2-b598-420a-a0a5-10607ccf558e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("689509dc-d929-47a7-b132-2e6b48016ba4"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("689b4033-c5de-463c-a34d-53dcf993d93d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("68a48811-bbd4-4139-983c-5fb30054c8d0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("68d4e0e5-7154-41fd-91a2-143bb9b648d4"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("68dfd22c-ea5c-4a20-86c3-ce61c13e9ef2"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("68edb66d-a78d-43ad-b363-21ec60687f96"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("690b74e9-3e35-41a5-a03d-7c8801125552"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("693acc49-a649-4027-8735-847f4c22783b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("69853a4b-ff4f-45f1-907a-a8cdb442d3fc"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("69f63eca-7315-49ef-9517-9b5edc7147d1"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("69fd837e-1eaa-4cf8-9ca2-7386684989f8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6a3b4ecc-437f-48c0-95d0-ae49f2d3c57f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6a760f22-f973-4392-a874-49c8a1c008aa"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6b068c45-7422-42fd-9b46-8b3a2e032da3"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6b281757-4ed5-4873-b192-fe39ccca593b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6b5d6637-b315-4fb4-ad6d-9df9ebff903b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6b62af01-bd46-4772-8797-695bfe62717b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6b7ec776-9580-4a54-9538-415f884a0801"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6b8b6116-0214-4d31-b3dc-1bf905a1dc15"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6b8d5921-caa9-43fc-b9fd-a5c429134dc4"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6bc8c948-9445-441c-afaf-e8da559e79cc"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6bd9a303-5764-4fb9-991c-6b305a3c662b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6bebc908-fdca-40c6-a11d-5779dc0e08ea"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6c18b6e6-97c6-410a-befe-6d010a8aef7c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6c49180a-4b10-46f3-a3d8-2ebd75a4aa21"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6c5c4b8d-4775-40fd-bdd8-025bedc16fed"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6c7ece7a-95a8-4c72-ad9c-636e099e947d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6c93c012-8f63-4b40-8380-4509b98952ff"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6cae025c-9b7b-4035-b057-0147b7f31790"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6cdf11f1-0d6f-4220-b932-d8d2cb00d8ef"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6d0a531d-9f59-4d66-ba3d-7962207a2158"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6d378de3-b16e-46bd-a901-981266aa3ecb"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6da74eef-b0e3-4f04-be14-fda298b4ed80"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6e9e7eea-e05e-45c5-85dc-563f78512d1d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6ead1d96-794a-49da-85a7-43d33e51d0d9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6ed905bd-9a9b-4f58-aa84-2c0921e58409"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6ef4c7e7-0b72-4c09-bef8-80ab05eb0623"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6f233e09-5ccb-4f7d-a9ea-a572a81c89cd"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6f2f390e-f2bd-4c8f-bd27-3aa650eb7480"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6f883bc9-01e3-4e13-a709-c157767747b4"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6f8f8394-0a78-4696-abd1-2289e5ccceff"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("702e1c9a-5965-4556-a408-763e6450dc07"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7046d1ea-ad8c-4f70-b47d-a58e59f0254c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7048cb16-f1e4-4fdc-99ed-806fb7131ef3"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7062b144-ca9e-41d7-b763-884820e9f390"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("707207b4-3be8-424c-86d8-7d014abba57e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("70771493-508f-4927-8aa6-cb509ced6989"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("70ff558b-ba72-4407-8e3f-4c4de913867a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("71753775-70d5-4e0b-81b8-2dee013c8b9f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("717fb02a-d703-46f4-8872-c87cded72010"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("71d2189c-ad28-4a2e-9b5e-a9bc9e21877d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("725b7561-ecff-4877-bf67-03ff8c2b401d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("72f6c7a4-c596-4b5a-8b82-6e6ce30b12f5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7308bdb2-cc93-4583-a359-8551b8ec1cdf"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("731277ac-272b-4131-b6cc-7a8db657774d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("732d9f0a-5d01-40ba-ad14-d26896d0ee88"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7330f94e-adc4-45f7-af24-9b0f26647cdd"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("734d465d-bc38-463f-9478-71b5b85d9e78"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("735c5632-3a83-4666-b47c-d59d71af41de"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("738d4a8d-a2b1-4eb7-b5fb-faf4caa8652a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("73a3a509-a5ea-4652-b1a5-613bd35243b9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("73adcec9-11ea-4a37-a828-a383accdb329"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("73cce602-e78f-437e-95d6-0054c03306d8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("73ed8e44-7b34-4271-880a-087e89ca8045"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("740922bd-503a-4666-bd26-caef2c61ddbc"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("741188bf-6d81-4e28-91ab-d25e335c04be"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("742cabc6-092e-4966-947b-3d367b9b2af1"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("743fe59f-1324-40fe-882c-50a96f691118"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7477f9b2-9702-45b5-a34c-65c01add0fac"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("748bb4da-be4c-4d4b-98c0-b9b8f9517075"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("74e0eb3c-6ceb-491f-89f2-a4e347132120"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("74e163bf-4f1c-4ac2-b58c-e703a7f6d857"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("750530a1-0439-42be-980d-540be3148a60"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7518c8b2-be38-42c2-8ae3-aff25afb05b0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("752e92ba-3447-444f-aa0a-c75cb4edf765"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("75381907-195d-4a4c-9c53-ceca2bde5a22"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("75566116-9a45-41c8-9040-d5701a21a4e6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7588b46a-c049-47d7-8967-590e4c687d8e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("75a4b9f3-8347-4567-bf7b-d5d017649e4b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("75ecd4d7-fb5e-4c07-b787-e06b300a1653"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("75f08a2d-0f26-4585-8d61-a4bc56870f22"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("75f20edd-1664-4d50-bb6a-c779ad87a773"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7603ab8c-968f-439b-bdbe-b782c073d7f4"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7622a564-c52b-43b4-8114-84f8d9aaa78d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7644effc-d801-4a02-bf5c-4559d4f9cada"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7651639a-0aaa-489e-b09b-081123ae839d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("76967cd3-398c-4adb-a14c-df4356f45f8f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("76a18376-e4c7-4373-932b-c46c6d722dc4"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("76adae69-7eeb-4f10-8107-fe1836af820e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("76b13bdc-e0ee-4593-99d6-653ca2085ee2"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("76f5b3ef-4e87-4eac-b7d1-290270544791"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("76fa89ac-b753-4126-a0e1-e03c5562a60c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7752124e-3ade-47de-a2fc-cd54f3600cf9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("77722912-7585-4bef-8116-dcec4718c98f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7772800b-c730-4c55-8a0d-52f40908d7e0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("77a4e585-aba4-44c7-93df-41cf9019acd6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("77a75042-cfb4-4789-82e8-6065b4909997"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("77b5b603-a87d-420d-9568-c26cd991ab03"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7804d247-6417-4b4d-b0b0-ea4e9a3bd153"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("782256da-8eb7-48cf-a568-b9e438257fcf"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7848abf4-eabf-411e-9072-b206391c7d02"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("78a5f339-40e0-4798-85e1-a6ea8a3b2e8b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("791e90b4-c278-456d-880c-a53b06c4eed4"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("79524ec2-3dbf-4d8a-af63-be18e915b953"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("79989c94-096a-42e5-86e9-54496e2255bd"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("79a5cadc-4c8a-48d5-9589-c0f1f89b0cab"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("79d58a88-8be7-433b-b0f9-b56cd59a6a14"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("79ea2fd5-e049-470c-9299-a56b3cdf29a8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7a7941ba-09f0-465b-870d-68c07a510b98"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7a7e8fd1-cf47-46e8-af43-33b4981bd0d5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7a8399e5-0d3d-4be5-9f32-764f8f417c2e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7a8eac2a-8349-43ea-9cca-71a8653b763a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7aa3cf06-18d7-4399-8618-1efca6b071b5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7aec7644-23c9-4373-aabf-f41b39306c29"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7b382d3c-c525-47f6-b3bd-a52112b9c3ab"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7b63b14f-2467-4c72-930b-50d2918a2d18"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7bbdcb33-fedf-49f5-a0d8-fdb3ebf21efe"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7bda4429-a989-4aa1-8e0b-633e0cb35475"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7c1e887a-1a6b-4d24-9505-6a646d0aad1c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7c8452d9-b030-42cb-bef7-6ea0d7c3962d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7ca4ed44-ab25-4f35-9ff7-a2a9a5b5c179"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7cb1ffcd-fadb-4971-9e70-5190f5bad7e9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7cd7db73-e8fc-4c6c-93fa-a23003807f1b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7cfe171e-221a-4490-bee3-d8fd833d0d76"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7d3ad107-fd7e-47b4-b166-ec00fb93ff71"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7d44306f-9b5c-4c22-a59d-e29233d645a0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7d8f2a27-71d5-4b8e-8920-894e548b6c44"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7dafcdaf-24bd-4f17-a82a-fbcf8f251ab9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7de02baa-59ce-4935-9c5c-c211f34ec4a6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7de34469-0e70-4dac-b332-064d2c37673d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7ded373a-9c34-43df-8e77-6c002e9a3d5f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7ec9a7f3-761c-4a71-82a1-6092801f47ab"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7ecab0ee-c41d-444d-9520-462f7ed202d8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7ecba2a4-e38d-4696-8eae-d0472cc88846"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7ef73381-837c-4ef2-821f-42829f02b129"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7f309218-5591-474f-b04d-c2fd3895fb3f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7f648961-a63d-4bde-b7f3-32f6b5861373"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7f97b210-c796-40e1-8834-c363c6da9470"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7fb9a4f0-fb88-41d5-ba47-0155cc0ac060"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8001cf73-50a1-4861-b14c-5c4d171879a0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("802952d9-3e1c-4679-b39f-2af7a4fa5b98"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("80852e3c-d1fd-47c9-a315-064394689c60"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("80862308-4c4a-4d36-9b9d-587794551d1b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8094d9b4-6b2d-4614-815e-0304d9c7f05b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("80a05f33-8225-44ea-8011-819dd9ca2daa"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("80cc809d-2d3c-44f4-ab8c-e244844ca236"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("80ce5893-9a79-4492-b2d2-944f0be923b9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("81110b31-8b05-4d6b-8689-e15046947374"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("812a7beb-a791-44bd-a183-80ab09aedac9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("81713596-a4d8-426d-9531-7397709e4134"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8176f6cf-87b2-4bd9-90b2-dc717d16defd"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8186d2bd-9804-4cdd-a05b-ffa7222efa07"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("81c20efd-aef9-4987-9113-31b30af14084"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8206fd68-d394-4255-a26f-91dfb5c07b51"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("822ac69e-48e6-4543-9a6c-b55e23f75d41"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("825e8662-bc93-41b3-b994-0003aed3952d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8261de11-3fd0-4dd1-8397-6107b84499e7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("82621ad6-d432-425b-9aba-1b4ba9eb59c3"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8270e249-6f8a-45fe-b40c-8cd1cc55e264"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("82a6ad3f-3646-4c12-881a-de38bc893bbd"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("82afc3cd-ffc4-4faf-af47-b3839d12190d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("82baac60-977e-4778-9c92-6338b47a480a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("82be3d5f-af3e-402f-b613-b9be2dbb3c91"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("82c8090f-0073-467f-97c2-c4639253b50d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8300f384-ae4c-4dc2-8d11-8a31412adfc7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8316caa5-6b1b-4e4b-a622-13c562ab6010"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8357f3c4-bd0d-41f1-af1c-f6e90888fac5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8367b6fa-78f0-4e4e-b417-c7721212adf8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8375c130-9cb0-4d5e-b86a-7f6ade623b3b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8392b1a8-95bb-4947-b705-f9bc1268162b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("839464c3-199b-48e7-8156-e715abb971de"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("840b0679-9168-4c39-8cdd-b53de203b53b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("843111ab-fcbf-45f1-92c8-2ee8dbfd5da8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("843f3167-f954-4dcd-86e1-ea696892c90c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8449bb92-7f10-45c2-8825-7f846ba37011"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("84992cbd-0ee6-4166-b666-5d25f3a9e827"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("84bbaa1c-524c-4925-a6b8-1bfbfc7fe861"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("84e5bc67-40d6-49cd-9b16-f24015626571"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8516bb0f-fee7-492a-872c-e93ed391ddb0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("851b3ab2-2748-40a0-9d99-ea1b8a9f2c30"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("85466ca4-c7ac-49d4-92ea-4dc2ab330852"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("85626e72-57fc-4ee9-a84c-781fa7f2ae14"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8574e03a-3f1d-4415-851a-887f1b298cb7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("85b46862-d64d-4eec-a4fa-3120b6d9b60c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("85f6f283-6622-4f82-ae37-9aa499989e24"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("863d333f-9174-48a8-b054-6edb167144c1"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("86c56323-86e3-44eb-92d1-50b061a15076"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("86d68f4d-a885-434b-970a-87751cae1070"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("872c7132-4b7f-4443-b06e-4e7dc9ac8a41"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("877d6f14-19ea-4baf-8a81-f6c38b5ca4f8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8788eb1a-7148-459e-b190-138969009a87"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("878d634a-2cee-439f-a3c9-3f88b4a4e61e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("878e10ac-cffa-4b3a-bb5c-cc0d59ae8221"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("87d68682-c0e8-4500-93c5-3f7ff456164e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("88007c7a-3457-4dd3-90e3-86922919bcd6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8811f5d8-179f-4cc7-897c-973ef91e337e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8835ae44-434f-4134-af1c-eb0e6c5fb817"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("888bd3d5-86e6-41bb-83c4-ec3d31629ee4"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("88bc0af5-32bd-40cd-9a66-8aa683487d62"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("89132be1-8ec1-4a69-bfc4-13d0a819c10b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("894e7409-da7a-4ef8-b374-ed20d67e1c44"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("898e9e44-ba66-46b1-b5b4-7f01ea926292"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("899aa4dd-74b5-4159-b722-48fdd4822a4b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("89f96a83-60af-4e38-8f22-bff721d7ccda"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8a1675e3-881b-4c2d-a98f-52559e2497ac"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8a335abc-15e4-4ca7-ad61-01d5434f6f3a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8a9fc70c-03a6-4b93-a9ea-55352bd19273"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8aa2df3f-91c7-4949-b5d9-85368e8ae4bc"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8aefeb1c-88ae-4f29-8c38-d889998f832f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8af59050-5c7d-48d9-b6a5-1e71d79a83fc"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8afd4cc6-6ee2-45c7-bf44-86b1dc36f1d4"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8b09f020-7f69-4471-b25b-f3db7550df60"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8b310878-d485-4bb2-9a50-35217398aba8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8b4b8c8b-2c88-4cd1-801a-777d1856677f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8be27f5e-e082-40d3-8bb3-0d3ae2513d1d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8c250855-95d2-49d1-ba53-7b91d29fee03"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8c2f07dc-c6ce-4fe4-83fa-df96ada34f9b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8c3ed0a2-2f4d-4a4c-bb1e-a06c20194d2c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8c807866-feba-496e-937d-e7996c276366"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8cc05fe7-ed83-45b1-b333-ae708454d891"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8cda6df0-be6c-4d64-8636-7a2e3ce3d755"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8cf23147-2506-46bd-b731-00b15527f686"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8d00899b-c61a-4a25-a4d2-40a3e656d530"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8d02bfce-06d7-44ab-848d-a9db1a1bdadc"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8d1d9756-ef6b-416b-8d94-58077e1fd621"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8d30ed21-d0d0-419f-8b8b-856afbf71f2f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8d324f1a-95e9-4478-8f71-7b59afa80edf"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8d48fd92-358f-40fe-91e0-299caee16019"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8d55d3aa-dd6f-42cc-8bed-0e905f398da5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8db70b38-3d87-4105-af92-1115d6e3e070"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8dba7455-2fb1-4cfd-988c-e31d7d030a53"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8dcc2dc9-e5b0-4f0e-aa2e-677c318bbd98"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8df64559-8b93-4c06-b12a-dbc7d9cf554b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8dfdcb39-7808-4a1c-94cc-91feaea89902"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8e0eea77-c780-4dc9-a817-80195be0cda9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8e143dbd-6c89-499b-a21f-5b87feac2c06"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8e16865f-5f5b-47a8-9ea3-31f3ba670433"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8e4bd930-f37b-4eb4-ac94-6996e1413374"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8e9b4527-04e3-4a2d-940a-7d6fe9071e9b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8f07558e-ce24-4435-a266-167d8ee7c572"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8f0862ef-c776-481d-ba62-424cbd418ced"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8f4d2d50-f246-46af-a380-acc469e45d6f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8f64afd7-3068-467a-8a3b-792a75a2a113"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8f652efe-7f41-40bd-b854-92c228fa7130"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8f6c70cf-d87a-4b23-9477-bbbb41b40f6d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8f8bd196-37f8-4886-bcab-85cd10a4b8f4"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8fae3d5d-bbf1-464d-8f30-ab4987b0a28a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8fbb17b7-77fa-4395-bb5a-6fde9a6a0a68"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8fcf9358-37aa-4674-8bb6-7982fc422350"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8fd4ea15-20b3-4bc8-a595-4524f028a3d7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8fe42b9a-1e67-41fa-a069-2b1503593bf0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8fff0509-d2b9-4384-809a-794092904692"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("90298a35-1f1c-4223-993d-a445726f08ab"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("90332e2d-8fa1-40f0-8f2a-63b10ac4ab1f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9078fa42-43b2-4f70-a41d-9c4a4fa39850"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("90d396ae-e34c-464d-a8fd-21fa9d69f04a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("90dda72e-0233-4a44-944e-54b02ad22fa8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("910a8447-ccb7-4406-8201-10a84c726e39"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("91270f41-d6c6-4424-b9ae-ee87aa5b7799"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("91277f6b-edce-4355-a10b-a594475f481e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("913129c6-f6ec-4984-8889-ade735df6f1c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("916701ec-cf74-4862-bfed-914420c60d77"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("919d8745-1326-460a-a3a7-e69758915fb7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("91a78289-62df-4639-bded-fd2affe58447"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("91fcfccc-53f5-483c-875b-c53bd743a36e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("91ffd336-1220-4c87-be4d-114ac343ada5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9260b0bb-a8cb-4b3a-afb1-414e39b2736b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("92884580-8d37-4161-964b-c160133e12cb"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("92ded31e-06ea-4932-9639-8ceecfd05e8b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("92e25534-7fc2-4563-bf6d-c4e28fff52fb"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("92e2dc3c-3101-4b8b-8270-f7f4e7b00459"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("92fa27a2-726c-462a-b629-1bc53013b91f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("92fb1a16-ff06-4425-a2e2-1c2cc09f48d8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("931ed6bc-064e-4cb9-8151-bcbee30f25de"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("936184ac-9b1a-4dea-87fd-c8fbdbeff8e8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("93c795fc-800b-4ffe-8c57-a2208764e71a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("93d2ad81-bcfc-4e10-b0af-c5e6ccafc1e2"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("93e62708-ff41-42d8-ac7c-70136ae6480b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("944c021f-1019-4cb5-96ca-c052c86d1fea"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("945059d1-1bf5-4117-91f1-ee0ce9eaf694"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9465ad42-2570-49c3-9214-7b8128a75dc1"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("94937de5-91fa-4a35-8a84-7944928502a5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("949c8474-c00a-4130-bb35-c7445cb1885d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("94f8b8a1-19ff-43da-b638-f8f4fa6e2ebf"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9505b86c-8d13-4353-a37c-919dd8b67100"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("95154f00-abd0-4330-a5dd-a9450229461b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9542afff-82f3-4e50-a9a5-57d1d6992cef"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("95be21ee-9b90-4e4c-a104-ad413b9a3712"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("95df119c-a535-4163-acc2-7a3576720cb6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("95f0064c-6ae0-4c44-a96d-744849d14d15"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("95f3bd36-3c5c-42ff-9222-2fc04bb6dc84"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9604bb69-2505-4b31-82b1-1bdc87baffcc"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9647aef3-4848-46c1-93b5-f59a8b794691"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("96b84e54-4f40-44c9-96bb-71cf795acc06"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("96be7eeb-a178-4958-87f8-5cf9ea086bec"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("96d77074-bef1-4522-8a42-aa03da8365c2"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("96e593ee-6bb4-4c16-8276-bcd17439e309"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("97756563-1c87-4e5a-9fd1-42b42bf3ec86"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9799ad2f-f611-488d-ab8e-d79c2caea3ad"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("97ac7c7c-19f4-4414-a713-73872ddfa4de"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("97fa2cd9-fc3b-4d28-881d-fe9b2915fd93"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("984c5341-1667-425f-8fba-d20df6b378bb"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("987ae7ca-ab5f-46bf-870b-db3cce5b99a9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("98be8ccc-3ebc-4b4d-90d3-20fb5d97bb22"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9918f4b4-b96d-47ff-a398-47412fe6e7a3"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("996fbfb7-dc91-42d5-bb49-fd0ae10990da"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9984666b-9a9c-4ab3-8950-8bba1f733669"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("99c1894e-8cb2-4c28-877b-db2dd835d705"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("99c2f2e0-51b8-4dcc-b6bb-36a3a4ee19cb"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("99cb1ea9-b4f9-44cd-a8e1-8847ac5dcde8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("99dc47e0-86d3-46bd-bab0-39e8b2821eab"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("99f2ee9e-c767-4c83-99bf-9a334ea281e3"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("99ff97db-7ec4-42e2-a07e-f922c06a8967"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9a4a99f7-7741-4cd2-b213-0f7b5bf574bc"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9acc22c4-c625-4381-b1f7-25c1af4e7779"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9af48f73-514c-43c6-8141-0ebf5c3e634a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9b36b8cf-3848-4009-8786-dbac9ab957e0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9b4ed7f0-ec01-4a51-9bd8-cf528e4b90bd"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9b4f0fda-5942-4bc5-bd4e-8797caa79a37"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9b94f081-556e-4bc9-acf2-8794b0c33c03"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9c1652bf-d3c4-43ec-9aab-bc877bd29e63"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9c7c9a16-d994-4610-9177-8259af7bc012"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9c9d0929-f580-4d8c-8d1a-896978445338"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9cba6eae-16f8-4aa0-9d17-93bd12a8bda2"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9d1022ea-20f0-4fbc-9644-72fb283c54f9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9d325e86-1b3a-4ca3-b94a-47402533458b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9d33b786-b705-4473-9436-7d912cc0d3c0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9d41db8e-6fcd-449d-869c-85d74ff0a3af"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9d423006-3647-48cd-b629-698090ce4bc8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9d478b6e-01c4-451c-8728-11a9a6146ab8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9d4d906c-74b9-4ad8-9711-5c291ca33cac"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9d947c4c-0da8-4aa8-978c-9ae337585c0e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9d96fa44-b9b0-48b9-b65a-31403dc54e8e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9dc4fdce-4205-4eb8-a6ce-6e8e7d9e1df8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9e367893-cff1-4c02-b97c-890f0056618f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9e42f292-cee2-4b5d-a7ef-7cdd36631b47"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9f38e273-14f9-4696-ba90-8f7b5dcb035d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9f3cbcad-1008-40d4-9d24-09c1b7835160"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9f542ac3-cc98-445e-be82-9c505c7aef6d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9f62e88f-e34a-476a-b621-7142f116967c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9f791e88-4732-4ec0-bedf-483439257833"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a00c0994-009e-44c3-ba53-e74c3b4f5500"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a01b3467-d412-4845-a433-6f42e1b638f5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a0391990-0102-4bbf-8d9e-0b204d9c1716"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a0450f4d-5e80-4aaa-88fb-5cd74f9524ef"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a047ad19-8a9d-48cb-9f89-a91ced273443"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a057cea6-b264-47d4-86c8-fe76260e33e1"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a0b21914-e5aa-4c14-affd-163f7f6d5b14"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a0dafbad-f44f-4a97-836d-7424a11fb281"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a146ec86-4547-402c-a995-bb9419c29344"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a168b9b1-8eaa-46a9-9b1c-129427dbb8ca"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a1e129f5-629b-4585-8c5e-694aad18b37c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a1ff9aea-0b69-4a21-8d5c-83850a07e04f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a23a3eb5-e230-4603-9949-0172fcaa76fb"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a24b1afb-6d15-476c-a5b2-c9550669883e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a2916131-a665-424f-804e-36a4b2361c85"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a29959a1-7c19-46a0-a1d2-392ba4934a0c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a2c79ffc-c5d5-43b3-82d9-1eb540d57805"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a2ccdaa6-bae0-48c3-9776-f49072c96dbb"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a3489136-86d4-4462-8d86-5516f2b5b4ae"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a36d1a53-a318-4965-be40-6be47c112b9b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a42733d1-4a80-4ea9-b805-9fedbdac4d2a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a4ab58c0-a4c6-4e8e-ba44-cacd23036d06"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a50f5127-f0c7-4f7d-83fe-aa9fd6946f8c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a5293b76-fac0-4b46-a48c-07cf92fa5ef1"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a56a8c42-f1e4-4988-9950-f825a07f74bd"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a5a1ea09-44fd-4468-9f50-c3b74744781c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a5b6f114-cc46-481a-8e64-f9ff52f621a6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a5e0cd95-cbbb-4b8d-a8ad-99b81a50be8f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a622194e-c1df-4f27-a8f5-cf8c57fa5380"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a634a867-f53e-40ef-9f4a-e67d3a56af18"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a6442ffc-d6de-4312-8d53-d657c3ab7ff9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a64778bc-d332-4d40-aac8-b8212f74880f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a686e272-60f9-473b-b1a8-cb5350b7fafa"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a6cf9e94-88cd-4a2a-8f72-d92f517f7da2"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a6e9bfcf-3870-496d-9cad-262d7c3c7216"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a6ee40aa-94e6-4d61-afa5-2dfd0ee8baeb"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a6fab7ee-c355-49ae-aa68-19828dd7a84d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a6fafb8b-2eb6-47ef-9d50-1aa0ff06df6b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a7042311-8fc8-40e9-920b-9937fb2facc0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a715cc9d-04d4-46de-88fe-895967338509"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a725a810-2554-4f81-9d16-21a36b582e97"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a748cab6-aaf6-4388-8aa1-fd9d03d10919"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a7ab895e-68f1-4dd2-b8f7-0cd50fc6be4f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a7c109ea-4260-4e9d-bd62-c741a4453031"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a80875e8-f7b5-4855-ba3f-97ccaa937cfe"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a81f4263-4a75-4c22-b1c4-ef66a5a533d8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a8323393-5ec2-42e7-8d73-76be8aee33b3"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a83b2c93-3809-415e-bda0-5310dcda734e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a856ddfa-fd83-4a10-9a00-59612905a114"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a89bfa9e-51a8-46b4-8ba7-7ad1e8d97072"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a8f56577-e884-4315-90cf-406eb15e5146"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a9333780-bfb9-4dda-8319-bd6400d6f4ae"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a95c27b8-1a97-4a3b-a9f4-b18721063f1f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a9f10176-2c1f-4dac-a0f5-24722e9797ab"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("aa1a0627-dee0-4a2a-a770-53073d154c07"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("aa46b694-7bfd-42d1-af4c-9125e263abe5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("aa4b190d-4376-415b-a78b-1cbd267cb923"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("aa549e15-f91e-4c4d-a12d-f3b2cd22e415"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("aad8fe97-278a-424e-ae8f-50f0e5f3ac4e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ab2b34d0-5a97-4521-8083-301423063a6b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ab5520a6-1220-46bf-bbda-759395473b4e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ab6818a1-2296-43d8-a925-d2f1df414a01"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("abaaa9f1-353e-4941-90bd-0f8503f7f0c6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("abd77df9-de8e-4fea-9630-69015aa691b9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ac2069d5-c000-46cc-acdb-3c4a33fd87ad"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ac2c966c-3b76-4675-b015-af47a4c499ca"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ac8dd31d-bc56-4ed3-8311-277c0edc98a2"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ac90e7af-c5f8-4d86-9f77-1f2911c24541"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("acaabc4d-96fd-4b2a-9f2d-4b6224d0c3d7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ace453a2-6786-4403-8c1e-941000b9cbe9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ad2f87ac-ee5c-40b7-a22c-153341060661"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ad5d3ba7-adc4-48f9-8bd0-4c51869a1789"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ad7e9145-24d7-4f72-9a04-1ffefd3f45f0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ad813942-10c1-41e8-87a0-ce14bc09e545"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ad9f2bee-1b60-4f87-b0c0-e062dc5a3616"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ade030c9-2808-40df-aa8a-3781856aa75d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ae12666d-f58b-4b99-9f75-a2ab22e3d3f2"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ae3452ca-2a9e-435d-ba7d-c821dab5fd11"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ae815a15-5ebe-4932-b04b-2321a4a6ee45"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ae8c3f08-ebf0-4f68-976b-3b3f771ddaa6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("af2645e9-c27f-4cc2-bdec-307e791a4cb4"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("af5b01b4-c7fc-4bcf-bfd9-0ef151682040"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("af783163-5ab8-44e5-8d2a-5077fd167c10"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("af8f92f3-c16a-4343-b752-f90481a0c6c6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("afa5bf91-0e04-4a4e-ab0f-30507ab6ead9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("afdf8059-99e7-47a6-b6dc-f344756ac6ce"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b009b380-8868-48aa-9a43-0b64fc9490f8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b01a47d5-2738-466a-b5cb-1f900ae7fed3"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b0369394-5dde-4ca7-8b9f-cfe8851d73d5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b075d0e6-13b4-40b4-95cd-226f8b2ca356"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b08fa67b-bcc0-4a41-8898-32d34fdab49b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b09200df-d8b9-4d28-a439-f525670bd8b0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b0932dea-ee81-45fe-91f5-a6b34a59904d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b094c5a5-cf52-42c1-a548-84d350ffaab5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b0bad895-85bc-45c4-b7cb-7bc157384051"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b0e1417a-1837-4b29-8ce5-f2a5922082cf"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b0ee512e-c08f-4516-b544-b471fc341118"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b0f5d0a7-cc3c-4dc6-aeac-d53af7f67e12"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b10634d5-61e7-453e-b3b2-e36e9934500f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b11d6292-01c0-40b5-b94e-3aea95d2e262"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b127fcac-23f4-40c4-a7ec-b3570e998874"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b1509c7e-0432-4d12-9f2a-f949b07dcbad"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b1a33f92-1b9f-4fac-8a7a-ba90f9ad174b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b1d99dcc-9fd1-4671-919f-97cba4f4f972"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b1ddff20-7be7-4711-a2ec-eee463b72bea"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b24fa313-7804-4469-8ab3-51c9986e2803"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b2d8ef59-6540-4746-ae2e-1daeb9e37c62"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b3438597-463c-4630-870d-19f7dfd9a6a5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b359efbe-c353-4c99-a31d-a17302f90fba"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b383dab6-7c3d-485c-b025-fbec5e5c8bb0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b3912cf9-c309-491d-b672-2cbedaf825b3"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b393471a-c055-462a-9a72-9e6c702f5d68"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b3a2a73a-258c-4c62-a84d-b32745769072"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b3d1d72b-1a01-4827-ad7d-4c733b2d5f6d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b46c9ebc-b827-4316-a398-314d84a7aa04"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b48370f7-b865-4092-8b36-7096bb0fc3f2"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b4dbdf28-2dbc-4926-b949-2899fbbdb04d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b4ea95a5-f688-4fc9-a7c6-5b475dbe5670"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b4ebaf34-b0b4-438a-ad63-d497db3267db"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b4edc1a9-02f0-48e5-ad18-e67ef5529c38"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b500b5d3-2dbe-4248-a824-28e50e93c496"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b504b26d-45e9-48b2-86d1-ae28acac3f49"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b5932136-7a3c-47fe-8e35-6e16c8bc0b38"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b5c0471b-f427-43c7-8568-0a2c38aa1013"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b5d46fde-5942-4c06-ba8c-826f28d30c5b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b5e15d02-cb71-4d0c-989d-5b788a08b4c9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b60a4d84-170f-4641-b735-71a5cb59561e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b61b6085-1d98-444d-b716-91da84214704"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b6e8291e-f76a-421b-9209-cee6f06e5c6a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b714a270-b444-4286-90f0-2ff14f7de344"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b724c63f-204b-44b6-b0be-01634768609a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b741a185-53df-41f0-ac0d-7503812daf9f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b7595fa6-66e9-4b41-a000-e0a2f90f26a7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b76efc44-6678-45c1-a40a-bbf962850ae2"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b77324be-caf5-475b-b069-ee77c3b147ce"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b7cd69d0-8574-4597-bdae-976128798734"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b7d74a9f-d676-4f58-ad82-e207060d0d7d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b81981d7-c0ee-4b9a-b876-8c828165f6a2"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b86a0596-84e0-40fc-a583-e5b1bb4062cc"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b8938139-d10c-47e4-a773-f81621a5a4de"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b8a3fb30-14c7-4c33-a646-9c5daedd026a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b8ab13b6-f46b-404b-a849-37004f4ec893"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b90bd506-41bb-4466-a00e-3b78a0e15907"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b9399863-afd2-404a-9cd1-54b0ca54c273"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b995f724-0440-4702-8959-44d4c38c0cc4"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b9bf8b25-342d-4fda-a24c-15fac5040265"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b9ce2aab-8c81-494b-987e-c9331f1e43be"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ba17b076-2d26-43fc-9699-f3d465eb6822"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ba1d854e-a771-40ea-990d-bddb42106084"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ba2f9096-fe30-423f-b9cb-b5fc36c04361"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ba4aad2c-d49b-4b9b-ada9-441af75ec314"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ba7463e2-d78b-4a49-9d11-1cef8c53576d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("bb59b1cd-dc87-4a34-b058-a3dea82bd8ac"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("bb7e410b-2a29-40b3-bd0b-a000fcf9f3d3"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("bb7fe797-d547-4598-a706-8d868a5b2a2a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("bb85c7f1-514e-4ccd-8878-3e69e8392fe4"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("bb8e7b5a-f635-4e88-b96d-d10303f954ba"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("bbae8113-7dfb-4846-9443-b3bd1ee6fef6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("bbbed8b2-85c7-4562-8fba-fc0b283e94fe"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("bc117c81-102f-4b79-b6f3-b5f6bf313f9b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("bc778b4d-8f6b-4c76-be8e-f0104078c2e3"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("bd2f5b46-76dd-49ea-8dd8-7a0a6edf5ed3"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("bd4284d6-881f-4fd3-b55e-e237da6276f5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("bd6ba6a1-dd99-4d37-a4bd-cf2a42790cf5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("bd8c3173-8bc0-477f-a4d0-9c2034191ce8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("bd9b557c-ccf6-4de7-9a8a-dc92e9733863"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("bdba8802-72c0-468f-8752-538461aa0d7d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("bdc0b764-fdf2-49f8-bd42-82ce39b58e0a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("bde24b87-1127-48e0-8781-a67790cfaa29"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("be09de32-af0f-4ab9-aef3-3ccf11ac5517"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("be12f399-6502-4b1e-bbb0-563a6e2d826f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("be1f7bd9-45b0-449c-9d2d-9b03796a5780"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("be477e90-58b9-41cf-9bad-6a8cb6f02afb"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("be95ea6e-c145-4eba-a13e-db4ce46e513e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("bf13c995-0a10-4a93-81c7-3205da3e27c1"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("bf237e14-eefd-4df6-862d-cfb8676fd8c6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("bfbb0798-8883-43bd-a939-e7cdce91bfe2"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c0156b00-1ab6-4bbe-9d53-d2e5845723b9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c03c244d-18fc-482e-808e-d68340e46fe2"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c04980ab-ecec-4768-9190-352550d9d3d5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c0b43a2a-ee16-49c8-866c-a7606303bf3b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c0f9ceab-6821-4f25-a2f5-de645d8929cd"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c0fe602f-7947-4e21-a2c3-5a0f6173e8c5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c1287c1f-f94c-4b6b-8455-6ab4844e6ed9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c16edb28-1e08-42cb-97f6-360d63b0e7db"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c1af3251-14cf-4a2a-9813-ee407aa5256a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c1d57bc5-5f95-46c1-9480-e54b47053b8e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c1df4012-55f0-48ba-b0f4-b7d19f78ab29"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c1f0aa54-34f0-4413-b2fc-de970cc22afd"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c23841d6-c106-4237-b2f0-37c7948fb80a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c2794c3c-8bb2-4770-af97-693549b5393b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c28fae23-0941-47c5-a786-64f48c6b51de"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c31d8972-3acb-4561-82a9-c7802602d0a0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c35f94e5-9870-479d-98be-559c6ece63d3"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c361eeaa-9201-4463-bb9e-eb1c000d5088"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c389a18a-8355-4215-9b3c-598d2c24613c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c416fc0c-b976-47d1-853d-27a5600509ab"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c45b5cc2-0a39-44d9-ad2e-852e032c10bb"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c47f33ec-bad8-4b1f-b93f-d5ff4cb97a37"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c47f90cc-2677-4c9b-9642-3d1cbaa57551"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c4985fa2-4f47-4d4b-ad04-e62c41528f69"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c4e6c135-9bd1-450c-9b83-26aaa98b4f3c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c4f93439-2c82-41c0-92b5-2b6b7837b367"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c51568f3-30dc-445a-8b58-d322ebf7ec45"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c5271e45-4ec0-49a1-9e38-291bee29c595"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c53973ed-e05e-4bfa-b3b3-bc1031c481e9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c56f6875-4575-4518-bd70-fa64bbc0b0b1"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c5b96d63-e4b1-4083-8179-7ab9eeb5cdee"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c5c21379-212c-4299-bd59-22b894c535f5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c5d2f92b-dcbc-4688-acb5-21e0229497d4"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c5e49420-9dad-43d2-82d8-99a0fc1aa901"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c5ea4c27-88ee-4705-8001-2b24866b09b9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c63056cf-3af9-4c5f-a121-13c0f78c312d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c6dc4d6d-e6e6-4da9-8dd3-aa63a23a6394"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c6def189-29db-4752-8a04-0665e2022c30"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c6f1935a-5bcc-4623-a9fc-714a52f28b87"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c766a01a-f6a2-4072-966e-cb14dd605a15"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c7f4b2c1-62f5-45b2-9bdb-1afe5d1e4ce7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c7f8bae8-792d-47f4-9367-9d72db7fdb91"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c8012ead-438f-4d75-99f8-c1b9bc0c58ea"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c806d146-4e64-4d96-a054-c959d232ec47"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c84b5d7e-5b1e-4d12-b24a-6cf426bf3090"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c8b2d5a7-01a8-4b29-ae53-b5ff2d4a5ac9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c8d7188a-2b91-4344-96a5-c9656ff65bc8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c8db0527-5be2-40fd-b74b-26aeae0c29be"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c9018d7e-91e2-4834-bccd-13ab863f978f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c93002a8-46aa-4318-86ba-cb1ace5f0d7f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c9c395c7-dbd1-4a88-bef3-fdc7517e53d6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c9eea828-f4f6-442b-b4ab-4c768c9fbe08"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ca0661f7-c906-469f-9e07-03611b63a732"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ca2c8c0d-f51e-41f8-80e3-5b71e07bff09"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ca449a7c-7512-43bc-862a-6f1d838cd8d5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ca4e606c-90b3-4a61-bf57-20f828e58fe2"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ca74f0a9-1509-4cb5-bd7e-bc8a54f8f84f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("caa10698-4485-475a-9e87-c537fd8e4cc1"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("cab1f7ab-b05c-4bd3-95a2-801ccf1a51f3"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("cad2a545-70fd-43d2-b3c9-98e334554912"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("caea9ab2-4db7-4ef9-a5a5-8dc1b0be9b8e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("caeb3f43-056a-4b56-b94f-581f0696919c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("cb3e50c0-40a9-4b4e-9705-6f2008f56592"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("cb427f97-4d24-4cb6-86b9-86601c5b695b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("cb4505df-5542-4907-9019-038b339fbc79"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("cb992416-0831-47ca-8776-41edb36464a6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("cb9c27bf-2fb1-40c6-bde0-783a65f19c75"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("cbdc1c75-88f0-46b5-93eb-aca938af5ba0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("cbe6d563-eae4-4c9d-b891-a02295fadd3e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("cbfcc9d2-7d8b-4e6b-83c9-e708ff39574f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("cc4495cf-5ad0-45db-a54d-9f15e3e00a9d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("cce515d7-6e1b-403a-aee7-bd6a8684b891"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("cd1bdfbf-0690-4dd4-93c4-17fb286344d0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("cd3637d6-0101-4506-8af0-148373a58bda"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("cd3af8db-16de-4d40-9413-9edbbd632198"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("cdddf2a8-627c-4a16-8ce9-22a8e7771798"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ce15e970-f0e1-45b1-9628-fa5763dc19bf"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ce4a64c8-c238-42c1-8c59-c48ff7f46d9c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ce8302b7-508b-435b-95ed-451613e03a96"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("cf08921b-e8f8-4b20-9a93-34a9224cc5f5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("cf22fc43-3c60-4800-b742-d42dacccb33d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("cf397d0a-66ee-40e5-a670-64156f1fb572"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("cf9f4ee7-1a37-48d9-8e83-4dbc75a18a0e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("cfcf7ae5-3212-4ab3-a041-79d130b89112"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("cfe56b8b-9460-43fb-9596-f5508d92d6f2"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d005f440-b514-4fab-9143-5e21b45de47d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d006549b-a1ec-4bd7-83d2-1c70bfc46c47"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d02b6e5c-5881-403f-a017-6d8ad9982d5c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d0690fdf-edbd-40bc-8eda-f320f645f05f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d08fddbb-996a-42de-b978-8e44b3fc3c64"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d0b8e759-b2b5-4f51-affb-b768b2c26026"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d0c61f60-1c94-4c7c-a7a1-6ecb9639e516"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d0cbbab4-7449-4a90-a7f6-b2caa02616f7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d0f0178e-8d9b-4fc9-9726-4ca51d2657d7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d0f6ed5c-cccf-4fd5-9c88-e4f60c42677b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d0f820e4-17a6-4a63-a626-b9dd0f660da8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d13d8378-641b-487c-b5a2-afa8b218a655"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d167eca9-1b6a-49bf-a1fb-c6b397a8823e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d188c3a7-499e-4cdd-b4ed-a34e6c0ce08f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d1fcfb05-ebe0-471d-ae42-7ec550bc2769"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d217b5ec-2e3c-4a6c-90a4-a0f9d4776da9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d21cd1d1-4b97-4114-9163-d4df6ae35ee5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d23f7737-8ecd-4897-a8c5-c90fb6255c8d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d2554651-ecca-4991-b4ca-62f3a5290807"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d272a428-a91e-44c3-bdf2-15e2f51ef5c8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d2b7f2cc-3d16-4afa-9e4a-b0bd62fdc187"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d2f4140b-f3dc-4eef-946b-a9b909c8917f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d30bb32c-07c2-4668-b257-f58d9dacbfb8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d30f867e-3767-479a-b4b6-caa6ef587033"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d313d5fb-154d-4ffc-b012-408aa374b42d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d31e3eb4-4ea8-4881-b7c4-dc990bc207a0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d33d5ac6-fec4-4ee6-bec9-78f70f63580b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d34e49fd-01be-4f8e-8aca-e3be7bf356d3"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d35f3818-6992-4d32-be2c-c42c6fff61f8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d3b0d79b-4741-428b-824d-e98dd5d8d3c5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d403121c-79f6-4d3a-9088-149b0558e878"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d40c6393-f3a4-4282-9ec6-b912c1e56d7d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d40d8390-8abb-4109-a025-b81e9bc2a41c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d49391e7-b1c0-4f5b-98b8-abc5513d15e8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d4a6eff9-fbad-4f0c-9a2b-ae8a6362c794"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d4ed37cd-f314-4be1-a49a-9920389d8d20"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d4f0b29c-59db-4948-908a-f3dd4eb5a91f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d526c198-94dd-45b7-91b1-3c118c991072"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d554c863-efe9-4aa6-9777-d3aa0d30fc76"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d56187ae-6c84-4854-bd2c-c62d47bdc185"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d564a982-5196-4ad8-819d-a8ea17e85614"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d6612991-e472-4c52-910a-c2ff1111d326"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d6642c9e-617d-44df-bd10-cc6f25600279"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d6916503-d9da-4fdc-855f-b76c48dca98d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d6c65afa-98c8-4bfe-939a-a3b2723483f9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d75bd09b-4bad-48c3-8080-5c4c3e98298f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d7733b81-5436-44fe-9507-bf3b69789fdb"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d7768f73-c0c7-4c16-a225-2a64c5575f26"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d7871045-0a1e-47fa-af33-ca0a252f06fe"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d791d361-7477-498e-af46-6e1acf764418"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d81173c0-a593-4780-b913-9a9a75713d1c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d833e4a3-d668-4a46-96a8-885faccbf38a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d8936d39-17bc-4d18-984d-b9d06771755c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d8e8516a-70cf-4f07-b8c1-fb1e0a34b4f2"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d904bcf8-bd2c-499f-ab29-ed8ad11c048a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d9489cd4-c99a-4619-84f5-c802546e8c2f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d9505e72-082a-4772-97a9-4a1f060d2364"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d96d6983-7dab-44e4-8df7-ab4e90432a43"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d9c30724-4010-42dc-91e5-7f5293b57ce7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d9c38734-2b4d-45e2-bda4-bca3a44cf6f0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("da5fc0bc-2ae9-4311-ad51-27d374c6b63e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("da70c3cc-2649-4d80-84f8-904aa033a478"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("da8723e5-e152-4255-8b17-935794b396f8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("da8aaa46-2434-454d-b5a6-d052f37f85ed"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("da9e69c6-bafe-43c9-ac7b-325521a17dc7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("daaddafb-9e5b-407f-bbb8-ae64b3e77d32"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("daba812b-6b8d-4985-8e44-ffe68a678c2e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("dac12299-4bc1-4043-8a91-829f51b2b4ec"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("dad1f2f0-26f0-40ff-9880-69869471f600"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("db700fd7-0464-4588-aed6-bd815d3abef5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("db853097-40ad-4eec-942b-70ca70ed7a93"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("dbe03c07-c7c4-4b2e-b799-67a62e4e9b6a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("dbf19c97-0aa5-4a00-a04a-e082b21b4da2"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("dc40d307-15ac-49a5-8f83-9b0cc00cccdc"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("dc4f30ce-1f4c-4006-ba7c-9af5b6f1a6b7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("dc55af5e-cb8f-434c-bf04-7b1abbe7c084"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("dc8ab290-b5d3-4c0a-94b1-5775fc5af443"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("dc964b4a-3102-446b-be6d-27b6406bce49"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("dc977c5f-110e-4d31-a91f-8eb3a72e07c6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("dd303be9-b768-4f96-b0ef-62ee0ee05189"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("dd4ab15d-8181-4745-9fef-cd40a7db2595"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("dd51e366-6b2e-4df7-8a87-f6f882985cd9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("dd74d98d-279e-4d89-aeef-dba8242b1730"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("dd8326c9-1a45-4c8d-a324-c2c6f8ffd9e8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ddfc9379-0cd2-4d8f-8d60-9de421a6c5ae"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("de0ed51e-81ef-4d2a-b00f-a5494b320490"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("de42b10d-c9d1-4a89-8e0f-64f53bbf9487"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("de552b6c-448e-4d7e-b158-8b087a380983"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("de6c0a92-1dde-45b7-8eee-4f6e9d8bc2e7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("de87c813-c7fb-43ba-a4cc-ca0c73e75a29"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("de8a5425-60d9-43ee-a99a-1af8ea724d3d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("df00483a-ab33-4bb5-9ad6-f49bba14eb86"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("df012151-2665-4b3a-b0e0-2e06b3c3ba8d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("df0ad2cc-2384-4a1e-9665-d6a5a017b9c9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("df8ed884-d0a2-4ae1-8449-1c24c0134f75"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("df9507ea-c998-4a39-86ff-eade84e8165f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("dfaa8799-02cc-425f-b77d-78fa16f83cbb"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("dff9472f-9e0e-4194-9bfd-f1a3b47b1352"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e03a0e07-f459-4632-8091-d476961c2924"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e03f20da-f100-4653-845d-5988fe9426c4"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e0445402-2733-4aab-aa8e-1d96a03b10b9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e07d21a3-fb20-44b6-8ad2-59dc555f3093"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e0aea476-b488-4c6c-9dd1-83e18223b100"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e0aef8be-fc21-4a5e-9304-dbe38a6e0055"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e0b8b9fa-8b02-4252-bed8-0b7f6f68df1d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e0edc443-05cb-46ed-b3b6-02cd4ed01c48"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e0ee6c0b-d81d-400f-8a12-9dc4eb6309a8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e0f053bf-392b-42f6-aea9-60238c1c40f4"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e1090d4a-9c19-442c-bd8b-4f668a5346c4"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e1148d89-8e02-495a-9d6c-aeb15c397c20"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e12ba69b-a002-4515-9e49-2c9b862efb88"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e15a93b7-5296-4233-8855-68d9a8a56551"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e1858c75-659d-4542-9068-aeba745fb72a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e1a90632-070b-4d64-b02b-8452b667ca9f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e1c6a94f-3e3e-4c7a-81b7-06e59fd0522c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e20dbb12-143a-4f12-bef4-e60b0782ca70"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e235aa54-7c9a-4fa3-8e02-93d488b3f395"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e2c85faa-12d5-4bc2-814c-47a18bcef5aa"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e2d2c5a6-ea7f-47f9-aacd-37777f6eadef"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e2e11be9-30ab-4674-b2a5-9e8a202003c6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e2eea4f0-5351-4ac0-a1fc-208944fa06bf"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e3217a84-07a5-46d0-9f71-d88925c96a91"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e36116d4-7dcb-4156-a262-ed5841db46fa"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e3ec5822-212e-43a3-ac71-980258883a4e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e3ee0872-032e-43a5-a653-050406f50d78"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e3ef48af-7a82-4cd8-a7bf-3f15302179ad"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e43d7f10-5ffb-4f73-9e94-e96df292c24e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e476bedc-e9a0-4712-a9dd-fbbddcdb68e9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e485602f-75f3-483b-a031-f98d40dc3992"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e4c2c604-ed14-401f-a6d6-27e54a870f8d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e4de772e-08be-4568-92b4-0c6b8c414bc6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e5185420-9f8e-4c2a-b64e-075e95c74ae1"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e5407292-d96b-4370-abbd-6b0cc7126904"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e54e98b3-21c2-45f5-b771-727162ff46b7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e5687a38-03cf-42da-b605-f1f989fe47d5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e5b6f308-b651-4a0f-a1f1-a0672dba004f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e60edb18-4647-41bd-a999-9a8d7a27ddfc"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e65e88af-9a57-45c0-8ed4-06c86be83b05"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e6759210-0976-48a5-9e17-7d87c77f0a4e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e678940a-8ef9-420b-aa7e-4675f9924d7a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e7796247-305f-43d3-8e4f-8ed4ed5f3707"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e807718b-ccf8-4591-86b5-9210b45a9cf3"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e850fc06-9ab9-47ef-b1ac-f8cd71aee48e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e8573983-9da2-4de7-905c-ed6e99171156"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e87b5dff-9713-4ce6-8a3b-bbda20bf36c9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e894f3fa-6f5c-44c9-8be1-b69880a91087"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e89f6e45-73b2-4edc-a0b9-b5cc33bf03b7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e8a2e040-b79c-497d-b06a-c398d274d805"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e8ca0225-809f-4e97-a345-5ccb589ebccd"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e8da5a30-bd90-40a1-a08c-3924989970db"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e906cefa-7869-4c9d-a031-c9d0e057e330"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e949b371-bd92-41c6-8b2b-496cc2ee6f39"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e9568024-ca71-48b4-9df1-0d7d7da77b47"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e9752e40-4933-45ea-94c4-d430e505eb79"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e9bb45be-c962-4898-b85e-78e505beb3aa"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e9db1d2c-c092-4b96-959c-e6e49b3bf685"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e9e124f8-4172-4dbc-a361-92514ce7edc7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ea060f51-ec47-4f86-8d2b-16d0fb4f779d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ea1f1f16-b89d-4360-81de-dcdba78a10d1"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ea8b79de-7593-4ff8-93db-96dc03498c0e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("eab76fae-9f79-46c7-b45c-4fe045e2074d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("eaeba795-c510-4905-afca-4fa71b2fda23"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("eaed7f23-49c8-4a00-bf64-68b8255e49d5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("eaef7076-30e4-4284-89fc-aa08b0ccf41a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("eb162764-b8f3-4315-9d64-635d2bdc1959"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("eb65cb20-1345-4c92-a318-63f394503a52"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("eb79a44e-cd6e-4960-abda-a881e0851304"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("eba29847-22e9-4e6b-8cf7-3919971aaec6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ebb0c244-5a35-4a63-a3b7-d878648d774c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ebc3de0a-8a5e-4c35-8a55-53f12cd4110d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ebeadfa0-96d0-46ac-a6c6-015b2c37531c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ec17389d-4769-418b-a580-309f63b86abe"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ec394be2-d98d-469d-a775-095ada1fadfd"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ec58dc4b-e68c-4978-9193-d399c0bc9d7c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ec8763d0-b9b3-4105-8009-0c5a12d114d0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ecb7ec68-3a57-4174-8c33-201d7b535f45"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ece5432a-5102-466c-b17d-1e13f3d2a6c2"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ece6efe0-9e8f-4a24-910a-3f7de2c3d606"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ed0911a4-c826-4336-a02a-9b84f25496d6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ed0e81ec-819f-419a-addc-ac25fdb9b4db"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ed23419a-1083-4d82-a58e-87e370cdd556"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ed247101-f5e0-4d4b-a731-1c45857886de"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ed8d7fff-a9af-4fba-90e7-2f9e7ffe3a43"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ed969adf-13ba-4d70-99d7-fcbb28a29fe7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("edf17c48-b549-441e-87e4-08507e0961f8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ee464a46-6110-4639-b36e-8211a642618f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ee571a97-0208-4d35-a555-799ec1bbcd4a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ee6fb4fd-5e7b-44db-a13f-662881177d78"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ee82f812-1f99-4b75-85bb-ae8403ebdf60"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("eec6ab17-135c-4053-ad5a-1defbbaf98cc"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ef282e34-12b1-416a-960d-882038d985f9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ef43f5b9-514e-4f23-8c3b-3dcf8842d5c4"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ef4ab814-d8d5-4ddf-b959-28cc8ddc8c75"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ef4ca8c1-d5b9-45d8-a0a2-5e23870472cc"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ef5a08f1-06e8-493e-83a4-30ff51386132"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ef89f5fc-4b03-4d29-95cc-91b7d2c5245d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f047ad8b-5a1d-4593-b7c1-2fd5f956d4de"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f07c735b-5cf5-48b8-94a9-c08f66888996"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f0cb3e5b-2e19-4831-bb17-fcf0e823e7f9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f0df8572-edcc-498b-9d70-f8572d9c38dd"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f0e90200-11d9-4d22-a754-c698193f6ef8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f0eab488-e363-4323-995f-66f0326a2268"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f1116e13-0874-4326-816a-511fd77054f0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f11b4e59-1fe5-4456-9b40-4184719df21d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f124603a-2278-4ccf-a4a6-461226d5657d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f135879a-5ca7-49e8-bc17-2433ce6278ff"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f143c67f-be3b-42c6-ac46-7df141adb5c7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f19a9050-00c4-4367-a447-70f927c6f0c6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f1b68d8d-2655-4bd3-a52e-2dc688c4db9e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f1cc0730-3a1e-4ea3-9237-90ed6a4ebc82"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f1efb698-4cd8-4664-af7f-c0d06fef4779"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f205833e-389b-4f4d-8aed-0417fa4004d4"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f2065003-0b31-4e8a-aea3-851af15e0f1d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f223dd10-8ef7-4278-bbe3-dc6cd99986a7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f23449d0-38c9-4c35-8af9-c8543976aed3"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f23cb04d-8a5e-4b98-a620-ed37367e2d88"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f24805ed-2f25-43b7-8c96-12cbd465df94"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f25fc247-0efa-46e3-8e70-14d5b6192322"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f26da82b-6236-4f3e-9812-0d4a0bdaaec5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f28e0b68-7eff-4685-8e6f-d66bf242aa37"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f2d770c9-5854-474d-a240-5b3f633b82bb"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f302411a-8f80-4b2c-9450-c29c0e69accb"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f305d9a6-04af-40f5-becd-7bc5cab5bc4f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f318d464-ee13-4b24-b3a1-c8c382ac6a6f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f331ad21-e6b4-4925-a72d-300daf3e3f7e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f3572d73-a2e8-4b7e-beb2-f1e2d1b59c69"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f358a7ba-aade-4840-9e74-b1bc5753f570"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f35b6b01-4b43-481c-844d-c99d69df45f6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f36b3d42-2f37-46dc-8f34-ac8e43cb83eb"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f3709dba-1031-42a7-939c-d8122b184c0d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f385e867-e3fc-4bb9-9165-90fceb4e5c10"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f3921185-d2e0-4187-ab02-a80dbe5eb010"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f394e7c3-8cf4-4069-aaa1-2bc7cb4a0feb"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f395edab-b225-4a69-a63e-2d323c8fc233"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f39b4fd5-6b5a-41f9-a376-c5b382def9a7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f39c191d-310f-47f5-9096-da9d404cdc01"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f3a072d3-28f6-420a-8bfe-dd28194323ad"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f3a70137-9dc2-427f-a3a8-8f303db6d799"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f3d33379-798b-4805-bd47-dfdcd21b4876"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f415b417-eb73-40dc-94a8-c6e6db1b3041"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f4261555-e189-4145-a386-d1b48bed1dce"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f42b34b9-e6e6-4da5-aadf-c7f21aa91b2f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f436098c-d507-42e7-9e47-2125eb1e232f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f4508692-a6fb-437f-a5f2-b369f4efc55f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f495f25d-39c5-49fe-8382-c81ea52416d8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f4a62944-3672-4ea8-8a1a-e4a88079b03e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f4b2b856-79db-4391-bef6-2c2c20064db9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f4c8804d-3cdd-4467-9cc3-c33e319579f4"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f4e86f46-52fb-4029-9ea7-8f757ecb9e89"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f4e9f12b-7a7c-486a-bf1f-d9bfbcaf55c9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f4fb8e30-75a5-478f-8562-99794366a5dd"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f508fd2e-eea1-4360-ad42-3c5a29b8eb0c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f514142e-d9be-40ef-a2af-76377125c9d2"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f549136d-72e5-4597-a594-5449672c8dde"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f54b936d-a93d-46d2-9458-d7ce381707a0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f57675b4-6f0e-4de2-94eb-d1754d67aa59"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f593283c-66d2-48dc-8e13-7f0fc76c2686"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f5b2282e-e887-49d8-a9b8-38a030e662eb"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f604d521-76cb-4bf5-bc6f-1c828a7ad4bd"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f6107b8a-b2c4-43e7-9212-5bf7913bb590"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f629e740-71c4-4ea7-a0eb-3aa01dd911f3"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f6462c63-b140-44ff-a1fb-ab94c55d1da5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f6ec2749-41f2-4d85-a3ea-c5f0fb877a49"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f7033e45-b877-4cc9-ab81-00be6d83f568"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f7168e43-3705-41a1-b27b-182b876f5e48"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f71d2954-affe-4a39-b3da-f9b50ab58af6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f748042f-1d33-4f5c-b0eb-bec5f94f0925"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f7b525ce-ee6f-422d-9005-6f686c4c83d2"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f81ff0e0-3f39-48a8-bb70-6f6cefac57d8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f82e97f6-b2e8-427a-a18c-81e781cc03b3"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f8415194-0d45-48cd-b194-bc950f02ee6b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f896d8e4-7cb0-467f-b6ad-62daf8a45379"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f89a7c97-f5a1-4fc6-af19-9e76b9cabfa1"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f8c14f34-6d49-4a63-b5c6-589a10f349f9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f8db3950-de0e-44bc-9ce6-2dd27a17ceb8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f90c044f-e0f7-4d74-adf0-d49a67d20e98"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f94d3fe0-7619-4c5f-9b44-3bd3a25b3d2c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f9d879dc-6308-4c4e-9d45-7bed52ba8b7f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f9da902c-5e21-475e-9247-d740ada178fc"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f9fcb607-1012-4a2c-9c14-2542bbc435e2"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("fa2bbcc3-4020-4518-82e1-fea9b664e810"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("fa51c1b0-dc30-42c5-8213-9ddaeaff3e95"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("fa58d436-c996-4f44-b4ec-4af1c580ed8d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("fa7dda66-691e-455f-9cee-e269daa4b726"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("fa804c1c-7c4d-443f-8e3c-abf263c6c62a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("fae7e7d0-5a8a-409d-979c-869dec518f0a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("fae959a2-b028-4ba8-b05c-754e25798b2b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("fb1419d5-bac9-462f-9317-56e41239d5f0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("fb609ef8-b33a-4cf1-897e-95b3b512e51a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("fb69e684-ca64-4d1f-941c-198c896afe14"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("fba00902-86ed-4574-ac73-dbae282c67e6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("fbb5e2fe-b11b-46cf-a173-1e28ba95a9ec"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("fbeb8e3d-df5e-4620-8d22-68606f8fbce2"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("fbee8811-9c44-4e0a-8f6b-d324cc337bb0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("fc044c98-d459-4f9e-98e6-9d1bcad5546d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("fc75352d-d09e-4a31-9e2c-46681cb151e7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("fc930191-3a70-4ebc-8364-b31d72960634"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("fcc67ce9-d2f7-4ee9-bce4-b12f3a3b175c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("fceb10d4-2df2-420b-bc6d-03bbbd0fa971"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("fd090339-2970-4c3d-bcd0-3e3f3d02c95e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("fd1477b8-5152-4800-a9ff-2dc1a0ead5bb"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("fd14d677-32d3-491e-8e9c-7d0860d3b00b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("fd1b4b8b-ca2b-44d4-aa12-7f54db8cb24d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("fd44ffd8-b003-4fd1-ad70-b1dd58691efe"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("fd45a0ac-3a7d-46c9-baf7-b1789e371c48"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("fd48f74f-41a6-460a-8334-de1ba115de4a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("fd64b17f-adb0-4377-b525-bfb4d1aff8cc"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("fd7f0d93-3fa3-4acd-938e-e182e8722909"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("fe0ee47e-f87b-4651-9b05-5b3611fc9b40"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("fe84424c-f233-414b-a0f3-4d22c72c02fa"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ff3f23f1-d237-4028-8723-29d7c1e8f489"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ff481306-7d5b-4122-b46b-c6fee34abc43"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ff8b2f4d-fa71-48d5-a648-3e1474aabfb2"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ffb6663f-7034-4592-a149-adf0d5d49ee5"));

            migrationBuilder.DropColumn(
                name: "reference",
                table: "training_subjects");

            migrationBuilder.DropColumn(
                name: "citizen_names",
                table: "countries");

            migrationBuilder.DropColumn(
                name: "official_name",
                table: "countries");

            migrationBuilder.AlterColumn<string>(
                name: "training_country_id",
                table: "qualifications",
                type: "character varying(4)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "country_id",
                table: "countries",
                type: "character varying(4)",
                maxLength: 4,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.InsertData(
                table: "countries",
                columns: new[] { "country_id", "name" },
                values: new object[] { "UK", "United Kingdom" });

            migrationBuilder.InsertData(
                table: "training_subjects",
                columns: new[] { "training_subject_id", "is_active", "name" },
                values: new object[] { new Guid("02d718fb-2686-41ee-8819-79266b139ec7"), true, "Test subject" });
        }
    }
}
