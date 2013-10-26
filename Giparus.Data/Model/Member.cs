using System;

namespace Giparus.Data.Model
{
    public class Member
    {
        public NodeType Type { get; set; }
        public string Role { get; set; }
        public long Reference { get; set; }

        public override string ToString()
        {
            return String.Format("{0}:{1} role as {2}", this.Reference, this.Type, this.Role);
        }
    }
}
