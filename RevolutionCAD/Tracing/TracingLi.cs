using RevolutionCAD.Placement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionCAD.Tracing
{
    /// <summary>
    /// Трассировка по методу Ли
    /// </summary>
    public class TracingLi
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

            // тут выполняете действия по алгоритму используя список проводков и обязательно логируете действия
            // пример логирования в классе TestTracing



            for (int boardNum = 0; boardNum < boardsWiresPositions.Count; boardNum++)
            {
                var boardWiresPositions = boardsWiresPositions[boardNum];

                var boardDRPs = new List<Matrix<Cell>>();
                boards.Add(boardDRPs);

                boardDRPs.Add(plc.BoardsDRPs[boardNum]);

                foreach (var wire in boardWiresPositions)
                {
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

                    currentDRP[startPos.Row, startPos.Column].State = CellState.PointA;
                    currentDRP[endPos.Row, endPos.Column].State = CellState.PointB;

                    var neighbors = new List<Position>();
                    neighbors.Add(startPos);

                    int currentWeight = 0;

                    do
                    {
                        fullDrp = ApplicationData.MergeLayersDRPs(boardDRPs);
                        
                        foreach (var neighbor in neighbors)
                        {
                            currentDRP[neighbor.Row, neighbor.Column].Weight = currentWeight;
                            if (currentDRP[neighbor.Row, neighbor.Column].State != CellState.Contact && currentDRP[neighbor.Row, neighbor.Column].State != CellState.PointA)
                                currentDRP[neighbor.Row, neighbor.Column].State = CellState.Wave;
                        }
                        log.Add(new StepTracingLog(boards, $"Распроcтраняем волну с весом {currentWeight} для {boardDRPs.Count - 1}-го проводника в {boardNum+1} узле"));

                        currentWeight++;
                        neighbors = getNeighbors(fullDrp, neighbors);

                    } while (neighbors.Count > 0 && !neighbors.Any(x => x.Column == endPos.Column && x.Row == endPos.Row));


                    currentDRP[endPos.Row, endPos.Column].Weight = currentWeight;

                    neighbors = new List<Position>();
                    neighbors.Add(endPos);

                    Position currentPos = new Position(-1,-1);
                    do
                    {
                        fullDrp = ApplicationData.MergeLayersDRPs(boardDRPs);
                        foreach (var neighbor in neighbors)
                        {
                            if (currentDRP[neighbor.Row, neighbor.Column].Weight == currentWeight)
                            {
                                currentDRP[neighbor.Row, neighbor.Column].State = CellState.Wire;
                                currentPos = neighbor.Clone();
                                break;
                            }
                        }
                        

                        currentWeight--;
                        neighbors = getNeighborsOnlyWave(fullDrp, currentPos);

                    } while (currentWeight>=0);

                    for (int i = 0; i < currentDRP.RowsCount; i++)
                    {
                        for (int j = 0; j < currentDRP.ColsCount; j++)
                        {
                            currentDRP[i, j].Weight = -1;
                            if (currentDRP[i, j].State == CellState.Wave)
                            {
                                currentDRP[i, j].State = CellState.Empty;
                            }
                                
                        }
                    }

                    log.Add(new StepTracingLog(boards, $"Волна достигла точки Б. Определяем точки, где будет проходить проводник №{boardDRPs.Count - 1} в {boardNum + 1} узле"));

                    for (int i = 0; i < currentDRP.RowsCount; i++)
                    {
                        for (int j = 0; j < currentDRP.ColsCount; j++)
                        {
                            Cell leftCell = new Cell();
                            Cell rightCell = new Cell();
                            Cell topCell = new Cell();
                            Cell bottomCell = new Cell();
                            Cell currentCell = new Cell();

                            if (j != 0)
                                leftCell = currentDRP[i, j - 1];
                            else
                                leftCell.State = CellState.Empty;

                            if (j != currentDRP.ColsCount - 1)
                                rightCell = currentDRP[i, j + 1];
                            else
                                rightCell.State = CellState.Empty;

                            if (i != 0)
                                topCell = currentDRP[i - 1, j];
                            else
                                topCell.State = CellState.Empty;

                            if (i != currentDRP.RowsCount - 1)
                                bottomCell = currentDRP[i + 1, j];
                            else
                                bottomCell.State = CellState.Empty;

                            currentCell = currentDRP[i, j];

                            if (currentCell.State == CellState.Wire &&
                                leftCell.isConnectible &&
                                rightCell.isConnectible &&
                                topCell.isConnectible &&
                                bottomCell.isConnectible)
                            {
                                currentDRP[i, j].State = CellState.WireCross;
                            }
                            else if (currentCell.State == CellState.Wire && topCell.isConnectible && bottomCell.isConnectible)
                            {
                                currentDRP[i, j].State = CellState.WireVertical;
                            }
                            else if (currentCell.State == CellState.Wire && leftCell.isConnectible && rightCell.isConnectible)
                            {
                                currentDRP[i, j].State = CellState.WireHorizontal;
                            }
                            else if (currentCell.State == CellState.Wire && topCell.isConnectible && leftCell.isConnectible)
                            {
                                currentDRP[i, j].State = CellState.WireTopLeft;
                            }
                            else if (currentCell.State == CellState.Wire && topCell.isConnectible && rightCell.isConnectible)
                            {
                                currentDRP[i, j].State = CellState.WireTopRight;
                            }
                            else if (currentCell.State == CellState.Wire && bottomCell.isConnectible && leftCell.isConnectible)
                            {
                                currentDRP[i, j].State = CellState.WireBottomLeft;
                            }
                            else if (currentCell.State == CellState.Wire && bottomCell.isConnectible && rightCell.isConnectible)
                            {
                                currentDRP[i, j].State = CellState.WireBottomRight;
                            }
                            

                        }
                    }
                    

                    currentDRP[startPos.Row, startPos.Column].State = CellState.Contact;
                    currentDRP[endPos.Row, endPos.Column].State = CellState.Contact;
                    log.Add(new StepTracingLog(boards, $"Построили на базе точек проводник №{boardDRPs.Count - 1} в {boardNum + 1} узле"));
                    

                }


            }



            return log;
        }

        public static bool isInDrp(Matrix<Cell> matr, int row, int col)
        {
            if (row > 0 && row < matr.RowsCount)
            {
                if (col > 0 && col < matr.ColsCount)
                {
                    return true;
                }
            }
            return false;
        }

        public static List<Position> getNeighbors(Matrix<Cell> drp, List<Position> positions)
        {
            var allNeighbors = new List<Position>();

            foreach (var pos in positions)
            {
                var neighbors = getNeighbors(drp, pos);
                foreach (var neighbor in neighbors)
                {
                    if (!allNeighbors.Any(x => x.Column == neighbor.Column && x.Row == neighbor.Row))
                    {
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
                        if (drp[aplicant.Row, aplicant.Column].isBusy == false)
                        {
                            neighbors.Add(aplicant);
                        }
                    }
                }
            }

            return neighbors;

        }

        public static List<Position> getNeighborsOnlyWave(Matrix<Cell> drp, Position pos)
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
                        if (drp[aplicant.Row, aplicant.Column].State == CellState.Wave)
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
