using Civilka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Civilka.classes.Enums;

namespace Civilka.classes {
    class Cell {

        public int id;
        public Point site;
        public List<Vertex> vertices = new List<Vertex>();
        public List<Edge> edges = new List<Edge>();
        public Province province; // Owner of this cell
        public List<Cell> neighbors = new List<Cell>();
        public CellType type;
        public Cell(Point site) {
            //this.id = id;
            this.site = site;
        }
    }
}
