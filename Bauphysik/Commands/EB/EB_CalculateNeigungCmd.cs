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
    public class EB_CalculateNeigungCmd : Command
    {

        public override string EnglishName => "EB_BerechneNeigung";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            #region Get LamaData

            // Return if ActiveLamaData == null
            if (LamaDoc.Instance.ActiveLamaData == null)
            {
                RhinoApp.WriteLine("Keine Tabellen geladen.");
                return Result.Success;
            }

            #endregion

            // <-- Code for object selection comes here -->

            //Get Innenflaechen
            var gc = new GetObject();
            gc.SetCommandPrompt("Wähle Innenflächen um Neigung zu berechnen");

            OptionToggle overWriteBoolOpt = new OptionToggle(false, "Nein", "Ja");
            gc.AddOptionToggle("Überschreiben", ref overWriteBoolOpt);

            ObjRef[] selectedObjectRefs = null;

            while (true)
            {
                gc.GeometryFilter = ObjectType.Surface;
                gc.EnablePreSelect(true, true);
                Rhino.Input.GetResult get_rc = gc.GetMultiple(1, 0);

                if (gc.CommandResult() != Rhino.Commands.Result.Success)
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

            // Read RhinoObjects
            LamaDoc.Instance.ActiveLamaData.ReadRhinoObjects(selectedObjectRefs);

            // Write LamaData to Model
            Connector connector = new Connector(LamaDoc.Instance.ActiveLamaData);
            BauModel model = connector.Read();
            if (model == null)
            {
                RhinoApp.WriteLine(Names.shift + Names.warning + "Fehler beim Laden des Modells." + Names.cancelCalc);
                return Result.Failure;
            }

            #endregion

            // <-- Code for calculations comes here -->

            // Start message
            RhinoApp.WriteLine("Starte Berechne Neigung...");

            //Calculate orientation
            if (model.Innenflaechen != null)
                foreach (Innenflaeche innenflaeche in model.Innenflaechen)
                    innenflaeche.CalculateNeigung(overWriteBool);

            // End message
            RhinoApp.WriteLine("Berechne Neigung abgeschlossen.");

            // <-- Code for calculations ends here -->

            #region Write model

            // Write back to LamaData
            connector.Write(model);

            // Update Rhino objects
            LamaDoc.Instance.ActiveLamaData.WriteRhinoObjects();

            // Update Panel
            LamaDoc.Instance.Update();

            // Update views
            doc.Views.Redraw();

            #endregion

            return Result.Success;
        }
    }
}
