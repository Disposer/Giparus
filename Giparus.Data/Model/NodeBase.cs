using System;
using System.Globalization;

namespace Giparus.Data.Model
{
    public class NodeBase
    {
        #region Fields
        public int Srid = 4326;
        #endregion

        #region Properties
        public long Id { get; set; }
        public NodeType Type { get; set; }
        public TagCollection Tags { get; set; }
        public long? ChangeSetId { get; set; }
        public bool? Visible { get; set; }
        public DateTime? TimeStamp { get; set; }
        public int? Version { get; set; }
        public long? UserId { get; set; }
        public string User { get; set; }
        #endregion

        public override string ToString()
        {
            return this.Id.ToString(CultureInfo.InvariantCulture);
        }
    }
}
