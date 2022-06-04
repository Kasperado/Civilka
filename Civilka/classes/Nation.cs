using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civilka.classes {
    class Nation {

        int id;
        public string name;
        public string nameAdjective;
        public Province capital;
        public List<Province> provinces = new List<Province>();
        public List<Edge> borders = new List<Edge>();
        public List<Edge> trueBorders = new List<Edge>(); // For rendering // TODO it would be better for them to be Points, but its harder to make an ordered list
        //this.mainCulture;
        public string color;
        public string borderColor;
        //this.relations = [];
        public Nation() {

        }

        public void addProvince(Province province) {
            this.provinces.Add(province);
            province.setOwner(this);
            this.updateBorder();
            this.trueNationsBorders();
        }

        private void updateBorder() {
            List<Edge> newBorders = new List<Edge>();
            // Go through every province to get which edges neighbor other countries
            for (int i = 0; i < this.provinces.Count; i++) {
                Province province = this.provinces[i];
                for (int n = 0; n < province.neighbors.Count; n++) {
                    Province neighbor = province.neighbors[n];
                    if (neighbor.owner == province.owner) continue; // Same nation
                    // Find which edge is it
                    Edge borderEdge = this.findBorderEdge(province.cell, neighbor.cell);
                    if (borderEdge != null) newBorders.Add(borderEdge);
                    else throw new Exception("Null edge somehow.");
                }
            }
            this.borders = newBorders;
        }

        Edge findBorderEdge(Cell c1, Cell c2) {
            for (int i = 0; i < c1.edges.Count; i++) {
                Edge c1e = c1.edges[i];
                for (int j = 0; j < c2.edges.Count; j++) {
                    Edge c2e = c2.edges[j];
                    // Same points at same positions within the edges means it's the same edge
                    bool vaBool = (c1e.va.site.x == c2e.va.site.x && c1e.va.site.y == c2e.va.site.y);
                    bool vbBool = (c1e.vb.site.x == c2e.vb.site.x && c1e.vb.site.y == c2e.vb.site.y);
                    if (vaBool && vbBool) return c1e;
                }
            }
            return null;
        }

        class BorderLink {

            public Point va;
            public Point vb;
            public BorderLink next = null;
            public Point nextPoint = null;
            public BorderLink back = null;
            public Point backPoint = null;
            public BorderLink(Point p1, Point p2) {
                this.va = p1;
                this.vb = p2;
            }
        }
        List<Edge> trueNationsBorders() {
            // Setup
            List<BorderLink> allBorders = new List<BorderLink>();
            for (int i = 0; i < this.borders.Count; i++) {
                Edge b = this.borders[i];
                allBorders.Add(new BorderLink(b.va.site, b.vb.site));
            }

            for (int i = 0; i < allBorders.Count; i++) {
                BorderLink currentBorder = allBorders[i];
                for (int j = 0; j < allBorders.Count; j++) {
                    BorderLink neighborBorder = allBorders[j];
                    if (currentBorder == neighborBorder) continue; // Same case
                    // Prepare cases
                    bool vavaBool = (currentBorder.va == neighborBorder.va);
                    bool vavbBool = (currentBorder.va == neighborBorder.vb);
                    bool vbvbBool = (currentBorder.vb == neighborBorder.vb);
                    bool vbvaBool = (currentBorder.vb == neighborBorder.va);
                    if (vavaBool) {
                        currentBorder.next = neighborBorder;
                        currentBorder.nextPoint = neighborBorder.va;
                    }
                    if (vavbBool) {
                        currentBorder.next = neighborBorder;
                        currentBorder.nextPoint = neighborBorder.vb;
                    }
                    if (vbvbBool) {
                        currentBorder.back = neighborBorder;
                        currentBorder.backPoint = neighborBorder.vb;
                    }
                    if (vbvaBool) {
                        currentBorder.back = neighborBorder;
                        currentBorder.backPoint = neighborBorder.va;
                    }

                    /*
                       bool vavaBool = (currentBorder.va.x == neighborBorder.va.x && currentBorder.va.y == neighborBorder.va.y);
                    bool vavbBool = (currentBorder.va.x == neighborBorder.vb.x && currentBorder.va.y == neighborBorder.vb.y);
                    bool vbvbBool = (currentBorder.vb.x == neighborBorder.vb.x && currentBorder.vb.y == neighborBorder.vb.y);
                    bool vbvaBool = (currentBorder.vb.x == neighborBorder.va.x && currentBorder.vb.y == neighborBorder.va.y);
                    
                     */

                }
            }

            //Console.WriteLine(allBorders.Count);

            for (int i = 0; i < allBorders.Count; i++) {
                BorderLink b = allBorders[i];
                //Console.WriteLine(b.back != null);
                //TODO fronts and backs are not going in one directions
            }
            // Start linking borders to create sets // For example having borders on two continents that are not connected
            List<List<BorderLink>> allSets = new List<List<BorderLink>>();


            return null;
        }
        // Finds province nearby and adds it to the civ
        public void expand() {
            bool addedProvince = false;
            List<Province> validProvinces = new List<Province>(this.provinces);
            while (!addedProvince) {
                int randProvinceIndex = this.closestActiveToCapital(validProvinces);
                Province randProvince = validProvinces[randProvinceIndex];
                // Check if neighbor is valid
                for (int j = 0; j < randProvince.neighbors.Count; j++) {
                    Province neighbor = randProvince.neighbors[j];
                    // Not owned
                    if (neighbor.owner != null) continue;
                    // Administrative reach
                    double distance = Misc.distanceBetweenPoints(this.capital.cell.site.x, this.capital.cell.site.y, neighbor.cell.site.x, neighbor.cell.site.y);
                    // All good
                    this.addProvince(neighbor);
                    addedProvince = true;
                    break; // Add only one
                }
                // This randProvince is used
                validProvinces.RemoveAt(randProvinceIndex);
                // All valid options exhausted
                if (validProvinces.Count == 0) break;
            }
        }
        // Create new same culture civ nearby
        void settle() {
            List<Province> validProvinces = new List<Province>(this.provinces);
            // Select province
            for (int i = 0; i < validProvinces.Count; i++) {
                Province province = validProvinces[i];
                // No owner
                if (province.owner != null) continue;
                // Neighbours have no owners
                bool nValid = true;
                for (int n = 0; n < province.neighbors.Count; n++) {
                    Province neighbor = province.neighbors[n];
                    if (neighbor.owner != null) nValid = false;
                }
                if (!nValid) continue;
                // Not too close not too far
                double distance = Misc.distanceBetweenPoints(this.capital.cell.site.x, this.capital.cell.site.y, province.cell.site.x, province.cell.site.y);
                if (distance > 40) continue;
                // Land route

                // Add new nation
                Nation nation = new Nation();
                string newColor = this.color;
                //if (random() < 0.2) newColor = adjust(newColor, random(-100, 100));
                nation.color = newColor;
                nation.capital = province;
                nation.addProvince(province);
                // TODO add nation
                //this.gameData.addNation(nation);
                return; // Stop function
            }
            // If new settle place not found then expand instead
            this.expand();
        }
        int closestActiveToCapital(List<Province> active) {
            Cell capital = active[0].owner.capital.cell;
            int closestIndex = 0;
            double closestDistance = double.PositiveInfinity; // Anything will be lower than infinity
            for (int i = 0; i < active.Count; i++) {
                Province province = active[i];
                double distance = Misc.distanceBetweenPoints(capital.site.x, capital.site.y, province.cell.site.x, province.cell.site.y);
                if (distance < closestDistance) {
                    closestIndex = i;
                    closestDistance = distance;
                }
            }
            return closestIndex;
        }

    }

}
