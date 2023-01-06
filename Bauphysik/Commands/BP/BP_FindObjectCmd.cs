using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Input.Custom;
using System.Windows.Markup;
using System.Data;
using Bauphysik.Helpers;
using Bauphysik.Data;
using LayerManager;
using LayerManager.Parser;

namespace Bauphysik.Commands
{
    public class BP_FindObjectCmd : Command
    {

        public override string EnglishName => "BP_FindeObjekt";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            var gc = new GetString();
            gc.SetCommandPrompt("Gebe Guid des Objektes ein");
            gc.AcceptNothing(false);
            gc.Get();

            if (gc.CommandResult() != Result.Success)
                return gc.CommandResult();

            string str = gc.StringResult();

            RhinoApp.WriteLine("Starte Finde Objekt...");

            if (!Guid.TryParse(str, out Guid guid))
            {
                RhinoApp.WriteLine(Names.shift + Names.warning + str + " ist keine gültige Guid");
                return Result.Failure;
            }

            RhinoObject rhObj = doc.Objects.FindId(guid);
            if (rhObj == null)
            {
                RhinoApp.WriteLine(Names.shift + Names.warning + str + " Kein RhinoObject gefunden");
                return Result.Failure;
            }

            RhinoApp.WriteLine(Names.shift + "Objekt mit ID " + str + " gefunden");

            RhinoHelpers.SelectObjRefs(new ObjRef(doc, guid));

            RhinoApp.WriteLine("Finde Objekt abgeschlossen");

            return Result.Success;
        }

    }
}
