using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityBuilder.Persistence.Buildings
{
    public class Stadium : Building
    {
        public Stadium(int id, int row, int col) : base(id, row, col)
        {
        }

        public int buildingCost = 5000;
        public int maintainCost = 100;

    }
}
