using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityBuilder.Persistence.Buildings
{
    public class Commercials : Building
    {
        public Commercials(int id, int row, int col) : base(id, row, col)
        {
            Capacity = 6;
            Occupants = 0;
            Level = 1;
        }

        public int taxCount(double taxPercent)
        {
            return (int)(Occupants * taxPercent);
        }
    }
}
