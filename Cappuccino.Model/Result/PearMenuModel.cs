using System.Collections.Generic;

namespace Cappuccino.Model
{
    public class PearMenuModel
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public int Type { get; set; }
        public string Icon { get; set; }
        public string OpenType { get; set; }
        public string Href { get; set; }
        public List<PearMenuModel> Children { get; set; } = new List<PearMenuModel>();
        public long ParentId { get; set; }
    }

}
