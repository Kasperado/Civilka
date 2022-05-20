using Civilka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Civilka.classes.Enums;

namespace Civilka.classes {
    class Vertex {

        // Voronoi
        public Point site;
        public List<Vertex> neighbors = new List<Vertex>();
        public List<Edge> edges = new List<Edge>();
        public List<Cell> cells = new List<Cell>();
        // Geography
        public VertexType type;
        public int height = -1;
        public Vertex(Point site) {
            this.site = site;
        }
    }
}
