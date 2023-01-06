using Rhino;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bauphysik.Helpers;

namespace Bauphysik.Data
{
    public abstract class Konstruktion : FassadenElement
    {

        public double? R_iw { get; set; }
        public double? UWert { get; set; }

        /// <summary>
        /// Calculate Reiw
        /// </summary>
        /// <param name="ss">Fassadenflaeche from related Raum<</param>
        /// <param name="relatedInnenflaeche">Underlying Innenflaeche</param>
        public void BerechneReiw37(double ss, Innenflaeche relatedInnenflaeche)
        {
            RhinoApp.WriteLine();
            if (this is Nebenkonstruktion) RhinoApp.WriteLine(Names.shift3 + "Nebenkonstruktion (Guid: " + ObjectGuid.ToString() + ")");
            if (this is Hauptkonstruktion) RhinoApp.WriteLine(Names.shift3 + "Hauptkonstruktion (Guid: " + ObjectGuid.ToString() + ")");

            if (RhinoHelpers.CheckIsNull(R_iw, Names.KonstruktionAttributeEnum.Riw.ToString(), this.ObjectGuid.ToString(), true)) return;
            
            double flaeche = 0;
            if (this is Nebenkonstruktion nkonstr)
            {
                if (RhinoHelpers.CheckIsNull(nkonstr.Flaeche, Names.FassadenElementAttributeEnum.Flaeche.ToString(), this.ObjectGuid.ToString(), true)) return;
                flaeche = nkonstr.Flaeche ?? 0;
            }

            if (this is Hauptkonstruktion hkonstr)
            {
                flaeche = hkonstr.Flaeche(relatedInnenflaeche);
            }

            if (flaeche <= 0)
            {
                RhinoApp.WriteLine(Names.shift + Names.warning + "Flaeche <= 0 " + Names.cancelCalc);
                return;
            }

            double r_iw = R_iw ?? 0;



            if (ss <= 0)
            {
                RhinoApp.WriteLine(Names.shift + Names.warning + "ss <= 0  " + Names.cancelCalc);
                return;
            }


            RhinoApp.WriteLine(Names.shift4 + "Flaeche = " + flaeche.ToString());
            RhinoApp.WriteLine(Names.shift4 + "Riw = " + r_iw.ToString());

            double r_eiw = r_iw + 10 * Math.Log10(ss / flaeche);

            Reiw = Math.Round(r_eiw, 3);

            RhinoApp.WriteLine(Names.shift4 + "R_e,i,w = " + R_iw + " + 10 * log10(" + ss + "/ " + flaeche + ") = " + Reiw);   ;
        }


    }
}
