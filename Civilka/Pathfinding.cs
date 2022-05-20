using Civilka.classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civilka {
    class Pathfinding {

        class Node {

            public int id = -1;
            public Cell cell = null;
            public List<Node> neighbors = new List<Node>(); // Data transfered from cells
            public double distanceToStart = 0;
            public double distanceToTarget; // Estimated absolute distance to the target
            public Node cameFrom = null; // Backward connection
            public Node(Cell cell) {
                this.id = cell.id;
                this.cell = cell;
            }
            public double getCost() {
                // Counting only distance from start would create Dijkstra path, but it has big performance hit.
                // Counting only distance to target is fastest, but paths created are often sub-optimal.
                return this.distanceToStart + this.distanceToTarget;
            }

        }

        public static List<Cell> aStarPathfinding(List<Cell> validCells, Cell startCell, Cell targetCell, List<Func<Province>> options = null) {
            if (validCells == null || startCell == null || targetCell == null) return null;
            // Setup
            List<Node> allNodes = new List<Node>();
            List<Node> activeNodes = new List<Node>(); // Nodes to be checked
            List<Node> processedNodes = new List<Node>(); // Nodes which were active once
            // Create nodes out of valid cells
            for (int i = 0; i < validCells.Count; i++) {
                Cell cell = validCells[i];
                Node newNode = new Node(cell);
                allNodes.Add(newNode);
            }
            // Find start node and assign it
            Node startNode = getNodeFromID(allNodes, startCell.id);
            if (startNode != null) activeNodes.Add(startNode);
            else return null;
            // Check if target node is in allNodes
            Node targetNode = getNodeFromID(allNodes, targetCell.id);
            if (targetNode == null) return null;
            // Start search for best path
            while (activeNodes.Count != 0) {
                // Assign cell which is has the lowest cost
                Node currentNode = activeNodes[0];
                for (int i = 0; i < activeNodes.Count; i++) {
                    Node candidateNode = activeNodes[i];
                    if (candidateNode.getCost() < currentNode.getCost()) currentNode = candidateNode;
                }
                // Add to proccessed
                processedNodes.Add(currentNode);
                // If target cell was assigned - return path
                if (currentNode.id == targetCell.id) return getAStarPath(currentNode);
                // Remove current node from active ones
                int currentNodeIndex = currentNode.cell.id;
                for (int i = activeNodes.Count - 1; i >= 0; i--) {
                    Node n = activeNodes[i];
                    if (n.cell.id == currentNodeIndex) activeNodes.RemoveAt(i);
                }
                // Go though all neighbors of this node
                for (int i = 0; i < currentNode.cell.neighbors.Count; i++) {
                    // Get neighbor
                    int neighborID = currentNode.cell.neighbors[i].id;
                    Node neighbor = getNodeFromID(allNodes, neighborID);
                    if (neighbor == null) continue; // Cell not in nodes
                    if (processedNodes.Contains(neighbor)) continue; // No need to process it again
                    // Start calucations
                    bool isInActive = activeNodes.Contains(neighbor);
                    double startScore = currentNode.distanceToStart + Misc.distanceBetweenPoints(currentNode.cell.site.x, currentNode.cell.site.y, neighbor.cell.site.x, neighbor.cell.site.y);
                    if (!isInActive || startScore < neighbor.distanceToStart) {
                        neighbor.distanceToStart = startScore;
                        neighbor.distanceToTarget = Misc.distanceBetweenPoints(neighbor.cell.site.x, neighbor.cell.site.y, targetCell.site.x, targetCell.site.y);
                        neighbor.cameFrom = currentNode;
                        if (!isInActive) activeNodes.Add(neighbor);
                    }
                }
            }
            // No path found - return null
            return null;
        }

        static Node getNodeFromID(List<Node> allNodes, int id) {
            for (int i = 0; i < allNodes.Count; i++) {
                Node node = allNodes[i];
                if (node.id == id) return node;
            }
            return null;
        }
        // Returns path to start from given node
        private static List<Cell> getAStarPath(Node node) {
            List<Cell> path = new List<Cell>(); // Path of cells
            Node currentNode = node;
            // Loop until first node
            while (currentNode.cameFrom != null) {
                path.Add(currentNode.cell); // Push cell
                currentNode = currentNode.cameFrom; // Assign next node
                if (currentNode.cameFrom == null) path.Add(currentNode.cell); // Push last cell, before loop breaks
            }
            return path;
        }

    }
}
