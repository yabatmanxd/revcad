using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RevolutionCAD.Placement
{
    public class IterPair
    {
        public static List<StepPlacementLog> Place(Matrix<int> R, out string errMsg)
        {
            // если в методе произошла какая-то критическая ошибка, записывайте её в эту переменную и делайте return null
            errMsg = "";

            var log = new List<StepPlacementLog>();

            // так как итерационный метод должен основываться на резульатах последовательного размещения
            // считываем результаты размещения
            var plc = ApplicationData.ReadPlacement(out errMsg);

            if (errMsg != "")
            {
                return log;
            }

            var cmp = ApplicationData.ReadComposition(out errMsg);

            // если произошла ошибка при чтении результатов размещения - заканчиваем алгоритм
            if (errMsg != "")
            {
                return null;
            }

            // считываем матрицы плат с уже размещёнными элементами
            List<Matrix<int>> boardsMatrices = plc.BoardsMatrices;

            var st = new StepPlacementLog(boardsMatrices, "Начинаем итерационное разщмещение...");
            log.Add(st);

            // запускаем цикл по всем платам
            for (int boardNum = 0; boardNum < boardsMatrices.Count; boardNum++)
            {
                // получаем матрицу, в котором хранятся номера элементов текущей платы
                var boardMatr = boardsMatrices[boardNum];

                // создаем список для пар позиций, элементы в которых нужно попробовать поменять местами
                List<PairPos> pairs = new List<PairPos>();

                // формируем список пар соседей
                // проходим по всем позициям платы
                for (int i = 0; i < boardMatr.RowsCount; i++)
                {
                    for (int j = 0; j < boardMatr.ColsCount; j++)
                    {
                        // если текущая позиция содержит элемент
                        if (boardMatr[i,j] != -1)
                        {
                            var currentPos = new Position(i, j);
                            // получаем позиции соседей для текущей позиции, которые содержат элемент
                            var neighbors = getNeighbors(boardMatr, currentPos);

                            // если найден хоть 1 сосед
                            if (neighbors.Count > 0)
                            {
                                // для каждого соседа создаём пару с текущим элементом
                                foreach(var neighbor in neighbors)
                                {
                                    var currentPair = new PairPos();
                                    currentPair.FirstPos = currentPos.Clone();
                                    currentPair.SecondPos = neighbor.Clone();

                                    // если такой пары нет в уже сформированных, то добавляем её в общий список
                                    if (!pairs.Any(x => x.Equals(currentPair)))
                                    {
                                        pairs.Add(currentPair);
                                    }
                                }
                            }
                        }
                        
                    }
                }
                // максимальное приращение
                int deltaLMax;
                // пара с этим максимальным приращением
                PairPos pairMaxDeltaL = new PairPos();

                string stepMsg = "";

                // запускаем цикл, пока будут положительные приращения
                do
                {
                    deltaLMax = 0;
                    pairMaxDeltaL = null;
                    stepMsg = "";

                    // запускаем цикл по всем парам
                    foreach (var pair in pairs)
                    {
                        int firstEl = boardMatr[pair.FirstPos.Row, pair.FirstPos.Column];
                        int secondEl = boardMatr[pair.SecondPos.Row, pair.SecondPos.Column];
                        // формируем по кусочкам сообщение
                        stepMsg += $"\u0394L({firstEl}-{secondEl}) = ";

                        // формируем список остальных элементов на плате, кроме элоементов текущей пары
                        var otherElements = cmp.BoardsElements[boardNum].Select(x => x).ToList();
                        otherElements.Remove(firstEl);
                        otherElements.Remove(secondEl);
                        // добавляем элемент разъёма
                        otherElements.Add(0);
                        otherElements.Sort();

                        // текущее приращение
                        int L = 0;

                        // это просто 2 списка для формирования сообщения шага
                        List<string> operationsDef = new List<string>(); // в этом хранится буквенное обозначение операции, например: (r1 - r2)*(d1 - d2)
                        List<string> operationsValue = new List<string>(); // а тут именно числовое (5 - 8)*(1 - 3)

                        // начинаем считать приращение для текущей пары
                        // для этого запускаем цикл по всем остальным элементам
                        foreach (var otherElement in otherElements)
                        {
                            // получаем позицию рассматриваемого элемента
                            Position currentElPos = getPosByElementNumber(boardMatr, otherElement);


                            operationsDef.Add($"(r{otherElement}-{firstEl} - r{otherElement}-{secondEl})*" +
                                $"(d{otherElement}-{firstEl} - d{otherElement}-{secondEl})");

                            operationsValue.Add($"({R[otherElement, firstEl]}-{R[otherElement, secondEl]})*" +
                                $"({getLength(currentElPos, pair.FirstPos)}-{getLength(currentElPos, pair.SecondPos)})");

                            // прибавляем к общему приращению 
                            // (количество связей между текущим и первым из пары + количество связей между текущим и вторым из пары) *
                            // (длинна между позицией текущего и первого из пары - длинна между позицией текущего и второго из пары)
                            L += (R[otherElement,firstEl] - R[otherElement, secondEl]) *
                                (getLength(currentElPos, pair.FirstPos) - getLength(currentElPos, pair.SecondPos));
                            
                        }
                        // добавляем к сообщению шага сразу все описания операций
                        stepMsg += string.Join("+", operationsDef);
                        stepMsg += "=";
                        // потом все подставленные значения
                        stepMsg += string.Join("+", operationsValue);
                        stepMsg += "=";
                        stepMsg += L + "\n";
                        // если у текущей пары L больше чем у уже найденной ранее
                        if (L > deltaLMax)
                        {
                            deltaLMax = L; // запоминаем максимальное приращение
                            pairMaxDeltaL = pair; // и текущую пару
                        }
                    }

                    // нашли пару с максимальным приращением

                    // только если это приращение больше 0, нужно поменять элементы на позициях платы местами
                    if (deltaLMax > 0)
                    {
                        // т.к. у нас есть только позиции на плате, элементы в которых нужно поменять местами
                        // достаём номера элементов по этим позиция из матрицы платы
                        int firstElement = boardMatr[pairMaxDeltaL.FirstPos.Row, pairMaxDeltaL.FirstPos.Column];
                        int secondElement = boardMatr[pairMaxDeltaL.SecondPos.Row, pairMaxDeltaL.SecondPos.Column];

                        // добавляем описание
                        stepMsg += $" -- Наибольшее \u0394L у пары ({firstElement}-{secondElement}) = {deltaLMax}. Меняем их местами";

                        // и наконец меняцм местами
                        boardMatr[pairMaxDeltaL.FirstPos.Row, pairMaxDeltaL.FirstPos.Column] = secondElement;
                        boardMatr[pairMaxDeltaL.SecondPos.Row, pairMaxDeltaL.SecondPos.Column] = firstElement;
                        
                    } else
                    {
                        stepMsg += " -- Положительных приращений нет";
                    }

                    // логируем изменения
                    var step = new StepPlacementLog(boardsMatrices, stepMsg);
                    log.Add(step);

                    // цикл будет менять местами элементы с максимальным положительным приращением, пока не найдёт
                    // ни одного положительного приращения между парами
                } while (deltaLMax > 0);
                
            }
            
            return log;
        }

        private class PairPos
        {
            public Position FirstPos { get; set; }
            public Position SecondPos { get; set; }

            public bool Equals(PairPos pair)
            {
                if (pair.FirstPos.Equals(this.FirstPos) && pair.SecondPos.Equals(this.SecondPos) ||
                    pair.SecondPos.Equals(this.FirstPos) && pair.FirstPos.Equals(this.SecondPos))
                    return true;
                else
                    return false;
            }
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

        /// <summary>
        /// Вовзращает список позиций соседей, в которых есть элементы
        /// </summary>
        private static List<Position> getNeighbors(Matrix<int> board, Position pos)
        {
            var neighbors = new List<Position>();
            var aplicants = new List<Position>();
            
            for (int i = 0; i < 4; i++)
            {
                aplicants.Add(pos.Clone());
            }

            aplicants[0].Column += 1; // правый сосед
            aplicants[1].Column -= 1; // левый сосед
            aplicants[2].Row -= 1; // верхний сосед
            aplicants[3].Row += 1; // нижний сосед

            foreach (var aplicant in aplicants)
            {
                // проверка на то, находится ли сосед в пределах матрицы платы
                if (aplicant.Row >= 0 && aplicant.Row < board.RowsCount)
                {
                    if (aplicant.Column >= 0 && aplicant.Column < board.ColsCount)
                    {
                        if (board[aplicant.Row, aplicant.Column] != -1)
                        {
                            neighbors.Add(aplicant);
                        }
                    }
                }
            }

            return neighbors;
        }
    }
}
