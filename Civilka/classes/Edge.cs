using Civilka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Civilka.classes.Enums;

namespace Civilka.classes {
    class Edge {

        // Voronoi
        public Vertex va;
        public Vertex vb;
        public Cell toLeft;
        public Cell toRight;
        // Geography
        public bool isRiver = false;
        // Render
        public List<Edge> noisyEdges;
        public List<Quadrilateral> allQuadsDebug;

        public class Quadrilateral {

            public List<Point> points = new List<Point>(4);
            public Quadrilateral parent;
            public Quadrilateral leftChild;
            public Quadrilateral rightChild;
            public Point middlePoint;
            public Quadrilateral() { 

            }
        }
        public void createNoisyBorders(int detail = 2, double randomness = 0) {
            if (toLeft == null || toRight == null) return; // Map Edge
            // Setup
            List<Quadrilateral> allQuads = new List<Quadrilateral>();
            Quadrilateral initalQuad = new Quadrilateral();
            initalQuad.points.Add(va.site);
            initalQuad.points.Add(toLeft.site);
            initalQuad.points.Add(vb.site);
            initalQuad.points.Add(toRight.site);
            allQuads.Add(initalQuad);
            // Divide until max depth is reached
            List<Quadrilateral> activeQuads = new List<Quadrilateral>();
            activeQuads.Add(initalQuad);
            int depth = detail;
            while (depth > 0) {
                depth--;
                // Store all newly calculated quads for next loop
                List<Quadrilateral> newlyCreatedQuads = new List<Quadrilateral>();
                // Proccess all active quads
                for (int i = 0; i < activeQuads.Count; i++) {
                    Quadrilateral quad = activeQuads[i];
                    double amplitude = Misc.getRandomDouble(0.5 - randomness / 2, 0.5 + randomness / 2);
                    // Get middle point
                    Point middleV = Misc.mixPoints(quad.points[0], quad.points[2]); // Middle position of vertices
                    Point middleC = Misc.mixPoints(quad.points[1], quad.points[3], amplitude); // Middle position of cells
                    Point middlePoint = Misc.mixPoints(middleV, middleC); // Merge them
                    quad.middlePoint = middlePoint;
                    // Calculate left quad
                    Quadrilateral newQuadLeft = new Quadrilateral();
                    newQuadLeft.parent = quad;
                    quad.leftChild = newQuadLeft;
                    newQuadLeft.points.Add(quad.points[0]); // va
                    newQuadLeft.points.Add(Misc.mixPoints(quad.points[0], quad.points[1])); // toLeft
                    newQuadLeft.points.Add(middlePoint); // vb
                    newQuadLeft.points.Add(Misc.mixPoints(quad.points[3], quad.points[0])); // toRight
                    newlyCreatedQuads.Add(newQuadLeft);
                    allQuads.Add(newQuadLeft);
                    // Calculate right quad
                    Quadrilateral newQuadRight = new Quadrilateral();
                    newQuadRight.parent = quad;
                    quad.rightChild = newQuadRight;
                    newQuadRight.points.Add(middlePoint); // va
                    newQuadRight.points.Add(Misc.mixPoints(quad.points[1], quad.points[2])); // toLeft
                    newQuadRight.points.Add(quad.points[2]); // vb
                    newQuadRight.points.Add(Misc.mixPoints(quad.points[2], quad.points[3])); // toRight
                    newlyCreatedQuads.Add(newQuadRight);
                    allQuads.Add(newQuadRight);
                }
                // Move for the next loop
                activeQuads = newlyCreatedQuads;
            }
            // Calculate edges from quads
            List<Edge> finalEdges = new List<Edge>();
            int edgesAmount = (int)Math.Pow(2, detail);
            int startingPosition = allQuads.Count - edgesAmount;
            // Everythings is in layers and already ordered, I just need to start on right position
            for (int i = startingPosition; i < allQuads.Count; i++) {
                Quadrilateral quad = allQuads[i];
                Edge newEdge = new Edge();
                newEdge.va = new Vertex(quad.points[0]);
                newEdge.vb = new Vertex(quad.points[2]);
                finalEdges.Add(newEdge);
            }
            // Set
            noisyEdges = finalEdges;
            this.allQuadsDebug = allQuads;
        }

    }
}
