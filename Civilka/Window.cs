using Civilka.classes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Civilka.classes.Edge;
using Point = Civilka.classes.Point;

namespace Civilka {
    internal class Window {

        public class MyForm : Form {
            readonly GameData gameData;
            public MyForm(GameData gameData) {
                this.gameData = gameData;
                this.DoubleBuffered = true;
            }

            protected override void OnMouseMove(MouseEventArgs e) {
                if (e.X < 0 || e.Y < 0 || e.X > gameData.width || e.Y > gameData.height) return;
                this.gameData.mouseX = e.X;
                this.gameData.mouseY = e.Y;
                Cell targetCell = gameData.getCellFromMouse();
                if (gameData.selectedCell != null) {
                    if (gameData.selectedCell.type == Enums.CellType.OCEAN) gameData.path = Pathfinding.aStarPathfinding(gameData.waterCells, gameData.selectedCell, targetCell);
                    if (gameData.selectedCell.type == Enums.CellType.LAND) gameData.path = Pathfinding.aStarPathfinding(gameData.landCells, gameData.selectedCell, targetCell);
                }
                    
                base.OnMouseMove(e);
                Refresh();
            }

            protected override void OnMouseDown(MouseEventArgs e) {
                // Get target cell and assign it to the 
                Cell targetCell = gameData.getCellFromMouse();
                if (targetCell == null) return;
                // Unselect cell
                if (gameData.selectedCell != null && targetCell.id == gameData.selectedCell.id) {
                    gameData.selectedCell = null;
                    return;
                } 
                gameData.selectedCell = targetCell;
                // The rest
                base.OnMouseMove(e);
                Refresh();
            }

            protected override void OnPaint(PaintEventArgs e) {
                base.OnPaint(e);

                Graphics g = e.Graphics;
                Pen selPen = new Pen(Color.Red, 1);
                SolidBrush myBrush = new SolidBrush(Color.Blue);
                // Config
                bool landmassDebug = true;
                bool drawGrid = false;
                bool drawSimpleCells = true;
                bool debugDetailedCells = false;
                bool drawCellConnections = false;


                // Fill cells
                selPen.Color = Color.White;
                for (int i = 0; i < this.gameData.cells.Count; i++) {
                    Cell cell = this.gameData.cells[i];
                    if (cell.type == Enums.CellType.LAND) myBrush.Color = Color.ForestGreen;
                    else myBrush.Color = Color.Blue;
                    PointF[] fPoints = new PointF[cell.vertices.Count];
                    for (int j = 0; j < cell.vertices.Count; j++) {
                        fPoints[j] = new PointF((float)cell.vertices[j].site.x, (float)cell.vertices[j].site.y);
                    }
                    g.FillPolygon(myBrush, fPoints);
                    g.DrawRectangle(selPen, (float)cell.site.x, (float)cell.site.y, 1, 1);
                }

                // Fill nations
                for (int i = 0; i < this.gameData.nations.Count; i++) {
                    Nation nation = this.gameData.nations[i];
                    myBrush.Color = ColorTranslator.FromHtml(nation.color);
                    // Draw every cell
                    for (int c = 0; c < nation.provinces.Count; c++) {
                        List<Vertex> vers = nation.provinces[c].cell.vertices;
                        PointF[] fPoints = new PointF[vers.Count];
                        for (int j = 0; j < vers.Count; j++) {
                            Vertex v = vers[j];
                            fPoints[j] = new PointF((float)v.site.x, (float)v.site.y);
                        }
                        g.FillPolygon(myBrush, fPoints);
                    }
                    // Capital dot
                    selPen.Color = Color.Red;
                    g.DrawRectangle(selPen, (float)nation.capital.cell.site.x, (float)nation.capital.cell.site.y, 1, 1);
                }


                if (drawSimpleCells) {
                    // Draw cell edges
                    selPen.Color = Color.Gray;
                    for (int i = 0; i < this.gameData.cells.Count; i++) {
                        Cell cell = this.gameData.cells[i];
                        for (int j = 0; j < cell.vertices.Count; j++) {
                            Point p = cell.vertices[j].site;
                            Point p2 = null;
                            if (j < cell.vertices.Count - 1) p2 = cell.vertices[j + 1].site;
                            if (p2 == null) break;
                            g.DrawLine(selPen, (float)p.x, (float)p.y, (float)p2.x, (float)p2.y);
                        }
                    }
                } else {
                    // Draw cell edges with edges
                    for (int i = 0; i < gameData.edges.Count; i++) {
                        Edge edge = gameData.edges[i];
                        if (edge.noisyEdges == null) continue;
                        for (int j = 0; j < edge.noisyEdges.Count; j++) {
                            Edge detailedEdge = edge.noisyEdges[j];
                            g.DrawLine(selPen, (float)detailedEdge.va.site.x, (float)detailedEdge.va.site.y, (float)detailedEdge.vb.site.x, (float)detailedEdge.vb.site.y);
                        }
                    }
                }

                // Render Quads DEBUG, shows quads in first cell
                if (debugDetailedCells) {
                    selPen.Color = Color.Red;
                    for (int i = 0; i < gameData.edges[0].allQuadsDebug.Count; i++) {
                        Quadrilateral quad = gameData.edges[0].allQuadsDebug[i];
                        g.DrawLine(selPen, (float)quad.points[0].x, (float)quad.points[0].y, (float)quad.points[1].x, (float)quad.points[1].y);
                        g.DrawLine(selPen, (float)quad.points[1].x, (float)quad.points[1].y, (float)quad.points[2].x, (float)quad.points[2].y);
                        g.DrawLine(selPen, (float)quad.points[2].x, (float)quad.points[2].y, (float)quad.points[3].x, (float)quad.points[3].y);
                        g.DrawLine(selPen, (float)quad.points[3].x, (float)quad.points[3].y, (float)quad.points[0].x, (float)quad.points[0].y);
                    }
                }

                // Draw nations borders
                selPen.Width = 3;
                for (int i = 0; i < this.gameData.nations.Count; i++) {
                    Nation nation = this.gameData.nations[i];
                    selPen.Color = ColorTranslator.FromHtml(nation.borderColor);
                    for (int j = 0; j < nation.borders.Count; j++) {
                        Edge border = nation.borders[j];
                        g.DrawLine(selPen, (float)border.va.site.x, (float)border.va.site.y, (float)border.vb.site.x, (float)border.vb.site.y);
                    }

                }

                // Landmass
                selPen.Color = Color.Red;
                for (int i = 0; i < this.gameData.landmasses.Count; i++) {
                    Landmass land = this.gameData.landmasses[i];
                    PointF[] fPoints = new PointF[land.points.Count];
                    for (int j = 0; j < land.points.Count; j++) {
                        fPoints[j] = new PointF((float)land.points[j].x, (float)land.points[j].y);
                    }
                    if (landmassDebug) g.DrawPolygon(selPen, fPoints);
                }
                // River
                selPen.Color = Color.Blue;
                selPen.Width = 2;
                for (int i = 0; i < this.gameData.rivers.Count; i++) {
                    River river = this.gameData.rivers[i];
                    for (int j = 0; j < river.vertices.Count; j++) {
                        Point p = river.vertices[j].site;
                        Point p2 = null;
                        if (j < river.vertices.Count - 1) p2 = river.vertices[j + 1].site;
                        if (p2 == null) break;
                        g.DrawLine(selPen, (float)p.x, (float)p.y, (float)p2.x, (float)p2.y);
                    }
                }

                // Connections
                selPen.Color = Color.Red;
                selPen.Width = 1;
                for (int i = 0; i < this.gameData.cells.Count; i++) {
                    Cell cell = this.gameData.cells[i];
                    for (int j = 0; j < cell.neighbors.Count; j++) {
                        Point n = cell.neighbors[j].site;
                        if (drawCellConnections) g.DrawLine(selPen, (float)cell.site.x, (float)cell.site.y, (float)n.x, (float)n.y);
                    }
                }

                // Selected cell
                if (gameData.selectedCell != null) {
                    selPen.Width = 2;
                    selPen.Color = Color.Red;
                    for (int j = 0; j < gameData.selectedCell.vertices.Count; j++) {
                        Point p = gameData.selectedCell.vertices[j].site;
                        Point p2 = null;
                        if (j < gameData.selectedCell.vertices.Count - 1) p2 = gameData.selectedCell.vertices[j + 1].site;
                        if (p2 == null) break;
                        g.DrawLine(selPen, (float)p.x, (float)p.y, (float)p2.x, (float)p2.y);
                    }
                    List<Vertex> vertices = gameData.selectedCell.vertices;
                    g.DrawLine(selPen, (float)vertices[0].site.x, (float)vertices[0].site.y, (float)vertices[vertices.Count - 1].site.x, (float)vertices[vertices.Count - 1].site.y);
                }
                
                // Pathfinding
                selPen.Color = Color.Pink;
                selPen.Width = 2;
                if (gameData.path != null) {
                    for (int i = 0; i < this.gameData.path.Count; i++) {
                        Cell cell = this.gameData.path[i];
                        Cell nextCell = null;
                        if (i < gameData.path.Count - 1) nextCell = gameData.path[i + 1];
                        if (nextCell == null) continue;
                        g.DrawLine(selPen, (float)cell.site.x, (float)cell.site.y, (float)nextCell.site.x, (float)nextCell.site.y);
                    }
                }
                g.DrawRectangle(selPen, (float)gameData.mouseX, (float)gameData.mouseY, 1, 1);
                // Draw grid
                if (drawGrid) {
                    selPen.Color = Color.Red;
                    selPen.Width = 1;
                    int tileSize = this.gameData.w;
                    for (int i = 0; i < this.gameData.cols; i++) {
                        g.DrawLine(selPen, (float)i * tileSize, (float)0, (float)i * tileSize, (float)this.gameData.height);
                    }
                    for (int y = 0; y < this.gameData.rows; y++) {
                        g.DrawLine(selPen, (float)0, (float)y * tileSize, (float)this.gameData.width, (float)y * tileSize);
                    }
                }

            }

        }
    }
}
