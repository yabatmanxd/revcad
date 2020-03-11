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
        //Скударнов С.А.
        /// <summary>
        /// Итерационная компоновка по мультиграфу
        /// </summary>
        public static List<StepCompositionLog> Compose(out string error_msg)
        {
            var log = new List<StepCompositionLog>();

        START:
            error_msg = "";
            string msg = "";
            //с этого обязательно должен начинаться метод
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
            var step = new StepCompositionLog(boards, msg);

            R = R.RemoveCol(0); 
            R = R.RemoveRow(0); // удаляем разъём

            // формируем матрицу таким образом, чтобы строки и столбцы скомпонованных плат оказались рядом
            
            sn = R.ColsCount; // размерность матрицы
            kol_plat = boards.Count; // кол-во плат
            buf = Math.Ceiling((double)sn / kol_plat);
            buf *= kol_plat; // необходимая размерность матрицы R  
                             // поскольку у нас матрица должна разбиваться на равные части - 
                             // необходимо добавить нулевые строки и столбцы

            //***************************************************************************************************************************

            List<int> el_position = new List<int>(); // хранит порядок расположения элементов в преобразоваанной матрице R
            for (int i = 0; i < sn; i++)
                el_position.Add(0);
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

            int [,] R_buf = new int [sn,sn];// буфферная матрица для хранения порядка элементов в порядке расположения по платам

            for (int i=0;i<el_position.Count;i++) //строки располагает в нужном порядке
                {
                for (int j = 0; j < el_position.Count; j++)
                {
                    R_buf[i, j] = R[i, el_position[j]];
                }
                }

            for (int i = 0; i < sn; i++) //перезаписываем матрицу с расположенными в нужном порядке строками
                for (int j = 0; j < sn; j++)
                    R[i,j] = R_buf[i, j];

            for (int i = 0; i < el_position.Count; i++) //столбцы располагает в нужном порядке 
            {
                for (int j = 0; j < el_position.Count; j++)
                {
                    R_buf[i, j] = R[el_position[j],i];
                }
            }

            for (int i = 0; i < sn; i++) //перезаписываем матрицу с расположенными в нужном порядке элементами
                for (int j = 0; j < sn; j++)
                    R[i, j] = R_buf[i, j];

            // Алгоритм, который создаёт матрицу с равным количеством элементов т.е. добавляет где необходимо 0 столбцы и строки
            // Необходим, чтоб на каждой плате условно было одинаковое кол-во эл-ов 

            int position = 0; // позиция, в которую необходимо добавить нулевую строку/столбец            
            for (int i = 0; i < kol_elem_for_plata.Length; i++)
            {
                position++;
                if (kol_elem_for_plata[i] < Max_kol_elem_for_plata)
                {
                    kol_elem_for_plata[i]++;
                    R = AddZero(R, position);
                    i--;
                    el_position.Insert(position, -1); // дополнение массива -1 - признак фиктивного элемента
                }
            }
            

            // в зависимости от кол-ва плат выполняем итерации
            switch (kol_plat)
            {
                case 0:
                    error_msg = "...отец сына. А тот ему и говорит - плохо нам без мамы;";
                    break;
                case 1:
                    error_msg = "Это бесполезно - все элементы распологаются на одной плате";
                    break;
                case 2:
                    // варианты перестановок - между 1 и 2 платой

                    List<int> delta = new List<int>(sn); // список для хранения результатов расчёта.
                    int delta1 = 0, delta2 = 0;
                    for (int index_delta = 0; index_delta < sn; index_delta++)
                    {
                        delta.Add(0);
                        for (int i1 = 0; i1 < (buf / kol_plat); i1++)// сумма связей на 1 плате
                            delta1 += R[index_delta, i1];
                        for (int i2 = (int)(buf / kol_plat) + 1; i2 < sn; i2++)// сумма связей на 2 плате
                            delta2 += R[index_delta, i2];

                        if (index_delta < (buf / kol_plat))
                            delta[index_delta] = delta2 - delta1;// итерация в 1 части плат
                        else // дошли по строкам до 2 платы и делаем обратное
                            delta[index_delta] = delta1 - delta2;
                    }
                    int index = (int)(buf / kol_plat);
                    List<int> listMax1 = delta.GetRange(0, index);
                    List<int> listMax2 = delta.GetRange(index, index);

                    int max1 = listMax1.Max();
                    int max2 = listMax2.Max();

                    int count = R[listMax1.IndexOf(listMax1.Max()), listMax2.IndexOf(listMax2.Max()) + (int)(buf / kol_plat)];
                    int deltaR = max1 + max2 - 2 * count;

                    if (deltaR > 0)
                    {
                        // имеет смысл менять местами элементы
                        // индексы элементов с нуля
                        int elem1 = el_position[listMax1.IndexOf(max1)];
                        int elem2 = el_position[listMax2.IndexOf(max2) + index];

                        // поменять строки и столбцы на платах
                        //R = ReplaceMatrix(elem1, elem2, R);
                        // обновляем el_position
                        int yy = el_position[listMax1.IndexOf(max1)];
                        el_position[listMax1.IndexOf(max1)] = el_position[listMax2.IndexOf(max2) + index];
                        el_position[listMax2.IndexOf(max2) + index] = yy;

                        // поменять файл компоновки
                        boards[0].Remove(elem1 + 1);
                        boards[1].Remove(elem2 + 1);
                        boards[0].Add(elem2 + 1);
                        boards[1].Add(elem1 + 1);
                        
                        // возвращаем лог
                        msg = "Меняем местами элементы " + (elem1 + 1) + " и " + (elem2 + 1) +
                            " - они имеют максимальное приращение delta = " + max1 + " + " + max2 +
                            " - 2*" + count + " = " + deltaR + "\n";
                        step = new StepCompositionLog(boards, msg);
                        log.Add(step);

                        // сериализация результата
                        var result = new CompositionResult();
                        result.BoardsElements = log.Last().BoardsList;
                        ApplicationData.WriteComposition(result, out msg);
                        
                        goto START;
                    }
                    else
                    {
                        msg = "Не существует приращений delta > 0, итерационная компоновка завершена\n";
                        step = new StepCompositionLog(boards, msg);
                        log.Add(step);
                        return log;
                    }

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

        /// <summary>
        /// Вставка нулевой строки и нулевого столбца в матрицу matrix в позицию position
        /// </summary>
        private static Matrix<int> AddZero(Matrix<int> matrix, int position)
        {
            Matrix<int> test = new Matrix<int>(matrix.RowsCount + 1, matrix.ColsCount + 1);
            for (int i = 0; i < test.ColsCount; i++)
                for (int j = 0; j < test.ColsCount; j++)
                {
                    if (i < position && j < position)
                        test[i, j] = matrix[i, j];
                    if (i == position || j == position)
                        test[i, j] = 0;
                    else
                    {
                        if (i > position || j > position)
                        {
                            if (i > position && j > position)
                                test[i, j] = matrix[i - 1, j - 1];
                            else
                            {
                                if (i > position)
                                    test[i, j] = matrix[i - 1, j];
                                if (j > position)
                                    test[i, j] = matrix[i, j - 1];
                            }
                        }
                    }
                }
            return test;
        }
        



    }
}
