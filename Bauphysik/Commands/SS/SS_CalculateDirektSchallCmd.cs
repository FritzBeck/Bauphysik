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

namespace Bauphysik.Commands
{
    public class SS_CalculateDirektSchallCmd : Command
    {
        public override string EnglishName => "SS_BerechneDirektSchall";

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

            RhinoApp.WriteLine("Starte Berechnung von Direktschalldämmmaß...");

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
            if (model.Raume == null || model.Raume.Count == 0)
            {
                RhinoApp.WriteLine("Keine Räume geladen.");
                return Result.Failure;
            }

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
                RhinoApp.WriteLine(Names.shift + "Raum " + "(" + raum.ObjectGuid + ")");
                RhinoApp.WriteLine(Names.shift2 + "Name = " + raum.Raumname);
                RhinoApp.WriteLine(Names.shift2 + "Kategorie = " + raum.Raumkategorie);

                if (raum.RelatedGuids == null || raum.RelatedGuids.Count == 0)
                {
                    RhinoApp.WriteLine();
                    RhinoApp.WriteLine(Names.shift + "!!! Raum hat keine Innenflaechen  =>  Berechnung abgebrochen");
                    continue;
                }

                RhinoApp.Write(Names.shift2);
                double ss = raum.Ss(model.Innenflaechen, true);

                if (ss <= 0)
                {
                    RhinoApp.WriteLine();
                    RhinoApp.WriteLine(Names.shift + "!!! ss <= 0  =>  Berechnung abgebrochen");
                    continue;
                }
                

                foreach (Guid guid in raum.RelatedGuids)
                {
                    Innenflaeche innenflaeche = model.FindInnenflaeche(guid);
                    if (innenflaeche == null || innenflaeche.Fassade == null) continue;

                    RhinoApp.WriteLine();
                    RhinoApp.WriteLine(Names.dashSepShort);
                    RhinoApp.WriteLine();
                    RhinoApp.WriteLine(Names.shift2 + "Innenflaeche (Guid: " + innenflaeche.ObjectGuid + ")");

                    innenflaeche.Fassade.BerechneDirektSchall(ss, innenflaeche, overWriteBool);
                }


            }

            RhinoApp.WriteLine();
            RhinoApp.WriteLine(Names.hashSep + Names.hashSep);
            RhinoApp.WriteLine(Names.hashSep + Names.hashSep);

            //Write Model to Data
            connector.Write(model, ref data);

            //Update Rhino and XML File from Data
            XMLWriter writer = new XMLWriter();
            writer.WriteFile(data, filepath);

            RhinoWriter rhinoWriter = new RhinoWriter();
            rhinoWriter.UpdateRhino(data);

            //Update Views in Rhino
            RhinoDoc.ActiveDoc.Views.Redraw();

            RhinoApp.WriteLine("Berechnung von Direktschalldämmmaß abgeschlossen.");

            return Result.Success;
        }
    }
}
