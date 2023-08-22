using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bauphysik.Data
{
    public class Bauteil
    {
        public Guid ObjectId;

        public string Name;

        public string Art;

        public double? Dicke;

        public Bauteil(Guid guid, string name)
        {
            ObjectId = guid;
            Name = name;
        }
    }
}
