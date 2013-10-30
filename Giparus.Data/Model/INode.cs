namespace Giparus.Data.Model
{
    public interface INode
    {
        double Latitude { get; }
        double Longtitude { get; }
        void MakePosition(double latitude, double longtitude);
    }
}
