using RevolutionCAD.Composition;
using RevolutionCAD.Tracing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RevolutionCAD.Placement
{
    public class PlacementResult
    {
        public List<Matrix<int>> BoardsMatrices { get; set; } // список плат с номерами элементов, которые расположены в определённой позиции на плате
        public List<Matrix<Cell>> BoardsDRPs { get; set; } // список дрп плат, на которых размещены элементы с контактами
        public List<Dictionary<int,List<Position>>> BoardsElementsContactsPos { get; set; } // список словарей плат, в словаре по ключу (номеру элемента) можно получить список координат  определённого контакта
        public List<List<Wire>> BoardsWiresPositions { get; set; } // список плат, в котором хранится список проводов по 2 контакта для каждой платы

        public void CreateBoardsDRPs(CompositionResult cmp, List<int> dips, out string err)
        {
            err = "";
            var elInBoards = cmp.BoardsElements;
            var boardsWires = cmp.BoardsWires;
            if (BoardsMatrices == null)
            {
                err = "Список плат пустой";
                return;
            }
            if (BoardsMatrices.Count == 0)
            {
                err = "Список плат пустой";
                return;
            }
            

            // список в котором будет хранится размер разъёма для каждой платы
            var BoardsCountContactsConnector = new List<int>();
            BoardsElementsContactsPos = new List<Dictionary<int, List<Position>>>();
            BoardsDRPs = new List<Matrix<Cell>>();

            // запускаем цикл для формирования своего дрп для каждого узла
            for (int numBoard = 0; numBoard < BoardsMatrices.Count; numBoard++)
            {
                int countContactsConnector = 0;

                countContactsConnector = boardsWires[numBoard].Count(x => x.Any(y => y.ElementNumber == 0));

                BoardsCountContactsConnector.Add(countContactsConnector);

                
                // расчёт размера ДРП платы
                int drpHeight = 0;
                int drpWidth = 0;
                var brdMatr = BoardsMatrices[numBoard];

                // подсчитываем необходимую высоту платы
                // запускаем цикл по столбцам, и определяем его высоту суммируя размер для каждого элемента
                for (int j = 0; j<brdMatr.ColsCount; j++)
                {
                    int currentColHeight = 0;
                    for (int i = 0; i<brdMatr.RowsCount; i++)
                    {
                        int elementNumber = brdMatr[i, j];
                        if (elementNumber != -1) // пропускаем пустые места
                        {
                            int elementDip = dips[elementNumber];
                            int pinsInColumn = elementDip / 2;
                            currentColHeight += ApplicationData.ElementsDistance + pinsInColumn + (ApplicationData.PinDistance * pinsInColumn) - ApplicationData.PinDistance;
                        }
                        
                    }
                    if (currentColHeight > drpHeight)
                        drpHeight = currentColHeight;
                }

                // подсчитываем необходимую ширину платы
                for (int i = 0; i < brdMatr.RowsCount; i++)
                {
                    int currentRowWidth = 0;
                    for (int j = 0; j < brdMatr.ColsCount; j++)
                    {
                        int elementNumber = brdMatr[i, j];
                        if (elementNumber != -1) // пропускаем пустые места
                        {
                            int elementDip = dips[elementNumber];
                            currentRowWidth += ApplicationData.ElementsDistance + 2 + ApplicationData.RowDistance;
                        }

                    }
                    if (currentRowWidth > drpWidth)
                        drpWidth = currentRowWidth;
                }

                drpHeight += ApplicationData.ElementsDistance;
                drpWidth += ApplicationData.ElementsDistance + 2; // ширина + 2, чтобы учесть разъём

                // создаём итоговое дрп на базе расчётов
                var boardDRP = new Matrix<Cell>(drpHeight, drpWidth);

                // заполняем его пустыми ячейками
                for (int drpRow = 0; drpRow < boardDRP.RowsCount; drpRow++)
                {
                    for (int drpCol = 0; drpCol < boardDRP.ColsCount; drpCol++)
                    {
                        boardDRP[drpRow, drpCol] = new Cell();
                    }
                }

                // создаём словарь координат контактов разъёма и элементов
                var ElementsContactsPos = new Dictionary<int, List<Position>>();

                // переходим к размещению разъёма
                var heightConnector = BoardsCountContactsConnector.Last();

                int startRowConnector = drpHeight / 2 - heightConnector / 2;

                var ConnectorContactsPos = new List<Position>();
                for (int r = 0; r<heightConnector; r++)
                {
                    boardDRP[startRowConnector + r, r % 2].State = CellState.Contact;
                    ConnectorContactsPos.Add(new Position(startRowConnector + r, r % 2));
                }

                ElementsContactsPos.Add(0, ConnectorContactsPos);
                

                Position startPos = new Position(ApplicationData.ElementsDistance, ApplicationData.ElementsDistance + 2);
                Position currentPos = new Position(ApplicationData.ElementsDistance, ApplicationData.ElementsDistance + 2);

                
                // запускаем цикл по столбцам матрицы, в которой хранятся номера элементов
                for (int j = 0; j < brdMatr.ColsCount; j++)
                {
                    // запускаем цикл по строкам матрицы, в которой хранятся номера элементов
                    for (int i = 0; i < brdMatr.RowsCount; i++)
                    {
                        // узнаём номер элемента в позиции
                        int elementNumber = brdMatr[i, j];
                        
                        // если -1 - значит место не занято
                        if (elementNumber != -1) // пропускаем пустые места
                        {
                            // список координат каждого контакта
                            var ElementContactsPos = new List<Position>();

                            // добавляем заглушку, т.к. нумерация ножек начинается с 1, а у нас список и 0 элемент должен существовать
                            ElementContactsPos.Add(new Position(-1, -1));

                            int elementNumberLabelRow = currentPos.Row;
                            int elementNumberLabelColumn = currentPos.Column + (int)(ApplicationData.RowDistance / 2);

                            boardDRP[elementNumberLabelRow, elementNumberLabelColumn].Description = $"D{elementNumber}";

                            // узнаём номер дипа
                            int elementDip = dips[elementNumber];
                            // количество контактов в столбце = номер дипа / 2
                            int pinsInColumn = elementDip / 2;
                            int offsetRow = 0;
                            // сначала формируем первый ряд контактов элемента сверху вниз
                            while( offsetRow < elementDip - ApplicationData.PinDistance)
                            {
                                boardDRP[currentPos.Row + offsetRow, currentPos.Column].State = CellState.Contact;
                                // записываем текущую координату в список координат контактов
                                ElementContactsPos.Add(new Position(currentPos.Row + offsetRow, currentPos.Column));
                                offsetRow += 1 + ApplicationData.PinDistance;
                            }
                            offsetRow -= 1 + ApplicationData.PinDistance;
                            // сдвигаемся вправо на расстояние в клетках от первого ряда контактов
                            currentPos.Column += ApplicationData.RowDistance;
                            // теперь идём обратно вверх
                            while (offsetRow >= 0)
                            {
                                boardDRP[currentPos.Row + offsetRow, currentPos.Column].State = CellState.Contact;
                                // записываем текущую координату в список координат контактов
                                ElementContactsPos.Add(new Position(currentPos.Row + offsetRow, currentPos.Column));
                                offsetRow -= 1 + ApplicationData.PinDistance;
                            }
                            // добавляем сформированный список координат каждого контакта
                            ElementsContactsPos.Add(elementNumber, ElementContactsPos);

                            // возвращаемся опять в позицию для печати первого ряда контактов, но уже следующего элемента
                            currentPos.Column -= ApplicationData.RowDistance;
                            // пропускаем ячейки с уже размещённым элементом
                            currentPos.Row += elementDip - ApplicationData.PinDistance;

                            currentPos.Row += ApplicationData.ElementsDistance;
                        } else
                        { 
                            // если позиция пустая, то нужно пропустить определённое количество клеточек
                            // формула приблизительная
                            currentPos.Row += drpHeight / (brdMatr.RowsCount + 1);
                        }

                    }
                    // возвращаемся к начальной точке размещения элементов
                    currentPos.Row = startPos.Row;
                    currentPos.Column = startPos.Column;
                    // пропускаем определённое количество столбцов, которое определяется по количеству уже размещённых умноженное на отступы
                    currentPos.Column += ((j + 1) * ApplicationData.RowDistance) + ((j + 1) * ApplicationData.ElementsDistance);

                }
                
                BoardsDRPs.Add(boardDRP);
                BoardsElementsContactsPos.Add(ElementsContactsPos);

            }

            return;
            
        }

        public void CreateWires(List<List<List<Contact>>> BoardsWires)
        {
            BoardsWiresPositions = new List<List<Wire>>();
            
            for(int boardNum = 0; boardNum < BoardsWires.Count; boardNum++)
            {
                int countConnectorPins = 0;
                var boardWires = new List<Wire>();

                foreach(var contactsPair in BoardsWires[boardNum])
                {
                    var wire = new Wire();
                    var contactA = contactsPair[0];
                    if (contactA.ElementNumber == 0)
                    {
                        contactA.ElementContact = countConnectorPins++;
                    }
                    var contactB = contactsPair[1];
                    if (contactB.ElementNumber == 0)
                    {
                        contactB.ElementContact = countConnectorPins++;
                    }
                    int drpRowContact = BoardsElementsContactsPos[boardNum][contactA.ElementNumber][contactA.ElementContact].Row;
                    int drpColumnContact = BoardsElementsContactsPos[boardNum][contactA.ElementNumber][contactA.ElementContact].Column;
                    wire.A.PositionContact = new Position(drpRowContact, drpColumnContact);

                    drpRowContact = BoardsElementsContactsPos[boardNum][contactB.ElementNumber][contactB.ElementContact].Row;
                    drpColumnContact = BoardsElementsContactsPos[boardNum][contactB.ElementNumber][contactB.ElementContact].Column;
                    wire.B.PositionContact = new Position(drpRowContact, drpColumnContact);

                    boardWires.Add(wire);
                }

                BoardsWiresPositions.Add(boardWires);
            }
        }
    }
}
