﻿using RevolutionCAD.Composition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionCAD.Placement
{
    public class PosledMaxPlacedMinUnplaced
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

                // начальная позиция - условные координаты разъема
                var pos = new Position(-1, -1);

                // таким способом копируем список всех элементов в список неразмещённых
                List<int> unplacedElements = boardElements.Select(x => x).ToList();

                // в списке размещённых по умолчанию только разъём
                List<int> placedElements = new List<int>();
                placedElements.Add(0);
                
                // запускаем цикл, пока не разместим все элементы
                while (unplacedElements.Count > 0)
                {
                    // список с количеством связей с ранее размещёнными элементами ещё неразмещённых
                    var countRelationWithPlaced = new List<int>();
                    // список с количеством связей неразмещённых элементов с неразмещёнными
                    var countRelationWithUnplaced = new List<int>();
                    
                    countRelationWithUnplaced = countRelations(unplacedElements, unplacedElements);
                    countRelationWithPlaced = countRelations(unplacedElements, placedElements);
                    
                    int posMaxPlacedMinUnplaced = -1;
                    int maxPlaced = int.MinValue;
                    int minUnplaced = int.MaxValue;
                    for (int currPos = 0; currPos < unplacedElements.Count; currPos++)
                    {
                        if (countRelationWithPlaced[currPos] > maxPlaced)
                        {
                            posMaxPlacedMinUnplaced = currPos;
                            maxPlaced = countRelationWithPlaced[currPos];
                            minUnplaced = countRelationWithUnplaced[currPos];
                        } else if (countRelationWithPlaced[currPos] == maxPlaced)
                        {
                            if (countRelationWithUnplaced[currPos] < minUnplaced)
                            {
                                posMaxPlacedMinUnplaced = currPos;
                                maxPlaced = countRelationWithPlaced[currPos];
                                minUnplaced = countRelationWithUnplaced[currPos];
                            }
                        }
                    }
                    
                    // получаем номер элемента по позиции в списке
                    var elementNumber = unplacedElements[posMaxPlacedMinUnplaced];

                    string msg = "Количество связей с размещёнными элементами: ";
                    for (int i = 0; i < unplacedElements.Count; i++)
                    {
                        msg += $"D{unplacedElements[i]}={countRelationWithPlaced[i]}; ";
                    }
                    msg += "\n";
                    msg += "Количество связей с неразмещёнными элементами: ";
                    for (int i = 0; i < unplacedElements.Count; i++)
                    {
                        msg += $"D{unplacedElements[i]}={countRelationWithUnplaced[i]}; ";
                    }
                    msg += "\n";
                    msg += $"Максимальное количество c размещёнными и минимум с неразмещёнными у элемента D{elementNumber}\n";
                    msg += $"Найдём оптимальную позицию:\n";

                    // получаем список незанятых соседних ячеек на основе позиции элемента размещённого на предыдущем шаге
                    var neighbors = getNeigbors(boardMatr, pos);

                    // начинаем определение оптимальной позиции для размещения элемента
                    Position minLpos = null;
                    int minL = int.MaxValue;
                    foreach (var neighbor in neighbors)
                    {
                        msg += $"D{elementNumber} -> {boardMatr.getRelativePosByAbsolute(neighbor)}; L=";
                        List<string> operations = new List<string>(0);
                        int L = 0;
                        // запускаем цикл по всем элементам
                        for (int j = 0; j < matrR.ColsCount; j++)
                        {
                            // если этот элемент был размещён
                            if (placedElements.Contains(j))
                            {
                                // если у размещаемого элемента есть связи с текущим рассматриваемым
                                if (matrR[elementNumber, j] != 0)
                                {
                                    // количество связей определяем по матрице R
                                    int countRelationsWithElement = matrR[elementNumber, j];
                                    // получаем расстояние между элементами
                                    int length = getLength(neighbor, getPosByElementNumber(boardMatr, j));
                                    operations.Add($"{countRelationsWithElement}*{length}");
                                    // суммируем длину умноженную на количество связей в L
                                    L += countRelationsWithElement * length;
                                }
                            }
                        }
                        msg += String.Join("+", operations);
                        msg += $"={L}\n";
                        // если подсчитанное L меньше просчитанного на предыдущих шагах
                        if (L < minL)
                        {
                            minL = L;
                            // принимаем эту позицию как оптимальную
                            minLpos = neighbor.Clone();
                        }

                    }
                    int minLposRelative = boardMatr.getRelativePosByAbsolute(minLpos);
                    msg += $"Минимальное значение L в {minLposRelative} позиции\n";
                    pos = minLpos.Clone();

                    // устанавливаем элемент в позицию
                    boards.Last().setValueByPlatePos(minLposRelative, elementNumber);

                    // начинаем логирование действия

                    msg += $"Помещаем его в {minLposRelative} позицию на {boards.Count} плату";
                    var step = new StepPlacementLog(boards, msg);
                    log.Add(step);
                    // закончили логирование

                    // убираем из списка неразмещённых элемент, который только что разместили
                    unplacedElements.Remove(elementNumber);
                    // и добавляем в список размещённых
                    placedElements.Add(elementNumber);
                }
            }
            return log;
        }
        
        // метод который возвращает список количества связей с указанным элементом всех элементов из переданного списка
        // позиция в итоговом списке соответствует позиции в переданном списке
        private static List<int> countRelations(List<int> unplacedElements, List<int> elements)
        {
            var relationsCount = new List<int>();

            foreach (var unplacedElement in unplacedElements)
            {
                int cnt = 0;
                foreach (var element in elements)
                {
                    cnt += matrR[unplacedElement, element];
                }
                relationsCount.Add(cnt);
            }

            return relationsCount;
        }

        private static Position getPosByElementNumber(Matrix<int> boardMatr, int elementNumber)
        {
            Position pos = new Position(-1, -1);
            if (elementNumber == 0)
            {
                pos.Row = -1;
                pos.Column = -1;
                return pos;
            }
            for (int i = 0; i < boardMatr.RowsCount; i++)
            {
                for (int j = 0; j < boardMatr.ColsCount; j++)
                {
                    if (boardMatr[i, j] == elementNumber)
                    {
                        pos.Row = i;
                        pos.Column = j;
                    }
                }
            }
            return pos;
        }

        private static List<Position> getNeigbors(Matrix<int> boardMatr, Position pos)
        {
            var neigborsPos = new List<Position>();
            if (pos.Column == -1 && pos.Row == -1)
            {
                for (int row = 0; row < boardMatr.RowsCount; row++)
                    neigborsPos.Add(new Position(row, 0));
            }
            else
            {
                var aplicants = new List<Position>();

                for (int i = 0; i < 8; i++)
                {
                    aplicants.Add(pos.Clone());
                }

                aplicants[0].Column += 1;

                aplicants[1].Column -= 1;

                aplicants[2].Row -= 1;

                aplicants[3].Row += 1;

                aplicants[4].Row += 1;
                aplicants[4].Column += 1;

                aplicants[5].Row += 1;
                aplicants[5].Column -= 1;

                aplicants[6].Row -= 1;
                aplicants[6].Column += 1;

                aplicants[7].Row -= 1;
                aplicants[7].Column -= 1;

                foreach (var aplicant in aplicants)
                {
                    if (aplicant.Row >= 0 && aplicant.Row < boardMatr.RowsCount)
                    {
                        if (aplicant.Column >= 0 && aplicant.Column < boardMatr.ColsCount)
                        {
                            if (boardMatr[aplicant.Row, aplicant.Column] == -1)
                                neigborsPos.Add(aplicant);
                        }
                    }
                }
            }
            return neigborsPos;
        }

        private static int getLength(Position A, Position B)
        {
            if (B.Row == -1 && B.Column == -1)
            {
                return A.Column + 1;
            }
            else
            {
                int length = Math.Abs(B.Column - A.Column) + Math.Abs(B.Row - A.Row);
                return length;
            }
        }
    }
}
