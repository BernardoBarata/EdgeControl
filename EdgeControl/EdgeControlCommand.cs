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
                if (eraseAnswer.ToLower() == "y") objManipulator.EraseAndRedrawLines(go, doc, allVerticalLines);
                else if (eraseAnswer.ToLower() != "n") RhinoApp.WriteLine($"Invalid answer, please type either y (yes) or n (no)");

            }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            GetObject go = objManipulator.SetupObjectGetter();

            bool bHavePreselectedObjects = false;

            if (!objManipulator.ObjectRetrieval(go, ref bHavePreselectedObjects)) return Result.Cancel;

            var objList = go.Objects();
            List<Mesh> meshList = new List<Mesh>();
            foreach (var obj in objList)
            {
                meshList.Add(obj.Mesh());
            }

            var allVerticalLines = vertCalculator.CalculateVerticalMeshLineLength(meshList, 5);

            PromptErase(go, doc, allVerticalLines);

            objManipulator.CleanupSelection(go, bHavePreselectedObjects, doc);

            return Result.Success;
        }


    }
}
