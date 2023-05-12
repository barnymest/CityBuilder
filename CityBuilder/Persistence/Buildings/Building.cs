using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityBuilder.Persistence
{
    public class Building
    {
        private int _id;
        private int _row;
        private int _col;

        public Building(int id, int row, int col)
        {
            _id = id;
            _row = row;
            _col = col;
        }

        public int Id { get { return _id; } }
        public int Row { get { return _row; } set { _row = value; } }
        public int Col { get { return _col; } set { _col = value; } }
        public int Quality { get; set; }
        public int FireRisk { get; set; }
        public int Level { get; set; }
        public int Capacity { get; set; }
        public int Occupants { get; set; }
    }
}
