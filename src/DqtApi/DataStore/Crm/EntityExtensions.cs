using Microsoft.Xrm.Sdk;

namespace DqtApi.DataStore.Crm
{
    public static class EntityExtensions
    {
        public static T Extract<T>(this Entity source)
            where T : Entity, new()
        {
            string entityAlias = typeof(T).Name;

            return Extract<T>(source, entityAlias);
        }

        public static T Extract<T>(this Entity source, string entityAlias)
            where T : Entity, new()
        {
            var attributes = source.Attributes
                .MapCollection<object, AttributeCollection>(attribute => source.GetAttributeValue<AliasedValue>(attribute.Key).Value, entityAlias);

            if (!attributes.ContainsKey($"{entityAlias}id"))
            {
                return null;
            }

            var formattedValues = source.FormattedValues
                .MapCollection<string, FormattedValueCollection>(formattedValue => formattedValue.Value, entityAlias);

            var entity = new T()
            {
                Attributes = attributes
            };

            entity.FormattedValues.AddRange(formattedValues);

            return entity;
        }
    }
}
