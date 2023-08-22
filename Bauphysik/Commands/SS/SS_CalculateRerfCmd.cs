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
using LayerManager.Data;
using LayerManager.Doc;

namespace Bauphysik.Commands
{
    public class SS_CalculateRerfCmd : Command
    {
        public override string EnglishName => "SS_BerechneRerf";

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

            // Read RhinoObjects
            lmData.ReadRhinoObjects(selectedObjectRefs);

            // <-- Code for object selection ends here -->

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
            #endregion


            // <-- Code for calculations comes here -->

            // Start message
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
                RhinoApp.WriteLine(Names.shift + "Raum " + "(" + raum.ObjectId + ")");
                RhinoApp.WriteLine(Names.shift2 + "Name = " + raum.Raumname);
                RhinoApp.WriteLine(Names.shift2 + "Kategorie = " + raum.Raumkategorie);

                if (raum.Innenflaechen == null || raum.Innenflaechen.Count == 0)
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

            // End message
            RhinoApp.WriteLine("Berechnung von Rerf abgeschlossen.");

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
