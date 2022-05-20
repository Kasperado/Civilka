using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Civilka.classes.Enums;

namespace Civilka.classes {
     class River {

            public List<Vertex> vertices = new List<Vertex>();
            public River(Vertex vertex) {
                this.vertices.Add(vertex);
            }
    }
}
