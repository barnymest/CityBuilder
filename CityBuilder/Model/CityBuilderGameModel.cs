using CityBuilder.Model.Events;
using CityBuilder.Persistence;
using CityBuilder.Persistence.Buildings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CityBuilder.Model
{
    public class CityBuilderGameModel
    {
        //variables
        private GameData modelGameData;
        public int costPerPeriod;
        public int incomePerPeriod;
        private ICityBuilderDataAccess _dataAccess;
        private int gameSpeed;
        private int ids;

        //properties
        public GameData modelgd { get => modelGameData; }
        public int GameSpeed { get => gameSpeed; set => gameSpeed = value; }

        //model events
        public event EventHandler<int>? GameAdvanced;
        public event EventHandler<ZoneToBuildEventArgs>? BuildingBuilt;
        public event EventHandler<GrownForestEventArgs>? ForestGrowth;
        public event EventHandler<PeopleMovedInEventArgs>? PeopleMovedIn;
        public event EventHandler<GameOverEventArgs>? GameOver;

        //constructor
        public CityBuilderGameModel(CityBuilderDataAccess dataAccess)
        {
            modelGameData = new GameData();
            _dataAccess = dataAccess;
            GameSpeed = 0;
            ids = 0;
        }

        //time management
        public void AdvanceTime()
        {
            modelGameData.TimerCounter++;
            CheckForestGrowth();
            BuildAccordingBuildingM();
            CalculateQuality();
            PopulationChange();
            CalculateHappiness();
            MonthlyFinance();
            GameOverCheck();
            OnGameAdvanced();
        }

        private void GameOverCheck()
        {
            if (modelgd.TimerCounter % 30 == 0)
            {
                if (modelgd.Funds < 0)
                    modelgd.NegBalance++;
                else
                    modelgd.NegBalance = 0;
                if (modelgd.Happiness < 25)
                    modelgd.LowHappiness++;
                else
                    modelgd.LowHappiness = 0;
                if (modelgd.NegBalance == 3)
                    OnGameOver(1);
                if (modelgd.LowHappiness == 3)
                    OnGameOver(2);
            }
        }

        private void OnGameOver(int v)
        {
            GameOver?.Invoke(this, new GameOverEventArgs(v));
        }

        private void OnGameAdvanced()
        {
            GameAdvanced?.Invoke(this, modelGameData.TimerCounter);
        }

        //population changing
        private void PopulationChange()
        {
            if (modelgd.buildings is not null && modelgd.peoples is not null)
            {
                Random rand = new();
                List<int> homes = new();
                foreach (Building b in modelgd.buildings)
                {
                    if (b.GetType().Name == "House" && b.Quality > rand.Next(100))
                    {
                        homes.Add(b.Id);
                    }
                }
                foreach (int id in homes)
                {
                    PeopleMovingIn(id);
                }
            }
        }

        private void PeopleMovingIn(int id)
        {
            if (modelgd.buildings is not null && modelgd.peoples is not null)
            {
                int ix = modelgd.buildings.FindIndex(x => x.Id == id);
                House temp = (House)modelgd.buildings[ix];
                if (temp.Occupants < temp.Capacity)
                {
                    int wpid = JobFinder(temp.Row, temp.Col);
                    if (wpid != -1)
                    {
                        modelgd.peoples.Add(new People(temp.Id, wpid));
                        temp.Occupants++;
                        modelgd.buildings[ix] = temp;
                        modelgd.Population++;
                        OnPeopleMovedIn(temp.Row, temp.Col);
                    }
                }
            }
        }

        private void OnPeopleMovedIn(int row, int col)
        {
            PeopleMovedIn?.Invoke(this, new PeopleMovedInEventArgs(row, col));
        }

        private int JobFinder(int row, int col)
        {
            if (modelgd.buildings is not null)
            {
                int id = -1;
                double d = 100.0;
                foreach (Building b in modelgd.buildings)
                {
                    if (b.GetType().Name == "Factory")
                    {
                        Factory temp = (Factory)b;
                        if (temp.Occupants < temp.Capacity && Distance(row, col, temp.Row, temp.Col) < d)
                        {
                            id = temp.Id;
                            d = Distance(row, col, temp.Row, temp.Col);
                        }
                    }
                    if (b.GetType().Name == "Commercials")
                    {
                        Commercials temp = (Commercials)b;
                        if (temp.Occupants < temp.Capacity && Distance(row, col, temp.Row, temp.Col) < d)
                        {
                            id = temp.Id;
                            d = Distance(row, col, temp.Row, temp.Col);
                        }
                    }
                }
                
                if (id == -1)
                    return -1;
                int ix = modelgd.buildings.FindIndex(x => x.Id.Equals(id));
                if (modelgd.buildings[ix].GetType().Name == "Factory")
                {
                    Factory temp = (Factory)modelgd.buildings[ix];
                    temp.Occupants++;
                    modelgd.buildings[ix] = temp;
                }
                else
                {
                    Commercials temp = (Commercials)modelgd.buildings[ix];
                    temp.Occupants++;
                    modelgd.buildings[ix] = temp;
                }
                return id;
            }
            return -1;
        }

        //building after zone placement
        private void BuildAccordingBuildingM()
        {
            if (modelgd.buildings is not null)
            {
                for (int i = 0; i < 25; i++)
                {
                    for (int j = 0; j < 25; j++)
                    {
                        if (modelgd.GameBoard[i, j].Zone == ZoneTypes.Residential &&
                            modelgd.TimerCounter == modelgd.GameBoard[i, j].ZonePlaced + 3)
                        {
                            House newBuilding = new House(ids, i, j);
                            ids++;
                            modelgd.buildings.Add(newBuilding);
                            modelgd.GameBoard[i, j].BuildingID = newBuilding.Id;
                            OnBuildingBuilt(i, j);
                        }
                        else if (modelgd.GameBoard[i, j].Zone == ZoneTypes.Commercial &&
                            modelgd.TimerCounter == modelgd.GameBoard[i, j].ZonePlaced + 3)
                        {
                            Commercials newBuilding = new Commercials(ids, i, j);
                            ids++;
                            modelgd.buildings.Add(newBuilding);
                            modelgd.GameBoard[i, j].BuildingID = newBuilding.Id;
                            OnBuildingBuilt(i, j);
                        }
                        else if (modelgd.GameBoard[i, j].Zone == ZoneTypes.Industrial &&
                            modelgd.TimerCounter == modelgd.GameBoard[i, j].ZonePlaced + 3)
                        {
                            Factory newBuilding = new Factory(ids, i, j);
                            ids++;
                            modelgd.buildings.Add(newBuilding);
                            modelgd.GameBoard[i, j].BuildingID = newBuilding.Id;
                            OnBuildingBuilt(i, j);
                        }
                    }
                }
            }
            
        }

        private void OnBuildingBuilt(int i, int j)
        {
            BuildingBuilt?.Invoke(this, new ZoneToBuildEventArgs(i, j));
        }

        //growth of forest built
        private void CheckForestGrowth()
        {
            if (modelgd.buildings is not null)
            {
                foreach (var b in modelgd.buildings)
                {
                    if (b.GetType().Name == "Forest")
                    {
                        Forest temp = (Forest)b;
                        if ((modelgd.TimerCounter - temp.forestBuilt) % 360 == 0)
                        {
                            temp.Level++;
                            if (temp.Level == 10)
                            {
                                modelgd.buildings.Remove(b);
                                modelgd.GameBoard[temp.Row, temp.Col].BuildingID = -1;
                            }
                            else
                            {
                                int ix = modelgd.buildings.FindIndex(x => x.Id == temp.Id);
                                modelgd.buildings[ix] = temp;
                            }
                            OnForestGrowth(temp.Row, temp.Col, temp.Level);
                        }
                    }
                }
            }
        }

        private void OnForestGrowth(int row, int col, int level)
        {
            ForestGrowth?.Invoke(this, new GrownForestEventArgs(row, col, level));
        }

        //game related methods
        public void NewGame()
        {
            modelGameData = new GameData();
            modelGameData.TimerCounter = 0;
            GenerateFields();
        }
        public void ExitGame()
        {
            modelGameData = new GameData();
            modelGameData.TimerCounter = 0;
        }

        public void SaveGame(string fileName)
        {
            Debug.WriteLine("model --> save game");
            _dataAccess.SaveAsync(fileName, modelgd);
        }

        public async void LoadGame(string fileName)
        {
            Debug.WriteLine("model --> load game");
            modelGameData = await _dataAccess.LoadAsync(fileName);
        }

        private void GenerateFields()
        {

            Random r = new Random();
            SimplexNoise.Noise.Seed = r.Next(100000000, 999999999);


            int length = 25, width = 25;
            float[,] noiseValues = SimplexNoise.Noise.Calc2D(length, width, 0.05f);

            foreach (Field field in modelGameData.GameBoard)
            {
                if(Math.Round(noiseValues[field.Column, field.Row], 0) < 50)
                {
                    field.Type = FieldTypes.Water;
                }
                else
                {
                    field.Type = FieldTypes.Land;
                }

                if (Math.Round(noiseValues[field.Column, field.Row], 0) > 230)
                {
                    field.Type = FieldTypes.Forest;
                }


                if (field.Row > 8 && field.Row < 16 && field.Row + field.Column <= 20)
                {
                    field.Type = FieldTypes.Land;
                }

                if (field.Row == 12 && field.Column == 0)
                {
                    field.Type = FieldTypes.Road;
                }

                
                
                field.Zone = ZoneTypes.None;
            }
        }

        public double Distance(int x1, int y1, int x2, int y2)
        {
            return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        }

        //zone placement
        public void ZoneChanged(int row, int column, ZoneTypes _zone)
        {
            if(_zone != ZoneTypes.None)
            {
                if(CanBePlaced1x1(row, column))
                {
                    modelgd.Funds -= 100;
                    modelgd.GameBoard[row, column].Zone = _zone;
                    modelgd.GameBoard[row, column].ZonePlaced = modelgd.TimerCounter;
                }
            }
        }

        private bool CanBePlaced1x1(int row, int column)
        {
            if (modelgd.GameBoard[row, column].Zone == ZoneTypes.None && 
                modelgd.GameBoard[row, column].Type != FieldTypes.Road &&
                modelgd.GameBoard[row, column].Type != FieldTypes.Bridge)
            {
                if (row == 0)
                {
                    if (column == 0 && (modelgd.GameBoard[row + 1, column].Type == FieldTypes.Road ||
                        modelgd.GameBoard[row, column + 1].Type == FieldTypes.Road))
                    {
                        return true;
                    }
                    else if (column == 24 && (modelgd.GameBoard[row + 1, column].Type == FieldTypes.Road ||
                        modelgd.GameBoard[row, column - 1].Type == FieldTypes.Road))
                    {
                        return true;
                    }
                    else if ((column != 0 && column != 24) && (modelgd.GameBoard[row + 1, column].Type == FieldTypes.Road ||
                        modelgd.GameBoard[row, column - 1].Type == FieldTypes.Road ||
                        modelgd.GameBoard[row, column + 1].Type == FieldTypes.Road))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (row == 24)
                {
                    if (column == 0 && (modelgd.GameBoard[row - 1, column].Type == FieldTypes.Road ||
                        modelgd.GameBoard[row, column + 1].Type == FieldTypes.Road))
                    {
                        return true;
                    }
                    else if (column == 24 && (modelgd.GameBoard[row - 1, column].Type == FieldTypes.Road ||
                        modelgd.GameBoard[row, column - 1].Type == FieldTypes.Road))
                    {
                        return true;
                    }
                    else if ((column != 0 && column != 24) && (modelgd.GameBoard[row - 1, column].Type == FieldTypes.Road ||
                        modelgd.GameBoard[row, column - 1].Type == FieldTypes.Road ||
                        modelgd.GameBoard[row, column + 1].Type == FieldTypes.Road))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (column == 0 && (modelgd.GameBoard[row - 1, column].Type == FieldTypes.Road ||
                        modelgd.GameBoard[row + 1, column].Type == FieldTypes.Road ||
                        modelgd.GameBoard[row, column + 1].Type == FieldTypes.Road))
                    {
                        return true;
                    }
                    else if (column == 24 && (modelgd.GameBoard[row - 1, column].Type == FieldTypes.Road ||
                        modelgd.GameBoard[row + 1, column].Type == FieldTypes.Road ||
                        modelgd.GameBoard[row, column - 1].Type == FieldTypes.Road))
                    {
                        return true;
                    }
                    else if ((column != 0 && column != 24) && (modelgd.GameBoard[row - 1, column].Type == FieldTypes.Road ||
                        modelgd.GameBoard[row + 1, column].Type == FieldTypes.Road ||
                        modelgd.GameBoard[row, column - 1].Type == FieldTypes.Road ||
                        modelgd.GameBoard[row, column + 1].Type == FieldTypes.Road))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
        }

        private bool CanBePlaced2x2(int row, int column)
        {
            if (row == 24 || column == 24)
                return false;
            if (modelgd.GameBoard[row, column].Zone == ZoneTypes.None &&
                modelgd.GameBoard[row + 1, column].Zone == ZoneTypes.None &&
                modelgd.GameBoard[row, column + 1].Zone == ZoneTypes.None &&
                modelgd.GameBoard[row + 1, column + 1].Zone == ZoneTypes.None &&
                modelgd.GameBoard[row, column].Type != FieldTypes.Road &&
                modelgd.GameBoard[row + 1, column].Type != FieldTypes.Road &&
                modelgd.GameBoard[row, column + 1].Type != FieldTypes.Road &&
                modelgd.GameBoard[row + 1, column + 1].Type != FieldTypes.Road &&
                modelgd.GameBoard[row, column].Type != FieldTypes.Bridge &&
                modelgd.GameBoard[row + 1, column].Type != FieldTypes.Bridge &&
                modelgd.GameBoard[row, column + 1].Type != FieldTypes.Bridge &&
                modelgd.GameBoard[row + 1, column + 1].Type != FieldTypes.Bridge &&
                modelgd.GameBoard[row, column].BuildingID == -1 &&
                modelgd.GameBoard[row + 1, column].BuildingID == -1 &&
                modelgd.GameBoard[row, column + 1].BuildingID == -1 &&
                modelgd.GameBoard[row + 1, column + 1].BuildingID == -1)
            {
                if (CanBePlaced1x1(row, column) || CanBePlaced1x1(row + 1, column) ||
                    CanBePlaced1x1(row, column + 1) || CanBePlaced1x1(row + 1, column + 1))
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        //building community
        public void communityBuilding(int row, int column, int build)
        {
            switch(build)
            {
                case 0:
                    BuildPoliceStation(row, column);
                    break;
                case 1:
                    BuildStadium(row, column);
                    break;
                case 2:
                    BuildFireStation(row, column);
                    break;
                case 3:
                    BuildForest(row, column);
                    break;
            }
        }

        private void BuildForest(int row, int column)
        {
            if (modelgd.buildings is not null)
            {
                if (modelgd.GameBoard[row, column].Type == FieldTypes.Land &&
                    modelgd.GameBoard[row, column].Zone == ZoneTypes.None &&
                    modelgd.GameBoard[row, column].BuildingID == -1)
                {
                    Forest newBuilding = new(ids, row, column)
                    {
                        Level = 1,
                        forestBuilt = modelgd.TimerCounter
                    };
                    ids++;
                    modelgd.GameBoard[row, column].Type = FieldTypes.Forest;
                    modelgd.GameBoard[row, column].BuildingID = newBuilding.Id;
                    costPerPeriod += newBuilding.maintainUnitCost;
                    modelgd.Funds -= newBuilding.buildingCost;
                    modelgd.buildings.Add(newBuilding);
                }
            }
        }

        private void BuildPoliceStation(int row, int column)
        {
            if (modelgd.buildings is not null)
            {
                if (CanBePlaced1x1(row, column))
                {
                    PoliceStation newBuilding = new PoliceStation(ids, row, column);
                    ids++;
                    modelgd.GameBoard[row, column].BuildingID = newBuilding.Id;
                    modelgd.buildings.Add(newBuilding);
                    modelgd.Funds -= newBuilding.buildingCost;
                    costPerPeriod += newBuilding.maintainCost;
                }
            }
        }

        private void BuildStadium(int row, int column)
        {
            if (modelgd.buildings is not null)
            {
                if (CanBePlaced2x2(row, column))
                {
                    Stadium newBuilding = new Stadium(ids, row, column);
                    ids++;
                    modelgd.GameBoard[row, column].BuildingID = newBuilding.Id;
                    modelgd.GameBoard[row + 1, column].BuildingID = newBuilding.Id;
                    modelgd.GameBoard[row, column + 1].BuildingID = newBuilding.Id;
                    modelgd.GameBoard[row + 1, column + 1].BuildingID = newBuilding.Id;
                    modelgd.buildings.Add(newBuilding);
                    modelgd.Funds -= newBuilding.buildingCost;
                    costPerPeriod += newBuilding.maintainCost;
                }
            }
        }

        private void BuildFireStation(int row, int column)
        {
            if (modelgd.buildings is not null)
            {
                if (CanBePlaced1x1(row, column))
                {
                    FireStation newBuilding = new FireStation(ids, row, column);
                    ids++;
                    modelgd.GameBoard[row, column].BuildingID = newBuilding.Id;
                    modelgd.buildings.Add(newBuilding);
                    modelgd.Funds -= newBuilding.buildingCost;
                    costPerPeriod += newBuilding.maintainCost;
                }
            }
        }

        public void roadBuilding(int row, int column)
        {
            if(CanBePlaced1x1(row, column))
            {
                if (modelGameData.GameBoard[row, column].Type == FieldTypes.Water)
                {
                    modelGameData.GameBoard[row, column].Type = FieldTypes.Bridge;
                    modelGameData.Funds -= 500;
                    costPerPeriod += 10;
                }
                else
                {
                    modelGameData.GameBoard[row, column].Type = FieldTypes.Road;
                    modelGameData.Funds -= 100;
                    costPerPeriod += 5;
                }
            }
        }

        //upgrade and destroy
        public void UpgradeM(int id)
        {
            if (modelgd.buildings is not null)
            {
                int ix = modelgd.buildings.FindIndex(x => x.Id.Equals(id));
                if (modelgd.buildings[ix].GetType().Name == "House")
                {
                    House temp = (House)modelgd.buildings[ix];
                    int[] cap = { 7, 12 };
                    if (temp.Level < 3)
                    {
                        temp.Level++;
                        temp.Capacity = cap[temp.Level - 2];
                        modelgd.buildings[ix] = temp;
                        modelgd.Funds -= 500;
                    }    
                }
                if (modelgd.buildings[ix].GetType().Name == "Commercials")
                {
                    Commercials temp = (Commercials)modelgd.buildings[ix];
                    int[] cap = { 10, 15 };
                    if (temp.Level < 3)
                    {
                        temp.Level++;
                        temp.Capacity = cap[temp.Level - 2];
                        modelgd.buildings[ix] = temp;
                        modelgd.Funds -= 1000;
                    }
                }
                if (modelgd.buildings[ix].GetType().Name == "Factory")
                {
                    Factory temp = (Factory)modelgd.buildings[ix];
                    int[] cap = { 14, 20 };
                    if (temp.Level < 3)
                    {
                        temp.Level++;
                        temp.Capacity = cap[temp.Level - 2];
                        modelgd.buildings[ix] = temp;
                        modelgd.Funds -= 1000;
                    }
                }
            }
        }

        public void DestroyM(int row, int column)
        {
            if (modelgd.buildings is not null && modelgd.peoples is not null)
            {
                if (row == 12 && column == 0) return;

                if (modelgd.GameBoard[row, column].Type == FieldTypes.Road)
                {
                    modelgd.GameBoard[row, column].Type = FieldTypes.Land;
                    modelgd.Funds -= 50;
                    costPerPeriod -= 5;
                }
                if (modelgd.GameBoard[row, column].Type == FieldTypes.Bridge)
                {
                    modelgd.GameBoard[row, column].Type = FieldTypes.Water;
                    modelgd.Funds -= 100;
                    costPerPeriod -= 10;
                }
                if (modelgd.GameBoard[row, column].Type == FieldTypes.Forest)
                {
                    modelgd.GameBoard[row, column].Type = FieldTypes.Land;
                }
                if (modelgd.GameBoard[row, column].Zone != ZoneTypes.None)
                {
                    modelgd.GameBoard[row, column].Zone = ZoneTypes.None;
                }
                if (modelgd.GameBoard[row, column].BuildingID != -1)
                {
                    modelgd.Funds -= 200;
                    int id = modelgd.GameBoard[row, column].BuildingID;
                    int ixb = modelgd.buildings.FindIndex(x => x.Id.Equals(id));
                    if (modelgd.buildings[ixb].GetType().Name == "House")
                    {
                        modelgd.Population -= modelgd.buildings[ixb].Occupants;
                        List<int> wpids = new List<int>();
                        for (int i = 0; i < modelgd.peoples.Count; i++)
                        {
                            if (modelgd.peoples[i].HomeId == id)
                            {
                                wpids.Add(modelgd.peoples[i].WorkplaceId);
                            }
                        }
                        foreach (int item in wpids)
                        {
                            int x = modelgd.buildings.FindIndex(x => x.Id.Equals(item));
                            modelgd.buildings[x].Occupants--;
                        }
                        modelgd.peoples.RemoveAll(x => x.HomeId.Equals(id));
                    }
                    if (modelgd.buildings[ixb].GetType().Name == "Factory" || modelgd.buildings[ixb].GetType().Name == "Commercials")
                    {
                        modelgd.Population -= modelgd.buildings[ixb].Occupants;
                        List<int> hids = new List<int>();
                        for (int i = 0; i < modelgd.peoples.Count; i++)
                        {
                            if (modelgd.peoples[i].WorkplaceId == id)
                            {
                                hids.Add(modelgd.peoples[i].HomeId);
                            }
                        }
                        foreach (int item in hids)
                        {
                            int x = modelgd.buildings.FindIndex(x => x.Id.Equals(item));
                            modelgd.buildings[x].Occupants--;
                        }
                        modelgd.peoples.RemoveAll(x => x.WorkplaceId.Equals(id));
                    }
                    
                    modelgd.buildings.RemoveAt(ixb);
                    for(int i = 0; i < 25; i++)
                    {
                        for(int k = 0; k < 25; k++)
                        {
                            if (modelgd.GameBoard[i, k].BuildingID == id)
                            {
                                modelgd.GameBoard[i, k].BuildingID = -1;
                            }
                        }
                    }
                }
            }
        }

        //building quality
        public void CalculateQuality()
        {
            if (modelgd.buildings is not null)
            {
                foreach (Building b in modelgd.buildings)
                {
                    int quality = 50;
                    if (b.GetType().Name == "House")
                    {
                        if (StadiumEffect(b.Row, b.Col))
                        {
                            quality += 20;
                        }
                        quality += ForestEffect(b.Row, b.Col);
                        quality += PoliceEffect(b.Row, b.Col);
                        quality += IndustryEffect(b.Row, b.Col);
                        if (FireEffect(b.Row, b.Col))
                        {
                            b.FireRisk = 1;
                        }
                        else
                        {
                            b.FireRisk = 5;
                            quality -= 10;
                        }
                        b.Quality = quality;
                    }
                    if (b.GetType().Name == "Factory" || b.GetType().Name == "Commercials")
                    {
                        if (StadiumEffect(b.Row, b.Col))
                        {
                            quality += 20;
                        }
                        quality += ForestEffect(b.Row, b.Col);
                        quality += PoliceEffect(b.Row, b.Col);
                        if (FireEffect(b.Row, b.Col))
                        {
                            b.FireRisk = 2;
                        }
                        else
                        {
                            b.FireRisk = 10;
                            quality -= 20;
                        }
                        b.Quality = quality;
                    }
                }
            }
        }

        private int IndustryEffect(int row, int col)
        {
            int xs, xe, ys, ye;
            if (row < 2)
            {
                xs = 0;
                xe = row + 2;
            }
            else if (row > 22)
            {
                xs = row - 2;
                xe = 24;
            }
            else
            {
                xs = row - 2;
                xe = row + 2;
            }
            if (col < 2)
            {
                ys = 0;
                ye = col + 2;
            }
            else if (col > 22)
            {
                ys = col - 2;
                ye = 24;
            }
            else
            {
                ys = col - 2;
                ye = col + 2;
            }

            for (int i = xs; i <= xe; i++)
            {
                for (int j = ys; j <= ye; j++)
                {
                    if (modelgd.GameBoard[i, j].Zone == ZoneTypes.Industrial)
                    {
                        List<Tuple<int, int, int>> path = PathFinder(row, col, i, j);
                        if (path.Count == 0 || (path.Count == 1 && (row != i || col != j)))
                        {
                            return -20;
                        }
                        else
                        {
                            foreach (Tuple<int, int, int> p in path)
                            {
                                if (modelgd.GameBoard[p.Item1, p.Item2].Type == FieldTypes.Forest)
                                    return -5;
                            }
                            return -10;
                        }
                    }
                }
            }
            return 0;
        }

        private bool StadiumEffect(int row, int col)
        {
            if (modelgd.buildings is not null)
            {
                foreach (Building b in modelgd.buildings)
                {
                    if (b.GetType().Name == "Stadium")
                    {
                        if (row <= b.Row && col <= b.Col && Math.Abs(row - b.Row) + Math.Abs(col - b.Col) <= 10)
                            return true;
                        if (Math.Abs(row - b.Row) + Math.Abs(col - b.Col) <= 11)
                            return true;
                    }
                }
                return false;
            }
            return false;
        }

        private bool FireEffect(int row, int col)
        {
            if (modelgd.buildings is not null)
            {
                foreach (Building b in modelgd.buildings)
                {
                    if (b.GetType().Name == "FireStation")
                    {
                        if (Math.Abs(row - b.Row) + Math.Abs(col - b.Col) <= 10)
                            return true;
                    }
                }
            }
            return false;
        }

        private int PoliceEffect(int row, int col)
        {
            if (modelgd.buildings is not null)
            {
                foreach (Building b in modelgd.buildings)
                {
                    if (b.GetType().Name == "PoliceStation")
                    {
                        if (Math.Abs(row - b.Row) + Math.Abs(col - b.Col) <= 10)
                            return 10;
                    }
                }
            }
            return -20;
        }

        private int ForestEffect(int row, int col)
        {
            int xs, xe, ys, ye;
            if (row < 3)
            {
                xs = 0;
                xe = row + 3;
            }
            else if (row > 21)
            {
                xs = row - 3;
                xe = 24;
            }
            else
            {
                xs = row - 3;
                xe = row + 3;
            }
            if (col < 3)
            {
                ys = 0;
                ye = col + 3;
            }
            else if (col > 21)
            {
                ys = col - 3;
                ye = 24;
            }
            else
            {
                ys = col - 3;
                ye = col + 3;
            }
            int effect = 0;
            for (int i = xs; i <= xe; i++)
            {
                for (int j = ys; j <= ye; j++)
                {
                    if(i < 0 ||
                        i >= modelgd.GameBoard.GetLength(0) || j < 0 || j >= modelgd.GameBoard.GetLength(1)) continue;
                    if (modelgd.GameBoard[i, j].Type == FieldTypes.Forest &&
                        Math.Abs(row - i) + Math.Abs(col - j) <= 3)
                    {
                        List<Tuple<int, int, int>> path = PathFinder(row, col, i, j);
                        int tempeff;
                        if (path.Count == 0 || (path.Count == 1 && (row != i || col != j)))
                        {
                            if (modelgd.GameBoard[i, j].BuildingID == -1)
                                tempeff = 20;
                            else
                            {
                                Forest tmp = (Forest)modelgd.buildings.Find(x => x.Id.Equals(modelgd.GameBoard[i, j].BuildingID));
                                tempeff = 2 * tmp.Level;
                            }
                        }
                        else
                        {
                            if (row == i || col == j)
                            {
                                foreach (Tuple<int, int, int> p in path)
                                {
                                    if (modelgd.GameBoard[p.Item1, p.Item2].BuildingID != -1)
                                    {
                                        tempeff = 0;
                                    }
                                }
                                if (modelgd.GameBoard[i, j].BuildingID == -1)
                                    tempeff = 10;
                                else
                                {
                                    Forest tmp = (Forest)modelgd.buildings.Find(x => x.Id.Equals(modelgd.GameBoard[i, j].BuildingID));
                                    tempeff = tmp.Level;
                                }
                            }
                            else
                            {
                                foreach (Tuple<int, int, int> p in path)
                                {
                                    if (modelgd.GameBoard[p.Item1, p.Item2].BuildingID == -1)
                                    {
                                        if (modelgd.GameBoard[i, j].BuildingID == -1)
                                            tempeff = 10;
                                        else
                                        {
                                            Forest tmp = (Forest)modelgd.buildings.Find(x => x.Id.Equals(modelgd.GameBoard[i, j].BuildingID));
                                            tempeff = tmp.Level;
                                        }
                                    }
                                }
                                tempeff = 0;
                            }
                        }
                        if (tempeff == 20)
                            return 20;
                        if (tempeff > effect)
                            effect = tempeff;
                    }
                }
            }
            return effect;
        }

        private List<Tuple<int, int, int>> PathFinder(int row1, int col1, int row2, int col2)
        {
            int[] dx = { -1, 1 };
            int[] dy = { -1, 1 };
            Queue<Tuple<int, int, int>> queue = new();
            queue.Enqueue(new Tuple<int, int, int>(row1, col1, Math.Abs(row1 - row2) + Math.Abs(col1 - col2)));
            List<Tuple<int, int, int>> path = new();

            while (queue.Count > 0)
            {
                Tuple<int, int, int> curr = queue.Dequeue();
                int x = curr.Item1;
                int y = curr.Item2;
                int d = curr.Item3;

                if (x == col1 && y == col2)
                {
                    return path;
                }

                if (Math.Abs(x - row2) > Math.Abs(y - col2))
                {
                    for (int i = 0; i < dx.Length; i++)
                    {
                        int nextX = x + dx[i];
                        if (nextX < 0 || nextX > 24)
                            continue;
                        else
                        {
                            int nextD = Math.Abs(nextX - row2) + Math.Abs(y - col2);
                            if (nextD < d && nextD > 0)
                            {
                                queue.Enqueue(new Tuple<int, int, int>(nextX, y, nextD));
                                path.Add(new Tuple<int, int, int>(nextX, y, nextD));
                            }
                            else if (nextD < d && nextD == 0)
                            {
                                queue.Enqueue(new Tuple<int, int, int>(nextX, y, nextD));
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < dy.Length; i++)
                    {
                        int nextY = y + dy[i];
                        if (nextY < 0 || nextY > 24)
                            continue;
                        else
                        {
                            int nextD = Math.Abs(x - row2) + Math.Abs(nextY - col2);
                            if (nextD < d)
                            {
                                queue.Enqueue(new Tuple<int, int, int>(x, nextY, nextD));
                                path.Add(new Tuple<int, int, int>(x, nextY, nextD));
                            }
                        }
                    }
                }
            }
            return path;
        }

        //Financials methods
        public void TaxChange(int change)
        {
            if (modelGameData.TaxRate + change >= 0 && modelGameData.TaxRate + change <= 100)
                modelGameData.TaxRate += change;
        }

        private void MonthlyFinance()
        {
            incomePerPeriod = modelgd.TaxRate * modelgd.Population;
            if (modelgd.TimerCounter % 30 == 0)
                modelGameData.Funds += incomePerPeriod - costPerPeriod;
        }

        //happiness
        public void CalculateHappiness()
        {
            if (modelgd.peoples is not null && modelgd.buildings is not null)
            {
                int h = 0;
                int c = modelgd.peoples.Count;
                for (int i = 0; i < modelgd.peoples.Count; i++)
                {
                    Building? home = modelgd.buildings.Find(x => x.Id.Equals(modelgd.peoples[i].HomeId));
                    int a = home!.Quality;
                    Building? wp = modelgd.buildings.Find(x => x.Id == modelgd.peoples[i].WorkplaceId);
                    a += wp!.Quality;
                    
                    a /= 2;
                    a += DistanceEffect(modelgd.peoples[i].HomeId, modelgd.peoples[i].WorkplaceId);
                    if (a > 100)
                        a = 100;
                    if (a < 0)
                        a = 0;
                    a += TaxEffect();
                    if (a > 100)
                        a = 100;
                    if (a < 0)
                        a = 0;
                    modelgd.peoples[i].Happiness = a;
                    h += a;
                }
                if (c == 0)
                {
                    return;
                }
                modelGameData.Happiness = h / c;
            }
            else
                modelGameData.Happiness = 75;
        }

        private int TaxEffect()
        {
            if (modelgd.TaxRate >= 50)
                return -1 * modelgd.TaxRate;
            if (modelgd.TaxRate >= 25)
                return -2 * (modelgd.TaxRate - 25);
            if (modelgd.TaxRate >= 20)
                return 0;
            if (modelgd.TaxRate >= 10)
                return 2 * (20 - modelgd.TaxRate);
            else
                return 20 + 5 * (10 - modelgd.TaxRate);
        }

        private int DistanceEffect(int homeId, int workplaceId)
        {
            if (modelgd.buildings is not null)
            {
                Building? home = modelgd.buildings.Find(x => x.Id.Equals(homeId));
                Building? wp = modelgd.buildings.Find(x => x.Id.Equals(workplaceId));
                if (Distance(home!.Row, home.Col, wp!.Row, wp.Col) <= 2.0)
                {
                    return 15;
                }
                if (Distance(home!.Row, home.Col, wp!.Row, wp.Col) <= 4.0)
                {
                    return 5;
                }
                if (Distance(home!.Row, home.Col, wp!.Row, wp.Col) <= 6.0)
                {
                    return 0;
                }
                if (Distance(home!.Row, home.Col, wp!.Row, wp.Col) <= 8.0)
                {
                    return -10;
                }
                if (Distance(home!.Row, home.Col, wp!.Row, wp.Col) <= 10.0)
                {
                    return -25;
                }
                else
                {
                    return -50;
                }
            }
            return 0;
        }

        public List<Tuple<int, int>> MoveBetweenHomeAndWork(int row1, int col1, int row2, int col2)
        {
            List<Tuple<int, int>> result = new List<Tuple<int, int>>();
            bool[,] visited = new bool[25, 25];
            int[,] distances = new int[25, 25];

            for (int i = 0; i < 25; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    visited[i, j] = false;
                    distances[i, j] = 100;
                }
            }
            Queue<Tuple<int, int, int>> queue = new Queue<Tuple<int, int, int>>();
            queue.Enqueue(new Tuple<int, int, int>(row2, col2, 0));
            visited[row2, col2] = true;
            distances[row2, col2] = 0;
            int[] dx = { 0, 1, 0, -1 };
            int[] dy = { 1, 0, -1, 0 };

            while (queue.Count > 0)
            {
                Tuple<int, int, int> tuple = queue.Dequeue();
                int nextX, nextY;

                for (int i = 0; i < dx.Length; i++)
                {
                    nextX = tuple.Item1 + dx[i];
                    nextY = tuple.Item2 + dy[i];
                    if (nextX < 0 || nextX > 24 || nextY < 0 || nextY > 24)
                        continue;
                    if (modelgd.GameBoard[nextX, nextY].Type != FieldTypes.Road
                        && modelgd.GameBoard[nextX, nextY].Type != FieldTypes.Bridge
                        && (nextX != row1 || nextY != col1))
                        continue;
                    if (visited[nextX, nextY])
                        continue;
                    distances[nextX, nextY] = tuple.Item3 + 1;
                    visited[nextX, nextY] = true;
                    queue.Enqueue(new Tuple<int, int, int>(nextX, nextY, distances[nextX, nextY]));
                }
            }
            if (distances[row1, col1] == 100)
                return result;

            result.Add(new Tuple<int, int>(row1, col1));
            int currX = row1;
            int currY = col1;
            int currD = distances[row1, col1];
            bool onRoad = true;

            while (currX != row2 && currY != col2 && onRoad && currD > 0)
            {
                onRoad = false;
                int nextX, nextY, nextD;
                int newX = -1;
                int newY = -1;
                for (int i = 0; i < dx.Length; i++)
                {
                    nextX = currX + dx[i];
                    nextY = currY + dy[i];
                    if (nextX < 0 || nextX > 24 || nextY < 0 || nextY > 24)
                        continue;
                    nextD = distances[nextX, nextY];
                    if (nextD < currD)
                    {
                        result.Add(new Tuple<int, int>(nextX, nextY));
                        newX = nextX;
                        newY = nextY;
                        currD = nextD;
                        onRoad = true;
                    }
                }
                currX = newX;
                currY = newY;
            }
            result.Add(new Tuple<int, int>(row2, col2));
            return result;
        }
    }
}
