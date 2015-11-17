using System;

namespace eDoctrinaUtils
{
    public struct Region
    {
        public string name { get; set; }
        public string type { get; set; }
        public int outputPosition { get; set; }
        public int indexOutputPosition { get; set; }
        public string value { get; set; }
        //public int indexOfFirstBubble { get; set; }//можно добавить в каждый регион
        public int subLinesAmount { get; set; }
        public int subLineHeight { get; set; }
        public int rotate { get; set; }
        public double? percent_confident_text_region { get; set; }
        public RegionsArea[] areas { get; set; }
        public bool? active { get; set; }
        public string QRCodeFormat { get; set; }
    }
    ////-------------------------------------------------------------------------
    //public struct QRCodeFormatItem
    //{
    //    public string key { get; set; }
    //    public char value { get; set; }
    //}
    //-------------------------------------------------------------------------
    //-------------------------------------------------------------------------
    //-------------------------------------------------------------------------
}
