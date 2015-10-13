using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eDoctrinaOcrEd
{
    [Serializable]
    public class TestUids
    {
        public struct AreaSettings
        { 
        public int BubblesPrrLine{ get; set; }
        public int SubLineAmout { get; set; }
        }
        public Dictionary<string, AreaSettings[]> Test 
        { 
            get; 
            set; 
        }
        //{
        //    get
        //    {
        //        return Test;
        //    }
        //    set
        //    {
        //        Test = value;
        //    }
        //}
    }
}
