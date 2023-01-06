using Rhino;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bauphysik.Helpers;

namespace Bauphysik.Data
{
    public class Einbauteil : FassadenElement
    {

        public int? Anzahl;

        public double? D_newlab;

        public double? L_lab;

        public double? L_situ;



        public Einbauteil(Guid objectGuid)
        {
            ObjectGuid = objectGuid;
        }

        public Einbauteil()
        {
        }

        /// <summary>
        /// Calculate Reiw
        /// </summary>
        public void BerechneReiw38()
        {
            RhinoApp.WriteLine();
            RhinoApp.WriteLine(Names.shift + "Berechne Reiw38:");
            RhinoApp.WriteLine(Names.shift2 + "Einbauteil");
            RhinoApp.WriteLine(Names.shift3 + "Guid: " + ObjectGuid.ToString());


            if (RhinoHelpers.CheckIsNull(Anzahl, Names.EinbauteilAttributeEnum.Anzahl.ToString(), this.ObjectGuid.ToString(), true)) return;
            if (RhinoHelpers.CheckIsNull(L_situ, Names.EinbauteilAttributeEnum.lSitu.ToString(), this.ObjectGuid.ToString(), true)) return;
            if (RhinoHelpers.CheckIsNull(L_lab, Names.EinbauteilAttributeEnum.lLab.ToString(), this.ObjectGuid.ToString(), true)) return;
            if (RhinoHelpers.CheckIsNull(D_newlab, Names.EinbauteilAttributeEnum.DnewLab.ToString(), this.ObjectGuid.ToString(), true)) return;

            double anzahl = Anzahl ?? 0;
            double l_situ = L_situ ?? 0;
            double l_lab = L_lab ?? 0;
            double d_newlab = D_newlab ?? 0;

            RhinoApp.WriteLine(Names.shift3 + "Anzahl: " + anzahl.ToString());
            RhinoApp.WriteLine(Names.shift3 + "Lsitu: " + l_situ.ToString());
            RhinoApp.WriteLine(Names.shift3 + "Llab: " + l_lab.ToString());
            RhinoApp.WriteLine(Names.shift3 + "DnewLab: " + d_newlab.ToString());


            double Reiw = d_newlab + 10 * Math.Log10(anzahl * l_situ / l_lab);

            this.Reiw = Math.Round(Reiw, 2);
            D_newlab = Math.Round(d_newlab, 2);
            L_situ = Math.Round(l_situ, 2);
            L_lab = Math.Round(l_lab, 2);

            RhinoApp.WriteLine(Names.shift2 + "R_e,i,w = " + D_newlab + " + 10 * log10(" + Anzahl + " * " + L_situ + "/ " + L_lab + ") =" + Reiw);
        }

    }
}
