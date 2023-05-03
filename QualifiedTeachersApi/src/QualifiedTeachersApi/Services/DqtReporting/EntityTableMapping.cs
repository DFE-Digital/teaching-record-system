using System;
using System.Linq;
using System.Text;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;

namespace QualifiedTeachersApi.Services.DqtReporting;

public class EntityTableMapping
{
    public const string IdColumnName = "Id";
    public required string EntityLogicalName { get; init; }
    public required string TableName { get; init; }
    public required AttributeColumnMapping[] Attributes { get; init; }

    public int ColumnCount => Attributes.SelectMany(a => a.ColumnDefinitions).Count();

    public static EntityTableMapping Create(EntityMetadata entityMetadata)
    {
        var entityLogicalName = entityMetadata.LogicalName;

        var attributes = entityMetadata.Attributes
            .OrderBy(a => a.ColumnNumber)
            .Where(a => a.AttributeType != AttributeTypeCode.Virtual)
            .Select(attr =>
            {
                return attr switch
                {
                    _ when attr.LogicalName == entityMetadata.PrimaryIdAttribute => CreateIdMapping(),
                    { AttributeType: AttributeTypeCode.Boolean } => CreateOneToOneMapping(typeof(bool), "bit"),
                    { AttributeType: AttributeTypeCode.DateTime } => CreateOneToOneMapping(typeof(DateTime), "datetime"),
                    { AttributeType: AttributeTypeCode.Decimal } => CreateOneToOneMapping(typeof(decimal), "decimal"),
                    { AttributeType: AttributeTypeCode.Double } => CreateOneToOneMapping(typeof(double), "float"),
                    { AttributeType: AttributeTypeCode.Integer } => CreateOneToOneMapping(typeof(string), "int"),
                    { AttributeType: AttributeTypeCode.Money } => CreateOneToOneMapping(typeof(decimal), "decimal", attrValue => ((Money)attrValue).Value),
                    { AttributeType: AttributeTypeCode.State } => CreateOneToOneMappingForOptionSetValue(),
                    { AttributeType: AttributeTypeCode.Status } => CreateOneToOneMappingForOptionSetValue(),
                    { AttributeType: AttributeTypeCode.Uniqueidentifier } => CreateOneToOneMapping(typeof(Guid), "uniqueidentifier"),
                    { AttributeType: AttributeTypeCode.BigInt } => CreateOneToOneMapping(typeof(long), "bigint"),
                    { AttributeType: AttributeTypeCode.Picklist } => CreateOneToOneMappingForOptionSetValue(),
                    { AttributeType: AttributeTypeCode.Memo } => CreateOneToOneMapping(typeof(string), "nvarchar(max)"),
                    { AttributeType: AttributeTypeCode.String } => CreateStringMapping(),
                    { AttributeType: AttributeTypeCode.Lookup } => CreateLookupMapping(),
                    { AttributeType: AttributeTypeCode.Owner } => CreateLookupMapping(),
                    { AttributeType: AttributeTypeCode.Customer } => CreateLookupMapping(),
                    { AttributeType: AttributeTypeCode.EntityName } => CreateEntityNameMapping(),
                    { AttributeType: AttributeTypeCode.PartyList } => CreateOneToOneMapping(typeof(Guid), "uniqueidentifier"),
                    _ => throw new NotSupportedException($"Cannot derive table mapping for '{attr.LogicalName}' attribute.")
                };

                AttributeColumnMapping CreateOneToOneMappingForOptionSetValue() =>
                    CreateOneToOneMapping(typeof(int), "int", attrValue => ((OptionSetValue)attrValue).Value);

                AttributeColumnMapping CreateOneToOneMapping(Type type, string columnDefinition, Func<object, object>? getColumnValueFromAttribute = null) =>
                    new AttributeColumnMapping()
                    {
                        AttributeName = attr.LogicalName,
                        ColumnDefinitions = new[]
                        {
                            new AttributeColumnDefinition()
                            {
                                ColumnName = attr.LogicalName,
                                Type = type,
                                ColumnDefinition = columnDefinition,
                                GetColumnValueFromAttribute = getColumnValueFromAttribute ?? (attrValue => attrValue)
                            }
                        }
                    };

                AttributeColumnMapping CreateIdMapping() => new AttributeColumnMapping()
                {
                    AttributeName = attr.LogicalName,
                    ColumnDefinitions = new[]
                    {
                        new AttributeColumnDefinition()
                        {
                            ColumnName = IdColumnName,
                            Type = typeof(Guid),
                            ColumnDefinition = "uniqueidentifier",
                            GetColumnValueFromAttribute = attrValue => attrValue
                        }
                    }
                };

                AttributeColumnMapping CreateStringMapping()
                {
                    var maxLength = ((StringAttributeMetadata)attr).MaxLength;
                    var lengthDefinition = maxLength == 1073741823 ? "max" : maxLength.ToString();
                    return CreateOneToOneMapping(typeof(string), $"nvarchar({lengthDefinition})");
                }

                AttributeColumnMapping CreateLookupMapping() => new AttributeColumnMapping()
                {
                    AttributeName = attr.LogicalName,
                    ColumnDefinitions = new[]
                    {
                        new AttributeColumnDefinition()
                        {
                            ColumnName = attr.LogicalName,
                            Type = typeof(Guid),
                            ColumnDefinition = "uniqueidentifier",
                            GetColumnValueFromAttribute = attrValue => ((EntityReference)attrValue).Id
                        },
                        new AttributeColumnDefinition()
                        {
                            ColumnName = $"{attr.LogicalName}_entitytype",
                            Type = typeof(string),
                            ColumnDefinition = "nvarchar(128)",
                            GetColumnValueFromAttribute = attrValue => ((EntityReference)attrValue).LogicalName
                        }
                    }
                };

                AttributeColumnMapping CreateEntityNameMapping() => new AttributeColumnMapping()
                {
                    AttributeName = attr.LogicalName,
                    ColumnDefinitions = new[]
                    {
                        new AttributeColumnDefinition()
                        {
                            ColumnName = attr.LogicalName,
                            Type = typeof(string),
                            ColumnDefinition = "nvarchar(4000)",
                            GetColumnValueFromAttribute = attrValue => attrValue
                        }
                    }
                };
            })
            .ToArray();

        return new EntityTableMapping()
        {
            EntityLogicalName = entityLogicalName,
            TableName = entityLogicalName,
            Attributes = attributes
        };
    }

    public string GetCreateTableSql()
    {
        var allColumns = Attributes.SelectMany(a => a.ColumnDefinitions).ToArray();

        var sqlBuilder = new StringBuilder();
        sqlBuilder.AppendLine($"create table [{TableName}] (");
        sqlBuilder.AppendJoin(",\n", allColumns.Select(c =>
        {
            var line = $"\t[{c.ColumnName}] {c.ColumnDefinition}";

            if (c.ColumnName == IdColumnName)
            {
                line += " not null primary key";
            }

            return line;
        }));
        sqlBuilder.AppendLine("\n)");

        return sqlBuilder.ToString();
    }

    public string GetMergeSql(string sourceTableName)
    {
        var allColumns = Attributes.SelectMany(a => a.ColumnDefinitions).ToArray();

        var sqlBuilder = new StringBuilder();
        sqlBuilder.AppendLine($"merge into [{TableName}] as target");
        sqlBuilder.AppendLine("using (select");
        sqlBuilder.AppendJoin(",\n", allColumns.Select(c => $"\t[{c.ColumnName}]"));
        sqlBuilder.AppendLine($"\n from [{sourceTableName}]) as source");
        sqlBuilder.AppendLine($"on source.[{IdColumnName}] = target.[{IdColumnName}]");
        sqlBuilder.AppendLine("when matched then update set");
        sqlBuilder.AppendJoin(",\n", allColumns.Where(c => c.ColumnName != IdColumnName).Select(p => $"\t[{p.ColumnName}] = source.[{p.ColumnName}]"));
        sqlBuilder.AppendLine("\nwhen not matched then insert (");
        sqlBuilder.AppendJoin(",\n", allColumns.Select(c => $"\t[{c.ColumnName}]"));
        sqlBuilder.AppendLine("\n) values (");
        sqlBuilder.AppendJoin(",\n", allColumns.Select(c => $"\tsource.[{c.ColumnName}]"));
        sqlBuilder.AppendLine("\n)\noutput $action;");

        return sqlBuilder.ToString();
    }
}

public class AttributeColumnMapping
{
    public required string AttributeName { get; init; }
    public required AttributeColumnDefinition[] ColumnDefinitions { get; init; }
}

public class AttributeColumnDefinition
{
    public required string ColumnName { get; init; }
    public required Type Type { get; init; }
    public required string ColumnDefinition { get; init; }
    public required Func<object, object> GetColumnValueFromAttribute { get; init; }
}
