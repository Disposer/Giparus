using System.Collections.Generic;
using System.Linq;

namespace Giparus.Data.Model.MongoModel
{
    public class Way : NodeBase, IWay
    {
        #region Fields
        #endregion

        #region Properties

        public GeoType Shape { get; set; }
        public IList<double[]> Nodes { get; set; }
        public IList<long> NodeIds { get; set; }
        #endregion

        #region .ctor
        private Way() { base.Type = NodeType.Way; }
        #endregion

        public void MakeWay(Dictionary<long, INode> nodes)
        {
            if (this.NodeIds.Count == 1) this.Shape = GeoType.Point;
            else if (this.NodeIds.First().Equals(this.NodeIds.Last()))
            {
                if (this.NodeIds.Count == 2) this.Shape = GeoType.Point;
                if (this.NodeIds.Count == 3)
                {
                    this.Shape = GeoType.LineString;

                    var index = this.NodeIds.Count - 1;
                    this.NodeIds.RemoveAt(index);
                }
                else this.Shape = GeoType.Polygon;
            }
            else this.Shape = GeoType.LineString;

            this.Nodes = new List<double[]>(this.NodeIds.Count);
            foreach (var nodeIndex in this.NodeIds)
            {
                var node = nodes[nodeIndex];
                this.Nodes.Add(new[] { node.Latitude, node.Longtitude });
            }
        }

        public override string ToString()
        {
            return string.Format("{0}:{1} nodes", base.Id, this.NodeIds.Count);
        }
    }
}
