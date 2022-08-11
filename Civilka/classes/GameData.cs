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
        public int w;
        List<Vertex>[] vGrid;
        List<Cell>[] cGrid;
        List<Cell>[] cGridFull;
        int rows;
        public int rows;
        public int cols;
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
            this.cols = (int)Math.Floor((double)this.width / this.w);
            this.rows = (int)Math.Floor((double)this.height / this.w);
            // Veritices
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

        public int getGridPosition(double x, double y) {
            // Calculate in which grid cell those coordinates are
            int vCol = (int)Math.Floor(x / this.w);
            int vRow = (int)Math.Floor(y / this.w);
            // If point is on the right or bottom border I must decrease grid cell position by one to avoid index out of range error
            if (vCol == this.cols) vCol--;
            if (vRow == this.rows) vRow--;
            // Return position
            return (vCol + vRow * this.cols);
        }
        public void addNewVertex(Vertex newVertex) {
            // Since it's new vertex it needs to be checked agaist other ones
            List<Vertex> gridCell = this.vGrid[getGridPosition(newVertex.site.x, newVertex.site.y)];
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
            List<Cell> gridCell = this.cGrid[getGridPosition(newCell.site.x, newCell.site.y)];
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
        }
        public Vertex getVertexFromPosition(double vx, double vy) {
            List<Vertex> gridCell = this.vGrid[getGridPosition(vx, vy)];
            for (int i = 0; i < gridCell.Count; i++) {
                Vertex vertex = gridCell[i];
                if (vertex.site.x == vx && vertex.site.y == vy) return vertex;
            }
            return null;
        }

        public Cell getCellFromPosition(double cx, double cy) {
            List<Cell> gridCell = this.cGrid[getGridPosition(cx, cy)];
            for (int i = 0; i < gridCell.Count; i++) {
                Cell cell = gridCell[i];
                if (cell.site.x == cx && cell.site.y == cy) return cell;
            }
            return null;
        }
        public Cell getCellFromMouse() {
            // TODO (?) - This solution is not 'efficient', but it does the job quickly enough. (3ms with 10k cells)
            for (int i = 0; i < this.cells.Count; i++) {
                Cell cell = cells[i];
                if (Misc.isPointInPoly(new Point(this.mouseX, this.mouseY), cell.edges)) return cell;
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
