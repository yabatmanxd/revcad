using RevolutionCAD.Placement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionCAD.Tracing
{
    /// <summary>
    /// Трассировка по методу равномерного распределения проводников
    /// </summary>
    public class TracingUniformDistribution
    {
        /// <summary>
        /// Метод для трассировки
        /// </summary>
        /// <returns>Список логов шагов</returns>
        public static List<StepTracingLog> Trace(Scheme sch, PlacementResult plc, out string err)
        {
            // обязательно создаём лог действий
            var log = new List<StepTracingLog>();

            // при возникновении критической ошибки её нужно занести в эту переменную и сделать return null
            err = "";

            // формируем список плат, в котором хранится список слоёв (для каждого проводника свой слой ДРП)
            var boards = new List<List<Matrix<Cell>>>();

            // считываем список плат, в каждой плате хранится список проводников (Wire) соединяют всего 2 контакта
            List<List<Wire>> boardsWiresPositions = plc.BoardsWiresPositions;

            for (int boardNum = 0; boardNum < boardsWiresPositions.Count; boardNum++)
            {
                // получаем список проводов, которые необходимо развести на этой плате
                // элемент списка объект класса Wire, который хранит координаты точки А и Б на ДРП
                var boardWiresPositions = boardsWiresPositions[boardNum];

                // список ДРП текущей платы, в который будет заносится ДРП для каждого провода 
                var boardDRPs = new List<Matrix<Cell>>();

                // добавляем этот список в список всех плат
                boards.Add(boardDRPs);

                // первым слоем будет являтся ДРП на котором просто отмечены места контактов платы
                boardDRPs.Add(plc.BoardsDRPs[boardNum]);

                // запускаем цикл по проводам
                foreach (var wire in boardWiresPositions)
                {
                    // ДРП для провода формируем на основе шаблона ДРП, на котором просто отмечены контакты. Оно уже имеет необходимые размеры.
                    var currentDRP = new Matrix<Cell>(plc.BoardsDRPs[boardNum].RowsCount, plc.BoardsDRPs[boardNum].ColsCount);

                    // добавляем это ДРП в список ДРП платы
                    boardDRPs.Add(currentDRP);

                    // заполняем ДРП пустыми ячейками
                    for (int i = 0; i < currentDRP.RowsCount; i++)
                    {
                        for (int j = 0; j < currentDRP.ColsCount; j++)
                        {
                            currentDRP[i, j] = new Cell();
                        }
                    }

                    // дрп, в котором будут объединены все слои с проводами (для определения гдле занята ячейка - где нет)
                    Matrix<Cell> fullDrp;

                    // получаем из провода стартовую и конечную позицию
                    var startPos = wire.A.PositionContact;
                    var endPos = wire.B.PositionContact;

                    // список, в котором будут хранится приоритеты (смещение относительно текущей ячейки)
                    // если это стрелочка вверх, значит должна анализироваться верхняя ячейка
                    // смещение верхней ячейки относительно текущей: на строку выше, значит -1, а столбец тот же, значит 0
                    // если это стрелочка влево, значит относительно текущей ячейки это будет столбец левее, значит -1, а строка та же, значит 0
                    List<Position> prioritetsPos = new List<Position>();

                    // вычисляем разницу между значениями строк и столбцов точек А и Б
                    int rowsDiff = endPos.Row - startPos.Row;
                    int colsDiff = endPos.Column - startPos.Column;

                    // а это модули этих значений, тоже понадобятся
                    int rowsDiffModul = Math.Abs(rowsDiff);
                    int colsDiffModul = Math.Abs(colsDiff);

                    // определяем как раз таки позиции приоритетов, логику которых я чуть выше описывал
                    Position top = new Position(-1, 0);
                    Position bottom = new Position(1, 0);
                    Position left = new Position(0, -1);
                    Position right = new Position(0, 1);

                    // далее идёт задание приоритетов на основе подсчитанных разниц позиций
                    // логическому объяснению не поддаётся, только по результату моделирования на бумаге
                    if (colsDiff > rowsDiff)
                    {
                        if (colsDiffModul > rowsDiffModul)
                        {
                            if (rowsDiff < 0)
                                prioritetsPos = getPrioritets(0, false); // первый приоритет - стрелочка влево, остальные против часовой стрелки назначаем
                            else
                                prioritetsPos = getPrioritets(0, true); // первый приоритет - стрелочка влево, остальные по часовой стрелке назначаем
                        }
                        else
                        {
                            if (colsDiff > 0)
                                prioritetsPos = getPrioritets(90, true); // первый приоритет - стрелочка вниз, остальные по часовой стрелке назначаем
                            else
                                prioritetsPos = getPrioritets(90, false); // первый приоритет - стрелочка вниз, остальные против часовой стрелки назначаем
                        }
                    }
                    else
                    {
                        if (colsDiffModul > rowsDiffModul)
                        {
                            if (rowsDiff > 0)
                                prioritetsPos = getPrioritets(180, false); // первый приоритет - стрелочка вправо, остальные против часовой стрелки назначаем
                            else
                                prioritetsPos = getPrioritets(180, true); // первый приоритет - стрелочка вправо, остальные по часовой стрелке назначаем
                        }
                        else
                        {
                            if (colsDiff < 0)
                                prioritetsPos = getPrioritets(270, true); // первый приоритет - стрелочка вверх, остальные по часовой стрелке назначаем
                            else
                                prioritetsPos = getPrioritets(270, false); // первый приоритет - стрелочка вверх, остальные против часовой стрелки назначаем
                        }
                    }

                    // помечаем буквами стартовую и конечную позицию на текущем слое провода
                    currentDRP[startPos.Row, startPos.Column].State = CellState.PointA;
                    currentDRP[startPos.Row, startPos.Column].Weight = 0;
                    currentDRP[endPos.Row, endPos.Column].State = CellState.PointB;

                    // список позиций соседних ячеек
                    var neighbors = new List<Position>();

                    // объединяем все слои с проводами платы, чтобы на основе этого ДРП получить список именно незанятых соседей
                    fullDrp = ApplicationData.MergeLayersDRPs(boardDRPs);

                    // получаем список незанятых соседей у стартовой точки 
                    neighbors = getNeighbors(fullDrp, startPos);

                    // запускаем цикл, пока не закончатся свободные соседи или в списке соседей будет точка А
                    do
                    {
                        // проходим по всем соседним ячейкам чтобы поставить в них стрелочку в какую-то сторону
                        foreach (var neighbor in neighbors)
                        {
                            int minWeight = int.MaxValue;

                            // для этого проходим по всем приоритетам
                            foreach (var prioritet in prioritetsPos)
                            {
                                // определяем позицию, которую нужно проверить на основе текущего приоритета
                                // если первый приоритет стрелочка вверх - значит мы сначала должна проверить верхнюю ячейку на наличие стрелочки
                                // если она там обнаружится, то поставить в текущей ячейке стрелочку вверх, если нет, то перейти к следующему приоритету, проверить наличие стрелочки в ячейке по этому приоритету и т.д.
                                Position checkingPos = new Position(neighbor.Row + prioritet.Row, neighbor.Column + prioritet.Column);
                                
                                // определяем находится ли проверяемая позиция ваще в дрп, вдруг мы вылезли за какой-то край
                                if (checkingPos.Row >= 0 && checkingPos.Row < currentDRP.RowsCount)
                                {
                                    if (checkingPos.Column >= 0 && checkingPos.Column < currentDRP.ColsCount)
                                    {
                                        // если в проверяемой позиции есть стрелочка или точка А (к нему тоже нужно провести стрелочки на первом шаге)
                                        if (fullDrp[checkingPos.Row, checkingPos.Column].isArrow || fullDrp[checkingPos.Row, checkingPos.Column].State == CellState.PointA)
                                        {
                                            // устанавливаем необходимую стрелочку по значениям смещения этой стрелочки
                                            int currentWeight = currentDRP[neighbor.Row + prioritet.Row, neighbor.Column + prioritet.Column].Weight + getCountWiresNear(fullDrp, neighbor);
                                            if (currentWeight < minWeight)
                                            {
                                                minWeight = currentWeight;
                                                currentDRP[neighbor.Row, neighbor.Column].State = getArrowByPrioritet(prioritet.Row, prioritet.Column);
                                                currentDRP[neighbor.Row, neighbor.Column].Weight = currentWeight;
                                            }
                                        }
                                    }
                                }
                            }


                        }
                        log.Add(new StepTracingLog(boards, $"Распроcтраняем волну для {boardDRPs.Count - 1}-го проводника в {boardNum + 1} узле"));

                        fullDrp = ApplicationData.MergeLayersDRPs(boardDRPs);
                        neighbors = getNeighbors(fullDrp, neighbors);

                    } while (neighbors.Count > 0 && !neighbors.Any(x => x.Column == endPos.Column && x.Row == endPos.Row));

                    // если незанятых соседей не оказалось, значит трассировка невозможна
                    if (neighbors.Count == 0)
                    {
                        // очищаем текущее дрп
                        for (int i = 0; i < currentDRP.RowsCount; i++)
                        {
                            for (int j = 0; j < currentDRP.ColsCount; j++)
                            {
                                currentDRP[i, j] = new Cell();
                            }
                        }
                        // оставляем только 2 контакта, которые должны быть соеденены
                        currentDRP[startPos.Row, startPos.Column].State = CellState.Contact;
                        currentDRP[endPos.Row, endPos.Column].State = CellState.Contact;
                        log.Add(new StepTracingLog(boards, $"Невозможно выполнить трассировку {boardDRPs.Count - 1}-го проводника в {boardNum + 1} узле"));
                        continue;
                    }

                    // теперь начинаем с точки Б
                    // находим соседние ячейки точки Б в которых есть стрелочка и берём первую попавшуюся (это не важно)
                    var currentPos = getNeighborWithMinWeight(currentDRP, endPos);

                    // запускаем цикл, пока мы по этим стрелочкам не дойдём то точки А
                    do
                    {
                        // запомним какая стрелочка была в текущей ячейке, т.к. сейчас перезапишем состояние ячейки
                        var bufCellState = currentDRP[currentPos.Row, currentPos.Column].State;

                        // указываем что в этой позиции будет провод, а какой именно - вертикальный, горизонтальный это определим потом
                        currentDRP[currentPos.Row, currentPos.Column].State = CellState.Wire;

                        // теперь на основе стрелочки определяем в какую ячейку мы смещаемся. если это стрелочка влево - то столбец будет меньше на 1
                        // если вправо - то столбец на 1 больше, если стрелочка вверх, то на 1 строку выше
                        currentPos = getNextPosByCurrentArrow(currentPos, bufCellState);

                    } while (currentDRP[currentPos.Row, currentPos.Column].State != CellState.PointA);

                    // очищаем всё дрп от стрелочек и веса
                    for (int i = 0; i < currentDRP.RowsCount; i++)
                    {
                        for (int j = 0; j < currentDRP.ColsCount; j++)
                        {
                            currentDRP[i, j].Weight = -1;
                            if (currentDRP[i, j].isArrow)
                            {
                                currentDRP[i, j].State = CellState.Empty;
                            }

                        }
                    }

                    log.Add(new StepTracingLog(boards, $"Волна достигла точки Б. Определяем точки, где будет проходить проводник №{boardDRPs.Count - 1} в {boardNum + 1} узле"));

                    // начинаем долгую и мучительную спец операцию по определению какой формы проводник должен стоять в ячейке
                    // запускаем цикл по всем ячейкам дрп
                    for (int i = 0; i < currentDRP.RowsCount; i++)
                    {
                        for (int j = 0; j < currentDRP.ColsCount; j++)
                        {
                            // объявляем соседей, от них нам нужно будет только состояние
                            Cell leftCell = new Cell();
                            Cell rightCell = new Cell();
                            Cell topCell = new Cell();
                            Cell bottomCell = new Cell();

                            // блок, который присвоит пустое состояние ячейке, если она находится вне дрп, если находится в дрп, то присвоит нужную позицию
                            if (j > 0)
                                leftCell = currentDRP[i, j - 1];
                            else
                                leftCell.State = CellState.Empty;

                            if (j < currentDRP.ColsCount - 1)
                                rightCell = currentDRP[i, j + 1];
                            else
                                rightCell.State = CellState.Empty;

                            if (i > 0)
                                topCell = currentDRP[i - 1, j];
                            else
                                topCell.State = CellState.Empty;

                            if (i < currentDRP.RowsCount - 1)
                                bottomCell = currentDRP[i + 1, j];
                            else
                                bottomCell.State = CellState.Empty;
                            // конец блока

                            var currentCell = currentDRP[i, j];

                            // если текущая ячейка должна быть каким-то кабелем
                            // определяем значения ячеек вокруг и на основе этих данных узнаём какой имеено должен быть кабель
                            // идущим вертикально или слева вверх или горизонтальным и т.д.
                            if (currentCell.State == CellState.Wire)
                            {
                                // если есть кабель сверху и кабель в ячейке снизу, то в текущей ячейке должен стоять вертикальный проводник
                                if (topCell.isConnectible && bottomCell.isConnectible)
                                {
                                    currentDRP[i, j].State = CellState.WireVertical;
                                } // и т.д. для остальных видов проводника
                                else if (leftCell.isConnectible && rightCell.isConnectible)
                                {
                                    currentDRP[i, j].State = CellState.WireHorizontal;
                                }
                                else if (topCell.isConnectible && leftCell.isConnectible)
                                {
                                    currentDRP[i, j].State = CellState.WireTopLeft;
                                }
                                else if (topCell.isConnectible && rightCell.isConnectible)
                                {
                                    currentDRP[i, j].State = CellState.WireTopRight;
                                }
                                else if (bottomCell.isConnectible && leftCell.isConnectible)
                                {
                                    currentDRP[i, j].State = CellState.WireBottomLeft;
                                }
                                else if (bottomCell.isConnectible && rightCell.isConnectible)
                                {
                                    currentDRP[i, j].State = CellState.WireBottomRight;
                                }
                            }

                        }
                    }

                    // заменяем буквы просто контактами
                    currentDRP[startPos.Row, startPos.Column].State = CellState.Contact;
                    currentDRP[endPos.Row, endPos.Column].State = CellState.Contact;
                    log.Add(new StepTracingLog(boards, $"Построили на базе точек проводник №{boardDRPs.Count - 1} в {boardNum + 1} узле"));
                }


            }



            return log;
        }

        /// <summary>
        /// Формирует список позиций приоритетов на основе стартового угла и флага по часовой стрелке назначать следующие или против
        /// </summary>
        public static List<Position> getPrioritets(int startAngle, bool isClockwise)
        {
            int currentAngle = startAngle;

            var priors = new List<Position>();
            for (int i = 0; i < 4; i++)
            {
                double angle = Math.PI * currentAngle / 180.0;
                int row = (int)Math.Sin(angle);
                int col = (int)-Math.Cos(angle);
                var pos = new Position(row, col);
                priors.Add(pos);
                currentAngle += isClockwise ? -90 : 90;
            }
            return priors;
        }

        /// <summary>
        /// Возвращает количество проводов в соседних ячейках
        /// </summary>
        private static int getCountWiresNear(Matrix<Cell> drp, Position pos)
        {
            // список позиций незанятых соседей
            var neighbors = new List<Position>();
            // список претендентов на незанятого соседа
            var aplicants = new List<Position>();

            for (int i = 0; i < 4; i++)
            {
                aplicants.Add(pos.Clone());
            }

            aplicants[0].Column += 1; // правый сосед
            aplicants[1].Column -= 1; // левый сосед
            aplicants[2].Row -= 1; // верхний сосед
            aplicants[3].Row += 1; // нижний сосед

            foreach (var aplicant in aplicants)
            {
                // проверка на то, находится ли сосед в пределах дрп
                if (aplicant.Row >= 0 && aplicant.Row < drp.RowsCount)
                {
                    if (aplicant.Column >= 0 && aplicant.Column < drp.ColsCount)
                    {
                        // если находится в пределах дрп и в нём есть прводник
                        if (drp[aplicant.Row, aplicant.Column].isConnectible && drp[aplicant.Row, aplicant.Column].State != CellState.PointA && drp[aplicant.Row, aplicant.Column].State != CellState.PointB)
                        {
                            neighbors.Add(aplicant);
                        }
                    }
                }
            }

            return neighbors.Count;
        }

        /// <summary>
        /// Метод для определения по координатам в какую сторону должна смотреть стрелочка
        /// </summary>
        public static CellState getArrowByPrioritet(int row, int col)
        {
            // если у этой позиции значение строки -1 - значит стрелочка должна смотреть вверх и т.д.
            if (row == -1)
                return CellState.ArrowUp;
            if (row == 1)
                return CellState.ArrowDown;
            if (col == -1)
                return CellState.ArrowLeft;
            if (col == 1)
                return CellState.ArrowRight;
            else
                return CellState.Empty;
        }

        /// <summary>
        /// Метод для определения какая будет следующая позиция, на основе текущей позиции и значения куда смотрит стрелочка
        /// </summary>
        public static Position getNextPosByCurrentArrow(Position currentPos, CellState currentArrow)
        {
            switch (currentArrow)
            {
                case CellState.ArrowDown:
                    return new Position(currentPos.Row + 1, currentPos.Column);
                case CellState.ArrowUp:
                    return new Position(currentPos.Row - 1, currentPos.Column);
                case CellState.ArrowLeft:
                    return new Position(currentPos.Row, currentPos.Column - 1);
                case CellState.ArrowRight:
                    return new Position(currentPos.Row, currentPos.Column + 1);
                default:
                    return new Position(-1, -1);
            }
        }

        /// <summary>
        /// Метод для получения позиция соседних незанятых ячеек списка ячеек. Принимает ДРП по которому определять незанятые и список ячеек для которых необходимо получить соседей
        /// </summary>
        public static List<Position> getNeighbors(Matrix<Cell> drp, List<Position> positions)
        {
            var allNeighbors = new List<Position>();

            foreach (var pos in positions)
            {
                // используя перегруженный метод получаем список соседних ячеек для одной ячейки
                var neighbors = getNeighbors(drp, pos);
                foreach (var neighbor in neighbors)
                {
                    // если такого соседа ещё нет в списке соседей, то добавляем
                    if (!allNeighbors.Any(x => x.Column == neighbor.Column && x.Row == neighbor.Row))
                    {
                        allNeighbors.Add(neighbor);
                    }
                }
            }

            return allNeighbors;
        }

        /// <summary>
        /// Перегруженный метод для получения позиций соседних незанятых ячеек одной ячейки. Принимает ДРП по которому определять незанятые и ячейку для которой необходимо получить соседей
        /// </summary>
        public static List<Position> getNeighbors(Matrix<Cell> drp, Position pos)
        {
            // список позиций незанятых соседей
            var neighbors = new List<Position>();
            // список претендентов на незанятого соседа
            var aplicants = new List<Position>();

            for (int i = 0; i < 4; i++)
            {
                aplicants.Add(pos.Clone());
            }

            aplicants[0].Column += 1; // правый сосед
            aplicants[1].Column -= 1; // левый сосед
            aplicants[2].Row -= 1; // верхний сосед
            aplicants[3].Row += 1; // нижний сосед

            foreach (var aplicant in aplicants)
            {
                // проверка на то, находится ли сосед в пределах дрп
                if (aplicant.Row >= 0 && aplicant.Row < drp.RowsCount)
                {
                    if (aplicant.Column >= 0 && aplicant.Column < drp.ColsCount)
                    {
                        // если находится и не занят, то этот предендент нам подходит
                        if (drp[aplicant.Row, aplicant.Column].isBusy == false)
                        {
                            neighbors.Add(aplicant);
                        }
                    }
                }
            }

            return neighbors;

        }

        /// <summary>
        /// Метод для получения списка соседей, которые стрелочки
        /// </summary>
        public static Position getNeighborWithMinWeight(Matrix<Cell> drp, Position pos)
        {
            var aplicants = new List<Position>();

            for (int i = 0; i < 4; i++)
            {
                aplicants.Add(pos.Clone());
            }

            aplicants[0].Column += 1;
            aplicants[1].Column -= 1;
            aplicants[2].Row -= 1;
            aplicants[3].Row += 1;

            int minWeight = int.MaxValue;
            var neighbor = new Position(-1, -1);

            foreach (var aplicant in aplicants)
            {
                if (aplicant.Row >= 0 && aplicant.Row < drp.RowsCount)
                {
                    if (aplicant.Column >= 0 && aplicant.Column < drp.ColsCount)
                    {
                        if (drp[aplicant.Row, aplicant.Column].isArrow)
                        {
                            if (drp[aplicant.Row, aplicant.Column].Weight < minWeight)
                            {
                                minWeight = drp[aplicant.Row, aplicant.Column].Weight;
                                neighbor = aplicant.Clone();
                            }
                        }
                    }
                }
            }

            return neighbor;

        }
    }
}
