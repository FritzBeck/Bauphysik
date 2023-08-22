using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using Rhino.DocObjects;
using Rhino;
using System.Windows.Documents;
using Bauphysik.Data;
using Bauphysik.Helpers;
using LayerManager;
using LayerManager.Helpers;
using LayerManager.Parser;
using LayerManager.Data;
using System.Windows.Input;

namespace Bauphysik.Data
{
    public class Connector
    {
        private LamaData LmData;

        public Connector(LamaData lmData)
        {
            this.LmData = lmData;
        }

        #region Parse

        /// <summary>
        /// Create BauModel from LamaData
        /// </summary>
        /// <param name="data">LamaData LmData from LayerManager library</param>
        /// <returns></returns>
        public BauModel Read()
        {
            BauModel bauModel = new BauModel();

            bauModel.Innenflaechen = CreateInnenflaechen();
            bauModel.Raume = CreateRaume(bauModel.Innenflaechen);
            bauModel.Raumkategorien = CreateRaumKategorien();
            bauModel.Bauteile = CreateBauteile();

            return bauModel;
        }

        private List<Raumkategorie> CreateRaumKategorien()
        {
            List<Raumkategorie> raumkategories = new ();

            LamaTable lmTable = LmData.LamaTables.GetTable(Names.TabellenNameEnum.Raumkategorie.ToString());

            //get lauerName of layerGuid
            foreach (DataRow row in lmTable.Rows)
            {
                if (row is LamaRow lmRow)
                {
                    string kategorie = (string)lmRow.GetValue2(Names.RaumkategorieEnum.Raumkategorie.ToString(), typeof(string));
                    if (string.IsNullOrWhiteSpace(kategorie)) continue;

                    Guid guid = lmRow.GetObjectID();
                    if (guid == Guid.Empty) continue;

                    Raumkategorie raumKategorie = new Raumkategorie(guid, kategorie);
                    if (raumKategorie == null) continue;

                    raumKategorie.K = (double?)lmRow.GetValue2(Names.RaumkategorieEnum.KWert.ToString(), typeof(double?));
                    raumKategorie.liTag = (double?)lmRow.GetValue2(Names.RaumkategorieEnum.InnenTag.ToString(), typeof(double?));
                    raumKategorie.liNacht = (double?)lmRow.GetValue2(Names.RaumkategorieEnum.InnenNacht.ToString(), typeof(double?));

                    raumkategories.Add(raumKategorie);
                }
            }

            return raumkategories;
        }


        private List<Bauteil> CreateBauteile()
        {
            List<Bauteil> bauteile = new();
            LamaTable lmTable = LmData.LamaTables.GetTable(Names.TabellenNameEnum.Bauteil.ToString());

            //get lauerName of layerGuid
            foreach (DataRow row in lmTable.Rows)
            {
                if (row is LamaRow lmRow)
                {
                    Guid guid = lmRow.GetObjectID();
                    if (guid == Guid.Empty) continue;

                    Bauteil bauteil = CreateBauteil(guid);
                    if (bauteil != null) bauteile.Add(bauteil);
                }
            }

            return bauteile;
        }

        private Bauteil CreateBauteil(Guid guid)
        {
            Raum raum = new Raum(guid);
            LamaTable lmTable = LmData.LamaTables.GetTable(Names.TabellenNameEnum.Bauteil.ToString());
            if (lmTable == null) return null;

            DataRow row = lmTable.GetRowByObjectID(raum.ObjectId);
            if (row == null) return null;

            if (row is LayerRow lmRow)
            {
                string bauteilName = (string)lmRow.GetValue2(Names.BauteilAttributeEnum.Bauteil.ToString(), typeof(string));
                Bauteil bauteil = new Bauteil(guid, bauteilName);

                bauteil.Art = (string)lmRow.GetValue2(Names.BauteilAttributeEnum.Bauteilart.ToString(), typeof(string));
                bauteil.Dicke = (double?)lmRow.GetValue2(Names.BauteilAttributeEnum.Dicke.ToString(), typeof(double?));

                return bauteil;
            }

            return null;
        }

        private List<Raum> CreateRaume(List<Innenflaeche> innenflaeches)
        {
            List<Raum> raums= new List<Raum>();

            LamaTable lmTable = LmData.LamaTables.GetTable(Names.TabellenNameEnum.Raum.ToString());

            //get lauerName of layerGuid
            foreach (DataRow row in lmTable.Rows)
            {
                if (row is LayerRow laRow)
                {
                    Raum raum = CreateRaum(laRow.GetLayerID(), innenflaeches);
                    if (raum != null) raums.Add(raum);
                }
            }

            return raums;
        }

        private Raum CreateRaum(Guid guid, List<Innenflaeche> innenflaeches)
        {
            Raum raum = new Raum(guid);
            LamaTable lmTable = LmData.LamaTables.GetTable(Names.TabellenNameEnum.Raum.ToString());
            if (lmTable == null) return null;

            DataRow row = lmTable.GetRowByObjectID(raum.ObjectId);
            if (row == null) return null;

            if (row is LamaRow lmRow)
            {
                raum.Raumname = (string)lmRow.GetValue2(Names.RaumAttributesEnum.Raum.ToString(), typeof(string));
                raum.Raumgruppe = (string)lmRow.GetValue2(Names.RaumAttributesEnum.Raumgruppe.ToString(), typeof(string));
                raum.Raumkategorie = (string)lmRow.GetValue2(Names.RaumAttributesEnum.Raumkategorie.ToString(), typeof(string));

                raum.DIN_Laermpegel = (double?)lmRow.GetValue2(Names.RaumAttributesEnum.DINLaermpegel.ToString(), typeof(double?));
                raum.VDI_Verkehr_Tag = (double?)lmRow.GetValue2(Names.RaumAttributesEnum.VDIVerkehrTag.ToString(), typeof(double?));
                raum.VDI_Verkehr_Nacht = (double?)lmRow.GetValue2(Names.RaumAttributesEnum.VDIVerkehrNacht.ToString(), typeof(double?));
                raum.VDI_Gewerbe_Tag = (double?)lmRow.GetValue2(Names.RaumAttributesEnum.VDIGewerbeTag.ToString(), typeof(double?));
                raum.VDI_Gewerbe_Nacht = (double?)lmRow.GetValue2(Names.RaumAttributesEnum.VDIGewerbeNacht.ToString(), typeof(double?));

                raum.Rerf_DIN = (double?)lmRow.GetValue2(Names.RaumAttributesEnum.RerfDIN.ToString(), typeof(double?));
                raum.Rerf_VDI_Tag = (double?)lmRow.GetValue2(Names.RaumAttributesEnum.RerfVDITag.ToString(), typeof(double?));
                raum.Rerf_VDI_Nacht = (double?)lmRow.GetValue2(Names.RaumAttributesEnum.RerfVDINacht.ToString(), typeof(double?));

                //Get relatedObjects
                LamaTable lmTableInnen = LmData.LamaTables.GetTable(Names.TypValueEnum.Innenflaeche.ToString());
                if (lmTableInnen is IClassTable classTable)
                {
                    DataColumn raumCol = classTable.GetClassColumnByTable(lmTable);
                    if (raumCol != null)
                    {
                        List<DataRow> rowsInnenflaeche = lmTableInnen.GetRows(raumCol, raum.Raumname);
                        if (rowsInnenflaeche != null && rowsInnenflaeche.Count > 0)
                            foreach (DataRow rowsInnen in rowsInnenflaeche)
                                if (rowsInnen is LamaRow lmRowInnen)
                                {
                                    Innenflaeche innenflaeche = innenflaeches.Where(i => i.ObjectId == lmRowInnen.GetObjectID()).FirstOrDefault();
                                    if (innenflaeche != null) raum.Innenflaechen.Add(innenflaeche);
                                }

                    }
                }

            }

            return raum;
        }


        private List<Innenflaeche> CreateInnenflaechen()
        {
            List<Innenflaeche> innenflaeches = new();

            LamaTable lmTable = LmData.LamaTables.GetTable(Names.TypValueEnum.Innenflaeche.ToString());
            foreach (DataRow row in lmTable.Rows)
            {
                if (row is LamaRow lmRow)
                {
                    Guid rhObjGuid = lmRow.GetObjectID();
                    Innenflaeche innenflaeche = CreateInnenflaeche(rhObjGuid);
                    if (innenflaeche != null) innenflaeches.Add(innenflaeche);
                }
            }

            return innenflaeches;
        }

        private Innenflaeche CreateInnenflaeche(Guid rhObjGuid)
        {
            Innenflaeche innenflaeche = new Innenflaeche(rhObjGuid);

            LamaTable lmTable = LmData.LamaTables.GetTable(Names.TypValueEnum.Innenflaeche.ToString());

            DataRow row = lmTable.GetRowByObjectID(rhObjGuid);
            if (row == null) return default;

            if (row is LamaRow lmRow)
            {
                innenflaeche.Bauteil = (string)lmRow.GetValue2(Names.InnenflaecheAttributeEnum.Bauteil.ToString(), typeof(string));
                innenflaeche.Zone = (string)lmRow.GetValue2(Names.InnenflaecheAttributeEnum.Zone.ToString(), typeof(string));

/*                innenflaeche.FlaecheBrutto = (double?)lmRow.GetValue2(Names.InnenflaecheAttributeEnum.FlaecheBrutto.ToString(), typeof(double?));
                innenflaeche.FlaecheNetto = (double?)lmRow.GetValue2(Names.InnenflaecheAttributeEnum.FlaecheNetto.ToString(), typeof(double?));*/

                innenflaeche.Orientierung = (string)lmRow.GetValue2(Names.InnenflaecheAttributeEnum.Orientierung.ToString(), typeof(string));
                innenflaeche.Neigung = (double?)lmRow.GetValue2(Names.InnenflaecheAttributeEnum.Neigung.ToString(), typeof(double?));

                innenflaeche.Richtung = (int?)lmRow.GetValue2(Names.InnenflaecheAttributeEnum.Richtung.ToString(), typeof(int?));
                innenflaeche.Verschiebung = (double?)lmRow.GetValue2(Names.InnenflaecheAttributeEnum.Verschiebung.ToString(), typeof(double?));
                innenflaeche.Richtung_GUID = null;

                innenflaeche.Grundflaeche = (bool?)lmRow.GetValue2(Names.InnenflaecheAttributeEnum.Grundflaeche.ToString(), typeof(bool?));
                innenflaeche.Fassadenflaeche = (bool?)lmRow.GetValue2(Names.InnenflaecheAttributeEnum.Fassadenflaeche.ToString(), typeof(bool?));


/*                List<Guid> richtungGuids = GetGuidListFromAttribute(row, Names.InnenflaecheAttributeEnum.RichtungsObjekt.ToString());
                if (richtungGuids != null) innenflaeche.Richtung_GUID = richtungGuids[0];*/

                Guid refGuids = (Guid)lmRow.GetValue(Names.InnenflaecheAttributeEnum.Referenzflaeche.ToString(), typeof(Guid));
                if (refGuids != Guid.Empty) innenflaeche.Referenzflaeche = CreateReferenzflaeche((Guid)refGuids);

                //List<Guid> fassadeGuids = GetGuidListFromAttribute(row, Names.InnenflaecheAttributeEnum.Fassade.ToString());
                //if (fassadeGuids != null)
                innenflaeche.Fassade = CreateFassade(rhObjGuid);
            }

            return innenflaeche;

        }

        private Referenzflaeche CreateReferenzflaeche(Guid rhObjGuid)
        {
            Referenzflaeche refFlaeche = new Referenzflaeche(rhObjGuid);

            LamaTable lmTable = LmData.LamaTables.GetTable(Names.TypValueEnum.Referenzflaeche.ToString());

            DataRow row = lmTable.GetRowByObjectID(rhObjGuid);
            if (row == null) return default;

            return refFlaeche;
        }

        public Fassade CreateFassade(Guid rhObjGuid)
        {
            Fassade fassade = new Fassade(rhObjGuid);

            LamaTable lmTable = LmData.LamaTables.GetTable(Names.TypValueEnum.Innenflaeche.ToString());

            DataRow row = lmTable.GetRowByObjectID(rhObjGuid);
            if (row == null) return default;

            if (row is LamaRow lmRow)
            {
                fassade.R_wges = (double?)lmRow.GetValue2(Names.FassadeAttributeEnum.Rwges.ToString(), typeof(double?));

                List<object> windowGuids = (List<object>)lmRow.GetValue2(Names.TypValueEnum.Fenster.ToString(), typeof(Guid));
                if (windowGuids != null && windowGuids.Count > 0)
                {
                    foreach(object guid in windowGuids)
                    {
                        Fenster fenster = CreateFenster((Guid)guid);
                        if (fenster != null) fassade.Fensters.Add(fenster);
                    }
                    
                }

                List<Guid> einbauteilGuids = (List<Guid>)lmRow.GetValue2(Names.TypValueEnum.Einbauteil.ToString(), typeof(Guid));
                if (einbauteilGuids != null && einbauteilGuids.Count > 0)
                {
                    foreach (Guid guid in einbauteilGuids)
                    {
                        Einbauteil einbauteil = CreateEinbauteil(guid);
                        if (einbauteil != null) fassade.Einbauteils.Add(einbauteil);
                    }

                }

                List<Guid> nebkonstGuids = (List<Guid>)lmRow.GetValue2(Names.TypValueEnum.Nebenkonstruktion.ToString(), typeof(Guid));
                if (nebkonstGuids != null && nebkonstGuids.Count > 0)
                {
                    foreach (Guid guid in nebkonstGuids)
                    {
                        Nebenkonstruktion nebenKonstruktion = CreateNebenKonstruktion(guid);
                        if (nebenKonstruktion != null) fassade.Nebenkonstruktions.Add(nebenKonstruktion);
                    }

                }

                Guid hauptkonstrGuid = (Guid)lmRow.GetValue(Names.TypValueEnum.Hauptkonstruktion.ToString(), typeof(Guid));
                if (hauptkonstrGuid != Guid.Empty)
                        fassade.Hauptkonstruktion = CreateHauptkonstruktion(hauptkonstrGuid);

            }

            return fassade;
        }

        private Hauptkonstruktion CreateHauptkonstruktion(Guid rhObjGuid)
        {
            Hauptkonstruktion obj = new Hauptkonstruktion(rhObjGuid);

            LamaTable lmTable = LmData.LamaTables.GetTable(Names.TypValueEnum.Hauptkonstruktion.ToString());

            DataRow row = lmTable.GetRowByObjectID(obj.ObjectId);
            if (row == null) return default;

            if (row is LamaRow lmRow)
            {
                obj.Reiw = (double?)lmRow.GetValue2(Names.FassadenElementAttributeEnum.Reiw.ToString(), typeof(double?));

                obj.R_iw = (double?)lmRow.GetValue2(Names.KonstruktionAttributeEnum.Riw.ToString(), typeof(double?));
                obj.UWert = (double?)lmRow.GetValue2(Names.KonstruktionAttributeEnum.UWert.ToString(), typeof(double?));

                obj.U = (double?)lmRow.GetValue2(Names.FassadenElementAttributeEnum.U.ToString(), typeof(double?));
                obj.V = (double?)lmRow.GetValue2(Names.FassadenElementAttributeEnum.V.ToString(), typeof(double?));
                obj.IsAbstract = (bool?)lmRow.GetValue2(Names.FassadenElementAttributeEnum.IsAbstract.ToString(), typeof(bool?));
            }

            return obj;
        }

        private Nebenkonstruktion CreateNebenKonstruktion(Guid rhObjGuid)
        {
            Nebenkonstruktion obj = new Nebenkonstruktion(rhObjGuid);

            LamaTable lmTable = LmData.LamaTables.GetTable(Names.TypValueEnum.Nebenkonstruktion.ToString());

            DataRow row = lmTable.GetRowByObjectID(obj.ObjectId);
            if (row == null) return default;

            if (row is LamaRow lmRow)
            {
                obj.Flaeche = (double?)lmRow.GetValue2(Names.FassadenElementAttributeEnum.Flaeche.ToString(), typeof(double?));
                obj.Reiw = (double?)lmRow.GetValue2(Names.FassadenElementAttributeEnum.Reiw.ToString(), typeof(double?));

                obj.R_iw = (double?)lmRow.GetValue2(Names.KonstruktionAttributeEnum.Riw.ToString(), typeof(double?));
                obj.UWert = (double?)lmRow.GetValue2(Names.KonstruktionAttributeEnum.UWert.ToString(), typeof(double?));

                obj.U = (double?)lmRow.GetValue2(Names.FassadenElementAttributeEnum.U.ToString(), typeof(double?));
                obj.V = (double?)lmRow.GetValue2(Names.FassadenElementAttributeEnum.V.ToString(), typeof(double?));
                obj.IsAbstract = (bool?)lmRow.GetValue2(Names.FassadenElementAttributeEnum.IsAbstract.ToString(), typeof(bool?));
            }

            return obj;
        }

        private Einbauteil CreateEinbauteil(Guid rhObjGuid)
        {
            Einbauteil obj = new Einbauteil(rhObjGuid);

            LamaTable lmTable = LmData.LamaTables.GetTable(Names.TypValueEnum.Einbauteil.ToString());

            DataRow row = lmTable.GetRowByObjectID(obj.ObjectId);
            if (row == null) return default;

            if (row is LamaRow lmRow)
            {
                obj.Reiw = (double?)lmRow.GetValue2(Names.FassadenElementAttributeEnum.Reiw.ToString(), (typeof(double?)));

                obj.D_newlab = (double?)lmRow.GetValue2(Names.EinbauteilAttributeEnum.DnewLab.ToString(), typeof(double?));
                obj.L_lab = (double?)lmRow.GetValue2(Names.EinbauteilAttributeEnum.lLab.ToString(), typeof(double?));
                obj.L_situ = (double?)lmRow.GetValue2(Names.EinbauteilAttributeEnum.lSitu.ToString(), typeof(double?));
                obj.Anzahl = (int?)lmRow.GetValue2(Names.EinbauteilAttributeEnum.Anzahl.ToString(), typeof(int?));

                obj.U = (double?)lmRow.GetValue2(Names.FassadenElementAttributeEnum.U.ToString(), typeof(double?));
                obj.V = (double?)lmRow.GetValue2(Names.FassadenElementAttributeEnum.V.ToString(), typeof(double?));
                obj.IsAbstract = (bool?)lmRow.GetValue2(Names.FassadenElementAttributeEnum.IsAbstract.ToString(), typeof(bool?));
            }

            return obj;
        }

        public Fenster CreateFenster(Guid rhObjGuid)
        {
            Fenster obj = new Fenster(rhObjGuid);

            LamaTable lmTable = LmData.LamaTables.GetTable(Names.TypValueEnum.Fenster.ToString());

            DataRow row = lmTable.GetRowByObjectID(obj.ObjectId);
            if (row == null) return default;

            if (row is LamaRow lmRow)
            {
                obj.Reiw = (double?)lmRow.GetValue2(Names.FassadenElementAttributeEnum.Reiw.ToString(), typeof(double?));

                obj.Riw = (double?)lmRow.GetValue2(Names.FensterAttributeEnum.Riw.ToString(), typeof(double?));
                obj.Hoehe = (double?)lmRow.GetValue2(Names.FensterAttributeEnum.Hoehe.ToString(), typeof(double?));
                obj.Breite = (double?)lmRow.GetValue2(Names.FensterAttributeEnum.Breite.ToString(), typeof(double?));
                obj.Anzahl = (int?)lmRow.GetValue2(Names.FensterAttributeEnum.Anzahl.ToString(), typeof(int?));
                obj.Flaeche = (double?)lmRow.GetValue2(Names.FensterAttributeEnum.Flaeche.ToString(), typeof(double?));
                obj.UWert = (double?)lmRow.GetValue2(Names.FensterAttributeEnum.UWert.ToString(), typeof(double?));
                obj.GWert = (double?)lmRow.GetValue2(Names.FensterAttributeEnum.gWert.ToString(), typeof(double?));
                obj.Bruestungshoehe = (double?)lmRow.GetValue2(Names.FensterAttributeEnum.Bruestungshoehe.ToString(), typeof(double?));

                obj.U = (double?)lmRow.GetValue2(Names.FassadenElementAttributeEnum.U.ToString(), typeof(double?));
                obj.V = (double?)lmRow.GetValue2(Names.FassadenElementAttributeEnum.V.ToString(), typeof(double?));
                obj.IsAbstract = (bool?)lmRow.GetValue2(Names.FassadenElementAttributeEnum.IsAbstract.ToString(), typeof(bool?));
            }

            return obj;
        }


        #endregion

        #region Write

        public void Write(BauModel model)
        {

            if (model.Innenflaechen != null && model.Innenflaechen.Count > 0)
                foreach (Innenflaeche innenflaeche in model.Innenflaechen)
                    WriteInnenflaeche(innenflaeche);

            if (model.Raume != null && model.Raume.Count > 0)
                foreach (Raum raum in model.Raume)
                    WriteRaum(raum, model.Innenflaechen);

            if (model.Bauteile != null && model.Bauteile.Count > 0)
                foreach (Bauteil bauteil in model.Bauteile)
                    WriteBauteil(bauteil);

            if (model.Raumkategorien != null && model.Raumkategorien.Count > 0)
                foreach (Raumkategorie raumKategorie in model.Raumkategorien)
                    WriteRaumkategorie(raumKategorie);

        }


        private void WriteRaumkategorie(Raumkategorie obj)
        {
            LamaTable lmTable = LmData.LamaTables.GetTable(Helpers.Names.TabellenNameEnum.Raumkategorie.ToString());
            if (lmTable == null) return;

            DataRow row = GetOrCreateRow(obj.ObjectId, lmTable);

            if (row is LamaRow lmRow)
            {
                lmRow.SetValue(Names.RaumkategorieEnum.Raumkategorie.ToString(), obj.Name);
                lmRow.SetValue(Names.RaumkategorieEnum.KWert.ToString(), obj.K);
                lmRow.SetValue(Names.RaumkategorieEnum.InnenTag.ToString(), obj.liTag);
                lmRow.SetValue(Names.RaumkategorieEnum.InnenNacht.ToString(), obj.liNacht);
            }

        }


        private void WriteBauteil(Bauteil obj)
        {
            LamaTable lmTable = LmData.LamaTables.GetTable(Helpers.Names.TabellenNameEnum.Bauteil.ToString());
            if (lmTable == null) return;

            DataRow row = GetOrCreateRow(obj.ObjectId, lmTable);

            if (row is LamaRow lmRow)
            {
                lmRow.SetValue(Names.BauteilAttributeEnum.Bauteil.ToString(), obj.Name);
                lmRow.SetValue(Names.BauteilAttributeEnum.Bauteilart.ToString(), obj.Art);
                lmRow.SetValue(Names.BauteilAttributeEnum.Dicke.ToString(), obj.Dicke);
            }
        }

        private void WriteRaum(Raum obj, List<Innenflaeche> innenflaechen)
        {
            LamaTable lmTable = LmData.LamaTables.GetTable(Helpers.Names.TabellenNameEnum.Raum.ToString());
            if (lmTable == null) return;

            DataRow row = GetOrCreateRow(obj.ObjectId, lmTable);

            if (row is LamaRow lmRow)
            {
                lmRow.SetValue(Names.RaumAttributesEnum.Raum.ToString(), obj.Raumname);
                lmRow.SetValue(Names.RaumAttributesEnum.Raumkategorie.ToString(), obj.Raumkategorie);
                lmRow.SetValue(Names.RaumAttributesEnum.DINLaermpegel.ToString(), obj.DIN_Laermpegel);
                lmRow.SetValue(Names.RaumAttributesEnum.VDIVerkehrTag.ToString(), obj.VDI_Verkehr_Tag);
                lmRow.SetValue(Names.RaumAttributesEnum.VDIVerkehrNacht.ToString(), obj.VDI_Verkehr_Nacht);
                lmRow.SetValue(Names.RaumAttributesEnum.VDIGewerbeTag.ToString(), obj.VDI_Gewerbe_Tag);
                lmRow.SetValue(Names.RaumAttributesEnum.VDIGewerbeNacht.ToString(), obj.VDI_Gewerbe_Nacht);
                lmRow.SetValue(Names.RaumAttributesEnum.RerfDIN.ToString(), obj.Rerf_DIN);
                lmRow.SetValue(Names.RaumAttributesEnum.RerfVDITag.ToString(), obj.Rerf_VDI_Tag);
                lmRow.SetValue(Names.RaumAttributesEnum.RerfVDINacht.ToString(), obj.Rerf_VDI_Nacht);
                lmRow.SetValue(Names.RaumAttributesEnum.RerfMax.ToString(), obj.Rerf_Max);
                lmRow.SetValue(Names.RaumAttributesEnum.ss.ToString(), obj.Ss(false));
                lmRow.SetValue(Names.RaumAttributesEnum.sg.ToString(), obj.Sg(false));
            }
        }

        public void WriteInnenflaeche(Innenflaeche obj)
        {
            LamaTable lmTable = LmData.LamaTables.GetTable(Names.TypValueEnum.Innenflaeche.ToString());
            if (lmTable == null) return;

            DataRow row = GetOrCreateRow(obj.ObjectId, lmTable);

            if (row is LamaRow lmRow)
            {
                lmRow.SetValue(Names.BauteilAttributeEnum.Bauteil.ToString(), obj.Bauteil);
                lmRow.SetValue(Names.InnenflaecheAttributeEnum.Zone.ToString(), obj.Zone);
                lmRow.SetValue(Names.InnenflaecheAttributeEnum.Bruttoflaeche.ToString(), Math.Round(obj.FlaecheBrutto(), 3));
                lmRow.SetValue(Names.InnenflaecheAttributeEnum.Nettoflaeche.ToString(), Math.Round(obj.FlaecheNetto(), 3));
                lmRow.SetValue(Names.InnenflaecheAttributeEnum.Orientierung.ToString(), obj.Orientierung);
                lmRow.SetValue(Names.InnenflaecheAttributeEnum.Neigung.ToString(), obj.Neigung);
                lmRow.SetValue(Names.InnenflaecheAttributeEnum.Richtung.ToString(), obj.Richtung);
                lmRow.SetValue(Names.InnenflaecheAttributeEnum.Verschiebung.ToString(), obj.Verschiebung);
                lmRow.SetValue(Names.InnenflaecheAttributeEnum.Grundflaeche.ToString(), obj.Grundflaeche);
                lmRow.SetValue(Names.InnenflaecheAttributeEnum.Fassadenflaeche.ToString(), obj.Fassadenflaeche);

                if (obj.Referenzflaeche != null)
                {
                    lmRow.SetValue(Names.InnenflaecheAttributeEnum.Referenzflaeche.ToString(), obj.Referenzflaeche.ObjectId);
                    WriteReferenzflaeche(obj.Referenzflaeche, obj);
                }

                if (obj.Fassade != null)
                {
                    lmRow.SetValue(Names.InnenflaecheAttributeEnum.Fassade.ToString(), obj.Fassade.ObjectId);
                    WriteFassade(obj.Fassade, obj);
                }
            }


        }


        private void WriteFassade(Fassade obj, Innenflaeche relatedInnenflaeche)
        {
            LamaTable lmTable = LmData.LamaTables.GetTable(Names.TypValueEnum.Innenflaeche.ToString());
            if (lmTable == null) return;

            DataRow row = GetOrCreateRow(obj.ObjectId, lmTable);

            if (row is LamaRow lmRow)
            {
                lmRow.SetValue(Names.FassadeAttributeEnum.Rwges.ToString(), obj.R_wges);

                if (obj.Fensters != null && obj.Fensters.Count > 0)
                {
                    foreach (Fenster fenster in obj.Fensters)
                        WriteFenster(fenster, relatedInnenflaeche);

                    List<Guid> guids = obj.Fensters.Select(f => f.ObjectId).ToList();
                    lmRow.SetValue(Names.FassadeAttributeEnum.Fenster.ToString(), guids);
                }


                if (obj.Einbauteils != null && obj.Einbauteils.Count > 0)
                {
                    foreach (Einbauteil einbauteil in obj.Einbauteils)
                        WriteEinbauteil(einbauteil);

                    List<Guid> guids = obj.Einbauteils.Select(f => f.ObjectId).ToList();
                    lmRow.SetValue(Names.FassadeAttributeEnum.Einbauteil.ToString(), guids);
                }

                if (obj.Nebenkonstruktions != null && obj.Nebenkonstruktions.Count > 0)
                {
                    foreach (Nebenkonstruktion nebenkonstruktion in obj.Nebenkonstruktions)
                        WriteNebenkonstruktion(nebenkonstruktion);

                    List<Guid> guids = obj.Nebenkonstruktions.Select(f => f.ObjectId).ToList();
                    lmRow.SetValue(Names.FassadeAttributeEnum.Nebenkonstruktion.ToString(), guids);
                }

                if (obj.Hauptkonstruktion != null)
                {
                    WriteHauptkonstruktion(obj.Hauptkonstruktion, relatedInnenflaeche);
                    lmRow.SetValue(Names.FassadeAttributeEnum.Hauptkonstruktion.ToString(), obj.Hauptkonstruktion.ObjectId);
                }
            }


        }

        private void WriteHauptkonstruktion(Hauptkonstruktion obj, Innenflaeche relatedInnenflaeche)
        {
            LamaTable lmTable = LmData.LamaTables.GetTable(Names.TypValueEnum.Hauptkonstruktion.ToString());
            if (lmTable == null) return;

            DataRow row = GetOrCreateRow(obj.ObjectId, lmTable);

            if (row is LamaRow lmRow)
            {

                lmRow.SetValue(Names.FassadenElementAttributeEnum.Flaeche.ToString(), Math.Round(obj.Flaeche(relatedInnenflaeche), 3));
                lmRow.SetValue(Names.FassadenElementAttributeEnum.Reiw.ToString(), obj.Reiw);
                lmRow.SetValue(Names.KonstruktionAttributeEnum.Riw.ToString(), obj.R_iw);
                lmRow.SetValue(Names.KonstruktionAttributeEnum.UWert.ToString(), obj.UWert);

                lmRow.SetValue(Names.FassadenElementAttributeEnum.U.ToString(), obj.U);
                lmRow.SetValue(Names.FassadenElementAttributeEnum.V.ToString(), obj.V);
                lmRow.SetValue(Names.FassadenElementAttributeEnum.IsAbstract.ToString(), obj.IsAbstract);
            }
        }

        private void WriteNebenkonstruktion(Nebenkonstruktion obj)
        {
            LamaTable lmTable = LmData.LamaTables.GetTable(Names.TypValueEnum.Nebenkonstruktion.ToString());
            if (lmTable == null) return;

            DataRow row = GetOrCreateRow(obj.ObjectId, lmTable);

            if (row is LamaRow lmRow)
            {
                lmRow.SetValue(Names.FassadenElementAttributeEnum.Flaeche.ToString(), obj.Flaeche);
                lmRow.SetValue(Names.FassadenElementAttributeEnum.Reiw.ToString(), obj.Reiw);
                lmRow.SetValue(Names.KonstruktionAttributeEnum.Riw.ToString(), obj.R_iw);
                lmRow.SetValue(Names.KonstruktionAttributeEnum.UWert.ToString(), obj.UWert);

                lmRow.SetValue(Names.FassadenElementAttributeEnum.U.ToString(), obj.U);
                lmRow.SetValue(Names.FassadenElementAttributeEnum.V.ToString(), obj.V);
                lmRow.SetValue(Names.FassadenElementAttributeEnum.IsAbstract.ToString(), obj.IsAbstract);
            }
        }

        private void WriteEinbauteil(Einbauteil obj)
        {
            LamaTable lmTable = LmData.LamaTables.GetTable(Names.TypValueEnum.Einbauteil.ToString());
            if (lmTable == null) return;

            DataRow row = GetOrCreateRow(obj.ObjectId, lmTable);

            if (row is LamaRow lmRow)
            {
                lmRow.SetValue(Names.FassadenElementAttributeEnum.Reiw.ToString(), obj.Reiw);
                lmRow.SetValue(Names.EinbauteilAttributeEnum.DnewLab.ToString(), obj.D_newlab);
                lmRow.SetValue(Names.EinbauteilAttributeEnum.lLab.ToString(), obj.L_lab);
                lmRow.SetValue(Names.EinbauteilAttributeEnum.lSitu.ToString(), obj.L_situ);
                lmRow.SetValue(Names.EinbauteilAttributeEnum.Anzahl.ToString(), obj.Anzahl);

                lmRow.SetValue(Names.FassadenElementAttributeEnum.U.ToString(), obj.U);
                lmRow.SetValue(Names.FassadenElementAttributeEnum.V.ToString(), obj.V);
                lmRow.SetValue(Names.FassadenElementAttributeEnum.IsAbstract.ToString(), obj.IsAbstract);
            }

        }

        public void WriteFenster(Fenster obj, Innenflaeche relatedInnenflaeche)
        {
            LamaTable lmTable = LmData.LamaTables.GetTable(Names.TypValueEnum.Fenster.ToString());
            if (lmTable == null) return;

            DataRow row = GetOrCreateRow(obj.ObjectId, lmTable);

            if (row is LamaRow lmRow)
            {
                lmRow.SetValue(Names.FassadenElementAttributeEnum.Reiw.ToString(), obj.Reiw);
                lmRow.SetValue(Names.FensterAttributeEnum.Riw.ToString(), obj.Riw);
                lmRow.SetValue(Names.FensterAttributeEnum.Hoehe.ToString(), obj.Hoehe);
                lmRow.SetValue(Names.FensterAttributeEnum.Breite.ToString(), obj.Breite);
                lmRow.SetValue(Names.FensterAttributeEnum.Durchmesser.ToString(), obj.Durchmesser);
                lmRow.SetValue(Names.FensterAttributeEnum.Anzahl.ToString(), obj.Anzahl);

                lmRow.SetValue(Names.FensterAttributeEnum.Flaeche.ToString(), Math.Round(obj.FlaecheGesamt(), 3));
                lmRow.SetValue(Names.FensterAttributeEnum.UWert.ToString(), obj.UWert);
                lmRow.SetValue(Names.FensterAttributeEnum.gWert.ToString(), obj.GWert);
                lmRow.SetValue(Names.FensterAttributeEnum.Bruestungshoehe.ToString(), obj.Bruestungshoehe);

                lmRow.SetValue(Names.FassadenElementAttributeEnum.U.ToString(), obj.U);
                lmRow.SetValue(Names.FassadenElementAttributeEnum.V.ToString(), obj.V);
                lmRow.SetValue(Names.FassadenElementAttributeEnum.IsAbstract.ToString(), obj.IsAbstract);

            }

        }

        private void WriteReferenzflaeche(Referenzflaeche obj, Innenflaeche innenflaeche)
        {
            LamaTable lmTable = LmData.LamaTables.GetTable(Names.TypValueEnum.Referenzflaeche.ToString());
            if (lmTable == null) return;

            DataRow row = GetOrCreateRow(obj.ObjectId, lmTable);

            if (row is LamaRow lmRow)
            {
                lmRow.SetValue(Names.InnenflaecheAttributeEnum.Bruttoflaeche.ToString(), Math.Round(obj.FlaecheBrutto(), 3));
                lmRow.SetValue(Names.InnenflaecheAttributeEnum.Nettoflaeche.ToString(), Math.Round(obj.FlaecheNetto(innenflaeche), 3));
            }

        }

        #endregion

        private DataRow GetOrCreateRow(Guid objectId, LamaTable lmTable)
        {
            DataRow row = lmTable.GetRowByObjectID(objectId);
            if (row == null)
            {
                if (lmTable is RhinoTable rhTable)
                {
                    RhinoRow newRow = rhTable.NewRhinoRow();
                    newRow.SetRhinoObjectID(objectId);
                    rhTable.AddRhinoRow(newRow);

                    row = lmTable.GetRowByObjectID(objectId);
                }

                if (lmTable is LayerTable layerTable)
                {
                    LayerRow newRow = layerTable.NewLayerRow();
                    newRow.SetLayerID(objectId);
                    layerTable.AddLayerRow(newRow);

                    row = lmTable.GetRowByObjectID(objectId);
                }
            }

            return row;
        }

    }
}
