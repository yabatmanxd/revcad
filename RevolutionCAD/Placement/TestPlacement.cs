using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionCAD.Placement
{
    public class TestPlacement
    {
        public static List<StepPlacementLog> Place(out string err)
        {
            err = "";
            // создаём класс для логирования (обязательно)
            var log = new List<StepPlacementLog>();

            // для моего тестового заполнения не нужно знать какие элементы в каком узле
            // а вам для этого придётся выполнить чтение файла предыдущего этапа:
            //var ElementsInBoards = ApplicationData.ReadComposition(out err);
            

            // создаём список с матрицами для каждого узла
            var boards = new List<Matrix<int>>();

            // тут начинается алгоритм для тупого поочерёдного заполнения позиций матрицы элементами без всякого анализа

            boards.Add(new Matrix<int>(3,3)); // начинаем алгоритм с создания матрицы
            boards[0].Fill(-1);
            int pos = 0;
            for (int elem = 0; elem < 20; elem++)
            {
                if (pos > 6) // единственным условием является чтобы в 1 узле было по 7 элементов
                {
                    boards.Add(new Matrix<int>(3, 3));
                    boards.Last().Fill(-1);
                    pos = 0;
                }
                // этот метод позволяет обращаться к плате не как к массиву [строка, столбец], а просто от 0 до 8 если матрица 3х3, или от 0 до 15 если 4х4
                boards.Last().setValueByPlatePos(pos,elem);
                pos++;
                string msg = "Поместили элмент D" + elem + " на " + boards.Count + " плату"; // пишем сообщение чё произошло на этом шаге
                var step = new StepPlacementLog(boards, msg);
                log.Add(step);
            }

            return log;
        }
    }
}
