using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.Crm.Services.Utility;
using Microsoft.Xrm.Sdk.Metadata;

namespace DqtApi.CrmSvcUtilFilter
{
    public class CodeWriterFilter : ICodeWriterFilterService
    {
        private readonly ICodeWriterFilterService _defaultService;
        private readonly IDictionary<string, IEnumerable<string>> _validEntities;

        public CodeWriterFilter(ICodeWriterFilterService defaultService)
        {
            _defaultService = defaultService;
            _validEntities = new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase);

            LoadFilterData();
        }

        public bool GenerateAttribute(AttributeMetadata attributeMetadata, IServiceProvider services) =>
            _validEntities.ContainsKey(attributeMetadata.EntityLogicalName)
            && _validEntities[attributeMetadata.EntityLogicalName].Contains(attributeMetadata.LogicalName);

        public bool GenerateEntity(EntityMetadata entityMetadata, IServiceProvider services) =>
            _validEntities.ContainsKey(entityMetadata.LogicalName);

        public bool GenerateOption(OptionMetadata optionMetadata, IServiceProvider services) =>
            _defaultService.GenerateOption(optionMetadata, services);

        public bool GenerateOptionSet(OptionSetMetadataBase optionSetMetadata, IServiceProvider services) =>
            _defaultService.GenerateOptionSet(optionSetMetadata, services);

        public bool GenerateRelationship(RelationshipMetadataBase relationshipMetadata, EntityMetadata otherEntityMetadata, IServiceProvider services) =>
            _defaultService.GenerateRelationship(relationshipMetadata, otherEntityMetadata, services);

        public bool GenerateServiceContext(IServiceProvider services) =>
            _defaultService.GenerateServiceContext(services);

        private void LoadFilterData()
        {
            var filterFileLocation = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "filter.xml");

            XElement xml = XElement.Load(filterFileLocation);
            XElement entitiesElement = xml.Element("entities");

            foreach (XElement entityElement in entitiesElement.Elements("entity"))
            {
                var attributes = entityElement.Element("attributes");
                _validEntities.Add(entityElement.Attribute("name").Value.ToLowerInvariant(), attributes.Elements("attribute").Select(attribute => attribute.Attribute("name").Value.ToLowerInvariant()));
            }
        }
    }
}
