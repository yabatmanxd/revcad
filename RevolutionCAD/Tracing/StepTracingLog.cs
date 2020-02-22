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
        public List<List<Matrix<Cell>>> BoardsDRPs { get; set; } // имеем список узлов, в каждом элементе списка хранится список слоёв дрп для каждого провода
        public string Message { get; set; }

        public StepTracingLog(List<List<Matrix<Cell>>> drp, string msg)
        {
            // способ для получения копий списков, потому что иначе мы получим тупо ссылки на них
            BoardsDRPs = drp.Select(x=>x.Select(y=>y).ToList()).ToList();

            // а вот матрицы в списке остались ссылочные и соотвественно ячейки в них тоже
            // нужно скопируваты
            for (int boardNum = 0; boardNum < BoardsDRPs.Count; boardNum++)
            {
                for (int layerNumber = 0; layerNumber < BoardsDRPs[boardNum].Count; layerNumber++)
                {
                    var oldMatr = BoardsDRPs[boardNum][layerNumber];
                    var newMatr = new Matrix<Cell>(oldMatr.RowsCount, oldMatr.ColsCount);

                    for (int i = 0; i < oldMatr.RowsCount; i++)
                    {
                        for (int j = 0; j < oldMatr.ColsCount; j++)
                        {
                            newMatr[i, j] = oldMatr[i, j].Clone();
                        }
                    }

                    BoardsDRPs[boardNum][layerNumber] = newMatr;
                }
            }

            Message = msg;
        }
    }
}
