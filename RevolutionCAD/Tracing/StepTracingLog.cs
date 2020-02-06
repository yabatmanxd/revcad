using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionCAD.Tracing
{
    class StepTracingLog
    {
        /// <summary>
        /// Представляет дискретное рабочее поле, каждая ячейка представлена классом Cell
        /// у неё есть вес и состояние
        /// </summary>
        Matrix<Cell> DRP { get; set; }
        public string Message { get; set; }
    }
}
