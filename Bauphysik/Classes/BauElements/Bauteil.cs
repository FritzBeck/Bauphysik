using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bauphysik.Data
{
    public class Bauteil
    {
        public Guid ObjectGuid;

        public string Name;

        public string Art;

        public double? Dicke;

        public Bauteil(Guid guid, string name)
        {
            ObjectGuid = guid;
            Name = name;
        }
    }
}
