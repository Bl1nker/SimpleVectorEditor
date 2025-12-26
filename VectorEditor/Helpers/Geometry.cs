using System.Windows;

namespace VectorEditor.Helpers;

public static class Geometry
{
    public static double PointToSegmentDistance(Point p, Point a, Point b)
    {
        var ap = new Vector(p.X - a.X, p.Y - a.Y);
        var ab = new Vector(b.X - a.X, b.Y - a.Y);
        var abLengthSq = ab.LengthSquared;

        if (abLengthSq == 0)
        {
            return (p - a).Length;
        }

        var t = Vector.Multiply(ap, ab) / abLengthSq;
        t = Math.Max(0, Math.Min(1, t));

        var closest = new Point(a.X + t * ab.X, a.Y + t * ab.Y);

        return (closest - p).Length;
    }

    public static bool IsPointOnPolyline(System.Windows.Media.PointCollection points, Point pt, double tolerance = 10.0)
    {
        for (int i = 1; i < points.Count; i++)
        {
            if (PointToSegmentDistance(pt, points[i - 1], points[i]) <= tolerance)
                return true;
        }
        return false;
    }

    public static (int segmentIndex, Point projection)? FindNearestSegmentInsertion(System.Windows.Media.PointCollection points, Point clickPoint, double maxDistance = 15.0)
    {
        double minDist = double.MaxValue;
        int bestIndex = -1;
        Point bestInsert = new();

        for (int i = 1; i < points.Count; i++)
        {
            var p1 = points[i - 1];
            var p2 = points[i];
            var dist = PointToSegmentDistance(clickPoint, p1, p2);
            if (dist < minDist)
            {
                minDist = dist;
                bestIndex = i;

                var ap = new Vector(clickPoint.X - p1.X, clickPoint.Y - p1.Y);
                var ab = new Vector(p2.X - p1.X, p2.Y - p1.Y);
                var t = ab.LengthSquared == 0 ? 0 : Vector.Multiply(ap, ab) / ab.LengthSquared;
                t = Math.Max(0, Math.Min(1, t));

                bestInsert = new Point(p1.X + t * ab.X, p1.Y + t * ab.Y);
            }
        }
        
        if (bestIndex >= 0 && minDist <= maxDistance)
        {
            return (bestIndex, bestInsert);
        }

        return null;
    }
}
