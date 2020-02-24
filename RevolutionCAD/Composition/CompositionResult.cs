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

            BoardsWires = new List<List<List<Contact>>>();

            foreach(var board in BoardsElements)
            {
                BoardsWires.Add(new List<List<Contact>>());
            }

            var wiresContacts = sch.WiresContacts;

            for (int boardNum = 0; boardNum < BoardsElements.Count; boardNum++)
            {
                var boardElements = BoardsElements[boardNum];

                for (int wireNum = 0; wireNum < wiresContacts.Count; wireNum++)
                {
                    var new_wire = new List<Contact>();

                    var wire = wiresContacts[wireNum];

                    for (int contactNum = 0; contactNum < wire.Count; contactNum++)
                    {
                        var contact = wire[contactNum];
                        if (boardElements.Contains(contact.ElementNumber) || contact.ElementNumber == 0)
                        {
                            new_wire.Add(contact);
                            
                        } else
                        {
                            int findNumBoard = -1;
                            for (int f = 0; f < BoardsElements.Count; f++)
                            {
                                if (BoardsElements[f].Contains(contact.ElementNumber))
                                {
                                    findNumBoard = f;
                                    break;
                                }
                            }
                            var t_wire = new List<Contact>();
                            t_wire.Add(new Contact(0,0));
                            t_wire.Add(contact.Clone());
                            BoardsWires[findNumBoard].Add(t_wire);
                        }
                    }

                    BoardsWires[boardNum].Add(new_wire);


                }
            }
        }
    }
}
