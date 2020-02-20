using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionCAD.Composition
{
    class IterMultigraph
    {
        // метод должен возвратить целый лог действий
        //Скударнов С.А.
        public static List<StepCompositionLog> Compose()
        {
            //с этого обязательно должен начинаться метод
            var log = new List<StepCompositionLog>();
            var boards = new List<List<int>>();

            // таким макаром добавляется новая плата в список плат
            boards.Add(new List<int>());
            // таким образом добавляем в 1-ую плату 3-ий элемент
            boards[0].Add(3);



            // в конце каждого шага должно присутствовать это
            string msg = "тут результаты выполнения шага";
            var step = new StepCompositionLog(boards, msg);



            // этим метод должен обязательно закончиться
            return log;
        }
    }
}
