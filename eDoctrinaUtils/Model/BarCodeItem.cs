using System.Drawing;

namespace eDoctrinaUtils
{
    public class BarCodeItem : NotifyPropertyHelper
    {
        public BarCodeItem()
        { }

        public BarCodeItem(string name, string textType, string barcode, string barcodeMem, Rectangle rectangle)
        {
            Name = name;
            TextType = textType;
            Rectangle = rectangle;
            Barcode = barcode;
            BarcodeMem = barcodeMem;
            VerifyValue();
        }

        public BarCodeItem(string name, string textType, bool verify)
        {
            Name = name;
            TextType = textType;
            Verify = verify;
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

        private string barCodeValue;
        public string Value
        {
            get
            {
                return barCodeValue;
            }
            set
            {
                if (barCodeValue != value)
                {
                    barCodeValue = value;
                    NotifyPropertyChanged("Value");
                    NotifyPropertyChanged("BorderColor");
                }
            }
        }

        private string barcode;
        public string Barcode
        {
            get
            {
                return barcode;
            }
            set
            {
                if (barcode != value)
                {
                    barcode = value;
                    NotifyPropertyChanged("Barcode");
                }
            }
        }

        private string barcodeMem;
        public string BarcodeMem
        {
            get
            {
                return barcodeMem;
            }
            set
            {
                if (barcodeMem != value)
                {
                    barcodeMem = value;
                    NotifyPropertyChanged("BarcodeMem");
                }
            }
        }

        private string textType;
        public string TextType
        {
            get
            {
                return textType;
            }
            set
            {
                if (textType != value)
                {
                    textType = value;
                    NotifyPropertyChanged("TextType");
                }
            }
        }

        private bool verify;
        public bool Verify
        {
            get
            {
                return verify;
            }
            set
            {
                if (verify != value)
                {
                    verify = value;
                    NotifyPropertyChanged("Verify");
                    NotifyPropertyChanged("BorderColor");
                }
            }
        }

        public System.Drawing.Color BorderColor
        {
            get
            {
                if (Verify)
                {
                    return System.Drawing.Color.FromArgb((BorderColorOpacity == 0) ? 255 : 0, System.Drawing.Color.LightGreen);
                }
                if (Value == "" || (Barcode == "" && BarcodeMem == ""))
                {
                    return System.Drawing.Color.FromArgb(BorderColorOpacity, System.Drawing.Color.Red);
                }
                if (Barcode == "" || (BarcodeMem != "" && BarcodeMem != Barcode))
                {
                    return System.Drawing.Color.FromArgb(BorderColorOpacity, System.Drawing.Color.Yellow);
                }
                if (BarcodeMem == "")
                {
                    return System.Drawing.Color.FromArgb(BorderColorOpacity, System.Drawing.Color.Orange);
                }
                return System.Drawing.Color.FromArgb((BorderColorOpacity == 0) ? 255 : 0, System.Drawing.Color.LightGreen);
            }
            //set
            //{
            //    BorderColor = value;
            //}
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

        private Rectangle rectangle;
        public Rectangle Rectangle
        {
            get
            {
                return rectangle;
            }
            set
            {
                if (rectangle != value)
                {
                    rectangle = value;
                    NotifyPropertyChanged("Rectangle");
                }
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public void VerifyValue()
        {
            if (!Verify && Barcode == "" && BarcodeMem == "")
            {
                Value = Barcode;
                return;
            }
            if (!Verify && (Barcode == "" || (BarcodeMem != "" && BarcodeMem != Barcode)))
            {
                Value = BarcodeMem;
                return;
            }
            if (!Verify && BarcodeMem == "")
            {
                Value = Barcode;
                return;
            }
            if (!Verify && BarcodeMem == Barcode)
            {
                Value = Barcode;
                Verify = true;
            }
        }
    }
}
