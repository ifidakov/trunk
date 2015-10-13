using System;
using System.Collections.Generic;

namespace eDoctrinaUtils
{
    [Serializable]
    public class Regions
    {
        [NonSerialized]
        public string SheetIdentifierName;
        //-------------------------------------------------------------------------        
        public string name { get; set; }
        public double heightAndWidthRatio { get; set; }
        public double darknessPercent { get; set; }
        public double? percent_confident_text_region { get; set; }
        public double darknessDifferenceLevel { get; set; }
        public int indexOfFirstBubble { get; set; }
        public List<OutputPositionValue> additionalOutputData { get; set; }
        public string outputFileNameFormat { get; set; }
        public int answersAreasLeft { get; set; }
        public int answersAreasWidth { get; set; }
        public int regionsCount
        {
            set
            {
                regions = new Region[value];
            }
        }
        //-------------------------------------------------------------------------
        public Region[] regions { get; set; }
        //-------------------------------------------------------------------------
        public Region regionsAdd
        {
            set
            {
                if (regions == null)
                {
                    regions = new Region[0];
                }
                Region[] regions2 = new Region[regions.Length + 1];
                regions.CopyTo(regions2, 0);
                regions2[regions.Length] = value;
                regions2[regions.Length].areas = value.areas;
                regions2[regions.Length].name = value.name;
                regions2[regions.Length].type = value.type;
                //regions2[regions.Length].barCodeType = value.barCodeType;
                regions = regions2;
            }
        }
        //-------------------------------------------------------------------------
        public Regions Clone(Regions regions)
        {
            Regions reg = new Regions();
            foreach (var item in regions.regions)
            {
                //reg.regions.a
            }
            return reg;
        }
        //-------------------------------------------------------------------------
    }
}
