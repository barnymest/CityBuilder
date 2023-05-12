using CityBuilder.Model;
using CityBuilder.Persistence;
using CityBuilder.Persistence.Buildings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace CityBuilderWPF.ViewModel
{
    public class CityBuilderViewModel : ViewModelBase
    {
        private CityBuilderGameModel _model = null!;
        private Int32 _tableSize;
        private bool placementModeRoad = false;
        private bool placementModeResidential = false;
        private bool placementModeIndustrial = false;
        private bool placementModeCommercial = false;
        private bool placementModePoliceStation= false;
        private bool placementModeFireStation= false;
        private bool placementModeStadium = false;
        private bool placementModeForest = false;
        private bool placementModeDestroy = false;
        

        public DelegateCommand NewGameCommand { get; private set; }
        public DelegateCommand LoadGameCommand { get; private set; }
        public DelegateCommand SaveGameCommand { get; private set; }
        public DelegateCommand ExitCommand { get; private set; }
        public DelegateCommand PauseCommand { get; private set; }
        public ObservableCollection<Field> Fields { get; set; }

        //public Brush ActivePlacementMode { get => ActivePlacementModeMethod(); } //todo: get function out of Field
        public String TempActivePlacementMode { get => TempActivePlacementModeMethod(); } //tod change back
        public bool PlacementModeRoad { get => placementModeRoad; set => placementModeRoad = value; }
        public bool PlacementModeResidential { get => placementModeResidential; set => placementModeResidential = value; }
        public bool PlacementModeIndustrial { get => placementModeIndustrial; set => placementModeIndustrial= value; }
        public bool PlacementModeCommercial{ get => placementModeCommercial; set => placementModeCommercial = value; }
        public bool PlacementModePoliceStation{ get => placementModePoliceStation; set => placementModePoliceStation = value; }
        public bool PlacementModeFireStation{ get => placementModeFireStation; set => placementModeFireStation = value; }
        public bool PlacementModeStadium{ get => placementModeStadium; set => placementModeStadium = value; }
        public bool PlacementModeForest{ get => placementModeForest; set => placementModeForest= value; }
        public bool PlacementModeDestroy { get => placementModeDestroy; set => placementModeDestroy = value; }

        public String Happiness { get { return _model.modelgd.Happiness.ToString("g") + "%"; } }
        public String Money { get { return _model.modelgd.Funds.ToString("g") + "$"; } }
        public String TaxRate { get { return _model.modelgd.TaxRate.ToString("g") + "%"; } }
        //public String GameTime { get { return _model.modelgd.TimerCounter.ToString("g") + " Days"; } }
        ///Lehetséges kiírás
        public String GameTime { get { return DateTime.Now.AddDays(_model.modelgd.TimerCounter).ToString("yyyy/MM/dd"); } }

        public String Population { get { return _model.modelgd.Population.ToString("g") + " people"; } }

        public Int32 TableSize
        {
            get => _tableSize;
            set
            {
                if (_tableSize != value)
                {
                    _tableSize = value;
                    Debug.WriteLine(TableSize + " before");
                    OnPropertyChanged(nameof(TableSize));
                    Debug.WriteLine(TableSize + " after");
                }
            }
        }

        
        public CityBuilderViewModel(CityBuilderGameModel model)
        {
            _model = model;
            _model.GameAdvanced += new EventHandler<int>(Model_GameAdvanced);
            _model.BuildingBuilt += new EventHandler<CityBuilder.Model.Events.ZoneToBuildEventArgs>(BuildAccordingBuildingVM);
         
            NewGameCommand = new DelegateCommand(param => OnNewGame());
            ExitCommand = new DelegateCommand(param => OnExitGame());

            Fields = new ObservableCollection<Field>();
            GenerateTable();
            RefreshTable();
        }
        public void GenerateTable()
        { 
            if (Fields.Count > 0)
            {
                Fields.Clear();
            }

            for (int i = 0; i < 25; i++)
            {
                for (int j = 0; j < 25; j++)
                {
                    Fields.Add(new Field(i, j, FieldTypes.Land, ZoneTypes.None));
                }
            }
        }

        public void RefreshTable()
        {
            foreach (Field field in Fields)
            {
                if (_model.modelgd.GameBoard[field.X, field.Y].Zone == ZoneTypes.None)
                {

                    switch (_model.modelgd.GameBoard[field.X, field.Y].Type)
                    {
                        case FieldTypes.Land:
                            field.Color = "Green";
                            break;
                        case FieldTypes.Road:
                            field.Color = "Gray";
                            break;
                        case FieldTypes.Bridge:
                            field.Color = "DarkGray";
                            break;
                        case FieldTypes.Water:
                            field.Color = "Blue";
                            break;
                        case FieldTypes.Forest:
                            field.Color = "DarkGreen";
                            break;
                    }
                } else { 
                    switch (_model.modelgd.GameBoard[field.X, field.Y].Zone)
                    {
                        case ZoneTypes.Residential:
                            field.Color = "LightGreen";
                            break;
                        case ZoneTypes.Commercial:
                            field.Color = "Yellow";
                            break;
                        case ZoneTypes.Industrial:
                            field.Color = "LightSkyBlue";
                            break;
                        case ZoneTypes.None:
                            field.Color = "";
                            break;
                        
                        //todo: update field and the enums, why here?
                    }
                }
                if (_model.modelgd.GameBoard[field.X, field.Y].BuildingID > -1)
                {
                    //todo: firestation stadium ect
                    field.Color = "Black";
                }
            }
            OnPropertyChanged(nameof(GameTime));
            OnPropertyChanged(nameof(Money));
            OnPropertyChanged(nameof(Happiness));
            OnPropertyChanged(nameof(Population));
        }

        public void OnGameFieldClick(int x, int y)
        {

            if (placementModeDestroy)
            {
                _model.DestroyM(x, y);
                Debug.Write(x + " " + y + " Destroyed");
                RefreshTable();
                return;
            }

            if ((_model.modelgd.GameBoard[x, y].Type != FieldTypes.Water && _model.modelgd.GameBoard[x, y].Type != FieldTypes.Land) || _model.modelgd.GameBoard[x, y].Zone != ZoneTypes.None || _model.modelgd.GameBoard[x, y].BuildingID > -1) return;

            //todo: recheck logic, 
            if (IsPlaceable(x, y, 1))
            {
                if (placementModeRoad)
                {
                    _model.roadBuilding(x, y);    
                } 
                if (placementModeResidential)
                {
                    _model.ZoneChanged(x, y, ZoneTypes.Residential);
                }
                if (placementModeCommercial)
                {
                    _model.ZoneChanged(x, y, ZoneTypes.Commercial);
                }
                if (placementModeIndustrial)
                {
                    _model.ZoneChanged(x, y, ZoneTypes.Industrial);
                }
                if (placementModeFireStation)
                {
                    _model.communityBuilding(x, y, 2);
                }
                if (placementModePoliceStation)
                {
                    _model.communityBuilding(x, y, 0);
                }
                if (placementModeForest)
                {
                    _model.communityBuilding(x, y, 3);
                }
                if (placementModeStadium)
                {
                    _model.communityBuilding(x, y, 1);
                }

                RefreshTable();
            }
        }

        public void UpgradeVM(int id)
        {
            if (id < 0) return;
            _model.UpgradeM(id); 
            RefreshTable();
        }

        public void BuildAccordingBuildingVM(object? sender, CityBuilder.Model.Events.ZoneToBuildEventArgs args)
        {
            if (_model.modelgd.buildings is null) return;
            if (_model.modelgd.GameBoard[args.X, args.Y] is null) return;
            int id = _model.modelgd.GameBoard[args.X, args.Y].BuildingID;
            Building b = _model.modelgd.buildings.Find(x => x.Id == id);

            foreach(Field f in Fields)
            {
                if (_model.modelgd.GameBoard[f.X, f.Y].BuildingID == id)
                {
                    switch (b)
                    {
                        case House:
                            f.Color = "Peru";
                            break;
                        case Forest:
                            f.Color = "DarkGreen";
                            break;
                        case Commercials:
                            f.Color = "Violet";
                            break;
                        case Factory:
                            f.Color = "DarkYellow";
                            break;
                        case FireStation:
                            f.Color = "Red";
                            break;
                        case PoliceStation:
                            f.Color = "MidnightBlue";
                            break;
                        case Stadium:
                            f.Color = "DarkOrange";
                            break;
                    }
                }
            }

        }

        public void PlacementModeDestroyClicked()
        {
            PlacementModeDestroy = !PlacementModeDestroy;
            PlacementModeCommercial = false;
            PlacementModeIndustrial = false;
            PlacementModeResidential = false;
            PlacementModeRoad = false;
            PlacementModeForest = false;
            PlacementModeStadium = false;
            PlacementModeFireStation = false;
            PlacementModePoliceStation = false;
            Debug.Write(PlacementModeDestroy);
        }

        public void PlacementModeRoadClicked()
        {
            PlacementModeRoad = !PlacementModeRoad;
            PlacementModeCommercial = false; 
            PlacementModeIndustrial = false;
            PlacementModeResidential = false;
            PlacementModeDestroy = false;
            PlacementModeForest = false;
            PlacementModeStadium = false;
            PlacementModeFireStation = false;
            PlacementModePoliceStation = false;
            Debug.Write(PlacementModeRoad);
            //todo: update us
        }

        public void PlacementModeResidentialClicked()
        {
            PlacementModeResidential = !PlacementModeResidential;
            PlacementModeCommercial = false;
            PlacementModeIndustrial = false;
            PlacementModeRoad = false;
            PlacementModeDestroy = false;
            PlacementModeForest = false;
            PlacementModeStadium = false;
            PlacementModeFireStation = false;
            PlacementModePoliceStation = false;
            Debug.Write(PlacementModeResidential);
            //todo: update us
        }

        public void PlacementModeIndustrialClicked()
        {
            PlacementModeIndustrial = !PlacementModeIndustrial;
            PlacementModeCommercial = false;
            PlacementModeRoad = false;
            PlacementModeResidential = false;
            PlacementModeDestroy = false;
            PlacementModeForest = false;
            PlacementModeStadium = false;
            PlacementModeFireStation = false;
            PlacementModePoliceStation = false;
            Debug.Write(PlacementModeIndustrial);
            //todo: update us
        }

        public void PlacementModeCommercialClicked()
        {
            PlacementModeCommercial = !PlacementModeCommercial;
            PlacementModeRoad = false;
            PlacementModeIndustrial = false;
            PlacementModeResidential = false;
            PlacementModeDestroy = false;
            PlacementModeForest = false;
            PlacementModeStadium = false;
            PlacementModeFireStation = false;
            PlacementModePoliceStation = false;
            Debug.Write(PlacementModeCommercial);
        }

        public void PlacementModeForestClicked()
        {
            PlacementModeForest = !PlacementModeForest;
            PlacementModeRoad = false;
            PlacementModeIndustrial = false;
            PlacementModeResidential = false;
            PlacementModeDestroy = false;
            PlacementModeCommercial = false;
            PlacementModeStadium = false;
            PlacementModeFireStation = false;
            PlacementModePoliceStation = false;
        }

        public void PlacementModeStadiumClicked()
        {
            PlacementModeStadium = !PlacementModeStadium;
            PlacementModeRoad = false;
            PlacementModeIndustrial = false;
            PlacementModeResidential = false;
            PlacementModeDestroy = false;
            PlacementModeForest = false;
            PlacementModeCommercial = false;
            PlacementModeFireStation = false;
            PlacementModePoliceStation = false;
        }

        public void PlacementModeFireStationClicked()
        {
            PlacementModeFireStation = !PlacementModeFireStation;
            PlacementModeRoad = false;
            PlacementModeIndustrial = false;
            PlacementModeResidential = false;
            PlacementModeDestroy = false;
            PlacementModeForest = false;
            PlacementModeStadium = false;
            PlacementModeCommercial = false;
            PlacementModePoliceStation = false;
        }

        public void PlacementModePoliceStationClicked()
        {
            PlacementModePoliceStation = !PlacementModePoliceStation;
            PlacementModeRoad = false;
            PlacementModeIndustrial = false;
            PlacementModeResidential = false;
            PlacementModeDestroy = false;
            PlacementModeForest = false;
            PlacementModeStadium = false;
            PlacementModeFireStation = false;
            PlacementModeCommercial = false;
        }

        public void IncrementTax(int value)
        {
            _model.TaxChange(value);
            OnPropertyChanged(nameof(TaxRate));

        }
        public void DecrementTax(int value) 
        {
            _model.TaxChange(-1*value);
            OnPropertyChanged(nameof(TaxRate));
        }

        private Brush ActivePlacementModeMethod()
        {
             if(placementModeRoad)
             {
                return Brushes.Gray;
             }
            else if(placementModeResidential)
            {
                return Brushes.LightGreen;
            }
            else if (placementModeIndustrial)
            {
                return Brushes.LightSkyBlue;
            }
            else if (placementModeCommercial)
            {
                return Brushes.Yellow;
            }
            else if (PlacementModePoliceStation)
            {
                return Brushes.DarkBlue;
            }
            else if (PlacementModeFireStation)
            {
                return Brushes.Red;
            }
            else if (placementModeStadium)
            {
                return Brushes.Orange;
            }
            else if (placementModeForest)
            {
                return Brushes.DarkGreen;
            }
            else
            {
                return Brushes.DarkGray;
            }
             //todo: "cannot place red"
             ///note to "cannot place red": you could get a private prop, and you could handle this from VM as you do it with the placement modes, 
             ///maybe you could handle this from the view, and hook it onto hover / enter functions in MainWindow.xaml.cs 
           
    }
    

        private bool IsPlaceable(int x, int y, int sizeOfBlock)
        {
            bool isItPlaceable = false;

            switch (sizeOfBlock)
            {
                case 1:
                    if (
                ///let's check if there is any building in the 1x1 area you want to build the building
                _model.modelgd.GameBoard[x, y].Zone == ZoneTypes.None
                )
                    {
                        isItPlaceable = true;
                    }
                    break;
                case 2:
                    if (25 >= (x + 2) && 25 >= (y + 2))
                    {
                        if (
                ///let's check if there is any building in the 2x2 area you want to build the building
                _model.modelgd.GameBoard[x, y].Zone == ZoneTypes.None &&
                _model.modelgd.GameBoard[x + 1, y].Zone == ZoneTypes.None &&
                _model.modelgd.GameBoard[x, y + 1].Zone == ZoneTypes.None &&
                _model.modelgd.GameBoard[x + 1, y + 1].Zone == ZoneTypes.None &&
                ///let's check for anything non Land in the 2x2 area
                _model.modelgd.GameBoard[x, y].Type == FieldTypes.Land &&
                _model.modelgd.GameBoard[x + 1, y].Type == FieldTypes.Land &&
                _model.modelgd.GameBoard[x, y + 1].Type == FieldTypes.Land &&
                _model.modelgd.GameBoard[x + 1, y + 1].Type == FieldTypes.Land
                //let's check for the end of the map

                )
                        {
                            isItPlaceable = true;
                        }
                    }
                    break;
            }

            return isItPlaceable;

        }

        private String TempActivePlacementModeMethod()
        {
            if (placementModeRoad)
            {
                return "Gray";
            }
            else if (placementModeResidential)
            {
                return "LightGreen";
            }
            else if (placementModeIndustrial)
            {
                return "LightSkyBlue";
            }
            else if (placementModeCommercial)
            {
                return "Yellow";
            }
            else if (PlacementModePoliceStation)
            {
                return "DarkBlue";
            }
            else
            {
                return "DarkGray";
            }
            //todo: "cannot place red"
            ///note to "cannot place red": you could get a private prop, and you could handle this from VM as you do it with the placement modes, 
            ///maybe you could handle this from the view, and hook it onto hover / enter functions in MainWindow.xaml.cs 

        }

        private void OnNewGame()
        {
            NewGame?.Invoke(this, EventArgs.Empty);
            TableSize = _model.modelgd.GameBoard.Length;
            GenerateTable();
            RefreshTable();
            
            placementModeCommercial = false;
            placementModeDestroy= false;
            placementModeFireStation= false;
            placementModeForest= false;
            placementModeIndustrial= false;
            placementModePoliceStation= false;
            placementModeResidential= false;
            placementModeRoad= false;
            placementModeStadium = false;

        }

        private void OnExitGame()
        {
            _model.ExitGame();
            RefreshTable();
        }

        private void Model_GameAdvanced(object sender, int e)
        {
            RefreshTable();
        }

        public EventHandler? NewGame;
        public EventHandler? ExitGame;
    }
}
