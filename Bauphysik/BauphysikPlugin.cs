using LayerManager;
using Rhino;
using Rhino.FileIO;
using Rhino.PlugIns;
using System;
using System.Windows.Controls.Primitives;
using RhinoWindows.Controls;
using LayerManager.Parser;

namespace Bauphysik
{
    ///<summary>
    /// <para>Every RhinoCommon .rhp assembly must have one and only one PlugIn-derived
    /// class. DO NOT create instances of this class yourself. It is the
    /// responsibility of Rhino to create an instance of this class.</para>
    /// <para>To complete plug-in information, please also see all PlugInDescription
    /// attributes in AssemblyInfo.cs (you might need to click "Project" ->
    /// "Show All Files" to see it in the "Solution Explorer" window).</para>
    ///</summary>
    public class BauphysikPlugin : Rhino.PlugIns.PlugIn
    {
        private const int MAJOR = 1;
        private const int MINOR = 0;

        public DocumentData StringArray { get; private set; }

        public BauphysikPlugin()
        {
            Instance = this;
            StringArray = new DocumentData();
        }

        ///<summary>Gets the only instance of the BauphysikPlugin plug-in.</summary>
        public static BauphysikPlugin Instance { get; private set; }

        // You can override methods here to change the plug-in behavior on
        // loading and shut down, add options pages to the Rhino _Option command
        // and maintain plug-in wide options in a document.
        ///<summary>Gets the only instance of the LayerManagerPanelsPlugin plug-in.</summary>

        /// <summary>
        /// Called when the plug-in is being loaded.
        /// </summary>
        protected override LoadReturnCode OnLoad(ref string errorMessage)
        {
            // Add an event handler so we know when documents are closed.
            RhinoDoc.CloseDocument += OnCloseDocument;

            return LoadReturnCode.Success;
        }

        /// <summary>
        /// OnCloseDocument event handler.
        /// </summary>
        private void OnCloseDocument(object sender, DocumentEventArgs e)
        {
            // When the document is closed, clear our 
            // document user data containers.
            StringArray.Clear();
        }

        /// <summary>
        /// Called whenever a Rhino is about to save a .3dm file.  If you want to save
        //  plug-in document data when a model is saved in a version 5 .3dm file, then
        //  you must override this function to return true and you must override WriteDocument().
        /// </summary>
        protected override bool ShouldCallWriteDocument(FileWriteOptions options)
        {
            return !options.WriteGeometryOnly && !options.WriteSelectedObjectsOnly;
        }

        /// <summary>
        /// Called when Rhino is saving a .3dm file to allow the plug-in to save document user data.
        /// </summary>
        protected override void WriteDocument(RhinoDoc doc, BinaryArchiveWriter archive, FileWriteOptions options)
        {
            // Write the version of our document data
            archive.Write3dmChunkVersion(MAJOR, MINOR);
            // Write the string table
            StringArray.WriteDocument(archive);
        }

        /// <summary>
        /// Called whenever a Rhino document is being loaded and plug-in user data was
        /// encountered written by a plug-in with this plug-in's GUID.
        /// </summary>
        protected override void ReadDocument(RhinoDoc doc, BinaryArchiveReader archive, FileReadOptions options)
        {
            string pathUserData = null;
            //string pathUserData2;

            archive.Read3dmChunkVersion(out var major, out var minor);
            if (MAJOR == major && MINOR == minor)
            {
                // Always read user data even though you might not use it.
                RhinoApp.WriteLine("Bauphysik PlugIn");
                RhinoApp.WriteLine("Starte Verknüpfe Tabellen...");

                var string_table = new DocumentData();
                string_table.ReadDocument(archive);

                if (!options.ImportMode && !options.ImportReferenceMode)
                {
                    if (string_table.Count > 0)
                    {
                        StringArray.AddRange(string_table.ToArray());

                        try { pathUserData = StringArray.Item(0); }
                        catch { }

                        if (!string.IsNullOrWhiteSpace(pathUserData))
                        {
                            string filepath = this.StringArray.Item(0);
                            if (string.IsNullOrWhiteSpace(filepath)) return;

                            RhinoApp.WriteLine("Pfad: " + filepath);

                            XMLReader xMLReader = new XMLReader();
                            RootDB data = xMLReader.ReadFile(filepath);

                            if (data == null)
                            {
                                RhinoApp.WriteLine("Fehler beim Laden der Tabellen: " + pathUserData);
                                RhinoApp.WriteLine("Mögliche Fehlerquellen:");
                                RhinoApp.WriteLine("Ist der Pfad fehlerhaft? Ist Tabelle geöffnet? Ist die Struktur der Tabelle fehlerhaft?");
                            }
                            else
                            {
                                RhinoApp.WriteLine("Tabellen wurden verknüpft");
                                RhinoApp.WriteLine("Verknüpfe Tabellen abgeschlossen.");
                            }


                        }
                        else
                            RhinoApp.WriteLine("Datei kann nicht gelesen werden");
                    }
                    else
                        RhinoApp.WriteLine("Keine Tabellen verknüpft.");


                }
            }
        }


    }
}