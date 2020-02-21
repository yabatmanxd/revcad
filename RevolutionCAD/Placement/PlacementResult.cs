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
        public List<Matrix<int>> BoardsMatrices { get; set; }
        public List<Matrix<Cell>> BoardsDRPs { get; set; }
        public List<Dictionary<int,List<Point>>> BoardsElementsContactsPos { get; set; }

        public List<Matrix<Cell>> getBoardsDRPs(List<Matrix<int>> brdMatrs, Matrix<int> matrQ, List<List<int>> elInBoards, List<int> dips)
        {
            if (brdMatrs == null)
                return null;
            if (brdMatrs.Count == 0)
                return null;

            // список в котором будет хранится размер разъёма для каждой платы
            var BoardsCountContactsConnector = new List<int>();
            var boardsDRPs = new List<Matrix<Cell>>();

            // запускаем цикл для формирования своего дрп для каждого узла
            for (int boardNumber = 0; boardNumber < brdMatrs.Count; boardNumber++)
            {
                // запускаем цикл по матрице Q чтобы подсчитать количество связей элементов узла с разъёмом (чтобы потом сформировать определённого размера разъём)
                int countContactsConnector = 0;
                for (int j = 0; j < matrQ.ColsCount; j++)
                {
                    if (matrQ[0, j] == 1)
                    {
                        bool isConnectedToElementOnBoard = false;
                        foreach (var num_element in elInBoards[boardNumber])
                        {
                            if (matrQ[num_element, j] == 1)
                            {
                                isConnectedToElementOnBoard = true;
                            }
                        }
                        if (isConnectedToElementOnBoard)
                            countContactsConnector++;
                    }
                }
                BoardsCountContactsConnector.Add(countContactsConnector);

                
                // расчёт размера ДРП платы
                int drpHeight = 0;
                int drpWidth = 0;
                var brdMatr = brdMatrs[boardNumber];


                // подсчитываем необходимую высоту платы
                // запускаем цикл по столбцам, и определяем его высоту суммируя размер для каждого элемента
                for(int j = 0; j<brdMatr.ColsCount; j++)
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
                for (int j = 0; j < brdMatr.ColsCount; j++)
                {
                    int currentRowWidth = 0;
                    for (int i = 0; i < brdMatr.RowsCount; i++)
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
                drpWidth += ApplicationData.ElementsDistance;

                var boardDRP = new Matrix<Cell>(drpHeight,drpWidth);

                for (int i = 0; i<drpHeight; i++)
                {
                    for (int j = 0; j<drpWidth; j++)
                    {
                        boardDRP[i, j] = new Cell()
                        {
                            State = CellState.ArrowUp
                        };
                    }
                }

                boardDRP[0, 0].State = CellState.Contact;
                boardDRP[0, 1].State = CellState.Contact;
                boardDRP[1, 0].State = CellState.Contact;
                boardDRP[drpHeight-1, drpWidth-1].State = CellState.Contact;

                boardsDRPs.Add(boardDRP);

            }

            return boardsDRPs;
            
        }
    }
}
