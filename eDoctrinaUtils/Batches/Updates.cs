using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eDoctrinaUtils
{
    [Serializable]
    public class Updates
    {
        public string version { get; set; }
        public string description { get; set; }
        public string[] files { get; set; }
    }
}
