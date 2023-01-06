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

namespace Bauphysik.Data
{
    public class Connector
    {
        public Connector()
        {
        }

        #region Parse

        /// <summary>
        /// Create BauModel from RootDB
        /// </summary>
        /// <param name="data">RootDB data from LayerManager library</param>
        /// <returns></returns>
        public BauModel InitModel(RootDB data)
        {
            BauModel bauModel = new BauModel();

            CreateInnenflaechen(data, ref bauModel);
            CreateRaume(data, ref bauModel);
            CreateRaumKategorien(data, ref bauModel);
            CreateBauteile(data, ref bauModel);

            return bauModel;
        }

        private void CreateRaumKategorien(RootDB data, ref BauModel bauModel)
        {
            TableDB tableDB = data.TableDBs.GetTable(Helpers.Names.TabellenNameEnum.Raumkategorie.ToString());

            //get lauerName of layerGuid
            foreach (DataRow row in tableDB.Table.Rows)
            {
                string kategorie = (string)GetValueFromRow(row, Names.RaumkategorieEnum.Raumkategorie.ToString(), typeof(string));
                Guid guid = (Guid)row[0];

                Raumkategorie raumKategorie = new Raumkategorie(guid, kategorie);
                if (raumKategorie == null) continue;

                raumKategorie.K = (double?)GetValueFromRow(row, Names.RaumkategorieEnum.KWert.ToString(), typeof(double?));
                raumKategorie.liTag = (double?)GetValueFromRow(row, Names.RaumkategorieEnum.InnenTag.ToString(), typeof(double?));
                raumKategorie.liNacht = (double?)GetValueFromRow(row, Names.RaumkategorieEnum.InnenNacht.ToString(), typeof(double?));

                bauModel.Raumkategorien.Add(raumKategorie);
            }
        }


        private void CreateBauteile(RootDB data, ref BauModel bauModel)
        {
            TableDB tableDB = data.TableDBs.GetTable(Names.TabellenNameEnum.Bauteil.ToString());

            //get lauerName of layerGuid
            foreach (DataRow row in tableDB.Table.Rows)
            {
                Guid guid = (Guid)row[0];
                Bauteil bauteil = CreateBauteil(guid, data);
                if (bauteil != null) bauModel.Bauteile.Add(bauteil);
            }
        }

        private Bauteil CreateBauteil(Guid guid, RootDB data)
        {
            Raum raum = new Raum(guid);
            TableDB tableDB = data.TableDBs.GetTable(Names.TabellenNameEnum.Bauteil.ToString());
            if (tableDB == null) return null;

            DataRow row = tableDB.GetRowByID(raum.ObjectGuid);
            if (row == null) return null;

            string bauteilName = (string)GetValueFromRow(row, Names.BauteilAttributeEnum.Bauteil.ToString(), typeof(string));
            Bauteil bauteil = new Bauteil(guid, bauteilName);

            bauteil.Art = (string)GetValueFromRow(row, Names.BauteilAttributeEnum.Bauteilart.ToString(), typeof(string));
            bauteil.Dicke = (double?)GetValueFromRow(row, Names.BauteilAttributeEnum.Dicke.ToString(), typeof(double?));

            return bauteil;

        }

        private void CreateRaume(RootDB data, ref BauModel bauModel)
        {
            TableDB tableDB = data.TableDBs.GetTable(Names.TabellenNameEnum.Raum.ToString());

            //get lauerName of layerGuid
            foreach (DataRow row in tableDB.Table.Rows)
            {
                Guid guid = (Guid)row[0];
                Raum raum = CreateRaum(guid, data);
                if (raum != null) bauModel.Raume.Add(raum);
            }
        }

        private Raum CreateRaum(Guid guid, RootDB data)
        {
            Raum raum = new Raum(guid);
            TableDB tableDB = data.TableDBs.GetTable(Names.TabellenNameEnum.Raum.ToString());
            if (tableDB == null) return null;

            DataRow row = tableDB.GetRowByID(raum.ObjectGuid);
            if (row == null) return null;

            if (tableDB is AbstrTableDB raumTable)
            {
                raum.Raumname = (string)GetValueFromRow(row, Names.RaumAttributesEnum.Raum.ToString(), typeof(string));
                raum.Raumgruppe = (string)GetValueFromRow(row, Names.RaumAttributesEnum.Raumgruppe.ToString(), typeof(string));
                raum.Raumkategorie = (string)GetValueFromRow(row, Names.RaumAttributesEnum.Raumkategorie.ToString(), typeof(string));

                raum.DIN_Laermpegel = (double?)GetValueFromRow(row, Names.RaumAttributesEnum.DINLaermpegel.ToString(), typeof(double?));
                raum.VDI_Verkehr_Tag = (double?)GetValueFromRow(row, Names.RaumAttributesEnum.VDIVerkehrTag.ToString(), typeof(double?));
                raum.VDI_Verkehr_Nacht = (double?)GetValueFromRow(row, Names.RaumAttributesEnum.VDIVerkehrNacht.ToString(), typeof(double?));
                raum.VDI_Gewerbe_Tag = (double?)GetValueFromRow(row, Names.RaumAttributesEnum.VDIGewerbeTag.ToString(), typeof(double?));
                raum.VDI_Gewerbe_Nacht = (double?)GetValueFromRow(row, Names.RaumAttributesEnum.VDIGewerbeNacht.ToString(), typeof(double?));

                raum.Rerf_DIN = (double?)GetValueFromRow(row, Names.RaumAttributesEnum.RerfDIN.ToString(), typeof(double?));
                raum.Rerf_VDI_Tag = (double?)GetValueFromRow(row, Names.RaumAttributesEnum.RerfVDITag.ToString(), typeof(double?));
                raum.Rerf_VDI_Nacht = (double?)GetValueFromRow(row, Names.RaumAttributesEnum.RerfVDINacht.ToString(), typeof(double?));

                //Get relatedObjects
                TableDB innenTableDB = data.TableDBs.GetTable(Names.TypValueEnum.Innenflaeche.ToString());
                DataColumn raumCol = innenTableDB.GetColumnByRelatedID(raumTable.Id);
                if (raumCol != null)
                {
                    List<DataRow> rows = innenTableDB.Table.Rows.Cast<DataRow>().Where(r => DataHelpers.ConvertToString(r[raumCol]) == raum.Raumname).ToList();
                    if (rows != null && rows.Count > 0)
                    {
                        raum.RelatedGuids = rows.Select(r => (Guid)r[0]).ToList();
                    }
                }
            }

            return raum;
        }





        private void CreateInnenflaechen(RootDB data, ref BauModel bauModel)
        {

            TableDB tableDB = data.TableDBs.GetTable(Names.TypValueEnum.Innenflaeche.ToString());
            foreach (DataRow row in tableDB.Table.Rows)
            {
                Guid rhObjGuid = (Guid)row[0];
                Innenflaeche innenflaeche = CreateInnenflaeche(rhObjGuid, data);
                if (innenflaeche != null) bauModel.Innenflaechen.Add(innenflaeche);
            }

        }

        private Innenflaeche CreateInnenflaeche(Guid rhObjGuid, RootDB data)
        {
            Innenflaeche innenflaeche = new Innenflaeche(rhObjGuid);

            TableDB tableDB = data.TableDBs.GetTable(Names.TypValueEnum.Innenflaeche.ToString());

            DataRow row = tableDB.Table.Rows.Cast<DataRow>().Where(r => (Guid)r[0] == rhObjGuid).FirstOrDefault();
            if (row == null) return default;

            if (tableDB is RhinoTableDB rhinoTable)
            {
                innenflaeche.Bauteil = (string)GetValueFromRow(row, Names.InnenflaecheAttributeEnum.Bauteil.ToString(), typeof(string));
                innenflaeche.Zone = (string)GetValueFromRow(row, Names.InnenflaecheAttributeEnum.Zone.ToString(), typeof(string));

/*                innenflaeche.FlaecheBrutto = (double?)GetValueFromRow(row, Names.InnenflaecheAttributeEnum.FlaecheBrutto.ToString(), typeof(double?));
                innenflaeche.FlaecheNetto = (double?)GetValueFromRow(row, Names.InnenflaecheAttributeEnum.FlaecheNetto.ToString(), typeof(double?));*/

                innenflaeche.Orientierung = (string)GetValueFromRow(row, Names.InnenflaecheAttributeEnum.Orientierung.ToString(), typeof(string));
                innenflaeche.Neigung = (double?)GetValueFromRow(row, Names.InnenflaecheAttributeEnum.Neigung.ToString(), typeof(double?));

                innenflaeche.Richtung = (int?)GetValueFromRow(row, Names.InnenflaecheAttributeEnum.Richtung.ToString(), typeof(int?));
                innenflaeche.Verschiebung = (double?)GetValueFromRow(row, Names.InnenflaecheAttributeEnum.Verschiebung.ToString(), typeof(double?));
                innenflaeche.Richtung_GUID = null;

                innenflaeche.Grundflaeche = (bool?)GetValueFromRow(row, Names.InnenflaecheAttributeEnum.Grundflaeche.ToString(), typeof(bool?));
                innenflaeche.Fassadenflaeche = (bool?)GetValueFromRow(row, Names.InnenflaecheAttributeEnum.Fassadenflaeche.ToString(), typeof(bool?));


/*                List<Guid> richtungGuids = GetGuidListFromAttribute(row, Names.InnenflaecheAttributeEnum.RichtungsObjekt.ToString());
                if (richtungGuids != null) innenflaeche.Richtung_GUID = richtungGuids[0];*/

                List<Guid> refGuids = DataTableHelpers.GetGuidListFromAttribute(row, Names.InnenflaecheAttributeEnum.Referenzflaeche.ToString());
                if (refGuids != null) innenflaeche.Referenzflaeche = CreateReferenceflaeche(refGuids[0], data);

                //List<Guid> fassadeGuids = GetGuidListFromAttribute(row, Names.InnenflaecheAttributeEnum.Fassade.ToString());
                //if (fassadeGuids != null)
                innenflaeche.Fassade = CreateFassade(rhObjGuid, data);
            }

            return innenflaeche;

        }

        private Referenzflaeche CreateReferenceflaeche(Guid rhObjGuid, RootDB data)
        {
            Referenzflaeche refFlaeche = new Referenzflaeche(rhObjGuid);

            TableDB tableDB = data.TableDBs.GetTable(Names.TypValueEnum.Referenzflaeche.ToString());

            DataRow row = tableDB.Table.Rows.Cast<DataRow>().Where(r => (Guid)r[0] == rhObjGuid).FirstOrDefault();
            if (row == null) return default;

            return refFlaeche;
        }

        public Fassade CreateFassade(Guid rhObjGuid, RootDB data)
        {
            Fassade fassade = new Fassade(rhObjGuid);

            TableDB tableDB = data.TableDBs.GetTable(Names.TypValueEnum.Innenflaeche.ToString());

            DataRow row = tableDB.Table.Rows.Cast<DataRow>().Where(r => (Guid)r[0] == rhObjGuid).FirstOrDefault();
            if (row == null) return default;

            if (tableDB is RhinoTableDB rhinoTable)
            {
                fassade.R_wges = (double?)GetValueFromRow(row, Names.FassadeAttributeEnum.Rwges.ToString(), typeof(double?));

                List<Guid> windowGuids = DataTableHelpers.GetGuidListFromAttribute(row, Names.TypValueEnum.Fenster.ToString());
                if (windowGuids != null && windowGuids.Count > 0)
                {
                    foreach(Guid guid in windowGuids)
                    {
                        Fenster fenster = CreateFenster(guid, data);
                        if (fenster != null) fassade.Fensters.Add(fenster);
                    }
                    
                }


                List<Guid> einbauteilGuids = DataTableHelpers.GetGuidListFromAttribute(row, Names.TypValueEnum.Einbauteil.ToString());
                if (einbauteilGuids != null && einbauteilGuids.Count > 0)
                {
                    foreach (Guid guid in einbauteilGuids)
                    {
                        Einbauteil einbauteil = CreateEinbauteil(guid, data);
                        if (einbauteil != null) fassade.Einbauteils.Add(einbauteil);
                    }

                }

                List<Guid> nebkonstGuids = DataTableHelpers.GetGuidListFromAttribute(row, Names.TypValueEnum.Nebenkonstruktion.ToString());
                if (nebkonstGuids != null && nebkonstGuids.Count > 0)
                {
                    foreach (Guid guid in nebkonstGuids)
                    {
                        Nebenkonstruktion nebenKonstruktion = CreateNebenKonstruktion(guid, data);
                        if (nebenKonstruktion != null) fassade.Nebenkonstruktions.Add(nebenKonstruktion);
                    }

                }


                List<Guid> hauptkonstrGuids = DataTableHelpers.GetGuidListFromAttribute(row, Names.TypValueEnum.Hauptkonstruktion.ToString());
                if (hauptkonstrGuids != null && hauptkonstrGuids.Count > 0)
                        fassade.Hauptkonstruktion = CreateHauptkonstruktion(hauptkonstrGuids[0], data);

            }

            return fassade;

        }

        private Hauptkonstruktion CreateHauptkonstruktion(Guid rhObjGuid, RootDB data)
        {
            Hauptkonstruktion obj = new Hauptkonstruktion(rhObjGuid);

            TableDB tableDB = data.TableDBs.GetTable(Names.TypValueEnum.Hauptkonstruktion.ToString());

            DataRow row = tableDB.Table.Rows.Cast<DataRow>().Where(r => (Guid)r[0] == rhObjGuid).FirstOrDefault();
            if (row == null) return default;

            if (tableDB is RhinoTableDB rhinoTable)
            {
                obj.Reiw = (double?)GetValueFromRow(row, Names.FassadenElementAttributeEnum.Reiw.ToString(), typeof(double?));

                obj.R_iw = (double?)GetValueFromRow(row, Names.KonstruktionAttributeEnum.Riw.ToString(), typeof(double?));
                obj.UWert = (double?)GetValueFromRow(row, Names.KonstruktionAttributeEnum.UWert.ToString(), typeof(double?));
            }

            RhinoObject rhobj = obj.GetRhinoObject();
            if (rhobj != null)
            {
                obj.U = (double?)DataHelpers.ConvertFromString(TryGetUserString(rhobj, Names.FassadenElementAttributeEnum.U.ToString()), typeof(double?), false);
                obj.V = (double?)DataHelpers.ConvertFromString(TryGetUserString(rhobj, Names.FassadenElementAttributeEnum.V.ToString()), typeof(double?), false);
                obj.IsAbstract = (bool?)DataHelpers.ConvertFromString(TryGetUserString(rhobj, Names.FassadenElementAttributeEnum.IsAbstract.ToString()), typeof(bool?), false);
            }

            return obj;
        }

        private Nebenkonstruktion CreateNebenKonstruktion(Guid rhObjGuid, RootDB data)
        {
            Nebenkonstruktion obj = new Nebenkonstruktion(rhObjGuid);

            TableDB tableDB = data.TableDBs.GetTable(Names.TypValueEnum.Nebenkonstruktion.ToString());

            DataRow row = tableDB.Table.Rows.Cast<DataRow>().Where(r => (Guid)r[0] == rhObjGuid).FirstOrDefault();
            if (row == null) return default;

            if (tableDB is RhinoTableDB rhinoTable)
            {
                obj.Flaeche = (double?)GetValueFromRow(row, Names.FassadenElementAttributeEnum.Flaeche.ToString(), typeof(double?));
                obj.Reiw = (double?)GetValueFromRow(row, Names.FassadenElementAttributeEnum.Reiw.ToString(), typeof(double?));

                obj.R_iw = (double?)GetValueFromRow(row, Names.KonstruktionAttributeEnum.Riw.ToString(), typeof(double?));
                obj.UWert = (double?)GetValueFromRow(row, Names.KonstruktionAttributeEnum.UWert.ToString(), typeof(double?));
            }

            RhinoObject rhobj = obj.GetRhinoObject();
            if (rhobj != null)
            {
                obj.U = (double?)DataHelpers.ConvertFromString(TryGetUserString(rhobj, Names.FassadenElementAttributeEnum.U.ToString()), typeof(double?), false);
                obj.V = (double?)DataHelpers.ConvertFromString(TryGetUserString(rhobj, Names.FassadenElementAttributeEnum.V.ToString()), typeof(double?), false);
                obj.IsAbstract = (bool?)DataHelpers.ConvertFromString(TryGetUserString(rhobj, Names.FassadenElementAttributeEnum.IsAbstract.ToString()), typeof(bool?), false);
            }

            return obj;
        }

        private Einbauteil CreateEinbauteil(Guid rhObjGuid, RootDB data)
        {
            Einbauteil obj = new Einbauteil(rhObjGuid);

            TableDB tableDB = data.TableDBs.GetTable(Names.TypValueEnum.Einbauteil.ToString());

            DataRow row = tableDB.Table.Rows.Cast<DataRow>().Where(r => (Guid)r[0] == rhObjGuid).FirstOrDefault();
            if (row == null) return default;

            if (tableDB is RhinoTableDB rhinoTable)
            {
                obj.Reiw = (double?)GetValueFromRow(row, Names.FassadenElementAttributeEnum.Reiw.ToString(), typeof(double?));

                obj.D_newlab = (double?)GetValueFromRow(row, Names.EinbauteilAttributeEnum.DnewLab.ToString(), typeof(double?));
                obj.L_lab = (double?)GetValueFromRow(row, Names.EinbauteilAttributeEnum.lLab.ToString(), typeof(double?));
                obj.L_situ = (double?)GetValueFromRow(row, Names.EinbauteilAttributeEnum.lSitu.ToString(), typeof(double?));
                obj.Anzahl = (int?)GetValueFromRow(row, Names.EinbauteilAttributeEnum.Anzahl.ToString(), typeof(int?));
            }

            RhinoObject rhobj = obj.GetRhinoObject();
            if (rhobj != null)
            {
                obj.U = (double?)DataHelpers.ConvertFromString(TryGetUserString(rhobj, Names.FassadenElementAttributeEnum.U.ToString()), typeof(double?), false);
                obj.V = (double?)DataHelpers.ConvertFromString(TryGetUserString(rhobj, Names.FassadenElementAttributeEnum.V.ToString()), typeof(double?), false);
                obj.IsAbstract = (bool?)DataHelpers.ConvertFromString(TryGetUserString(rhobj, Names.FassadenElementAttributeEnum.IsAbstract.ToString()), typeof(bool?), false);
            }

            return obj;
        }

        public Fenster CreateFenster(Guid rhObjGuid, RootDB data)
        {
            Fenster obj = new Fenster(rhObjGuid);

            TableDB tableDB = data.TableDBs.GetTable(Names.TypValueEnum.Fenster.ToString());

            DataRow row = tableDB.Table.Rows.Cast<DataRow>().Where(r => (Guid)r[0] == rhObjGuid).FirstOrDefault();
            if (row == null) return default;

            if (tableDB is RhinoTableDB rhinoTable)
            {
                obj.Reiw = (double?)GetValueFromRow(row, Names.FassadenElementAttributeEnum.Reiw.ToString(), typeof(double?));

                obj.Riw = (double?)GetValueFromRow(row, Names.FensterAttributeEnum.Riw.ToString(), typeof(double?));
                obj.Hoehe = (double?)GetValueFromRow(row, Names.FensterAttributeEnum.Hoehe.ToString(), typeof(double?));
                obj.Breite = (double?)GetValueFromRow(row, Names.FensterAttributeEnum.Breite.ToString(), typeof(double?));
                obj.Anzahl = (int?)GetValueFromRow(row, Names.FensterAttributeEnum.Anzahl.ToString(), typeof(int?));
                obj.Flaeche = (double?)GetValueFromRow(row, Names.FensterAttributeEnum.Flaeche.ToString(), typeof(double?));
                obj.UWert = (double?)GetValueFromRow(row, Names.FensterAttributeEnum.UWert.ToString(), typeof(double?));
                obj.GWert = (double?)GetValueFromRow(row, Names.FensterAttributeEnum.gWert.ToString(), typeof(double?));
                obj.Bruestungshoehe = (double?)GetValueFromRow(row, Names.FensterAttributeEnum.Bruestungshoehe.ToString(), typeof(double?));
            }

            RhinoObject rhobj = obj.GetRhinoObject();
            if (rhobj != null)
            {
                obj.U = (double?)DataHelpers.ConvertFromString(TryGetUserString(rhobj, Names.FassadenElementAttributeEnum.U.ToString()), typeof(double?), false);
                obj.V = (double?)DataHelpers.ConvertFromString(TryGetUserString(rhobj, Names.FassadenElementAttributeEnum.V.ToString()), typeof(double?), false);
                obj.IsAbstract = (bool?)DataHelpers.ConvertFromString(TryGetUserString(rhobj, Names.FassadenElementAttributeEnum.IsAbstract.ToString()), typeof(bool?), false);
            }

            return obj;
        }


        #endregion

        #region Write

        public void Write(BauModel model, ref RootDB data)
        {

            if (model.Innenflaechen != null && model.Innenflaechen.Count >0)
                foreach (Innenflaeche innenflaeche in model.Innenflaechen)
                    WriteInnenflaeche(innenflaeche, ref data);

            if (model.Raume != null && model.Raume.Count > 0)
                foreach (Raum raum in model.Raume)
                    WriteRaum(raum, model.Innenflaechen, ref data);

            if (model.Bauteile != null && model.Bauteile.Count > 0)
                foreach (Bauteil bauteil in model.Bauteile)
                    WriteBauteil(bauteil, ref data);

            if (model.Raumkategorien != null && model.Raumkategorien.Count > 0)
                foreach (Raumkategorie raumKategorie in model.Raumkategorien)
                    WriteRaumkategorie(raumKategorie, ref data);

        }




        private void WriteRaumkategorie(Raumkategorie obj, ref RootDB data)
        {
            TableDB tableDB = data.TableDBs.GetTable(Helpers.Names.TabellenNameEnum.Raumkategorie.ToString());
            if (tableDB == null) return;

            DataRow row = tableDB.GetRowByID(obj.ObjectGuid);
            if (row == null) tableDB.AddRow(obj.ObjectGuid, obj.Name);

            tableDB.WriteValue(obj.ObjectGuid, Names.RaumkategorieEnum.Raumkategorie.ToString(), obj.Name);
            tableDB.WriteValue(obj.ObjectGuid, Names.RaumkategorieEnum.KWert.ToString(), obj.K);
            tableDB.WriteValue(obj.ObjectGuid, Names.RaumkategorieEnum.InnenTag.ToString(), obj.liTag);
            tableDB.WriteValue(obj.ObjectGuid, Names.RaumkategorieEnum.InnenNacht.ToString(), obj.liNacht);
        }


        private void WriteBauteil(Bauteil obj, ref RootDB data)
        {
            TableDB tableDB = data.TableDBs.GetTable(Helpers.Names.TabellenNameEnum.Bauteil.ToString());
            if (tableDB == null) return;

            DataRow row = tableDB.GetRowByID(obj.ObjectGuid);
            if (row == null) tableDB.AddRow(obj.ObjectGuid, obj.Name);

            tableDB.WriteValue(obj.ObjectGuid, Names.BauteilAttributeEnum.Bauteil.ToString(), obj.Name);
            tableDB.WriteValue(obj.ObjectGuid, Names.BauteilAttributeEnum.Bauteilart.ToString(), obj.Art);
            tableDB.WriteValue(obj.ObjectGuid, Names.BauteilAttributeEnum.Dicke.ToString(), obj.Dicke);

        }

        private void WriteRaum(Raum obj, List<Innenflaeche> innenflaechen, ref RootDB data)
        {
            TableDB tableDB = data.TableDBs.GetTable(Helpers.Names.TabellenNameEnum.Raum.ToString());

            if (tableDB == null) return;

            DataRow row = tableDB.GetRowByID(obj.ObjectGuid);
            if (row == null) tableDB.AddRow(obj.ObjectGuid, obj.Raumname);

            tableDB.WriteValue(obj.ObjectGuid, Names.RaumAttributesEnum.Raumgruppe.ToString(), obj.Raumname);
            tableDB.WriteValue(obj.ObjectGuid, Names.RaumAttributesEnum.Raumkategorie.ToString(), obj.Raumkategorie);
            tableDB.WriteValue(obj.ObjectGuid, Names.RaumAttributesEnum.DINLaermpegel.ToString(), obj.DIN_Laermpegel);
            tableDB.WriteValue(obj.ObjectGuid, Names.RaumAttributesEnum.VDIVerkehrTag.ToString(), obj.VDI_Verkehr_Tag);
            tableDB.WriteValue(obj.ObjectGuid, Names.RaumAttributesEnum.VDIVerkehrNacht.ToString(), obj.VDI_Verkehr_Nacht);
            tableDB.WriteValue(obj.ObjectGuid, Names.RaumAttributesEnum.VDIGewerbeTag.ToString(), obj.VDI_Gewerbe_Tag);
            tableDB.WriteValue(obj.ObjectGuid, Names.RaumAttributesEnum.VDIGewerbeNacht.ToString(), obj.VDI_Gewerbe_Nacht);
            tableDB.WriteValue(obj.ObjectGuid, Names.RaumAttributesEnum.RerfDIN.ToString(), obj.Rerf_DIN);
            tableDB.WriteValue(obj.ObjectGuid, Names.RaumAttributesEnum.RerfVDITag.ToString(), obj.Rerf_VDI_Tag);
            tableDB.WriteValue(obj.ObjectGuid, Names.RaumAttributesEnum.RerfVDINacht.ToString(), obj.Rerf_VDI_Nacht);
            tableDB.WriteValue(obj.ObjectGuid, Names.RaumAttributesEnum.RerfMax.ToString(), obj.Rerf_Max);
            tableDB.WriteValue(obj.ObjectGuid, Names.RaumAttributesEnum.ss.ToString(), obj.Ss(innenflaechen, false));
            tableDB.WriteValue(obj.ObjectGuid, Names.RaumAttributesEnum.sg.ToString(), obj.Sg(innenflaechen, false));
        }

        public void WriteInnenflaeche(Innenflaeche obj, ref RootDB data)
        {
            TableDB tableDB = data.TableDBs.GetTable(Names.TypValueEnum.Innenflaeche.ToString());
            if (tableDB == null) return;

            DataRow row = tableDB.GetRowByID(obj.ObjectGuid);
            if (row == null) tableDB.AddRow(obj.ObjectGuid, obj.ObjectGuid.ToString());


            tableDB.WriteValue(obj.ObjectGuid, Names.BauteilAttributeEnum.Bauteil.ToString(), obj.Bauteil);
            tableDB.WriteValue(obj.ObjectGuid, Names.InnenflaecheAttributeEnum.Zone.ToString(), obj.Zone);
            tableDB.WriteValue(obj.ObjectGuid, Names.InnenflaecheAttributeEnum.Bruttoflaeche.ToString(), Math.Round(obj.FlaecheBrutto(), 3));
            tableDB.WriteValue(obj.ObjectGuid, Names.InnenflaecheAttributeEnum.Nettoflaeche.ToString(), Math.Round(obj.FlaecheNetto(), 3));
            tableDB.WriteValue(obj.ObjectGuid, Names.InnenflaecheAttributeEnum.Orientierung.ToString(), obj.Orientierung);
            tableDB.WriteValue(obj.ObjectGuid, Names.InnenflaecheAttributeEnum.Neigung.ToString(), obj.Neigung);
            tableDB.WriteValue(obj.ObjectGuid, Names.InnenflaecheAttributeEnum.Richtung.ToString(), obj.Richtung);
            tableDB.WriteValue(obj.ObjectGuid, Names.InnenflaecheAttributeEnum.Verschiebung.ToString(), obj.Verschiebung);
            tableDB.WriteValue(obj.ObjectGuid, Names.InnenflaecheAttributeEnum.Grundflaeche.ToString(), obj.Grundflaeche);
            tableDB.WriteValue(obj.ObjectGuid, Names.InnenflaecheAttributeEnum.Fassadenflaeche.ToString(), obj.Fassadenflaeche);


            if (obj.Referenzflaeche != null)
            {
                tableDB.WriteValue(obj.ObjectGuid, Names.InnenflaecheAttributeEnum.Referenzflaeche.ToString(), obj.Referenzflaeche.ObjectGuid);
                WriteReferenzflaeche(obj.Referenzflaeche, obj, ref data);
            }

            if (obj.Fassade != null)
            {
                tableDB.WriteValue(obj.ObjectGuid, Names.InnenflaecheAttributeEnum.Fassade.ToString(), obj.Fassade.ObjectGuid);
                WriteFassade(obj.Fassade, obj, ref data);
            }
        }

        private void WriteFassade(Fassade obj, Innenflaeche relatedInnenflaeche, ref RootDB data)
        {
            TableDB tableDB = data.TableDBs.GetTable(Names.TypValueEnum.Innenflaeche.ToString());
            if (tableDB == null) return;

            DataRow row = tableDB.GetRowByID(obj.ObjectGuid);
            if (row == null) tableDB.AddRow(obj.ObjectGuid, obj.ObjectGuid.ToString());

            tableDB.WriteValue(obj.ObjectGuid, Names.FassadeAttributeEnum.Rwges.ToString(), obj.R_wges);

            if (obj.Fensters != null && obj.Fensters.Count > 0)
            {
                foreach (Fenster fenster in obj.Fensters)
                    WriteFenster(fenster, relatedInnenflaeche, ref data);

                List<Guid> guids = obj.Fensters.Select(f => f.ObjectGuid).ToList();
                tableDB.WriteValue(obj.ObjectGuid, Names.FassadeAttributeEnum.Fenster.ToString(), guids);
            }
                

            if (obj.Einbauteils != null && obj.Einbauteils.Count > 0)
            {
                foreach (Einbauteil einbauteil in obj.Einbauteils)
                    WriteEinbauteil(einbauteil, ref data);

                List<Guid> guids = obj.Einbauteils.Select(f => f.ObjectGuid).ToList();
                tableDB.WriteValue(obj.ObjectGuid, Names.FassadeAttributeEnum.Einbauteil.ToString(), guids);
            }

            if (obj.Nebenkonstruktions != null && obj.Nebenkonstruktions.Count > 0)
            {
                foreach (Nebenkonstruktion nebenkonstruktion in obj.Nebenkonstruktions)
                    WriteNebenkonstruktion(nebenkonstruktion, ref data);

                List<Guid> guids = obj.Nebenkonstruktions.Select(f => f.ObjectGuid).ToList();
                tableDB.WriteValue(obj.ObjectGuid, Names.FassadeAttributeEnum.Nebenkonstruktion.ToString(), guids);
            }
                
            if (obj.Hauptkonstruktion != null)
            {
                WriteHauptkonstruktion(obj.Hauptkonstruktion, relatedInnenflaeche, ref data);
                tableDB.WriteValue(obj.ObjectGuid, Names.FassadeAttributeEnum.Hauptkonstruktion.ToString(), obj.Hauptkonstruktion.ObjectGuid);
            }

        }

        private void WriteHauptkonstruktion(Hauptkonstruktion obj, Innenflaeche relatedInnenflaeche, ref RootDB data)
        {
            TableDB tableDB = data.TableDBs.GetTable(Names.TypValueEnum.Hauptkonstruktion.ToString());
            if (tableDB == null) return;

            DataRow row = tableDB.GetRowByID(obj.ObjectGuid);
            if (row == null) tableDB.AddRow(obj.ObjectGuid, obj.ObjectGuid.ToString());

            tableDB.WriteValue(obj.ObjectGuid, Names.FassadenElementAttributeEnum.Flaeche.ToString(), Math.Round(obj.Flaeche(relatedInnenflaeche), 3));
            tableDB.WriteValue(obj.ObjectGuid, Names.FassadenElementAttributeEnum.Reiw.ToString(), obj.Reiw);
            tableDB.WriteValue(obj.ObjectGuid, Names.KonstruktionAttributeEnum.Riw.ToString(), obj.R_iw);
            tableDB.WriteValue(obj.ObjectGuid, Names.KonstruktionAttributeEnum.UWert.ToString(), obj.UWert);

            RhinoObject rhobj = obj.GetRhinoObject();
            if (rhobj != null)
            {
                SetUserString(rhobj, Helpers.Names.FassadenElementAttributeEnum.U.ToString(), DataHelpers.ConvertToDataTableString(obj.U));
                SetUserString(rhobj, Helpers.Names.FassadenElementAttributeEnum.V.ToString(), DataHelpers.ConvertToDataTableString(obj.V));
                SetUserString(rhobj, Helpers.Names.FassadenElementAttributeEnum.IsAbstract.ToString(), DataHelpers.ConvertToDataTableString(obj.IsAbstract));
            }
        }

        private void WriteNebenkonstruktion(Nebenkonstruktion obj, ref RootDB data)
        {
            TableDB tableDB = data.TableDBs.GetTable(Names.TypValueEnum.Nebenkonstruktion.ToString());
            if (tableDB == null) return;

            DataRow row = tableDB.GetRowByID(obj.ObjectGuid);
            if (row == null) tableDB.AddRow(obj.ObjectGuid, obj.ObjectGuid.ToString());

            tableDB.WriteValue(obj.ObjectGuid, Names.FassadenElementAttributeEnum.Flaeche.ToString(), obj.Flaeche);
            tableDB.WriteValue(obj.ObjectGuid, Names.FassadenElementAttributeEnum.Reiw.ToString(), obj.Reiw);
            tableDB.WriteValue(obj.ObjectGuid, Names.KonstruktionAttributeEnum.Riw.ToString(), obj.R_iw);
            tableDB.WriteValue(obj.ObjectGuid, Names.KonstruktionAttributeEnum.UWert.ToString(), obj.UWert);

            RhinoObject rhobj = obj.GetRhinoObject();
            if (rhobj != null)
            {
                SetUserString(rhobj, Helpers.Names.FassadenElementAttributeEnum.U.ToString(), DataHelpers.ConvertToDataTableString(obj.U));
                SetUserString(rhobj, Helpers.Names.FassadenElementAttributeEnum.V.ToString(), DataHelpers.ConvertToDataTableString(obj.V));
                SetUserString(rhobj, Helpers.Names.FassadenElementAttributeEnum.IsAbstract.ToString(), DataHelpers.ConvertToDataTableString(obj.IsAbstract));
            }

        }

        private void WriteEinbauteil(Einbauteil obj, ref RootDB data)
        {
            TableDB tableDB = data.TableDBs.GetTable(Names.TypValueEnum.Einbauteil.ToString());
            if (tableDB == null) return;

            DataRow row = tableDB.GetRowByID(obj.ObjectGuid);
            if (row == null) tableDB.AddRow(obj.ObjectGuid, obj.ObjectGuid.ToString());

            tableDB.WriteValue(obj.ObjectGuid, Names.FassadenElementAttributeEnum.Reiw.ToString(), obj.Reiw);
            tableDB.WriteValue(obj.ObjectGuid, Names.EinbauteilAttributeEnum.DnewLab.ToString(), obj.D_newlab);
            tableDB.WriteValue(obj.ObjectGuid, Names.EinbauteilAttributeEnum.lLab.ToString(), obj.L_lab);
            tableDB.WriteValue(obj.ObjectGuid, Names.EinbauteilAttributeEnum.lSitu.ToString(), obj.L_situ);
            tableDB.WriteValue(obj.ObjectGuid, Names.EinbauteilAttributeEnum.Anzahl.ToString(), obj.Anzahl);

            RhinoObject rhobj = obj.GetRhinoObject();
            if (rhobj != null)
            {
                SetUserString(rhobj, Helpers.Names.FassadenElementAttributeEnum.U.ToString(), DataHelpers.ConvertToDataTableString(obj.U));
                SetUserString(rhobj, Helpers.Names.FassadenElementAttributeEnum.V.ToString(), DataHelpers.ConvertToDataTableString(obj.V));
                SetUserString(rhobj, Helpers.Names.FassadenElementAttributeEnum.IsAbstract.ToString(), DataHelpers.ConvertToDataTableString(obj.IsAbstract));
            }
        }

        public void WriteFenster(Fenster obj, Innenflaeche relatedInnenflaeche, ref RootDB data)
        {
            TableDB tableDB = data.TableDBs.GetTable(Names.TypValueEnum.Fenster.ToString());
            if (tableDB == null) return;

            DataRow row = tableDB.GetRowByID(obj.ObjectGuid);
            if (row == null) tableDB.AddRow(obj.ObjectGuid, obj.ObjectGuid.ToString());

            tableDB.WriteValue(obj.ObjectGuid, Names.FassadenElementAttributeEnum.Reiw.ToString(), obj.Reiw);
            tableDB.WriteValue(obj.ObjectGuid, Names.FensterAttributeEnum.Riw.ToString(), obj.Riw);
            tableDB.WriteValue(obj.ObjectGuid, Names.FensterAttributeEnum.Hoehe.ToString(), obj.Hoehe);
            tableDB.WriteValue(obj.ObjectGuid, Names.FensterAttributeEnum.Breite.ToString(), obj.Breite);
            tableDB.WriteValue(obj.ObjectGuid, Names.FensterAttributeEnum.Durchmesser.ToString(), obj.Durchmesser);
            tableDB.WriteValue(obj.ObjectGuid, Names.FensterAttributeEnum.Anzahl.ToString(), obj.Anzahl);

            tableDB.WriteValue(obj.ObjectGuid, Names.FensterAttributeEnum.Flaeche.ToString(), Math.Round(obj.FlaecheGesamt(),3));
            tableDB.WriteValue(obj.ObjectGuid, Names.FensterAttributeEnum.UWert.ToString(), obj.UWert);
            tableDB.WriteValue(obj.ObjectGuid, Names.FensterAttributeEnum.gWert.ToString(), obj.GWert);
            tableDB.WriteValue(obj.ObjectGuid, Names.FensterAttributeEnum.Bruestungshoehe.ToString(), obj.Bruestungshoehe);

            RhinoObject rhobj = obj.GetRhinoObject();
            if (rhobj != null)
            {
                SetUserString(rhobj, Helpers.Names.FassadenElementAttributeEnum.U.ToString(), DataHelpers.ConvertToDataTableString(obj.U));
                SetUserString(rhobj, Helpers.Names.FassadenElementAttributeEnum.V.ToString(), DataHelpers.ConvertToDataTableString(obj.V));
                SetUserString(rhobj, Helpers.Names.FassadenElementAttributeEnum.IsAbstract.ToString(), DataHelpers.ConvertToDataTableString(obj.IsAbstract));
            }

        }

        private void WriteReferenzflaeche(Referenzflaeche obj, Innenflaeche innenflaeche, ref RootDB data)
        {
            TableDB tableDB = data.TableDBs.GetTable(Names.TypValueEnum.Referenzflaeche.ToString());
            if (tableDB == null) return;

            DataRow row = tableDB.GetRowByID(obj.ObjectGuid);
            if (row == null) tableDB.AddRow(obj.ObjectGuid, obj.ObjectGuid.ToString());

            tableDB.WriteValue(obj.ObjectGuid, Names.InnenflaecheAttributeEnum.Bruttoflaeche.ToString(), Math.Round(obj.FlaecheBrutto(), 3));
            tableDB.WriteValue(obj.ObjectGuid, Names.InnenflaecheAttributeEnum.Nettoflaeche.ToString(), Math.Round(obj.FlaecheNetto(innenflaeche), 3));

        }

        #endregion



        private object GetValueFromRow(DataRow row, string str, Type type)
        {
            return LayerManager.Helpers.DataTableHelpers.GetValueFromRow(row, str, type);
        }

        private string TryGetUserString(RhinoObject rhObj, string str)
        {
            return LayerManager.Helpers.RhinoHelpers.TryGetUserString(rhObj, str);
        }

        private void SetUserString(RhinoObject rhObj, string str, object value)
        {
            LayerManager.Helpers.RhinoHelpers.SetUserString(rhObj, str, value);
        }
    }
}
