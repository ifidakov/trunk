using System;

namespace eDoctrinaUtils
{
    public class BarcodeEventArgs : EventArgs
    {
        public BarCodeItem BarCode;
        public BarcodeEventArgs(BarCodeItem barCode)
        {
            BarCode = barCode;
        }
    }
}
