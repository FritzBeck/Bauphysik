using Bauphysik.Data;
using Bauphysik.Helpers;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Input.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LayerManager;
using LayerManager.Parser;

namespace Bauphysik.Commands
{
    public class EB_CreateReferenzflaecheCmd : Command
    {
        public override string EnglishName => "EB_ErstelleEinzelneReferenzflaeche";

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
            gc.SetCommandPrompt("Wähle Innenfläche für die Referenzflaeche erstellt werden soll");

            ObjRef[] selectedObjRefs = null;
            while (true)
            {
                gc.GeometryFilter = ObjectType.Surface;
                gc.EnablePreSelect(true, true);
                Rhino.Input.GetResult get_rc = gc.Get();

                if (gc.CommandResult() != Rhino.Commands.Result.Success)
                    return gc.CommandResult();

                if (get_rc == Rhino.Input.GetResult.Object)
                {
                    selectedObjRefs = gc.Objects();
                    //Rhino.RhinoApp.WriteLine(" Zusammenfassen = {0}", reduceAttributeTableBoolOpt.CurrentValue);
                }
                else if (get_rc == Rhino.Input.GetResult.Option)
                {
                    continue;
                }
                break;
            }

            ObjRef selectedObjRef = selectedObjRefs[0];

            var gc2 = new GetObject();
            gc2.SetCommandPrompt("Wähle benachbarte Innenflächen");
            gc2.GeometryFilter = ObjectType.Surface;
            gc2.EnablePreSelect(false, true);
            gc2.AcceptNothing(true);
            gc2.GetMultiple(0, 0);

            if (gc2.CommandResult() != Rhino.Commands.Result.Success)
                return gc2.CommandResult();

            ObjRef[] adjacentObjRefs = gc2.Objects();


            //Get List of all Objects
            List<ObjRef> objRefList = new List<ObjRef>() { selectedObjRef };
            if (adjacentObjRefs != null && adjacentObjRefs.Length > 0) objRefList.AddRange(adjacentObjRefs);

            //Updata LayerManager.Data by selecting
            RhinoHelpers.SelectObjRefs(objRefList);

            RhinoReader rhinoReader = new RhinoReader();
            rhinoReader.UpdateFromLayers(ref data);
            rhinoReader.UpdateFromObjects(ref data, objRefList.ToArray());

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

            //Create Referenzflaeche
            Innenflaeche innenflaeche = model.FindInnenflaeche(selectedObjRef.ObjectId);
            if (innenflaeche == null)
            {
                RhinoApp.WriteLine("Keine Innenflaechen nicht gefunden.");
                return Result.Failure;
            }

            innenflaeche.CreateReferenzflaeche(model.Innenflaechen, model.Bauteile, true);

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

            RhinoApp.WriteLine("Erstelle Referenzflaechen abgeschlossen.");

            return Result.Success;
        }
    }
}

