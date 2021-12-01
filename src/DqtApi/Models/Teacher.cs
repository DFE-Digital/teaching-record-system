using System;
using System.Text.Json.Serialization;
using Microsoft.Xrm.Sdk;

namespace DqtApi.Models {
    public class Teacher {
        private Entity _entity;
        // TODO formatting through model binder?
        public string DateOfBirth => _entity.GetAttributeValue<DateTime>("dateofbirth").ToString();

        [JsonPropertyName("active_alert")]
        public bool ActiveAlert { get; set; }

        [JsonPropertyName("state")]
        public int State { get; set; }

        [JsonPropertyName("state_name")]
        public string StateName { get; set; }

        public Teacher(Entity entity) {
            _entity = entity;
        }
    }
}