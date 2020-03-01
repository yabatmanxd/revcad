using System;
using RevolutionCAD.Composition;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionCAD.Placement
{
    public class PosledMaxLastStepPlaced
    {
        static Matrix<int> matrR;

        public static List<StepPlacementLog> Place(CompositionResult cmp, Matrix<int> R, out string errMsg)
        {
            matrR = R;
            // если в методе произошла какая-то критическая ошибка, записывайте её в эту переменную и делайте return null
            errMsg = "";

            // считываем результат компоновки, в нём хранится список элементов для каждого узла
            var boardsElements = cmp.BoardsElements;

            // создаём класс для логирования (обязательно)
            var log = new List<StepPlacementLog>();

            // создаём список с матрицами для каждого узла
            var boards = new List<Matrix<int>>();

            // запускаем цикл по платам, в каждой плате резмещаем элементы, а потом переходим к следующей
            foreach (var boardElements in boardsElements)
            {
                // определяем размер платы (3х3, 4х4, 5х5) основываясь на количестве элементов, которые необходимо на ней расположить
                var size = Convert.ToInt32(Math.Ceiling(Math.Sqrt(boardElements.Count)));

                // создаём матрицу просчитанного размера
                var boardMatr = new Matrix<int>(size, size);

                // заполняем "-1" матрицу
                boardMatr.Fill(-1);

                // добавляем в общий список плат новую матрицу платы, в которую будем на этом шаге заносить элементы
                boards.Add(boardMatr);
                
                // изначально размещён был разъём
                int lastPlacedElementNumber = 0;
                
                // стартовая позиция будет 0
                int pos = 0;

                // таким способом копируем список всех элементов в список неразмещённых
                List<int> unplacedElements = boardElements.Select(x => x).ToList();

                // запускаем цикл, пока не разместим все элементы
                while(unplacedElements.Count>0)
                {
                    // с помощью метода получаем список количества связей для каждого неразмещённого элемента
                    // позциция элемента в списке неразмещённых соотвествует позиции в сформированном списке
                    // в метод передаётся список неразмещённых элементов и элемент с которым нужно посчитать связи
                    var countRelations = countRelationsWithPlacedOnLastStep(unplacedElements, lastPlacedElementNumber);
                    
                    // определяем максимальное число связей
                    var maxRelations = countRelations.Max();

                    // определяем позицию, в которой находится элемент с максимальным количеством связей
                    var elementPosMaxRelations = countRelations.IndexOf(maxRelations);

                    // получаем номер элемента по позиции в списке
                    var elementNumberMaxRelations = unplacedElements[elementPosMaxRelations];

                    // устанавливаем элемент в позицию
                    boards.Last().setValueByPlatePos(pos, elementNumberMaxRelations);
                    
                    // начинаем логирование действия
                    string msg = $"Элемент размещённый на предыдущем шаге: D{lastPlacedElementNumber}\n";
                    msg += "Количество связей неразмещённых элементов с ним: ";
                    for (int i = 0; i< unplacedElements.Count; i++)
                    {
                        msg += $"D{unplacedElements[i]}={countRelations[i]}; ";
                    }
                    msg += "\n";
                    msg += $"Максимальное количество связей у элемента D{elementNumberMaxRelations}\n";
                    msg += $"Помещаем его в {pos} позицию на {boards.Count} плату";
                    var step = new StepPlacementLog(boards, msg);
                    log.Add(step);
                    // закончили логирование

                    // убираем из списка неразмещённых элемент, который только что разместили
                    unplacedElements.Remove(elementNumberMaxRelations);

                    // перемещаемся на следующую позицию
                    pos++;

                    // запоминаем для следующего шага номер размещённого элемента
                    lastPlacedElementNumber = elementNumberMaxRelations;

                }                
            }
            return log;
        }


        // метод который возвращает список количества связей с указанным элементом всех элементов из переданного списка
        // позиция в итоговом списке соответствует позиции в переданном списке
        private static List<int> countRelationsWithPlacedOnLastStep(List<int> unplacedElements, int elementNumberPlacedOnLastStep)
        {
            var relationsCount = new List<int>();

            foreach (var unplacedElement in unplacedElements)
            {
                int cnt = 0;
                cnt += matrR[unplacedElement, elementNumberPlacedOnLastStep];

                relationsCount.Add(cnt);
            }

            return relationsCount;
        }
    }
}
