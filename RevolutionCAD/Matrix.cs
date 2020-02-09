using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionCAD
{
    /// <summary>
    /// Класс для представления матрицы любого типа (int, string и т.д.).
    /// Имеет свойства размерности ColsCount и RowsCount
    /// </summary>
    public class Matrix<T>
    {
        [JsonProperty]
        private T[,] _matrix { get; set; }

        public int ColsCount { private set; get; }
        public int RowsCount { private set; get; }
        
        public Matrix(int rowsCount, int colsCount) {
            ColsCount = colsCount;
            RowsCount = rowsCount;
            _matrix = new T[rowsCount, colsCount];
        }

        public T this[int row, int col]
        {
            get
            {
                return _matrix[row, col];
            }
            set
            {
                _matrix[row, col] = value;
            }
        }
    }
}
