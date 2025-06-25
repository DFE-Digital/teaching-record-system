using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingTrainingSubjectsAndCountries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "countries",
                columns: new[] { "country_id", "citizen_names", "name", "official_name" },
                values: new object[] { "HK", "Hongkonger or Cantonese", "Hong Kong", "Hong Kong Special Administrative Region of the People's Republic of China" });

            migrationBuilder.InsertData(
                table: "training_subjects",
                columns: new[] { "training_subject_id", "is_active", "name", "reference" },
                values: new object[,]
                {
                    { new Guid("00ac2bfd-091b-49c1-b785-04ca973cf974"), false, "Business Economics", "L1100" },
                    { new Guid("010d33db-a216-4c3e-bfb3-8c64f1126491"), false, "Manufacturing", "H700" },
                    { new Guid("0238f3f2-df59-477a-970a-23d06ce921e6"), false, "Games", "X2003" },
                    { new Guid("0328bf23-770a-4103-9645-e5efeb7ae8d8"), false, "Performing Arts", "W310" },
                    { new Guid("035d5c01-9ed4-43ac-be23-37f12d0c094d"), false, "Sport & exercise science", "C600" },
                    { new Guid("03abaddd-e1d2-4d98-ae33-e2872e0cb488"), false, "Business, Administration and Finance", "N990" },
                    { new Guid("03bb41de-a5d9-43ce-8632-489a34c98762"), false, "Social Work", "L8850" },
                    { new Guid("05168ad9-eedf-43c0-b141-e7ab26cab056"), false, "German", "R200" },
                    { new Guid("0523b697-4f5e-4f81-b04d-c2618bc0b218"), false, "Urban Studies", "K4600" },
                    { new Guid("0543a249-172a-4f0f-84c2-97123ca3f529"), false, "Pure Mathematics", "G1200" },
                    { new Guid("07526651-460a-49e4-be71-23d65e5c6ba1"), false, "Archaeology", "V6000" },
                    { new Guid("0807d6ed-2237-44fe-ac11-44cd70854c5c"), false, "Celtic Languages", "Q500" },
                    { new Guid("0de7e1fd-d679-4037-b0ec-244bfeb86159"), false, "Humanities", "Z0060" },
                    { new Guid("0e1cfcf8-d7ae-40a9-a09b-61f369648b9e"), false, "Engineering (Diploma)", "H990" },
                    { new Guid("10165d35-3e0d-4576-a2f1-b48bc79ac478"), false, "Welsh As A Second Language", "Q520" },
                    { new Guid("101e02c6-4634-46ad-89b2-b9a0d3d2d425"), false, "Genetics", "C4000" },
                    { new Guid("10d19613-df52-4667-8c8a-f97e90a98af0"), false, "Dummy Subject", "ZZ9999" },
                    { new Guid("1129fa2e-16bd-4ce6-a85e-30c5544571ca"), false, "Food Technology", "D4200" },
                    { new Guid("11910b21-f4d9-4211-a895-a158e2055d1f"), false, "Anthropology", "L6000" },
                    { new Guid("119caf30-8657-4658-b6d3-f9bfa3a7bb40"), false, "Italian", "R300" },
                    { new Guid("11fefa8c-c6bf-469b-afca-6cb9f355010d"), false, "Cinematics", "W5000" },
                    { new Guid("122a7153-b2af-4ba1-9b7c-688c040f6304"), false, "Social Anthropology", "L6001" },
                    { new Guid("13a673ca-3bc4-4e7e-a8cc-0ca0da648a81"), false, "Retail Business", "N900" },
                    { new Guid("13ac0aca-e015-4f48-a6e3-ddba998caa0f"), false, "Applied ICT", "I900" },
                    { new Guid("140251c7-4f16-4578-b09c-8a55e99dd8fa"), false, "Graphics", "W2102" },
                    { new Guid("14ac5701-c761-40e3-98f5-645b8e670bd3"), false, "Computer Science", "I100" },
                    { new Guid("15e98c51-fb05-4acf-a89f-c7a32ea8d77d"), false, "Audiology", "B6000" },
                    { new Guid("17f32ee4-bf01-4d3b-979e-137d0da56884"), false, "Management Studies", "N1100" },
                    { new Guid("18c1d74e-4b01-4ced-afb7-726b10a2c7fe"), false, "Creative and Media (Diploma)", "P900" },
                    { new Guid("18c42e4a-32ad-4c46-8f47-0d533512cfbe"), false, "Graphic Design", "W2100" },
                    { new Guid("18c68d1b-a6f8-4196-9896-f496e0e9acd1"), false, "Biochemistry", "C7000" },
                    { new Guid("18f5ab32-b0a3-4e73-ad69-99cef9a0d2af"), false, "Special Education Needs", "X161" },
                    { new Guid("19c96dff-0f10-4790-875e-9955c3a4aa81"), false, "Media Studies", "P300" },
                    { new Guid("1a0244d7-7c6e-475f-bea8-dd029c0f6de6"), false, "History Of Art", "V4000" },
                    { new Guid("1a1cec9c-b20e-4a00-ab88-0db2e9955b48"), false, "Painting", "W1100" },
                    { new Guid("1bb69e7b-1632-4d0d-bf38-e0dc522600f6"), false, "Sports Science", "X2009" },
                    { new Guid("1c199fdc-b262-4e8e-84de-45f999c2e3a5"), false, "Life Sciences", "C9100" },
                    { new Guid("1d160a84-a129-4a17-9020-f29dbeb0c13c"), false, "Sport & exercise science not elsewhere classified", "C690" },
                    { new Guid("1dca3d18-2e0a-4584-a3ee-294238acab74"), false, "Russian", "R700" },
                    { new Guid("1ef74550-5ca3-4b8d-864e-df3214a6187a"), false, "Applied Social Science", "L5001" },
                    { new Guid("1f0500fb-526b-499b-bdbf-a528247f1605"), false, "French Studies", "Z0102" },
                    { new Guid("20256fc9-048f-45af-a9bb-cbc3a2653976"), false, "Portuguese", "R5000" },
                    { new Guid("22e1b12a-c0d1-4da9-bf52-bd26f804eda1"), false, "Human Nutrition", "B4004" },
                    { new Guid("22f3c277-60dd-492a-8e89-910793f6b6a1"), false, "Theology", "V8001" },
                    { new Guid("231e041e-6bd2-4291-b231-6bb81c3e378c"), false, "Celtic Studies", "Q5000" },
                    { new Guid("268239ce-58a9-4e93-9d1d-6dfe70f597fa"), false, "Applied Mathematics", "G1100" },
                    { new Guid("27e40a69-cd31-4b59-9734-c86a76195077"), false, "Geology", "F6000" },
                    { new Guid("28d2b3ea-b6cc-4798-a3aa-bbf7deed2986"), false, "Information and Communications Technology", "G500" },
                    { new Guid("2c5166c0-156e-4761-becf-ff1e979edd8b"), false, "Computing Science", "G5006" },
                    { new Guid("2d1a3209-2e2b-45a3-9c15-1da3991075e2"), false, "Ancient language studies not elsewhere classified", "Q490" },
                    { new Guid("2e93acd2-bb31-489b-bd81-ae048dd3eeab"), false, "English As A Second Language", "Q3003" },
                    { new Guid("308a311e-2adf-4fc4-b776-83e8754bcbba"), false, "Building Technology", "K2100" },
                    { new Guid("308db47c-7294-4efe-8cf2-9510206234f6"), false, "Biological sciences (Combined/General Sciences)", "C900" },
                    { new Guid("314f08d0-0ec4-490b-9155-bde3ef22319a"), false, "Sport and Active Leisure", "N890" },
                    { new Guid("316e5ff6-2726-4e7e-b24a-3d6e724e463a"), false, "Environmental Geography", "L8003" },
                    { new Guid("324e2215-43c5-49b3-a7ff-8825a6a56618"), false, "Sports Studies", "X2002" },
                    { new Guid("3297cc84-ef28-459e-a677-98aeab40d6e2"), false, "Electrical Engineering", "H5000" },
                    { new Guid("3299f015-d343-4da4-bda3-eb52a8a84e31"), false, "Industrial Design", "W2302" },
                    { new Guid("348312fe-b121-42cd-b176-04810fb7b6c5"), false, "Bacteriology", "C5000" },
                    { new Guid("3568d883-64a0-4bd0-8023-0238447fd402"), false, "Fashion", "J4601" },
                    { new Guid("357306a9-01e3-49ec-a84c-b988996b7794"), false, "Nutrition", "B4000" },
                    { new Guid("360e84d7-f7b2-49d4-b3c0-a644cb0cd1bd"), false, "Home Economics", "N220" },
                    { new Guid("36885588-cafd-477e-a57d-dd513f378c11"), false, "Geography", "L700" },
                    { new Guid("39ea8e94-9ee1-4132-bf0a-97cb171a3c8f"), false, "Travel and Tourism", "N800" },
                    { new Guid("3cdf6ac7-ef7b-4623-b140-4d3bdfd953fb"), false, "Applied Biology", "C1100" },
                    { new Guid("3d68cafd-9943-4478-b1e5-cf04749df428"), false, "Early Years", "X110" },
                    { new Guid("3e05f006-d481-4af7-98af-17ff4199f0e4"), false, "Environmental Studies", "F9002" },
                    { new Guid("3e07d15f-be5c-440c-97dd-665fbca8d92a"), false, "Applied Biology", "C110" },
                    { new Guid("3e7409e6-d66a-4506-8ad3-ef9e24ba4c08"), false, "Classical Studies", "Z0031" },
                    { new Guid("3f2dcbb4-42e8-47d4-a5c1-03203be5d20a"), false, "Pharmaceutical Chemistry", "B9400" },
                    { new Guid("3f8565ba-426e-49f4-948e-b87bdb8e0c95"), false, "Environmental Biology", "C1600" },
                    { new Guid("435b8cf2-a928-4462-aeb2-85a8314e4a8b"), false, "Animal Science", "D2200" },
                    { new Guid("440351ce-b3c4-4741-806c-f0e90586ffaa"), false, "English As A Second Or Other Language", "Q330" },
                    { new Guid("443744a3-b0d6-45eb-bc4e-5d94b458936f"), false, "Photography", "W5500" },
                    { new Guid("44e3757e-64ac-416d-88c0-868857dd617f"), false, "Offshore Engineering", "H3600" },
                    { new Guid("477b6d3a-21ff-4bb2-a1a0-2c25e1e6e338"), false, "Museum Studies", "P1500" },
                    { new Guid("47b75dd9-62cd-4fd3-8f74-e6f7d19b6170"), false, "Primary Foundation", "X121" },
                    { new Guid("4a0e49c9-0e6e-491c-8e3e-daca26954215"), false, "Business Studies", "N100" },
                    { new Guid("4ab47749-da2a-4a70-935d-7aa72c53f277"), false, "Theology and Religious Studies", "V8880" },
                    { new Guid("4cb166c6-3467-43dc-b30d-49def64a0f22"), false, "History Of Music", "W3200" },
                    { new Guid("50432b25-e158-4b8c-aa22-05b423884474"), false, "Classical Greek Language", "Q710" },
                    { new Guid("5179e94b-3c9c-4fbb-b2ad-f4f9d67e2d89"), false, "Combined/General Science", "C000" },
                    { new Guid("53ac82df-04a9-4d66-8526-9e91a9130113"), false, "Human Biology", "B1500" },
                    { new Guid("54e4ab48-c2e4-43bc-801b-1c663637d6d8"), false, "Interior Design", "W2300" },
                    { new Guid("55bec1cf-3528-4bd5-94d4-8953d219c699"), false, "Dance", "W500" },
                    { new Guid("55f70db7-0d45-42b2-a4b1-694455afc9e7"), false, "Emotion and Motivation", "C8002" },
                    { new Guid("57b5a78f-71e0-4f85-81c7-0d9e51c4261b"), false, "American Studies", "Q4000" },
                    { new Guid("5b0cc7b1-4341-4706-9276-08cf8593c58c"), false, "Sculpture", "W1200" },
                    { new Guid("5c89579c-a863-4092-a640-f558dc7754fb"), false, "Applied Chemistry", "F1100" },
                    { new Guid("5f85f86f-64fb-4055-8c21-99a3d33797c0"), false, "Physical Education", "C600PE" },
                    { new Guid("60ce90cb-9e55-4735-8937-f0a26985b9d3"), false, "English as a second or other language", "999004" },
                    { new Guid("61646813-5e24-451e-9e2b-805de2577d42"), false, "Environmental and Land-based Studies", "F750" },
                    { new Guid("6177667e-a305-492f-9e56-29d8a612d731"), false, "Fine Art", "W8810" },
                    { new Guid("6350b867-dcd8-44e2-8aa9-5256be04c13f"), false, "English Studies", "Q3006" },
                    { new Guid("649d7736-d301-4c42-873a-b24486fd35d7"), false, "Physical Education", "999002" },
                    { new Guid("658abc1e-d723-430f-aa72-3d79cc53b9d0"), false, "Accountancy", "N4000" },
                    { new Guid("65e0db6e-6a0f-4743-aa2d-8eb15270506f"), false, "Financial Management", "N3000" },
                    { new Guid("6617e320-3731-4f5b-b94b-da23fdb0a417"), false, "Environmental Chemistry", "F140" },
                    { new Guid("66fc85d8-15f5-4654-8a02-84f425e7a571"), false, "Social Psychology", "L7400" },
                    { new Guid("670545e3-63f4-46af-b37d-ddfceb22e7ce"), false, "Creative and Media", "P390" },
                    { new Guid("674b877f-875c-4bc4-a246-7f05e04b1d80"), false, "Biological Chemistry", "C7001" },
                    { new Guid("67c3c91c-1c02-412a-897d-4b31fdef325b"), false, "Hospitality", "N862" },
                    { new Guid("67d5028a-1f6d-4b29-bf13-7db7f01d7f10"), false, "Ancient History", "V1100" },
                    { new Guid("68740ca9-570a-4a1a-b494-94988959c3ad"), false, "Medicine", "A3002" },
                    { new Guid("6889c95d-53f7-43ab-b410-da4bb85b761b"), false, "Physics with Maths", "F390" },
                    { new Guid("695d5a9a-3caf-4547-9097-a3e298dd0b00"), false, "Applied Chemistry", "F110" },
                    { new Guid("69a562cd-3220-4f2f-bece-e96a51e45032"), false, "Brewing", "J8001" },
                    { new Guid("6b621dd5-58fc-4e79-9c2f-a3f245cb17b9"), false, "Engineering", "H900" },
                    { new Guid("6b80a66e-bbfb-4d8f-9ca7-8f5a1225ea97"), false, "Philosophy", "V7000" },
                    { new Guid("6ca63e8f-bcb7-4bfc-8e1f-8e1cbe2395b7"), false, "General Studies", "Y4001" },
                    { new Guid("6cbe0a30-195c-4737-9758-089f283aa65d"), false, "Music", "W300" },
                    { new Guid("6cd526c4-5964-41a2-ba47-edfb69c9235f"), false, "Finance", "N3001" },
                    { new Guid("6cf7abc1-1f45-4f7b-a1e0-f34cb047f710"), false, "Zoology", "C3000" },
                    { new Guid("6d2e7516-7f58-486e-b879-5f200eaf64f8"), false, "Brazilian Studies", "R6000" },
                    { new Guid("6d7b6ee6-b092-4caa-8dde-08dfca8c1252"), false, "Humanities and Restricted Specialisms", "Y000" },
                    { new Guid("72afa95b-6037-465b-8891-4fa93d58c942"), false, "Byzantine History", "V1003" },
                    { new Guid("730de735-5303-474d-b79a-93ec566066dd"), false, "Design and Technology", "W200" },
                    { new Guid("74da0dee-6009-4bdb-b4a8-531c102b2949"), false, "Clinical Medicine", "A3000" },
                    { new Guid("75298232-873d-46d4-8073-8e6f44ccdb71"), false, "Food Science", "D4000" },
                    { new Guid("753b54c2-2ee5-4140-9f8f-18e7affbddb9"), false, "Food Technology", "D600" },
                    { new Guid("758fa901-b3a6-4010-93de-e5b471b6f0a6"), false, "Medieval History", "V1200" },
                    { new Guid("75d489dd-4f1a-4538-877f-ed79acdf97e0"), false, "Portuguese", "R500" },
                    { new Guid("7667249e-1128-4d23-8e33-f0ba582b9245"), false, "Agriculture", "D2000" },
                    { new Guid("784f5fc8-c4fd-4b20-9665-a2f5ed98fa5b"), false, "Applied Art and Design", "W990" },
                    { new Guid("78562ea9-d848-45e1-8453-a7c28d2a3684"), false, "Psychology", "Z0058" },
                    { new Guid("7934a041-f1f0-4cff-a28b-cc7ffdcb4ddf"), false, "Economic History", "V3000" },
                    { new Guid("7ab36e59-3c55-47e2-add4-ef1386707491"), false, "Chemistry", "F100" },
                    { new Guid("7b3ca799-036b-4e1f-a517-0748385b607f"), false, "Physics", "F300" },
                    { new Guid("7e1929b3-9a31-4c97-9796-263c5aeb8dff"), false, "Botany", "C2000" },
                    { new Guid("7fac9220-8908-4eab-a244-bc06463da525"), false, "Gujarati", "T5005" },
                    { new Guid("804f589c-ca50-495e-9195-2593f81a1a01"), false, "Health and Social Care", "L510" },
                    { new Guid("8101c1fe-06c7-4fe9-9771-6a0a4457b5d6"), false, "Accounting", "N4001" },
                    { new Guid("82ee964d-b2f7-4692-a98a-4acf90a6227b"), false, "Religious Education", "V600" },
                    { new Guid("8343aab3-e5bf-468c-8f3b-7cb1a6c92cf7"), false, "Applied Linguistics", "Q1100" },
                    { new Guid("84f721f3-12e2-4c77-93e4-9a5f785a2797"), false, "Materials", "J500" },
                    { new Guid("8618d636-62a3-484b-af8e-34b0853a9595"), false, "Applied Business", "N190" },
                    { new Guid("86bc6c04-2318-4b56-ba2a-aa66da2a632a"), false, "Modern History", "V1300" },
                    { new Guid("884424ab-1eeb-450d-a6e0-cf2b69fdb539"), false, "Media Studies", "P4000" },
                    { new Guid("88946a0d-f633-4bac-945e-c6cc2c4c45b5"), false, "Information and Communications Technology", "I200" },
                    { new Guid("89afb187-1d2b-4d41-9e0b-8e3ed17c2c0c"), false, "Linguistics", "Q1000" },
                    { new Guid("8a52025c-5ad8-4c17-84d9-8f25a98e2007"), false, "English Literature", "Q3001" },
                    { new Guid("8c273472-006e-4b5c-ba54-8a343526326a"), false, "Textiles", "J4100" },
                    { new Guid("8c7c72a7-fc62-45a8-a197-e866105008bd"), false, "Librarianship", "P1000" },
                    { new Guid("8e750d7b-7be2-495a-9196-ce1700cd7671"), false, "Printmaking", "W6700" },
                    { new Guid("8f3f9881-03eb-4270-aab7-211b48ff1fe9"), false, "English History", "V1400" },
                    { new Guid("90e76e2e-3753-40d8-8f98-b9f886e3d9e0"), false, "Dietetics", "B4001" },
                    { new Guid("91428fec-6a8f-4bd5-9913-ebe7dff6dc31"), false, "Dutch", "T2200" },
                    { new Guid("923de12e-cb98-4aa8-9e63-8499ba0e5264"), false, "Citizenship", "L230" },
                    { new Guid("9305a485-bfc2-4c7f-9f24-6186d29b5d93"), false, "Materials Science", "F2000" },
                    { new Guid("948c9092-ff5d-43f9-ad24-c81de57c9379"), false, "Textile Design", "W2200" },
                    { new Guid("94e13788-d55d-4d0a-8c7a-ef6b0c94257b"), false, "Hair and Beauty", "B990" },
                    { new Guid("95ec5c7d-dd43-4f7e-9791-ee9daaa0a977"), false, "Biology", "C100" },
                    { new Guid("95f2bd8b-1d45-463e-aa30-2b0240783a88"), false, "Recreation & leisure studies", "N870" },
                    { new Guid("96db8451-abf0-49a6-bd18-24eb0b3731d2"), false, "European History", "V1001" },
                    { new Guid("971a7967-17e4-4807-9efa-57f5c57d36b8"), false, "Chemical Physics", "F3300" },
                    { new Guid("98348fed-53ce-484f-9146-7ea03aae7b64"), false, "Performing Arts", "W4300" },
                    { new Guid("9a219104-6cf5-47c0-88b4-94bcbba724c0"), false, "Combined Studies", "Y4101" },
                    { new Guid("9a34ba9e-e6b0-4748-aa70-1e28ce3fd0f1"), false, "Society, Health and Development", "L990" },
                    { new Guid("9a60669a-f507-4548-a361-8ef1f898635d"), false, "History", "V100" },
                    { new Guid("9bf0ea21-c8e5-4ed4-b59e-6ca39c9b389b"), false, "Textiles", "J420" },
                    { new Guid("9caa584a-bb89-450d-8d8d-16ba0e84e28e"), false, "Citizenship", "999001" },
                    { new Guid("9db68509-e882-4b0e-8e97-a7d09e51d059"), false, "Educational Psychology", "L7500" },
                    { new Guid("9e195a7b-3d32-44d4-a889-a4ffce932cd2"), false, "Theatre Studies", "W4400" },
                    { new Guid("9f3ac919-3a14-4616-9117-0bc5d5c90fbc"), false, "Applied Psychology", "C8100" },
                    { new Guid("a18c817a-6e6b-46ae-b7e1-4cae51fbefe6"), false, "Cultural Studies", "L6200" },
                    { new Guid("a1c4a34d-e37f-416e-8238-ebf7c1a4f88f"), false, "Criminology", "L3900" },
                    { new Guid("a28b3011-a907-4d7d-955b-1ec13937d1a9"), false, "Rehabilitation Studies", "L3404" },
                    { new Guid("a3a755bc-c46c-4db2-bb5d-3a2f517b5ee7"), false, "Microbiology", "C5001" },
                    { new Guid("a3f24be9-543c-4a60-8e87-1d2fbf909b7b"), false, "Combined/General Science", "F000" },
                    { new Guid("a53281e2-b459-47f8-81ec-4045cb1838e8"), false, "Information Technology", "G5601" },
                    { new Guid("a6474c68-6473-4b78-a9fc-96bb2c8531b5"), false, "Computer Science", "G5001" },
                    { new Guid("a65eef4e-de3a-481e-8b0c-a2fd20201716"), false, "British History", "V1406" },
                    { new Guid("af4ac22b-45fc-4940-bdea-720f744b16a0"), false, "Psycholinguistics", "Q1600" },
                    { new Guid("af689ace-b7b4-450e-b970-718023470056"), false, "Statistics", "G4000" },
                    { new Guid("b045b29e-4748-4226-9067-df9bcfa8a4dd"), false, "General Primary", "X120" },
                    { new Guid("b1dccac7-ed0c-4e0f-ba2e-6450e0de287e"), false, "Advanced Study Of Early Years", "X900" },
                    { new Guid("b397a256-5a83-484e-ac4c-3821eba09c51"), false, "Mathematics", "G100" },
                    { new Guid("b3ed7edc-48ed-4f47-815e-378279adef10"), false, "Religious Studies", "V8000" },
                    { new Guid("b6f2e64a-e6d1-4cb7-ac4d-14a463322f45"), false, "Drawing", "W1007" },
                    { new Guid("b79bd83b-1bfb-4bf9-8dc4-a118e1540b04"), false, "English", "Q300" },
                    { new Guid("b7e5969a-e774-407b-a5b0-c7a26362dfdf"), false, "Biblical Studies", "V8200" },
                    { new Guid("b94f26df-8d52-47fa-a978-3a3fca9eaf06"), false, "Theoretical Physics", "F3201" },
                    { new Guid("b9b6c9d4-d093-4d6d-9a70-9679d019025e"), false, "Phse", "L390" },
                    { new Guid("b9eaf229-74de-4bb6-9122-bd6cdf82b2c0"), false, "Austrian", "ZZ9000" },
                    { new Guid("ba535897-21ef-4d3e-9c13-17b23c6ee4eb"), false, "Caribbean Studies", "T9001" },
                    { new Guid("bc4b74cf-1262-49d2-9bb0-0d92482eba8b"), false, "Law", "M990" },
                    { new Guid("bf7d5681-1bdb-4c77-8c75-9681e835dd5c"), false, "Health Studies", "B9003" },
                    { new Guid("bfa75e7f-2552-45ea-bb78-a5ea9c00310e"), false, "Training teachers - specialist", "X160" },
                    { new Guid("c08fc7b8-2f1d-4397-b9d6-4f00715b7328"), false, "Mechanics", "F3007" },
                    { new Guid("c1e22bb8-533a-4689-8537-0a09b09d0aaa"), false, "Applied Physics", "F310" },
                    { new Guid("c2b29f64-673a-4f0b-bc34-de35c8b02f27"), false, "Latin Language", "Q610" },
                    { new Guid("c3c4100a-8b46-4112-ada9-61302b436f3b"), false, "Natural Sciences", "Y1600" },
                    { new Guid("c4c69938-9c5c-46dc-a528-ceb6791011de"), false, "Modern Languages", "T2004" },
                    { new Guid("c73984dd-572a-4eb1-91f7-12a6e01b9ba9"), false, "Law", "M200" },
                    { new Guid("c88e2d61-af9e-48a3-8d76-0b06bcdb9599"), false, "Physiology", "B1000" },
                    { new Guid("cc74eb90-5bd1-4f61-8079-1ce19fdc7205"), false, "International Politics", "M1500" },
                    { new Guid("cddccad7-fe95-4f83-b1b5-bc08f9c0c394"), false, "Human Geography", "L8200" },
                    { new Guid("ce63a226-e135-4e7e-951c-53bffc456ef7"), false, "Archive Studies", "P1600" },
                    { new Guid("ce7d5f85-8830-4401-bd38-06a14831c799"), false, "Welsh Literature", "Q5203" },
                    { new Guid("ce889e64-a01c-4f57-8121-52356fc9e096"), false, "Business Administration", "N1200" },
                    { new Guid("ce959395-4633-4e19-b001-9a475ae67653"), false, "Inorganic Chemistry", "F1901" },
                    { new Guid("d1c608d4-3490-41d3-914f-89deff0aab74"), false, "Physical Geography", "F8400" },
                    { new Guid("d1e609da-a1b6-4c46-8665-3b9352c33734"), false, "Experimental Psychology", "C8001" },
                    { new Guid("d204118b-41f8-462c-875a-73012272c126"), false, "Public Services", "L430" },
                    { new Guid("d227cd41-132e-4143-a17c-40313d348af3"), false, "French", "R100" },
                    { new Guid("d24cd9b5-8134-4877-af1f-177a37bbc030"), false, "Development Studies", "M9200" },
                    { new Guid("d2b10830-695b-48f5-ac2a-51e30f5f0af9"), false, "Phonetics", "Q1300" },
                    { new Guid("d3d9d21f-fe99-4110-97e4-e0a8bc66880b"), false, "Economics", "L100" },
                    { new Guid("d4596df0-10aa-4eb5-9169-daaf7d83c6bd"), false, "French Literature", "R1102" },
                    { new Guid("d47f0c82-af98-47d6-a521-839a0b9b810b"), false, "Classics", "Q800" },
                    { new Guid("d63080e2-0473-4b02-aa29-9bf7c93fa9da"), false, "German Literature", "R2101" },
                    { new Guid("d9a1e6c9-3afd-41cc-807d-ed197d03d3e3"), false, "European Studies", "T2000" },
                    { new Guid("da17ea40-3fc7-45ea-9e4c-e6d056098294"), false, "Welsh", "Q560" },
                    { new Guid("da5c71cf-5011-452c-ac9c-e86c407b43f5"), false, "Divinity", "V8002" },
                    { new Guid("dad16907-e3ef-455a-a409-4b51f92615f4"), false, "Chinese", "ZZ9002" },
                    { new Guid("db15d068-5529-4242-8f85-0c18ecba5c9f"), false, "Sociology", "L3000" },
                    { new Guid("ddd0d723-8226-4b3a-81a7-1293474fc3c7"), false, "Other Modern Language", "R900" },
                    { new Guid("e167c3c7-cd26-42d1-9213-20a01c2e8c26"), false, "Careers Education", "L500" },
                    { new Guid("e2e7be55-7a68-41b0-8957-ff49d8a7bcc3"), false, "Visual Communication", "W1501" },
                    { new Guid("e5e63d47-c866-4fd3-b1ef-ff2f3023e11e"), false, "Design Studies", "W2000" },
                    { new Guid("e68cd222-b101-4fcd-b023-f513dc7e11b7"), false, "Academic Studies In Education", "X8830" },
                    { new Guid("e8fa2a9d-39b4-46ec-b0b7-d0b4daa41498"), false, "Journalism", "P4500" },
                    { new Guid("e92a3d96-7b7f-4778-8cf6-9ee96e75e4f8"), false, "Physical sciences (Combined/General Sciences)", "F900" },
                    { new Guid("e9847152-9e38-4cfb-ae6e-54635d6e004a"), false, "General Science", "Y1002" },
                    { new Guid("eba938fc-cd46-4032-80f6-682bf0d06c6d"), false, "Ceramics", "J3001" },
                    { new Guid("ec16d283-cd7c-49e2-a8d0-c82c7b1ec03a"), false, "Philology", "Q1402" },
                    { new Guid("ed4acaa0-8d31-4fd2-ad5f-4d7ae7abbe22"), false, "English Language", "Q3005" },
                    { new Guid("ed4f50d2-2780-4b13-9e19-28a82f602d31"), false, "Ecology", "C9000" },
                    { new Guid("ed7ea186-a794-4439-8667-cdf2d00cb2a7"), false, "Psychology", "C800" },
                    { new Guid("edd3e60e-676c-4967-bbe4-2c0f4b14ed71"), false, "Politics", "M1000" },
                    { new Guid("ee1d7f99-577b-4bce-9bf4-37e333a0302b"), false, "Environmental Science", "F9000" },
                    { new Guid("eea5b18c-8165-4b32-87d9-ff93755609f6"), false, "Art", "W900" },
                    { new Guid("f329b47b-45ed-460f-8620-3bf30916e7ae"), false, "Horticulture", "D2500" },
                    { new Guid("f3673e45-c78c-4e1b-856e-9286efff456d"), false, "Graphics", "W210" },
                    { new Guid("f3b3f9f5-2d14-44a0-bd56-b797df3dd77c"), false, "Spanish", "R400" },
                    { new Guid("f3e91599-2a2e-4f81-b4e0-9098a1ce8ec7"), false, "Design and technology", "999003" },
                    { new Guid("f51e3ba8-7844-40c3-bbd5-8d17ddf439f0"), false, "Manufacturing and Product Design", "H790" },
                    { new Guid("f5a66fd5-5488-4462-8eee-6330640703d9"), false, "Marketing", "N5000" },
                    { new Guid("f5bd64ad-7941-47da-aed7-10470d49670c"), false, "Needlecraft", "W9005" },
                    { new Guid("f797e941-4ac2-466d-b08d-1ebad9b7d178"), false, "Drama", "W400" },
                    { new Guid("fa645de7-8767-49c5-9fd2-5a393c914755"), false, "Welsh Studies", "Q5204" },
                    { new Guid("fb0e95e8-6c93-4700-a755-1f46b95b1e72"), false, "Social Policy", "L4200" },
                    { new Guid("fb2a1c5c-94c5-4695-a4da-1a5adb8d2420"), false, "Institutional Management", "N7000" },
                    { new Guid("fc515e31-5e08-40f6-bf72-3aefbb5419d6"), false, "Construction and the Built Environment", "K290" },
                    { new Guid("fca2b0af-5ca1-48ee-829b-df31bb53bd5d"), false, "Social Sciences/Social Studies", "L900" },
                    { new Guid("fd7f7073-488a-4816-a1d4-336928ac62b7"), false, "Biophysical Science", "C6001" },
                    { new Guid("febe507d-ebbd-4b75-a401-442d42c3f54f"), false, "Ethics", "V7603" },
                    { new Guid("feee2354-fb8a-442a-ac5d-9bc35582ce64"), false, "Computing", "G5003" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "countries",
                keyColumn: "country_id",
                keyValue: "HK");

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("00ac2bfd-091b-49c1-b785-04ca973cf974"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("010d33db-a216-4c3e-bfb3-8c64f1126491"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("0238f3f2-df59-477a-970a-23d06ce921e6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("0328bf23-770a-4103-9645-e5efeb7ae8d8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("035d5c01-9ed4-43ac-be23-37f12d0c094d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("03abaddd-e1d2-4d98-ae33-e2872e0cb488"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("03bb41de-a5d9-43ce-8632-489a34c98762"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("05168ad9-eedf-43c0-b141-e7ab26cab056"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("0523b697-4f5e-4f81-b04d-c2618bc0b218"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("0543a249-172a-4f0f-84c2-97123ca3f529"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("07526651-460a-49e4-be71-23d65e5c6ba1"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("0807d6ed-2237-44fe-ac11-44cd70854c5c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("0de7e1fd-d679-4037-b0ec-244bfeb86159"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("0e1cfcf8-d7ae-40a9-a09b-61f369648b9e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("10165d35-3e0d-4576-a2f1-b48bc79ac478"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("101e02c6-4634-46ad-89b2-b9a0d3d2d425"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("10d19613-df52-4667-8c8a-f97e90a98af0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1129fa2e-16bd-4ce6-a85e-30c5544571ca"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("11910b21-f4d9-4211-a895-a158e2055d1f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("119caf30-8657-4658-b6d3-f9bfa3a7bb40"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("11fefa8c-c6bf-469b-afca-6cb9f355010d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("122a7153-b2af-4ba1-9b7c-688c040f6304"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("13a673ca-3bc4-4e7e-a8cc-0ca0da648a81"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("13ac0aca-e015-4f48-a6e3-ddba998caa0f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("140251c7-4f16-4578-b09c-8a55e99dd8fa"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("14ac5701-c761-40e3-98f5-645b8e670bd3"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("15e98c51-fb05-4acf-a89f-c7a32ea8d77d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("17f32ee4-bf01-4d3b-979e-137d0da56884"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("18c1d74e-4b01-4ced-afb7-726b10a2c7fe"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("18c42e4a-32ad-4c46-8f47-0d533512cfbe"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("18c68d1b-a6f8-4196-9896-f496e0e9acd1"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("18f5ab32-b0a3-4e73-ad69-99cef9a0d2af"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("19c96dff-0f10-4790-875e-9955c3a4aa81"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1a0244d7-7c6e-475f-bea8-dd029c0f6de6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1a1cec9c-b20e-4a00-ab88-0db2e9955b48"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1bb69e7b-1632-4d0d-bf38-e0dc522600f6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1c199fdc-b262-4e8e-84de-45f999c2e3a5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1d160a84-a129-4a17-9020-f29dbeb0c13c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1dca3d18-2e0a-4584-a3ee-294238acab74"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1ef74550-5ca3-4b8d-864e-df3214a6187a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("1f0500fb-526b-499b-bdbf-a528247f1605"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("20256fc9-048f-45af-a9bb-cbc3a2653976"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("22e1b12a-c0d1-4da9-bf52-bd26f804eda1"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("22f3c277-60dd-492a-8e89-910793f6b6a1"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("231e041e-6bd2-4291-b231-6bb81c3e378c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("268239ce-58a9-4e93-9d1d-6dfe70f597fa"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("27e40a69-cd31-4b59-9734-c86a76195077"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("28d2b3ea-b6cc-4798-a3aa-bbf7deed2986"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2c5166c0-156e-4761-becf-ff1e979edd8b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2d1a3209-2e2b-45a3-9c15-1da3991075e2"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("2e93acd2-bb31-489b-bd81-ae048dd3eeab"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("308a311e-2adf-4fc4-b776-83e8754bcbba"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("308db47c-7294-4efe-8cf2-9510206234f6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("314f08d0-0ec4-490b-9155-bde3ef22319a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("316e5ff6-2726-4e7e-b24a-3d6e724e463a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("324e2215-43c5-49b3-a7ff-8825a6a56618"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3297cc84-ef28-459e-a677-98aeab40d6e2"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3299f015-d343-4da4-bda3-eb52a8a84e31"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("348312fe-b121-42cd-b176-04810fb7b6c5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3568d883-64a0-4bd0-8023-0238447fd402"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("357306a9-01e3-49ec-a84c-b988996b7794"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("360e84d7-f7b2-49d4-b3c0-a644cb0cd1bd"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("36885588-cafd-477e-a57d-dd513f378c11"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("39ea8e94-9ee1-4132-bf0a-97cb171a3c8f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3cdf6ac7-ef7b-4623-b140-4d3bdfd953fb"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3d68cafd-9943-4478-b1e5-cf04749df428"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3e05f006-d481-4af7-98af-17ff4199f0e4"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3e07d15f-be5c-440c-97dd-665fbca8d92a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3e7409e6-d66a-4506-8ad3-ef9e24ba4c08"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3f2dcbb4-42e8-47d4-a5c1-03203be5d20a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("3f8565ba-426e-49f4-948e-b87bdb8e0c95"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("435b8cf2-a928-4462-aeb2-85a8314e4a8b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("440351ce-b3c4-4741-806c-f0e90586ffaa"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("443744a3-b0d6-45eb-bc4e-5d94b458936f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("44e3757e-64ac-416d-88c0-868857dd617f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("477b6d3a-21ff-4bb2-a1a0-2c25e1e6e338"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("47b75dd9-62cd-4fd3-8f74-e6f7d19b6170"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("4a0e49c9-0e6e-491c-8e3e-daca26954215"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("4ab47749-da2a-4a70-935d-7aa72c53f277"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("4cb166c6-3467-43dc-b30d-49def64a0f22"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("50432b25-e158-4b8c-aa22-05b423884474"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5179e94b-3c9c-4fbb-b2ad-f4f9d67e2d89"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("53ac82df-04a9-4d66-8526-9e91a9130113"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("54e4ab48-c2e4-43bc-801b-1c663637d6d8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("55bec1cf-3528-4bd5-94d4-8953d219c699"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("55f70db7-0d45-42b2-a4b1-694455afc9e7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("57b5a78f-71e0-4f85-81c7-0d9e51c4261b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5b0cc7b1-4341-4706-9276-08cf8593c58c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5c89579c-a863-4092-a640-f558dc7754fb"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("5f85f86f-64fb-4055-8c21-99a3d33797c0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("60ce90cb-9e55-4735-8937-f0a26985b9d3"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("61646813-5e24-451e-9e2b-805de2577d42"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6177667e-a305-492f-9e56-29d8a612d731"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6350b867-dcd8-44e2-8aa9-5256be04c13f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("649d7736-d301-4c42-873a-b24486fd35d7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("658abc1e-d723-430f-aa72-3d79cc53b9d0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("65e0db6e-6a0f-4743-aa2d-8eb15270506f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6617e320-3731-4f5b-b94b-da23fdb0a417"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("66fc85d8-15f5-4654-8a02-84f425e7a571"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("670545e3-63f4-46af-b37d-ddfceb22e7ce"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("674b877f-875c-4bc4-a246-7f05e04b1d80"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("67c3c91c-1c02-412a-897d-4b31fdef325b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("67d5028a-1f6d-4b29-bf13-7db7f01d7f10"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("68740ca9-570a-4a1a-b494-94988959c3ad"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6889c95d-53f7-43ab-b410-da4bb85b761b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("695d5a9a-3caf-4547-9097-a3e298dd0b00"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("69a562cd-3220-4f2f-bece-e96a51e45032"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6b621dd5-58fc-4e79-9c2f-a3f245cb17b9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6b80a66e-bbfb-4d8f-9ca7-8f5a1225ea97"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6ca63e8f-bcb7-4bfc-8e1f-8e1cbe2395b7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6cbe0a30-195c-4737-9758-089f283aa65d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6cd526c4-5964-41a2-ba47-edfb69c9235f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6cf7abc1-1f45-4f7b-a1e0-f34cb047f710"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6d2e7516-7f58-486e-b879-5f200eaf64f8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("6d7b6ee6-b092-4caa-8dde-08dfca8c1252"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("72afa95b-6037-465b-8891-4fa93d58c942"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("730de735-5303-474d-b79a-93ec566066dd"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("74da0dee-6009-4bdb-b4a8-531c102b2949"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("75298232-873d-46d4-8073-8e6f44ccdb71"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("753b54c2-2ee5-4140-9f8f-18e7affbddb9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("758fa901-b3a6-4010-93de-e5b471b6f0a6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("75d489dd-4f1a-4538-877f-ed79acdf97e0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7667249e-1128-4d23-8e33-f0ba582b9245"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("784f5fc8-c4fd-4b20-9665-a2f5ed98fa5b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("78562ea9-d848-45e1-8453-a7c28d2a3684"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7934a041-f1f0-4cff-a28b-cc7ffdcb4ddf"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7ab36e59-3c55-47e2-add4-ef1386707491"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7b3ca799-036b-4e1f-a517-0748385b607f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7e1929b3-9a31-4c97-9796-263c5aeb8dff"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("7fac9220-8908-4eab-a244-bc06463da525"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("804f589c-ca50-495e-9195-2593f81a1a01"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8101c1fe-06c7-4fe9-9771-6a0a4457b5d6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("82ee964d-b2f7-4692-a98a-4acf90a6227b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8343aab3-e5bf-468c-8f3b-7cb1a6c92cf7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("84f721f3-12e2-4c77-93e4-9a5f785a2797"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8618d636-62a3-484b-af8e-34b0853a9595"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("86bc6c04-2318-4b56-ba2a-aa66da2a632a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("884424ab-1eeb-450d-a6e0-cf2b69fdb539"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("88946a0d-f633-4bac-945e-c6cc2c4c45b5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("89afb187-1d2b-4d41-9e0b-8e3ed17c2c0c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8a52025c-5ad8-4c17-84d9-8f25a98e2007"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8c273472-006e-4b5c-ba54-8a343526326a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8c7c72a7-fc62-45a8-a197-e866105008bd"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8e750d7b-7be2-495a-9196-ce1700cd7671"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("8f3f9881-03eb-4270-aab7-211b48ff1fe9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("90e76e2e-3753-40d8-8f98-b9f886e3d9e0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("91428fec-6a8f-4bd5-9913-ebe7dff6dc31"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("923de12e-cb98-4aa8-9e63-8499ba0e5264"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9305a485-bfc2-4c7f-9f24-6186d29b5d93"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("948c9092-ff5d-43f9-ad24-c81de57c9379"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("94e13788-d55d-4d0a-8c7a-ef6b0c94257b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("95ec5c7d-dd43-4f7e-9791-ee9daaa0a977"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("95f2bd8b-1d45-463e-aa30-2b0240783a88"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("96db8451-abf0-49a6-bd18-24eb0b3731d2"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("971a7967-17e4-4807-9efa-57f5c57d36b8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("98348fed-53ce-484f-9146-7ea03aae7b64"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9a219104-6cf5-47c0-88b4-94bcbba724c0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9a34ba9e-e6b0-4748-aa70-1e28ce3fd0f1"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9a60669a-f507-4548-a361-8ef1f898635d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9bf0ea21-c8e5-4ed4-b59e-6ca39c9b389b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9caa584a-bb89-450d-8d8d-16ba0e84e28e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9db68509-e882-4b0e-8e97-a7d09e51d059"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9e195a7b-3d32-44d4-a889-a4ffce932cd2"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("9f3ac919-3a14-4616-9117-0bc5d5c90fbc"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a18c817a-6e6b-46ae-b7e1-4cae51fbefe6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a1c4a34d-e37f-416e-8238-ebf7c1a4f88f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a28b3011-a907-4d7d-955b-1ec13937d1a9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a3a755bc-c46c-4db2-bb5d-3a2f517b5ee7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a3f24be9-543c-4a60-8e87-1d2fbf909b7b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a53281e2-b459-47f8-81ec-4045cb1838e8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a6474c68-6473-4b78-a9fc-96bb2c8531b5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("a65eef4e-de3a-481e-8b0c-a2fd20201716"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("af4ac22b-45fc-4940-bdea-720f744b16a0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("af689ace-b7b4-450e-b970-718023470056"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b045b29e-4748-4226-9067-df9bcfa8a4dd"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b1dccac7-ed0c-4e0f-ba2e-6450e0de287e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b397a256-5a83-484e-ac4c-3821eba09c51"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b3ed7edc-48ed-4f47-815e-378279adef10"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b6f2e64a-e6d1-4cb7-ac4d-14a463322f45"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b79bd83b-1bfb-4bf9-8dc4-a118e1540b04"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b7e5969a-e774-407b-a5b0-c7a26362dfdf"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b94f26df-8d52-47fa-a978-3a3fca9eaf06"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b9b6c9d4-d093-4d6d-9a70-9679d019025e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("b9eaf229-74de-4bb6-9122-bd6cdf82b2c0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ba535897-21ef-4d3e-9c13-17b23c6ee4eb"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("bc4b74cf-1262-49d2-9bb0-0d92482eba8b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("bf7d5681-1bdb-4c77-8c75-9681e835dd5c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("bfa75e7f-2552-45ea-bb78-a5ea9c00310e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c08fc7b8-2f1d-4397-b9d6-4f00715b7328"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c1e22bb8-533a-4689-8537-0a09b09d0aaa"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c2b29f64-673a-4f0b-bc34-de35c8b02f27"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c3c4100a-8b46-4112-ada9-61302b436f3b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c4c69938-9c5c-46dc-a528-ceb6791011de"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c73984dd-572a-4eb1-91f7-12a6e01b9ba9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("c88e2d61-af9e-48a3-8d76-0b06bcdb9599"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("cc74eb90-5bd1-4f61-8079-1ce19fdc7205"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("cddccad7-fe95-4f83-b1b5-bc08f9c0c394"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ce63a226-e135-4e7e-951c-53bffc456ef7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ce7d5f85-8830-4401-bd38-06a14831c799"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ce889e64-a01c-4f57-8121-52356fc9e096"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ce959395-4633-4e19-b001-9a475ae67653"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d1c608d4-3490-41d3-914f-89deff0aab74"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d1e609da-a1b6-4c46-8665-3b9352c33734"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d204118b-41f8-462c-875a-73012272c126"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d227cd41-132e-4143-a17c-40313d348af3"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d24cd9b5-8134-4877-af1f-177a37bbc030"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d2b10830-695b-48f5-ac2a-51e30f5f0af9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d3d9d21f-fe99-4110-97e4-e0a8bc66880b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d4596df0-10aa-4eb5-9169-daaf7d83c6bd"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d47f0c82-af98-47d6-a521-839a0b9b810b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d63080e2-0473-4b02-aa29-9bf7c93fa9da"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("d9a1e6c9-3afd-41cc-807d-ed197d03d3e3"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("da17ea40-3fc7-45ea-9e4c-e6d056098294"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("da5c71cf-5011-452c-ac9c-e86c407b43f5"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("dad16907-e3ef-455a-a409-4b51f92615f4"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("db15d068-5529-4242-8f85-0c18ecba5c9f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ddd0d723-8226-4b3a-81a7-1293474fc3c7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e167c3c7-cd26-42d1-9213-20a01c2e8c26"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e2e7be55-7a68-41b0-8957-ff49d8a7bcc3"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e5e63d47-c866-4fd3-b1ef-ff2f3023e11e"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e68cd222-b101-4fcd-b023-f513dc7e11b7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e8fa2a9d-39b4-46ec-b0b7-d0b4daa41498"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e92a3d96-7b7f-4778-8cf6-9ee96e75e4f8"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("e9847152-9e38-4cfb-ae6e-54635d6e004a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("eba938fc-cd46-4032-80f6-682bf0d06c6d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ec16d283-cd7c-49e2-a8d0-c82c7b1ec03a"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ed4acaa0-8d31-4fd2-ad5f-4d7ae7abbe22"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ed4f50d2-2780-4b13-9e19-28a82f602d31"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ed7ea186-a794-4439-8667-cdf2d00cb2a7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("edd3e60e-676c-4967-bbe4-2c0f4b14ed71"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("ee1d7f99-577b-4bce-9bf4-37e333a0302b"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("eea5b18c-8165-4b32-87d9-ff93755609f6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f329b47b-45ed-460f-8620-3bf30916e7ae"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f3673e45-c78c-4e1b-856e-9286efff456d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f3b3f9f5-2d14-44a0-bd56-b797df3dd77c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f3e91599-2a2e-4f81-b4e0-9098a1ce8ec7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f51e3ba8-7844-40c3-bbd5-8d17ddf439f0"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f5a66fd5-5488-4462-8eee-6330640703d9"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f5bd64ad-7941-47da-aed7-10470d49670c"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("f797e941-4ac2-466d-b08d-1ebad9b7d178"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("fa645de7-8767-49c5-9fd2-5a393c914755"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("fb0e95e8-6c93-4700-a755-1f46b95b1e72"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("fb2a1c5c-94c5-4695-a4da-1a5adb8d2420"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("fc515e31-5e08-40f6-bf72-3aefbb5419d6"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("fca2b0af-5ca1-48ee-829b-df31bb53bd5d"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("fd7f7073-488a-4816-a1d4-336928ac62b7"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("febe507d-ebbd-4b75-a401-442d42c3f54f"));

            migrationBuilder.DeleteData(
                table: "training_subjects",
                keyColumn: "training_subject_id",
                keyValue: new Guid("feee2354-fb8a-442a-ac5d-9bc35582ce64"));
        }
    }
}
