using Microsoft.SqlServer.Types;

namespace Giparus.Data.Model.MongoModel
{
    public class Node : NodeBase, INode
    {
        #region Properties
        public double[] LatLong { get; set; }
        public double Latitude { get; set; }
        public double Longtitude { get; set; } 
        #endregion

        #region .ctor
        public Node() { base.Type = NodeType.Node; }
        #endregion

        public void MakePosition(double latitude, double longtitude)
        {
            this.Latitude = latitude;
            this.Longtitude = longtitude;
            this.LatLong = new[] { latitude, longtitude };
        }

        public override string ToString()
        {
            return string.Format("{0}:{1},{2}", base.Id, this.LatLong[0], this.LatLong[1]);
        }
    }
}
