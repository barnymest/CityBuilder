using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityBuilder.Persistence.Buildings
{
    public class Forest : Building
    {
        public Forest(int id, int row, int col) : base(id, row, col)
        {
        }

        public int Level { get; set; }
        public int buildingCost = 500;
        public int maintainUnitCost = 10;
        public int forestBuilt;
    }
}
