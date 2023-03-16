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
        List<Vertex>[] vGrid; // Stores all the vertices
        List<Cell>[] cGrid; // Stores all the cells by their site
        List<Cell>[] cGridFull; // Stores all the cells that intersect with given tile
        List<Edge>[] gridTilesEdges; // Stores edges of tiles
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
            this.w = 50;
            if (width % this.w != 0) throw new Exception("Width must be divisible by " + this.w);
            if (height % this.w != 0) throw new Exception("Height must be divisible by " + this.w);
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
            // Tile Edges
            this.gridTilesEdges = new List<Edge>[cols * rows];
            for (int i = 0; i < gridTilesEdges.Length; i++) {
                this.gridTilesEdges[i] = new List<Edge>();
                this.gridTilesEdges[i] = this.getGridListEdges(i);
        }
        }
        public List<Edge> getGridListEdges(int n) {
            List<Edge> listEdges = new List<Edge>();
            Edge temp;
            int truePosX = (n * this.w) % this.width;
            int truePosY = (int)Math.Floor(((double)n * this.w) / this.width) * this.w;
            // Top
            temp = new Edge();
            temp.va = new Vertex(new Point(truePosX, truePosY));
            temp.vb = new Vertex(new Point(truePosX + this.w, truePosY));
            listEdges.Add(temp);
            // Right
            temp = new Edge();
            temp.va = new Vertex(new Point(truePosX + this.w, truePosY));
            temp.vb = new Vertex(new Point(truePosX + this.w, truePosY + this.w));
            listEdges.Add(temp);
            // Bot
            temp = new Edge();
            temp.va = new Vertex(new Point(truePosX + this.w, truePosY + this.w));
            temp.vb = new Vertex(new Point(truePosX, truePosY + this.w));
            listEdges.Add(temp);
            // Left
            temp = new Edge();
            temp.va = new Vertex(new Point(truePosX, truePosY + this.w));
            temp.vb = new Vertex(new Point(truePosX, truePosY));
            listEdges.Add(temp);
            return listEdges;
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

        public void assignCellToGrid(Cell newCell) {
            List<int> tilesActive = new List<int>();
            List<int> tilesDone = new List<int>();
            tilesActive.Add(this.getGridPosition(newCell.site.x, newCell.site.y));
            int arrLen = cols * rows;
            while (tilesActive.Count > 0) {
                bool isCellInTile = false;
                int currentTileID = tilesActive[0];
                List<Cell> currentTile = this.cGridFull[currentTileID];
                List<Edge> tileEdges = this.gridTilesEdges[currentTileID];
                // Check if any tile vertex is in cell (vertices are ordered so check only va) - (big cells, small tiles)
                // If tile vertex is in cell then they must intersect
                for (int l = 0; l < tileEdges.Count; l++) {
                    if (Misc.isPointInPoly(tileEdges[l].va.site, newCell.edges)) {
                        isCellInTile = true;
                        break;
                    }
                }
                // Check the edges of the cell against edges of the grid list
                for (int e = 0; e < newCell.edges.Count; e++) {
                    if (isCellInTile) break; // Stop checking
                    Edge cellEdge = newCell.edges[e];
                    // Check if any cell vertex is in tile (va and vb are unordered so check both) - (small cells, big tiles)
                    if (Misc.isPointInPoly(cellEdge.va.site, tileEdges) || Misc.isPointInPoly(cellEdge.vb.site, tileEdges)) isCellInTile = true;
                    // Check if edges intersect (This is needed in some scenarios) - (usually in map corners)
                    for (int l = 0; l < tileEdges.Count; l++) {
                        if (isCellInTile) break; // Stop checking
                        Edge te = tileEdges[l];
                        if (Misc.lineIntersects(cellEdge.va.site.x, cellEdge.va.site.y, cellEdge.vb.site.x, cellEdge.vb.site.y, te.va.site.x, te.va.site.y, te.vb.site.x, te.vb.site.y)) isCellInTile = true;
                    }
                }
                // Expand search
                if (isCellInTile) {
                    currentTile.Add(newCell);
                    int[] neighbors = new int[] {
                        currentTileID + this.cols - 1, // Upper row
                        currentTileID + this.cols, 
                        currentTileID + this.cols + 1,
                        currentTileID - this.cols - 1, // Lower row
                        currentTileID - this.cols,
                        currentTileID - this.cols + 1,
                        currentTileID - 1, // Left
                        currentTileID + 1, // Right
                    };
                    // Check if neighbors are valid
                    for (int i = 0; i < neighbors.Length; i++) {
                        int n = neighbors[i];
                        if (!tilesDone.Contains(n) && !tilesActive.Contains(n) && n >= 0 && n < arrLen) tilesActive.Add(n);
                    }
                }
                // Move from active to done
                tilesDone.Add(currentTileID);
                tilesActive.RemoveAt(0);
            }
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
            int tilePosition = getGridPosition((double)this.mouseX, (double)this.mouseY);
            List<Cell> cellsInGrid = this.cGridFull[tilePosition];
            for (int i = 0; i < cellsInGrid.Count; i++) {
                Cell cell = cellsInGrid[i];
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
