using GeoAPI.Geometries;
using GERMAG.Shared;
using GERMAG.Shared.PointProperties;
using NetTopologySuite.Geometries;
using System.Collections.Generic;

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
        NetTopologySuite.Geometries.MultiPoint? CandidateMultiPoint;
        //NetTopologySuite.Geometries.MultiPolygon? CandidateBuffer;
        NetTopologySuite.Geometries.Geometry? CandidateBufferRing;
        NetTopologySuite.Geometries.Point? lastCurrentPoint;
        //NetTopologySuite.Geometries.Geometry? CandidateGeometry;
        ProbePoint? CandidateChoosenPoint;
        NetTopologySuite.Geometries.Geometry? smallestAreaGeometry = null;
        int smallestAreaIndex;
        bool probePointGotRest = true;

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

        while (currentArea > 0)
        {
            CandidateMultiPoint = new GeometryFactory().CreateMultiPointFromCoords(CandidatePoints.ToArray());

            NetTopologySuite.Geometries.Polygon? candidateBuffer;
            List<NetTopologySuite.Geometries.Geometry?> combinedBufferCollection = [];

            foreach (var TempCandidatePoint in CandidateMultiPoint.Geometries)
            {
                candidateBuffer = (NetTopologySuite.Geometries.Polygon)TempCandidatePoint.Buffer(OfficalParameters.ProbeDistance + (OfficalParameters.ProbeDiameter / 2));

                var TempBufferIntersectionSingle = currentGeometry?.Intersection(candidateBuffer);

                if (TempBufferIntersectionSingle != null)
                {
                    combinedBufferCollection.Add(TempBufferIntersectionSingle);
                }
            }








/*            if (probePointGotRest && currentGeometry is not null)
            {
                // Use the first point from CandidateMultiPoint
                var singlePoint = (Point)CandidateMultiPoint[0];

                if (currentGeometry is Polygon polygon)
                {
                    // Subtract the single point from the polygon's vertices
                    Geometry resultGeometry = SubtractPointFromPolygon(polygon, singlePoint);

                    // Check if the result is valid
                    if (resultGeometry.IsValid)
                    {
                        // Update currentGeometry with the result
                        currentGeometry = resultGeometry;
                    }
                    else
                    {
                        // Handle the case where the result is invalid, e.g., set currentGeometry to null or take another appropriate action
                        currentGeometry = null;
                    }
                }
                else if (currentGeometry is MultiPolygon multiPolygon)
                {
                    // Iterate through all polygons in the MultiPolygon
                    List<Geometry> validPolygons = new List<Geometry>();

                    foreach (var subGeometry in multiPolygon.Geometries)
                    {
                        if (subGeometry is Polygon subPolygon)
                        {
                            // Subtract the single point from the sub-polygon's vertices
                            Geometry resultGeometry = SubtractPointFromPolygon(subPolygon, singlePoint);

                            // Check if the result is valid
                            if (resultGeometry.IsValid)
                            {
                                validPolygons.Add(resultGeometry);
                            }
                            // Handle the case where the result is invalid, if needed
                        }
                    }

                    // Create a new MultiPolygon with valid polygons
                    currentGeometry = validPolygons.Count > 0 ? new MultiPolygon(validPolygons.ToArray()) : null;
                }
            }

            // Helper method to subtract a point from a polygon's vertices
            private Geometry SubtractPointFromPolygon(Polygon polygon, Point point)
            {
                // Get the polygon's coordinates
                Coordinate[] coordinates = polygon.Coordinates;

                // Remove the point from the coordinates
                var remainingCoordinates = coordinates.Where(coord => !coord.Equals2D(point.Coordinate)).ToArray();

                // Create a new polygon with the remaining coordinates
                return remainingCoordinates.Length >= 3 ? new Polygon(new LinearRing(remainingCoordinates)) : null;
            }*/








            if (probePointGotRest)
            {


            }
            probePointGotRest = false;









            smallestAreaIndex = -1;
            double smallestArea = double.MaxValue;


            for (int i = 0; i < combinedBufferCollection.Count; i++)
            {
                var candidateGeometry = combinedBufferCollection[i];

                if (candidateGeometry is not null)
                {
                    if (!candidateGeometry.IsValid)
                    {
                        continue;
                    }

                    if (candidateGeometry is not MultiPolygon)
                    {
                        if (candidateGeometry is GeometryCollection geometryCollection)
                        {
                            List<NetTopologySuite.Geometries.Polygon> polygonsInGeometryCollection = [];

                            foreach (var Geometry in geometryCollection)
                            {
                                if (Geometry is Polygon localPolygon)
                                {
                                    polygonsInGeometryCollection.Add(localPolygon);
                                }
                            }
                            candidateGeometry = new NetTopologySuite.Geometries.MultiPolygon(polygonsInGeometryCollection.ToArray());
                        }
                    }

                    if (candidateGeometry is MultiPolygon multiPolygon)
                    {
                        double totalArea = 0.0;

                        foreach (var polygon in multiPolygon)
                        {
                            totalArea += polygon.Area;
                        }

                        if (totalArea < smallestArea)
                        {
                            smallestArea = totalArea;
                            smallestAreaGeometry = multiPolygon;
                            smallestAreaIndex = i;
                        }



                        /*                         double minArea = multiPolygon.Area;

                                                if (minArea < smallestArea)
                                                {
                                                    smallestArea = minArea;
                                                    smallestAreaGeometry = multiPolygon;
                                                    smallestAreaIndex = i;
                                                }*/
                    }
                    else if (candidateGeometry is NetTopologySuite.Geometries.Polygon polygon)
                    {
                        double minArea = polygon.Area;

                        if (minArea < smallestArea)
                        {
                            smallestArea = minArea;
                            smallestAreaGeometry = polygon;
                            smallestAreaIndex = i;
                        }
                    }
                }
            }


            Console.WriteLine(currentArea);
            Console.WriteLine(smallestArea);
            Console.WriteLine(" ");


            if (smallestArea >0.5 && smallestAreaGeometry!.IsValid)
            {
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
                        if (lineString is NetTopologySuite.Geometries.LinearRing && CandidateBufferRing is NetTopologySuite.Geometries.LinearRing)
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

                if (currentGeometry is MultiPolygon multiPolygon)
                {
                    List<Polygon> validPolygons = new List<Polygon>();

                    foreach (var polygon in multiPolygon.Geometries.OfType<Polygon>())
                    {
                        double polygonArea = polygon.Area;

                        if (polygonArea >= 1)
                        {
                            validPolygons.Add(polygon);
                        }
                    }

                    currentGeometry = validPolygons.Count > 0 ? new MultiPolygon(validPolygons.ToArray()) : null;
                }
                else if (currentGeometry is Polygon polygon)
                {
                    double polygonArea = polygon.Area;

                    if (polygonArea < 1)
                    {
                        currentGeometry = null;
                    }
                }

                currentOutline = currentGeometry?.Boundary;
                currentPoints = currentGeometry?.Coordinates;
                currentArea = currentGeometry?.Area;

            }
            else
            {
                CandidatePoints.Clear();
            }

            if (currentArea == 0)
            {
                return 0;
            }

            if (CandidatePoints.Count == 0)
            {
                distances = new double[currentPoints!.Length];

                for (int i = 0; i < currentPoints?.Length; i++)
                {
                    distances[i] = centroid?.Distance(new NetTopologySuite.Geometries.Point(currentPoints[i])) ?? 0;
                }

                indexOfCandidate = Array.IndexOf(distances, distances.Max());
                probePointGotRest = true;

                if (indexOfCandidate != -1)
                {
                    CandidatePoints.Add(currentPoints?[indexOfCandidate]);
                }

                if (CandidatePoints.Count == 0)
                {
                    return 0;
                }
            }
        }


        

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