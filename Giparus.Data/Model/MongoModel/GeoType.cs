﻿namespace Giparus.Data.Model.MongoModel
{
    public enum GeoType
    {
        Point = 1,
        LineString = 2,
        Polygon = 3,
        MultiPoint = 4,
        MultiLineString = 5,
        MultiPolygon = 6,
        GeometryCollection = 7,
        CircularString = 8,
        CompoundCurve = 9,
        CurvePolygon = 10,
        FullGlobe = 11,
    }
}
