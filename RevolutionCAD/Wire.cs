using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionCAD
{
    public class Wire
    {
        public Contact A { get; set; }
        public Contact B { get; set; }

        public Wire()
        {
            A = new Contact();
            B = new Contact();
        }
    }
}
