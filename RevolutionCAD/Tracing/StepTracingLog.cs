using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionCAD.Tracing
{
    public class StepTracingLog
    {
        /// <summary>
        /// Представляет дискретное рабочее поле, каждая ячейка представлена классом Cell
        /// у неё есть вес и состояние
        /// </summary>
        public List<List<Matrix<Cell>>> DRP { get; set; } // имеем список узлов, в каждом элементе списка хранится список слоёв дрп для каждого провода
        public string Message { get; set; }

        public StepTracingLog(List<List<Matrix<Cell>>> drp, string msg)
        {
            DRP = drp.Select(x=>x.Select(y=>y.Copy() as Matrix<Cell>).ToList()).ToList();
            Message = msg;
        }
    }
}
