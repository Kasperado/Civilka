using Civilka.classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Point = Civilka.classes.Point;

namespace Civilka {
    class Misc {

        static readonly Random seed = new Random();
        public static int seedUsed = seed.Next(50000);
        static readonly Random rng = new Random(seedUsed);
        public static Stopwatch stopWatch = new Stopwatch();

        public static double getRandomDouble(double min, double max) { 
            return rng.NextDouble() * (max - min) + min;
        }
        // Inclusive
        public static int getRandomInt(int min, int max) {
            return rng.Next(min, max + 1);
        }
        public static bool isPointInPoly(Point point, List<Edge> edges) {
            bool inCell = false;
            // Raycast line
            Point[] rayCast = { new Point(-1000, -1000), new Point(point.x, point.y) };
            // Every all edges
            for (int i = 0; i < edges.Count; i++) {
                Edge e = edges[i];
                if (lineIntersects(rayCast[0].x, rayCast[0].y, rayCast[1].x, rayCast[1].y, e.va.site.x, e.va.site.y, e.vb.site.x, e.vb.site.y)) inCell = !inCell;
            }
            return inCell;
        }

        // Returns true if the line from (a,b)->(c,d) intersects with (p,q)->(r,s)
        public static bool lineIntersects(double a, double b, double c, double d, double p, double q, double r, double s) {
            double det, gamma, lambda;
            det = (c - a) * (s - q) - (r - p) * (d - b);
            if (det == 0) {
                return false;
            } else {
                lambda = ((s - q) * (r - a) + (p - r) * (s - b)) / det;
                gamma = ((b - d) * (r - a) + (c - a) * (s - b)) / det;
                return (0 < lambda && lambda < 1) && (0 < gamma && gamma < 1);
            }
        }

        public static double distanceBetweenPoints(double x1, double y1, double x2, double y2) {
            double a = x1 - x2;
            double b = y1 - y2;
            return Math.Sqrt(a * a + b * b);
        }

        public static Point mixPoints(Point p1, Point p2, double favor = 0.5) {
            double newX = p1.x * (1 - favor) + p2.x * favor;
            double newY = p1.y * (1 - favor) + p2.y * favor;
            Point newPoint = new Point(newX, newY);
            return newPoint;
        }
        public static double getAngle(double  p1x, double  p1y, double p2x, double p2y) {
            double angle = Math.Atan2(p2y - p1y, p2x - p1x); // range (-PI, PI]
            angle *= 180 / Math.PI; // rads to degs, range (-180, 180]
            if (angle < 0) angle = 360 + angle; // range [0, 360)
            return angle;
        }

        public static string getRandomHexColor() {
            return String.Format("#{0:X6}", rng.Next(0x1000000));
        }

        public static string adjustColor(string color, double amount) {
            double power = 1 + amount; // Amount -1.0 to 1.0
            if (power < 0) power = 0;
            Color c1 = ColorTranslator.FromHtml(color);
            int newR = (int)(c1.R * power);
            if (newR > 255) newR = 255;
            int newG = (int)(c1.G * power);
            if (newG > 255) newG = 255;
            int newB = (int)(c1.B * power);
            if (newB > 255) newB = 255;
            Color c2 = Color.FromArgb(c1.A, newR, newG, newB);
            return ColorTranslator.ToHtml(c2);
}

    }
}
