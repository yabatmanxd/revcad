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

            var cmp = ApplicationData.ReadComposition(out err);
            
            for (int numBoard = 0; numBoard < cmp.Count; numBoard++)
            {
                var size = Convert.ToInt32(Math.Ceiling(Math.Sqrt(cmp[numBoard].Count)));
                var boardMatr = new Matrix<int>(size, size);
                boardMatr.Fill(-1);
                boards.Add(boardMatr);
                for (int pos = 0; pos < cmp[numBoard].Count; pos++)
                {
                    boards.Last().setValueByPlatePos(pos, cmp[numBoard][pos]);

                    string msg = "Поместили элемент D" + cmp[numBoard][pos] + " на " + boards.Count + " плату"; // пишем сообщение чё произошло на этом шаге
                    var step = new StepPlacementLog(boards, msg);
                    log.Add(step);
                }
               
            }
            

            return log;
        }
    }
}
