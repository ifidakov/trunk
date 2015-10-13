using System.Drawing;
using System.IO;

namespace eDoctrinaUtils
{
    public class MiniatureItem : NotifyPropertyHelper
    {
        private int index;
        public int Index
        {
            get
            {
                return index;
            }
            set
            {
                if (index != value)
                {
                    index = value;
                    NotifyPropertyChanged("Index");
                }
            }
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
        
        private string sheetIdentifierName;
        public string SheetIdentifierName
        {
            get
            {
                return sheetIdentifierName;
            }
            set
            {
                if (sheetIdentifierName != value)
                {
                    sheetIdentifierName = value;
                    NotifyPropertyChanged("SheetIdentifierName");
                }
            }
        }

        private string sheetIdentifierImagePath;
        public string SheetIdentifierImagePath
        {
            get
            {
                return sheetIdentifierImagePath;
            }
            set
            {
                if (sheetIdentifierImagePath != value)
                {
                    sheetIdentifierImagePath = value;
                    NotifyPropertyChanged("SheetIdentifierImagePath");
                    NotifyPropertyChanged("SheetIdentifierImage");
                }
            }
        }

        public Image SheetIdentifierImage
        {
            get
            {
                if (File.Exists(SheetIdentifierImagePath))
                {
                    return Image.FromFile(SheetIdentifierImagePath);
                }
                return Image.FromFile("Miniatures/NoImage.png");
            }
        }

        public override string ToString()
        {
            return SheetIdentifierName;
        }
    }
}