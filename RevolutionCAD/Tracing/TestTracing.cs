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
            var log = new List<StepTracingLog>();

            err = "";
            var boards = new List<List<Matrix<Cell>>>();

            for (int numBoard = 0; numBoard < plc.BoardsMatrices.Count; numBoard++)
            {
                var layers = new List<Matrix<Cell>>();
                layers.Add(plc.BoardsDRPs[numBoard]);
                boards.Add(layers);
                log.Add(new StepTracingLog(boards,"Здесь могла быть ваша реклама..."));
            }

            return log;
            
        }
    }
}
