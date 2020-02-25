using RevolutionCAD.Composition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionCAD.Placement
{
    public class PosledMaxLastAllStepPlaced
    {
        public static List<StepPlacementLog> Place(CompositionResult cmp, Matrix<int> R, out string errMsg)
        {
            // если в методе произошла какая-то критическая ошибка, записывайте её в эту переменную и делайте return null
            errMsg = "";

            // считываем результат компоновки, в нём хранится список элементов для каждого узла
            var boardsElements = cmp.BoardsElements;

            // создаём класс для логирования (обязательно)
            var log = new List<StepPlacementLog>();

            // создаём список с матрицами для каждого узла
            var boards = new List<Matrix<int>>();

            // производите какие-то действия по алгоритму и логируете их. пример логирования можете глянуть в классе TestPlacement

            return log;
        }
    }
}
