using System.Collections.Generic;

namespace Giparus.Data.Model.SqlModel
{
    public class Relation : NodeBase
    {
        public IList<Member> Members { get; set; }
    }
}
