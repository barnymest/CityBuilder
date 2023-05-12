using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityBuilder.Model.Events
{
    public class GrownForestEventArgs : EventArgs
    {
        private int x;
        private int y;
        private int level;

        public GrownForestEventArgs(int x, int y, int level)
        {
            this.x = x;
            this.y = y;
            this.level = level;
        }

        public int X { get => x; set => x = value; }
        public int Y { get => y; set => y = value; }
        public int Level { get => level; set => level = value; }
    }
}
