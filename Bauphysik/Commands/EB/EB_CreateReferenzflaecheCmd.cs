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
using LayerManager.Data;
using LayerManager.Doc;

namespace Bauphysik.Commands
{
    public class EB_CreateReferenzflaecheCmd : Command
    {
        public override string EnglishName => "EB_ErstelleEinzelneReferenzflaeche";

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

            // Read RhinoObjects
            LamaDoc.Instance.ActiveLamaData.ReadRhinoObjects(objRefList.ToArray());

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

            // Start message
            RhinoApp.WriteLine("Starte erstelle Referenzflaechen abgeschlossen.");

            //Create Referenzflaeche
            Innenflaeche innenflaeche = model.GetInnenflaeche(selectedObjRef.ObjectId);
            if (innenflaeche == null)
            {
                RhinoApp.WriteLine("Keine Innenflaechen nicht gefunden.");
                return Result.Failure;
            }

            innenflaeche.CreateReferenzflaeche(model.Innenflaechen, model.Bauteile, true);
            
            // End message
            RhinoApp.WriteLine("Erstelle Referenzflaechen abgeschlossen.");

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

