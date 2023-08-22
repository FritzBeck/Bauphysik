using Bauphysik.Helpers;
using Rhino;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.AccessControl;

namespace Bauphysik.Data
{
    public class Raum : SpatialElement
    {
        public Guid ObjectId { get; set; }

        public string Raumname { get; set; }
        public string Raumkategorie { get; set; }
        public string Raumgruppe { get; set; }
        public double? DIN_Laermpegel { get; set; }
        public double? VDI_Verkehr_Tag { get; set; }
        public double? VDI_Verkehr_Nacht { get; set; }
        public double? VDI_Gewerbe_Tag { get; set; }
        public double? VDI_Gewerbe_Nacht { get; set; }
        public double? Rerf_DIN { get; set; }
        public double? Rerf_VDI_Tag { get; set; }
        public double? Rerf_VDI_Nacht { get; set; }

        public double? Rerf_Max { get; set; }

        public List<Innenflaeche> Innenflaechen { get; set; } = new List<Innenflaeche>();


        public Raum(Guid guid)
        {
            ObjectId = guid;
        }


        public double Sg(bool writeOutput)
        {
            if (Innenflaechen == null || Innenflaechen.Count == 0) return 0;

            List<double> bruttoFlaechen = Innenflaechen.Cast<Innenflaeche>().Where(i => i.Grundflaeche == true).Select(i => i.FlaecheBrutto()).ToList();

            double sg = 0;
            if (writeOutput) RhinoApp.Write("sg = ");
            for (int i = 0; i < bruttoFlaechen.Count; i++)
            {
                sg += bruttoFlaechen[i];
                if (writeOutput) RhinoApp.Write(Math.Round(bruttoFlaechen[i], 3).ToString());
                if (i < bruttoFlaechen.Count - 1)
                    if (writeOutput) RhinoApp.Write(" + ");
            }

            if (bruttoFlaechen != null && bruttoFlaechen.Count > 0)
                if (writeOutput) RhinoApp.Write(" = ");

            sg = Math.Round(sg, 3);
            if (writeOutput) RhinoApp.WriteLine(sg.ToString());

            return sg;
        }

        public double Ss(bool writeOutput)
        {
            if (Innenflaechen == null || Innenflaechen.Count == 0) return 0;
            List<double> bruttoFlaechen = Innenflaechen.Cast<Innenflaeche>().Where(i => i.Grundflaeche == true).Select(i => i.FlaecheBrutto()).ToList();

            double ss = 0;
            if (writeOutput) RhinoApp.Write("ss = ");
            for (int i = 0; i < bruttoFlaechen.Count; i++)
            {
                ss += bruttoFlaechen[i];
                if (writeOutput) RhinoApp.Write(Math.Round(bruttoFlaechen[i], 3).ToString());
                if (i < bruttoFlaechen.Count - 1)
                    if (writeOutput) RhinoApp.Write(" + ");
            }

            if (bruttoFlaechen != null && bruttoFlaechen.Count > 0)
                if (writeOutput) RhinoApp.Write(" = ");

            ss = Math.Round(ss, 3);
            if (writeOutput) RhinoApp.WriteLine(ss.ToString());

            return ss;
        }


        public void BerechneNachVDI(List<Innenflaeche> innenflaechen, List<Raumkategorie> raumkategorien, bool overWriteBool)
        {

            if (Rerf_VDI_Tag != null && !overWriteBool) return;
            if (Innenflaechen == null || Innenflaechen.Count == 0)
            {
                RhinoApp.WriteLine(Names.shift + Names.warning + "Dem Raum sind keine Innenflaechen zugeordnet."+ Names.cancelCalc);
                return;
            }

            if (RhinoHelpers.CheckIsNull(VDI_Verkehr_Tag, Names.RaumAttributesEnum.VDIVerkehrTag.ToString(), this.Raumname, true)) return;
            if (RhinoHelpers.CheckIsNull(VDI_Gewerbe_Tag, Names.RaumAttributesEnum.VDIVerkehrNacht.ToString(), this.Raumname, true)) return;
            if (RhinoHelpers.CheckIsNull(VDI_Verkehr_Nacht, Names.RaumAttributesEnum.VDIGewerbeTag.ToString(), this.Raumname, true)) return;
            if (RhinoHelpers.CheckIsNull(VDI_Gewerbe_Nacht, Names.RaumAttributesEnum.VDIGewerbeNacht.ToString(), this.Raumname, true)) return;

            if (raumkategorien == null || raumkategorien.Count == 0) return;
            Raumkategorie raumkategorie = raumkategorien.Cast<Raumkategorie>().Where(r => r.Name == Raumkategorie).FirstOrDefault();
            if (RhinoHelpers.CheckIsNull(raumkategorie, Names.RaumAttributesEnum.Raumkategorie.ToString(), "RaumkategorieTabelle", true)) return;
            if (RhinoHelpers.CheckIsNull(raumkategorie.K, Names.RaumkategorieEnum.KWert.ToString(), raumkategorie.Name, true)) return;
            if (RhinoHelpers.CheckIsNull(raumkategorie.liTag, Names.RaumkategorieEnum.InnenTag.ToString(), raumkategorie.Name, true)) return;

            double vDI_Verkehr_Tag = VDI_Verkehr_Tag ?? 0;
            double vDI_Gewerbe_Tag = VDI_Gewerbe_Tag ?? 0;
            double vDI_Verkehr_Nacht = VDI_Verkehr_Nacht ?? 0;
            double vDI_Gewerbe_Nacht = VDI_Gewerbe_Nacht ?? 0;

            double laTag = Math.Round(10 * Math.Log10(Math.Pow(10, 0.1 * (vDI_Verkehr_Tag + 3)) + Math.Pow(10, 0.1 * (vDI_Gewerbe_Tag + 3))), 2);
            double laNacht = Math.Round(10 * Math.Log10(Math.Pow(10, 0.1 * (vDI_Verkehr_Nacht + 3)) + Math.Pow(10, 0.1 * (vDI_Gewerbe_Nacht + 3))), 2);

            double k = raumkategorie.K ?? 0;
            double liTag = raumkategorie.liTag ?? 0;

            RhinoApp.WriteLine();
            RhinoApp.WriteLine(Names.shift2 + "- Berechnung nach VDI");
            RhinoApp.WriteLine();
            RhinoApp.WriteLine(Names.shift3 + "k = " + k);

            RhinoApp.Write(Names.shift3);
            double ss = Ss(true);

            if (ss <= 0)
            {
                RhinoApp.WriteLine();
                RhinoApp.WriteLine(Names.shift + Names.warning + "ss <= 0  =>  Berechnung abgebrochen");
                return;
            }

            RhinoApp.Write(Names.shift3);
            double sg = Sg(true);
            

            if (sg <= 0)
            {
                RhinoApp.WriteLine();
                RhinoApp.WriteLine(Names.shift + Names.warning + "sg <= 0  =>  Berechnung abgebrochen");
                return;
            }

            // Berechnung Tag
            double rValueTag = laTag - liTag + 10 * Math.Log10(ss / (0.8 * sg)) + k + 2;
            rValueTag = Math.Round(rValueTag, 1);

            RhinoApp.WriteLine();
            RhinoApp.WriteLine(Names.shift3 + "- Berechnung Tag:");
            RhinoApp.WriteLine();
            RhinoApp.WriteLine(Names.shift4 + "la_verkehr_tag = " + VDI_Verkehr_Tag);
            RhinoApp.WriteLine(Names.shift4 + "la_gewerbe_tag = " + VDI_Gewerbe_Tag);
            RhinoApp.WriteLine(Names.shift4 + "laTag = 10 * log(10 ^ (0,1 * (" + VDI_Verkehr_Tag + " + 3)) + 10 ^ (0,1 * (" + VDI_Gewerbe_Tag + " + 3)) = " + laTag);
            RhinoApp.WriteLine(Names.shift4 + "liTag = " + liTag);
            RhinoApp.WriteLine(Names.shift4 + Names.RaumAttributesEnum.RerfVDITag.ToString() + " = " + laTag + " - " + liTag + " + 10 * log(" + ss + " / (0.8 * " + sg + ")) + " + k + " + 2 = " + rValueTag.ToString());

            Rerf_VDI_Tag = rValueTag;

            // Berechnung Nacht (nur für Wohnung)
            if (Raumkategorie != Names.RaumkategorieEnum.Gewerbe.ToString())
            {
                if (RhinoHelpers.CheckIsNull(raumkategorie.liNacht, Names.RaumkategorieEnum.InnenNacht.ToString(), raumkategorie.Name, true)) return;
                double liNacht = raumkategorie.liNacht ?? 0;

                double rValueNacht = laNacht - liNacht + 10 * Math.Log10(ss / (0.8 * sg)) + k + 2;
                rValueNacht = Math.Round(rValueNacht, 1);

                RhinoApp.WriteLine();
                RhinoApp.WriteLine(Names.shift3 + "- Berechnung Nacht: ");
                RhinoApp.WriteLine();
                RhinoApp.WriteLine(Names.shift4 + "la_verkehr_nacht = " + VDI_Verkehr_Nacht);
                RhinoApp.WriteLine(Names.shift4 + "la_gewerbe_nacht = " + VDI_Gewerbe_Nacht);
                RhinoApp.WriteLine(Names.shift4 + "laNacht = 10 * log(10 ^ (0,1 * (" + VDI_Verkehr_Nacht + " + 3)) + 10 ^ (0,1 * (" + VDI_Gewerbe_Nacht + " + 3)) = " + laNacht);
                RhinoApp.WriteLine(Names.shift4 + "liNacht = " + liNacht);
                RhinoApp.WriteLine(Names.shift4 + Names.RaumAttributesEnum.RerfVDINacht.ToString() + " = " + laNacht + " - " + liNacht + " + 10 * log(" + ss + " / (0.8 * " + sg + ")) + " + k + " + 2 = " + rValueNacht.ToString());

                Rerf_VDI_Nacht = rValueNacht;
            }
        }

        public void BerechneNachDIN(List<Innenflaeche> innenflaechen, List<Raumkategorie> raumkategorien, bool overWriteBool)
        {
            if (Rerf_DIN != null && !overWriteBool) return;

            if (RhinoHelpers.CheckIsNull(DIN_Laermpegel, Names.RaumAttributesEnum.DINLaermpegel.ToString(), this.Raumname, true)) return;

            if (raumkategorien == null || raumkategorien.Count == 0) return;
            Raumkategorie raumkategorie = raumkategorien.Cast<Raumkategorie>().Where(r => r.Name == Raumkategorie).FirstOrDefault();
            if (RhinoHelpers.CheckIsNull(raumkategorie, Names.RaumAttributesEnum.Raumkategorie.ToString(), "RaumkategorieTabelle", true)) return;
            if (RhinoHelpers.CheckIsNull(raumkategorie.K, Names.RaumkategorieEnum.KWert.ToString(), raumkategorie.Name, true)) return;

            double la = DIN_Laermpegel ?? 0;
            double k = raumkategorie.K ?? 0;

            RhinoApp.WriteLine(Names.shift2 + "- Berechnung nach DIN");
            RhinoApp.WriteLine();
            RhinoApp.WriteLine(Names.shift3 + "k = " + k);

            RhinoApp.Write(Names.shift3);
            double ss = Ss(true);

            if (ss <= 0)
            {
                RhinoApp.WriteLine();
                RhinoApp.WriteLine(Names.shift + Names.warning + "ss <= 0  =>  Berechnung abgebrochen");
                return;
            }

            RhinoApp.Write(Names.shift3);
            double sg = Sg(true);

            if (sg <= 0)
            {
                RhinoApp.WriteLine();
                RhinoApp.WriteLine(Names.shift + Names.warning + "sg <= 0  =>  Berechnung abgebrochen");
                return;
            }

            RhinoApp.WriteLine(Names.shift3 + "la = " + la);
            RhinoApp.WriteLine();

            double rValue = la - k + 10 * Math.Log10(ss / (0.8 * sg)) + 2;
            rValue = Math.Round(rValue, 1);

            RhinoApp.WriteLine(Names.shift3 + Names.RaumAttributesEnum.RerfDIN.ToString() + " = " + la + " - " + k + " + 10 * log(" + ss + " / (0.8 * " + sg + ")) + 2 = " + rValue);

            Rerf_DIN = rValue;

        }

        public void SetMaxRErf()
        {
            double rerf_VDI_Tag = Rerf_VDI_Tag ?? 0;
            double rerf_VDI_Nacht = Rerf_VDI_Nacht ?? 0;
            double rerf_DIN = Rerf_DIN ?? 0;

            double[] r_erf_Array = { rerf_VDI_Tag, rerf_VDI_Nacht, rerf_DIN };
            Rerf_Max = r_erf_Array.Max();

            RhinoApp.WriteLine();
            RhinoApp.WriteLine(Names.shift2 + "- Ergebnis");
            RhinoApp.WriteLine();
            RhinoApp.WriteLine(Names.shift3 + Names.RaumAttributesEnum.RerfVDITag.ToString() + " = " + rerf_VDI_Tag);
            RhinoApp.WriteLine(Names.shift3 + Names.RaumAttributesEnum.RerfVDINacht.ToString() + " = " + rerf_VDI_Nacht);
            RhinoApp.WriteLine(Names.shift3 + Names.RaumAttributesEnum.RerfDIN.ToString() + " = " + rerf_DIN);
            RhinoApp.WriteLine(Names.shift3 + "=> rErf_Max = " + Rerf_Max);
        }

    }
}
