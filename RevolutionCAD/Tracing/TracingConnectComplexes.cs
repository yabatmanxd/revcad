using RevolutionCAD.Placement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionCAD.Tracing
{
    /// <summary>
    /// Трассировка по методу соединительных комплексов
    /// </summary>
    public class TracingConnectComplexes
    {
        /// <summary>
        /// Метод для трассировки
        /// </summary>
        /// <returns>Список логов шагов</returns>
        public static List<StepTracingLog> Trace(Scheme sch, PlacementResult plc, bool isOptimized, out string err)
        {
            // обязательно создаём лог действий
            var log = new List<StepTracingLog>();

            // при возникновении критической ошибки её нужно занести в эту переменную и сделать return null
            err = "";

            // формируем список плат, в котором хранится список слоёв (для каждого проводника свой слой ДРП)
            var boards = new List<List<Matrix<Cell>>>();

            // считываем список плат, в каждой плате хранится список проводников (Wire) соединяют всего 2 контакта
            List<List<Wire>> boardsWiresPositions = plc.BoardsWiresPositions;

            // запускаем цикл по платам
            for (int boardNum = 0; boardNum < boardsWiresPositions.Count; boardNum++)
            {
                // получаем список проводов, которые необходимо развести на этой плате
                // элемент списка объект класса Wire, который хранит координаты точки А и Б на ДРП
                var boardWiresPositions = boardsWiresPositions[boardNum];

                // список ДРП текущей платы, в который будет заносится ДРП для каждого провода 
                var boardDRPs = new List<Matrix<Cell>>();

                // добавляем этот список в список всех плат
                boards.Add(boardDRPs);

                Matrix<Cell> currentDRP = null;

                // первым слоем будет являтся ДРП на котором просто отмечены места контактов платы
                boardDRPs.Add(plc.BoardsDRPs[boardNum]);

                // запускаем цикл по проводам
                for (int wireNum = 0; wireNum < boardWiresPositions.Count; wireNum++)
                {
                    var wire = boardWiresPositions[wireNum];
                    
                    bool isNeedToCreateWireDrp = true;

                    if (wireNum > 0)
                    {
                        var prevWire = boardWiresPositions[wireNum - 1];
                        if (wire.A.PositionContact.Equals(prevWire.B.PositionContact))
                        {
                            isNeedToCreateWireDrp = false;
                            currentDRP = boardDRPs.Last();
                        }
                    }

                    if (isNeedToCreateWireDrp)
                    {
                        currentDRP = new Matrix<Cell>(plc.BoardsDRPs[boardNum].RowsCount, plc.BoardsDRPs[boardNum].ColsCount);
                        
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
                    }
                    

                    // дрп, в котором будут объединены все слои с проводами (для определения где занята ячейка - где нет)
                    Matrix<Cell> fullDrp;

                    // получаем из провода стартовую и конечную позицию
                    var startPos = wire.A.PositionContact;
                    var endPos = wire.B.PositionContact;

                    // помечаем буквами стартовую и конечную позицию на текущем слое провода
                    currentDRP[startPos.Row, startPos.Column].State = CellState.PointA;
                    currentDRP[endPos.Row, endPos.Column].State = CellState.PointB;

                    log.Add(new StepTracingLog(boards, $"Начинаем трассировку {wireNum + 1}-го проводника в {boardNum + 1} узле"));

                    int currentWeight = 0;

                    // у начальной позиции вес 0
                    currentDRP[startPos.Row, startPos.Column].Weight = currentWeight;

                    // объединяем все слои
                    fullDrp = ApplicationData.MergeLayersDRPs(boardDRPs);

                    var neighbors = new List<Position>();

                    if(isNeedToCreateWireDrp)
                    {
                        neighbors = getNeighbors(fullDrp, startPos);
                    } else
                    {
                        for (int i = 0; i < currentDRP.RowsCount; i++)
                        {
                            for (int j = 0; j < currentDRP.ColsCount; j++)
                            {
                                if (currentDRP[i, j].isConnectible && currentDRP[i, j].State != CellState.PointB)
                                {
                                    neighbors.Add(new Position(i, j));
                                }
                            }
                        }
                        neighbors = getNeighbors(fullDrp, neighbors);
                    }

                    // запускаем цикл пока не будет найдено ни одного незанятого соседа или в списке соседей окажется точка Б 
                    do
                    {
                        // увеличиваем вес
                        currentWeight++;

                        // запускаем цикл по всем незанятым соседним ячейкам
                        foreach (var neighbor in neighbors)
                        {
                            // распространяем волну
                            currentDRP[neighbor.Row, neighbor.Column].Weight = currentWeight;
                            currentDRP[neighbor.Row, neighbor.Column].State = CellState.Wave;
                            fullDrp[neighbor.Row, neighbor.Column].Weight = currentWeight;
                            fullDrp[neighbor.Row, neighbor.Column].State = CellState.Wave;
                        }

                        if (!isOptimized)
                            log.Add(new StepTracingLog(boards, $"Распроcтраняем волну с весом {currentWeight} для {wireNum + 1}-го проводника в {boardNum + 1} узле"));

                        // и получаем список незанятых соседей для ячеек текущей волны
                        neighbors = getNeighbors(fullDrp, neighbors);

                    } while (neighbors.Count > 0 && !neighbors.Any(x => x.Equals(endPos)));

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
                        log.Add(new StepTracingLog(boards, $"Невозможно выполнить трассировку {wireNum + 1}-го проводника в {boardNum + 1} узле"));
                        continue;
                    }

                    if (isOptimized)
                        log.Add(new StepTracingLog(boards, $"Волна {wireNum + 1}-го проводника достигла точки Б в {boardNum + 1} узле"));


                    // теперь начинаем обратный крестовый поход от точки Б
                    neighbors = new List<Position>();
                    neighbors.Add(endPos);

                    Position currentPos = endPos.Clone();
                    fullDrp = ApplicationData.MergeLayersDRPs(boardDRPs);
                    do
                    {
                        // получаем список волновых соседей
                        neighbors = getNeighborsOnlyWave(fullDrp, currentPos);

                        foreach (var neighbor in neighbors)
                        {
                            // находим в списке соседей первую ячейку с волной необходимого веса
                            if (currentDRP[neighbor.Row, neighbor.Column].Weight == currentWeight)
                            {
                                // помечаем что в этой ячейке будет находится проводник, но какой имеено определим ниже
                                currentDRP[neighbor.Row, neighbor.Column].State = CellState.Wire;
                                fullDrp[neighbor.Row, neighbor.Column].State = CellState.Wire;
                                currentPos = neighbor.Clone();
                                break;
                            }
                        }

                        // на каждом шаге уменьшаем вес
                        currentWeight--;

                    } while (currentWeight >= 0);

                    // очищаем всё дрп от веса и ячеек с волной
                    for (int i = 0; i < currentDRP.RowsCount; i++)
                    {
                        for (int j = 0; j < currentDRP.ColsCount; j++)
                        {
                            currentDRP[i, j].Weight = -1;
                            if (currentDRP[i, j].State == CellState.Wave)
                            {
                                currentDRP[i, j].State = CellState.Empty;
                            }
                        }
                    }

                    log.Add(new StepTracingLog(boards, $"Волна достигла точки Б. Определяем точки, где будет проходить проводник №{wireNum + 1} в {boardNum + 1} узле"));

                    
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
                            if (currentCell.isConnectibleOrContact && currentCell.State != CellState.PointA && currentCell.State != CellState.PointB)
                            {
                                // если есть кабель сверху и кабель в ячейке снизу, то в текущей ячейке должен стоять вертикальный проводник
                                if (topCell.isConnectibleOrContact && bottomCell.isConnectibleOrContact && leftCell.isConnectibleOrContact && rightCell.isConnectibleOrContact)
                                {
                                    currentDRP[i, j].State = CellState.WireCross;
                                }
                                else if (topCell.isConnectibleOrContact && bottomCell.isConnectibleOrContact && leftCell.isConnectibleOrContact)
                                {
                                    currentDRP[i, j].State = CellState.WireLeftTopBottom;
                                }
                                else if (topCell.isConnectibleOrContact && bottomCell.isConnectibleOrContact && rightCell.isConnectibleOrContact)
                                {
                                    currentDRP[i, j].State = CellState.WireRightTopBottom;
                                }
                                else if (leftCell.isConnectibleOrContact && rightCell.isConnectibleOrContact && topCell.isConnectibleOrContact)
                                {
                                    currentDRP[i, j].State = CellState.WireLeftRightTop;
                                }
                                else if (leftCell.isConnectibleOrContact && rightCell.isConnectibleOrContact && bottomCell.isConnectibleOrContact)
                                {
                                    currentDRP[i, j].State = CellState.WireLeftRightBottom;
                                }
                                else if (topCell.isConnectibleOrContact && bottomCell.isConnectibleOrContact)
                                {
                                    currentDRP[i, j].State = CellState.WireVertical;
                                }
                                else if (leftCell.isConnectibleOrContact && rightCell.isConnectibleOrContact)
                                {
                                    currentDRP[i, j].State = CellState.WireHorizontal;
                                }
                                else if (topCell.isConnectibleOrContact && leftCell.isConnectibleOrContact)
                                {
                                    currentDRP[i, j].State = CellState.WireTopLeft;
                                }
                                else if (topCell.isConnectibleOrContact && rightCell.isConnectibleOrContact)
                                {
                                    currentDRP[i, j].State = CellState.WireTopRight;
                                }
                                else if (bottomCell.isConnectibleOrContact && leftCell.isConnectibleOrContact)
                                {
                                    currentDRP[i, j].State = CellState.WireBottomLeft;
                                }
                                else if (bottomCell.isConnectibleOrContact && rightCell.isConnectibleOrContact)
                                {
                                    currentDRP[i, j].State = CellState.WireBottomRight;
                                }
                            }

                        }
                    }

                    // заменяем буквы просто контактами
                    currentDRP[startPos.Row, startPos.Column].State = CellState.Contact;
                    currentDRP[endPos.Row, endPos.Column].State = CellState.Contact;
                    log.Add(new StepTracingLog(boards, $"Построили на базе точек проводник №{wireNum + 1} в {boardNum + 1} узле"));

                }
            }
            return log;
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
        /// Метод для получения списка соседей, которые являются волной для одной точки
        /// </summary>
        public static List<Position> getNeighborsOnlyWave(Matrix<Cell> drp, Position pos)
        {
            var neighbors = new List<Position>();
            var aplicants = new List<Position>();

            for (int i = 0; i < 4; i++)
            {
                aplicants.Add(pos.Clone());
            }

            aplicants[0].Column += 1;
            aplicants[1].Column -= 1;
            aplicants[2].Row -= 1;
            aplicants[3].Row += 1;

            foreach (var aplicant in aplicants)
            {
                if (aplicant.Row >= 0 && aplicant.Row < drp.RowsCount)
                {
                    if (aplicant.Column >= 0 && aplicant.Column < drp.ColsCount)
                    {
                        if (drp[aplicant.Row, aplicant.Column].State == CellState.Wave)
                        {
                            neighbors.Add(aplicant);
                        }
                    }
                }
            }

            return neighbors;

        }
    }
}
