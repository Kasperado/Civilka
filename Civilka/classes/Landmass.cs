using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civilka.classes {
    class Landmass {

        public List<Point> points = new List<Point>();
        public List<Edge> edges = new List<Edge>();

        public Landmass(List<Point> points, List<Edge> edges) { 
            this.points = points;
            this.edges = edges;
        }

    }
}
