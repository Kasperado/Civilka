using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civilka.classes {
    class Culture {

           
        int id;
        List<Nation> nations = new List<Nation>();
        //this.parents = [];
        //this.children = [];
        bool isMixed = false;
        bool isCore = false;
        int sophisticationLevel = 0;
        //this.traits = [];
           
        void applyTraitsToNation() { }

        void applyTraitsToProvince() { }

        
    }
}
