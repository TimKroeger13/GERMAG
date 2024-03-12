using GeoAPI.Geometries;
using GERMAG.Shared;
using GERMAG.Shared.PointProperties;
using NetTopologySuite.Geometries;

namespace GERMAG.Server.GeometryCalculations;

public interface IGeoThermalProbesCalcualtion
{
    Task<int> CalculateGeoThermalProbes(Restricion RestrictionAreas);
}

public class GeoThermalProbesCalcualtion : IGeoThermalProbesCalcualtion
{
    public async Task<int> CalculateGeoThermalProbes(Restricion RestrictionAreas)
    {
        var geometryFactory = new GeometryFactory();

        NetTopologySuite.Geometries.Geometry? currentGeometry;
        NetTopologySuite.Geometries.Geometry? currentOutline;
        NetTopologySuite.Geometries.Coordinate[]? currentPoints;
        double? currentArea;

        List<NetTopologySuite.Geometries.Coordinate?> CandidatePoints = [];
        NetTopologySuite.Geometries.Geometry? CandidateMultiPoint;
        NetTopologySuite.Geometries.Geometry? CandidateBuffer;
        NetTopologySuite.Geometries.Geometry? CandidateBufferRing;
        NetTopologySuite.Geometries.Point? lastCurrentPoint;
        NetTopologySuite.Geometries.Geometry? CandidateGeometry;
        ProbePoint? CandidateChoosenPoint;
        NetTopologySuite.Geometries.Geometry? smallestAreaGeometry = null;
        int smallestAreaIndex;

        List<ProbePoint?> ReportGeothermalPoints = [];

        var centroid = RestrictionAreas.Geometry_Usable?.Centroid;

        //Inital cycle

        currentGeometry = RestrictionAreas?.Geometry_Usable;
        currentOutline = RestrictionAreas?.Geometry_Usable?.Boundary;
        currentPoints = RestrictionAreas?.Geometry_Usable?.Coordinates;
        currentArea = RestrictionAreas?.Geometry_Usable?.Area;

        if (currentArea == 0)
        {
            return 0;
        }

        double[] distances = new double[currentPoints!.Length];

        for (int i = 0; i < currentPoints?.Length; i++)
        {
            distances[i] = centroid?.Distance(new NetTopologySuite.Geometries.Point(currentPoints[i])) ?? 0;
        }

        int indexOfCandidate = Array.IndexOf(distances, distances.Max());

        if (indexOfCandidate != -1)
        {
            CandidatePoints.Add(currentPoints?[indexOfCandidate]);
        }

        if (CandidatePoints.Count == 0)
        {
            return 0;
        }

        //loop start

        smallestAreaIndex = -1;

        CandidateMultiPoint = new GeometryFactory().CreateMultiPointFromCoords(CandidatePoints.ToArray());

        CandidateBuffer = CandidateMultiPoint.Buffer(OfficalParameters.ProbeDistance + (OfficalParameters.ProbeDiameter / 2));

        CandidateGeometry = currentGeometry?.Intersection(CandidateBuffer);

        if (CandidateGeometry is MultiPolygon multiPolygon)
        {
            double smallestArea = double.MaxValue;

            var i = 0;

            foreach (var geometry in multiPolygon.Geometries)
            {
                double minArea = geometry.Area;

                if (minArea < smallestArea)
                {
                    smallestArea = minArea;
                    smallestAreaGeometry = geometry;
                    smallestAreaIndex = i;
                }
                i = i++;
            }
        }
        else if (CandidateGeometry is Polygon polygon)
        {
            smallestAreaGeometry = polygon;
            smallestAreaIndex = 0;
        }

        lastCurrentPoint = new GeometryFactory().CreatePoint(CandidatePoints[smallestAreaIndex]);

        CandidateChoosenPoint = new()
        {
            Geometry = lastCurrentPoint,
            Properties = new Shared.PointProperties.Properties { GeoPoten = null, ThermalCon = null }
        };

        ReportGeothermalPoints.Add(CandidateChoosenPoint);

        CandidatePoints.Clear();

        //Find Candiate Points in nearby geometry

        CandidateBufferRing = lastCurrentPoint.Buffer(OfficalParameters.ProbeDistance + (OfficalParameters.ProbeDiameter / 2)).Boundary;

        if (currentOutline is NetTopologySuite.Geometries.MultiLineString multiLineString)
        {
            foreach (var lineString in multiLineString.Geometries)
            {
                if(lineString is NetTopologySuite.Geometries.LinearRing && CandidateBufferRing is NetTopologySuite.Geometries.LinearRing)
                {
                    CandidatePoints = await FindNewCandidates(lineString, CandidateBufferRing, CandidatePoints);
                }
            }
        }
        else if (currentOutline is NetTopologySuite.Geometries.LinearRing SoloLineString)
        {
            CandidatePoints = await FindNewCandidates(SoloLineString, CandidateBufferRing, CandidatePoints);
        }

        //Update Data

        currentGeometry = currentGeometry?.Difference(smallestAreaGeometry);
        currentOutline = currentGeometry?.Boundary;
        currentPoints = currentGeometry?.Coordinates;
        currentArea = currentGeometry?.Area;

        if (currentArea == 0)
        {
            return 0;
        }

        /*        NetTopologySuite.Geometries.Geometry? intersectionResult = pointBuffer.Intersection(currentOutline);

                if (intersectionResult is NetTopologySuite.Geometries.MultiPoint multiPoint)
                {
                    // Extract points from the MultiPoint and add them to CandidatePoints
                    CandidatePoints.AddRange(multiPoint.Coordinates);
                }
                else if (intersectionResult is NetTopologySuite.Geometries.Point point)
                {
                    // Add the single point to CandidatePoints
                    CandidatePoints.Add(point.Coordinate);
                }*/

        //!!! Case for RestrictionAreas = 0

        return 1;
    }

    private async Task<List<NetTopologySuite.Geometries.Coordinate?>> FindNewCandidates(NetTopologySuite.Geometries.Geometry SearchLineRing, NetTopologySuite.Geometries.Geometry CandidateBufferRing, List<NetTopologySuite.Geometries.Coordinate?> CandidatePoints)
    {
        return await Task.Run(() =>
        {
            var lineStringIntersection = CandidateBufferRing?.Intersection(SearchLineRing);

            if (lineStringIntersection is NetTopologySuite.Geometries.Point point)
            {
                CandidatePoints.Add(point.Coordinate);
            }
            else if (lineStringIntersection is NetTopologySuite.Geometries.MultiPoint multiPoint)
            {
                foreach (var multiPointCoordinate in multiPoint.Coordinates)
                {
                    CandidatePoints.Add(multiPointCoordinate);
                }
            }
            else if (lineStringIntersection is NetTopologySuite.Geometries.GeometryCollection geometryCollection)
            {
                foreach (var geometry in geometryCollection.Geometries)
                {
                    if (geometry is NetTopologySuite.Geometries.Point collectionPoint)
                    {
                        CandidatePoints.Add(collectionPoint.Coordinate);
                    }
                }
            }

            return CandidatePoints;
        });
    }

}