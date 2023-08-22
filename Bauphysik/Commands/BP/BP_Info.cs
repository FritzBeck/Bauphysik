using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Input.Custom;
using System.Windows.Markup;
using System.Data;
using Bauphysik.Helpers;
using Bauphysik.Data;
using LayerManager;
using LayerManager.Parser;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Diagnostics;

namespace Bauphysik.Commands
{
    public class BP_Info : Command
    {

        public override string EnglishName => "BP_Info";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {

            RhinoApp.WriteLine("Starte Info...");

            try
            {
                Process.Start("https://sway.office.com/nYJSNyDEMzaMrss7?ref=Link");
            }
            catch
            {

            }

            RhinoApp.WriteLine("Info abgeschlossen");

            return Result.Success;
        }

    }
}
