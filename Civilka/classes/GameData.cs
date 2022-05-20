using Civilka;
using System;
using System.Collections.Generic;

namespace Civilka.classes {
    class GameData {

        // Game Data
        public int width;
        public int height;
        public List<Cell> landCells = new List<Cell>();
        public List<Cell> waterCells = new List<Cell>();
        public int mouseX = 0;
        public int mouseY = 0;
        // Transfer acceleration | Uniform grids will help with that // 3000ms -> 75ms on 1400:800 (10 mDistance)
        int w;
        List<Vertex>[] vGrid;
        List<Cell>[] cGrid;
        List<Cell>[] cGridFull;
        // Voronoi
        public List<Point> points = new List<Point>();
        public List<Vertex> vertices = new List<Vertex>();
        public List<Edge> edges = new List<Edge>();
        public List<Cell> cells = new List<Cell>();
        private int cellID = 0;
        // Game
        public List<Landmass> landmasses = new List<Landmass>();
        public List<River> rivers = new List<River>();
        public List<Province> provinces = new List<Province>();
        public List<Nation> nations = new List<Nation>();
        public List<Culture> cultures = new List<Culture>();
        public Nation wasteland; // Nation with all impassable terrain
        // Render
        bool needRenderUpdate = false;
        //
        public Cell selectedCell;
        public List<Cell> path;
        // Image user chosen by user
        public System.Drawing.Bitmap imageLand;

        public void init() {
            // Get w
            if (width % 100 != 0) throw new Exception("Width must be divisible by 100");
            if (height % 100 != 0) throw new Exception("Height must be divisible by 100");
            this.w = 20;
            // Veritices
            int cols = (int)Math.Floor((double)this.width / this.w);
            int rows = (int)Math.Floor((double)this.height / this.w);
            this.vGrid = new List<Vertex>[cols * rows];
            for (int i = 0; i < vGrid.Length; i++) {
                this.vGrid[i] = new List<Vertex>();
            }
            // Cell Site
            this.cGrid = new List<Cell>[cols * rows];
            for (int i = 0; i < cGrid.Length; i++) {
                this.cGrid[i] = new List<Cell>();
            }
            // Cell Full
            this.cGridFull = new List<Cell>[cols * rows];
            for (int i = 0; i < cGridFull.Length; i++) {
                this.cGridFull[i] = new List<Cell>();
            }
        }
        public void addNewVertex(Vertex newVertex) {
            // Since it's new vertex it needs to be checked agaist other ones
            int vCol = (int)Math.Floor(newVertex.site.x / this.w);
            int vRow = (int)Math.Floor(newVertex.site.y / this.w);
            List<Vertex> gridCell = this.vGrid[vCol * vRow];
            for (int i = 0; i < gridCell.Count; i++) {
                bool sameX = (gridCell[i].site.x == newVertex.site.x);
                bool sameY = (gridCell[i].site.y == newVertex.site.y);
                if (sameX && sameY) return; // Same Vertex
            }
            // If none of the vertices are the same
            gridCell.Add(newVertex); // Add to grid cell
            this.vertices.Add(newVertex); // Add to list
        }

        public void addNewCell(Cell newCell) {
            // Since it's new cell it needs to be checked agaist other ones
            int vCol = (int)Math.Floor(newCell.site.x / this.w);
            int vRow = (int)Math.Floor(newCell.site.y / this.w);
            List<Cell> gridCell = this.cGrid[vCol * vRow];
            for (int i = 0; i < gridCell.Count; i++) {
                bool sameX = (gridCell[i].site.x == newCell.site.x);
                bool sameY = (gridCell[i].site.y == newCell.site.y);
                if (sameX && sameY) return; // Same Vertex
            }
            // If none of the cells are the same
            this.cellID++;
            newCell.id = cellID;
            gridCell.Add(newCell); // Add to grid cell
            this.cells.Add(newCell); // Add to list

            // Start with initial one and if it breaks out add to active
            // TODO uniform grid
            List<List<Cell>> activeGridCells = new List<List<Cell>>();
            //activeGridCells.Add(gridCell); // Add initial one
            while (activeGridCells.Count > 0) {
                List<Cell> active = activeGridCells[0];
            }
            return;
            for (int i = 0; i < cGridFull.Length; i++) {
               
            }

            Point nw = new Point(vCol * this.w, vRow * this.w);
            Point ne = new Point(vCol * this.w + this.w, vRow * this.w);
            Point se = new Point(vCol * this.w + this.w, vRow * this.w + this.w);
            Point sw = new Point(vCol * this.w, vRow * this.w + this.w);
            for (int i = 0; i < newCell.edges.Count; i++) {
                Edge edge = newCell.edges[i];
                // North
                if (Misc.lineIntersects(nw.x, nw.y, ne.x, ne.y, edge.va.site.x, edge.va.site.y, edge.vb.site.x, edge.vb.site.y)) {

                }
                // East
                if (Misc.lineIntersects(ne.x, ne.y, se.x, se.y, edge.va.site.x, edge.va.site.y, edge.vb.site.x, edge.vb.site.y)) {

                }
                // South
                if (Misc.lineIntersects(se.x, se.y, sw.x, sw.y, edge.va.site.x, edge.va.site.y, edge.vb.site.x, edge.vb.site.y)) {

                }
                // West
                if (Misc.lineIntersects(sw.x, sw.y, nw.x, nw.y, edge.va.site.x, edge.va.site.y, edge.vb.site.x, edge.vb.site.y)) {

                }
            }


            if (true) {
                this.cGridFull[vCol * vRow].Add(newCell);
            }

        }
        public Vertex getVertexFromPosition(double vx, double vy) {
            int vCol = (int)Math.Floor(vx / this.w);
            int vRow = (int)Math.Floor(vy / this.w);
            List<Vertex> gridCell = this.vGrid[vCol * vRow];
            for (int i = 0; i < gridCell.Count; i++) {
                Vertex vertex = gridCell[i];
                if (vertex.site.x == vx && vertex.site.y == vy) return vertex;
            }
            return null;
        }

        public Cell getCellFromPosition(double cx, double cy) {
            int vCol = (int)Math.Floor(cx / this.w);
            int vRow = (int)Math.Floor(cy / this.w);
            List<Cell> gridCell = this.cGrid[vCol * vRow];
            for (int i = 0; i < gridCell.Count; i++) {
                Cell cell = gridCell[i];
                if (cell.site.x == cx && cell.site.y == cy) return cell;
            }
            return null;
        }
        public Cell getCellFromMouse() {
            int vCol = (int)Math.Floor((double)mouseX / this.w);
            int vRow = (int)Math.Floor((double)mouseY / this.w);
            List<Cell> gridCell = this.cGridFull[vCol * vRow];
            // TODO
            for (int i = 0; i < this.cells.Count; i++) {
                Cell cell = cells[i];
                if (Misc.isPointInPoly(new Point(mouseX, mouseY), cell.edges)) return cell;
            }
            return null;
        }

        public void addNation(Nation nation) {
            //nation.id = this.nationCounter;
            this.nations.Add(nation);
            this.needRenderUpdate = true;
        }
    }

}
