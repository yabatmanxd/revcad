using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionCAD.Tracing
{
    public class Cell
    {
        /// <summary>
        /// Поле в котором будет хранится состояние ячейки
        /// </summary>
        public CellState State { get; set; }
        public int Weight { get; set; }

        public bool isBusy
        {
            get
            {
                if (State == CellState.WireTopRight ||
                    State == CellState.WireTopLeft ||
                    State == CellState.WireBottomRight ||
                    State == CellState.WireBottomLeft ||
                    State == CellState.WireCross ||
                    State == CellState.Contact)
                    return true;
                else
                    return false;
            }   
        }
    }

    

    public enum CellState
    {
        Empty,
        ArrowUp,
        ArrowDown,
        ArrowLeft,
        ArrowRight,
        WireHorizontal,
        WireVertical,
        WireTopRight,
        WireTopLeft,
        WireBottomRight,
        WireBottomLeft,
        WireCross,
        Contact,
        PointA,
        PointB
    }
}
