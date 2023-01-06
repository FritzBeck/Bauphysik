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

namespace Bauphysik.Commands
{
    public class BP_CreateTableCmd : Command
    {

        public override string EnglishName => "BP_ErstelleTabelle";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {

            RootDB data = new RootDB();

            Category Allgemein = new Category("Allgemein");
            Category Geometry = new Category("Geometry");
            Category Schallschutz = new Category("Schallschutz");
            Category Relationen = new Category("Relationen");

            data.Categories.Add(Allgemein);
            data.Categories.Add(Geometry);
            data.Categories.Add(Schallschutz);
            data.Categories.Add(Relationen);

            ValueTableDB Flaeche = new ValueTableDB("Flaeche", Guid.NewGuid());
            ValueTableDB Durchmesser = new ValueTableDB("Durchmesser", Guid.NewGuid());
            ValueTableDB Anzahl = new ValueTableDB("Anzahl", Guid.NewGuid());
            ValueTableDB Breite = new ValueTableDB("Breite", Guid.NewGuid());
            ValueTableDB Hoehe = new ValueTableDB("Hoehe", Guid.NewGuid());
            ValueTableDB RerfMax = new ValueTableDB("RerfMax", Guid.NewGuid());
            ValueTableDB Verschiebung = new ValueTableDB("Verschiebung", Guid.NewGuid());
            ValueTableDB Dicke = new ValueTableDB("Dicke", Guid.NewGuid());
            ValueTableDB lSitu = new ValueTableDB("lSitu", Guid.NewGuid());
            ValueTableDB lLab = new ValueTableDB("lLab", Guid.NewGuid());
            ValueTableDB DnewLab = new ValueTableDB("DnewLab", Guid.NewGuid());
            ValueTableDB gWert = new ValueTableDB("gWert", Guid.NewGuid());
            ValueTableDB UWert = new ValueTableDB("UWert", Guid.NewGuid());
            ValueTableDB Riw = new ValueTableDB("Riw", Guid.NewGuid());
            ValueTableDB Reiw = new ValueTableDB("Reiw", Guid.NewGuid());
            ValueTableDB Rwges = new ValueTableDB("Rwges", Guid.NewGuid());
            ValueTableDB Bruttoflaeche = new ValueTableDB("Bruttoflaeche", Guid.NewGuid());
            ValueTableDB Nettoflaeche = new ValueTableDB("Nettoflaeche", Guid.NewGuid());
            ValueTableDB ss = new ValueTableDB("ss", Guid.NewGuid());
            ValueTableDB sg = new ValueTableDB("sg", Guid.NewGuid());
            ValueTableDB RerfVDINacht = new ValueTableDB("RerfVDINacht", Guid.NewGuid());
            ValueTableDB RerfVDITag = new ValueTableDB("RerfVDITag", Guid.NewGuid());
            ValueTableDB RerfDIN = new ValueTableDB("RerfDIN", Guid.NewGuid());
            ValueTableDB VDIGewerbeNacht = new ValueTableDB("VDIGewerbeNacht", Guid.NewGuid());
            ValueTableDB VDIGewerbeTag = new ValueTableDB("VDIGewerbeTag", Guid.NewGuid());
            ValueTableDB VDIVerkehrNacht = new ValueTableDB("VDIVerkehrNacht", Guid.NewGuid());
            ValueTableDB VDIVerkehrTag = new ValueTableDB("VDIVerkehrTag", Guid.NewGuid());
            ValueTableDB DINLaermpegel = new ValueTableDB("DINLaermpegel", Guid.NewGuid());
            ValueTableDB Neigung = new ValueTableDB("Neigung", Guid.NewGuid());

            data.TableDBs.Add(Flaeche);
            data.TableDBs.Add(Durchmesser);
            data.TableDBs.Add(Anzahl);
            data.TableDBs.Add(Breite);
            data.TableDBs.Add(Hoehe);
            data.TableDBs.Add(RerfMax);
            data.TableDBs.Add(Verschiebung);
            data.TableDBs.Add(Dicke);
            data.TableDBs.Add(lLab);
            data.TableDBs.Add(DnewLab);
            data.TableDBs.Add(gWert);
            data.TableDBs.Add(UWert);
            data.TableDBs.Add(Reiw);
            data.TableDBs.Add(Riw);
            data.TableDBs.Add(Rwges);
            data.TableDBs.Add(Bruttoflaeche);
            data.TableDBs.Add(Nettoflaeche);
            data.TableDBs.Add(RerfVDINacht);
            data.TableDBs.Add(RerfVDITag);
            data.TableDBs.Add(RerfDIN);
            data.TableDBs.Add(VDIGewerbeNacht);
            data.TableDBs.Add(VDIGewerbeTag);
            data.TableDBs.Add(VDIVerkehrNacht);
            data.TableDBs.Add(VDIVerkehrTag);
            data.TableDBs.Add(DINLaermpegel);
            data.TableDBs.Add(Neigung);


            AttrTableDB Bauteilart = new AttrTableDB("Bauteilart", Guid.NewGuid());
            AttrTableDB Geschoss = new AttrTableDB("Geschoss", Guid.NewGuid());
            AttrTableDB Raumkategorie = new AttrTableDB("Raumkategorie", Guid.NewGuid());
            
            AttrTableDB Zone = new AttrTableDB("Zone", Guid.NewGuid());
            AttrTableDB Richtung = new AttrTableDB("Richtung", Guid.NewGuid());
            Richtung.Table.Columns[3].ExtendedProperties["DataType"] = typeof(int);
            Richtung.AddRow(Guid.NewGuid(), "1");
            Richtung.AddRow(Guid.NewGuid(), "-1");

            AttrTableDB Orientierung = new AttrTableDB("Orientierung", Guid.NewGuid());
            Orientierung.AddRow(Guid.NewGuid(), "Nord");
            Orientierung.AddRow(Guid.NewGuid(), "N-W");
            Orientierung.AddRow(Guid.NewGuid(), "West");
            Orientierung.AddRow(Guid.NewGuid(), "S-W");
            Orientierung.AddRow(Guid.NewGuid(), "Süd");
            Orientierung.AddRow(Guid.NewGuid(), "S-O");
            Orientierung.AddRow(Guid.NewGuid(), "Ost");
            Orientierung.AddRow(Guid.NewGuid(), "N-O");

            AttrTableDB Fassadenflaeche = new AttrTableDB("Fassadenflaeche", Guid.NewGuid());
            Fassadenflaeche.Table.Columns[3].ExtendedProperties["DataType"] = typeof(bool);
            Fassadenflaeche.AddRow(Guid.NewGuid(), "Ja");
            Fassadenflaeche.AddRow(Guid.NewGuid(), "Nein");
            AttrTableDB Grundflaeche = new AttrTableDB("Grundflaeche", Guid.NewGuid());
            Grundflaeche.Table.Columns[3].ExtendedProperties["DataType"] = typeof(bool);
            Grundflaeche.AddRow(Guid.NewGuid(), "Ja");
            Grundflaeche.AddRow(Guid.NewGuid(), "Nein");


            data.TableDBs.Add(Bauteilart);
            data.TableDBs.Add(Geschoss);
            data.TableDBs.Add(Raumkategorie);
            data.TableDBs.Add(Zone);
            data.TableDBs.Add(Richtung);
            data.TableDBs.Add(Orientierung);
            data.TableDBs.Add(Fassadenflaeche);
            data.TableDBs.Add(Grundflaeche);


            AbstrTableDB Bauteil = new AbstrTableDB("Bauteil", Guid.NewGuid());
            Bauteil.AddColumn("Bauteilart", Bauteilart.Id, false, typeof(string), Allgemein.Id);
            Bauteil.AddColumn("Dicke", Dicke.Id, false, typeof(string), Geometry.Id);

            AbstrTableDB Raum = new AbstrTableDB("Raum", Guid.NewGuid());
            Raum.AddColumn(Geschoss.Name, Geschoss.Id, false, typeof(string), Allgemein.Id);
            Raum.AddColumn(Raumkategorie.Name, Raumkategorie.Id, false, typeof(string), Allgemein.Id);
            Raum.AddColumn(DINLaermpegel.Name, DINLaermpegel.Id, false, typeof(double), Schallschutz.Id);
            Raum.AddColumn(VDIVerkehrTag.Name, VDIVerkehrTag.Id, false, typeof(double), Schallschutz.Id);
            Raum.AddColumn(VDIVerkehrNacht.Name, VDIVerkehrNacht.Id, false, typeof(double), Schallschutz.Id);
            Raum.AddColumn(VDIGewerbeTag.Name, VDIGewerbeTag.Id, false, typeof(double), Schallschutz.Id);
            Raum.AddColumn(VDIGewerbeNacht.Name, VDIGewerbeNacht.Id, false, typeof(double), Schallschutz.Id);
            Raum.AddColumn(RerfDIN.Name, RerfDIN.Id, false, typeof(double), Schallschutz.Id);
            Raum.AddColumn(RerfVDITag.Name, RerfVDITag.Id, false, typeof(double), Schallschutz.Id);
            Raum.AddColumn(RerfVDINacht.Name, RerfVDINacht.Id, false, typeof(double), Schallschutz.Id);
            Raum.AddColumn(sg.Name, sg.Id, false, typeof(double), Schallschutz.Id);
            Raum.AddColumn(ss.Name, ss.Id, false, typeof(double), Schallschutz.Id);
            Raum.AddColumn(RerfMax.Name, RerfMax.Id, false, typeof(double), Schallschutz.Id);

            data.TableDBs.Add(Bauteil);
            data.TableDBs.Add(Raum);

            RhinoTableDB Referenzflaeche = new RhinoTableDB("Referenzflaeche", Guid.NewGuid());


            RhinoTableDB Hauptkonstruktion = new RhinoTableDB("Hauptkonstruktion", Guid.NewGuid());
            Hauptkonstruktion.AddColumn(Reiw.Name, Reiw.Id, false, typeof(double), Schallschutz.Id);
            Hauptkonstruktion.AddColumn(Riw.Name, Riw.Id, false, typeof(double), Schallschutz.Id);
            Hauptkonstruktion.AddColumn(Flaeche.Name, Flaeche.Id, false, typeof(double), Schallschutz.Id);

            RhinoTableDB Nebenkonstruktion = new RhinoTableDB("Nebenkonstruktion", Guid.NewGuid());
            Nebenkonstruktion.AddColumn(Reiw.Name, Reiw.Id, false, typeof(double), Schallschutz.Id);
            Nebenkonstruktion.AddColumn(Riw.Name, Riw.Id, false, typeof(double), Schallschutz.Id);
            Nebenkonstruktion.AddColumn(Flaeche.Name, Flaeche.Id, false, typeof(double), Schallschutz.Id);

            RhinoTableDB Einbauteil = new RhinoTableDB("Einbauteil", Guid.NewGuid());
            Einbauteil.AddColumn(Reiw.Name, Reiw.Id, false, typeof(double), Schallschutz.Id);
            Einbauteil.AddColumn(DnewLab.Name, DnewLab.Id, false, typeof(double), Schallschutz.Id);
            Einbauteil.AddColumn(lSitu.Name, lSitu.Id, false, typeof(double), Schallschutz.Id);
            Einbauteil.AddColumn(lLab.Name, lLab.Id, false, typeof(double), Schallschutz.Id);

            RhinoTableDB Fenster = new RhinoTableDB("Fenster", Guid.NewGuid());
            Fenster.AddColumn(Reiw.Name, Reiw.Id, false, typeof(double), Schallschutz.Id);
            Fenster.AddColumn(Riw.Name, Riw.Id, false, typeof(double), Schallschutz.Id);
            Fenster.AddColumn(UWert.Name, UWert.Id, false, typeof(double), Schallschutz.Id);
            Fenster.AddColumn(gWert.Name, gWert.Id, false, typeof(double), Schallschutz.Id);
            Fenster.AddColumn(Hoehe.Name, Hoehe.Id, false, typeof(double), Geometry.Id);
            Fenster.AddColumn(Breite.Name, Breite.Id, false, typeof(double), Geometry.Id);
            Fenster.AddColumn(Durchmesser.Name, Durchmesser.Id, false, typeof(double), Geometry.Id);
            Fenster.AddColumn(Anzahl.Name, Anzahl.Id, false, typeof(double), Geometry.Id);
            Fenster.AddColumn(Flaeche.Name, Flaeche.Id, false, typeof(double), Geometry.Id);


            RhinoTableDB Innenflaeche = new RhinoTableDB("Innenflaeche", Guid.NewGuid());
            Innenflaeche.AddColumn(Bauteil.Name, Bauteil.Id, false, typeof(string), Allgemein.Id);
            Innenflaeche.AddColumn(Neigung.Name, Neigung.Id, false, typeof(double), Allgemein.Id);
            Innenflaeche.AddColumn(Grundflaeche.Name, Grundflaeche.Id, false, typeof(string), Allgemein.Id);
            Innenflaeche.AddColumn(Fassadenflaeche.Name, Fassadenflaeche.Id, false, typeof(string), Allgemein.Id);
            Innenflaeche.AddColumn(Raum.Name, Raum.Id, false, typeof(string), Schallschutz.Id);
            Innenflaeche.AddColumn(Orientierung.Name, Orientierung.Id, false, typeof(string), Allgemein.Id);
            Innenflaeche.AddColumn(Nettoflaeche.Name, Nettoflaeche.Id, false, typeof(double), Geometry.Id);
            Innenflaeche.AddColumn(Bruttoflaeche.Name, Bruttoflaeche.Id, false, typeof(double), Geometry.Id);
            Innenflaeche.AddColumn(Zone.Name, Zone.Id, false, typeof(string), Allgemein.Id);
            Innenflaeche.AddColumn(Richtung.Name, Richtung.Id, false, typeof(string), Geometry.Id);
            Innenflaeche.AddColumn(Verschiebung.Name, Verschiebung.Id, false, typeof(double), Geometry.Id);
            Innenflaeche.AddColumn(Rwges.Name, Rwges.Id, false, typeof(double), Schallschutz.Id);
            Innenflaeche.AddColumn(Referenzflaeche.Name, Referenzflaeche.Id, false, typeof(Guid), Relationen.Id);
            Innenflaeche.AddColumn(Fenster.Name, Fenster.Id, true, typeof(Guid), Relationen.Id);
            Innenflaeche.AddColumn(Einbauteil.Name, Einbauteil.Id, true, typeof(Guid), Relationen.Id);
            Innenflaeche.AddColumn(Nebenkonstruktion.Name, Nebenkonstruktion.Id, true, typeof(Guid), Relationen.Id);
            Innenflaeche.AddColumn(Hauptkonstruktion.Name, Hauptkonstruktion.Id, false, typeof(Guid), Relationen.Id);


            data.TableDBs.Add(Referenzflaeche);
            data.TableDBs.Add(Hauptkonstruktion);
            data.TableDBs.Add(Nebenkonstruktion);
            data.TableDBs.Add(Einbauteil);
            data.TableDBs.Add(Fenster);
            data.TableDBs.Add(Innenflaeche);

            // Configure open file dialog box
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.FileName = "Document"; // Default file name
            dialog.DefaultExt = ".xml"; // Default file extension
            dialog.Filter = "XML file|*.xml"; // Filter files by extension

            // Process open file dialog box results
            if (dialog.ShowDialog() == true)
            {
                string filepath = dialog.FileName;

                XMLWriter xmlWriter = new XMLWriter();
                xmlWriter.WriteFile(data, filepath);
            }

            return Result.Success;
        }

    }
}
