using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RevolutionCAD.Composition
{
    class CompositionResult
    {
        public List<List<int>> ElementsInBoards { get; set; }
        public List<Matrix<int>> BoardMatrixR { get; set; }
        public List<Matrix<int>> BoardMatrixQ { get; set; }
    }
}