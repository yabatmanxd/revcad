using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionCAD
{
    public class Position
    {
        public int Row { get; set; }
        public int Column { get; set; }

        public Position(int r, int c)
        {
            Row = r;
            Column = c;
        }
        
        public Position Clone()
        {
            return (Position)this.MemberwiseClone();
        }
    }
}
