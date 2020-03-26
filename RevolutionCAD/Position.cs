using RevolutionCAD.Tracing;
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

        public bool Equals(Position p)
        {
            if (p.Column == this.Column && p.Row == this.Row)
                return true;
            else
                return false;
        }

        public bool isInDRP(Matrix<Cell> drp)
        {
            if (Row < drp.RowsCount && Row >= 0 && Column < drp.ColsCount && Column >= 0)
                return true;
            else return false;
        }
    }
}
