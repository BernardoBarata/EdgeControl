using System;
using System.Collections.Generic;
using Rhino.Geometry;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Input;
using Rhino.Input.Custom;

namespace EdgeControl
{
    public class EdgeControlCommand : Command
    {
        ObjectManipulator objManipulator;
        VerticalCalculator vertCalculator;
        public EdgeControlCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
            objManipulator = new ObjectManipulator();
            vertCalculator = new VerticalCalculator();
        }

        ///<summary>The only instance of this command.</summary>
        public static EdgeControlCommand Instance
        {
            get; private set;
        }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName
        {
            get { return "EdgeControlCommand"; }
        }

        private void PromptErase(GetObject go, RhinoDoc doc, List<Line> allVerticalLines)
        {
            string eraseAnswer = "";
            while (eraseAnswer.ToLower() != "n")
            {
                RhinoGet.GetString("Would you like to erase selected meshes and highlight the lines? (Y/N)", true, ref eraseAnswer);
                if (eraseAnswer.ToLower() == "y")
                {
                    objManipulator.EraseAndRedrawLines(go, doc, allVerticalLines);
                    break;
                }
                else if (eraseAnswer.ToLower() != "n") RhinoApp.WriteLine($"Invalid answer, please type either y (yes) or n (no)");

            }
        }

        private double PromptTolerance()
        {
            double toleranceAnswer = -1;
            while (toleranceAnswer < 0 || toleranceAnswer > 90)
            {
                RhinoGet.GetNumber("How much vertical tolerance do you want in degrees (°) ? - Min: 0°, Max: 90°", true, ref toleranceAnswer);
                if (toleranceAnswer < 0 || toleranceAnswer > 90) RhinoApp.WriteLine($"Invalid answer, please type in a tolerance within the limits");
            }
            return toleranceAnswer;
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            GetObject go = objManipulator.SetupObjectGetter();

            bool bHavePreselectedObjects = false;

            if (!objManipulator.ObjectRetrieval(go, ref bHavePreselectedObjects)) return Result.Cancel;

            double tolerance = PromptTolerance();

            var objList = go.Objects();
            List<Mesh> meshList = new List<Mesh>();
            foreach (var obj in objList)
            {
                meshList.Add(obj.Mesh());
            }

            var allVerticalLines = vertCalculator.CalculateVerticalMeshLineLength(meshList, tolerance);

            PromptErase(go, doc, allVerticalLines);

            objManipulator.CleanupSelection(go, bHavePreselectedObjects, doc);

            return Result.Success;
        }


    }
}
