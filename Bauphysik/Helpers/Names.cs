using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bauphysik.Helpers
{
    public abstract class Names
    {
        public const string shift = "      ";
        public const string shift2 = shift + shift;
        public const string shift3 = shift2 + shift;
        public const string shift4 = shift3 + shift;

        public const string warning = "!!! ";
        public const string cancelCalc = "  => Berechnung abgebrochen";

        public const string dashSepShort = "---";
        public const string dashSep = "--------------------------";
        public const string hashSep = "##########################";

        public enum RaumkategorieEnum
        {
            Raumkategorie,
            KWert,
            InnenTag,
            InnenNacht,
            Gewerbe,
            Wohnung
        }

        public enum BauteilArtEnum
        {
            IW,
            AW,
            D
        }

        public enum TabellenNameEnum
        {
            Bauteil,
            Raum,
            Raumkategorie
        }

        public enum BauteilAttributeEnum
        {
            Bauteil,
            Bauteilart,
            Dicke
        }

        public enum RhinoElementAttributeEnum
        {
            Element
        }

        public enum TypValueEnum
        {
            Fenster,
            Einbauteil,
            Fassade,
            Nebenkonstruktion,
            Hauptkonstruktion,
            Innenflaeche,
            Referenzflaeche
        }

        public enum InnenflaecheAttributeEnum
        {
            Bauteil,
            Zone,
            Bruttoflaeche,
            Nettoflaeche,
            Orientierung,
            Neigung,
            Richtung,
            Verschiebung,
            RichtungsObjekt,
            Grundflaeche,
            Fassadenflaeche,
            Fassade,
            Referenzflaeche,
        }

        public enum Referenzflaeche
        {
            FlaecheBrutto,
            FlaecheNetto,
        }

        public enum FassadeAttributeEnum
        {
            Rwges,
            Fenster,
            Einbauteil,
            Nebenkonstruktion,
            Hauptkonstruktion
        }

        public enum FassadeElementEnum
        {
            Fenster,
            Nebenkonstruktion,
            Hauptkonstruktion
        }

        public enum FassadenElementAttributeEnum
        {
            Reiw,
            Flaeche,
            U,
            V,
            IsAbstract
        }


        public enum FensterAttributeEnum
        {
            Hoehe,
            Breite,
            Durchmesser,
            Anzahl,
            Flaeche,
            gWert,
            Bruestungshoehe,
            Riw,
            UWert,
            EingabeTyp,
        }

        public enum KonstruktionAttributeEnum
        {
            Riw,
            UWert,
        }

        public enum EinbauteilAttributeEnum
        {
            Anzahl,
            DnewLab,
            lLab,
            lSitu
        }


        public enum RaumAttributesEnum
        {
            Raum,
            Raumgruppe,
            Raumkategorie,
            DINLaermpegel,
            VDIVerkehrTag,
            VDIVerkehrNacht,
            VDIGewerbeTag,
            VDIGewerbeNacht,
            RerfDIN,
            RerfVDITag,
            RerfVDINacht,
            RerfMax,
            ss,
            sg,
            RelatedGuids,
        }
    }
}
