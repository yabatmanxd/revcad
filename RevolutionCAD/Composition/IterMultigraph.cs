using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Newtonsoft.Json;
using System.IO;

namespace RevolutionCAD.Composition
{
    class IterMultigraph
    {
        // метод должен возвратить целый лог действий
        //Скударнов С.А.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="error_msg"></param>
        /// <returns></returns>
        public static List<StepCompositionLog> Compose(out string error_msg)
        {
            error_msg = "";
            //с этого обязательно должен начинаться метод
            var log = new List<StepCompositionLog>();
            int sn, kol_plat;
            double buf;

            // считываем файл схемы
            var sch = ApplicationData.ReadScheme(out error_msg);
            // считываем матрицу R
            Matrix<int> R = sch.MatrixR;

            // считываем результат последовательной компоновки
            var boards_t = ApplicationData.ReadComposition(out error_msg);
            if (boards_t == null)
            {
                error_msg = "Сначала необходимо выполнить последовательную компоновку";
                return null;
            }

            var boards = ApplicationData.ReadComposition(out error_msg).BoardsElements;
           
            
            R=R.RemoveCol(0); 
            R=R.RemoveRow(0); // удаляем разъём

            var Result = new Matrix<int>(R.ColsCount, R.ColsCount);
            // формируем матрицу таким образом, чтобы строки и столбцы скомпонованных плат оказались рядом
            
            // TODO
            // для теста
            // -------------------------------------------------------------
   /*         Matrix<int> M = new Matrix<int>(3, 3);

            for (int i = 0; i < M.ColsCount; i++)
                for (int j = 0; j < M.ColsCount; j++)
                {
                    if (i == 0) M[i, j] = 0;
                    if (i == 1) M[i, j] = 1;
                    if (i == 2) M[i, j] = 2;
                }
            M[0, 0] = 0;
            M[0, 1] = 2;
            M[0, 2] = 1;

            M[1, 0] = 2;
            M[1, 1] = 0;
            M[1, 2] = 3;

            M[2, 0] = 1;
            M[2, 1] = 3;
            M[2, 2] = 0;
            */
                       
            var X = ReplaceMatrix(0, 1, R); // пример замены строк и столбцов (строка/столбец 1, строка/столбец 1, изменяемая матрица )
            // -------------------------------------------------------------


            sn = R.ColsCount; // размерность матрицы
            kol_plat = boards.Count; // кол-во плат
            buf = Math.Ceiling((double)sn / kol_plat);
            buf *= kol_plat; // необходимая размерность матрицы R  
                             // поскольку у нас матрица должна разбиваться на равные части - 
                             // необходимо добавить нулевые строки и столбцы

            // добавление нулевых строк и столбцов
            while (sn < buf)
            {
                sn++;
                R.AddColumn();
                R.AddRow();
            }
            // в зависимости от кол-ва плат выполняем итерации
            switch (kol_plat)
            {
                case 0:
                    error_msg = "...отец сына. А тот ему и говорит - плохо нам без мамы;";
                    break;
                case 1:
                    error_msg = "Это бесполезно";
                    break;
                case 2:
                    // варианты перестановок - между 1 и 2 платой
                    List<int> delta = new List<int>(sn); //список для хранения результатов расчёта.
                    int delta1 = 0, delta2 = 0;
                    for (int index_delta = 0; index_delta < sn; index_delta++)
                    {
                        for (int i1 = 0; i1 < (buf / kol_plat); i1++)//сумма связей на 1 плате
                            delta1 += R[index_delta, i1];
                        for (int i2 = (int)(buf / kol_plat) + 1; i2 < sn; i2++)//сумма связей на 2 плате
                            delta2 += R[index_delta, i2];

                        if (index_delta < (buf / kol_plat))
                            delta[index_delta] = delta2 - delta1;//итерация в 1 части плат
                        else //дошли по строкам до 2 платы и делаем обратное
                            delta[index_delta] = delta1 - delta2;
                    }
                    List<int> listMax1 = delta.GetRange(0, (int)(buf / kol_plat));
                    List<int> listMax2 = delta.GetRange((int)(buf / kol_plat), sn);

                    int max1 = listMax1.Max();
                    int max2 = listMax2.Max();

                    int count = R[listMax1.IndexOf(listMax1.Max()), listMax2.IndexOf(listMax2.Max()) + (int)(buf / kol_plat)];
                    int deltaR = max1 + max2 - 2 * count;

                    break;
                case 3:
                    // варианты перестановок - 
                    // между 1 и 2 платой
                    // между 1 и 3
                    // между 2 и 3

                    // TODO




                    break;
                default:
                    error_msg = "Мы не проходили";
                    break;

            }


            // в конце каждого шага должно присутствовать это
            string msg = "тут результаты выполнения шага";
            var step = new StepCompositionLog(boards, msg);

            // этим метод должен обязательно закончиться
            return log;
        }

        /// <summary>
        /// Меняет местами строки и столбцы под номерами num1 и num2 матрицы Matr
        /// </summary>
        private static Matrix<int> ReplaceMatrix(int num1, int num2, Matrix<int> Matr)
        {
            var Result = new Matrix<int>(Matr.RowsCount, Matr.ColsCount);
            Result = Matr.Copy();
            
            for(int i = 0; i < Result.RowsCount; i++)
            {
                int buf = Result[i, num1];
                Result[i, num1] = Result[i, num2];
                Result[i, num2] = buf;
            }
            for(int j = 0; j < Result.ColsCount; j++)
            {
                int buf = Result[num1, j];
                Result[num1, j] = Result[num2, j];
                Result[num2, j] = buf;
            }

            return Result;
        }
    }
}
