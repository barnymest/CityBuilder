using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityBuilder.Model.Events
{
    public class ZoneToBuildEventArgs : EventArgs
    {
        private int x, y;

        public ZoneToBuildEventArgs(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public int X { get; set; }
        public int Y { get; set; }
    }
}
