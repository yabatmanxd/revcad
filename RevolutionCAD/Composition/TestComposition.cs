using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionCAD.Composition
{
    class TestComposition
    {
        public static List<StepCompositionLog> Compose()
        {
            var log = new List<StepCompositionLog>();

            var boards = new List<List<int>>();
            boards.Add(new List<int>()); // начинаем алгоритм с создания и добавления нового списка элементов узла
            // этот пример помещает по 5 элементов в каждый узел, у вас может быть другое условие
            for (int elem = 0; elem < 20; elem++)
            {
                if (boards.Last().Count > 5)
                {
                    boards.Add(new List<int>());
                }

                boards.Last().Add(elem);
                string msg = "Поместили элмент D" + elem + " на " + boards.Count + " плату"; // пишем сообщение чё произошло на этом шаге
                var step = new StepCompositionLog(boards, msg);
                log.Add(step);
            }

            return log;
        }
        
    }
}
