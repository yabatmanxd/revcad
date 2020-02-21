using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionCAD.Tracing
{
    public class TestTracing
    {
        public static List<StepTracingLog> Trace()
        {
            var log = new List<StepTracingLog>();

            string err;
            var plc = ApplicationData.ReadPlacement(out err);
            var boards = new List<List<Matrix<Cell>>>();

            for (int numBoard = 0; numBoard < plc.BoardsMatrices.Count; numBoard++)
            {
                var layers = new List<Matrix<Cell>>();
                layers.Add(plc.BoardsDRPs[numBoard]);
                boards.Add(layers);
                log.Add(new StepTracingLog(boards,"Камута хировато"));
            }

            return log;
            
        }
    }
}
