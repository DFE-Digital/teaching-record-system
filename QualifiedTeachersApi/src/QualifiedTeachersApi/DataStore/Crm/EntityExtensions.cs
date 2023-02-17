using Microsoft.Xrm.Sdk;

namespace QualifiedTeachersApi.DataStore.Crm
{
    public static class EntityExtensions
    {
        public static T Extract<T>(this Entity source)
            where T : Entity, new()
        {
            string prefix = typeof(T).Name;

            return Extract<T>(source, prefix, idAttribute: prefix + "id");
        }

        public static T Extract<T>(this Entity source, string prefix, string idAttribute)
            where T : Entity, new()
        {
            var attributes = source.Attributes
                .MapCollection<object, AttributeCollection>(attribute => source.GetAttributeValue<AliasedValue>(attribute.Key).Value, prefix);

            if (!attributes.ContainsKey(idAttribute))
            {
                return null;
            }

            var formattedValues = source.FormattedValues
                .MapCollection<string, FormattedValueCollection>(formattedValue => formattedValue.Value, prefix);

            var entity = new T()
            {
                Attributes = attributes
            };

            entity.FormattedValues.AddRange(formattedValues);

            return entity;
        }
    }
}
