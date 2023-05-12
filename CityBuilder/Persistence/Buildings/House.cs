﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityBuilder.Persistence.Buildings
{
    public class House : Building
    {
        public House(int id, int row, int col) : base(id, row, col)
        {
            Capacity = 4;
            Occupants = 0;
            Level = 1;
        }

        public int taxCount(double taxPercent)
        {
            return (int)(Occupants * taxPercent);
        }
    }
}
