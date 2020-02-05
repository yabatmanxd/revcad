﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionCAD.Placement
{
    public class StepPlacementLog
    {
        /// <summary>
        /// Список матриц состояний размещения на каждом шаге. В матрице хранится информация о размещённом элементе, где 
        /// "-1" - ячейка закрыта (типо не рассматривается), "0" - ячейка открыта, но не занята
        /// </summary>
        public List<List<List<int>>> BoardsList { get; set; }
        /// <summary>
        /// Сообщение которое будет выведено в консоль при выполнении шага.
        /// </summary>
        public string Message { get; set; }
    }
}
