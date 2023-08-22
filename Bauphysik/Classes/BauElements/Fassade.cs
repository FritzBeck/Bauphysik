using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bauphysik.Helpers;

namespace Bauphysik.Data
{
    public class Fassade : RhinoElement
    {
        public double? R_wges { get; set; }

        public bool? Vollverglast { get; set; }

        public List<Fenster> Fensters { get; set; }
        public List<Einbauteil> Einbauteils { get; set; }
        public Hauptkonstruktion Hauptkonstruktion { get; set; }
        public List<Nebenkonstruktion> Nebenkonstruktions { get; set; }

        public Fassade(Guid fassadeGuid)
        {
            ObjectId = fassadeGuid;

            Fensters = new List<Fenster>();
            Einbauteils = new List<Einbauteil>();
            Nebenkonstruktions = new List<Nebenkonstruktion>();
        }

        public double FlaecheOeffnungen(Innenflaeche relatedInnenflaeche)
        {
            if (Fensters == null || Fensters.Count == 0) return 0;

            double totalArea = 0;
            foreach (Fenster fenster in Fensters)
            {
                double flaeche = fenster.FlaecheGesamt();

                totalArea += flaeche;
            }

            return totalArea;
        }


        /// <summary>
        /// Calculate Direktschalldämmmaß R'_w,ges
        /// </summary>
        /// <param name="ss"></param>
        /// <param name="relatedInnenflaeche"></param>
        /// <param name="overWrite"></param>
        public void BerechneDirektSchall(double ss, Innenflaeche relatedInnenflaeche, bool overWrite)
        {
            if (this.R_wges != null && !overWrite) return;

            List<double> schalldaemmmassList = new List<double>();

            RhinoApp.WriteLine();
            RhinoApp.WriteLine(Names.shift2 + "- Berechne Reiw");

            if (Fensters != null && Fensters.Count > 0)
                foreach (Fenster fenster in Fensters)
                {
                    fenster.BerechneReiw37(ss);
                    schalldaemmmassList.Add(fenster.Reiw ?? 0);
                }

            if (Einbauteils != null && Einbauteils.Count > 0)
                foreach (Einbauteil einbauteil in Einbauteils)
                {
                    einbauteil.BerechneReiw38();
                    schalldaemmmassList.Add(einbauteil.Reiw ?? 0);
                }

            if (Nebenkonstruktions != null && Nebenkonstruktions.Count > 0)
                foreach (Nebenkonstruktion nebenkonstruktion in Nebenkonstruktions)
                {
                    nebenkonstruktion.BerechneReiw37(ss, relatedInnenflaeche);
                    schalldaemmmassList.Add(nebenkonstruktion.Reiw ?? 0);
                }

            if (Hauptkonstruktion != null)
            {
                Hauptkonstruktion.BerechneReiw37(ss, relatedInnenflaeche);
                schalldaemmmassList.Add(Hauptkonstruktion.Reiw ?? 0);
            }

            if (schalldaemmmassList == null || schalldaemmmassList.Count == 0)
            {
                RhinoApp.WriteLine();
                RhinoApp.WriteLine(Names.shift + Names.warning + "Keine Fassadenelement vorhanden" + Names.cancelCalc);
                return;
            }

            RhinoApp.WriteLine();
            RhinoApp.WriteLine(Names.shift2 + "- Zwischenrechnung");
            RhinoApp.WriteLine();
            List<double> intermediateList = new List<double>();
            foreach (double R_eiw in schalldaemmmassList)
            {
                if (R_eiw == 0)
                {
                    RhinoApp.WriteLine(Names.shift3 + "Reiw = 0  =>  keine Berechnung");
                    continue;
                }

                double intermediate = Math.Pow(10, -(R_eiw / 10));
                //interMediateSum += intermediate;

                intermediateList.Add(intermediate);
                RhinoApp.WriteLine(Names.shift3 + "10^ -" + R_eiw + "/10 = " + Math.Round(intermediate, 5));
            }

            if (intermediateList == null || intermediateList.Count == 0)
            {
                RhinoApp.WriteLine();
                RhinoApp.WriteLine(Names.shift + Names.warning + "Keine Reiw vorhanden" + Names.cancelCalc);
                return;
            }

            RhinoApp.Write(Names.shift3);
            double interMediateSum = 0;
            for (int i = 0; i < intermediateList.Count; i++)
            {
                interMediateSum += intermediateList[i];
                RhinoApp.Write(intermediateList[i].ToString());
                if (i < intermediateList.Count - 1) RhinoApp.Write(" + ");
            }
            RhinoApp.WriteLine(" = " + interMediateSum);
            RhinoApp.WriteLine();

            double Rwges = -10 * Math.Log10(interMediateSum);
            this.R_wges = Math.Round(Rwges, 2);

            RhinoApp.WriteLine(Names.shift2 + "- Ergebnis");
            RhinoApp.WriteLine();
            RhinoApp.WriteLine(Names.shift3 + "R'_w,ges = -10 * log(" + interMediateSum + ") = " + R_wges);

        }

        public void UpdateAbstractView(Innenflaeche relatedInnenflaeche)
        {

            //move other Facadeelements           
            double lengthMovement = 0;

            //Fenster
            if (Fensters != null && Fensters.Count > 0)
                foreach (Fenster fenster in Fensters)
                    fenster.UpdateAbstractView(relatedInnenflaeche, ref lengthMovement);


            //Nebenkonstruktion
            if (Nebenkonstruktions != null && Nebenkonstruktions.Count > 0)
                foreach (Nebenkonstruktion nebenkonstruktion in Nebenkonstruktions)
                    nebenkonstruktion.UpdateAbstractView(relatedInnenflaeche, ref lengthMovement);


            //Hauptkonstruktion
            if (Hauptkonstruktion == null)
                Hauptkonstruktion = new Hauptkonstruktion(Guid.Empty);
            
            Hauptkonstruktion.UpdateAbstractView(relatedInnenflaeche, ref lengthMovement);


            double lengthMovement2 = 0.05;

            //Einbauteile
            if (Einbauteils != null && Einbauteils.Count > 0)
                foreach (Einbauteil einbauteil in Einbauteils)
                    einbauteil.UpdateAbstractView(relatedInnenflaeche, ref lengthMovement2);

            RhinoDoc.ActiveDoc.Views.Redraw();

        }
        public void UpdateConcreteView(Innenflaeche relatedInnenflaeche)
        {

            //Fenster
            if (Fensters != null && Fensters.Count > 0)
                foreach (Fenster fenster in Fensters)
                    fenster.UpdateConcreteView(relatedInnenflaeche);


            //Nebenkonstruktion
            if (Nebenkonstruktions != null && Nebenkonstruktions.Count > 0)
                foreach (Nebenkonstruktion nebenkonstruktion in Nebenkonstruktions)
                    nebenkonstruktion.UpdateConcreteView(relatedInnenflaeche);


            //Hauptkonstruktion
            if (Hauptkonstruktion == null)
                Hauptkonstruktion = new Hauptkonstruktion(Guid.Empty);

            Hauptkonstruktion.UpdateConcreteView(relatedInnenflaeche);


            //Einbauteile
            if (Einbauteils != null && Einbauteils.Count > 0)
                foreach (Einbauteil einbauteil in Einbauteils)
                    einbauteil.UpdateConcreteView(relatedInnenflaeche);

        }


    }
}
