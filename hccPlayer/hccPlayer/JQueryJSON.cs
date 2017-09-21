using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hccPlayer
{    
    public class JQueryJSON
    {
        public string schema { get; set; }
        public Step[] step { get; set; }
    }

    public class Step
    {
        public string function { get; set; }
        public Arg[] args { get; set; }
        public bool isQuery { get; set; }
    }


    public class Arg
    {
        public string value { get; set; }
        public Step step { get; set; }        
    }
}

