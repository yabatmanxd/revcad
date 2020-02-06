using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionCAD
{
    /// <summary>
    /// Класс для представления матрицы любого типа (int, string).
    /// Имеет свойства размерности Width и Height
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Matrix<T>
    {
        private T[,] _matrix { get; set; }

        public int Width { private set; get; }
        public int Height { private set; get; }
        
        public Matrix(int width, int height) {
            Width = width;
            Height = height;
            _matrix = new T[height,width];
        }

        public T this[int x, int y]
        {
            get
            {
                return _matrix[x, y];
            }
            set
            {
                _matrix[x, y] = value;
            }
        }
    }
}
