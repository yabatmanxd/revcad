using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionCAD.Placement
{
    public class PosledMaxLastStepPlaced
    {
        public static List<StepPlacementLog> Place()
        {
            var log = new List<StepPlacementLog>();
            for (int step = 0; step < 4; step++)
            {
                var st_log = new StepPlacementLog();
                for (int brd = 0; brd < 4; brd++)
                {
                    var matr = new Matrix<int>(3, 3);
                    matr.Fill(step);
                    st_log.BoardsList.Add(matr);
                }
                st_log.Message = "шг"+step;
                log.Add(st_log);
            }
            
            return log;
        }
    }
}
