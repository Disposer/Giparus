using System.Collections.Generic;

namespace Giparus.Data.Model.SqlModel
{
    public class SqlRelation : NodeBase
    {
        public IList<Member> Members { get; set; }
    }
}
