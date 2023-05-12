using CityBuilder.Persistence;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityBuilder.Model.Events
{
    public class FieldChangedEvent : EventArgs
    {
        int x, y;
        FieldTypes ftype;
        ZoneTypes ztype;

        public ZoneTypes Ztype { get => ztype; set => ztype = value; }
        public FieldTypes Ftype { get => ftype; set => ftype = value; }
        public int Y { get => y; set => y = value; }
        public int X { get => x; set => x = value; }


        public FieldChangedEvent(int x, int y, FieldTypes ftype, ZoneTypes ztype)
        {
            X = x;
            Y = y;
            Ztype = ztype;
            Ftype = ftype;
        }


    }
}
