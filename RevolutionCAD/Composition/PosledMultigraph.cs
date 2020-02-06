using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionCAD.Composition
{
    class PosledMultigraph
    {
        // метод должен возвратить целый лог действий
        public static List<StepCompositionLog> Compose()
        {
            // пример логирования действий
            var log = new List<StepCompositionLog>();
            // создаём список элементов в узлах
            var boards = new List<List<int>>();

            // написали бяку

            int i = 0; // будет служить итератором по платам
            boards.Add(new List<int>()); // начинаем алгоритм с создания и добавления нового списка элементов узла

            // этот пример помещает по 5 элементов в каждый узел, у вас может быть другое условие
            for (int elem = 0; elem < 20; elem++)
            {
                // если в узле всё ещё меньше 5 элементов
                if (boards.Last().Count < 5)
                {
                    boards.Last().Add(elem); // добавляем номер элемента в список последнего узла

                    // начинаем логирование действия
                    var step = new StepCompositionLog();
                    step.Message = "Поместили элмент D" + elem + " на " + i + " плату"; // пишем сообщение чё произошло на этом шаге
                    step.BoardsList = boards; // сохраняем список на память
                    log.Add(step);
                }
                else
                {
                    // если уже 5 - добавляем новый узел и увеличиваем итератор по платам
                    boards.Add(new List<int>());
                }

            }


            // тут просто Сериализуем последнюю стадию логирования - это как раз та, на которой закончилось формирование и мы получили результат
            // serialize(log.Last().boards)

            // а вот в качестве результата выполнения метода возвращаем целый пошаговый лог
            return log;

        }
    }
}
