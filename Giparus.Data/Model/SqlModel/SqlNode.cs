using Microsoft.SqlServer.Types;

namespace Giparus.Data.Model.SqlModel
{    
    public class SqlNode : NodeBase, INode
    {
        #region Properties
        public SqlGeography Position { get; set; }
        public double Latitude { get; set; }
        public double Longtitude { get; set; } 
        #endregion

        #region .ctor
        public SqlNode() { base.Type = NodeType.Node; }
        #endregion

        public void MakePosition(double latitude, double longtitude)
        {
            this.Latitude = latitude;
            this.Longtitude = longtitude;

            var builder = new SqlGeographyBuilder();
            builder.SetSrid(base.Srid);
            builder.BeginGeography(OpenGisGeographyType.Point);

            builder.BeginFigure(latitude, longtitude);
            builder.EndFigure();

            builder.EndGeography();

            this.Position = builder.ConstructedGeography;
        }

        public void MakePosition()
        {
            var builder = new SqlGeographyBuilder();
            builder.SetSrid(base.Srid);
            builder.BeginGeography(OpenGisGeographyType.Point);

            builder.BeginFigure(this.Latitude, this.Longtitude);
            builder.EndFigure();

            builder.EndGeography();

            this.Position = builder.ConstructedGeography;
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", base.Id, this.Position);
        }
    }
}
