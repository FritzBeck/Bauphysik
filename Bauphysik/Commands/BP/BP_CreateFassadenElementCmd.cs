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

namespace Bauphysik.Commands
{
    public class BP_CreateFassadenElementCmd : Command
    {

        public override string EnglishName => "BP_ErstelleFassadenElement";

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


            RhinoReader rhinoReader = new RhinoReader();
            rhinoReader.UpdateFromLayers(ref data);
            rhinoReader.UpdateFromObjects(ref data, new ObjRef[] { selectedObjectRef } );
            rhinoReader.UpdateFromChildObjects(ref data);

            //Write Data to Model
            Connector connector = new Connector();
            BauModel model = connector.InitModel(data);
            if (model.Innenflaechen == null || model.Innenflaechen.Count == 0)
            {
                RhinoApp.WriteLine("Keine Innenflaechen geladen.");
                return Result.Failure;
            }


/*            if (model.Innenflaechen[0].Fassade == null)
            {
                RhinoApp.WriteLine("Keine Fassade geladen.");
                return Result.Failure;
            }

            Fassade fassade = model.Innenflaechen[0].Fassade;
            List<ObjRef> fassadeObjects = GetFassadeObjectGuids(fassade);

            data = model*/


            //Select Verfahren
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

            BrepFace brepFace = selectedObjectRef.Brep().Faces[0];
            brepFace.TryGetPlane(out Plane plane);

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

                RhinoApp.WriteLine("Starte Erstelle Fassadenelement...");

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

            connector.Write(model, ref data);

            doc.Views.Redraw();

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


            RhinoApp.WriteLine("Erstelle Fassadenelement abgeschlossen.");
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
