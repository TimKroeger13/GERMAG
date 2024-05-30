using GeoAPI.Geometries;
using GERMAG.Shared;
using GERMAG.Shared.PointProperties;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace GERMAG.Server.GeometryCalculations;

public interface IGeoThermalProbesCalcualtion
{
    Task<List<ProbePoint?>> CalculateGeoThermalProbes(Restricion RestrictionAreas);
}

public class GeoThermalProbesCalcualtion : IGeoThermalProbesCalcualtion
{
    public async Task<List<ProbePoint?>> CalculateGeoThermalProbes(Restricion RestrictionAreas)
    {
        var geometryFactory = new GeometryFactory();
        var geoJsonWriter = new GeoJsonWriter();

        NetTopologySuite.Geometries.Geometry? currentGeometry;
        NetTopologySuite.Geometries.Geometry? currentOutline;
        NetTopologySuite.Geometries.Coordinate[]? currentPoints;
        double? currentArea;

        List<NetTopologySuite.Geometries.Coordinate?> CandidatePoints = [];
        NetTopologySuite.Geometries.MultiPoint? CandidateMultiPoint;
        NetTopologySuite.Geometries.Geometry? CandidateBufferRing;
        NetTopologySuite.Geometries.Point? lastCurrentPoint;
        ProbePoint? CandidateChoosenPoint;
        NetTopologySuite.Geometries.Geometry? smallestAreaBuffer = null;
        int smallestAreaIndex;
        double[] distances;
        int indexOfCandidate;

        List<ProbePoint?> ReportGeothermalPoints = [];

        //check for to Large Areas

        if (RestrictionAreas.Geometry_Usable?.Area > OfficalParameters.MaximalAreaSizeForCalculations)
        { throw new Exception("Selected Area is to Large"); }

        //update Geometry

        RestrictionAreas.Geometry_Usable = RestrictionAreas?.Geometry_Usable?.Buffer(-(OfficalParameters.ProbeDiameter / 2));

        var centroid = RestrictionAreas?.Geometry_Usable?.Centroid;

        //Inital cycle

        currentGeometry = RestrictionAreas?.Geometry_Usable;
        currentOutline = RestrictionAreas?.Geometry_Usable?.Boundary;
        currentPoints = RestrictionAreas?.Geometry_Usable?.Coordinates;
        currentArea = RestrictionAreas?.Geometry_Usable?.Area;

        if (currentArea == 0)
        {
            return ReportGeothermalPoints;
        }

        distances = new double[currentPoints!.Length];

        for (int i = 0; i < currentPoints?.Length; i++)
        {
            distances[i] = centroid?.Distance(new NetTopologySuite.Geometries.Point(currentPoints[i])) ?? 0;
        }

        indexOfCandidate = Array.IndexOf(distances, distances.Max());

        if (indexOfCandidate != -1)
        {
            CandidatePoints.Add(currentPoints?[indexOfCandidate]);
        }

        if (CandidatePoints.Count == 0)
        {
            return ReportGeothermalPoints;
        }

        //loop start

        while (currentArea > 0)
        {
            CandidateMultiPoint = new GeometryFactory().CreateMultiPointFromCoords(CandidatePoints.ToArray());

            NetTopologySuite.Geometries.Polygon? candidateBuffer;
            List<NetTopologySuite.Geometries.Geometry?> combinedIntersectionCollection = [];
            List<NetTopologySuite.Geometries.Geometry?> combinedBufferCollection = [];

            foreach (var TempCandidatePoint in CandidateMultiPoint.Geometries)
            {
                candidateBuffer = (NetTopologySuite.Geometries.Polygon)TempCandidatePoint.Buffer(OfficalParameters.ProbeDistance + (OfficalParameters.ProbeDiameter / 2));

                var TempBufferIntersectionSingle = currentGeometry?.Intersection(candidateBuffer);

                if (TempBufferIntersectionSingle != null)
                {
                    combinedIntersectionCollection.Add(TempBufferIntersectionSingle);
                    combinedBufferCollection.Add(candidateBuffer);
                }
            }

            smallestAreaIndex = -1;
            double smallestArea = double.MaxValue;

            for (int i = 0; i < combinedIntersectionCollection.Count; i++)
            {
                var candidateGeometry = combinedIntersectionCollection[i];

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
                            smallestAreaBuffer = combinedBufferCollection[i];
                            smallestAreaIndex = i;
                        }
                    }
                    else if (candidateGeometry is NetTopologySuite.Geometries.Polygon polygon)
                    {
                        double minArea = polygon.Area;

                        if (minArea < smallestArea)
                        {
                            smallestArea = minArea;
                            smallestAreaBuffer = combinedBufferCollection[i];
                            smallestAreaIndex = i;
                        }
                    }
                }
            }

            if (smallestAreaBuffer!.IsValid)
            {
                lastCurrentPoint = new GeometryFactory().CreatePoint(CandidatePoints[smallestAreaIndex]);

                CandidateChoosenPoint = new()
                {
                    Geometry = lastCurrentPoint,
                    GeometryJson = geoJsonWriter.Write(lastCurrentPoint),
                    Properties = new Shared.PointProperties.Properties { GeoPoten = null, MaxDepth = null, GeoPotenDepth = null }
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

                currentGeometry = currentGeometry?.Difference(smallestAreaBuffer);

                if (currentGeometry is MultiPolygon multiPolygon)
                {
                    List<Polygon> validPolygons = new();

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
                if (currentGeometry is Polygon || currentGeometry is MultiPolygon)
                {
                    currentOutline = currentGeometry?.Boundary;
                    currentPoints = currentGeometry?.Coordinates;
                    currentArea = currentGeometry?.Area;
                }else
                {
                    currentOutline = null;
                    currentPoints = null;
                    currentArea = 0;
                }
            }
            else
            {
                CandidatePoints.Clear();
            }

            if (currentArea == 0 || currentPoints == null)
            {
                return ReportGeothermalPoints;
            }

            if (CandidatePoints.Count == 0)
            {
                distances = new double[currentPoints!.Length];

                for (int i = 0; i < currentPoints?.Length; i++)
                {
                    distances[i] = centroid?.Distance(new NetTopologySuite.Geometries.Point(currentPoints[i])) ?? 0;
                }

                indexOfCandidate = Array.IndexOf(distances, distances.Max());

                if (indexOfCandidate != -1)
                {
                    CandidatePoints.Add(currentPoints?[indexOfCandidate]);
                }

                if (CandidatePoints.Count == 0 || currentPoints == null)
                {
                    return ReportGeothermalPoints;
                }
            }
        }

        return ReportGeothermalPoints;
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
                CandidatePoints.AddRange(multiPoint.Coordinates);
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