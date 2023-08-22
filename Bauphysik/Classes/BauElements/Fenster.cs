using Bauphysik.Helpers;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Bauphysik.Data
{
    public class Fenster : FassadenElement
    {

        public double? Hoehe;

        public double? Breite;

        public double? Durchmesser;

        

        public int? Anzahl;

        public double? Flaeche;


        public double? GWert;

        public double? Bruestungshoehe;

        public double? Riw;

        public double? UWert;


        public Guid relatedInnenflaeche;


        public InputTypeEnum inputType;

        public enum InputTypeEnum
        {
            Seite,
            Flaeche,
            Vollverglast,
            
        }


        public Fenster(Guid objectGuid)
        {
            ObjectId = objectGuid;
            Typ = Names.TypValueEnum.Fenster.ToString();
        }

        public Fenster()
        {

        }

        private double? FlaecheEinzeln()
        {

            if (Hoehe != null && Breite != null)
            {
                double hoehe = Hoehe ?? 0;
                double breite = Breite ?? 0;
                Durchmesser = null;
                return hoehe * breite;
            }

            if (Durchmesser != null)
            {
                double d = Durchmesser ?? 0;
                return d == 0 ? d : (d/2) * (d/2) * Math.PI;
            }

            return null;
        }


        public double FlaecheGesamt()
        {
            double? flaecheEinzeln = FlaecheEinzeln();

            if (flaecheEinzeln == null && Flaeche != null)
                return Flaeche ?? 0;

            int anzahl = Anzahl ?? 1;
            Flaeche = flaecheEinzeln * anzahl;

            return Flaeche ?? 0;
        }


        /// <summary>
        /// Update Geometry of Fenster based on Dimensions
        /// </summary>
        /// <param name="relatedInnenflaeche">Underlying Innenflaeche</param>
        public void SetGeometry(Innenflaeche relatedInnenflaeche)
        {
            Guid newGuid = Guid.Empty;
            if (relatedInnenflaeche.Fassade.Vollverglast == true)
            {
                Brep brep = relatedInnenflaeche.GetBrep();
                newGuid = RhinoDoc.ActiveDoc.Objects.AddBrep(brep.DuplicateBrep());
            }

            if (Hoehe != null && Breite != null)
            {
                Brep wallBrep = relatedInnenflaeche.GetBrep();
                if (wallBrep == null) return;

                if (!wallBrep.IsSurface) return;
                BrepFace brepFace = wallBrep.Faces[0];
                double areaWall = AreaMassProperties.Compute(brepFace).Area;

                Plane plane;
                brepFace.TryGetPlane(out plane);

                Interval widthInterval = new Interval(0, Breite ?? 1);
                Interval heightInterval = new Interval(0, Hoehe ?? 1);
                Rectangle3d rect = new Rectangle3d(plane, widthInterval, heightInterval);

                newGuid = RhinoDoc.ActiveDoc.Objects.AddRectangle(rect);
            }

            if (Durchmesser != null)
            {
                Brep wallBrep = relatedInnenflaeche.GetBrep();
                if (wallBrep == null) return;

                if (!wallBrep.IsSurface) return;
                BrepFace brepFace = wallBrep.Faces[0];
                double areaWall = AreaMassProperties.Compute(brepFace).Area;

                Plane plane;
                brepFace.TryGetPlane(out plane);

                double radius = (Durchmesser ?? 1) / 2;
                Circle circle = new Circle(plane, radius);

                newGuid = RhinoDoc.ActiveDoc.Objects.AddCircle(circle);
            }

            if (newGuid != Guid.Empty)
            {
                RhinoObject rhObj = this.GetRhinoObject();
                if (rhObj != null) RhinoDoc.ActiveDoc.Objects.Delete(rhObj);

                this.ObjectId = newGuid;
            }

        }

        /// <summary>
        /// Get Dimensions of underlying geometry (rectangle or circle)
        /// </summary>
        public void GetDimensions()
        {
            ObjRef brep = GetObjRef();
            if (brep == null) return;

            Curve crv = brep.Curve();
            if (crv == null) return;

            if (crv.IsCircle())
            {
                if (crv.TryGetCircle(out Circle circle))
                {
                    this.Durchmesser = Math.Round(circle.Diameter, 3);
                    this.Hoehe = null;
                    this.Breite = null;
                }
            }

            if (crv.TryGetPolyline(out Polyline polyline))
            {
                if (polyline.Count() == 5)
                {
                    Point3d p1 = polyline[1];

                    Point3d p0 = polyline[0];
                    Point3d p2 = polyline[2];

                    double x = (p0 - p1).Length;
                    double y = (p2 - p1).Length;

                    if (p1.Z == p0.Z)
                    {
                        this.Breite = Math.Round(x, 3);
                        this.Hoehe = Math.Round(y, 3);
                        this.Durchmesser = null;
                    }
                    else if (p1.Z == p2.Z)
                    {
                        this.Breite = Math.Round(y, 3);
                        this.Hoehe = Math.Round(x, 3);
                        this.Durchmesser = null;
                    }

                }
            }

        }


        /// <summary>
        /// Calculate Reiw
        /// </summary>
        /// <param name="ss">Fassadenflaeche from related Raum</param>
        public void BerechneReiw37(double ss)
        {

            RhinoApp.WriteLine();
            RhinoApp.WriteLine(Names.shift3 + "Fenster (Guid: " + ObjectId.ToString() + ")");

            if (RhinoHelpers.CheckIsNull(Riw, Names.FensterAttributeEnum.Riw.ToString(), this.ObjectId.ToString(), true)) return;

            double flaeche = FlaecheGesamt();
            double r_iw = Riw ?? 0;

            RhinoApp.WriteLine(Names.shift4 + "Flaeche = " + flaeche.ToString());
            RhinoApp.WriteLine(Names.shift4 + "Riw = " + Riw.ToString());

            double r_eiw = r_iw + 10 * Math.Log10(ss / flaeche);
            Reiw = Math.Round(r_eiw, 3);

            
            RhinoApp.WriteLine(Names.shift4 + "R_e,i,w = " + r_iw + " + 10 * log10(" + ss + "/ " + flaeche + ") = " + Reiw);
        }


    }

}