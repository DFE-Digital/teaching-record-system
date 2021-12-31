using DqtApi.DAL;
using Microsoft.Xrm.Sdk;

namespace DqtApi.V1.Responses
{
    public static class EntityExtensions
    {
        public static U Extract<T, U>(this Entity source)
            where T : Entity, new()
            where U : LinkedEntity<T>, new()
        {
            var entityName = typeof(T).Name;

            var attributes = source.Attributes
                .MapCollection<object, AttributeCollection>(attribute => source.GetAttributeValue<AliasedValue>(attribute.Key).Value, entityName);

            if (!attributes.ContainsKey($"{entityName}id"))
            {
                return null;
            }

            var extraction = new T { Attributes = attributes };

            var formattedValues = source.FormattedValues
                .MapCollection<string, FormattedValueCollection>(formattedValue => formattedValue.Value, entityName);

            return new U
            {
                Entity = extraction,
                FormattedValues = formattedValues
            };
        }
    }
}
