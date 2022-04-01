using System;
using Rhino.Geometry;
using Rhino;
using Rhino.DocObjects;
using Rhino.Input;
using Rhino.Input.Custom;
using System.Collections.Generic;

namespace EdgeControl
{
    public class VerticalCalculator
    {
        //Extracts edges from the mesh
        private List<Line> ExtractVerticalEdgesFromMesh(Mesh mesh, double tolerance, ref double meshLength)
        {
            int edgeCount = mesh.TopologyEdges.Count;
            double totalLength = 0;
            List<Line> verticalLines = new List<Line>();
            for (int i = 0; i < edgeCount; i++)
            {
                var line = mesh.TopologyEdges.EdgeLine(i);
                if (AssessLineVerticality(line, tolerance))
                {
                    verticalLines.Add(line);
                    totalLength += line.Length;
                }
            }

            meshLength = totalLength;

            return verticalLines;
        }

        //Calculates how vertical an edge is
        private bool AssessLineVerticality(Line line, double tolerance)
        {
            double diffX = Math.Abs(line.FromX - line.ToX);
            double diffY = Math.Abs(line.FromY - line.ToY);
            double diffZ = Math.Abs(line.FromZ - line.ToZ);
            double baseH = 0;

            if (diffZ == 0) return false; //not a vertical line if there is no difference in z

            else if (diffX == 0 && diffY == 0) return true; //vertical line if there is a difference in z but not in the others

            else if (diffX != 0 && diffY != 0)
            {
                var beta = Math.Atan2(diffY, diffX); //triangle with diffy and diffx as catheti, beta is the angle between diffX and hypothenuse
                baseH = diffY / Math.Sin(beta); //baseH is the hypothenuse of the triangle
            }
            else
            {
                baseH = (diffX == 0) ? diffY : diffX;
            }

            var gamma = Math.Atan2(baseH, diffZ); //triangle with diffZ and baseH as catheti, gamma is the angle between diffZ and the hypothenuse - vertical angle of line

            var verticalAngleDegrees = (180 / Math.PI) * gamma;

            bool verticality = Math.Abs(verticalAngleDegrees) < tolerance ? true : false;

            return verticality;
        }

        //Returns all the vertical edges and prints their total length
        public List<Line> CalculateVerticalMeshLineLength(List<Mesh> meshList, double tolerance)
        {
            int verticalLineCount = 0;
            double meshLength = 0;
            double totalLength = 0;
            List<Line> AllVerticalLines = new List<Line>();

            foreach (var mesh in meshList)
            {
                var verticalLines = ExtractVerticalEdgesFromMesh(mesh, tolerance, ref meshLength);
                verticalLineCount += verticalLines.Count;
                totalLength += meshLength;
                verticalLines.ForEach(item => AllVerticalLines.Add(item));
            }

            RhinoApp.WriteLine($"The amount of vertical lines in the selected meshes within a tolerance of {tolerance}° is {AllVerticalLines.Count}");
            RhinoApp.WriteLine($"The total length of the combined vertical lines in the selected meshes is {totalLength}");
            return AllVerticalLines;
        }
    }
}
