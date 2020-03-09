using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RevolutionCAD
{
    /// <summary>
    /// Класс для хранения информации о схеме
    /// </summary>
    public class Scheme
    {
        public string SchemeDefinition { get; set; } // текстовое описание схемы
        public List<int> DIPNumbers { get; set; } // номера DIP элементов
        public List<List<Contact>> WiresContacts { get; set; } // список Проводов, в котором хранится список контактов, которые они соединяют
        public Matrix<int> MatrixR { get; set; } // матрица R (количество связей между элементами).
        public Matrix<int> MatrixQ { get; set; } // матрица Q (какие провода какие элементы связывают)
    }
}