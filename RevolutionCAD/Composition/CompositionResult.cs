using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionCAD.Composition
{
    public class CompositionResult
    {
        public List<List<int>> BoardsElements { get; set; } // список узлов с элементами входящими в узлы
        public List<List<List<Contact>>> BoardsWires { get; set; } // список контактов, соединяющих провода в узлах

        public void CreateBoardsWires(Scheme sch, out string err_msg)
        {
            err_msg = "";

            var wiresContacts = sch.WiresContacts;

            BoardsWires = new List<List<List<Contact>>>();

            foreach(var board in BoardsElements)
            {
                BoardsWires.Add(new List<List<Contact>>());

                foreach (var wire in wiresContacts)
                {
                    // текущий провод
                    var new_wire = new List<Contact>();

                    bool isConnector = false; // флаг наличия коннектора для провода
                    bool canWired = false; // флаг существования провода для текущей платы

                    // проверка, есть ли провод для этой платы
                    foreach (Contact c in wire)
                    {
                        if (board.Contains(c.ElementNumber)) canWired = true;
                    }

                    if (canWired) {
                        foreach (Contact c in wire)
                        {
                            if (board.Contains(c.ElementNumber))
                            {
                                new_wire.Add(c.Clone());
                            }
                            else if (c.ElementNumber == 0 && !isConnector)
                            {
                                new_wire.Add(c.Clone());
                                isConnector = true;
                            }
                            else if (!isConnector)
                            {
                                new_wire.Add(new Contact(0, 0));
                                isConnector = true;
                            }
                        }

                        if (new_wire.Count > 0) BoardsWires.Last().Add(new_wire);
                    }
                }
            }
        }
    }
}
