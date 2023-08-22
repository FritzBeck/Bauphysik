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
using System.Drawing;
using System.Windows.Markup;
using Bauphysik.Data;
using Bauphysik.Helpers;
using LayerManager;
using LayerManager.Parser;
using LayerManager.Data;
using LayerManager.Doc;

namespace Bauphysik.Commands
{
    public class SS_CalculateDirektSchallCmd : Command
    {
        public override string EnglishName => "SS_BerechneDirektSchall";

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

            var gc = new GetObject();
            gc.SetCommandPrompt("Wähle Innenflächen um R'_w,ges zu berechnen");

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

            // Read RhinoObjects
            lmData.ReadRhinoObjects(selectedObjectRefs);

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

            #region Error handling

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
            if (model.Raume == null || model.Raume.Count == 0)
            {
                RhinoApp.WriteLine("Keine Räume geladen.");
                return Result.Failure;
            }

            #endregion

            // <-- Code for calculations comes here -->

            // Start message
            RhinoApp.WriteLine("Starte Berechnung von Direktschalldämmmaß...");

            RhinoApp.WriteLine();
            RhinoApp.WriteLine(Names.hashSep + Names.hashSep);
            RhinoApp.WriteLine();
            RhinoApp.WriteLine(Names.shift + "Berechnung Direktschalldämmmaß");
            RhinoApp.WriteLine();
            RhinoApp.WriteLine(Names.hashSep + Names.hashSep);

            //Calculate DirektSchall
            bool first = true;
            foreach (Raum raum in model.Raume)
            {
                //Skip dashSep for first Raum
                if (first)
                {
                    first = false;
                }
                else
                {
                    RhinoApp.WriteLine();
                    RhinoApp.WriteLine(Names.dashSep + Names.dashSep);
                }
                     
                RhinoApp.WriteLine();
                RhinoApp.WriteLine(Names.shift + "Raum " + "(" + raum.ObjectId + ")");
                RhinoApp.WriteLine(Names.shift2 + "Name = " + raum.Raumname);
                RhinoApp.WriteLine(Names.shift2 + "Kategorie = " + raum.Raumkategorie);

                if (raum.Innenflaechen == null || raum.Innenflaechen.Count == 0)
                {
                    RhinoApp.WriteLine();
                    RhinoApp.WriteLine(Names.shift + "!!! Raum hat keine Innenflaechen  =>  Berechnung abgebrochen");
                    continue;
                }

                RhinoApp.Write(Names.shift2);
                double ss = raum.Ss(true);

                if (ss <= 0)
                {
                    RhinoApp.WriteLine();
                    RhinoApp.WriteLine(Names.shift + "!!! ss <= 0  =>  Berechnung abgebrochen");
                    continue;
                }
                
                foreach (Innenflaeche innenflaeche in raum.Innenflaechen)
                {
/*                    Innenflaeche innenflaeche = model.GetInnenflaeche(guid);
                    if (innenflaeche == null || innenflaeche.Fassade == null) continue;
*/
                    RhinoApp.WriteLine();
                    RhinoApp.WriteLine(Names.dashSepShort);
                    RhinoApp.WriteLine();
                    RhinoApp.WriteLine(Names.shift2 + "Innenflaeche (Guid: " + innenflaeche.ObjectId + ")");

                    innenflaeche.Fassade.BerechneDirektSchall(ss, innenflaeche, overWriteBool);
                }


            }

            RhinoApp.WriteLine();
            RhinoApp.WriteLine(Names.hashSep + Names.hashSep);
            RhinoApp.WriteLine(Names.hashSep + Names.hashSep);


            // End message
            RhinoApp.WriteLine("Berechnung von Direktschalldämmmaß abgeschlossen.");

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
