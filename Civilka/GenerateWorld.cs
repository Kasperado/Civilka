using Civilka.classes;
using System;
using System.Collections.Generic;
using System.Drawing;
using VoronoiLib.Structures;
using static Civilka.classes.Enums;
using Point = Civilka.classes.Point;


namespace Civilka {

    class WorldGeneration {
        public static List<Point> PoissonDiscDistribution(int width, int height, int minDistance, int maxTries, int boundaryOffset) {
            // Setup
            double w = minDistance / Math.Sqrt(2);
            int cols, rows;
            int biggerDimension = (width > height) ? width : height;
            List<Point> active = new List<Point>();
            List<Point> ordered = new List<Point>();
            // STEP-0 - Grid for storing data and accelerating spatial searches
            cols = (int)Math.Floor(width / w);
            rows = (int)Math.Floor(height / w);
            Point[] grid = new Point[cols * rows];
            for (int i = 0; i < grid.Length; i++) {
                grid[i] = null;
            }
            // STEP-1 - Select the initial candidate point (from middle)
            int x = width / 2;
            int y = height / 2;
            int gi = (int)Math.Floor(x / w);
            int gj = (int)Math.Floor(y / w);
            Point middlePoint = new Point(x, y);
            grid[gi + gj * cols] = middlePoint;
            active.Add(middlePoint);
            // STEP-2 - Generate next points from initial candidate
            while (active.Count > 0) {
                // Choose randomly active point
                int randIndex = Misc.getRandomInt(0, active.Count - 1);
                Point current = active[randIndex];
                bool nextFound = false; // Flag if next point was found
                // Try to put points near active one
                for (int n = 0; n < maxTries; n++) {
                    // Create new point by moving it from current position randomly
                    Point candidate = new Point(current.x, current.y);
                    Point angle = new Point(Misc.getRandomDouble(-1, 1), Misc.getRandomDouble(-1, 1)); // Random angle (random x and y between (-1,1))
                    double power = minDistance + (Misc.getRandomDouble(0, 1) * minDistance); // Power between 100% and 200%
                    candidate.x += angle.x * power;
                    candidate.y += angle.y * power;
                    // Check if is in the grid
                    int col = (int)Math.Floor(candidate.x / w);
                    int row = (int)Math.Floor(candidate.y / w);
                    bool isInMapBoundaries = (col > -1 && row > -1 && col < cols && row < rows);
                    if (!isInMapBoundaries) continue;
                    bool isTooCloseToBoundaries = !(boundaryOffset <= candidate.x &&
                                                  candidate.x <= width - boundaryOffset &&
                                                  boundaryOffset <= candidate.y &&
                                                  candidate.y <= height - boundaryOffset);
                    bool isSpaceFree = (grid[col + row * cols] == null);
                    // Test
                    if (isInMapBoundaries && !isTooCloseToBoundaries && isSpaceFree) {
                        bool isOk = true; // Flag
                        // Check for any of neighbors in grid is too close
                        for (int i = -1; i <= 1; i++) {
                            for (int j = -1; j <= 1; j++) {
                                // Test if neighboring cell would be out of grid index
                                if ((col + i) >= cols || (row + j) >= rows) continue; // Too high (Right, Bot)
                                if ((col + i) < 0 || (row + j) < 0) continue; // Too low (Top, Left)
                                // Get neighbor index
                                int index = (col + i) + (row + j) * cols;
                                // Check grid cell is occupied
                                Point neighbor = grid[index];
                                if (neighbor != null) {
                                    // Check if point is too close
                                    double d = Math.Sqrt(Math.Pow(candidate.x - neighbor.x, 2) + Math.Pow(candidate.y - neighbor.y, 2));
                                    // Distance check
                                    if (d < minDistance) {
                                        isOk = false;
                                        break; // No need to check others
                                    }
                                }
                            }
                        }
                        // Should be added to list?
                        if (isOk) {
                            nextFound = true;
                            grid[col + row * cols] = candidate;
                            active.Add(candidate);
                            ordered.Add(candidate);
                            break;
                        }
                    }
                }
                // Remove active point if next one wasn't found
                if (!nextFound) active.Remove(current);
            }
            // Return  points
            return ordered;
        }
        public static void TransferData(GameData gameData, LinkedList<VEdge> dataEdges) {
            // Vertex | VPoints
            LinkedListNode<VEdge> ve = dataEdges.First;
            while ((ve != null)) {
                // Extract data from node
                VEdge vev = ve.Value;
                // Add Start and End vertices
                gameData.addNewVertex(new Vertex(new Point(vev.Start.X, vev.Start.Y)));
                gameData.addNewVertex(new Vertex(new Point(vev.End.X, vev.End.Y)));
                // Set next
                ve = ve.Next;
            }
            // Cells | FortuneSite
            LinkedListNode<VEdge> ve2 = dataEdges.First;
            while ((ve2 != null)) {
                // Extract data from node
                VEdge vev = ve2.Value;
                // Add Left and Right cells
                gameData.addNewCell(new Cell(new Point(vev.Left.X, vev.Left.Y)));
                gameData.addNewCell(new Cell(new Point(vev.Right.X, vev.Right.Y)));
                // Set next
                ve2 = ve2.Next;
            }
            // Edges + Extra | VEdge
            LinkedListNode<VEdge> ve3 = dataEdges.First;
            while ((ve3 != null)) {
                // Extract data from node
                VEdge vev = ve3.Value;
                //
                Edge newEdge = new Edge();
                // Copy values
                newEdge.va = gameData.getVertexFromPosition(vev.Start.X, vev.Start.Y);
                newEdge.vb = gameData.getVertexFromPosition(vev.End.X, vev.End.Y);
                newEdge.toLeft = vev.Left != null ? gameData.getCellFromPosition(vev.Left.X, vev.Left.Y) : null;
                newEdge.toRight = vev.Right != null ? gameData.getCellFromPosition(vev.Right.X, vev.Right.Y) : null;
                // Add edges reference to neighboring cells
                if (newEdge.toLeft != null) newEdge.toLeft.edges.Add(newEdge);
                if (newEdge.toRight != null) newEdge.toRight.edges.Add(newEdge);
                // Add edge reference to vertices
                newEdge.va.edges.Add(newEdge);
                newEdge.vb.edges.Add(newEdge);
                // Add to game
                gameData.edges.Add(newEdge);
                // Add vertex<->cell references
                if (newEdge.toLeft != null) {
                    // Add cells reference to vertices
                    addVertexToCell(newEdge.toLeft, newEdge.va);
                    addVertexToCell(newEdge.toLeft, newEdge.vb);
                }
                if (newEdge.toRight != null) {
                    // Add cells reference to vertices
                    addVertexToCell(newEdge.toRight, newEdge.va);
                    addVertexToCell(newEdge.toRight, newEdge.vb);
                }
                // Set next
                ve3 = ve3.Next;
            }
            // Add cells as neighbors using edges
            for (int i = 0; i < gameData.edges.Count; i++) {
                Edge edge = gameData.edges[i];
                // This Voronoi implementation does not have any null edges
                // if (edge.toLeft != null || edge.toRight != null) { }
                // Add eachother as neighbors
                Cell rs = gameData.getCellFromPosition(edge.toRight.site.x, edge.toRight.site.y);
                Cell ls = gameData.getCellFromPosition(edge.toLeft.site.x, edge.toLeft.site.y);
                rs.neighbors.Add(ls);
                ls.neighbors.Add(rs);
            }
            // Sort vertices for drawing
            for (int i = 0; i < gameData.cells.Count; i++) {
                sortVertices(gameData.cells[i], gameData.cells[i].vertices);
            }
            // Add vertex neighbors
            for (int i = 0; i < gameData.edges.Count; i++) {
                Edge edge = gameData.edges[i];
                edge.va.neighbors.Add(edge.vb);
                edge.vb.neighbors.Add(edge.va);
            }
            // Create new vertices and edges on borders to close map
            // Create corners of map boudaries
            gameData.vertices.Add(new Vertex(new Point(0, 0))); // northWest
            gameData.vertices.Add(new Vertex(new Point(gameData.width, 0))); // northEast
            gameData.vertices.Add(new Vertex(new Point(gameData.width, gameData.height))); // southEast
            gameData.vertices.Add(new Vertex(new Point(0, gameData.height))); // southWest
            // Create lists for convinience
            List<Vertex> northVertices = new List<Vertex>();
            List<Vertex> eastVertices = new List<Vertex>();
            List<Vertex> southVertices = new List<Vertex>();
            List<Vertex> westVertices = new List<Vertex>();
            // Assign existing vertices to those lists
            for (int i = 0; i < gameData.vertices.Count; i++) {
                Vertex v = gameData.vertices[i];
                if (v.site.y == 0) northVertices.Add(v);
                if (v.site.x == gameData.width) eastVertices.Add(v);
                if (v.site.y == gameData.height) southVertices.Add(v);
                if (v.site.x == 0) westVertices.Add(v);
            }
            // First I need to sort them, then I can loop though more easily
            // northVertices.Sort((a, b) => (a.site.x > b.site.x ? 1 : -1));
            northVertices.Sort((a, b) => a.site.x.CompareTo(b.site.x));
            eastVertices.Sort((a, b) => a.site.y.CompareTo(b.site.y));
            southVertices.Sort((a, b) => a.site.x.CompareTo(b.site.x));
            westVertices.Sort((a, b) => a.site.y.CompareTo(b.site.y));
            // Go through every list, create new edges and assign everything as neighbors
            fillMapBoudaries(gameData, northVertices);
            fillMapBoudaries(gameData, eastVertices);
            fillMapBoudaries(gameData, southVertices);
            fillMapBoudaries(gameData, westVertices);
        }
        static Cell getSharedCell(Vertex va, Vertex vb) {
            Cell sharedCell;
            if (va.cells.Count == 1) return va.cells[0]; // Corner case
            Cell cell1 = va.cells[0];
            Cell cell2 = va.cells[1];
            int c1index = vb.cells.IndexOf(cell1);
            if (c1index != -1) sharedCell = cell1;
            else sharedCell = cell2;
            // Return
            return sharedCell;
        }
        static void cornerCase(Vertex va, Vertex vb, GameData gameData) {
            // va is the one without cells
            // Choose which cell is closer (vb is map edge vertex meaning it has only 2 cells)
            Cell c1 = vb.cells[0];
            Cell c2 = vb.cells[1];
            // To know if cell is corner cell I need to check if it has vertices on both map boundaries
            bool c1horizontal = false;
            bool c1vertical = false;
            for (int i = 0; i < c1.vertices.Count; i++) {
                Vertex v = c1.vertices[i];
                if (!c1vertical) c1vertical = (v.site.x == 0 || v.site.x == gameData.width);
                if (!c1horizontal) c1horizontal = (v.site.y == 0 || v.site.y == gameData.height);
            }
            // Check who won
            if (c1horizontal && c1vertical) {
                c1.vertices.Add(va);
                va.cells.Add(c1);
                sortVertices(c1, c1.vertices);
            } else {
                c2.vertices.Add(va);
                va.cells.Add(c2);
                sortVertices(c2, c2.vertices);
            }
        }
        static void fillMapBoudaries(GameData gameData, List<Vertex> vList) {
            for (int i = 0; i < vList.Count; i++) {
                Vertex currentVertex = vList[i];
                Vertex nextVertex = null;
                if (i < vList.Count - 1) nextVertex = vList[i + 1];
                if (nextVertex == null) continue; // Last loop anyway
                // Add eachother as neighbours
                currentVertex.neighbors.Add(nextVertex);
                nextVertex.neighbors.Add(currentVertex);
                // Add cell to vertex and vice versa (corner case)
                if (currentVertex.cells.Count == 0) cornerCase(currentVertex, nextVertex, gameData);
                if (nextVertex.cells.Count == 0) cornerCase(nextVertex, currentVertex, gameData);
                // Create new Edge
                Edge e = new Edge();
                e.va = currentVertex;
                e.vb = nextVertex;
                // Add cell to edge and vice versa
                // First get cell shared between vertices
                Cell sharedCell = getSharedCell(currentVertex, nextVertex);
                sharedCell.edges.Add(e);
                // Which side ? (magic btw)
                int positionSide = Math.Sign((nextVertex.site.x - currentVertex.site.x) * (sharedCell.site.y - currentVertex.site.y) - (nextVertex.site.y - currentVertex.site.y) * (sharedCell.site.x - currentVertex.site.x));
                // Assign
                if (positionSide > 0) e.toRight = sharedCell;
                else e.toLeft = sharedCell;
                // Add newly created edge
                currentVertex.edges.Add(e);
                nextVertex.edges.Add(e);
                gameData.edges.Add(e);
            }
        }
        // Moves vertices to the mass center of delaunay triangle
        public static void relaxVertices(GameData gameData, double relaxPower) {
            double voronoiPower = 2 - relaxPower; // Used to multiply voronoi coordinates
            // Move vertices to more desirable location (center of mass)
            for (int i = 0; i < gameData.vertices.Count; i++) {
                Vertex vertex = gameData.vertices[i];
                int count = vertex.cells.Count;
                if (count == 1) { // Those should only be 4 corners of the map
                } else if (count == 2) { // Those should be only edges of the map
                    // Check which on which axis (map boudary) this vertex lies on
                    bool isVertical = (vertex.site.x == 0 || vertex.site.x == gameData.width);
                    if (isVertical) { // Is north or south
                        double newY = (vertex.cells[0].site.y + vertex.cells[1].site.y) / 2;
                        vertex.site.y = ((vertex.site.y * voronoiPower) + (newY * relaxPower)) / 2;
                    } else { // Is west or east
                        double newX = (vertex.cells[0].site.x + vertex.cells[1].site.x) / 2;
                        vertex.site.x = ((vertex.site.x * voronoiPower) + (newX * relaxPower)) / 2;
                    }

                } else if (count == 3) {
                    double newX = (vertex.cells[0].site.x + vertex.cells[1].site.x + vertex.cells[2].site.x) / 3;
                    double newY = (vertex.cells[0].site.y + vertex.cells[1].site.y + vertex.cells[2].site.y) / 3;
                    vertex.site.x = ((vertex.site.x * voronoiPower) + (newX * relaxPower)) / 2;
                    vertex.site.y = ((vertex.site.y * voronoiPower) + (newY * relaxPower)) / 2;
                }
            }
        }
        static void sortVertices(Cell cell, List<Vertex> vertices) {
            double cx = cell.site.x;
            double cy = cell.site.y;
            vertices.Sort((a, b) => (Misc.getAngle(cx, cy, a.site.x, a.site.y) > Misc.getAngle(cx, cy, b.site.x, b.site.y)) ? 1 : -1);
        }
        static void addVertexToCell(Cell cell, Vertex vertex) {
            bool isValid = true;
            // Check for duplicates
            for (int i = 0; i < cell.vertices.Count; i++) {
                Vertex v = cell.vertices[i];
                if (v.site.x == vertex.site.x && v.site.y == vertex.site.y) {
                    isValid = false;
                    break;
                }
            }
            if (isValid) {
                cell.vertices.Add(vertex);
                vertex.cells.Add(cell);
            }
        }
        // Generate new polygon by creating points around center and them appling some jitter
        public static Landmass createLandmass(int numberOfCorners, Point centerPos, double landmassWidth, double landmassHeight, double pointJitter, double angleJitter) {
            // Setup
            double angle = 0;
            double angleStep = (Math.PI * 2) / numberOfCorners;
            List<Point> randomPolyPoints = new List<Point>();
            // Check size and if not box, then asses which dimension is bigger
            bool isBox = (landmassWidth == landmassHeight);
            bool isWidthBigger = false;
            double biggerDimensionMultiplier = 0;
            if (!isBox) {
                isWidthBigger = (landmassWidth > landmassHeight);
                if (isWidthBigger) biggerDimensionMultiplier = 1 + (((landmassWidth / landmassHeight) - 1) / 2);
                else biggerDimensionMultiplier = 1 + (((landmassHeight / landmassWidth) - 1) / 2);
            }
            double power = (isBox) ? landmassWidth : (isWidthBigger ? landmassHeight : landmassWidth); // Smaller dimesion
            // Calculate Random Polygon Points
            for (int i = 0; i < numberOfCorners; i++) {
                double trueAngle = angle + (angleJitter * Misc.getRandomDouble(-angleStep / 2, angleStep / 2)); // Get current angle
                Point polyPoint = new Point(centerPos.x, centerPos.y); // Create point in the middle
                Point angleToAdd = new Point(Math.Cos(trueAngle - Math.PI), Math.Sin(trueAngle - Math.PI)); // Translate point to the desired location
                polyPoint.x += angleToAdd.x * power;
                polyPoint.y += angleToAdd.y * power;
                // TODO oval
                if (!isBox) {
                    double minusPI = angle - Math.PI;
                    double stregthPI = Math.Abs(minusPI) / Math.PI;
                    // Get range
                    //                 let fakeVec = p5.Vector.fromAngle(0);
                    Point fakePos = new Point(centerPos.x, centerPos.y);
                    //                fakeVec.setMag(m); // Move in m direction
                    //               fakeVec.add(fakePos);
                    if (isWidthBigger) {
                        //                   double range = fakeVec.x - startPos.x;
                        //                  double increase = (range * biggerDimensionMultiplier) - range;
                        //                  double additionalRange = increase * stregthPI;
                        //                  startPos.x += additionalRange;
                    } else {

                    }
                }
                // Add point jitter
                double pja = pointJitter * power / 2;
                double jitterX = Misc.getRandomDouble(-pja, pja);
                double jitterY = Misc.getRandomDouble(-pja, pja);
                polyPoint.x += jitterX;
                polyPoint.y += jitterY;
                // Increase angle by one step
                angle += angleStep;
                // Add to points array
                randomPolyPoints.Add(polyPoint);
            }
            // Calculate Random Polygon Edges
            List<Edge> randomPolyEdges = new List<Edge>();
            for (int i = 0; i < randomPolyPoints.Count; i++) {
                Point p = randomPolyPoints[i];
                Point p2 = null;
                if (i < randomPolyPoints.Count - 1) p2 = randomPolyPoints[i + 1];
                Vertex va = new Vertex(new Point(p.x, p.y));
                Vertex vb;
                if (p2 != null) vb = new Vertex(new Point(p2.x, p2.y));
                else vb = new Vertex(new Point(randomPolyPoints[0].x, randomPolyPoints[0].y));
                // Create edges
                Edge edge = new Edge();
                edge.va = va;
                edge.vb = vb;
                randomPolyEdges.Add(edge);
            }
            // Return
            Landmass randomPolygon = new Landmass(randomPolyPoints, randomPolyEdges);
            return randomPolygon;
        }
        // Assign type to the cell based on given data
        public static void assignTypeToCells(GameData gameData, List<Landmass> continents) {
            List<Cell> landCells = new List<Cell>();
            List<Cell> waterCells = new List<Cell>();
            // Using image
            if (gameData.imageLand != null) {
                Console.WriteLine("Using image to generate landmass!");
                for (int i = 0; i < gameData.cells.Count; i++) {
                    Point point = gameData.cells[i].site;
                    CellType type = CellType.OCEAN;
                    double fillX = point.x / gameData.width;
                    double fillY = point.y / gameData.height;
                    int pixelPositionX = (int) (fillX * gameData.imageLand.Width);
                    int pixelPositionY = (int) (fillY * gameData.imageLand.Height);
                    Color pixelData = gameData.imageLand.GetPixel(pixelPositionX, pixelPositionY);
                    if (pixelData.R == 0) {
                        type = CellType.LAND;
                    }
                    gameData.cells[i].type = type;
                    // Add to right collection
                    if (type == CellType.LAND) landCells.Add(gameData.cells[i]);
                    else waterCells.Add(gameData.cells[i]);
                }
            } else {
                // Normal generation
                for (int i = 0; i < gameData.cells.Count; i++) {
                    Point point = gameData.cells[i].site;
                    CellType type = CellType.OCEAN;
                    // Random Poly Islands
                    for (int j = 0; j < continents.Count; j++) {
                        Landmass rp = continents[j];
                        if (Misc.isPointInPoly(point, rp.edges)) {
                            type = CellType.LAND;
                            break; // We confirmed that this cell is on at least one land mass
                        }
                    }
                    // Set type
                    gameData.cells[i].type = type;
                    // Add to right collection
                    if (type == CellType.LAND) landCells.Add(gameData.cells[i]);
                    else waterCells.Add(gameData.cells[i]);
                }
            }
            // Transfer cells
            gameData.landCells = landCells;
            gameData.waterCells = waterCells;
        }
        // Creates provinces and transferms necessary data over
        public static void createProvinces(GameData gameData) {
            // Create provinces on land cells
            for (int i = 0; i < gameData.landCells.Count; i++) {
                Province newProvince = new Province(gameData.landCells[i]);
                gameData.provinces.Add(newProvince);
            }
            // Transfer neighbors from cells to provinces
            for (int i = 0; i < gameData.provinces.Count; i++) {
                Province p = gameData.provinces[i];
                for (int j = 0; j < p.cell.neighbors.Count; j++) {
                    Cell neighborCell = p.cell.neighbors[j];
                    if (neighborCell.type == CellType.LAND) p.neighbors.Add(neighborCell.province);
                }
            }
        }
        public static void createGeography(GameData gameData) {
            List<Vertex> coastVertex = new List<Vertex>();
            // Assign type
            for (int i = 0; i < gameData.vertices.Count; i++) {
                Vertex vertex = gameData.vertices[i];
                // Check cells to determine type
                int landCells = 0;
                int oceanCells = 0;
                // Check what vertex is surrounded with-
                for (int c = 0; c < vertex.cells.Count; c++) {
                    Cell cell = vertex.cells[c];
                    if (cell.type == CellType.LAND) landCells++;
                    else oceanCells++;
                }
                // -And assign type based on that
                if (landCells == vertex.cells.Count) {
                    vertex.type = VertexType.LAND; // All are land
                } else if (oceanCells == vertex.cells.Count) {
                    vertex.type = VertexType.OCEAN; // All are water
                } else {
                    vertex.type = VertexType.COAST; // Mixed
                    vertex.height = 0;
                    coastVertex.Add(vertex);
                }
            }
            // Assign height to Vertex, starting from the coast
            while (coastVertex.Count > 0) {
                int randIndex = Misc.getRandomInt(0, coastVertex.Count - 1);
                Vertex randVertex = coastVertex[randIndex];
                for (int n = 0; n < randVertex.neighbors.Count; n++) {
                    Vertex neighbor = randVertex.neighbors[n];
                    if (neighbor.type != VertexType.LAND) continue; // Not land, cannot have height
                    if (neighbor.height != -1) continue; // Already set, -1 is default
                    neighbor.height = (randVertex.height + 1);
                    coastVertex.Add(neighbor); // Check this new vertex's neighbors later
                }
                // Remove this vertex from active
                coastVertex.RemoveAt(randIndex);
            }
            // Create rivers using vertex height data
            createRivers(gameData);
        }
        public static void createRivers(GameData gameData) {
            for (int i = 0; i < gameData.vertices.Count; i++) {
                Vertex vertex = gameData.vertices[i];
                int rng = Misc.getRandomInt(1, 100);
                bool heightMinimum = (vertex.height > 5);
                bool rngResult = ((vertex.height / 2) > rng);
                if (heightMinimum && rngResult) {
                    // Check if starting vertex is lying on other river vertices
                    bool overlaps = false;
                    for (int j = 0; j < gameData.rivers.Count; j++) {
                        River river = gameData.rivers[j];
                        for (int x = 0; x < river.vertices.Count; x++) {
                            Vertex riverVertex = river.vertices[x];
                            if (vertex.site.x == riverVertex.site.x && riverVertex.site.y == vertex.site.y) overlaps = true;
                        }
                    }
                    if (overlaps) continue;
                    // Create new river
                    River newRiver = new River(vertex);
                    // Start going lower and lower until water or other river is hit
                    initRiver(newRiver, gameData);
                    // Add ready river
                    gameData.rivers.Add(newRiver);
                }
            }
        }
        static void initRiver(River river, GameData gameData) {
            Vertex currentVertex = river.vertices[0];
            bool hasReachedEnd = false;
            // Search for neighboring vertex with lower (or same) height
            while (!hasReachedEnd) {
                int lowestIndex = -1;
                for (int i = 0; i < currentVertex.neighbors.Count; i++) {
                    Vertex neighbor = currentVertex.neighbors[i];
                    if (currentVertex.height > neighbor.height && neighbor.type != VertexType.OCEAN) {
                        if (checkVertexWithOtherRivers(river, neighbor, gameData)) hasReachedEnd = true;
                        if (neighbor.type == VertexType.COAST) hasReachedEnd = true;
                        lowestIndex = i;
                        if (hasReachedEnd) break;
                    }
                }
                // Assign best vertex
                Vertex best = currentVertex.neighbors[lowestIndex];
                river.vertices.Add(best);
                // Tell edge that it's a river now
                for (int i = 0; i < best.edges.Count; i++) {
                    Edge edge = best.edges[i];
                    // Check which vertex is currentVertex (A or B)
                    if (edge.va == best && edge.vb == currentVertex || edge.vb == best && edge.va == currentVertex) {
                        edge.isRiver = true;
                        break;
                    }
                }
                // Update currentVertex
                currentVertex = best;
                // Error (???)
                if (lowestIndex == -1) {
                    Console.Write("error");
                    hasReachedEnd = true;
                }
            }
        }
        // Check if given vertex is lying on other river vertices
        static bool checkVertexWithOtherRivers(River currentRiver, Vertex vertex, GameData gameData) {
            bool overlaps = false;
            River hitStartRiver = null;
            // Go through every river
            for (int j = 0; j < gameData.rivers.Count; j++) {
                if (overlaps) break;
                River river = gameData.rivers[j];
                // Go through every vertex
                for (int x = 0; x < river.vertices.Count; x++) {
                    if (overlaps) break;
                    Vertex riverVertex = river.vertices[x];
                    if (vertex.site.x == riverVertex.site.x && riverVertex.site.y == vertex.site.y) overlaps = true;
                    if (overlaps && x == 0) hitStartRiver = river;
                }
            }
            // If this river hit a start of another river, then I need to merge them together
            if (hitStartRiver != null) {
                // First I remove this river from game object (it will get GC)
                for (int i = 0; i < gameData.rivers.Count; i++) {
                    Vertex sv = gameData.rivers[i].vertices[0];
                    if (sv.site.x == currentRiver.vertices[0].site.x && sv.site.y == currentRiver.vertices[0].site.y) {
                        gameData.rivers.RemoveAt(i);
                    }
                }
                // Now I add all points from this river to start of hit river
                for (int i = currentRiver.vertices.Count - 1; i >= 0; i--) {
                    Vertex v = currentRiver.vertices[i];
                    hitStartRiver.vertices.Insert(0, v);
                }

            }
            return overlaps;
        }
        public static void assignGeography(GameData gameData) {
            double halfHeight = gameData.height / 2;
            for (int i = 0; i < gameData.provinces.Count; i++) {
                Province province = gameData.provinces[i];
                // ELEVATION  
                Cell cell = province.cell;
                double averageVertexHeight = 0;
                // Get average height surrouding this province
                for (int v = 0; v < cell.vertices.Count; v++) {
                    Vertex vertex = cell.vertices[v];
                    averageVertexHeight += vertex.height;
                }
                averageVertexHeight /= cell.vertices.Count;
                // Assign right level of elevation to the province
                ElevationLevel level = ElevationLevel.IMPASSABLE;
                if (averageVertexHeight < 5) level = ElevationLevel.PLAINS;
                else if (averageVertexHeight < 10) level = ElevationLevel.HILLS;
                else if (averageVertexHeight < 15) level = ElevationLevel.HIGHLANDS;
                else if (averageVertexHeight < 20) level = ElevationLevel.MOUNTAINS;
                province.elevation = level;
                // TEMPERATURE
                // Depends on where the  province is in vertical place
                double py = province.cell.site.y;
                double amount;
                if (py > halfHeight) amount = 1 - ((py / halfHeight) - 1);
                else amount = (py / halfHeight);
                province.temperature = amount;
                // HUMIDITY
                double humidityVal = 0;
                // Water body 80% Search for nearby water bodies
                for (int j = 0; j < province.cell.neighbors.Count; j++) {
                    Cell neighbor = province.cell.neighbors[j];
                    if (neighbor.type == CellType.OCEAN) {
                        humidityVal += 0.8;
                        break;
                    }
                }
                // River 20% // Search for nearby rivers
                int riverNumber = 0;
                for (int j = 0; j < province.cell.edges.Count; j++) {
                    Edge edge = province.cell.edges[j];
                    if (edge.isRiver) {
                        riverNumber++;
                        break;
                    }
                }
                humidityVal += 0.2 * (riverNumber / province.cell.edges.Count);
                // Temperature inflence
                humidityVal = (humidityVal / 2) + (humidityVal * province.temperature) / 2;
                if (humidityVal > 1) humidityVal = 1;
                province.humidity = humidityVal;
            }
        }
        public static void spawnNations(int numberOfNations, int minDistance, int provinceLimit, GameData gameData) {
            int nationsToSpawn = numberOfNations;
            int nationsToSpawnCounter = nationsToSpawn;
            // Create wasteland for storing impassable terrain
            Nation nationwasteland = new Nation();
            nationwasteland.color = "Black";
            gameData.wasteland = nationwasteland;
            // Populate wasteland
            for (int i = 0; i < gameData.provinces.Count; i++) {
                Province province = gameData.provinces[i];
                if (wastelandTest(province)) gameData.wasteland.addProvince(province);
            }
            // Spawn nations in random province
            while (nationsToSpawnCounter != 0) {
                int rn = Misc.getRandomInt(0, gameData.provinces.Count - 1);
                Province province = gameData.provinces[rn];
                // Checks
                if (province.owner != null) continue; // This province already has an owner
                // Check if it's not too close to other nation capital
                bool isTooClose = false;
                for (int i = 0; i < gameData.nations.Count; i++) {
                    Nation nat = gameData.nations[i];
                    double distance = Misc.distanceBetweenPoints(province.cell.site.x, province.cell.site.y, nat.capital.cell.site.x, nat.capital.cell.site.y);
                    if (distance < minDistance) {
                        isTooClose = true;
                        break;
                    }
                }
                if (isTooClose) continue;
                // Prefer possible elevation
                if (province.elevation == ElevationLevel.MOUNTAINS) continue;
                // Not polar, not desert
                if (province.temperature < 0.1 || province.temperature > 0.9) continue;
                // Create nation on this province
                Nation nation = new Nation();
                nation.color = Misc.getRandomHexColor();
                nation.borderColor = Misc.adjustColor(nation.color, -0.5);
                nation.capital = province;
                nation.addProvince(province);
                gameData.addNation(nation);
                nationsToSpawnCounter--;
            }
            // Populate nations with provinces
            List<List<Province>> activeList = new List<List<Province>>();
            for (int i = 0; i < nationsToSpawn; i++) {
                List<Province> provinceList = new List<Province>();
                provinceList.Add(gameData.nations[i].provinces[0]);
                activeList.Add(provinceList);
            }
            // Each loop assign one province to the nation
            while (activeList.Count > 0) {
                int isEmpty = -1;
                for (int i = 0; i < activeList.Count; i++) {
                    // Check is province search should continue
                    bool outOfPlaces = (activeList[i].Count == 0);
                    bool isAtProvinceLimit = (outOfPlaces) ? false : (activeList[i][0].owner.provinces.Count >= provinceLimit);
                    if (outOfPlaces || isAtProvinceLimit) {
                        isEmpty = i; // Set which one of lists is empty and then remove it
                        break;
                    }
                    // Add next province to the nation
                    findNextProvince(activeList[i]);
                }
                if (isEmpty != -1) activeList.RemoveAt(isEmpty);
            }
        }
        static bool wastelandTest(Province province) {
            // If province surrouded by mountains?
            bool isSurrouded = true;
            int heightLimit = 200; // TODO
            // If one edge is open, then it's not
            for (int i = 0; i < province.cell.edges.Count; i++) {
                Edge edge = province.cell.edges[i];
                if (edge.va.height < heightLimit && edge.vb.height < heightLimit) {
                    isSurrouded = false;
                    break;
                }
            }
            return isSurrouded;
        }
        static void findNextProvince(List<Province> active) {
            // Choose random active province
            int randProvinceIndex = closestActiveToCapital(active);
            Province randProvince = active[randProvinceIndex];
            Nation randProvinceNation = randProvince.owner;
            bool foundNextOne = false;
            for (int j = 0; j < randProvince.neighbors.Count; j++) {
                Province neighbor = randProvince.neighbors[j];
                if (neighbor.owner != null) continue; // Already owned
                randProvinceNation.addProvince(neighbor);
                active.Add(neighbor);
                foundNextOne = true;
                break;
            }
            // Check if new neighbor was found
            if (!foundNextOne) active.RemoveAt(randProvinceIndex);
        }
        static int closestActiveToCapital(List<Province> active) {
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
