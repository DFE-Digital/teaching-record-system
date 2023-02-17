using System;
using System.Linq;
using Microsoft.Xrm.Sdk;
using QualifiedTeachersApi.DataStore.Crm.Models;
using QualifiedTeachersApi.V1.Handlers;
using Xunit;

namespace QualifiedTeachersApi.Tests.V1.UnitTests
{
    public class GetTeacherHandlerTests
    {
        [Fact]
        public void Given_a_contact_the_details_are_mapped()
        {
            var fullName = "Joe Bloggs";
            var nationalInsuranceNumber = "AB123456C";
            var trn = "1111111";
            var birthDate = new DateTime(1965, 1, 1);

            var contact = new Contact
            {
                dfeta_ActiveSanctions = true,
                BirthDate = birthDate,
                FullName = fullName,
                dfeta_NINumber = nationalInsuranceNumber,
                StateCode = ContactState.Active,
                dfeta_TRN = trn,
                FormattedValues =
                {
                    { Contact.Fields.StateCode, ContactState.Active.ToString() }
                }
            };

            var response = GetTeacherHandler.MapContactToResponse(contact);

            Assert.True(response.ActiveAlert);
            Assert.Equal(birthDate, response.DateOfBirth);
            Assert.Equal(fullName, response.Name);
            Assert.Equal(nationalInsuranceNumber, response.NationalInsuranceNumber);
            Assert.Equal(ContactState.Active, response.State);
            Assert.Equal(ContactState.Active.ToString(), response.StateName);
            Assert.Equal(trn, response.Trn);
        }

        [Fact]
        public void Given_a_contact_with_qualified_teacher_status_the_details_are_mapped()
        {
            var qualifiedTeacherStatusName = "Qualified";
            var qtsDate = new DateTime(2021, 1, 1);

            var prefix = nameof(dfeta_qtsregistration);

            var contact = new Contact
            {
                dfeta_ActiveSanctions = false,
                StateCode = ContactState.Active,
                Attributes =
                {
                    { $"{prefix}.{dfeta_qtsregistration.Fields.dfeta_name}",
                        new AliasedValue(dfeta_qtsregistration.EntityLogicalName, dfeta_qtsregistration.Fields.dfeta_name, qualifiedTeacherStatusName) },
                    { $"{prefix}.{dfeta_qtsregistration.Fields.dfeta_QTSDate}",
                        new AliasedValue(dfeta_qtsregistration.EntityLogicalName, dfeta_qtsregistration.Fields.dfeta_QTSDate, qtsDate) },
                    { $"{prefix}.{dfeta_qtsregistration.Fields.StateCode}",
                        new AliasedValue(dfeta_qtsregistration.EntityLogicalName, dfeta_qtsregistration.Fields.StateCode, new OptionSetValue((int)dfeta_qtsregistrationState.Active)) },
                    { $"{prefix}.{dfeta_qtsregistration.PrimaryIdAttribute}",
                        new AliasedValue(dfeta_qtsregistration.EntityLogicalName, dfeta_qtsregistration.PrimaryIdAttribute, Guid.NewGuid())}
                },
                FormattedValues =
                {
                    { Contact.Fields.StateCode, ContactState.Active.ToString() },
                    { $"{prefix}.{dfeta_qtsregistration.Fields.StateCode}", dfeta_qtsregistrationState.Active.ToString() }
                }
            };

            var response = GetTeacherHandler.MapContactToResponse(contact);

            var qualifiedTeacherStatus = response.QualifiedTeacherStatus;

            Assert.NotNull(qualifiedTeacherStatus);
            Assert.Equal(qualifiedTeacherStatusName, qualifiedTeacherStatus.Name);
            Assert.Equal(qtsDate, qualifiedTeacherStatus.QtsDate);
            Assert.Equal(dfeta_qtsregistrationState.Active, qualifiedTeacherStatus.State);
            Assert.Equal(dfeta_qtsregistrationState.Active.ToString(), qualifiedTeacherStatus.StateName);
        }

        [Fact]
        public void Given_a_contact_with_induction_the_details_are_mapped()
        {
            var completionDate = new DateTime(2021, 1, 1);
            var inductionStatusName = dfeta_InductionStatus.Pass.ToString();
            var startDate = new DateTime(2020, 10, 1);

            var prefix = nameof(dfeta_induction);

            var contact = new Contact
            {
                dfeta_ActiveSanctions = false,
                StateCode = ContactState.Active,
                Attributes =
                {
                    { $"{prefix}.{dfeta_induction.Fields.dfeta_CompletionDate}",
                        new AliasedValue(dfeta_induction.EntityLogicalName, dfeta_induction.Fields.dfeta_CompletionDate, completionDate) },
                    { $"{prefix}.{dfeta_induction.Fields.dfeta_StartDate}",
                        new AliasedValue(dfeta_induction.EntityLogicalName, dfeta_induction.Fields.dfeta_StartDate, startDate) },
                    { $"{prefix}.{dfeta_induction.Fields.StateCode}",
                        new AliasedValue(dfeta_induction.EntityLogicalName, dfeta_induction.Fields.StateCode, new OptionSetValue((int)dfeta_inductionState.Active)) },
                    { $"{prefix}.{dfeta_induction.PrimaryIdAttribute}",
                        new AliasedValue(dfeta_induction.EntityLogicalName, dfeta_induction.PrimaryIdAttribute, Guid.NewGuid())}
                },
                FormattedValues =
                {
                    { Contact.Fields.StateCode, ContactState.Active.ToString() },
                    { $"{prefix}.{dfeta_induction.Fields.dfeta_InductionStatus}", inductionStatusName },
                    { $"{prefix}.{dfeta_induction.Fields.StateCode}", dfeta_inductionState.Active.ToString() }
                }
            };

            var response = GetTeacherHandler.MapContactToResponse(contact);

            var induction = response.Induction;

            Assert.NotNull(induction);
            Assert.Equal(completionDate, induction.CompletionDate);
            Assert.Equal(inductionStatusName, induction.InductionStatusName);
            Assert.Equal(startDate, induction.StartDate);
            Assert.Equal(dfeta_inductionState.Active, induction.State);
            Assert.Equal(dfeta_inductionState.Active.ToString(), induction.StateName);
        }

        [Fact]
        public void Given_a_contact_with_initial_teacher_training_the_details_are_mapped()
        {
            var programmeEndDate = new DateTime(2021, 1, 1);
            var programmeStartDate = new DateTime(2020, 10, 1);
            var programmeType = dfeta_ITTProgrammeType.Apprenticeship.ToString();
            var qualification = "Qualification";
            var result = dfeta_ITTResult.ApplicationReceived.ToString();
            var subject1 = "Subject1";
            var subject2 = "Subject2";
            var subject3 = "Subject3";
            var subject1Code = "Code1";
            var subject2Code = "Code2";
            var subject3Code = "Code3";

            var prefix = nameof(dfeta_initialteachertraining);

            var subjectPrefix = nameof(dfeta_ittsubject);

            var contact = new Contact
            {
                dfeta_ActiveSanctions = false,
                StateCode = ContactState.Active,
                Attributes =
                {
                    { $"{prefix}.{dfeta_initialteachertraining.Fields.dfeta_ProgrammeEndDate}",
                        new AliasedValue(dfeta_initialteachertraining.EntityLogicalName, dfeta_initialteachertraining.Fields.dfeta_ProgrammeEndDate, programmeEndDate) },
                    { $"{prefix}.{dfeta_initialteachertraining.Fields.dfeta_ProgrammeStartDate}",
                        new AliasedValue(dfeta_initialteachertraining.EntityLogicalName, dfeta_initialteachertraining.Fields.dfeta_ProgrammeStartDate, programmeStartDate) },
                    { $"{prefix}.{dfeta_initialteachertraining.Fields.StateCode}",
                        new AliasedValue(dfeta_initialteachertraining.EntityLogicalName, dfeta_initialteachertraining.Fields.StateCode, new OptionSetValue((int)dfeta_initialteachertrainingState.Active)) },
                    { $"{prefix}.{dfeta_initialteachertraining.PrimaryIdAttribute}",
                        new AliasedValue(dfeta_initialteachertraining.EntityLogicalName, dfeta_initialteachertraining.PrimaryIdAttribute, Guid.NewGuid())},
                    { $"{prefix}.{subjectPrefix}1.{dfeta_ittsubject.Fields.dfeta_Value}",
                        new AliasedValue(dfeta_ittsubject.EntityLogicalName, dfeta_ittsubject.Fields.dfeta_Value, subject1Code) },
                    { $"{prefix}.{subjectPrefix}2.{dfeta_ittsubject.Fields.dfeta_Value}",
                        new AliasedValue(dfeta_ittsubject.EntityLogicalName, dfeta_ittsubject.Fields.dfeta_Value, subject2Code) },
                    { $"{prefix}.{subjectPrefix}3.{dfeta_ittsubject.Fields.dfeta_Value}",
                        new AliasedValue(dfeta_ittsubject.EntityLogicalName, dfeta_ittsubject.Fields.dfeta_Value, subject3Code) }
                },
                FormattedValues =
                {
                    { Contact.Fields.StateCode, ContactState.Active.ToString() },
                    { $"{prefix}.{dfeta_initialteachertraining.Fields.dfeta_ProgrammeType}", programmeType },
                    { $"{prefix}.{dfeta_initialteachertraining.Fields.dfeta_ITTQualificationId}", qualification },
                    { $"{prefix}.{dfeta_initialteachertraining.Fields.dfeta_Result}", result },
                    { $"{prefix}.{dfeta_initialteachertraining.Fields.dfeta_Subject1Id}", subject1 },
                    { $"{prefix}.{dfeta_initialteachertraining.Fields.dfeta_Subject2Id}", subject2 },
                    { $"{prefix}.{dfeta_initialteachertraining.Fields.dfeta_Subject3Id}", subject3 },
                    { $"{prefix}.{dfeta_initialteachertraining.Fields.StateCode}", dfeta_initialteachertrainingState.Active.ToString() }
                }
            };

            var response = GetTeacherHandler.MapContactToResponse(contact);

            var initialTeacherTraining = response.InitialTeacherTraining;

            Assert.NotNull(initialTeacherTraining);
            Assert.Equal(programmeEndDate, initialTeacherTraining.ProgrammeEndDate);
            Assert.Equal(programmeStartDate, initialTeacherTraining.ProgrammeStartDate);
            Assert.Equal(programmeType, initialTeacherTraining.ProgrammeType);
            Assert.Equal(qualification, initialTeacherTraining.Qualification);
            Assert.Equal(result, initialTeacherTraining.Result);
            Assert.Equal(dfeta_initialteachertrainingState.Active, initialTeacherTraining.State);
            Assert.Equal(dfeta_initialteachertrainingState.Active.ToString(), initialTeacherTraining.StateName);
            Assert.Equal(subject1, initialTeacherTraining.Subject1Id);
            Assert.Equal(subject2, initialTeacherTraining.Subject2Id);
            Assert.Equal(subject3, initialTeacherTraining.Subject3Id);
            Assert.Equal(subject1Code, initialTeacherTraining.Subject1Code);
            Assert.Equal(subject2Code, initialTeacherTraining.Subject2Code);
            Assert.Equal(subject3Code, initialTeacherTraining.Subject3Code);
        }

        [Fact]
        public void Given_a_contact_with_qualifications_the_details_are_mapped()
        {
            var qualification1Subject1 = "Subject 1";
            var qualification1Subject1Code = "X101";
            var qualification1Subject2 = "Subject 2";
            var qualification1Subject2Code = "X102";
            var qualification1Subject3 = "Subject 3";
            var qualification1Subject3Code = "X103";
            var qualification1HeName = "HE Name";
            var qualification1Class = dfeta_classdivision.Merit;

            var qualification1 = new dfeta_qualification()
            {
                dfeta_CompletionorAwardDate = new DateTime(2021, 12, 1),
                dfeta_HE_ClassDivision = qualification1Class,
                Attributes =
                {
                    { $"{nameof(dfeta_hequalification)}.{dfeta_hequalification.PrimaryIdAttribute}", new AliasedValue(dfeta_hequalification.EntityLogicalName, dfeta_hequalification.PrimaryIdAttribute, Guid.NewGuid()) },
                    { $"{nameof(dfeta_hequalification)}.{dfeta_hequalification.Fields.dfeta_name}", new AliasedValue(dfeta_hequalification.EntityLogicalName, dfeta_hequalification.Fields.dfeta_name, qualification1HeName) },
                    { $"{nameof(dfeta_hesubject)}1.{dfeta_hesubject.PrimaryIdAttribute}", new AliasedValue(dfeta_hesubject.EntityLogicalName, dfeta_hesubject.PrimaryIdAttribute, Guid.NewGuid()) },
                    { $"{nameof(dfeta_hesubject)}1.{dfeta_hesubject.Fields.dfeta_name}", new AliasedValue(dfeta_hesubject.EntityLogicalName, dfeta_hesubject.Fields.dfeta_name, qualification1Subject1) },
                    { $"{nameof(dfeta_hesubject)}1.{dfeta_hesubject.Fields.dfeta_Value}", new AliasedValue(dfeta_hesubject.EntityLogicalName, dfeta_hesubject.Fields.dfeta_Value, qualification1Subject1Code) },
                    { $"{nameof(dfeta_hesubject)}2.{dfeta_hesubject.PrimaryIdAttribute}", new AliasedValue(dfeta_hesubject.EntityLogicalName, dfeta_hesubject.PrimaryIdAttribute, Guid.NewGuid()) },
                    { $"{nameof(dfeta_hesubject)}2.{dfeta_hesubject.Fields.dfeta_name}", new AliasedValue(dfeta_hesubject.EntityLogicalName, dfeta_hesubject.Fields.dfeta_name, qualification1Subject2) },
                    { $"{nameof(dfeta_hesubject)}2.{dfeta_hesubject.Fields.dfeta_Value}", new AliasedValue(dfeta_hesubject.EntityLogicalName, dfeta_hesubject.Fields.dfeta_Value, qualification1Subject2Code) },
                    { $"{nameof(dfeta_hesubject)}3.{dfeta_hesubject.PrimaryIdAttribute}", new AliasedValue(dfeta_hesubject.EntityLogicalName, dfeta_hesubject.PrimaryIdAttribute, Guid.NewGuid()) },
                    { $"{nameof(dfeta_hesubject)}3.{dfeta_hesubject.Fields.dfeta_name}", new AliasedValue(dfeta_hesubject.EntityLogicalName, dfeta_hesubject.Fields.dfeta_name, qualification1Subject3) },
                    { $"{nameof(dfeta_hesubject)}3.{dfeta_hesubject.Fields.dfeta_Value}", new AliasedValue(dfeta_hesubject.EntityLogicalName, dfeta_hesubject.Fields.dfeta_Value, qualification1Subject3Code) },
                },
                FormattedValues =
                {
                    { dfeta_qualification.Fields.dfeta_Type, dfeta_qualification_dfeta_Type.HigherEducation.ToString() }
                }
            };

            var qualification2 = new dfeta_qualification()
            {
                dfeta_CompletionorAwardDate = new DateTime(2021, 12, 2),
                FormattedValues =
                {
                    { dfeta_qualification.Fields.dfeta_Type, dfeta_qualification_dfeta_Type.MandatoryQualification.ToString() }
                }
            };

            var contact = new Contact()
            {
                dfeta_ActiveSanctions = false,
                StateCode = ContactState.Active,
                dfeta_contact_dfeta_qualification = new[] { qualification1, qualification2 },
                FormattedValues =
                {
                    { Contact.Fields.StateCode, ContactState.Active.ToString() }
                }
            };

            var response = GetTeacherHandler.MapContactToResponse(contact);

            var qualifications = response.Qualifications;

            Assert.Collection(
                qualifications,
                qualification =>
                {
                    Assert.Equal("HigherEducation", qualification.Name);
                    Assert.Equal(new DateTime(2021, 12, 1), qualification.DateAwarded);
                    Assert.Equal(qualification1Subject1, qualification.Subject1);
                    Assert.Equal(qualification1Subject1Code, qualification.Subject1Code);
                    Assert.Equal(qualification1Subject2, qualification.Subject2);
                    Assert.Equal(qualification1Subject2Code, qualification.Subject2Code);
                    Assert.Equal(qualification1Subject3, qualification.Subject3);
                    Assert.Equal(qualification1Subject3Code, qualification.Subject3Code);
                    Assert.Equal("Merit", qualification.ClassDivision?.ToString());
                    Assert.Equal(qualification1HeName, qualification.HeQualificationName);
                },
                qualification =>
                {
                    Assert.Equal("MandatoryQualification", qualification.Name);
                    Assert.Equal(new DateTime(2021, 12, 2), qualification.DateAwarded);
                });
        }
    }
}
