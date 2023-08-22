using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bauphysik.Data
{
    public class Raumkategorie
    {
        public Guid ObjectId;

        public string Name;

        public double? K;

        public double? liTag;

        public double? liNacht;

        public Raumkategorie(Guid guid, string kategorie)
        {
            ObjectId = guid;
            Name = kategorie;
        }

    }
}
