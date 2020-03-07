using RevolutionCAD.Placement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionCAD.Tracing
{
    /// <summary>
    /// Трассировка по методу путевых координат
    /// </summary>
    public class TracingTrackCoordinates
    {
        /// <summary>
        /// Метод для трассировки
        /// </summary>
        /// <returns>Список логов шагов</returns>
        public static List<StepTracingLog> Trace(Scheme sch, PlacementResult plc, out string err)
        {
            // обязательно создаём лог действий
            var log = new List<StepTracingLog>();

            // при возникновении критической ошибки её нужно занести в эту переменную и сделать return null
            err = "";

            // формируем список плат, в котором хранится список слоёв (для каждого проводника свой слой ДРП)
            var boards = new List<List<Matrix<Cell>>>();

            // считываем список плат, в каждой плате хранится список проводников (Wire) соединяют всего 2 контакта
            List<List<Wire>> boardsWiresPositions = plc.BoardsWiresPositions;
            
            for(int boardNum = 0; boardNum < boardsWiresPositions.Count; boardNum++)
            {
                var boardWiresPositions = boardsWiresPositions[boardNum];

                var boardDRPs = new List<Matrix<Cell>>();
                boards.Add(boardDRPs);

                foreach (var wire in boardWiresPositions)
                {
                    boardDRPs.Add(plc.BoardsDRPs[boardNum]);
                    var currentDRP = new Matrix<Cell>(plc.BoardsDRPs[boardNum].RowsCount, plc.BoardsDRPs[boardNum].ColsCount);
                    boardDRPs.Add(currentDRP);

                    for (int i = 0; i < currentDRP.RowsCount; i++)
                    {
                        for (int j = 0; j < currentDRP.ColsCount; j++)
                        {
                            currentDRP[i, j] = new Cell();
                        }
                    }

                    Matrix<Cell> fullDrp;

                    var startPos = wire.A.PositionContact;
                    var endPos = wire.B.PositionContact;

                    List<Position> prioritetsPos = new List<Position>();


                    int rowsDiff = endPos.Row - startPos.Row;
                    int colsDiff = endPos.Column - startPos.Column;
                    int rowsDiffModul = Math.Abs(rowsDiff);
                    int colsDiffModul = Math.Abs(colsDiff);

                    Position top = new Position(-1, 0);
                    Position bottom = new Position(1, 0);
                    Position left = new Position(0, -1);
                    Position right = new Position(0, 1);

                    if (colsDiff > rowsDiff)
                    {
                        if (colsDiffModul > rowsDiffModul)
                        {
                            prioritetsPos.Add(left); // влево
                            if (rowsDiff < 0)
                            {
                                prioritetsPos.Add(bottom);
                                prioritetsPos.Add(right);
                                prioritetsPos.Add(top);
                            } else
                            {
                                prioritetsPos.Add(top);
                                prioritetsPos.Add(right);
                                prioritetsPos.Add(bottom);
                            }
                        } else
                        {
                            prioritetsPos.Add(bottom); // вниз
                            if (colsDiff > 0)
                            {
                                prioritetsPos.Add(left);
                                prioritetsPos.Add(top);
                                prioritetsPos.Add(right);
                            }
                            else
                            {
                                prioritetsPos.Add(right);
                                prioritetsPos.Add(top);
                                prioritetsPos.Add(left);
                            }
                        }
                    } else
                    {
                        if (colsDiffModul > rowsDiffModul)
                        {
                            prioritetsPos.Add(right); // вправо
                            if (rowsDiff > 0)
                            {
                                prioritetsPos.Add(top);
                                prioritetsPos.Add(left);
                                prioritetsPos.Add(bottom);
                            } else
                            {
                                prioritetsPos.Add(bottom);
                                prioritetsPos.Add(left);
                                prioritetsPos.Add(top);
                            }
                        }
                        else
                        {
                            prioritetsPos.Add(top); // вверх
                            if (colsDiff < 0)
                            {
                                prioritetsPos.Add(left);
                                prioritetsPos.Add(bottom);
                                prioritetsPos.Add(right);
                            } else
                            {
                                prioritetsPos.Add(right);
                                prioritetsPos.Add(bottom);
                                prioritetsPos.Add(left);
                            }
                        }
                    }
                    
                    currentDRP[startPos.Row, startPos.Column].State = CellState.PointA; 
                    currentDRP[endPos.Row, endPos.Column].State = CellState.PointB; 

                    var neighbors = new List<Position>();
                    neighbors.Add(startPos);

                    fullDrp = ApplicationData.MergeLayersDRPs(boardDRPs);
                    neighbors = getNeighbors(fullDrp, neighbors);

                    do
                    {
                        fullDrp = ApplicationData.MergeLayersDRPs(boardDRPs);

                        foreach (var neighbor in neighbors)
                        {
                            foreach(var prioritet in prioritetsPos)
                            {
                                Position checkingPos = new Position(neighbor.Row + prioritet.Row, neighbor.Column + prioritet.Column);
                                if (checkingPos.Row >= 0 && checkingPos.Row < currentDRP.RowsCount)
                                {
                                    if (checkingPos.Column >= 0 && checkingPos.Column < currentDRP.ColsCount)
                                    {
                                        if (fullDrp[checkingPos.Row,checkingPos.Column].isArrow || fullDrp[checkingPos.Row, checkingPos.Column].State == CellState.PointA)
                                        {
                                            currentDRP[neighbor.Row, neighbor.Column].State = getArrowByPrioritet(prioritet.Row, prioritet.Column);
                                            break;
                                        }                                       
                                    }
                                }
                            }

                            
                        }
                        log.Add(new StepTracingLog(boards, $"Распроcтраняем волну для {boardDRPs.Count - 1}-го проводника в {boardNum + 1} узле"));

                        
                        neighbors = getNeighbors(fullDrp, neighbors);

                    } while (neighbors.Count > 0 && !neighbors.Any(x => x.Column == endPos.Column && x.Row == endPos.Row));

                    // если незанятых соседей не оказалось, значит трассировка невозможна
                    if (neighbors.Count == 0)
                    {
                        // очищаем текущее дрп
                        for (int i = 0; i < currentDRP.RowsCount; i++)
                        {
                            for (int j = 0; j < currentDRP.ColsCount; j++)
                            {
                                currentDRP[i, j] = new Cell();
                            }
                        }
                        // оставляем только 2 контакта, которые должны быть соеденены
                        currentDRP[startPos.Row, startPos.Column].State = CellState.Contact;
                        currentDRP[endPos.Row, endPos.Column].State = CellState.Contact;
                        log.Add(new StepTracingLog(boards, $"Невозможно выполнить трассировку {boardDRPs.Count - 1}-го проводника в {boardNum + 1} узле"));
                        continue;
                    }

                    var currentPos = getNeighborsOnlyArrow(currentDRP, endPos)[0];

                    do
                    {
                        var bufCellState = currentDRP[currentPos.Row, currentPos.Column].State;
                        currentDRP[currentPos.Row, currentPos.Column].State = CellState.Wire;
                        currentPos = getNextPosByCurrentArrow(currentPos, bufCellState);

                    } while (currentDRP[currentPos.Row,currentPos.Column].State != CellState.PointA);

                    // очищаем всё дрп от веса и ячеек с волной
                    for (int i = 0; i < currentDRP.RowsCount; i++)
                    {
                        for (int j = 0; j < currentDRP.ColsCount; j++)
                        {
                            if (currentDRP[i, j].isArrow)
                            {
                                currentDRP[i, j].State = CellState.Empty;
                            }

                        }
                    }

                    log.Add(new StepTracingLog(boards, $"Волна достигла точки Б. Определяем точки, где будет проходить проводник №{boardDRPs.Count - 1} в {boardNum + 1} узле"));

                    // начинаем долгую и мучительную спец операцию по определению какой формы проводник должен стоять в ячейке
                    // запускаем цикл по всем ячейкам дрп
                    for (int i = 0; i < currentDRP.RowsCount; i++)
                    {
                        for (int j = 0; j < currentDRP.ColsCount; j++)
                        {
                            // объявляем соседей, от них нам нужно будет только состояние
                            Cell leftCell = new Cell();
                            Cell rightCell = new Cell();
                            Cell topCell = new Cell();
                            Cell bottomCell = new Cell();

                            // блок, который присвоит пустое состояние ячейке, если она находится вне дрп
                            if (j > 0)
                                leftCell = currentDRP[i, j - 1];
                            else
                                leftCell.State = CellState.Empty;

                            if (j < currentDRP.ColsCount - 1)
                                rightCell = currentDRP[i, j + 1];
                            else
                                rightCell.State = CellState.Empty;

                            if (i > 0)
                                topCell = currentDRP[i - 1, j];
                            else
                                topCell.State = CellState.Empty;

                            if (i < currentDRP.RowsCount - 1)
                                bottomCell = currentDRP[i + 1, j];
                            else
                                bottomCell.State = CellState.Empty;
                            // конец блока

                            var currentCell = currentDRP[i, j];

                            if (currentCell.State == CellState.Wire)
                            {
                                if (topCell.isConnectible && bottomCell.isConnectible)
                                {
                                    currentDRP[i, j].State = CellState.WireVertical;
                                }
                                else if (leftCell.isConnectible && rightCell.isConnectible)
                                {
                                    currentDRP[i, j].State = CellState.WireHorizontal;
                                }
                                else if (topCell.isConnectible && leftCell.isConnectible)
                                {
                                    currentDRP[i, j].State = CellState.WireTopLeft;
                                }
                                else if (topCell.isConnectible && rightCell.isConnectible)
                                {
                                    currentDRP[i, j].State = CellState.WireTopRight;
                                }
                                else if (bottomCell.isConnectible && leftCell.isConnectible)
                                {
                                    currentDRP[i, j].State = CellState.WireBottomLeft;
                                }
                                else if (bottomCell.isConnectible && rightCell.isConnectible)
                                {
                                    currentDRP[i, j].State = CellState.WireBottomRight;
                                }
                            }

                        }
                    }

                    // заменяем буквы просто контактами
                    currentDRP[startPos.Row, startPos.Column].State = CellState.Contact;
                    currentDRP[endPos.Row, endPos.Column].State = CellState.Contact;
                    log.Add(new StepTracingLog(boards, $"Построили на базе точек проводник №{boardDRPs.Count - 1} в {boardNum + 1} узле"));
                }

                
            }



            return log;
        }

        public static CellState getArrowByPrioritet(int row, int col)
        {
            if (row == -1)
                return CellState.ArrowUp;
            if (row == 1)
                return CellState.ArrowDown;
            if (col == -1)
                return CellState.ArrowLeft;
            if (col == 1)
                return CellState.ArrowRight;
            else
                return CellState.Empty;
        }

        public static Position getNextPosByCurrentArrow(Position currentPos, CellState currentArrow)
        {
            switch(currentArrow)
            {
                case CellState.ArrowDown:
                    return new Position(currentPos.Row + 1, currentPos.Column);
                case CellState.ArrowUp:
                    return new Position(currentPos.Row - 1, currentPos.Column);
                case CellState.ArrowLeft:
                    return new Position(currentPos.Row, currentPos.Column - 1);
                case CellState.ArrowRight:
                    return new Position(currentPos.Row, currentPos.Column + 1);
                default:
                    return new Position(-1, -1);
            }
        }

        public static List<Position> getNeighbors(Matrix<Cell> drp, List<Position> positions)
        {
            var allNeighbors = new List<Position>();

            foreach (var pos in positions)
            {
                var neighbors = getNeighbors(drp, pos);
                foreach (var neighbor in neighbors)
                {
                    if (!allNeighbors.Any(x=>x.Column == neighbor.Column && x.Row == neighbor.Row)) { 
                        allNeighbors.Add(neighbor);
                    }
                }
            }

            return allNeighbors;
        }

        public static List<Position> getNeighbors(Matrix<Cell> drp, Position pos)
        {
            var neighbors = new List<Position>();
            var aplicants = new List<Position>();

            for (int i = 0; i<4; i++)
            {
                aplicants.Add(pos.Clone());
            }

            aplicants[0].Column += 1;
            aplicants[1].Column -= 1;
            aplicants[2].Row -= 1;
            aplicants[3].Row += 1;

            foreach(var aplicant in aplicants)
            {
                if (aplicant.Row >= 0 && aplicant.Row < drp.RowsCount)
                {
                    if (aplicant.Column >= 0 && aplicant.Column < drp.ColsCount)
                    {
                        if (drp[aplicant.Row, aplicant.Column].isBusy == false)
                        {
                            neighbors.Add(aplicant);
                        }
                    }
                }
            }

            return neighbors;

        }

        public static List<Position> getNeighborsOnlyArrow(Matrix<Cell> drp, Position pos)
        {
            var neighbors = new List<Position>();
            var aplicants = new List<Position>();

            for (int i = 0; i < 4; i++)
            {
                aplicants.Add(pos.Clone());
            }

            aplicants[0].Column += 1;
            aplicants[1].Column -= 1;
            aplicants[2].Row -= 1;
            aplicants[3].Row += 1;

            foreach (var aplicant in aplicants)
            {
                if (aplicant.Row >= 0 && aplicant.Row < drp.RowsCount)
                {
                    if (aplicant.Column >= 0 && aplicant.Column < drp.ColsCount)
                    {
                        if (drp[aplicant.Row, aplicant.Column].isArrow)
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
