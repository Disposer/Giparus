namespace Giparus.Data.Model
{
    public interface INode
    {
        double Latitude { get; set; }
        double Longtitude { get; set; } 
        void MakePosition(double latitude, double longtitude);
    }
}
