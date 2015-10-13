using System.Drawing;

namespace eDoctrinaUtils
{
    public class BubbleItem : NotifyPropertyHelper
    {
        public BubbleItem()
        { }

        public BubbleItem(Bubble bubble, CheckedBubble checkedBubble)
        { 
            Bubble = bubble;
            CheckedBubble = checkedBubble;
        }
        
        private string name;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                if (name != value)
                {
                    name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        private Bubble bubble;
        public Bubble Bubble
        {
            get
            {
                return bubble;
            }
            set
            {
                //if (bubble != value)
                {
                    bubble = value;
                    NotifyPropertyChanged("Bubble");
                }
            }
        }

        private CheckedBubble checkedBubble;
        public CheckedBubble CheckedBubble
        {
            get
            {
                return checkedBubble;
            }
            set
            {
                if (checkedBubble != value)
                {
                    checkedBubble = value;
                    NotifyPropertyChanged("CheckedBubble");
                }
            }
        }

        public Color BorderColor
        {
            get
            {
                System.Drawing.Color color = System.Drawing.SystemColors.Control;
                if (BorderColorOpacity != 255) color = System.Drawing.Color.Blue;
                else color = System.Drawing.SystemColors.Control;
                return color;
            }
        }

        private byte borderColorOpacity = 255;
        public byte BorderColorOpacity
        {
            get
            {
                return borderColorOpacity;
            }
            set
            {
                if (borderColorOpacity != value)
                {
                    borderColorOpacity = value;
                    NotifyPropertyChanged("BorderColorOpacity");
                    NotifyPropertyChanged("BorderColor");
                }
            }
        }
    }
}
