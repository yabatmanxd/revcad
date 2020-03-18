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

            // ниже написан тестовый код
            // он просто создаёт 3 платы
            for(int boardNum = 0; boardNum < 3; boardNum++)
            {
                var layers = new List<List<Matrix<Cell>>>();
                boards.Add(layers);

                // по 3 слоя в каждой плате
                for (int layerNum = 0; layerNum < 3; layerNum++)
                {
                    var wires = new List<Matrix<Cell>>();
                    layers.Add(wires);

                    // и по 3 провода в каждом слое
                    for (int wireNum = 0; wireNum < 3; wireNum++)
                    {
                        var drp = new Matrix<Cell>(30,10);
                        wires.Add(drp);

                        for(int i = 0; i<30; i++)
                        {
                            for (int j = 0; j < 10; j++)
                            {
                                drp[i, j] = new Cell();
                            }
                        }

                        drp[wireNum, 0].State = CellState.WireTopRight + wireNum;
                        drp[wireNum, 1].State = CellState.WireTopRight + wireNum;
                        drp[wireNum, 2].State = CellState.WireTopRight + wireNum;

                        log.Add(new StepLayeringLog(boards, $"Мойте руки, носите маски, соблюдайте карантин..."));

                    }
                }
            }

            return log;
        }
    }
}
