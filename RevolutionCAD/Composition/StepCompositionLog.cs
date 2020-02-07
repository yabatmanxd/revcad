using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionCAD.Composition
{
    public class StepCompositionLog
    {
        /// <summary>
        /// Список плат в который на каждом шаге будут заносится список откомпонованных элементов.
        /// Грубо говоря это слепок состояния всех плат в определённый момент.
        /// </summary>
        public List<List<int>> BoardsList { get; set; }
        /// <summary>
        /// Сообщение которое будет выведено в консоль при выполнении шага.
        /// </summary>
        public string Message { get; set; }

        public StepCompositionLog(List<List<int>> boardList, string msg)
        {
            BoardsList = boardList.Select(x=>x.Select(y=>y).ToList()).ToList(); // замудренный метод копирования массивов
            Message = msg;
        }
    }
}
