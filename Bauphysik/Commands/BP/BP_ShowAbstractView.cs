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
using Bauphysik.Data;
using LayerManager;
using LayerManager.Parser;
using LayerManager.Data;
using LayerManager.Doc;
using Bauphysik.Helpers;

namespace Bauphysik.Commands
{
    public class BP_ShowAbstractView : Command
    {

        public override string EnglishName => "BP_AbstrakteAnsicht";

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

            // Get Innenflaechen
            var gc = new GetObject();
            gc.SetCommandPrompt("Wähle Objekte um abstrakte Ansicht zu zeigen.");

            ObjRef[] selectedObjectRefs = null;

            while (true)
            {
                //gc.GeometryFilter = ObjectType.Surface;
                gc.EnablePreSelect(true, true);
                Rhino.Input.GetResult get_rc = gc.GetMultiple(1, 0);

                if (gc.CommandResult() != Result.Success)
                    return gc.CommandResult();

                if (get_rc == Rhino.Input.GetResult.Object)
                {
                    selectedObjectRefs = gc.Objects();
                }
                else if (get_rc == Rhino.Input.GetResult.Option)
                {
                    continue;
                }
                break;
            }

            // Read RhinoObjects
            lmData.ReadRhinoObjects();

            // Read only selected Innenflaeche
            LamaTable lmTable = lmData.LamaTables.GetTable(Names.TypValueEnum.Innenflaeche.ToString());
            if (lmTable == null)
            {
                RhinoApp.WriteLine("Keine Tabelle 'Innenflaeche' gefunden.");
                return Result.Success;
            }

            lmTable.Clear();

            if (lmTable is RhinoElementTable rhinoTable)
            {
                rhinoTable.ReadRhinoObjects(selectedObjectRefs);

                RhinoApp.WriteLine("ObjRefs" + selectedObjectRefs.Length.ToString());
                RhinoApp.WriteLine("Rows" + rhinoTable.Rows.Count.ToString());
            }


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
            RhinoApp.WriteLine("Starte Zeige abstrakte Ansicht...");

            //Update Innenflaechen
            if (model.Innenflaechen != null)
                foreach (Innenflaeche innenflaeche in model.Innenflaechen)
                {
                    if (innenflaeche.Fassade == null) continue;
                    innenflaeche.Fassade.UpdateAbstractView(innenflaeche);
                }

            // End message
            RhinoApp.WriteLine("Zeige abstakte Ansicht abgeschlossen");

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
