using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Civilka.classes.Enums;

namespace Civilka.classes {
    class Province {

        public Cell cell;
        public Nation owner;
        public List<Province> neighbors = new List<Province>();
        /*
               // General
               Population
               Size (poly size?)
               Development value

               // Economy

               // Culture

               // Combat



               */

        // Location
        public ElevationLevel elevation;
        public double temperature;
        public double humidity;
                // Buildings
                //this.buildings = [];
        public Province(Cell cell) {
           this.cell = cell;
           cell.province = this;
        }

           public void setOwner(Nation nation) {
                this.owner = nation;
            }
    }

}
