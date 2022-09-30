using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace agility.models
{
    public class Model
    {
        public int? id { get; set; }
        public DateTime? lastModifiedDate { get; set; }
        public string? displayName { get; set; }
        public string? referenceName { get; set; }

        public string? lastModifiedBy { get; set; }

        public List<ModelField?> fields { get; set; } = new List<ModelField?>();

        public int? lastModifiedAuthorID { get; set; }

        public string? description { get; set; }

        public bool? allowTagging { get; set; }

        public string? contentDefinitionTypeName { get; set; }

        public bool? isPublished { get; set; }
        public bool? wasUnpublished { get; set; }
    }

    public class ModelField
    {
        public string? name { get; set; }
        public string? label { get; set; }
        public string? type { get; set; }
        public Dictionary<string, string>? settings { get; set; } = new Dictionary<string, string>();
        public string? labelHelpDescription { get; set; }
        public int? itemOrder { get; set; }

        public bool? designerOnly { get; set; }
        public bool? isDataField { get; set; }
        public bool? editable { get; set; }
        public bool? hiddenField { get; set; }
        public string? fieldID { get; set; }
        public string? description { get; set; }
    }
}
