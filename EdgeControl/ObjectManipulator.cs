
using Rhino.Geometry;
using Rhino;
using Rhino.DocObjects;
using Rhino.Input;
using Rhino.Input.Custom;
using System.Collections.Generic;


namespace EdgeControl
{
    public class ObjectManipulator
    {
        //Setup of the object getter
        public GetObject SetupObjectGetter()
        {
            const ObjectType geometryFilter = ObjectType.Mesh;

            GetObject go = new GetObject();
            go.SetCommandPrompt("Select any number of meshes");
            go.GeometryFilter = geometryFilter;

            go.GroupSelect = true;
            go.SubObjectSelect = false;
            go.EnableClearObjectsOnEntry(false);
            go.EnableUnselectObjectsOnExit(false);
            go.DeselectAllBeforePostSelect = false;

            return go;
        }

        //Get selected objects by the user
        public bool ObjectRetrieval(GetObject go, ref bool bHavePreselectedObjects)
        {
            for (; ; )
            {
                GetResult res = go.GetMultiple(1, 0);

                if (res == GetResult.Option)
                {
                    go.EnablePreSelect(false, true);
                    continue;
                }

                else if (res != GetResult.Object)
                    return false;

                if (go.ObjectsWerePreselected)
                {
                    bHavePreselectedObjects = true;
                    go.EnablePreSelect(false, true);
                    continue;
                }

                break;
            }
            return true;
        }

        //Cleans up the selected objects after the command has been run
        public void CleanupSelection(GetObject go, bool bHavePreselectedObjects, RhinoDoc doc)
        {
            if (bHavePreselectedObjects)
            {
                for (int i = 0; i < go.ObjectCount; i++)
                {
                    RhinoObject rhinoObject = go.Object(i).Object();
                    if (null != rhinoObject)
                        rhinoObject.Select(false);
                }
                doc.Views.Redraw();
            }
        }

        //Erase the selected meshes and redraw the vertical lines
        public void EraseAndRedrawLines(GetObject go, RhinoDoc doc, List<Line> allVerticalLines)
        {
            for (int i = 0; i < go.ObjectCount; i++)
            {
                RhinoObject rhinoObject = go.Object(i).Object();
                if (null != rhinoObject)
                    doc.Objects.Delete(rhinoObject);
            }
            foreach (var line in allVerticalLines)
            {
                doc.Objects.AddLine(line);
            }
            doc.Views.Redraw();
        }
    }
}
