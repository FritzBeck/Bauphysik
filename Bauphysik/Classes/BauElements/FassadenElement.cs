using Bauphysik.Helpers;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Bauphysik.Data
{
    public abstract class FassadenElement : RhinoElement
    {

        public double? Reiw;

        public double? U;

        public double? V;

        public bool? IsAbstract;

        private double GetArea(Innenflaeche innenflaeche)
        {
            if (this is Fenster fenster)
                return fenster.FlaecheGesamt();

            if (this is Nebenkonstruktion nebenkonstruktion)
                return nebenkonstruktion.Flaeche ?? 0;

            if (this is Hauptkonstruktion hauptkonstruktion)
                return hauptkonstruktion.Flaeche(innenflaeche);

            return 0;
        }

        /// <summary>
        /// Update abstract view on Fassadenelement
        /// </summary>
        /// <param name="innenflaeche"></param>
        public void UpdateAbstractView(Innenflaeche innenflaeche, ref double lengthMovement)
        {
            this.UpdateUV(innenflaeche);
            this.CreateAbstractViewObject(innenflaeche);
            this.MoveToAbstractView(innenflaeche, ref lengthMovement);

            IsAbstract = true;
        }


        /// <summary>
        /// Store position of concrete Fassadenelement as u, v coordinates related to BrepFace of innenflaeche
        /// </summary>
        /// <param name="innenflaeche"></param>
        private void UpdateUV(Innenflaeche innenflaeche)
        {
            if (IsAbstract != true)
            {
                //get brep
                ObjRef objRef = this.GetObjRef();
                if (objRef == null) return;

                Curve crv = objRef.Curve();
                if (crv == null) return;

                AreaMassProperties prop = AreaMassProperties.Compute(crv);
                if (prop == null) return;
                
                Point3d centroid = prop.Centroid;

                //get wall brep
                Brep wallBrep = innenflaeche.GetBrep();
                if (wallBrep == null) return;
                if (!wallBrep.IsSurface) return;

                if (wallBrep.Faces[0].ClosestPoint(centroid, out double u, out double v))
                {
                    U = u;
                    V = v;
                }

            }
        }

        /// <summary>
        /// Create abstract Fassadenelement
        /// </summary>
        /// <param name="innenflaeche"></param>
        public void CreateAbstractViewObject(Innenflaeche innenflaeche)
        {
            this.DeleteRhinoObject();

            Brep wallBrep = innenflaeche.GetBrep();
            if (wallBrep == null) return;

            if (!wallBrep.IsSurface) return;
            BrepFace brepFace = wallBrep.Faces[0];
            double areaWall = AreaMassProperties.Compute(brepFace).Area;

            Plane plane;
            brepFace.TryGetPlane(out plane);

            double heigth;
            double length;
            if (this is Einbauteil)
            {
                heigth = 0.1;
                length = 0.1;
            }
            else
            {
                double areFacadeElement = GetArea(innenflaeche);
                heigth = 0.5;
                length = areFacadeElement / areaWall * 2;
            }

            Interval widthInterval = new Interval(0, heigth);
            Interval heightInterval = new Interval(0, length);
            PlaneSurface surface = new PlaneSurface(plane, widthInterval, heightInterval);
            ObjectId = RhinoDoc.ActiveDoc.Objects.AddSurface(surface);

            return;
        }


        /// <summary>
        /// Move abstract Fassadenelement to correct position
        /// </summary>
        /// <param name="innenflaeche"></param>
        /// <param name="lengthMovement"></param>
        private void MoveToAbstractView(Innenflaeche innenflaeche,ref double lengthMovement)
        {
            //Get innenflaeche
            Brep wallBrep = innenflaeche.GetBrep();
            if (wallBrep == null) return;

            Point3d wallBrepCenter = AreaMassProperties.Compute(wallBrep).Centroid;
            wallBrep.Faces[0].TryGetPlane(out Plane wallPlane);

            List<BrepEdge> edges = wallBrep.Edges.ToList();
            BrepEdge edge = GetLowestEdge(edges);

            Vector3d sideVector = edge.PointAtStart - edge.PointAtEnd;
            Point3d sidePt = edge.PointAtStart - sideVector * 0.5;

            Vector3d sidePtCentroidVector = sidePt - wallBrepCenter;
            sidePtCentroidVector.Unitize();
            Point3d referencePt = wallBrepCenter + sidePtCentroidVector * 1;

            //Point3d referencePoint = wallBrep.Faces[0].PointAt(this.U ?? 0.5, this.V ?? 0.5);

            //Get obj to transform
            ObjRef objRef = this.GetObjRef();
            if (objRef == null) return;


            if (this is Einbauteil)
            {
                //set referencepoint 2 for Einbauteil
                sideVector.Unitize();
                referencePt = referencePt + sideVector * 0.4;
                wallBrepCenter = wallBrepCenter + sideVector * 0.4;

                //move to reference point
                Point3d elementCenter = AreaMassProperties.Compute(objRef.Brep()).Centroid;
                Vector3d vector = referencePt - elementCenter;
                RhinoDoc.ActiveDoc.Objects.Transform(objRef, Transform.Translation(vector), true);

                //move to right place
                Point3d elementCenter2 = AreaMassProperties.Compute(objRef.Brep()).Centroid;
                Vector3d vector2 = wallBrepCenter - elementCenter2;
                Vector3d transfVector = vector2 * lengthMovement;
                Transform xform = Transform.Translation(transfVector);
                RhinoDoc.ActiveDoc.Objects.Transform(objRef, xform, true);

                lengthMovement += 0.2;

            }
            else
            {
                //move to reference point
                Point3d elementCenter = AreaMassProperties.Compute(objRef.Brep()).Centroid;
                Vector3d vector = referencePt - elementCenter;
                RhinoDoc.ActiveDoc.Objects.Transform(objRef, Transform.Translation(vector), true);

                double height = 2;
                double area = GetArea(innenflaeche);
                double areaWall = innenflaeche.FlaecheBrutto();

                lengthMovement += area / areaWall * height / 2;

                //move to right place
                Point3d elementCenter2 = AreaMassProperties.Compute(objRef.Brep()).Centroid;
                Vector3d vector2 = wallBrepCenter - elementCenter2;
                Vector3d transfVector = vector2 * lengthMovement;
                Transform xform = Transform.Translation(transfVector);
                RhinoDoc.ActiveDoc.Objects.Transform(objRef, xform, true);


                lengthMovement += area / areaWall * height / 2;
            }



        }

        /// <summary>
        /// Update concrete view on Fassadenelement
        /// </summary>
        /// <param name="innenflaeche"></param>
        public void UpdateConcreteView(Innenflaeche innenflaeche)
        {
            UpdateUV(innenflaeche);
            CreateConcreteViewObject(innenflaeche);
            MoveToConcreteView(innenflaeche);

            IsAbstract = false;
        }

        /// <summary>
        /// Create concrete Fassadenelement
        /// </summary>
        /// <param name="innenflaeche"></param>
        public void CreateConcreteViewObject(Innenflaeche innenflaeche)
        {
            this.DeleteRhinoObject();

            Brep brep = innenflaeche.GetBrep();
            if (brep == null) return;

            brep.Faces[0].TryGetPlane(out Plane plane);
            if (plane == null) return;

            if (this is Fenster fenster)
            {
                if (fenster.Breite != null && fenster.Hoehe != null)
                {
                    Rectangle3d rect = new Rectangle3d(plane, fenster.Breite ?? 0, fenster.Hoehe ?? 0);
                    this.ObjectId = RhinoDoc.ActiveDoc.Objects.AddRectangle(rect);

                    return;
                }

                if (fenster.Durchmesser != null)
                {
                    Circle circle = new Circle(plane, fenster.Durchmesser/2 ?? 0);
                    this.ObjectId = RhinoDoc.ActiveDoc.Objects.AddCircle(circle);

                    return;
                }
            }
            else
            {
                Rectangle3d rect = new Rectangle3d(plane, 0.5, 0.5);
                this.ObjectId = RhinoDoc.ActiveDoc.Objects.AddRectangle(rect);
            }

        }

        /// <summary>
        /// Move concrete Fassdenelemente to correct position
        /// </summary>
        /// <param name="innenflaeche"></param>
        private void MoveToConcreteView(Innenflaeche innenflaeche)
        {
            //Get innenflaeche
            Brep wallBrep = innenflaeche.GetBrep();
            if (wallBrep == null) return;

            Point3d wallBrepCenter = AreaMassProperties.Compute(wallBrep).Centroid;
            Point3d referencePoint = wallBrep.Faces[0].PointAt(this.U ?? 0.5, this.V ?? 0.5);

            //Get obj to transform
            ObjRef objRef = this.GetObjRef();
            if (objRef == null) return;

            Curve crv = objRef.Curve();
            if (crv == null) return;

            //move to reference point
            Point3d brepCenter = AreaMassProperties.Compute(crv).Centroid;
            Vector3d vector = referencePoint - brepCenter;
            RhinoDoc.ActiveDoc.Objects.Transform(objRef, Transform.Translation(vector), true);

            RhinoDoc.ActiveDoc.Views.Redraw();
        }

        /// <summary>
        /// Helper function to move abstract Fassadenelement to correct position.
        /// </summary>
        /// <param name="edges"></param>
        /// <returns></returns>
        private BrepEdge GetLowestEdge(List<BrepEdge> edges)
        {
            BrepEdge edge = edges[0];
            double minZ = edges[0].PointAtStart.Z + edges[0].PointAtEnd.Z;
            for (int i = 0; i < edges.Count; i++)
            {
                double z = edges[i].PointAtStart.Z + edges[i].PointAtEnd.Z;
                if (z < minZ)
                {
                    edge = edges[i];
                    minZ = z;
                }
            }

            return edge;
        }

    }
}
