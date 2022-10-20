using System.Collections;

namespace agility.models
{
    public class ContentList
    {
        public int? recordOffset { get; set; }
        public int? totalCount { get; set; }
        public int? pageSize { get; set; }
        public ArrayList? items { get; set; }
    }
}
