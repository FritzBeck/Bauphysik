using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Input.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bauphysik.Data;
using Bauphysik.Helpers;
using LayerManager;
using LayerManager.Parser;

namespace Bauphysik.Commands
{
    public class EB_CreateReferenzflaechenCmd : Command
    {
        public override string EnglishName => "EB_ErstelleReferenzflaechen";

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
            gc.SetCommandPrompt("Wähle Innenflächen um Referenzflaechen zu erstellen");

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

            RhinoApp.WriteLine("Starte Erstelle Referenzflaechen...");

            //Create Referenzflaechen
            foreach (Innenflaeche innenflaeche in model.Innenflaechen)
                innenflaeche.CreateReferenzflaeche(model.Innenflaechen, model.Bauteile, overWriteBool);

            foreach (Innenflaeche innenflaeche1 in model.Innenflaechen)
                innenflaeche1.RemoveDuplicateReferenzflaechen(model.Innenflaechen);


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

            RhinoDoc.ActiveDoc.Objects.Select(objRefs, false);
            RhinoDoc.ActiveDoc.Objects.Select(objRefs, true);

            RhinoApp.WriteLine("Referenzflaechen erstellen abgeschlossen.");

            return Result.Success;
        }
    }
}

