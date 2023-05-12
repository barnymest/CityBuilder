﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityBuilder.Persistence.Buildings
{
    public class FireStation : Building
    {
        public FireStation(int id, int row, int col) : base(id, row, col)
        {
        }

        public int buildingCost = 2000;
        public int maintainCost = 50;
    }
}