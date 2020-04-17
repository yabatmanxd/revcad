using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionCAD.Placement
{
    public class IterShaffer
    {
        public static List<StepPlacementLog> Place(Matrix<int> R, out string errMsg)
        {
            // если в методе произошла какая-то критическая ошибка, записывайте её в эту переменную и делайте return null
            errMsg = "";

            var log = new List<StepPlacementLog>();

            // так как итерационный метод должен основываться на резульатах последовательного размещения
            // считываем результаты размещения
            var plc = ApplicationData.ReadPlacement(out errMsg);

            // если произошла ошибка при чтении результатов размещения - заканчиваем алгоритм
            if (errMsg != "")
            {
                return null;
            }

            // считываем матрицы плат с уже размещёнными элементами
            List<Matrix<int>> boardsMatrices = plc.BoardsMatrices;

            // переменные для операций с матрицами, списками и прочим
            int coloumn, row, coloumn_next, row_next, coef, index, i, j, maxL, exchange_coloumn, exchange_row, buf, boardNum;
            string stepMsg;
            bool flag;

            // номер текущей платы
            boardNum = 0;
            log.Add(new StepPlacementLog(boardsMatrices, "Начинаем итерационное размещение методом Шаффера..."));
            // цикл по платам с уже размещёнными элементами
            foreach (var boardMatrix in boardsMatrices)
            {
                boardNum++;
                // если у платы меньше 3 строк или столбцов, то нет смысла переставлять столбцы и строки
                if (boardMatrix.ColsCount < 3 || boardMatrix.RowsCount < 3)
                {
                    stepMsg = $" -- Плата №{boardNum} имеет размер менее 3х3. Нет смысла её рассматривать .\n";
                    var step = new StepPlacementLog(boardsMatrices, stepMsg);
                    log.Add(step);
                    continue;
                }
                // цикл по столбцам платы
                flag = true;
                // цикл продолжается, пока есть положительные приращения deltaL
                while (flag == true)
                {
                    stepMsg = $" -- Рассматриваем столбцы платы №{boardNum}.\n";
                    // создание матрицы со связями для текущей платы
                    Matrix<int> n = new Matrix<int>(boardMatrix.RowsCount, boardMatrix.ColsCount);
                    n.Fill(0);
                    // цикл по столбцами (слева направо); последний столбец не рассматриваем
                    for (coloumn = 0; coloumn < boardMatrix.ColsCount - 1; coloumn++)
                    {
                        // цикл по элементам столбца (сверху вниз)
                        for (row = 0; row < boardMatrix.RowsCount; row++)
                        {
                            // проверка, размещен ли элемент в этом месте платы
                            if (boardMatrix[row, coloumn] == -1)
                                continue;
                            // цикл по оставшимся столбцам матрицы для подсчета связей текущего столбца с каждым
                            for (coloumn_next = coloumn + 1; coloumn_next < boardMatrix.ColsCount; coloumn_next++)
                            {
                                // цикл по элементам следующих столбцов для подсчета связей
                                // текущего элемента текущего столбца с каждым из элементов следующих столбцов
                                for (row_next = 0; row_next < boardMatrix.RowsCount; row_next++)
                                {
                                    // проверка, размещен ли элемент в этом месте платы
                                    if (boardMatrix[row_next, coloumn_next] == -1)
                                        continue;
                                    // добавление к ячейке n очередной связи (из матрицы R) текущего  
                                    // элемента текущего столбца с очередным элементом одного из следующих столбцов
                                    n[coloumn, coloumn_next] += R[boardMatrix[row, coloumn], boardMatrix[row_next, coloumn_next]];
                                    // заполняем матрицу зеркально ниже нулевой диагонали
                                    n[coloumn_next, coloumn] += R[boardMatrix[row, coloumn], boardMatrix[row_next, coloumn_next]];
                                }
                            }
                        }
                    }
                    // вычисление deltaL для определения пар столбцов, которые надо поменять местами
                    List<int> deltaL = new List<int>(n.ColsCount - 1);
                    // цикл для вычисления каждого deltaL
                    for (index = 0; index < deltaL.Capacity; index++)
                    {
                        stepMsg += $"\u0394L{index + 1}-{index + 2} = ";
                        // это просто 2 списка для формирования сообщения шага
                        List<string> operationsDef = new List<string>(); // в этом хранится буквенное обозначение операции, например: (r1 - r2)*(d1 - d2)
                        List<string> operationsValue = new List<string>(); // а тут именно числовое (5 - 8)*(1 - 3)

                        // список номеров столбцов, кроме двух текущих
                        List<int> other = new List<int>();
                        for (i = 0; i < deltaL.Capacity + 1; i++)
                        {
                            if (i != index && i != index + 1)
                                other.Add(i);
                        }
                        // обнуление текущего L для его наращивания в цикле
                        deltaL.Add(0);
                        // цикл по списку не текущих столбцов. в каждом проходе реализация формулы k*(l13 - l23) (числа для примера)
                        for (j = 0; j < other.Count; j++)
                        {
                            // расстояние между столбцами
                            coef = Math.Abs(index - other[j]) - Math.Abs(index + 1 - other[j]);
                            // формула k*(l13 - l23)
                            deltaL[index] += coef * (n[index, other[j]] - n[index + 1, other[j]]);

                            operationsDef.Add($"(r{index + 1}-{other[j] + 1} - r{index + 2}-{other[j] + 1})*" +
                                    $"(d{index + 1}-{other[j] + 1} - d{index + 2}-{other[j] + 1})");

                            operationsValue.Add($"({n[index, other[j]]}-{n[index + 1, other[j]]})*" +
                                $"({Math.Abs(index - other[j])}-{Math.Abs(index + 1 - other[j])})");
                        }
                        // добавляем к сообщению шага сразу все описания операций
                        stepMsg += string.Join(" + ", operationsDef);
                        stepMsg += " = ";
                        // потом все подставленные значения
                        stepMsg += string.Join(" + ", operationsValue);
                        stepMsg += " = ";
                        stepMsg += deltaL[index] + ".\n";
                    }

                    // наибольшее значения из списка deltaL
                    maxL = 0;
                    // номер столбца, который нужно поменять местами с его соседом справа
                    exchange_coloumn = -1;
                    // цикл для поиска положительных значений в списке deltaL для определения пар столбцов, которые надо поменять местами
                    for (i = 0; i < deltaL.Count; i++)
                    {
                        if (deltaL[i] > maxL)
                        {
                            maxL = deltaL[i];
                            exchange_coloumn = i;
                        }
                    }
                    // проверка необходимости перестановки столбцов
                    if (maxL > 0)
                    {
                        stepMsg += $" -- Наибольшее \u0394L у столбцов {exchange_coloumn + 1} и {exchange_coloumn + 2} = {maxL}. Меняем их местами.";
                        // перестановка пары столбцов
                        for (row = 0; row < boardMatrix.RowsCount; row++)
                        {
                            buf = boardMatrix[row, exchange_coloumn];
                            boardMatrix[row, exchange_coloumn] = boardMatrix[row, exchange_coloumn + 1];
                            boardMatrix[row, exchange_coloumn + 1] = buf;
                        }
                        flag = true;
                    }
                    else
                    {
                        stepMsg += " -- Положительных приращений нет";
                        flag = false;
                    }

                    var step = new StepPlacementLog(boardsMatrices, stepMsg);
                    log.Add(step);
                }

                // цикл по строкам платы
                flag = true;
                // цикл продолжается, пока есть положительные приращения deltaL
                while (flag == true)
                {
                    stepMsg = $" -- Рассматриваем строки платы №{boardNum}.\n";
                    // создание матрицы со связями для текущей платы
                    Matrix<int> n = new Matrix<int>(boardMatrix.ColsCount, boardMatrix.RowsCount);
                    n.Fill(0);
                    // цикл по строкам (сверху вниз); последнюю строку не рассматриваем
                    for (row = 0; row < boardMatrix.RowsCount - 1; row++)
                    {
                        // цикл по элементам строки (слева направо)
                        for (coloumn = 0; coloumn < boardMatrix.ColsCount; coloumn++)
                        {
                            // проверка, размещен ли элемент в этом месте платы
                            if (boardMatrix[row, coloumn] == -1)
                                continue;
                            // цикл по оставшимся строкам матрицы для подсчета связей текущей строки с каждой
                            for (row_next = row + 1; row_next < boardMatrix.RowsCount; row_next++)
                            {
                                // цикл по элементам следующих строк для подсчета связей
                                // текущего элемента текущей строки с каждым из элементов следующих строк
                                for (coloumn_next = 0; coloumn_next < boardMatrix.ColsCount; coloumn_next++)
                                {
                                    // проверка, размещен ли элемент в этом месте платы
                                    if (boardMatrix[row_next, coloumn_next] == -1)
                                        continue;
                                    // добавление к ячейке n очередной связи (из матрицы R) текущего  
                                    // элемента текущей строки с очередным элементом одной из следующих строк
                                    n[coloumn, coloumn_next] += R[boardMatrix[row, coloumn], boardMatrix[row_next, coloumn_next]];
                                    // заполняем матрицу зеркально ниже нулевой диагонали
                                    n[coloumn_next, coloumn] += R[boardMatrix[row, coloumn], boardMatrix[row_next, coloumn_next]];
                                }
                            }
                        }
                    }
                    // вычисление deltaL для определения пар строк, которые надо поменять местами
                    List<int> deltaL = new List<int>(n.RowsCount - 1);
                    // цикл для вычисления каждого deltaL
                    for (index = 0; index < deltaL.Capacity; index++)
                    {
                        stepMsg += $"\u0394L{index + 1}-{index + 2} = ";
                        // это просто 2 списка для формирования сообщения шага
                        List<string> operationsDef = new List<string>(); // в этом хранится буквенное обозначение операции, например: (r1 - r2)*(d1 - d2)
                        List<string> operationsValue = new List<string>(); // а тут именно числовое (5 - 8)*(1 - 3)

                        // список номеров строк, кроме двух текущих
                        List<int> other = new List<int>();
                        for (i = 0; i < deltaL.Capacity + 1; i++)
                        {
                            if (i != index && i != index + 1)
                                other.Add(i);
                        }
                        // обнуление текущего L для его наращивания в цикле
                        deltaL.Add(0);
                        // цикл по списку не текущих столбцов. в каждом проходе реализация формулы k*(l13 - l23) (числа для примера)
                        for (j = 0; j < other.Count; j++)
                        {
                            // расстояние между строками
                            coef = Math.Abs(index - other[j]) - Math.Abs(index + 1 - other[j]);
                            // формула k*(l13 - l23)
                            deltaL[index] += coef * (n[index, other[j]] - n[index + 1, other[j]]);

                            operationsDef.Add($"(r{index + 1}-{other[j] + 1} - r{index + 2}-{other[j] + 1})*" +
                                    $"(d{index + 1}-{other[j] + 1} - d{index + 2}-{other[j] + 1})");

                            operationsValue.Add($"({n[index, other[j]]}-{n[index + 1, other[j]]})*" +
                                $"({Math.Abs(index - other[j])}-{Math.Abs(index + 1 - other[j])})");
                        }
                        // добавляем к сообщению шага сразу все описания операций
                        stepMsg += string.Join(" + ", operationsDef);
                        stepMsg += " = ";
                        // потом все подставленные значения
                        stepMsg += string.Join(" + ", operationsValue);
                        stepMsg += " = ";
                        stepMsg += deltaL[index] + ".\n";
                    }

                    // наибольшее значения из списка deltaL
                    maxL = 0;
                    // номер строки, которую нужно поменять местами с её соседом снизу
                    exchange_row = -1;
                    // цикл для поиска положительных значений в списке deltaL для определения пар строк, которые надо поменять местами
                    for (i = 0; i < deltaL.Count; i++)
                    {
                        if (deltaL[i] > maxL)
                        {
                            maxL = deltaL[i];
                            exchange_row = i;
                        }
                    }
                    // проверка необходимости перестановки строк
                    if (maxL > 0)
                    {
                        stepMsg += $" -- Наибольшее \u0394L у строк {exchange_row + 1} и {exchange_row + 2} = {maxL}. Меняем их местами.";
                        // перестановка пары столбцов
                        for (row = 0; row < boardMatrix.RowsCount; row++)
                        {
                            buf = boardMatrix[row, exchange_row];
                            boardMatrix[row, exchange_row] = boardMatrix[row, exchange_row + 1];
                            boardMatrix[row, exchange_row + 1] = buf;
                        }
                        flag = true;
                    }
                    else
                    {
                        stepMsg += " -- Положительных приращений нет";
                        flag = false;
                    }

                    var step = new StepPlacementLog(boardsMatrices, stepMsg);
                    log.Add(step);
                }
            }

            return log;
        }
    }
}