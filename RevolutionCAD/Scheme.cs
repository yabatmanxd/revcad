using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RevolutionCAD
{
    /// <summary>
    /// Класс для хранения информации о схеме, в нём будет хранится матрицы R, Q, сопоставление номера микросхемы и DIP и само текстовое описание схемы
    /// </summary>
    public class Scheme
    {
        public string SchemeDefinition { get; set; }
        public List<int> DIPNumbers { get; set; }
        public Matrix<int> MatrixR { get; set; }
        public Matrix<int> MatrixQ { get; set; }
    }
}