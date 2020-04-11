using RevolutionCAD.Placement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionCAD.Tracing
{
    /// <summary>
    /// Трассировка по четырехлучевому методу
    /// </summary>
    public class TracingTwoBeam
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

                    List<Beam> prioritetsPosBeamsA = new List<Beam>();
                    List<Beam> prioritetsPosBeamsB = new List<Beam>();

                    Position top = new Position(-1, 0);
                    Position bottom = new Position(1, 0);
                    Position left = new Position(0, -1);
                    Position right = new Position(0, 1);

                    if (startPos.Row < endPos.Row)
                    {
                        prioritetsPosBeamsA.Add(new Beam(left, top));
                        prioritetsPosBeamsA.Add(new Beam(right, top));
                        prioritetsPosBeamsB.Add(new Beam(right, bottom));
                        prioritetsPosBeamsB.Add(new Beam(left, bottom));
                    }
                    else
                    {
                        prioritetsPosBeamsA.Add(new Beam(left, bottom));
                        prioritetsPosBeamsA.Add(new Beam(right, bottom));
                        prioritetsPosBeamsB.Add(new Beam(right, top));
                        prioritetsPosBeamsB.Add(new Beam(left, top));
                    }

                    if (startPos.Column < endPos.Column)
                    {
                        prioritetsPosBeamsA.Add(new Beam(top, left));
                        prioritetsPosBeamsA.Add(new Beam(bottom, left));
                        prioritetsPosBeamsB.Add(new Beam(top, right));
                        prioritetsPosBeamsB.Add(new Beam(bottom, right));
                    }
                    else
                    {
                        prioritetsPosBeamsA.Add(new Beam(top, right));
                        prioritetsPosBeamsA.Add(new Beam(bottom, right));
                        prioritetsPosBeamsB.Add(new Beam(top, left));
                        prioritetsPosBeamsB.Add(new Beam(bottom, left));
                    }



                    // помечаем буквами стартовую и конечную позицию на текущем слое провода
                    currentDRP[startPos.Row, startPos.Column].State = CellState.PointA;
                    currentDRP[endPos.Row, endPos.Column].State = CellState.PointB;

                    // сообщаем о начале трассировки нового провода и печатаем сформированные приоритеты
                    string stepMsg = $"Начинаем трассировку {boardDRPs.Count - 1}-го проводника в {boardNum + 1} узле\n";
                    stepMsg += "Сформированы следующие приоритеты для лучей:\n";
                    int iterator = 1;
                    foreach (var prioritet in prioritetsPosBeamsA)
                    {
                        stepMsg += $"A{iterator}({getUnicodeArrowByPrioritetPos(prioritet.FirstPrioritetPos)},{getUnicodeArrowByPrioritetPos(prioritet.SecondPrioritetPos)}) ";
                        iterator++;
                    }
                    stepMsg += "\n";
                    iterator = 1;
                    foreach (var prioritet in prioritetsPosBeamsB)
                    {
                        stepMsg += $"B{iterator}({getUnicodeArrowByPrioritetPos(prioritet.FirstPrioritetPos)},{getUnicodeArrowByPrioritetPos(prioritet.SecondPrioritetPos)}) ";
                        iterator++;
                    }
                    log.Add(new StepTracingLog(boards, stepMsg));

                    // объединяем все слои с проводами платы, чтобы на основе этого ДРП получить список именно незанятых соседей
                    fullDrp = ApplicationData.MergeLayersDRPs(boardDRPs);

                    var beamsACurrentPos = new List<Position>();
                    var beamsBCurrentPos = new List<Position>();

                    for (int beamNum = 0; beamNum < prioritetsPosBeamsA.Count; beamNum++)
                    {
                        var posPrioritet = prioritetsPosBeamsA[beamNum].FirstPrioritetPos;
                        var pos = new Position(startPos.Row - posPrioritet.Row, startPos.Column - posPrioritet.Column);
                        if (pos.isInDRP(fullDrp))
                        {
                            if (fullDrp[pos.Row, pos.Column].isBusyForBeam == false)
                            {
                                beamsACurrentPos.Add(pos);
                                currentDRP[pos.Row, pos.Column].State = getArrowByPrioritet(posPrioritet.Row, posPrioritet.Column);
                                currentDRP[pos.Row, pos.Column].Weight = 1;
                                fullDrp[pos.Row, pos.Column].State = getArrowByPrioritet(posPrioritet.Row, posPrioritet.Column);
                                fullDrp[pos.Row, pos.Column].Weight = 1;
                                continue;
                            }
                        }
                        prioritetsPosBeamsA.RemoveAt(beamNum);
                        beamNum--;
                    }

                    for (int beamNum = 0; beamNum < prioritetsPosBeamsB.Count; beamNum++)
                    {
                        var posPrioritet = prioritetsPosBeamsB[beamNum].FirstPrioritetPos;
                        var pos = new Position(endPos.Row - posPrioritet.Row, endPos.Column - posPrioritet.Column);
                        if (pos.isInDRP(fullDrp))
                        {
                            if (fullDrp[pos.Row, pos.Column].isBusyForBeam == false)
                            {
                                beamsBCurrentPos.Add(pos);
                                currentDRP[pos.Row, pos.Column].State = getArrowByPrioritet(posPrioritet.Row, posPrioritet.Column);
                                currentDRP[pos.Row, pos.Column].Weight = 2;
                                fullDrp[pos.Row, pos.Column].State = getArrowByPrioritet(posPrioritet.Row, posPrioritet.Column);
                                fullDrp[pos.Row, pos.Column].Weight = 2;
                                continue;
                            }

                        }
                        prioritetsPosBeamsB.RemoveAt(beamNum);
                        beamNum--;

                    }
                    if (!isOptimized)
                        log.Add(new StepTracingLog(boards, $"Проведены лучи на 1 шаг {boardDRPs.Count - 1}-го проводника в {boardNum + 1} узле. Точек пересечения не найдено. Продолжаем."));

                    Position collisionPosBeamsA = null;
                    Position collisionPosBeamsB = null;
                    bool isChanged = false;

                    do
                    {
                        isChanged = false;

                        for (int beamNum = 0; beamNum < beamsACurrentPos.Count; beamNum++)
                        {
                            var currPos = beamsACurrentPos[beamNum];
                            var prioritets = new List<Position>();
                            prioritets.Add(prioritetsPosBeamsA[beamNum].FirstPrioritetPos);
                            prioritets.Add(prioritetsPosBeamsA[beamNum].SecondPrioritetPos);
                            foreach (var prioritet in prioritets)
                            {
                                var checkingPos = new Position(currPos.Row - prioritet.Row, currPos.Column - prioritet.Column);
                                if (checkingPos.isInDRP(fullDrp))
                                {
                                    if (fullDrp[checkingPos.Row, checkingPos.Column].isBusyForBeam == false)
                                    {
                                        currentDRP[checkingPos.Row, checkingPos.Column].State = getArrowByPrioritet(prioritet.Row, prioritet.Column);
                                        currentDRP[checkingPos.Row, checkingPos.Column].Weight = 1;
                                        fullDrp[checkingPos.Row, checkingPos.Column].State = getArrowByPrioritet(prioritet.Row, prioritet.Column);
                                        fullDrp[checkingPos.Row, checkingPos.Column].Weight = 1;
                                        isChanged = true;
                                        beamsACurrentPos[beamNum] = checkingPos;
                                        break;
                                    }
                                    else if (fullDrp[checkingPos.Row, checkingPos.Column].isArrow && fullDrp[checkingPos.Row, checkingPos.Column].Weight == 2)
                                    {
                                        isChanged = true;
                                        collisionPosBeamsA = currPos;
                                        collisionPosBeamsB = checkingPos;
                                        beamNum = 228;
                                        break;
                                    }

                                }
                            }

                        }

                        if (collisionPosBeamsA != null)
                            break;


                        for (int beamNum = 0; beamNum < beamsBCurrentPos.Count; beamNum++)
                        {
                            var currPos = beamsBCurrentPos[beamNum];
                            var prioritets = new List<Position>();
                            prioritets.Add(prioritetsPosBeamsB[beamNum].FirstPrioritetPos);
                            prioritets.Add(prioritetsPosBeamsB[beamNum].SecondPrioritetPos);
                            foreach (var prioritet in prioritets)
                            {
                                var checkingPos = new Position(currPos.Row - prioritet.Row, currPos.Column - prioritet.Column);
                                if (checkingPos.isInDRP(fullDrp))
                                {
                                    if (fullDrp[checkingPos.Row, checkingPos.Column].isBusyForBeam == false)
                                    {
                                        currentDRP[checkingPos.Row, checkingPos.Column].State = getArrowByPrioritet(prioritet.Row, prioritet.Column);
                                        currentDRP[checkingPos.Row, checkingPos.Column].Weight = 2;
                                        fullDrp[checkingPos.Row, checkingPos.Column].State = getArrowByPrioritet(prioritet.Row, prioritet.Column);
                                        fullDrp[checkingPos.Row, checkingPos.Column].Weight = 2;
                                        isChanged = true;
                                        beamsBCurrentPos[beamNum] = checkingPos;
                                        break;
                                    }
                                    else if (fullDrp[checkingPos.Row, checkingPos.Column].isArrow && fullDrp[checkingPos.Row, checkingPos.Column].Weight == 1)
                                    {
                                        isChanged = true;
                                        collisionPosBeamsB = currPos;
                                        collisionPosBeamsA = checkingPos;
                                        beamNum = 228;
                                        break;
                                    }

                                }
                            }

                        }
                        if (!isOptimized)
                            log.Add(new StepTracingLog(boards, $"Проведены лучи на 1 шаг {boardDRPs.Count - 1}-го проводника в {boardNum + 1} узле. Точек пересечения не найдено. Продолжаем."));

                    } while (collisionPosBeamsA == null && isChanged == true);

                    if (isChanged == false)
                    {
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
                        log.Add(new StepTracingLog(boards, $"Невозможно выполнить трассировку {boardDRPs.Count - 1}-го проводника в {boardNum + 1} узле. Все лучи упёрлись в препятствие и не пересеклись."));
                        continue;
                    }
                    else
                    {
                        if (isOptimized)
                            log.Add(new StepTracingLog(boards, $"Лучи пересеклись при трассировке {boardDRPs.Count - 1}-го проводника в {boardNum + 1} узле"));
                    }

                    var currentPos = collisionPosBeamsA;

                    do
                    {
                        var bufCellState = currentDRP[currentPos.Row, currentPos.Column].State;

                        currentDRP[currentPos.Row, currentPos.Column].State = CellState.Wire;
                        currentDRP[currentPos.Row, currentPos.Column].Weight = -1;

                        currentPos = getNextPosByCurrentArrow(currentPos, bufCellState);

                    } while (currentDRP[currentPos.Row, currentPos.Column].State != CellState.PointA);


                    currentPos = collisionPosBeamsB;

                    do
                    {
                        var bufCellState = currentDRP[currentPos.Row, currentPos.Column].State;

                        currentDRP[currentPos.Row, currentPos.Column].State = CellState.Wire;
                        currentDRP[currentPos.Row, currentPos.Column].Weight = -1;

                        currentPos = getNextPosByCurrentArrow(currentPos, bufCellState);

                    } while (currentDRP[currentPos.Row, currentPos.Column].State != CellState.PointB);


                    // очищаем всё дрп от стрелочек
                    for (int i = 0; i < currentDRP.RowsCount; i++)
                    {
                        for (int j = 0; j < currentDRP.ColsCount; j++)
                        {
                            if (currentDRP[i, j].isArrow)
                            {
                                currentDRP[i, j].State = CellState.Empty;
                                currentDRP[i, j].Weight = -1;
                            }
                        }
                    }

                    log.Add(new StepTracingLog(boards, $"Определяем точки, где будет проходить проводник №{boardDRPs.Count - 1} в {boardNum + 1} узле"));

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

        private class Beam
        {
            public Position FirstPrioritetPos { get; set; }
            public Position SecondPrioritetPos { get; set; }

            public Beam()
            {

            }

            public Beam(Position fp, Position sp)
            {
                FirstPrioritetPos = fp;
                SecondPrioritetPos = sp;
            }
        }

        private static string getUnicodeArrowByPrioritetPos(Position prioritet)
        {
            string res = "";
            switch (getArrowByPrioritet(prioritet.Row, prioritet.Column))
            {
                case CellState.ArrowUp:
                    res = "\u2191";
                    break;
                case CellState.ArrowDown:
                    res = "\u2193";
                    break;
                case CellState.ArrowLeft:
                    res = "\u2190";
                    break;
                case CellState.ArrowRight:
                    res = "\u2192";
                    break;
            }
            return res;
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

    }
}
