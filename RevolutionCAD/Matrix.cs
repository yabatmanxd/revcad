using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionCAD
{
    public class Matrix<T>
    {
        private T[,] _matrix { get; set; }

        public int Width { private set; get; }
        public int Height { private set; get; }
        
        public Matrix(int width, int height) {
            Width = width;
            Height = height;
            _matrix = new T[width,height];
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
