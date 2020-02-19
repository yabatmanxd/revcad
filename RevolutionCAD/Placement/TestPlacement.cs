using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionCAD.Placement
{
    public class TestPlacement
    {
        public static List<StepPlacementLog> Place()
        {
            var log = new List<StepPlacementLog>();

            var boards = new List<Matrix<int>>();

            boards.Add(new Matrix<int>(3,3)); // начинаем алгоритм с создания матрицы
            // этот пример помещает по 5 элементов в каждый узел, у вас может быть другое условие
            boards[0].Fill(-1);
            int pos = 0;
            for (int elem = 0; elem < 20; elem++)
            {
                if (pos > 6)
                {
                    boards.Add(new Matrix<int>(3, 3));
                    boards.Last().Fill(-1);
                    pos = 0;
                }

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
