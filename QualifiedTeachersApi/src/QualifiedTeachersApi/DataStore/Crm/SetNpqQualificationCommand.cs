#nullable disable
using System;
using QualifiedTeachersApi.DataStore.Crm.Models;

namespace QualifiedTeachersApi.DataStore.Crm;

public class SetNpqQualificationCommand
{
    public DateTime? CompletionDate { get; set; }
    public Guid TeacherId { get; set; }
    public dfeta_qualification_dfeta_Type QualificationType { get; set; }
}
