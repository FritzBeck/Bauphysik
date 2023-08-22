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
using LayerManager.Data;
using LayerManager.Doc;

namespace Bauphysik.Commands
{
    public class EB_CalculateOrientationCmd : Command
    {

        public override string EnglishName => "EB_BerechneOrientierung";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            #region Get LamaData

            // Return if ActiveLamaData == null
            if (LamaDoc.Instance.ActiveLamaData == null)
            {
                RhinoApp.WriteLine("Keine Tabellen geladen.");
                return Result.Success;
            }

            // Abbreviate ActiveLamaData
            LamaData lmData = LamaDoc.Instance.ActiveLamaData;

            #endregion

            // <-- Code for object selection comes here -->

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



            // <-- Code for object selection ends here -->

            

            #region Get Model

            // Write Data to Model
            Connector connector = new Connector(lmData);
            BauModel model = connector.Read();
            if (model == null)
            {
                RhinoApp.WriteLine("Fehler beim Laden des Modells.");
                return Result.Failure;
            }

            #endregion

            // <-- Code for calculations comes here -->

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

            // Start message
            RhinoApp.WriteLine("Starte Berechnung der Orientierung...");

            //Calculate orientation
            foreach (Innenflaeche innenflaeche in model.Innenflaechen)
                innenflaeche.CalculateOrientation(lineCurve, overWriteBool);

            // End message
            RhinoApp.WriteLine("Berechnung der Orientierung abgeschlossen.");

            // <-- Code for calculations ends here -->

            #region Write model

            // Write back to LamaData
            connector.Write(model);

            // Update Rhino objects
            lmData.WriteRhinoObjects();

            
            

            // Update Panel
            LamaDoc.Instance.Update();

            // Update views
            doc.Views.Redraw();

            #endregion

            return Result.Success;
        }
    }
}
