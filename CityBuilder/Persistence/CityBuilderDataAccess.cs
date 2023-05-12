using CityBuilder.Persistence.Buildings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace CityBuilder.Persistence
{
    public class CityBuilderDataAccess : ICityBuilderDataAccess
    {
        public async Task<GameData> LoadAsync(string path)
        {
            if (path == null) 
                throw new ArgumentNullException(nameof(path));
            var gameData = new GameData();

            try
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    String? line = await reader.ReadLineAsync();
                    if (line == null)
                        throw new CityBuilderException();
                    String[] data = line.Split(' ');
                    gameData.Funds = int.Parse(data[0]);
                    gameData.TimerCounter = int.Parse(data[1]);
                    gameData.TaxRate = int.Parse(data[2]);

                    gameData.peoples = new List<People>();
                    line = await reader.ReadLineAsync();
                    if (line != null)
                    {
                        data = line.Split(' ');
                        for (int i = 0; i < data.Length; i += 2)
                        {
                            People temp = new People(int.Parse(data[i]), int.Parse(data[i + 1]));
                            gameData.peoples.Add(temp);
                        }
                    }
                    
                    gameData.buildings = new List<Building>();
                    line = await reader.ReadLineAsync();
                    if (line != null)
                    {
                        data = line.Split(' ');
                        for (int i = 0; i < data.Length; i += 5)
                        {
                            if (data[i] == "House")
                            {
                                House tmp = new House(int.Parse(data[i + 1]), int.Parse(data[i + 2]), int.Parse(data[i + 3]));
                                tmp.Level = int.Parse(data[i + 4]);
                                gameData.buildings.Add(tmp);
                            }
                            if (data[i] == "Factory")
                            {
                                Factory tmp = new Factory(int.Parse(data[i + 1]), int.Parse(data[i + 2]), int.Parse(data[i + 3]));
                                tmp.Level = int.Parse(data[i + 4]);
                                gameData.buildings.Add(tmp);
                            }
                            if (data[i] == "Commercials")
                            {
                                Commercials tmp = new Commercials(int.Parse(data[i + 1]), int.Parse(data[i + 2]), int.Parse(data[i + 3]));
                                tmp.Level = int.Parse(data[i + 4]);
                                gameData.buildings.Add(tmp);
                            }
                            if (data[i] == "PoliceStation")
                            {
                                PoliceStation tmp = new PoliceStation(int.Parse(data[i + 1]), int.Parse(data[i + 2]), int.Parse(data[i + 3]));
                                gameData.buildings.Add(tmp);
                            }
                            if (data[i] == "FireStation")
                            {
                                FireStation tmp = new FireStation(int.Parse(data[i + 1]), int.Parse(data[i + 2]), int.Parse(data[i + 3]));
                                gameData.buildings.Add(tmp);
                            }
                            if (data[i] == "Stadium")
                            {
                                Stadium tmp = new Stadium(int.Parse(data[i + 1]), int.Parse(data[i + 2]), int.Parse(data[i + 3]));
                                gameData.buildings.Add(tmp);
                            }
                        }
                    }
                    

                    gameData.GameBoard = new Field[25, 25];
                    line = await reader.ReadLineAsync();
                    if (line == null)
                        throw new CityBuilderException();
                    data = line.Split(' ');
                    for (int i = 0; i < data.Length; i += 6)
                    {
                        FieldTypes ft = ConvertStringToFieldTypes(data[i + 2]);
                        ZoneTypes zt = ConvertStringToZoneTypes(data[i + 3]);
                        gameData.GameBoard[i / 6, i % 6] = new Field(int.Parse(data[i]), int.Parse(data[i + 1]), ft, zt);
                        gameData.GameBoard[i / 6, i % 6].ZonePlaced = int.Parse(data[i + 4]);
                        gameData.GameBoard[i / 6, i % 6].BuildingID = int.Parse(data[i + 5]);
                    }
                }
                
            }
            catch
            {
                throw new CityBuilderException();
            }

            return gameData;
        }

        private ZoneTypes ConvertStringToZoneTypes(string v)
        {
            if (v == "ZoneTypes.Residential")
                return ZoneTypes.Residential;
            else if (v == "ZoneTypes.Industrial")
                return ZoneTypes.Industrial;
            else if (v == "ZoneTypes.Commercial")
                return ZoneTypes.Commercial;
            else
                return ZoneTypes.None;
        }

        private FieldTypes ConvertStringToFieldTypes(string v)
        {
            if (v == "FieldTypes.Water")
                return FieldTypes.Water;
            if (v == "FieldTypes.Road")
                return FieldTypes.Road;
            if (v == "FieldTypes.Bridge")
                return FieldTypes.Bridge;
            if (v == "FieldTypes.Forest")
                return FieldTypes.Forest;
            else
                return FieldTypes.Land;
        }

        public async Task SaveAsync(string path, GameData table)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(new FileStream(path, FileMode.Create, FileAccess.Write)))
                {
                    await writer.WriteAsync(" " + table.Funds);
                    await writer.WriteAsync(" " + table.TimerCounter);
                    await writer.WriteLineAsync(" " + table.TaxRate);

                    if (table.peoples != null)
                    {
                        foreach (People p in table.peoples)
                        {
                            await writer.WriteAsync(p.HomeId + " " + p.WorkplaceId + " ");
                        }
                        await writer.WriteLineAsync();
                    }
                    if (table.buildings != null)
                    {
                        foreach (Building b in table.buildings)
                        {
                            await writer.WriteAsync(b.GetType().Name + " " + b.Id + " " + b.Row + " " + b.Col + " ");
                            if (b.GetType().Name == "House")
                            {
                                House tmp = (House)b;
                                await writer.WriteAsync(tmp.Level + " ");
                            }
                            else if (b.GetType().Name == "Factory")
                            {
                                Factory tmp = (Factory)b;
                                await writer.WriteAsync(tmp.Level + " ");
                            }
                            else if (b.GetType().Name == "Commercials")
                            {
                                Commercials tmp = (Commercials)b;
                                await writer.WriteAsync(tmp.Level + " ");
                            }
                            else
                            {
                                await writer.WriteAsync("-1 ");
                            }
                        }
                        await writer.WriteLineAsync();
                    }
                    for (int i = 0; i < 25; i++)
                    {
                        for (int j = 0; j < 25; j++)
                        {
                            await writer.WriteAsync(table.GameBoard[i, j].Row + " " + table.GameBoard[i, j].Column + " " +
                                table.GameBoard[i, j].Type + " " + table.GameBoard[i, j].Zone + " " +
                                table.GameBoard[i, j].ZonePlaced + " " + table.GameBoard[i, j].BuildingID + " ");
                        }
                        await writer.WriteLineAsync();
                    }
                }
            }
            catch
            {
                throw new CityBuilderException();
            }
        }
    }
}
