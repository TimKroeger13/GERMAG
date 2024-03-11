using GeoAPI.Geometries;
using GERMAG.Shared;
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
        NetTopologySuite.Geometries.Geometry? currentGeometry;
        NetTopologySuite.Geometries.Geometry? currentOutline;
        NetTopologySuite.Geometries.Coordinate[]? currentPoints;
        double? currentArea;

        List<NetTopologySuite.Geometries.Coordinate?> CandidatePoints = [];
        NetTopologySuite.Geometries.Geometry? CandidateMultiPoint;
        NetTopologySuite.Geometries.Geometry? CandidateBuffer;
        NetTopologySuite.Geometries.Geometry? CandidateGeometry;
        NetTopologySuite.Geometries.Geometry? CandidateChoosenPoint;
        NetTopologySuite.Geometries.Geometry? smallestAreaGeometry = null;
        int smallestAreaIndex;

        var centroid = RestrictionAreas.Geometry_Usable?.Centroid;

        //Inital cycle

        currentOutline = RestrictionAreas?.Geometry_Usable?.Boundary;
        currentPoints = RestrictionAreas?.Geometry_Usable?.Coordinates;
        currentGeometry = RestrictionAreas?.Geometry_Usable;
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

        if(CandidatePoints.Count == 0)
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

        CandidateChoosenPoint = new GeometryFactory().CreatePoint(CandidatePoints[0]);


        //<= Save CandidateChoosenPoint in a List of a self defined class Structure. 

        //!!! Case for RestrictionAreas = 0




        return 1;
    }
}
