using Bauphysik.Helpers;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bauphysik.Data
{
    public class Referenzflaeche : RhinoElement
    {
        public Referenzflaeche(Guid objectGuid)
        {
            ObjectId = objectGuid;
        }

        public double FlaecheBrutto()
        {
            Brep brep = GetBrep();
            if (brep == null) return 0;

            AreaMassProperties massProp = AreaMassProperties.Compute(brep);
            if (massProp == null) return 0;

            return massProp.Area;
        }

        /// <summary>
        /// Area without openings 
        /// </summary>
        /// <param name="relatedInnenflaeche"></param>
        /// <returns></returns>
        public double FlaecheNetto(Innenflaeche relatedInnenflaeche)
        {
            double brutto = FlaecheBrutto();
            double openings = relatedInnenflaeche.Fassade == null ? 0 : relatedInnenflaeche.Fassade.FlaecheOeffnungen(relatedInnenflaeche);

            return brutto - openings;
        }

    }
}
