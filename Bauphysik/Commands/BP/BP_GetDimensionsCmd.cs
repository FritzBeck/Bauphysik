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

namespace Bauphysik.Commands
{
    public class BP_GetDimensionsCmd : Command
    {

        public override string EnglishName => "BP_LeseDimensionen";

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

            RhinoApp.WriteLine("Starte Lese Dimensionen...");

            RhinoReader rhinoReader = new RhinoReader();
            rhinoReader.UpdateFromLayers(ref data);
            rhinoReader.UpdateFromObjects(ref data, selectedObjectRefs);

            //Get Fenster 
            List<Fenster> fensters = GetFensters(data);

            //Handle Errors 2
            if (fensters == null || fensters.Count == 0)
            {
                RhinoApp.WriteLine("Fehler beim Laden des Modells.");
                return Result.Failure;
            }

            foreach (Fenster fenster in fensters)
            {
                if (fenster == null) continue;
                fenster.GetDimensions();
            }

            //Write Fenster to Data
            WriteFensters(fensters, ref data);

            //Update Rhino and XML File from Data
            XMLWriter writer = new XMLWriter();
            writer.WriteFile(data, filepath);

            RhinoWriter rhinoWriter = new RhinoWriter();
            rhinoWriter.UpdateRhino(data);

            //Update Panel
            ObjRef[] objRefs = RhinoDoc.ActiveDoc.Objects.GetSelectedObjects(false, false).Cast<RhinoObject>().Select(i => new ObjRef(i)).ToArray();
            if (objRefs == null || objRefs.Length == 0) return Result.Failure;

            RhinoDoc.ActiveDoc.Objects.Select(objRefs, false);
            RhinoDoc.ActiveDoc.Objects.Select(objRefs, true);

            RhinoApp.WriteLine("Lese Dimensionen abgeschlossen.");
            return Result.Success;
        }

        public List<Fenster> GetFensters(RootDB data)
        {
            Connector connector = new Connector();
            List<Fenster> fensters = new List<Fenster>();

            TableDB tableDB = data.TableDBs.GetTable(Helpers.Names.TypValueEnum.Fenster.ToString());
            if (tableDB == null || tableDB.Table == null || tableDB.Table.Rows.Count == 0) return null;

            foreach (DataRow row in tableDB.Table.Rows)
            {
                Guid guid = TableDB.GetRowID(row);
                Fenster fenster = connector.CreateFenster(guid, data);
                if (fenster != null) fensters.Add(fenster);
            }

            return fensters;
        }


        public void WriteFensters(List<Fenster> fensters, ref RootDB data)
        {
            Connector connector = new Connector();

            foreach (Fenster fenster in fensters)
            {
                if (fenster == null) continue;
                connector.WriteFenster(fenster, null, ref data);
            }
        }
    }
}
