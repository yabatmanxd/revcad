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
            
            // считываем список плат, в каждой плате хранится список проводников, в каждом проводнике хранится список поводничков
            // проводнички (Wire) соединяют всего 2 контакта
            List<List<List<Wire>>> boardsWiresPositions = plc.BoardsWiresPositions;


            // ниже я тупо добавляю в слой каждой платы её ДРП с размещёнными на ней контактами элементов и разъёма, никакой логики тут нет
            for (int numBoard = 0; numBoard < plc.BoardsMatrices.Count; numBoard++)
            {
                var layers = new List<Matrix<Cell>>();
                // в свойстве BoardsDRPs хранится матрица с расставленными контактами разъёма и элементов
                layers.Add(plc.BoardsDRPs[numBoard]);
                boards.Add(layers);
                log.Add(new StepTracingLog(boards,"Здесь могла быть ваша реклама..."));
            }

            return log;
            
        }
    }
}
