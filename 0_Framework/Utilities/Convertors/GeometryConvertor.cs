using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace _0_Framework.Utilities.Convertors
{
    public static class GeometryConvertor
    {
        public static TGeometry? ConvertToGeometry<TGeometry>(string geoData) where TGeometry : Geometry
        {
            if (string.IsNullOrWhiteSpace(geoData))
                throw new ArgumentException("Input data is null or empty.", nameof(geoData));

            Geometry geometry;

            try
            {
                // Check if the data starts with "POINT", "POLYGON", etc., indicating WKT format
                if (geoData.Trim().StartsWith("POINT", StringComparison.OrdinalIgnoreCase) ||
                    geoData.Trim().StartsWith("POLYGON", StringComparison.OrdinalIgnoreCase) ||
                    geoData.Trim().StartsWith("LINESTRING", StringComparison.OrdinalIgnoreCase))
                {
                    // Parse WKT format
                    var wktReader = new WKTReader();
                    geometry = wktReader.Read(geoData);
                }
                else
                {
                    // Parse GeoJSON format
                    var geoJsonReader = new GeoJsonReader();
                    geometry = geoJsonReader.Read<Geometry>(geoData);
                }

                if (geometry is Polygon polygon)
                {
                    var exteriorRing = polygon.ExteriorRing;

                    var isCCW = Orientation.IsCCW(exteriorRing.Coordinates);

                    if (!isCCW)
                    {
                        geometry = geometry.Reverse();
                    }
                }

                return geometry as TGeometry;
            }
            catch (Exception ex)
            {
                throw new Exception("Error parsing geometry data", ex);
            }
        }

        // Optional: Retain only specific validation methods if needed
        public static bool IsPointValidForGeography(Point point)
        {
            return point.X >= -180 && point.X <= 180 && point.Y >= -90 && point.Y <= 90;
        }

        public static bool IsPolygonValidForGeography(Polygon polygon)
        {
            foreach (Coordinate coord in polygon.Coordinates)
            {
                if (coord.X < -180 || coord.X > 180 || coord.Y < -90 || coord.Y > 90)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
