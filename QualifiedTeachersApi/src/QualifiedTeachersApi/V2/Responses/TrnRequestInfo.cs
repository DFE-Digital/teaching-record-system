using System;
using System.Text.Json.Serialization;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace QualifiedTeachersApi.V2.Responses
{
    public class TrnRequestInfo
    {
        [SwaggerSchema(Nullable = false)]
        public string RequestId { get; set; }

        public TrnRequestStatus Status { get; set; }

        public string Trn { get; set; }

        public bool PotentialDuplicate { get; set; }

        public DateOnly? QtsDate { get; set; }

        [JsonIgnore]
        public bool WasCreated { get; set; }
    }

    public class TrnRequestInfoExample : IExamplesProvider<TrnRequestInfo>
    {
        public TrnRequestInfo GetExamples() => new()
        {
            RequestId = "72888c5d-db14-4222-829b-7db9c2ec0dc3",
            Status = TrnRequestStatus.Completed,
            Trn = "1234567",
            PotentialDuplicate = false,
            QtsDate = null
        };
    }
}
