namespace DqtApi.Responses
{
    public class Subject : LinkedEntity<dfeta_ittsubject>
    {
        public Subject() { }

        public string Code => Entity.dfeta_Value;
    }
}
