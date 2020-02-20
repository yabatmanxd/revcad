using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RevolutionCAD.Composition
{
    /// <summary>
    /// Класс, в котором будет сохранятся результаты компоновки. Для каждого узла будет своя матрица R и Q
    /// </summary>
    public class CompositionResult
    {
        public List<List<int>> ElementsInBoards { get; set; }
        public List<Matrix<int>> BoardsMatricesR { get; set; }
        public List<Matrix<int>> BoardsMatricesQ { get; set; }
    }
}