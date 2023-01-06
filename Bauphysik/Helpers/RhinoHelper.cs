using Rhino.Input.Custom;
using Rhino;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.DocObjects;
using Rhino.Geometry;
using LayerManager;
using Bauphysik.Data;

namespace Bauphysik.Helpers
{
    public static class RhinoHelpers
    {
        #region Message

        public static bool CheckIsNull(object value, string attributeName, bool returnBool)
        {
            if (value == null)
            {
                MsgValueMissing(attributeName, returnBool);
                return true;
            }

            return false;
        }

        public static void MsgValueMissing(string valueName, bool cancelBool)
        {
            RhinoApp.WriteLine();
            RhinoApp.Write(Names.shift + Names.warning + "Wert '" + valueName + "' fehlt");
            if (cancelBool) RhinoApp.Write(Names.cancelCalc);
        }

        public static bool CheckIsNull(object value, string attributeName, string id, bool returnBool)
        {
            if (value == null)
            {
                MsgValueMissing(attributeName, returnBool, id);
                return true;
            }

            return false;
        }

        public static void MsgValueMissing(string valueName, bool cancelBool, string id)
        {
            RhinoApp.Write(Names.shift + Names.warning + "Wert '" + valueName + "' fehlt (" + id + ")");
            if (cancelBool)
                RhinoApp.WriteLine(Names.cancelCalc);
            else
                RhinoApp.WriteLine();
        }

        public static void MsgValueSmallerThan(string valueName, string value, bool equalBool, bool cancelBool)
        {
            RhinoApp.WriteLine();
            RhinoApp.Write(Names.shift + Names.warning + "Wert '" + valueName + "' <");
            if (equalBool) RhinoApp.Write("=");
            RhinoApp.Write(value);
            if (cancelBool) RhinoApp.Write(Names.cancelCalc);
        }


        public static bool GetBoolOption(string msg)
        {
            bool continueBool;

            GetOption go = new GetOption();
            go.SetCommandPrompt(msg);

            go.AddOption("Ja");
            go.AddOption("Nein");

            while (true)
            {
                Rhino.Input.GetResult get_rc = go.Get();

                /*                if (go.CommandResult() != Rhino.Commands.Result.Success)
                                    return go.CommandResult();*/

                if (get_rc == Rhino.Input.GetResult.Option)
                    Rhino.RhinoApp.WriteLine("  Fortsetzen = {0}", go.Option().LocalName);
                else
                    continue;

                break;
            }

            if (go.Option().Index == 1)
                continueBool = true;
            else
                continueBool = false;

            return continueBool;
        }


        #endregion

        #region Misc

        public static Rhino.Commands.Result ZoomToObject(Rhino.RhinoDoc doc, List<Guid> idList)
        {
            BoundingBox bbox = BoundingBox.Unset;

            foreach (Guid id in idList)
            {
                var obj = doc.Objects.Find(id);
                if (obj == null)
                    return Rhino.Commands.Result.Failure;

                BoundingBox bbObj = obj.Geometry.GetBoundingBox(true);
                bbox.Union(bbObj);

            }

            const double pad = 0.5;    // A little padding...
            double dx = (bbox.Max.X - bbox.Min.X) * pad;
            double dy = (bbox.Max.Y - bbox.Min.Y) * pad;
            double dz = (bbox.Max.Z - bbox.Min.Z) * pad;
            bbox.Inflate(dx, dy, dz);

            var view = doc.Views.ActiveView;
            if (view == null)
                return Rhino.Commands.Result.Failure;

            view.ActiveViewport.ZoomBoundingBox(bbox);
            view.Redraw();

            return Rhino.Commands.Result.Success;
        }


        public static void HighlightObjRefs(ObjRef objRef)
        {
            RhinoDoc doc = RhinoDoc.ActiveDoc;

            foreach (RhinoObject rhobj in doc.Objects)
                rhobj.Highlight(false);

            RhinoObject obj = doc.Objects.FindId(objRef.ObjectId);
            if (obj != null) obj.Highlight(true);

            doc.Views.Redraw();
        }

        public static void HighlightObjRefs(List<ObjRef> objRefs)
        {
            RhinoDoc doc = RhinoDoc.ActiveDoc;

            foreach (RhinoObject rhobj in doc.Objects)
                rhobj.Highlight(false);

            foreach (ObjRef objRef in objRefs)
            {
                RhinoObject rhobj = doc.Objects.FindId(objRef.ObjectId);
                if (rhobj != null) rhobj.Highlight(true);
            }

            doc.Views.Redraw();
        }


        public static void SelectObjRefs(ObjRef objRef)
        {
            RhinoDoc doc = RhinoDoc.ActiveDoc;

            foreach (RhinoObject rhobj in doc.Objects.GetSelectedObjects(false, false))
                doc.Objects.Select(rhobj.Id, false);

            doc.Objects.Select(objRef);

            doc.Views.Redraw();
        }

        public static void SelectObjRefs(List<ObjRef> objRefs)
        {
            RhinoDoc doc = RhinoDoc.ActiveDoc;

            foreach (RhinoObject rhobj in doc.Objects.GetSelectedObjects(false, false))
                doc.Objects.Select(rhobj.Id, false);

            foreach (ObjRef objRef in objRefs)
            {
                doc.Objects.Select(objRef);
            }

            doc.Views.Redraw();
        }

        #endregion


    }
}
