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

        public T getValueByPlatePos(int pos)
        {
            if (pos < 0 || pos > ColsCount * RowsCount)
            {
                throw new IndexOutOfRangeException();
            }
            else
            {
                int j = pos / RowsCount;
                int i = pos % ColsCount;
                if (j % 2 == 1)
                {
                    i = ColsCount - i - 1;
                }
                return _matrix[i, j];
            }
        }

        public void setValueByPlatePos(int pos, T element)
        {
            if (pos < 0 || pos > ColsCount * RowsCount)
            {
                throw new IndexOutOfRangeException();
            }
            else
            {
                int j = pos / RowsCount;
                int i = pos % ColsCount;
                if (j%2 == 1)
                {
                    i = ColsCount - i - 1;
                }
                _matrix[i, j] = element;
            }
        }

        public int getRelativePosByAbsolute(Position pos)
        {
            int relPos;
            if (pos.Column %2 == 0)
            {
                relPos = pos.Column * RowsCount + pos.Row;
            } else
            {
                relPos = pos.Column * RowsCount + (ColsCount - pos.Row) - 1;
            }
            return relPos;
        }

        public Matrix<T> RemoveRow(int row)
        {
            var newMatr = new Matrix<T>(RowsCount-1, ColsCount);

            int index = 0;
            for (int i = 0; i < RowsCount; i++)
            {
                if (i == row)
                    continue;
                else
                {
                    for (int j = 0; j < ColsCount; j++)
                    {
                        newMatr[index, j] = _matrix[i, j];
                    }
                    index++;
                }
            }
            return newMatr;
        }

        public Matrix<T> RemoveCol(int col)
        {
            var newMatr = new Matrix<T>(RowsCount, ColsCount-1);

            int index = 0;
            for (int i = 0; i < RowsCount; i++)
            {
                for (int j = 0; j < ColsCount; j++)
                {
                    if (j == col)
                        continue;
                    else
                    {
                        newMatr[i, index] = _matrix[i, j];
                        index++;
                    }
                }
                index = 0;
                
            }
            return newMatr;
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

        public Matrix<T> Copy()
        {
            var newMatrix = new Matrix<T>(RowsCount, ColsCount);
            for (int i = 0; i < RowsCount; i++)
            {
                for (int j = 0; j < ColsCount; j++)
                {
                    newMatrix[i, j] = _matrix[i, j];
                }
            }
            return newMatrix;
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
