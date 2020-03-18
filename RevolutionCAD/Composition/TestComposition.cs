using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionCAD.Composition
{
    public class TestComposition
    {
        /// <summary>
        /// Итерационная компоновка по мультиграфу
        /// </summary>
        public static List<StepCompositionLog> Compose(int countOfElements, int limitsOfWires, out string error_msg)
        {
            var log = new List<StepCompositionLog>();

            error_msg = "";

            // считываем схему (потом достанем из неё матрицу R)
            var sch = ApplicationData.ReadScheme(out error_msg);

            if (error_msg != "")
            {
                return log;
            }
            
            var elements = sch.DIPNumbers;
            var boards = new List<List<int>>();
            boards.Add(new List<int>());

            int boardNum = 0;
            for(int element = 1; element < elements.Count; element++)
            {
                if (boards[boardNum].Count >= countOfElements)
                {
                    boardNum++;
                    boards.Add(new List<int>());
                }
                boards[boardNum].Add(element);
                log.Add(new StepCompositionLog(boards, $"Просто взяли и добавили элемент D{element} в узел №{boardNum+1}"));
            }

            

            return log;
        }
        

    }
}
