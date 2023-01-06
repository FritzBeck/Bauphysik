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
using System.Windows.Documents;
using System.ComponentModel;
using System.Collections;
using System.Windows.Markup;
using Bauphysik.Data;
using Bauphysik.Helpers;
using LayerManager;
using LayerManager.Parser;

namespace Bauphysik.Commands
{
    public class SS_CalculateRerfCmd : Command
    {
        public override string EnglishName => "SS_BerechneRerf";

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
            gc.SetCommandPrompt("Wähle Innenflächen um R_erf zu berechnen");

            OptionToggle overWriteBoolOpt = new OptionToggle(false, "Nein", "Ja");
            gc.AddOptionToggle("Überschreiben", ref overWriteBoolOpt);

            string[] optArray =
            {
                "DIN+VDI",
                "DIN",
                "VDI",
            };

            int listIndex = 0;
            int optList = gc.AddOptionList("Verfahren", optArray, listIndex);

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
                    RhinoApp.WriteLine(" Verfahren = {0}", optArray[listIndex]);
                }
                else if (get_rc == Rhino.Input.GetResult.Option)
                {
                    if (gc.OptionIndex() == optList)
                        listIndex = gc.Option().CurrentListOptionIndex;
                    continue;
                }
                break;
            }

            bool overWriteBool = overWriteBoolOpt.CurrentValue;

            //Select Verfahren
            /*string[] stringArray =
            {
                "DIN",
                "VDI",
            };

            var go3 = new GetOption();
            go3.SetCommandPrompt("Welches Verfahren?");

            for (int i = 0; i < stringArray.Length; i++)
                go3.AddOption(stringArray[i]);

            go3.Get();
            if (go3.CommandResult() != Result.Success)
                return go3.CommandResult();

            int selectedIndex = go3.Option().Index - 1;*/


            //Get Korrekturfaktor
            /*            double k = 0;
                        if (selectedIndex == 1)
                        {
                            var gs = new GetString();
                            gs.SetCommandPrompt("Korrekturfaktor eingeben:");
                            gs.Get();
                            if (gs.CommandResult() != Result.Success)
                                return gs.CommandResult();

                            string kString = gs.StringResult().Trim();

                            double.TryParse(kString, out k);
                        }*/


            RhinoReader rhinoReader = new RhinoReader();
            rhinoReader.UpdateFromLayers(ref data);
            rhinoReader.UpdateFromObjects(ref data, selectedObjectRefs);

            //Write Data to Model
            Connector connector = new Connector();
            BauModel model = connector.InitModel(data);

            //Handle Errors 2
            if (model == null)
            {
                RhinoApp.WriteLine(Names.shift + Names.warning + "Fehler beim Laden des Modells.");
                return Result.Failure;
            }
            if (model.Innenflaechen == null || model.Innenflaechen.Count == 0)
            {
                RhinoApp.WriteLine(Names.shift + Names.warning + "Keine Innenflaechen geladen.");
                return Result.Failure;
            }
            if (model.Raume == null || model.Raume.Count == 0)
            {
                RhinoApp.WriteLine(Names.shift + Names.warning + "Keine Räume geladen.");
                return Result.Failure;
            }
            if (model.Raumkategorien == null || model.Raumkategorien.Count == 0)
            {
                RhinoApp.WriteLine(Names.shift + Names.warning + "Keine Raumkategorien geladen.");
                return Result.Failure;
            }

            RhinoApp.WriteLine("Starte Berechnung von Rerf...");
            RhinoApp.WriteLine();
            RhinoApp.WriteLine(Names.hashSep + Names.hashSep);
            RhinoApp.WriteLine();
            RhinoApp.WriteLine(Names.shift + "Berechnung Rerf");
            RhinoApp.WriteLine();
            RhinoApp.WriteLine(Names.hashSep + Names.hashSep);

            //Calculate Rerf
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
                    RhinoApp.WriteLine(Names.shift + Names.warning + "Raum hat keine Innenflaechen  =>  Berechnung abgebrochen");
                    continue;
                }

                Raumkategorie raumkategorie = model.Raumkategorien.Cast<Raumkategorie>().Where(r => r.Name == raum.Raumkategorie).FirstOrDefault();

                if (raumkategorie == null)
                {
                    RhinoApp.WriteLine();
                    RhinoApp.WriteLine(Names.shift + "!!! Raumkategorie wurde nicht gefunden  =>  Berechnung abgebrochen");
                    continue;
                }

                if (listIndex != 2) raum.BerechneNachDIN(model.Innenflaechen, model.Raumkategorien, overWriteBool);
                if (listIndex != 1) raum.BerechneNachVDI(model.Innenflaechen, model.Raumkategorien, overWriteBool);
                raum.SetMaxRErf();
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

            RhinoApp.WriteLine("Berechnung von Rerf abgeschlossen.");

            return Result.Success;
        }
    }
}
