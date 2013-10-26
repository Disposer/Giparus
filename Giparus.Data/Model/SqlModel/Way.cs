using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Types;

namespace Giparus.Data.Model.SqlModel
{
    public class Way : NodeBase, IWay
    {
        #region Fields
        #endregion

        #region Properties
        public OpenGisGeographyType Shape { get; set; }
        public SqlGeography Lines { get; set; }
        public IList<long> NodeIds { get; set; }
        #endregion

        #region .ctor
        private Way() { base.Type = NodeType.Way; }
        #endregion

        public void MakeWay(Dictionary<long, INode> nodes)
        {
            var builder = new SqlGeographyBuilder();
            builder.SetSrid(base.Srid);

            if (this.NodeIds.Count == 1)
            {
                this.Shape = OpenGisGeographyType.Point;
                builder.BeginGeography(OpenGisGeographyType.Point);
            }
            else if (this.NodeIds.First().Equals(this.NodeIds.Last()))
            {
                if (this.NodeIds.Count == 2)
                {
                    builder.BeginGeography(OpenGisGeographyType.Point);
                    this.Shape = OpenGisGeographyType.Point;
                }
                if (this.NodeIds.Count == 3)
                {
                    builder.BeginGeography(OpenGisGeographyType.LineString);
                    this.Shape = OpenGisGeographyType.LineString;

                    var index = this.NodeIds.Count - 1;
                    this.NodeIds.RemoveAt(index);
                }
                else
                {
                    this.Shape = OpenGisGeographyType.Polygon;
                    builder.BeginGeography(OpenGisGeographyType.Polygon);
                }
            }
            else
            {
                this.Shape = OpenGisGeographyType.LineString;
                builder.BeginGeography(OpenGisGeographyType.LineString);
            }

            builder.BeginFigure(nodes[this.NodeIds[0]].Latitude, nodes[this.NodeIds[0]].Longtitude);
            for (var index = 1; index < this.NodeIds.Count; index++)
            {
                var nodeId = this.NodeIds[index];
                var node = nodes[nodeId];
                builder.AddLine(node.Latitude, node.Longtitude);
            }

            builder.EndFigure();
            builder.EndGeography();

            this.Lines = builder.ConstructedGeography;
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", base.Id, this.Lines);
        }
    }
}
