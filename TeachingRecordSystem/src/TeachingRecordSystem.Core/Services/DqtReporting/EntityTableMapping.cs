#nullable enable
using System.Diagnostics;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;

namespace TeachingRecordSystem.Core.Services.DqtReporting;

[DebuggerDisplay("{EntityLogicalName,nq}")]
public class EntityTableMapping
{
    private const string IdColumnName = "Id";
    private const string DeleteLogTableName = "__DeleteLog";
    private const string InsertedColumnName = "__Inserted";
    private const string UpdatedColumnName = "__Updated";

    public required string EntityLogicalName { get; init; }
    public required string TableName { get; init; }
    public required AttributeColumnMapping[] Attributes { get; init; }

    public int ColumnCount => Attributes.SelectMany(a => a.ColumnDefinitions).Count();

    public static EntityTableMapping Create(EntityMetadata entityMetadata)
    {
        var entityLogicalName = entityMetadata.LogicalName;

        var attributes = entityMetadata.Attributes
            .OrderBy(a => a.ColumnNumber)
            .Where(a => a.AttributeType != AttributeTypeCode.Virtual && a.LogicalName != "msft_datastate")
            .Select(attr =>
            {
                return attr switch
                {
                    _ when attr.LogicalName == entityMetadata.PrimaryIdAttribute => CreateIdMapping(),
                    { AttributeType: AttributeTypeCode.Boolean } => CreateOneToOneMapping(typeof(bool), "bit"),
                    { AttributeType: AttributeTypeCode.DateTime } => CreateOneToOneMapping(typeof(DateTime), "datetime"),
                    { AttributeType: AttributeTypeCode.Decimal } => CreateDecimalMapping(),
                    { AttributeType: AttributeTypeCode.Double } => CreateDoubleMapping(),
                    { AttributeType: AttributeTypeCode.Integer } => CreateOneToOneMapping(typeof(int), "int"),
                    { AttributeType: AttributeTypeCode.Money } => CreateMoneyMapping(),
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

                AttributeColumnMapping CreateDoubleMapping()
                {
                    var precision = ((DoubleAttributeMetadata)attr).Precision;
                    return CreateOneToOneMapping(typeof(decimal), $"decimal(38, {precision})");
                }

                AttributeColumnMapping CreateDecimalMapping()
                {
                    var precision = ((DecimalAttributeMetadata)attr).Precision;
                    return CreateOneToOneMapping(typeof(decimal), $"decimal(38, {precision})");
                }

                AttributeColumnMapping CreateMoneyMapping()
                {
                    var precision = ((MoneyAttributeMetadata)attr).Precision;
                    return CreateOneToOneMapping(typeof(decimal), $"decimal(38, {precision})", attrValue => ((Money)attrValue).Value);
                }
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

        sqlBuilder.AppendFormat(",\n\t[{0}] datetime,\n", InsertedColumnName);
        sqlBuilder.AppendFormat("\t[{0}] datetime\n", UpdatedColumnName);
        sqlBuilder.AppendLine(")");

        return sqlBuilder.ToString();
    }

    public SqlCommand GetDeleteSqlCommand(IEnumerable<Guid> ids, IClock clock)
    {
        var idParameters = ids.Select((id, i) => new SqlParameter($"@id{i}", id)).ToArray();

        var sqlBuilder = new StringBuilder();
        sqlBuilder.AppendFormat("delete from [{0}]\n", TableName);
        sqlBuilder.AppendFormat("output deleted.[{0}], @EntityType, @UtcNow into [{1}]\n", IdColumnName, DeleteLogTableName);
        sqlBuilder.AppendFormat("where [{0}] in ({1})\n", IdColumnName, string.Join(", ", idParameters.Select(p => $"{p.ParameterName}")));

        var command = new SqlCommand(sqlBuilder.ToString());
        command.Parameters.Add(new SqlParameter("@UtcNow", clock.UtcNow));
        command.Parameters.Add(new SqlParameter("@EntityType", EntityLogicalName));
        command.Parameters.AddRange(idParameters);

        return command;
    }

    public SqlCommand GetMergeSqlCommand(string sourceTableName, IClock clock)
    {
        var allColumns = Attributes.SelectMany(a => a.ColumnDefinitions).ToArray();

        var sqlBuilder = new StringBuilder();
        sqlBuilder.AppendLine($"merge into [{TableName}] as target");
        sqlBuilder.AppendLine("using (select");
        sqlBuilder.AppendJoin(",\n", allColumns.Select(c => $"\t[{c.ColumnName}]"));
        sqlBuilder.AppendLine($"\n from [{sourceTableName}]) as source");
        sqlBuilder.AppendLine($"on source.[{IdColumnName}] = target.[{IdColumnName}]");
        sqlBuilder.AppendLine("when matched then update set");
        sqlBuilder.AppendFormat("\t[{0}] = @UtcNow,\n", UpdatedColumnName);
        sqlBuilder.AppendJoin(",\n", allColumns.Where(c => c.ColumnName != IdColumnName).Select(p => $"\t[{p.ColumnName}] = source.[{p.ColumnName}]"));
        sqlBuilder.AppendLine("\nwhen not matched then insert (");
        sqlBuilder.AppendFormat("\t[{0}],\n\t[{1}],\n", InsertedColumnName, UpdatedColumnName);
        sqlBuilder.AppendJoin(",\n", allColumns.Select(c => $"\t[{c.ColumnName}]"));
        sqlBuilder.AppendLine("\n) values (");
        sqlBuilder.AppendFormat("\t@UtcNow,\n\t@UtcNow,\n");
        sqlBuilder.AppendJoin(",\n", allColumns.Select(c => $"\tsource.[{c.ColumnName}]"));
        sqlBuilder.AppendLine("\n)\noutput $action;");

        var command = new SqlCommand(sqlBuilder.ToString());
        command.Parameters.Add(new SqlParameter("@UtcNow", clock.UtcNow));

        return command;
    }

    public string GetAddAttributeColumnsSql(AttributeColumnMapping attribute)
    {
        var sqlBuilder = new StringBuilder();

        foreach (var column in attribute.ColumnDefinitions)
        {
            sqlBuilder.AppendLine($"alter table [{TableName}] add [{column.ColumnName}] {column.ColumnDefinition}");
        }

        return sqlBuilder.ToString();
    }
}

[DebuggerDisplay("{AttributeName,nq}")]
public class AttributeColumnMapping
{
    public required string AttributeName { get; init; }
    public required AttributeColumnDefinition[] ColumnDefinitions { get; init; }
}

[DebuggerDisplay("{ColumnName,nq}")]
public class AttributeColumnDefinition
{
    public required string ColumnName { get; init; }
    public required Type Type { get; init; }
    public required string ColumnDefinition { get; init; }
    public required Func<object, object> GetColumnValueFromAttribute { get; init; }
}
