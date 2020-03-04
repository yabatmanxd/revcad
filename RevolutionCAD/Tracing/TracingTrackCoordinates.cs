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

            // тут выполняете действия по алгоритму используя список проводков и обязательно логируете действия
            // пример логирования в классе TestTracing



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

                    currentDRP[startPos.Row, startPos.Column].State = CellState.PointA; 
                    currentDRP[endPos.Row, endPos.Column].State = CellState.PointB; 

                    var neighbors = new List<Position>();
                    neighbors.Add(startPos);

                    do
                    {
                        fullDrp = ApplicationData.MergeLayersDRPs(boardDRPs);

                        neighbors = getNeighbors(fullDrp, neighbors);
                        foreach(var neighbor in neighbors)
                        {
                            currentDRP[neighbor.Row, neighbor.Column].State = CellState.WireCross;
                            
                        }
                        log.Add(new StepTracingLog(boards, "Здесь мог быть ваш Погорелов..."));

                    } while (neighbors.Count > 0 && !neighbors.Any(x => x.Column == endPos.Column && x.Row == endPos.Row));

                    break;

                }

                
            }



            return log;
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
    }
}
