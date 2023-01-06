using Bauphysik.Data;
using Bauphysik.Helpers;
using Rhino;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bauphysik.Data
{
    public class Hauptkonstruktion : Konstruktion
    {

        public Hauptkonstruktion(Guid objectGuid)
        {
            ObjectGuid = objectGuid;
        }

        /// <summary>
        /// Residual area after substraction of area of other Fassadenelemente from area of Innenflaeche
        /// </summary>
        /// <param name="innenflaeche"></param>
        /// <returns></returns>
        public double Flaeche(Innenflaeche innenflaeche)
        {
            double totalArea = innenflaeche.FlaecheBrutto();

            if (innenflaeche.Fassade != null)
            {
                if (innenflaeche.Fassade.Fensters != null && innenflaeche.Fassade.Fensters.Count > 0)
                    foreach (Fenster fenster in innenflaeche.Fassade.Fensters)
                        totalArea -= fenster.FlaecheGesamt();

                if (innenflaeche.Fassade.Nebenkonstruktions != null && innenflaeche.Fassade.Nebenkonstruktions.Count() > 0)
                    foreach (Nebenkonstruktion konstr in innenflaeche.Fassade.Nebenkonstruktions)
                        totalArea -= konstr.Flaeche ?? 0;
            }
            
            return totalArea;
        }

    }
}
