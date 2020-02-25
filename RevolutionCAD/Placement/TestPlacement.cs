using RevolutionCAD.Composition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionCAD.Placement
{
    public class TestPlacement
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

            // тестовый алгоритм просто последовательно размещает все элементы на плату

            // запускаем цикл по платам, в каждой плате резмещаем элементы, а потом переходим к следующей
            foreach(var boardElements in boardsElements)
            {
                // определяем размер платы (3х3, 4х4, 5х5) основываясь на количестве элементов, которые необходимо на ней расположить
                var size = Convert.ToInt32(Math.Ceiling(Math.Sqrt(boardElements.Count)));

                // создаём матрицу просчитанного размера
                var boardMatr = new Matrix<int>(size, size);

                // заполняем "-1" матрицу
                boardMatr.Fill(-1);

                // добавляем в общий список плат новую матрицу платы, в которую будем на этом шаге заносить элементы
                boards.Add(boardMatr);

                // определяет в какую позицию будет поставлен элемент
                int pos = 0;

                // запускаем цикл по элементам, которые должны быть размещены на плате
                foreach(var element in boardElements)
                {
                    // добавляем в последнюю плату элемент на определённую позицию. позиция тут задаётся от 1 до 9. нумерация немного не такая как в идз:
                    // 1 4 7
                    // 2 5 8
                    // 3 6 9 (для платы 3х3)
                    boards.Last().setValueByPlatePos(pos, element);
                    pos++;


                    // записываем результат
                    string msg = "Поместили элемент D" + element + " на " + boards.Count + " плату"; // пишем сообщение чё произошло на этом шаге
                    var step = new StepPlacementLog(boards, msg);
                    log.Add(step);
                }
               
            }
            
            // возвращаем список шагов
            return log;
        }
    }
}
