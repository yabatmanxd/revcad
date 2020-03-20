using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Newtonsoft.Json;
using System.IO;

namespace RevolutionCAD.Composition
{
    public class IterMultigraphNew
    {
        /// <summary>
        /// Итерационная компоновка по мультиграфу
        /// </summary>
        public static List<StepCompositionLog> Compose(out string error_msg)
        {
            var log = new List<StepCompositionLog>();

            error_msg = "";

            // считываем схему (потом достанем из неё матрицу R)
            var sch = ApplicationData.ReadScheme(out error_msg);

            if (error_msg != "")
            {
                return log;
            }

            // считываем результаты компоновки
            var cmp = ApplicationData.ReadComposition(out error_msg);

            if (error_msg != "")
            {
                return log;
            }

            // считываем матрицу R
            var matrR = sch.MatrixR;

            // считываем элементы в уже скомпонованных узлах
            var boardsElements = cmp.BoardsElements;

            if (boardsElements.Count < 2)
            {
                log.Add(new StepCompositionLog(boardsElements, "Для выполнения итерационной компоновки необходимо от 2 узлов"));
                return log;
            }

            // список в котором будут хранится пары узлов (1-2, 1-3, 1-4, 2-3, 2-4, 3-4)
            var boardsPairs = new List<PairBoards>();
            
            // формируем пары узлов
            for (int i = 0; i< boardsElements.Count - 1; i++)
            {
                for (int j = i+1; j < boardsElements.Count; j++)
                {
                    var pair = new PairBoards(i, j);
                    boardsPairs.Add(pair);
                }
            }
            
            // максимальное значение дельта r у пары элементов
            int maxDeltaR;
            // пара элементов у которой максимальное дельта r 
            PairElements maxDeltaRpair;

            // запускаем цикл до тех пор, пока не найдём ни одного положительного приращения дельта r
            do
            {
                maxDeltaR = Int32.MinValue;
                maxDeltaRpair = null;
                string logMessage = "";

                // список пар элементов в разных узлах
                var elementsPairs = new List<PairElements>();
                
                // проходим по всем парам узлов
                foreach (var boardPair in boardsPairs)
                {
                    // формируем пары элементов в разных платах
                    for (int i = 0; i < boardsElements[boardPair.FirstBoard].Count; i++)
                    {
                        for (int j = 0; j < boardsElements[boardPair.SecondBoard].Count; j++)
                        {
                            var pair = new PairElements();
                            pair.FirstElement.ElementNumber = boardsElements[boardPair.FirstBoard][i];
                            pair.FirstElement.BoardNumber = boardPair.FirstBoard;
                            pair.SecondElement.ElementNumber = boardsElements[boardPair.SecondBoard][j];
                            pair.SecondElement.BoardNumber = boardPair.SecondBoard;
                            elementsPairs.Add(pair);
                        }
                    }
                }

                // на выходе получаем список elementsPairs всех возможных пар элементов в разных узлах для перестановки

                // теперь запускаем цикл по всем этим парам
                foreach (var elementPair in elementsPairs)
                {
                    

                    int delta_r = 0;

                    // получаем номер первого элемента из пары
                    int firstElement = elementPair.FirstElement.ElementNumber;
                    // и номер узла в котором этот элемент находится
                    int firstElementBoard = elementPair.FirstElement.BoardNumber;
                    // получаем список элементов, которые находятся в узле первого элемента из пары
                    List<int> elementOfFirstBoard = boardsElements[firstElementBoard];

                    // получаем номер второго элемента из пары
                    int secondElement = elementPair.SecondElement.ElementNumber;
                    // номер узла второго элемента из пары
                    int secondElementBoard = elementPair.SecondElement.BoardNumber;
                    // список элементов входящих в узел элемента из второй пары
                    List<int> elementOfSecondBoard = boardsElements[secondElementBoard];
                    
                    // считаем количество связей элемента из первого узла с элементами второго узла (внешние связи)
                    int term1 = getCountRelationElementWithElements(firstElement, elementOfSecondBoard, matrR) -
                        getCountRelationElementWithElements(firstElement, elementOfFirstBoard, matrR); // вычитаем количество связей первого элемента с элементами собственного узла (внутренние связи)

                    // теперь тоже самое для второго элемента

                    // считаем количество связей элемента из второго узла с элементами первого узла (внешние связи)
                    int term2 = getCountRelationElementWithElements(secondElement, elementOfFirstBoard, matrR) - 
                        getCountRelationElementWithElements(secondElement, elementOfSecondBoard, matrR); // и вычитаем количество связей второго элемента с элементами собственного узла (внутренние связи)

                    // третье составляющее формулы дельта r это количество связей первого элемента и второго
                    int term3 = matrR[firstElement, secondElement];
                    
                    // теперь подставляем посчитанные значения в формулу
                    delta_r = term1 + term2 - (2 * term3);

                    // формируем сообщение о результатах просчётов
                    logMessage += $"\u0394r({firstElement}-{secondElement}) = {term1} + {term2} - 2*{term3} = {delta_r}\n";

                    // если посчитанное дельта r пары элементов больше уже найденного, запоминаем эту пару
                    if (delta_r > maxDeltaR)
                    {
                        maxDeltaR = delta_r;
                        maxDeltaRpair = elementPair.Clone();
                    }
                }

                // на выходе получили пару элементов maxDeltaRpair с максимальным приращением

                // если приращение положительное - значит нужно поменять элементы в узлах местами
                if (maxDeltaR > 0)
                {
                    // удаляем первый элемент пары из его собственного узла
                    boardsElements[maxDeltaRpair.FirstElement.BoardNumber].Remove(maxDeltaRpair.FirstElement.ElementNumber);
                    // и добавляем этот элемент во второй узел пары
                    boardsElements[maxDeltaRpair.FirstElement.BoardNumber].Add(maxDeltaRpair.SecondElement.ElementNumber);
                    // а второй элемент пары удаляем из его собственного второго узла пары
                    boardsElements[maxDeltaRpair.SecondElement.BoardNumber].Remove(maxDeltaRpair.SecondElement.ElementNumber);
                    // и добавляем в первый узел пары
                    boardsElements[maxDeltaRpair.SecondElement.BoardNumber].Add(maxDeltaRpair.FirstElement.ElementNumber);

                    // формируем сообщение
                    logMessage += $"Максимальное положительное \u0394r у элементов {maxDeltaRpair.FirstElement.ElementNumber} и {maxDeltaRpair.SecondElement.ElementNumber}. Меняем их местами.";
                    
                } else
                {
                    logMessage += $"Положительного \u0394r не найдено.";
                }
                // фиксируем изменение
                log.Add(new StepCompositionLog(boardsElements, logMessage));


                // и так продолжается пока среди всех пар перестановок в узлах не найдётся ни одного положительного приращения
                // каждый цикл будут формироваться новые пары перестановок элементов в узлах, будет находится максимальное приращение при перестановке
                // элементов в узлах и осуществлятся перестановка этих элементов в узлах
            } while (maxDeltaR > 0);
            
            return log;
        }

        /// <summary>
        /// Метод, который по матрице R считает количество связей указанного в первом параметре элемена и списка элементов из второго параметра
        /// </summary>
        private static int getCountRelationElementWithElements(int element, List<int> elements, Matrix<int> matr)
        {
            int count = 0;
            foreach(int otherElement in elements)
            {
                count += matr[element, otherElement];
            }
            return count;
        }

        private class Element
        {
            public int ElementNumber { get; set; }
            public int BoardNumber { get; set; }

            public Element()
            {

            }

            public Element(int elNum, int brdNum)
            {
                ElementNumber = elNum;
                BoardNumber = brdNum;
            }

        }

        private class PairElements
        {
            public Element FirstElement { get; set; }
            public Element SecondElement { get; set; }

            public PairElements()
            {
                FirstElement = new Element();
                SecondElement = new Element();
            }

            public PairElements Clone()
            {
                var p = new PairElements();
                p.FirstElement.BoardNumber = FirstElement.BoardNumber;
                p.FirstElement.ElementNumber = FirstElement.ElementNumber;
                p.SecondElement.BoardNumber = SecondElement.BoardNumber;
                p.SecondElement.ElementNumber = SecondElement.ElementNumber;
                return p;
            }
        }

        private class PairBoards
        {
            public int FirstBoard { get; set; }
            public int SecondBoard { get; set; }

            public PairBoards()
            {

            }

            public PairBoards(int fBrd, int sBrd)
            {
                FirstBoard = fBrd;
                SecondBoard = sBrd;
            }
        }


    }
}
