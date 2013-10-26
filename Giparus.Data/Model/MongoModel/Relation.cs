using System.Collections.Generic;

namespace Giparus.Data.Model.MongoModel
{
    public class Relation : NodeBase
    {
        public IList<Member> Members { get; set; }
    }
}
