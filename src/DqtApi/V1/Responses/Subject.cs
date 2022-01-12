using DqtApi.DataStore.Crm.Models;

namespace DqtApi.V1.Responses
{
    public class Subject : LinkedEntity<dfeta_ittsubject>
    {
        public Subject() { }

        public string Code => Entity.dfeta_Value;
    }
}
