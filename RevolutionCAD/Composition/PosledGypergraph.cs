using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionCAD.Composition
{
    class PosledGypergraph
    {
        // метод должен возвратить целый лог действий
        public static List<StepCompositionLog> Compose()
        {
            // пример логирования действий
            var log = new List<StepCompositionLog>();
            // создаём список элементов в узлах
            var boards = new List<List<int>>();
            boards.Add(new List<int>()); // начинаем алгоритм с создания и добавления нового списка элементов узла

            // этот пример помещает по 5 элементов в каждый узел, у вас может быть другое условие
            for (int elem = 0; elem < 20; elem++)
            {
                // если в узле уже больше 5
                if (boards.Last().Count > 5)
                {
                    boards.Add(new List<int>());
                }
                
                boards.Last().Add(elem);

                // начинаем логирование действия
                string msg = "Поместили элмент D" + elem + " на " + boards.Count + " плату"; // пишем сообщение чё произошло на этом шаге
                var step = new StepCompositionLog(boards, msg);
                log.Add(step);

            }


            // тут просто Сериализуем последнюю стадию логирования - это как раз та, на которой закончилось формирование и мы получили результат
            // serialize(log.Last().boards)

            // а вот в качестве результата выполнения метода возвращаем целый пошаговый лог
            return log;

        }
    }
}
