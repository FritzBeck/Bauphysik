using Bauphysik.Helpers;
using Rhino;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using System.Data;

namespace Bauphysik.Data
{
    public class BauModel
    {
        public List<Innenflaeche> Innenflaechen { get; set; }
        public List<Raum> Raume { get; set; }

        public List<Raumkategorie> Raumkategorien { get; set; }
        public List<Bauteil> Bauteile { get; set; }

        public BauModel()
        {
            this.Innenflaechen = new List<Innenflaeche>();
            this.Raume = new List<Raum>();
            this.Bauteile = new List<Bauteil>();
            this.Raumkategorien = new List<Raumkategorie>();
        }

        /// <summary>
        /// Get Innenflaeche by Guid
        /// </summary>
        /// <param name="objectId"></param>
        /// <returns></returns>
        public Innenflaeche FindInnenflaeche(Guid objectId)
        {
            foreach (Innenflaeche innenflaeche in Innenflaechen)
                if (innenflaeche.ObjectGuid == objectId) 
                    return innenflaeche;

            return null;
        }

    }
}
