using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CityBuilder.Persistence
{
    public enum FieldTypes { Land, Water, Road, Bridge, Forest };
    public enum ZoneTypes { None, Residential, Industrial, Commercial };

    public class GameData
    {
        //private data
        private Field[,] gameBoard;
        private int happiness;
        private int funds;
        private int population;
        private int timerCounter;
        private int taxRate;
        private int negBalance;
        private int lowHappiness;

        //constructor
        public GameData()
        {
            gameBoard = new Field[25, 25];
            happiness = 75;
            funds = 10000;
            population = 0;
            timerCounter = 0;
            taxRate = 20;
            negBalance = 0;
            lowHappiness = 0;
            buildings = new List<Building>();
            peoples = new List<People>();

            Field[,] temp = new Field[25, 25];

            if (peoples != null)
            {
                foreach (People people in peoples)
                {
                    happiness += people.Happiness;
                }
            }

            for (int i = 0; i < 25; i++)
            {
                for(int j = 0; j < 25; j++)
                {
                    temp[i, j] = new Field(i ,j, FieldTypes.Land, ZoneTypes.None) ;
                }
            }

            GameBoard = temp;
        }

        //properties
        public int Happiness { get { return happiness; } set { happiness = value; } }
        public int Funds { get { return funds; } set { funds = value; } }
        public int Population { get { return population; } set { population = value; } }
        public int TimerCounter { get { return timerCounter; } set { timerCounter = value; } }
        public int NegBalance { get { return negBalance; } set { negBalance = value; } }
        public int LowHappiness { get { return lowHappiness; } set { lowHappiness = value; } }

        public Field[,] GameBoard { get => gameBoard; set => gameBoard = value; }
        public int TaxRate { get => taxRate; set => taxRate = value; }

        public List<People>? peoples;
        public List<Building>? buildings;
    }
}
