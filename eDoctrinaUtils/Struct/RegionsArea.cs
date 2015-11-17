using System.Drawing;

namespace eDoctrinaUtils
{
    public struct RegionsArea
    {
        public string type { get; set; }
        public int left { get; set; }
        public int top { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int indexOfFirstBubble { get; set; }//можно добавить в каждую арею
        public int lineHeight { get; set; }
        public int bubblesPerLine { get; set; }
        public int subLinesAmount { get; set; }
        public int subLineHeight { get; set; }
        public int questionIndex { get; set; }
        public string bubblesFormat { get; set; }
        public string bubbleLines { get; set; }
        public string bubblesOrientation { get; set; }
        public Rectangle bubble { get; set; }
    }
}
