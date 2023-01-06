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

namespace Bauphysik.Commands
{
    public class BP_GetFilepath : Command
    {

        public override string EnglishName => "BP_VerknüpfeTabellen";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {

            if (BauphysikPlugin.Instance.StringArray != null && BauphysikPlugin.Instance.StringArray.Count > 0)
            {
                string filepath = BauphysikPlugin.Instance.StringArray.Item(0);
                RhinoApp.WriteLine("Pfad: " + filepath);

                var go = new GetOption();
                go.SetCommandPrompt("Sollen andere Tabellen verknüpft werden?");
                go.AddOption("Ja");
                go.AddOption("Nein");
                go.Get();

                if (go.CommandResult() != Result.Success)
                    return go.CommandResult();

                if (go.Option().Index == 2)
                {
                    RhinoApp.WriteLine("Verknüpfe Tabellen abgebrochen.");
                    return Result.Success;
                }
            }

            RhinoApp.WriteLine("Starte Verknüpfe Tabellen...");

            // Configure open file dialog box
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "Document"; // Default file name
            dialog.DefaultExt = ".xml"; // Default file extension
            dialog.Filter = "XML file|*.xml"; // Filter files by extension

            // Process open file dialog box results
            if (dialog.ShowDialog() == true)
            {
                string filepath = dialog.FileName;

                XMLReader xMLReader = new XMLReader();
                RootDB data = xMLReader.ReadFile(filepath);

                if (data == null)
                {
                    RhinoApp.WriteLine(Helpers.Names.warning + " Datei kann nicht gelesen werden");
                    return Result.Failure;
                }

                string newFilepath = dialog.FileName;
                BauphysikPlugin.Instance.StringArray.Replace(0, newFilepath);

                RhinoApp.WriteLine("Pfad: " + newFilepath);
                RhinoApp.WriteLine("Tabellen wurden verknüpft");
                RhinoApp.WriteLine("Verknüpfe Tabellen abgeschlossen.");
            }

            return Result.Success;
        }

    }
}
