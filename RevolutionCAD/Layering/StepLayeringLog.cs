using RevolutionCAD.Tracing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionCAD.Layering
{
    public class StepLayeringLog
    {
        /// <summary>
        /// Представляет дискретное рабочее поле, каждая ячейка представлена классом Cell
        /// у неё есть вес и состояние
        /// </summary>
        public List<List<Matrix<Cell>>> BoardsLayersDRPs { get; set; } // имеем список узлов, в каждом элементе списка хранится список слоёв дрп, в каждом элементе списка хранится список слоёв проводов для каждого слоя
        public string Message { get; set; }

        public StepLayeringLog(List<List<List<Matrix<Cell>>>> boardsLayers, string msg)
        {
            // способ для получения копий списков, потому что иначе мы получим тупо ссылки на них
            BoardsLayersDRPs = new List<List<Matrix<Cell>>>();

            // а вот матрицы в списке остались ссылочные и соотвественно ячейки в них тоже
            // нужно скопируваты
            for (int boardNum = 0; boardNum < boardsLayers.Count; boardNum++)
            {
                var boardLayers = new List<Matrix<Cell>>();
                for (int layerNumber = 0; layerNumber < boardsLayers[boardNum].Count; layerNumber++)
                {
                    var matr = ApplicationData.MergeLayersDRPs(boardsLayers[boardNum][layerNumber]);
                    boardLayers.Add(matr);
                }
                BoardsLayersDRPs.Add(boardLayers);
            }

            Message = msg;
        }
    }
}
