using System;
using DqtApi.DataStore.Crm.Models;

namespace DqtApi.DataStore.Crm
{
    public class SetNpqQualificationCommand
    {
        public DateTime? CompletionDate { get; set; }
        public Guid TeacherId { get; set; }
        public dfeta_qualification_dfeta_Type QualificationType { get; set; }
    }
}
