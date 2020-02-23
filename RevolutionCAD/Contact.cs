using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevolutionCAD
{
    public class Contact
    {
        public int ElementNumber { get; set; } // номер элемента (для D1 - 1, D2 - 2)
        public int ElementContact { get; set; } // номер контакта элемента (для D1.8 - 8, D2.3 - 3)
        public Position PositionContact { get; set; } // координаты контакта на ДРП

        public Contact()
        {
            PositionContact = new Position(-1,-1);
        }

        public Contact(bool IsCreateConnectorContact)
        {
            ElementContact = 0;
            ElementNumber = 0;
        }

        public Contact Clone()
        {
            var contact = new Contact();
            contact.ElementNumber = ElementNumber;
            contact.ElementContact = ElementContact;
            contact.PositionContact = PositionContact.Clone();
        }
    }
}
