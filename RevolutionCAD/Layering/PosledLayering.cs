using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RevolutionCAD.Tracing;

namespace RevolutionCAD.Layering
{
    public static class PosledLayering
    {
        public static List<StepLayeringLog> Layer(List<List<Matrix<Cell>>> trc, out string err_msg)
        {
            // обязательно создаём лог действий
            var log = new List<StepLayeringLog>();

            // при возникновении критической ошибки её нужно занести в эту переменную и сделать return null
            err_msg = "";

            var boards = new List<List<List<Matrix<Cell>>>>();
            
            for (int boardNum = 0; boardNum < trc.Count; boardNum++)
            {
                var board = new List<List<Matrix<Cell>>>();
                
                board.Add(trc[boardNum]);

                boards.Add(board);
            }

            log.Add(new StepLayeringLog(boards, "Начинаем расслоение"));


            for (int boardNum = 0; boardNum < boards.Count; boardNum++)
            {
                var board = boards[boardNum];

                for (int layerNum = 0; layerNum < board.Count; layerNum++)
                {
                    int wireMaxIntersections;
                    int maxIntersections;
                    
                    do
                    {
                        string logMessage = "";

                        var layerWires = board[layerNum];

                        var fullDRP = ApplicationData.MergeLayersDRPs(layerWires);

                        var intersectionsPositions = new List<Position>();

                        for (int i = 0; i < fullDRP.RowsCount; i++)
                        {
                            for (int j = 0; j < fullDRP.ColsCount; j++)
                            {
                                if (fullDRP[i, j].State == CellState.WireCross)
                                {
                                    var pos = new Position(i, j);
                                    intersectionsPositions.Add(pos);
                                }
                            }
                        }

                        var intersectionsMatr = new Matrix<int>(layerWires.Count, layerWires.Count);
                        intersectionsMatr.Fill(0);

                        foreach (var intersectPos in intersectionsPositions)
                        {
                            int wire1;
                            int wire2;
                            wire1 = layerWires.IndexOf(layerWires.Where(x => x[intersectPos.Row, intersectPos.Column].State == CellState.WireHorizontal ||
                                                                            x[intersectPos.Row, intersectPos.Column].State == CellState.WireVertical).First());

                            wire2 = layerWires.IndexOf(layerWires.Where(x => x[intersectPos.Row, intersectPos.Column].State == CellState.WireHorizontal ||
                                                                              x[intersectPos.Row, intersectPos.Column].State == CellState.WireVertical).Last());

                            intersectionsMatr[wire1, wire2]++;
                            intersectionsMatr[wire2, wire1]++;
                        }

                        wireMaxIntersections = -1;
                        maxIntersections = int.MinValue;

                        for (int i = 0; i < intersectionsMatr.RowsCount; i++)
                        {
                            int intersections = 0;
                            for (int j = 0; j < intersectionsMatr.ColsCount; j++)
                            {
                                intersections += intersectionsMatr[i, j];
                            }

                            logMessage += $"Количество пересечений проводника №{i} на {layerNum} слое {boardNum} платы = {intersections}\n";

                            if (intersections > maxIntersections)
                            {
                                maxIntersections = intersections;
                                wireMaxIntersections = i;
                            }

                        }

                        if (maxIntersections > 0)
                        {
                            if (layerNum + 1 != board.Count - 1)
                            {
                                board.Add(new List<Matrix<Cell>>());
                            }
                            board[layerNum + 1].Add(layerWires[wireMaxIntersections]);
                            board[layerNum].Remove(layerWires[wireMaxIntersections]);
                            log.Add(new StepLayeringLog(boards, $"У проводника {wireMaxIntersections} максимальное количество пересечений = {maxIntersections}. Выносим его на новый слой."));
                        }
                    } while (maxIntersections > 0);
                    
                    
                }

            }


            // ниже написан тестовый код
            // он просто создаёт 3 платы
            //for(int boardNum = 0; boardNum < 3; boardNum++)
            //{
            //    var layers = new List<List<Matrix<Cell>>>();
            //    boards.Add(layers);

            //    // по 3 слоя в каждой плате
            //    for (int layerNum = 0; layerNum < 3; layerNum++)
            //    {
            //        var wires = new List<Matrix<Cell>>();
            //        layers.Add(wires);

            //        // и по 3 провода в каждом слое
            //        for (int wireNum = 0; wireNum < 3; wireNum++)
            //        {
            //            var drp = new Matrix<Cell>(30,10);
            //            wires.Add(drp);

            //            for(int i = 0; i<30; i++)
            //            {
            //                for (int j = 0; j < 10; j++)
            //                {
            //                    drp[i, j] = new Cell();
            //                }
            //            }

            //            drp[wireNum, 0].State = CellState.WireTopRight + wireNum;
            //            drp[wireNum, 1].State = CellState.WireTopRight + wireNum;
            //            drp[wireNum, 2].State = CellState.WireTopRight + wireNum;

            //            log.Add(new StepLayeringLog(boards, $"Мойте руки, носите маски, соблюдайте карантин..."));

            //        }
            //    }
            //}

            return log;
        }
    }
}
