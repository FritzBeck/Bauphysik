using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using Rhino.Input.Custom;
using Rhino.UI;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Bauphysik.Helpers;
using Rhino.Geometry.Collections;
using static Bauphysik.Helpers.Names;

namespace Bauphysik.Data
{
    public class Innenflaeche : RhinoElement
    {
        public string Bauteil { get; set; }
        public string Zone { get; set; }

        

        public string Orientierung { get; set; }
        public double? Neigung { get; set; }

        public int? Richtung { get; set; }
        public double? Verschiebung { get; set; }
        public Guid? Richtung_GUID { get; set; }

        public bool? Grundflaeche { get; set; }
        public bool? Fassadenflaeche { get; set; }

        public Fassade Fassade { get; set; }
        public Referenzflaeche Referenzflaeche { get; set; }


        public Innenflaeche(Guid rhObjGuid)
        {
            ObjectGuid = rhObjGuid;
        }


        public double FlaecheBrutto()
        {
            Brep brep = GetBrep();
            if (brep == null) return 0;

            AreaMassProperties massProp = AreaMassProperties.Compute(brep);
            if (massProp == null) return 0;

            return massProp.Area;
        }

        public double FlaecheNetto()
        {
            double brutto = FlaecheBrutto();
            double openings = Fassade == null ? 0 : Fassade.FlaecheOeffnungen(this);

            return brutto - openings;
        }


        /// <summary>
        /// Calculate Richtung if Neigung is null or overwrite is true. 
        /// Checks the number of intersection of ray curve being normal to Innenflaeche and other Innenflaechen.
        /// Zero or even number means 1 and not even means -1.
        /// </summary>
        /// <param name="innenflaechen"></param>
        /// <param name="overWrite"></param>
        public void CalculateRichtung(List<Innenflaeche> innenflaechen, bool overWrite)
        {
            if (Richtung != null && !overWrite) return;

            //Create Ray
            Curve rayCurve = CreateCurveOut(10000, false, false);
            if (rayCurve == null) return;

            //Count intersection of Ray with Innenflaechen
            int m = 0;
            foreach (Innenflaeche innenflaeche in innenflaechen)
            {
                if (innenflaeche.ObjectGuid == this.ObjectGuid) continue;

                Brep brep = innenflaeche.GetBrep();
                if (brep == null || brep.Faces == null || brep.Faces.Count() == 0) continue;

                Intersection.CurveBrepFace(rayCurve, brep.Faces[0], 0.0, out Curve[] overlapCurves, out Point3d[] intersectionPoints);

                if (intersectionPoints != null && intersectionPoints.Length > 0)
                    m += 1;
            }

            //1 if no. of intersections is even or zero, otherwise -1
            Richtung = (m != 0 && m % 2 != 0) ? -1 : 1;

        }

        /// <summary>
        /// Calculate Verschiebung according to Bauteil if Neigung is null or overwrite is true.
        /// </summary>
        /// <param name="bauteilTabelle"></param>
        /// <param name="overWrite"></param>
        public void CalculateVerschiebung(List<Bauteil> bauteilTabelle, bool overWrite)
        {
            if (Verschiebung != null && !overWrite) return;

            if (RhinoHelpers.CheckIsNull(Bauteil, Names.BauteilAttributeEnum.Bauteil.ToString(), this.ObjectGuid.ToString(), true)) return;

            RhinoDoc doc = RhinoDoc.ActiveDoc;

            //berechne Faktor (0 bis 1)
            Verschiebung = 1;

            Curve crv1 = CreateCurveOut(5, true, false);
            if (crv1 == null) return;

            Point3d ptIn = crv1.PointAtStart;
            Point3d ptOut = crv1.PointAtEnd;
            Vector3d normalVect = ptOut - ptIn;

            //GetBauteilArt
            Bauteil bauteil = bauteilTabelle.Cast<Bauteil>().Where(b => b.Name == Bauteil).FirstOrDefault();
            if (RhinoHelpers.CheckIsNull(bauteil, Names.BauteilAttributeEnum.Bauteil.ToString(), "BauteilTabelle", true)) return;
            if (RhinoHelpers.CheckIsNull(bauteil.Art, Names.BauteilAttributeEnum.Bauteilart.ToString(), bauteil.Name, true)) return;

            if (bauteil.Art == BauteilArtEnum.D.ToString())
            {
                //Falls nach aussen "unten" ist dann verschiebe nicht
                Point3d ptOut2 = ptIn - normalVect;
                if (ptOut2.Z > ptOut.Z) Verschiebung = 0;
            }

            if (bauteil.Art == BauteilArtEnum.IW.ToString())
                Verschiebung = 0.5;

        }

        /// <summary>
        /// Calculates Orientation ("Nord", "N-O", etc.) if Neigung is null or overwrite is true
        /// </summary>
        /// <param name="lineCurve"></param>
        /// <param name="overWrite"></param>
        public void CalculateOrientation(LineCurve lineCurve, bool overWrite)
        {
            if (this.Orientierung != null && !overWrite) return;

            Vector3d v1 = lineCurve.PointAtEnd - lineCurve.PointAtStart;
            v1.Z = 0;

            if (v1.IsTiny(Rhino.RhinoMath.ZeroTolerance)) return;

            Curve crv = CreateCurveOut(5, true, false);
            if (crv == null)
            {
                Orientierung = null;
                return;
            }

            Vector3d v2 = crv.PointAtEnd - crv.PointAtStart;
            v2.Z = 0;

            if (v2.IsZero)
            {
                Orientierung = null;
                return;
            }

            Vector3d vN = Plane.WorldXY.Normal;

            Vector3d cross = Rhino.Geometry.Vector3d.CrossProduct(v1, v2);
            double angleRadians = Math.Atan2(cross * vN, v1 * v2);
            double angleDegree = (180 / Math.PI) * angleRadians;

            angleDegree = angleDegree < 0 ? angleDegree * -1 : 360 - angleDegree;
            //angleDegree = 360 - angleDegree;

            Orientierung = GetOrientation(angleDegree);

        }

        /// <summary>
        /// Calculates Neigung of Innenflaeche if Neigung is null or overwrite is true
        /// </summary>
        /// <param name="overWrite"></param>
        public void CalculateNeigung(bool overWrite)
        {
            if (Neigung != null && !overWrite) return;

            RhinoObject rhObj = RhinoDoc.ActiveDoc.Objects.Find(this.ObjectGuid);
            if (rhObj == null) return;

            ObjRef objRef = new ObjRef(rhObj);
            BrepFace brepFace = objRef.Brep().Faces[0];

            Plane plane;
            brepFace.TryGetPlane(out plane);

            Vector3d v1 = Plane.WorldXY.Normal;
            Vector3d v2 = brepFace.NormalAt(0.5, 0.5);

            double angleRadians1 = Vector3d.VectorAngle(v1, v2);
            double angleDegree1 = (180 / Math.PI) * angleRadians1;

            double angleRadians2 = Vector3d.VectorAngle(v1, -v2);
            double angleDegree2 = (180 / Math.PI) * angleRadians2;

            double angleDegree = angleDegree1 < angleDegree2 ? angleDegree1 : angleDegree2;
            Neigung = Math.Round(angleDegree, 3);
        }

        /// <summary>
        /// Get Orientation ("Nord", "N-O", etc.) based on angle degree
        /// </summary>
        /// <param name="angleDegree"></param>
        /// <returns></returns>
        private string GetOrientation(double angleDegree)
        {
            string orientation = "-";

            switch (angleDegree)
            {
                case < 22.5:
                    orientation = "Nord";
                    break;
                case < 67.5:
                    orientation = "N-O";
                    break;
                case < 112.5:
                    orientation = "Ost";
                    break;
                case < 157.75:
                    orientation = "S-O";
                    break;
                case < 202.5:
                    orientation = "Süd";
                    break;
                case < 247.55:
                    orientation = "S-W";
                    break;
                case < 292.5:
                    orientation = "West";
                    break;
                case < 337.5:
                    orientation = "N-W";
                    break;
                case <= 360:
                    orientation = "Nord";
                    break;

                default:
                    RhinoApp.WriteLine("Winkel ist größer als 360 Grad: {0}", angleDegree);
                    break;

            }

            return orientation;
        }

        /// <summary>
        /// Creates Referenzflaeche taking adjacent Innenflaechen into account
        /// </summary>
        /// <param name="innenflaeches"></param>
        /// <param name="bauteilTabelle"></param>
        /// <param name="overWriteBool"></param>
        public void CreateReferenzflaeche(List<Innenflaeche> innenflaeches, List<Bauteil> bauteilTabelle, bool overWriteBool)
        {

            if (Richtung == null) return;
            if (Verschiebung == null) return;

            //Delete Refernzflaeche
            if (Referenzflaeche != null)
            {
                if (!overWriteBool) return;

                RhinoObject refObj = Referenzflaeche.GetRhinoObject();
                if (refObj != null) RhinoDoc.ActiveDoc.Objects.Delete(refObj);
                Referenzflaeche = null;
            }

            Brep referenzBrep = CreateReferenzBrep(innenflaeches, bauteilTabelle);
            if (referenzBrep == null) return;

            //Erstelle Referenzflaeche
            Guid referenzGuid = RhinoDoc.ActiveDoc.Objects.Add(referenzBrep);
            Referenzflaeche = new Referenzflaeche(referenzGuid);
            ObjRef referenzObjRef = this.Referenzflaeche.GetObjRef();
            //RhinoDoc.ActiveDoc.Objects.Replace(referenzObjRef.ObjectId, newReferenzBrep);

        }

        #region Helpers

        /// <summary>
        /// Creates line in out direction of Innenflaeche (according to Richtung and/or Verschiebung)
        /// </summary>
        /// <param name="length"></param>
        /// <param name="applyRichtung"></param>
        /// <param name="applyVerschiebung"></param>
        /// <returns></returns>
        private Curve CreateCurveOut(double length, bool applyRichtung, bool applyVerschiebung)
        {

            double richtung = applyRichtung ? (Richtung ?? 1) : 1;
            double verschiebung = applyVerschiebung ? (Verschiebung ?? 0) : 1;

            Brep brep = GetBrep();
            if (brep == null) return null;

            if (brep.Faces == null | brep.Faces.Count() == 0) return null;
            BrepFace brepFace = brep.Faces[0];

            Point3d ptIn = AreaMassProperties.Compute(brepFace).Centroid;
            Point3d ptOut = ptIn + (brepFace.NormalAt(0.5, 0.5) * richtung * verschiebung * length);

            Curve crv = new LineCurve(ptIn, ptOut);

            return crv;
        }


        /// <summary>
        /// Duplicates Brep of Innenflaeche and move it according to Bauteildicke in out direction
        /// </summary>
        /// <param name="innenflaeches"></param>
        /// <param name="bauteilTabelle"></param>
        /// <returns></returns>
        private Brep CreateOffsetBrepByThickness(List<Innenflaeche> innenflaeches, List<Bauteil> bauteilTabelle)
        {
            //Dupliziere geometry der innenflaeche
            Brep brep = GetBrep();
            if (brep == null) return null;
            Brep referenzBrep = (Brep)brep.Duplicate();

            //Get Bauteildicke
            TryGetBauteilDicke(bauteilTabelle, out double bauteilDicke);
            bauteilDicke *= Verschiebung ?? 0;

            //Get Vector in outside direction
            if (bauteilDicke > 0)
            {
                Curve outCurve = this.CreateCurveOut(bauteilDicke, true, false);
                Vector3d outVect = outCurve.PointAtEnd - outCurve.PointAtStart;

                //Verschiebe geometry nach aussen
                if (outVect != null && !outVect.IsZero)
                {
                    Transform translateForm = Transform.Translation(outVect);
                    referenzBrep.Transform(translateForm);
                }
            }

            return referenzBrep;

        }

        /// <summary>
        /// Creates Brep of Referenzflaeche taking adjacent Innenflaechen into account
        /// </summary>
        /// <param name="innenflaeches"></param>
        /// <param name="bauteilTabelle"></param>
        /// <returns></returns>
        private Brep CreateReferenzBrep(List<Innenflaeche> innenflaeches, List<Bauteil> bauteilTabelle)
        {
            Brep brep = GetBrep();
            if (brep == null) return null;
            BrepEdgeList brepEdgeList = brep.Edges;

            //Dupliziere Brep und verschiebe sie entsprechend der Dicke nach aussen 
            Brep referenzBrep = CreateOffsetBrepByThickness(innenflaeches, bauteilTabelle);

            List<Curve> offsetCrvs = new List<Curve>();
            for(int i = 0; i < referenzBrep.Edges.Count(); i++)
            {
                //Verschieb Kanten der ReferenzBrep nach aussen
                Curve offsetEdge = MoveBrepEdgeByAdjacentThickness(innenflaeches, bauteilTabelle, brep.Edges[i], referenzBrep, referenzBrep.Edges[i]);
                offsetCrvs.Add(offsetEdge);

/*                RhinoDoc.ActiveDoc.Objects.AddCurve(offsetEdge);
                RhinoDoc.ActiveDoc.Views.Redraw();*/
            }

            //Verlängere die Kante (damit später Schnittpunkt mit benachbarten Kanten gefunden werden kann)
            List<Curve> extendedOffsetCrvs = new List<Curve>();
            foreach (Curve curve in offsetCrvs)
            {
                Curve crv = ExtendCurve(curve, 100);
                extendedOffsetCrvs.Add(crv);
            }

            //Sortiere Kanten sodass benachbarte hinterheinander sind
            List<int> orderedIndices = new List<int>() { 0 };
            for (int i = 0; i < brepEdgeList.Count; i++)
            {
                int currentIndex = orderedIndices[i];
                for (int j = 0; j < brepEdgeList.Count; j++)
                {
                    if (currentIndex == j || orderedIndices.Contains(j)) continue;
                    if (CheckIntersection(brepEdgeList[currentIndex], brepEdgeList[j]))
                    {
                        orderedIndices.Add(j);
                        break;
                    }
                }
            }

            List<BrepEdge> orderedEdges = new List<BrepEdge>();
            List<Curve> orderedExtendedOffsetCurves = new List<Curve>();
            foreach (int i in orderedIndices)
            {
                orderedEdges.Add(brepEdgeList[i]);
                orderedExtendedOffsetCurves.Add(extendedOffsetCrvs[i]);
            }

            //Erstelle Polyline durch Verschneidung der extendedOffsetCrvs
            Polyline point3ds = new Polyline();
            for (int i = 0; i < brepEdgeList.Count; i++)
            {
                int previousIndex = i == 0 ? offsetCrvs.Count - 1 : i - 1;
                int nextIndex = i == offsetCrvs.Count - 1 ? 0 : i + 1;

                //Falls parallel dann bekomme intersection points mit hilfslinie
                CurveIntersections nextCurveIntersection = Rhino.Geometry.Intersect.Intersection.CurveCurve(orderedExtendedOffsetCurves[i], orderedExtendedOffsetCurves[nextIndex], 0.0, 0.0);
                if (nextCurveIntersection == null || nextCurveIntersection.Count == 0)
                {
                    // oder auch mit Hilfslinie berechnen..
                    var edgeEvents = Rhino.Geometry.Intersect.Intersection.CurveCurve(orderedExtendedOffsetCurves[i], orderedExtendedOffsetCurves[nextIndex], 0.0, 0.0);
                    Point3d ptStart = edgeEvents[0].PointA;

                    BrepFace brepFace = brep.Faces[0];
                    Plane plane;
                    brepFace.TryGetPlane(out plane);


                    //Bestimme Ebene in der verschoben werden soll und berechne normale
                    Point3d ponto = orderedEdges[i].PointAtNormalizedLength(0.5);
                    double rotationAngle = (Math.PI / 180) * 90;
                    Vector3d rotationAxis = orderedEdges[i].PointAtStart - orderedEdges[i].PointAtEnd;

                    var xform = Transform.Rotation(rotationAngle, rotationAxis, ponto);
                    plane.Transform(xform);

                    Vector3d normal = plane.Normal;
                    Point3d ptEnd = ptStart + normal;
                    LineCurve helperOffsetCurveIntermediate1 = new LineCurve(ptStart, ptEnd);

                    Curve helperOffsetCurveIntermediate2 = helperOffsetCurveIntermediate1.Extend(CurveEnd.Start, 100, CurveExtensionStyle.Line);
                    Curve helperOffsetCurve = helperOffsetCurveIntermediate2.Extend(CurveEnd.End, 100, CurveExtensionStyle.Line);


                    var offsetEvents = Rhino.Geometry.Intersect.Intersection.CurveCurve(helperOffsetCurve, orderedExtendedOffsetCurves[i], 0.0, 0.0);
                    point3ds.Add(offsetEvents[0].PointA);

                    var offsetEvents2 = Rhino.Geometry.Intersect.Intersection.CurveCurve(helperOffsetCurve, orderedExtendedOffsetCurves[nextIndex], 0.0, 0.0);
                    point3ds.Add(offsetEvents2[0].PointA);

                    continue;
                }



                //Falls Dicke kleiner als die Verschneidung benachbarter Kanten dann lass Kante weg und füge Schnittpunkt der benachbarten kanten hinzu
                CurveIntersections crvIntersections = Rhino.Geometry.Intersect.Intersection.CurveCurve(orderedExtendedOffsetCurves[previousIndex], orderedExtendedOffsetCurves[nextIndex], 0.0, 0.0);
                if (crvIntersections != null && crvIntersections.Count != 0 && crvIntersections[0] != null && crvIntersections[0].PointA != null)
                {
                    Point3d intersectionPoint = crvIntersections[0].PointA;

                    orderedExtendedOffsetCurves[i].ClosestPoint(intersectionPoint, out double param);
                    Point3d closestPointOnEdge = orderedExtendedOffsetCurves[i].PointAt(param);

                    double distanceToIntersectionPt = closestPointOnEdge.DistanceTo(intersectionPoint);

                    Innenflaeche adjacentInneflaeche = GetAdjacentInnenflaeche(orderedEdges[i], innenflaeches, 0);
                    adjacentInneflaeche.TryGetBauteilDicke(bauteilTabelle, out double adjacentDicke);
                    adjacentDicke *= adjacentInneflaeche.Verschiebung ?? 0;

                    if (distanceToIntersectionPt < adjacentDicke)
                    {
                        point3ds.Add(intersectionPoint);
                        continue;
                    }

                }

                CurveIntersections crvIntersections2 = Rhino.Geometry.Intersect.Intersection.CurveCurve(orderedExtendedOffsetCurves[i], orderedExtendedOffsetCurves[nextIndex], 0.0, 0.0);
                if (crvIntersections2 != null && crvIntersections2.Count > 0 && crvIntersections2[0] != null && crvIntersections2[0].PointA != null)
                {
                    point3ds.Add(crvIntersections2[0].PointA);
                }

            }

            if (point3ds == null || point3ds.Count < 3)
                return null;

            point3ds.Add(point3ds[0]);
            return CreateBrepFromPolyline(point3ds);
        }


        /// <summary>
        /// Extends curve at the start and end
        /// </summary>
        /// <param name="curve"></param>
        /// <returns>Returns extended curve</returns>
        private Curve ExtendCurve(Curve curve, double length)
        {
            Curve crv2 = curve.Extend(CurveEnd.Start, length, CurveExtensionStyle.Line);
            return crv2.Extend(CurveEnd.End, length, CurveExtensionStyle.Line);
        }

        /// <summary>
        /// Moves BrepEdge of Innenflaeche according to thickness of adjacent Innenflaeche
        /// </summary>
        /// <param name="innenflaeches"></param>
        /// <param name="bauteilTabelle"></param>
        /// <param name="brepEdge"></param>
        /// <param name="referenzBrep"></param>
        /// <param name="referenzBrepEdge"></param>
        /// <returns>Returns moved BrepEdge as curve</returns>
        private Curve MoveBrepEdgeByAdjacentThickness(List<Innenflaeche> innenflaeches, List<Bauteil> bauteilTabelle, BrepEdge brepEdge, Brep referenzBrep, BrepEdge referenzBrepEdge)
        {
            Curve offsetEdge = (Curve)referenzBrepEdge.Duplicate();

            Innenflaeche adjacentInneflaeche = GetAdjacentInnenflaeche(brepEdge, innenflaeches, 0);
            adjacentInneflaeche.TryGetBauteilDicke(bauteilTabelle, out double adjacentDicke);
            adjacentDicke *= adjacentInneflaeche.Verschiebung ?? 0;

            if (adjacentDicke > 0)
            {
                Vector3d normalVect = referenzBrep.Faces[0].NormalAt(0.5, 0.5);

                Brep adjacentBrep = adjacentInneflaeche.GetBrep();
                Vector3d normalAdjacent = adjacentBrep.Faces[0].NormalAt(0.5, 0.5);

                double vectorsAngleRadians = Vector3d.VectorAngle(normalVect, normalAdjacent);
                adjacentDicke *= Math.Tan(vectorsAngleRadians / 2);
                //adjacentDicke *= adjacentInneflaeche.Verschiebung ?? 0;

                Vector3d faceOutVect = GetOutVect(referenzBrep, adjacentInneflaeche.GetBrep(), referenzBrepEdge);
                faceOutVect *= adjacentDicke;

                Transform xform = Transform.Translation(faceOutVect);
                offsetEdge.Transform(xform);
            }

            return offsetEdge;
        }


        /// <summary>
        /// Tries to get thickness of Bauteil of Innenflaeche
        /// </summary>
        /// <param name="bauteilListe"></param>
        /// <param name="bauteilDicke"></param>
        /// <returns>Returns true if Bauteildicke was found</returns>
        public bool TryGetBauteilDicke(List<Bauteil> bauteilListe, out double bauteilDicke)
        {
            bauteilDicke = 0;

            Bauteil bauteil = bauteilListe.Cast<Bauteil>().Where(b => b.Name == this.Bauteil).FirstOrDefault();
            if (bauteil == null) return false;
            if (bauteil.Dicke == null) return false;

            bauteilDicke = bauteil.Dicke ?? 0;
            if (bauteilDicke > 0) bauteilDicke /= 100;

            return true;
        }

        /// <summary>
        /// Checks Intersection of two curves
        /// </summary>
        /// <param name="referenzBrep"></param>
        /// <param name="adjacentBrep"></param>
        /// <param name="brepEdge"></param>
        /// <returns>Returns true if curves intersects</returns>
        private bool CheckIntersection(Curve crv1, Curve crv2)
        {

            var events = Rhino.Geometry.Intersect.Intersection.CurveCurve(crv1, crv2, 0, 0);

            if (events != null && events.Count > 0)
                return true;

            return false;

        }


        /// <summary>
        /// Creates Brep based on closed Polyline.
        /// </summary>
        /// <param name="point3ds"></param>
        /// <returns>Returns null if Brep creation fails</returns>
        private Brep CreateBrepFromPolyline(Polyline point3ds)
        {
            if (point3ds.Count > 1)
            {
                point3ds.Insert(0, point3ds[point3ds.Count - 1]);
                Curve crv = point3ds.ToNurbsCurve();
                Brep[] breps = Brep.CreatePlanarBreps(crv, 0.001);
                if (breps != null && breps.Length > 0)
                    return breps[0];

            }

            return null;
        }


        /// <summary>
        /// Gets the unitzed Vector in plane of brep at the side of the adjacentBrep.
        /// Vector points from edge in out direction of brep.
        /// </summary>
        /// <param name="referenzBrep"></param>
        /// <param name="adjacentBrep"></param>
        /// <param name="brepEdge"></param>
        /// <returns></returns>
        private Vector3d GetOutVect(Brep referenzBrep, Brep adjacentBrep, BrepEdge brepEdge)
        {


            Point3d ponto = brepEdge.PointAtNormalizedLength(0.5);
            Vector3d adjBrepOutVect = adjacentBrep.Faces[0].NormalAt(0.5, 0.5) * (Richtung ?? 1);

            Plane plane;
            referenzBrep.Faces[0].TryGetPlane(out plane);

            Point3d pointOut = ponto + adjBrepOutVect;
            Point3d pointOutOnPlane = plane.ClosestPoint(pointOut);

            Vector3d outVect = pointOutOnPlane - ponto;
            outVect.Unitize();

            /*          OLD
                        AreaMassProperties massProp = AreaMassProperties.Compute(referenzBrep);
                        Point3d insidePt = massProp.Centroid;

                        //Point3d onEdge = brepEdge.PointAtEnd + (brepEdge.PointAtEnd - brepEdge.PointAtStart) / 2;
                        Point3d onEdge = brepEdge.PointAt(0.5);

                        Vector3d outVect = onEdge - insidePt;
                        outVect.Unitize();*/

            return outVect;
        }


        /// <summary>
        /// Gets Innenflaeche adjacent to specific edge of Innenflaeche
        /// </summary>
        /// <param name="edge">Edge for which adjacent Innenflaeche shall be identified</param>
        /// <param name="innenflaeches"></param>
        /// <param name="adjacentWallTolerance">Distance tolerance for finding adjacent wall</param>
        /// <returns>Returns null if no adjacent Innenflaeche was found</returns>
        private Innenflaeche GetAdjacentInnenflaeche(BrepEdge edge, List<Innenflaeche> innenflaeches, double adjacentWallTolerance)
        {

            foreach (Innenflaeche innenflaeche in innenflaeches)
            {
                if (innenflaeche.ObjectGuid == ObjectGuid) continue;

                ObjRef objRefToCheck = new ObjRef(RhinoDoc.ActiveDoc, innenflaeche.ObjectGuid);
                if (objRefToCheck == null) return null;

                foreach (BrepEdge edgeToCheck in objRefToCheck.Brep().Edges)
                {
                    CurveIntersections events = Intersection.CurveCurve(edge, edgeToCheck, 0.0, adjacentWallTolerance);
                    if (events == null || events.Count == 0) continue;
                    if (!events[0].IsOverlap) continue;

                    return innenflaeche;
                }

            }

            return null;
        }

        /// <summary>
        /// Checks whether there is another Referenzflaeche with same Geometry.
        /// If yes, then Referenzflaeche will be deleted and the other Referenzflaeche will be assigned to this Innenflaeche.
        /// </summary>
        /// <param name="innenflaeches"></param>
        public bool RemoveDuplicateReferenzflaechen(List<Innenflaeche> innenflaeches)
        {
            if (Referenzflaeche == null) return false;

            Brep referenzBrep = this.Referenzflaeche.GetBrep();
            if (referenzBrep == null) return false;

            //Checke ob es schon eine Referenzflaeche mit der selben Geometry gibt
            foreach (Innenflaeche innenflaeche in innenflaeches)
            {
                //Falls beide die gleiche Referenzflaeche haben, dann weiter
                if (innenflaeche.Referenzflaeche == null || innenflaeche.Referenzflaeche.ObjectGuid == Referenzflaeche.ObjectGuid) continue;

                Brep brepToCheck = innenflaeche.Referenzflaeche.GetBrep();
                if (referenzBrep.IsDuplicate(brepToCheck, 0))
                {
                    //Falls ja, dann loesche neue Referenzflaeche..
                    RhinoDoc.ActiveDoc.Objects.Delete(this.Referenzflaeche.GetRhinoObject());

                    //..und update referenzflaeche von dieser Innenflaeche
                    this.Referenzflaeche = innenflaeche.Referenzflaeche;
                    return true;
                }
            }

            return false;
        }


        #endregion




        #region old

        private double CalculateExtensionLength(Brep adjacentBrep, double bauteilDicke2)
        {

            //ToDo: Make it work for other elements
            if (adjacentBrep.Faces == null | adjacentBrep.Faces.Count() == 0) return 0;


            BrepFace adjacentBrepFace = adjacentBrep.Faces[0];
            Vector3d normalAdjacent = adjacentBrepFace.NormalAt(0.5, 0.5);

            Brep brep = GetBrep();
            Vector3d normal = brep.Faces[0].NormalAt(0.5, 0.5);

            /*            BrepFace brepFace = referenzBrep.Faces[0];
                        Vector3d normal = brepFace.NormalAt(0.5, 0.5);*/

            double vectorsAngleRadians = Vector3d.VectorAngle(normal, normalAdjacent);
            return bauteilDicke2 * Math.Tan((vectorsAngleRadians / 2));

        }


        private double GetExtensionLength(Innenflaeche adjacentInneflaeche, List<Bauteil> bauteile)
        {
            //Get Bauteildicke der Adjacent Innenflaechen
            double extensionLength = 0;

            if (adjacentInneflaeche != null)
            {
                if (adjacentInneflaeche.TryGetBauteilDicke(bauteile, out double adjacentDicke))
                {
                    adjacentDicke *= adjacentInneflaeche.Verschiebung ?? 1;

                    //Brep adjacentBrep = new ObjRef(RhinoDoc.ActiveDoc, adjacentInneflaeche.ObjectGuid).Brep();
                    //extensionLength = CalculateExtensionLength(adjacentBrep, adjacentDicke);
                    extensionLength = adjacentDicke;
                }
            }

            return extensionLength;
        }

        private Surface CreateExtendedSurface(BrepTrim referenzTrim, ObjRef referenzObjRef, double extensionLength)
        {
            if (referenzTrim == null) return null;

            Surface surface = referenzObjRef.Surface();
            if (surface == null) return null;

            return surface.Extend(referenzTrim.IsoStatus, extensionLength, true);
        }


        //Move edges of Referenzflaeche
        /* OLD
          for (int i = 0; i < referenzBrep.Trims.Count; i++)
                    {
                        BrepEdge brepEdge = brep.Trims[i].Edge;
                        double extensionLength = GetExtensionLength(innenflaeches, bauteilTabelle, brepEdge);

                        //Extend surface of referenzflaeche

                        //Surface ist beschränkt auf Flaechen mit 4 Kanten
                        Surface extended_surface = CreateExtendedSurface(referenzBrep.Trims[i], referenzObjRef, extensionLength);
                        Brep newReferenzBrep = Brep.CreateFromSurface(extended_surface);

                        RhinoDoc.ActiveDoc.Objects.Replace(referenzObjRef.ObjectId, newReferenzBrep);
                    }*/

        //Verschiebe Kanten nach aussen

        /*            for (int i = 0; i < referenzBrep.Edges.Count; i++)
                    {
                        Innenflaeche adjacentInneflaeche = GetAdjacentInnenflaeche(brep.Edges[i], innenflaeches, 0);
                        double extensionLength = GetExtensionLength(adjacentInneflaeche, bauteilTabelle);

                        Vector3d faceOutVect = GetOutVect(referenzBrep, referenzBrep.Edges[i]);
                        faceOutVect *= extensionLength;
                        //new_polyline_curve.SetPoint(i, point);

                        //Get Vector in outside Direction based on Adjacent thickness
                        Transform xform = Transform.Translation(faceOutVect);
                        Curve offsetEdge = (Curve)referenzBrep.Edges[i].Duplicate();
                        offsetEdge.Transform(xform);

                        offsetCrvs.Add(offsetEdge);

                        RhinoDoc.ActiveDoc.Objects.AddCurve(offsetEdge);
                        RhinoDoc.ActiveDoc.Views.Redraw();
                    }*/

        #endregion

    }
}
