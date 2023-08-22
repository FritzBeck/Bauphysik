using Bauphysik.Data;
using Bauphysik.Helpers;
using LayerManager.Base;
using LayerManager.Data;
using LayerManager.Parser;
using Rhino;
using Rhino.Commands;
using System;

namespace Bauphysik.Commands
{
    public class BP_CreateTableCmd : Command
    {

        public override string EnglishName => "BP_ErstelleTabelle";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // Create Or Get LamaBase
            LamaMainParser lmMainParser = new LamaMainParser();
            LamaBase lmBase = lmMainParser.ReadLama();
            if (lmBase == null)
            {
                RhinoApp.WriteLine("Lama konnte nicht gelesen werden.");
                return Result.Failure;
            }

            LamaModel lmModel = lmBase.NewLamaModel();
            lmModel.Name = "Bauphysik";
            lmBase.Models.Add(lmModel);

            LamaProject lmProject = lmModel.NewLamaProject();
            lmProject.Name = "ProjektVorlage";
            lmModel.Projects.Add(lmProject);

            lmMainParser.WriteLama(lmBase);

            // Start message
            RhinoApp.WriteLine("Starte Erstelle Tabellen...");

            // Create LamaData
            LamaData lmData = lmProject.NewLamaData();

            // Add Categories
            LamaCategory Allgemein = new LamaCategory("Allgemein");
            lmData.LamaCategories.Add(Allgemein);
            
            LamaCategory Geometrie = new LamaCategory("Geometrie");
            lmData.LamaCategories.Add(Geometrie);
            
            LamaCategory Schallschutz = new LamaCategory("Schallschutz");
            lmData.LamaCategories.Add(Schallschutz);
            
            LamaCategory Relationen = new LamaCategory("Relationen");
            lmData.LamaCategories.Add(Relationen);

            // Add number tables
            NumberTable Anzahl = new NumberTable(Names.FensterAttributeEnum.Anzahl.ToString());
            lmData.LamaTables.Add(Anzahl);

            NumberTable Verschiebung = new NumberTable(Names.InnenflaecheAttributeEnum.Verschiebung.ToString());
            lmData.LamaTables.Add(Verschiebung);

            NumberTable Neigung = new NumberTable(Names.InnenflaecheAttributeEnum.Neigung.ToString());
            lmData.LamaTables.Add(Neigung);

            NumberTable Bruttoflaeche = new NumberTable(Names.InnenflaecheAttributeEnum.Bruttoflaeche.ToString());
            lmData.LamaTables.Add(Bruttoflaeche);

            NumberTable Nettoflaeche = new NumberTable(Names.InnenflaecheAttributeEnum.Nettoflaeche.ToString());
            lmData.LamaTables.Add(Nettoflaeche);

            NumberTable Dicke = new NumberTable(Names.BauteilAttributeEnum.Dicke.ToString());
            lmData.LamaTables.Add(Dicke);
            
            NumberTable lSitu = new NumberTable(Names.EinbauteilAttributeEnum.lSitu.ToString());
            lmData.LamaTables.Add(lSitu);
            
            NumberTable lLab = new NumberTable(Names.EinbauteilAttributeEnum.lLab.ToString());
            lmData.LamaTables.Add(lLab);
            
            NumberTable DnewLab = new NumberTable(Names.EinbauteilAttributeEnum.DnewLab.ToString());
            lmData.LamaTables.Add(DnewLab);

            NumberTable Durchmesser = new NumberTable(Names.FensterAttributeEnum.Durchmesser.ToString());
            lmData.LamaTables.Add(Durchmesser);

            NumberTable Breite = new NumberTable(Names.FensterAttributeEnum.Breite.ToString());
            lmData.LamaTables.Add(Breite);

            NumberTable Hoehe = new NumberTable(Names.FensterAttributeEnum.Hoehe.ToString());
            lmData.LamaTables.Add(Hoehe);

            NumberTable gWert = new NumberTable(Names.FensterAttributeEnum.gWert.ToString());
            lmData.LamaTables.Add(gWert);
            
            NumberTable UWert = new NumberTable(Names.FensterAttributeEnum.UWert.ToString());
            lmData.LamaTables.Add(UWert);

            NumberTable Flaeche = new NumberTable(Names.FassadenElementAttributeEnum.Flaeche.ToString());
            lmData.LamaTables.Add(Flaeche);

            NumberTable Reiw = new NumberTable(Names.FassadenElementAttributeEnum.Reiw.ToString());
            lmData.LamaTables.Add(Reiw);
            
            NumberTable Riw = new NumberTable(Names.KonstruktionAttributeEnum.Riw.ToString());
            lmData.LamaTables.Add(Riw);
            
            NumberTable Rwges = new NumberTable(Names.FassadeAttributeEnum.Rwges.ToString());
            lmData.LamaTables.Add(Rwges);

            NumberTable RerfMax = new NumberTable(Names.RaumAttributesEnum.RerfMax.ToString());
            lmData.LamaTables.Add(RerfMax);

            NumberTable ss = new NumberTable(Names.RaumAttributesEnum.ss.ToString());
            lmData.LamaTables.Add(ss);

            NumberTable sg = new NumberTable(Names.RaumAttributesEnum.sg.ToString());
            lmData.LamaTables.Add(sg);

            NumberTable RerfVDINacht = new NumberTable(Names.RaumAttributesEnum.RerfVDINacht.ToString());
            lmData.LamaTables.Add(RerfVDINacht);
            
            NumberTable RerfVDITag = new NumberTable(Names.RaumAttributesEnum.RerfVDITag.ToString());
            lmData.LamaTables.Add(RerfVDITag);
            
            NumberTable RerfDIN = new NumberTable(Names.RaumAttributesEnum.RerfDIN.ToString());
            lmData.LamaTables.Add(RerfDIN);
            
            NumberTable VDIGewerbeNacht = new NumberTable(Names.RaumAttributesEnum.VDIGewerbeNacht.ToString());
            lmData.LamaTables.Add(VDIGewerbeNacht);
            
            NumberTable VDIGewerbeTag = new NumberTable(Names.RaumAttributesEnum.VDIGewerbeTag.ToString());
            lmData.LamaTables.Add(VDIGewerbeTag);
            
            NumberTable VDIVerkehrNacht = new NumberTable(Names.RaumAttributesEnum.VDIVerkehrNacht.ToString());
            lmData.LamaTables.Add(VDIVerkehrNacht);
            
            NumberTable VDIVerkehrTag = new NumberTable(Names.RaumAttributesEnum.VDIVerkehrTag.ToString());
            lmData.LamaTables.Add(VDIVerkehrTag);
            
            NumberTable DINLaermpegel = new NumberTable(Names.RaumAttributesEnum.DINLaermpegel.ToString());
            lmData.LamaTables.Add(DINLaermpegel);
            
            // Add DropDown Tables
            DropdownTable Bauteilart = new DropdownTable(Names.BauteilAttributeEnum.Bauteilart.ToString());
            Bauteilart.AddLayerRow(Names.BauteilArtEnum.D.ToString());
            Bauteilart.AddLayerRow(Names.BauteilArtEnum.IW.ToString());
            Bauteilart.AddLayerRow(Names.BauteilArtEnum.AW.ToString());
            lmData.LamaTables.Add(Bauteilart);

            DropdownTable Geschoss = new DropdownTable(Names.InnenflaecheAttributeEnum.Geschoss.ToString());
            lmData.LamaTables.Add(Geschoss);

            DropdownTable Raumkategorie = new DropdownTable(Names.TabellenNameEnum.Raumkategorie.ToString());
            Raumkategorie.AddLeafColumn(Names.RaumkategorieEnum.KWert.ToString(), typeof(double), Allgemein.Id, false);
            Raumkategorie.AddLeafColumn(Names.RaumkategorieEnum.InnenTag.ToString(), typeof(double), Allgemein.Id, false);
            Raumkategorie.AddLeafColumn(Names.RaumkategorieEnum.InnenNacht.ToString(), typeof(double), Allgemein.Id, false);
            lmData.LamaTables.Add(Raumkategorie);

            DropdownTable Zone = new DropdownTable(Names.InnenflaecheAttributeEnum.Zone.ToString());
            lmData.LamaTables.Add(Zone);

            DropdownTable Richtung = new DropdownTable(Names.InnenflaecheAttributeEnum.Richtung.ToString());
            Richtung.AddLayerRow("1");
            Richtung.AddLayerRow("-1");
            lmData.LamaTables.Add(Richtung);

            DropdownTable Orientierung = new DropdownTable(Names.InnenflaecheAttributeEnum.Orientierung.ToString());
            Orientierung.AddLayerRow("Nord");
            Orientierung.AddLayerRow("N-W");
            Orientierung.AddLayerRow("West");
            Orientierung.AddLayerRow("S-W");
            Orientierung.AddLayerRow("Süd");
            Orientierung.AddLayerRow("S-O");
            Orientierung.AddLayerRow("Ost");
            Orientierung.AddLayerRow("N-O");
            lmData.LamaTables.Add(Orientierung);

            DropdownTable Bauteil = new DropdownTable(Names.BauteilAttributeEnum.Bauteil.ToString());
            Bauteil.AddClassColumn(Bauteilart, Allgemein.Id, false);
            Bauteil.AddClassColumn(Dicke, Geometrie.Id, false);
            lmData.LamaTables.Add(Bauteil);

            DropdownTable Raumgruppe = new DropdownTable(Names.RaumAttributesEnum.Raumgruppe.ToString());
            lmData.LamaTables.Add(Raumgruppe);

            DropdownTable Raum = new DropdownTable(Names.TabellenNameEnum.Raum.ToString());
            Raum.AddClassColumn(Geschoss, Allgemein, false);
            Raum.AddClassColumn(Raumkategorie, Allgemein, false);
            Raum.AddClassColumn(Raumgruppe, Allgemein, false);
            Raum.AddClassColumn(DINLaermpegel, Schallschutz, false);
            Raum.AddClassColumn(VDIVerkehrTag, Schallschutz, false);
            Raum.AddClassColumn(VDIVerkehrNacht, Schallschutz, false);
            Raum.AddClassColumn(VDIGewerbeTag, Schallschutz, false);
            Raum.AddClassColumn(VDIGewerbeNacht, Schallschutz, false);
            Raum.AddClassColumn(RerfDIN, Schallschutz, false);
            Raum.AddClassColumn(RerfVDITag, Schallschutz, false);
            Raum.AddClassColumn(RerfVDINacht, Schallschutz, false);
            Raum.AddClassColumn(sg, Schallschutz, false);
            Raum.AddClassColumn(ss, Schallschutz, false);
            Raum.AddClassColumn(RerfMax, Schallschutz, false);
            lmData.LamaTables.Add(Raum);

            // Add BoolTables
            BoolTable Fassadenflaeche = new(Names.InnenflaecheAttributeEnum.Fassadenflaeche.ToString());
            lmData.LamaTables.Add(Fassadenflaeche);

            BoolTable Grundflaeche = new(Names.InnenflaecheAttributeEnum.Grundflaeche.ToString());
            lmData.LamaTables.Add(Grundflaeche);


            // Add RhinoElementTables
            RhinoElementTable Referenzflaeche = new (Names.InnenflaecheAttributeEnum.Referenzflaeche.ToString());
            Referenzflaeche.AddClassColumn(Anzahl, Allgemein, false);
            lmData.LamaTables.Add(Referenzflaeche);

            RhinoElementTable Hauptkonstruktion = new ("Hauptkonstruktion");
            Hauptkonstruktion.AddClassColumn(Anzahl, Allgemein, false);
            Hauptkonstruktion.AddClassColumn(Reiw, Schallschutz, false);
            Hauptkonstruktion.AddClassColumn(Riw, Schallschutz, false);
            Hauptkonstruktion.AddClassColumn(Flaeche, Schallschutz, false);
            lmData.LamaTables.Add(Hauptkonstruktion);

            RhinoElementTable Nebenkonstruktion = new ("Nebenkonstruktion");
            Nebenkonstruktion.AddClassColumn(Anzahl, Allgemein, false);
            Nebenkonstruktion.AddClassColumn(Reiw, Schallschutz, false);
            Nebenkonstruktion.AddClassColumn(Riw, Schallschutz, false);
            Nebenkonstruktion.AddClassColumn(Flaeche, Schallschutz, false);
            lmData.LamaTables.Add(Nebenkonstruktion);

            RhinoElementTable Einbauteil = new ("Einbauteil");
            Einbauteil.AddClassColumn(Anzahl, Allgemein, false);
            Einbauteil.AddClassColumn(Reiw, Schallschutz, false);
            Einbauteil.AddClassColumn(DnewLab, Schallschutz, false);
            Einbauteil.AddClassColumn(lSitu, Schallschutz, false);
            Einbauteil.AddClassColumn(lLab, Schallschutz, false);
            lmData.LamaTables.Add(Einbauteil);

            RhinoElementTable Fenster = new ("Fenster");
            Fenster.AddClassColumn(Anzahl, Allgemein, false);
            Fenster.AddClassColumn(Reiw, Schallschutz, false);
            Fenster.AddClassColumn(Riw, Schallschutz, false);
            Fenster.AddClassColumn(UWert, Schallschutz, false);
            Fenster.AddClassColumn(gWert, Schallschutz, false);
            Fenster.AddClassColumn(Hoehe, Geometrie, false);
            Fenster.AddClassColumn(Breite, Geometrie, false);
            Fenster.AddClassColumn(Durchmesser, Geometrie, false);
            Fenster.AddClassColumn(Flaeche, Geometrie, false);
            Fenster.AddLeafColumn(Names.FassadenElementAttributeEnum.U.ToString(), typeof(double), Allgemein.Id, false);
            Fenster.AddLeafColumn(Names.FassadenElementAttributeEnum.V.ToString(), typeof(double), Allgemein.Id, false);
            lmData.LamaTables.Add(Fenster);

            RhinoElementTable Innenflaeche = new RhinoElementTable(Names.TypValueEnum.Innenflaeche.ToString());
            Innenflaeche.AddClassColumn(Anzahl, Allgemein, false);
            Innenflaeche.AddClassColumn(Bauteil, Allgemein, false);
            Innenflaeche.AddClassColumn(Neigung, Allgemein, false);
            Innenflaeche.AddClassColumn(Grundflaeche, Allgemein, false);
            Innenflaeche.AddClassColumn(Fassadenflaeche, Allgemein, false);
            Innenflaeche.AddClassColumn(Raum, Schallschutz, false);
            Innenflaeche.AddClassColumn(Orientierung, Allgemein, false);
            Innenflaeche.AddClassColumn(Nettoflaeche, Geometrie, false);
            Innenflaeche.AddClassColumn(Bruttoflaeche, Geometrie, false);
            Innenflaeche.AddClassColumn(Zone, Allgemein, false);
            Innenflaeche.AddClassColumn(Richtung, Geometrie, false);
            Innenflaeche.AddClassColumn(Verschiebung, Geometrie, false);
            Innenflaeche.AddClassColumn(Rwges, Schallschutz, false);
            Innenflaeche.AddClassColumn(Referenzflaeche, Relationen, false);
            Innenflaeche.AddClassColumn(Fenster, Relationen, true);
            Innenflaeche.AddClassColumn(Einbauteil, Relationen, true);
            Innenflaeche.AddClassColumn(Nebenkonstruktion, Relationen, true);
            Innenflaeche.AddClassColumn(Hauptkonstruktion, Relationen, false);
            lmData.LamaTables.Add(Innenflaeche);

            // Save File
            lmMainParser.WriteLamaData(lmData);

            // Start message
            RhinoApp.WriteLine("Erstelle Tabellen abgeschlossen");

            return Result.Success;
        }

    }
}
