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


            R = R.RemoveCol(0); 
            R= R.RemoveRow(0); // удаляем разъём


            var Result = new Matrix<int>(R.ColsCount, R.ColsCount);

            // формируем матрицу таким образом, чтобы строки и столбцы скомпонованных плат оказались рядом
            
            // TODO
            // для теста
            // -------------------------------------------------------------
            Matrix<int> M = new Matrix<int>(3, 3);

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


            var MM = AddZero(M, 1);


            var X = ReplaceMatrix(0, 1, R); // пример замены строк и столбцов (строка/столбец 1, строка/столбец 2, изменяемая матрица )
            // -------------------------------------------------------------


            sn = R.ColsCount; // размерность матрицы
            kol_plat = boards.Count; // кол-во плат
            buf = Math.Ceiling((double)sn / kol_plat);
            buf *= kol_plat; // необходимая размерность матрицы R  
                             // поскольку у нас матрица должна разбиваться на равные части - 
                             // необходимо добавить нулевые строки и столбцы
           
            //***************************************************************************************************************************
            int[] el_position = new int[sn]; //массив, который хранит порядок расположения элементов в преобразоваанной матрице R
            int k = 0;
            int[] kol_elem_for_plata = new int[kol_plat];//массив, который хранит кол-во микросхем на каждой плате. Нужен для поиска макс кол-ва эл-ов и в дальнейшем добавления 0 строк и столбцов в матрицы на которых меньше эл-ов

            for (int i=0;i<kol_plat;i++)
            {
                int kol_elements = boards[i].Count;
                kol_elem_for_plata[i] = kol_elements;
                for (int j = 0; j < kol_elements; j++)
                {
                    el_position[k] = boards[i][j]-1;// (-1) т.к. индекс начинается с 0, а номера эл-ов с 1
                    k++;
                }
            }

                int Max_kol_elem_for_plata = kol_elem_for_plata.Max(); // Максимальное кол-во микросхем на платах 

            //****************************************************************************************************************************
            /*el_position[0] = 2; //Значения для теста
            el_position[1] = 0;
            el_position[2] = 3;
            el_position[3] = 1;

            R[0,0]= R[1, 1] = R[2, 2]=R[3, 3] = R[2, 3] = R[3, 2] = 0;
            R[0, 1] = R[0, 2] = R[1, 0] = R[2, 0] = 2;
            R[1, 2] = R[1, 3] = R[2, 1] = R[3, 1] = 1;*/


            int [,] R_buf = new int [sn,sn];// буфферная матрица для хранения порядка элементов в порядке расположения по платам


            for (int i=0;i<el_position.Length;i++) //строки располагает в нужном порядке
                {
                for (int j = 0; j < el_position.Length; j++)
                {
                    R_buf[i, j] = R[i, el_position[j]];
                }
                }

            for (int i = 0; i < sn; i++) //перезаписываем матрицу с расположенными в нужном порядке строками
                for (int j = 0; j < sn; j++)
                    R[i,j] = R_buf[i, j];

            for (int i = 0; i < el_position.Length; i++) //столбцы располагает в нужном порядке 
            {
                for (int j = 0; j < el_position.Length; j++)
                {
                    R_buf[i, j] = R[el_position[j],i];
                }
            }

            for (int i = 0; i < sn; i++) //перезаписываем матрицу с расположенными в нужном порядке элементами
                for (int j = 0; j < sn; j++)
                    R[i, j] = R_buf[i, j];


            // добавление нулевых строк и столбцов
            while (sn < buf)
            {
                sn++;
                R.AddColumn();
                R.AddRow();
            }

            //Надо доработать алгоритм, который позволит добавлять пустые (нулевые строки/столбцы) в нужную плату


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

        private static Matrix<int> AddZero(Matrix<int> M, int position)
        {
            Matrix<int> NewM = new Matrix<int>(M.RowsCount + 1, M.ColsCount + 1);
            bool fl = false;

            for (int i = 0; i < M.ColsCount+1; i++) //строки располагает в нужном порядке
            {
                for (int j = 0; j < M.ColsCount + 1; j++)
                {
                    if (j < position)
                    { 
                        NewM[i, j] = M[i, j];
                        fl = false;
                    }

                    if (position == j)
                    { 
                        NewM[i, j] = 0;
                        fl = true;
                        break;
                    }

                    if (fl == true)
                    {
                        NewM[i, j] = M[i, j - 1];
                        fl = false;
                    }
                }
            }



           /* for (int i = 0; i < sn; i++) //перезаписываем матрицу с расположенными в нужном порядке строками
                for (int j = 0; j < sn; j++)
                    R[i, j] = R_buf[i, j];

            for (int i = 0; i < el_position.Length; i++) //столбцы располагает в нужном порядке 
            {
                for (int j = 0; j < el_position.Length; j++)
                {
                    R_buf[i, j] = R[el_position[j], i];
                }
            }*/



            /*for(int i = 0; i < NewM.ColsCount - 1; i++)
                for(int j = 0; j < NewM.ColsCount; j++)
                {
                    if (j < position)
                        NewM[i, j] = M[i, j];
                    if (j == position)
                        NewM[i, j] = 0;
                    if (j > position)
                        NewM[i, j] = M[i - 1, j];

                }

            for (int i = 0; i < NewM.ColsCount; i++)
                for (int j = 0; j < NewM.ColsCount; j++)
                {
                    if (j < position)
                        NewM[i, j] = M[i, j];
                    if (j == position)
                        NewM[i, j] = 0;
                    if (j > position)
                        NewM[i, j] = M[i, j + 1];

                }*/



            return NewM;
        }
    }
}
