using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionCAD.Tracing
{
    [Serializable]
    public class Cell
    {
        /// <summary>
        /// Поле в котором будет хранится состояние ячейки
        /// </summary>
        public CellState State { get; set; }
        /// <summary>
        /// Вес ячейки, по умолчанию -1 - ячейка без веса
        /// </summary>
        public int Weight { get; set; }
        /// <summary>
        /// Поле для хранения идентификатора элемента
        /// </summary>
        public string Description { get; set; }

        public bool isBusy
        {
            get
            {
                if (State == CellState.Wave ||
                    State == CellState.Wire ||
                    State == CellState.WireTopRight ||
                    State == CellState.WireTopLeft ||
                    State == CellState.WireBottomRight ||
                    State == CellState.WireBottomLeft ||
                    State == CellState.WireCross ||
                    State == CellState.WireHorizontal ||
                    State == CellState.WireVertical ||
                    State == CellState.ArrowDown ||
                    State == CellState.ArrowUp ||
                    State == CellState.ArrowLeft ||
                    State == CellState.ArrowRight ||
                    State == CellState.ArrowDownThroughHorizontal ||
                    State == CellState.ArrowUpThroughHorizontal ||
                    State == CellState.ArrowLeftThroughVertical ||
                    State == CellState.ArrowRightThroughVertical ||
                    State == CellState.PointA ||
                    State == CellState.Contact)
                    return true;
                else
                    return false;
            }   
        }

        public bool isBusyForBeam
        {
            get
            {
                if (State == CellState.WireTopRight ||
                    State == CellState.WireTopLeft ||
                    State == CellState.WireBottomRight ||
                    State == CellState.WireBottomLeft ||
                    State == CellState.WireHorizontal ||
                    State == CellState.WireVertical ||
                    State == CellState.ArrowDown ||
                    State == CellState.ArrowUp ||
                    State == CellState.ArrowLeft ||
                    State == CellState.ArrowRight ||
                    State == CellState.Contact)
                    return true;
                else
                    return false;
            }
        }

        public bool isConnectible
        {
            get
            {
                if (State == CellState.PointA ||
                    State == CellState.PointB ||
                    State == CellState.Wire ||
                    State == CellState.WireTopRight ||
                    State == CellState.WireTopLeft ||
                    State == CellState.WireBottomRight ||
                    State == CellState.WireBottomLeft ||
                    State == CellState.WireCross ||
                    State == CellState.WireHorizontal ||
                    State == CellState.WireVertical)
                    return true;
                else
                    return false;
            }
        }

        public bool isArrow
        {
            get
            {
                if (State == CellState.ArrowDown ||
                    State == CellState.ArrowUp ||
                    State == CellState.ArrowLeft ||
                    State == CellState.ArrowRight || 
                    State == CellState.ArrowUpThroughHorizontal || 
                    State == CellState.ArrowDownThroughHorizontal || 
                    State == CellState.ArrowLeftThroughVertical || 
                    State == CellState.ArrowRightThroughVertical)
                    return true;
                else
                    return false;
            }
        }



        public Cell()
        {
            State = CellState.Empty;
            Weight = -1;
        }

        public Cell Clone()
        {
            var cell = new Cell();
            cell.State = this.State;
            cell.Weight = this.Weight;
            cell.Description = this.Description;
            
            return cell;
        }
    }

    

    public enum CellState
    {
        Empty,
        ArrowUp,
        ArrowDown,
        ArrowLeft,
        ArrowRight,
        ArrowUpThroughHorizontal,
        ArrowDownThroughHorizontal,
        ArrowLeftThroughVertical,
        ArrowRightThroughVertical,
        WireHorizontal,
        WireVertical,
        WireTopRight,
        WireTopLeft,
        WireBottomRight,
        WireBottomLeft,
        WireCross,
        Contact,
        PointA,
        PointB,
        Wire,
        Wave

    }
}
