using System.Collections.Generic;
using Giparus.Data.Model.SqlModel;

namespace Giparus.Data.Model.SqlModel
{
    public class Relation : NodeBase
    {
        public IList<Member> Members { get; set; }
    }
}
