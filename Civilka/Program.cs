﻿using Civilka.classes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using VoronoiLib.Structures;
using static Civilka.classes.Edge;
using static Civilka.Window;
using Point = Civilka.classes.Point;

namespace Civilka {
    internal class Program {
       
        static void Main(string[] args) {

            int width = 1400;
            int height = 800;
            GameData gameData = new GameData();
            gameData.width = width;
            gameData.height = height;
            gameData.init();
            Console.WriteLine("Seed: " + Misc.seedUsed);

            // TODO - ask user for image location
            // TODO - relative path
            //string fileSpec = @"ABSOLUTE-PATH-TO-IMAGE";
            //gameData.imageLand = new Bitmap(fileSpec, true);

            // Generate Points
            Console.WriteLine("---Stopwatch START---");
            Misc.stopWatch.Start();
            int minDistance = 20;
            int maxTries = 20;
            int boundaryOffset = (int)Math.Floor((float)minDistance / 2);
            List<Point> points = WorldGeneration.PoissonDiscDistribution(width, height, minDistance, maxTries, boundaryOffset);
            gameData.points = points;
            Console.WriteLine("PDD: " + Misc.stopWatch.ElapsedMilliseconds + "ms");
            Misc.stopWatch.Restart();
            // Generate Voronoi
            List<FortuneSite> fPoints = new List<FortuneSite>();
            for (int i = 0; i < points.Count; i++) {
                fPoints.Add(new FortuneSite((float)points[i].x, (float)points[i].y));
            }
            LinkedList<VEdge> edges = VoronoiLib.FortunesAlgorithm.Run(fPoints, 0, 0, width, height);
            Console.WriteLine("Voronoi: " + Misc.stopWatch.ElapsedMilliseconds + "ms");
            Misc.stopWatch.Restart();
            // Transfer Data
            WorldGeneration.TransferData(gameData, edges);
            Console.WriteLine("Transfer: " + Misc.stopWatch.ElapsedMilliseconds + "ms");
            Misc.stopWatch.Reset();
            // Optional relaxation
            WorldGeneration.relaxVertices(gameData, 0.5);
            // Edge Noise
            for (int i = 0; i < gameData.edges.Count; i++) {
                Edge edge = gameData.edges[i];
                edge.createNoisyBorders(2, 0.4);
            }
            // Create land masses
            Landmass a = WorldGeneration.createLandmass(12, new Point(width * 0.3f, height/2), 300f, 300f, 0.5, 0.0);
            gameData.landmasses.Add(a);
            Landmass b = WorldGeneration.createLandmass(12, new Point(width * 0.7f, height / 2), 300f, 300f, 0.5, 0.0);
            gameData.landmasses.Add(b);
            // Give type to Cells (water, land)
            WorldGeneration.assignTypeToCells(gameData, gameData.landmasses);
            // Create provinces from land cells
            WorldGeneration.createProvinces(gameData);
            // Creates height and rivers
            WorldGeneration.createGeography(gameData);
            // Assign geography to the provinces
            WorldGeneration.assignGeography(gameData);
            // Spawn nations in random provinces
            WorldGeneration.spawnNations(10, minDistance*2, 10, gameData);
            Misc.stopWatch.Stop();
            Console.WriteLine("---Stopwatch END---");

            // Post generation report
            Console.WriteLine("Points: " + gameData.points.Count);
            Console.WriteLine("Vertices: " + gameData.vertices.Count);
            Console.WriteLine("Edges: " + gameData.edges.Count);
            Console.WriteLine("Cells: " + gameData.cells.Count);
            // Above voronoi, below game data
            Console.WriteLine("Provices: " + gameData.provinces.Count);
            Console.WriteLine("Rivers: " + gameData.rivers.Count);
            // Render
            MyForm window = new MyForm(gameData);
            window.Text = "Civilka";
            window.ClientSize = new Size(width, height);
            window.FormBorderStyle = FormBorderStyle.FixedSingle;
            window.MaximizeBox = false;
            Application.Run(window);
        }
    }
}
