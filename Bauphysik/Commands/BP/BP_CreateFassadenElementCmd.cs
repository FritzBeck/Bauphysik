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
using Rhino.ApplicationSettings;
using Bauphysik.Data;
using LayerManager;
using LayerManager.Parser;
using LayerManager.Data;
using LayerManager.Doc;

namespace Bauphysik.Commands
{
    public class BP_CreateFassadenElementCmd : Command
    {

        public override string EnglishName => "BP_ErstelleFassadenElement";

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
            gc.SetCommandPrompt("Wähle Innenflaeche um Fassadenelement zu erstellen");

            ObjRef selectedObjectRef = null;


            while (true)
            {
                gc.GeometryFilter = ObjectType.Surface;
                gc.EnablePreSelect(true, true);
                Rhino.Input.GetResult get_rc = gc.Get();

                if (gc.CommandResult() != Result.Success)
                    return gc.CommandResult();

                if (get_rc == Rhino.Input.GetResult.Object)
                {
                    selectedObjectRef = gc.Object(0);
                }
                else if (get_rc == Rhino.Input.GetResult.Option)
                {
                    continue;
                }
                break;
            }

            lmData.ReadRhinoObjects(new ObjRef[] { selectedObjectRef });
            ///rhinoReader.UpdateFromChildObjects(ref lmData);

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
            RhinoApp.WriteLine("Starte Erstelle Fassadenelement...");

            // Select Verfahren
            string[] elementArray =
            {
                "Fenster",
                "Einbauteil",
                "Nebenkonstruktion",
                "Haupkonstruktion"
            };

            var go3 = new GetOption();
            go3.SetCommandPrompt("Welches Element?");

            for (int i = 0; i < elementArray.Length; i++)
                go3.AddOption(elementArray[i]);

            go3.Get();
            if (go3.CommandResult() != Result.Success)
                return go3.CommandResult();

            int elementIndex = go3.Option().Index - 1;

            //Select point on surface
            var gp1 = new GetPointOnBreps();
            gp1.SetCommandPrompt("Punkt auf Innenflaeche");
            gp1.Breps.Add(selectedObjectRef.Brep());

            gp1.Get();
            if (gp1.CommandResult() != Result.Success)
                return gp1.CommandResult();

/*            BrepFace brepFace = selectedObjectRef.Brep().Faces[0];
            brepFace.TryGetPlane(out Plane plane);*/

            if (model.Innenflaechen == null || model.Innenflaechen.Count == 0) return Result.Failure;
            Innenflaeche innenflaeche = model.Innenflaechen[0];

            Guid guid = Guid.Empty;
            if (elementIndex == 0)
            {
                var go2 = new GetOption();
                go2.SetCommandPrompt("Welche Geometrie?");

                string[] stringArray =
                {
                    "Rechteck",
                    "Kreis",
                };

                for (int i = 0; i < stringArray.Length; i++)
                    go2.AddOption(stringArray[i]);

                go2.Get();
                if (go2.CommandResult() != Result.Success)
                    return go2.CommandResult();

                int typeIndex = go2.Option().Index - 1;

                Fenster fenster = new Fenster(guid);
                fenster.U = gp1.U;
                fenster.V = gp1.V;

                if (typeIndex == 0)
                {
                    fenster.Breite = 1;
                    fenster.Hoehe = 1;
                }

                if (typeIndex == 1)
                {
                    fenster.Durchmesser = 1;
                }

                fenster.UpdateConcreteView(innenflaeche);

                innenflaeche.Fassade.Fensters.Add(fenster);

            }
            else
            {
                if (elementIndex == 1)
                {
                    Einbauteil obj = new Einbauteil(guid);
                    obj.U = gp1.U;
                    obj.V = gp1.V;
                    obj.UpdateConcreteView(innenflaeche);

                    innenflaeche.Fassade.Einbauteils.Add(obj);
                }

                if (elementIndex == 2)
                {
                    Nebenkonstruktion obj = new Nebenkonstruktion(guid);
                    obj.U = gp1.U;
                    obj.V = gp1.V;
                    obj.UpdateConcreteView(innenflaeche);

                    innenflaeche.Fassade.Nebenkonstruktions.Add(obj);
                }

                if (elementIndex == 3)
                {
                    Hauptkonstruktion obj = new Hauptkonstruktion(guid);
                    obj.U = gp1.U;
                    obj.V = gp1.V;
                    obj.UpdateConcreteView(innenflaeche);

                    innenflaeche.Fassade.Hauptkonstruktion = obj;
                }
            }

            // End message
            RhinoApp.WriteLine("Erstelle Fassadenelement abgeschlossen.");

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


        class GetPointOnBreps : GetPoint
        {
            public readonly List<Brep> Breps;
            public Point3d ClosestPoint;
            public double U;
            public double V;

            public GetPointOnBreps()
            {
                Breps = new List<Brep>();
                ClosestPoint = Point3d.Unset;
            }

            public Point3d CalculateClosestPoint(Point3d point)
            {
                var closest_point = Point3d.Unset;
                var minimum_distance = Double.MaxValue;
                foreach (var brep in Breps)
                {
                    foreach (var face in brep.Faces)
                    {
                        double u, v;
                        if (face.ClosestPoint(point, out u, out v))
                        {
                            var face_point = face.PointAt(u, v);
                            double distance = face_point.DistanceTo(point);
                            if (distance < minimum_distance)
                            {
                                minimum_distance = distance;
                                closest_point = face_point;
                                U = u;
                                V = v;
                            }
                        }
                    }
                }
                return closest_point;
            }

            protected override void OnMouseMove(GetPointMouseEventArgs e)
            {
                ClosestPoint = CalculateClosestPoint(e.Point);
                base.OnMouseMove(e);
            }

            protected override void OnDynamicDraw(GetPointDrawEventArgs e)
            {
                if (ClosestPoint.IsValid)
                    e.Display.DrawPoint(ClosestPoint, AppearanceSettings.DefaultObjectColor);

                // Do not call base class...
                //base.OnDynamicDraw(e);
            }
        }
    }
}
