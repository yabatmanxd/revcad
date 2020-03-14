using RevolutionCAD.Placement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionCAD.Tracing
{
    public class TestTracing
    {
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


            // ниже я тупо добавляю в слой каждой платы её ДРП с размещёнными на ней контактами элементов и разъёма, никакой логики тут нет
            for (int numBoard = 0; numBoard < plc.BoardsMatrices.Count; numBoard++)
            {
                var layers = new List<Matrix<Cell>>();
                // в свойстве BoardsDRPs хранится матрица с расставленными контактами разъёма и элементов
                layers.Add(plc.BoardsDRPs[numBoard]);
                boards.Add(layers);
                log.Add(new StepTracingLog(boards,"Здесь могла быть ваша реклама..."));
            }


            // тестировка отображения веса
            log.Last().BoardsDRPs.Last().Last()[2, 2].Weight = 3;
            log.Last().BoardsDRPs.Last().Last()[3, 3].Weight = 3;
            log.Last().BoardsDRPs.Last().Last()[4, 4].Weight = 3;
            log.Last().BoardsDRPs.Last().Last()[4, 4].State = CellState.ArrowRight;
            log.Last().BoardsDRPs.Last().Last()[3, 3].State = CellState.ArrowUp;
            log.Last().BoardsDRPs.Last().Last()[2, 2].State = CellState.ArrowDown;
            // тестировка отображения св..номера элемента
            log.Last().BoardsDRPs.Last().Last()[5, 5].State = CellState.WireCross;
            log.Last().BoardsDRPs.Last().Last()[4, 5].State = CellState.WireVertical;
            log.Last().BoardsDRPs.Last().Last()[6, 5].State = CellState.WireVertical;
            log.Last().BoardsDRPs.Last().Last()[5, 4].State = CellState.WireHorizontal;
            log.Last().BoardsDRPs.Last().Last()[5, 6].State = CellState.WireHorizontal;
            log.Last().BoardsDRPs.Last().Last()[3, 5].State = CellState.WireBottomRight;
            log.Last().BoardsDRPs.Last().Last()[7, 5].State = CellState.WireTopLeft;
            log.Last().BoardsDRPs.Last().Last()[5, 3].State = CellState.WireTopRight;
            log.Last().BoardsDRPs.Last().Last()[5, 7].State = CellState.WireBottomLeft;
            log.Last().BoardsDRPs.Last().Last()[3, 6].State = CellState.WireHorizontal;
            log.Last().BoardsDRPs.Last().Last()[7, 4].State = CellState.WireHorizontal;
            log.Last().BoardsDRPs.Last().Last()[4, 3].State = CellState.WireVertical;
            log.Last().BoardsDRPs.Last().Last()[6, 7].State = CellState.WireVertical;
            log.Last().BoardsDRPs.Last().Last()[5, 5].Description = "D1";
            log.Last().BoardsDRPs.Last().Last()[5, 7].Description = "D3";
            log.Last().BoardsDRPs.Last().Last()[5, 7].Weight = 3;
            return log;
            
        }
    }
}
