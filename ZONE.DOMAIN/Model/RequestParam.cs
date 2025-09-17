using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZONE.DOMAIN.Model
{
    public class RequestParam
    {
        [DefaultValue(0)]
        public int? Start { get; set; }

        [DefaultValue(10)]
        public int? Length { get; set; }

        [DefaultValue("")]
        public string? SortBy { get; set; }

        [DefaultValue("")]
        public string? SortOrder { get; set; }

        [DefaultValue("")]
        public string? ColumnSearch { get; set; }

        [DefaultValue(false)]
        public bool? IsDeleted { get; set; }

        public Dictionary<string, object>? DynamicParams { get; set; } = new Dictionary<string, object>();

    }
}
