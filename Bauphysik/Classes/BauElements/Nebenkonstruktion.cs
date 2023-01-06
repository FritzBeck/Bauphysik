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
    public class Nebenkonstruktion : Konstruktion
    {
        public double? Flaeche;

        public Nebenkonstruktion(Guid objectGuid)
        {
            ObjectGuid = objectGuid;
        }

        public Nebenkonstruktion()
        {
        }

    }
}
