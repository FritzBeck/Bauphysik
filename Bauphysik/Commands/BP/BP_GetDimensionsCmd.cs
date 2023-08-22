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

namespace Bauphysik.Commands
{
    public class BP_GetDimensionsCmd : Command
    {
        public override string EnglishName => "BP_LeseDimensionen";

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

            //Get Innenflaechen
            var gc = new GetObject();
            gc.SetCommandPrompt("Wähle Objekte um Dimensionen zu lesen");

            ObjRef[] selectedObjectRefs = null;

            while (true)
            {
                gc.GeometryFilter = ObjectType.Curve;
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

            RhinoApp.WriteLine("Starte LeseDimensionen...");

            // Read RhinoObjects
            lmData.ReadRhinoObjects(selectedObjectRefs);

            // Get Fenster 
            List<Fenster> fensters = GetFensters(lmData);

            // Handle Errors 2
            if (fensters == null || fensters.Count == 0)
            {
                RhinoApp.WriteLine("Fehler beim Laden des Modells.");
                return Result.Failure;
            }

            // Get Dimensions
            foreach (Fenster fenster in fensters)
            {
                if (fenster == null) continue;
                fenster.GetDimensions();
            }

            // Write back to LamaData
            WriteFensters(fensters, lmData);

            // Update Rhino objects
            lmData.WriteRhinoObjects();

            // Update Panel
            LamaDoc.Instance.Update();

            // Update views
            doc.Views.Redraw();

            RhinoApp.WriteLine("Lese Dimensionen abgeschlossen.");
            return Result.Success;
        }

        public List<Fenster> GetFensters(LamaData lmData)
        {
            Connector connector = new Connector(lmData);
            List<Fenster> fensters = new List<Fenster>();

            LamaTable lmTable = lmData.LamaTables.GetTable(Helpers.Names.TypValueEnum.Fenster.ToString());
            if (lmTable == null || lmTable.Rows.Count == 0) return null;

            foreach (DataRow row in lmTable.Rows)
            {
                if (row is RhinoRow lmRow)
                {
                    Fenster fenster = connector.CreateFenster(lmRow.GetRhinoObjectID());
                    if (fenster != null) fensters.Add(fenster);
                }

            }

            return fensters;
        }

        public void WriteFensters(List<Fenster> fensters, LamaData lmData)
        {
            Connector connector = new Connector(lmData);

            foreach (Fenster fenster in fensters)
            {
                if (fenster == null) continue;
                connector.WriteFenster(fenster, null);
            }
        }
    }
}
