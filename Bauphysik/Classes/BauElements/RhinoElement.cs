using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bauphysik.Data
{
    public abstract class RhinoElement : IModelElement
    {
        public Guid ObjectGuid { get; set; }
        public string Typ { get; set; }


        public RhinoObject GetRhinoObject()
        {
            return RhinoDoc.ActiveDoc.Objects.Find(ObjectGuid);
        }

        public ObjRef GetObjRef()
        {
            return new ObjRef(RhinoDoc.ActiveDoc, ObjectGuid);
        }

        public Brep GetBrep()
        {
            RhinoObject rhObj = GetRhinoObject();
            if (rhObj == null) return null;

            ObjRef objRef = new ObjRef(rhObj);
            return objRef.Brep();
        }

        public void DeleteRhinoObject()
        {
            RhinoObject rhObj = GetRhinoObject();
            if (rhObj != null) RhinoDoc.ActiveDoc.Objects.Delete(rhObj);
        }

    }
}
