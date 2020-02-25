using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionCAD.Placement
{
    public class IterFull
    {
        public static List<StepPlacementLog> Place(Matrix<int> R, out string errMsg)
        {
            // если в методе произошла какая-то критическая ошибка, записывайте её в эту переменную и делайте return null
            errMsg = "";

            var log = new List<StepPlacementLog>();

            // так как итерационный метод должен основываться на резульатах последовательного размещения
            // считываем результаты размещения
            var plc = ApplicationData.ReadPlacement(out errMsg);

            // если произошла ошибка при чтении результатов размещения - заканчиваем алгоритм
            if (errMsg != "")
            {
                return null;
            }

            // считываем матрицы плат с уже размещёнными элементами
            List<Matrix<int>> boardsMatrices = plc.BoardsMatrices;

            // производим какие-то действия и обязательно их логируем. пример логирования находится в классе TestPlacement


            return log;
        }
    }
}
