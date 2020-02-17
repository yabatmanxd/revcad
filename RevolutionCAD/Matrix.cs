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

        public void Fill(T objectToFill)
        {
            for(int i = 0; i < RowsCount; i++)
                for (int j = 0; j < ColsCount; j++)
                    _matrix[i, j] = objectToFill;
        }

        public void AddRow()
        {
            var newMatr = new T[RowsCount + 1, ColsCount];

            for (int i = 0; i < RowsCount; i++)
            {
                for (int j = 0; j < ColsCount; j++)
                {
                    newMatr[i, j] = _matrix[i, j];
                }
            }
            _matrix = newMatr;
            RowsCount++;
        }

        public void AddColumn()
        {
            var newMatr = new T[RowsCount, ColsCount + 1];

            for (int i = 0; i < RowsCount; i++)
            {
                for (int j = 0; j < ColsCount; j++)
                {
                    newMatr[i, j] = _matrix[i, j];
                }
            }
            _matrix = newMatr;
            ColsCount++;
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
