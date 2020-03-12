using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionCAD
{

    public class Settings
    {
        public int ContactsDist { get; set; }
        public int RowsDist { get; set; }
        public int ElementsDist { get; set; }

        public Settings()
        {

        }

        public Settings(int? contD, int? rowsD, int? elDist)
        {
            ContactsDist = contD ?? 1;
            RowsDist = rowsD ?? 3;
            ElementsDist = elDist ?? 4;
        }
    }
}
