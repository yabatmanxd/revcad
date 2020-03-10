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

            for (int boardNum = 0; boardNum < boardsMatrices.Count; boardNum++)
            {
                var boardMatr = boardsMatrices[boardNum];

                List<PairPos> pairs = new List<PairPos>();


                for (int i = 0; i < boardMatr.RowsCount; i++)
                {
                    for (int j = 0; j < boardMatr.ColsCount; j++)
                    {
                        if (boardMatr[i,j] != -1)
                        {
                            var currentPos = new Position(i, j);
                            var neighbors = getNeighbors(boardMatr, currentPos);

                            if (neighbors.Count > 0)
                            {
                                foreach(var neighbor in neighbors)
                                {
                                    var currentPair = new PairPos();
                                    currentPair.FirstPos = currentPos.Clone();
                                    currentPair.SecondPos = neighbor.Clone();

                                    if (!pairs.Any(x => x.Equals(currentPair)))
                                    {
                                        pairs.Add(currentPair);
                                    }
                                }
                            }
                        }
                        
                    }
                }
                //string msg = $"Список сформированных пар для узла №{boardNum+1}:\n";
                //foreach(var pair in pairs)
                //{
                //    msg += $"{boardMatr.getRelativePosByAbsolute(pair.FirstPos)}-{boardMatr.getRelativePosByAbsolute(pair.SecondPos)}\n";
                //}
                //MessageBox.Show(msg);

                int deltaLMax = 0;
                PairPos pairMaxDeltaL = new PairPos();

                string stepMsg = "";

                do
                {
                    deltaLMax = 0;
                    pairMaxDeltaL = null;
                    stepMsg = "";

                    foreach (var pair in pairs)
                    {
                        int firstEl = boardMatr[pair.FirstPos.Row, pair.FirstPos.Column];
                        int secondEl = boardMatr[pair.SecondPos.Row, pair.SecondPos.Column];
                        stepMsg += $"deltaL({firstEl}-{secondEl}) = ";
                        var otherElements = cmp.BoardsElements[boardNum];
                        otherElements.Remove(firstEl);
                        otherElements.Remove(secondEl);
                        otherElements.Add(0);

                        int L = 0;
                        foreach(var otherElement in otherElements)
                        {
                            Position currentElPos = getPosByElementNumber(boardMatr, otherElement);

                            L += (R[otherElement,firstEl] - R[otherElement, secondEl]) *
                                (getLength(currentElPos, pair.FirstPos) - getLength(currentElPos, pair.SecondPos));
                            
                        }
                        stepMsg += L + "\n";
                        if (L > deltaLMax)
                        {
                            deltaLMax = L;
                            pairMaxDeltaL = pair;
                        }
                    }

                    
                    if (deltaLMax > 0)
                    {
                        int firstElement = boardMatr[pairMaxDeltaL.FirstPos.Row, pairMaxDeltaL.FirstPos.Column];
                        int secondElement = boardMatr[pairMaxDeltaL.SecondPos.Row, pairMaxDeltaL.SecondPos.Column];

                        stepMsg += $" -- Наибольшее deltaL у пары ({firstElement}-{secondElement}) = {deltaLMax}. Меняем их местами";

                        boardMatr[pairMaxDeltaL.FirstPos.Row, pairMaxDeltaL.FirstPos.Column] = secondElement;
                        boardMatr[pairMaxDeltaL.SecondPos.Row, pairMaxDeltaL.SecondPos.Column] = firstElement;
                        
                    } else
                    {
                        stepMsg += " -- Положительных приращений нет";
                    }

                    var step = new StepPlacementLog(boardsMatrices, stepMsg);
                    log.Add(step);

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
                // проверка на то, находится ли сосед в пределах дрп
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
