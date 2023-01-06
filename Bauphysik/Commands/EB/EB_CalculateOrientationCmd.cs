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
using Bauphysik.Data;
using Bauphysik.Helpers;
using LayerManager;
using LayerManager.Parser;

namespace Bauphysik.Commands
{
    public class EB_CalculateOrientationCmd : Command
    {

        public override string EnglishName => "EB_BerechneOrientierung";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            //Lade FilePath
            string filepath = BauphysikPlugin.Instance.StringArray.Item(0);
            if (string.IsNullOrWhiteSpace(filepath))
            {
                RhinoApp.WriteLine("Keine Tabellen verknüpft.");
                return Result.Success;
            }

            //Lade Data
            XMLReader xMLReader = new XMLReader();
            RootDB data = xMLReader.ReadFile(filepath);
            if (data == null)
            {
                RhinoApp.WriteLine("Kein gültiger Pfad verknüpft.");
                return Result.Success;
            }


            //Get Innenflaechen
            var gc = new GetObject();
            gc.SetCommandPrompt("Wähle Innenflächen um Orientierung zu berechnen");

            OptionToggle overWriteBoolOpt = new OptionToggle(false, "Nein", "Ja");
            gc.AddOptionToggle("Überschreiben", ref overWriteBoolOpt);

            ObjRef[] selectedObjectRefs = null;

            while (true)
            {
                gc.GeometryFilter = ObjectType.Surface;
                gc.EnablePreSelect(true, true);
                Rhino.Input.GetResult get_rc = gc.GetMultiple(1, 0);

                if (gc.CommandResult() != Result.Success)
                    return gc.CommandResult();

                if (get_rc == Rhino.Input.GetResult.Object)
                {
                    selectedObjectRefs = gc.Objects();
                    RhinoApp.WriteLine(" Überschreiben = {0}", overWriteBoolOpt.CurrentValue);
                }
                else if (get_rc == Rhino.Input.GetResult.Option)
                {
                    continue;
                }
                break;
            }

            bool overWriteBool = overWriteBoolOpt.CurrentValue;

            //Get Startpunkt
            GetPoint gp = new GetPoint();
            gp.SetCommandPrompt("Wähle Startpunkt (Süden)");
            gp.Get();
            if (gp.CommandResult() != Result.Success)
                return gp.CommandResult();

            Point3d pt_start = gp.Point();

            //Get Endpunkt
            gp.SetCommandPrompt("Wähle Endpunkt (Norden)");
            gp.SetBasePoint(pt_start, false);
            gp.DrawLineFromPoint(pt_start, true);
            gp.Get();
            if (gp.CommandResult() != Result.Success)
                return gp.CommandResult();

            Point3d pt_end = gp.Point();

            //Create Line
            LineCurve lineCurve = new LineCurve(pt_start, pt_end);

            RhinoApp.WriteLine("Starte Berchnung der Orientierung...");



            RhinoReader rhinoReader = new RhinoReader();
            rhinoReader.UpdateFromLayers(ref data);
            rhinoReader.UpdateFromObjects(ref data, selectedObjectRefs);

            //Write Data to Model
            Connector connector = new Connector();
            BauModel model = connector.InitModel(data);

            //Handle Errors 2
            if (model == null)
            {
                RhinoApp.WriteLine("Fehler beim Laden des Modells.");
                return Result.Failure;
            }
            if (model.Innenflaechen == null || model.Innenflaechen.Count == 0)
            {
                RhinoApp.WriteLine("Keine Innenflaechen geladen.");
                return Result.Failure;
            }

            //Calculate orientation
            foreach (Innenflaeche innenflaeche in model.Innenflaechen)
                innenflaeche.CalculateOrientation(lineCurve, overWriteBool);

            //Write Model to Data
            connector.Write(model, ref data);

            //Update Rhino and XML File from Data
            XMLWriter writer = new XMLWriter();
            writer.WriteFile(data, filepath);

            RhinoWriter rhinoWriter = new RhinoWriter();
            rhinoWriter.UpdateRhino(data);

            //Update Views in Rhino
            RhinoDoc.ActiveDoc.Views.Redraw();

            //Update Panel
            ObjRef[] objRefs = RhinoDoc.ActiveDoc.Objects.GetSelectedObjects(false, false).Cast<RhinoObject>().Select(i => new ObjRef(i)).ToArray();
            if (objRefs == null || objRefs.Length == 0) return Result.Failure;

            RhinoDoc.ActiveDoc.Objects.Select(objRefs[0], false);
            RhinoDoc.ActiveDoc.Objects.Select(objRefs[0], true);

            RhinoApp.WriteLine("Berechnung der Orientierung abgeschlossen.");

            return Result.Success;
        }
    }
}
