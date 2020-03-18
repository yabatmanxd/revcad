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

            // список в котором будут хранится пары плат (1-2, 1-3, 1-4, 2-3, 2-4, 3-4)
            var boardsPairs = new List<PairBoards>();
            
            // формируем пары плат
            for (int i = 0; i< boardsElements.Count - 1; i++)
            {
                for (int j = i+1; j < boardsElements.Count; j++)
                {
                    var pair = new PairBoards(i, j);
                    boardsPairs.Add(pair);
                }
            }

            
            
            int maxDeltaR;
            PairElements maxDeltaRpair;

            do
            {
                maxDeltaR = Int32.MinValue;
                maxDeltaRpair = null;
                string logMessage = "";

                var elementsPairs = new List<PairElements>();
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

                foreach (var elementPair in elementsPairs)
                {
                    int delta_r = 0;

                    int firstElement = elementPair.FirstElement.ElementNumber;
                    int firstElementBoard = elementPair.FirstElement.BoardNumber;
                    List<int> elementOfFirstBoard = boardsElements[firstElementBoard];

                    int secondElement = elementPair.SecondElement.ElementNumber;
                    int secondElementBoard = elementPair.SecondElement.BoardNumber;
                    List<int> elementOfSecondBoard = boardsElements[secondElementBoard];
                    
                    int term1 = getCountRelationElementWithElements(firstElement, elementOfSecondBoard, matrR) -
                        getCountRelationElementWithElements(firstElement, elementOfFirstBoard, matrR);

                    int term2 = getCountRelationElementWithElements(secondElement, elementOfFirstBoard, matrR) -
                        getCountRelationElementWithElements(secondElement, elementOfSecondBoard, matrR);

                    int term3 = matrR[firstElement, secondElement];
                    
                    delta_r = term1 + term2 - (2 * term3);

                    logMessage += $"\u0394r({firstElement}-{secondElement}) = {term1} + {term2} - 2*{term3} = {delta_r}\n";

                    if (delta_r > maxDeltaR)
                    {
                        maxDeltaR = delta_r;
                        maxDeltaRpair = elementPair.Clone();
                    }
                }

                if (maxDeltaR > 0)
                {
                    boardsElements[maxDeltaRpair.FirstElement.BoardNumber].Remove(maxDeltaRpair.FirstElement.ElementNumber);
                    boardsElements[maxDeltaRpair.FirstElement.BoardNumber].Add(maxDeltaRpair.SecondElement.ElementNumber);
                    boardsElements[maxDeltaRpair.SecondElement.BoardNumber].Remove(maxDeltaRpair.SecondElement.ElementNumber);
                    boardsElements[maxDeltaRpair.SecondElement.BoardNumber].Add(maxDeltaRpair.FirstElement.ElementNumber);

                    logMessage += $"Максимальное положительное \u0394r у элементов {maxDeltaRpair.FirstElement.ElementNumber} и {maxDeltaRpair.SecondElement.ElementNumber}. Меняем их местами.";
                    
                } else
                {
                    logMessage += $"Положительного \u0394r не найдено.";
                }
                log.Add(new StepCompositionLog(boardsElements, logMessage));

            } while (maxDeltaR > 0);

            
            
            

            return log;
        }

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
