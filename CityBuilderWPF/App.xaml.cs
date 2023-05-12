using CityBuilder.Model;
using CityBuilder.Model.Events;
using CityBuilder.Persistence;
using CityBuilder.Persistence.Buildings;
using CityBuilderWPF.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CityBuilderWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private CityBuilderGameModel model = null!;
        private CityBuilderViewModel viewModel = null!;
        private MainWindow view = null!;
        private DispatcherTimer timer = new DispatcherTimer();
        private int choosenX = -1;
        private int choosenY = -1;

        private void App_Startup(object sender, StartupEventArgs e)
        {
            model = new CityBuilderGameModel(new CityBuilderDataAccess());
            model.GameOver += new EventHandler<GameOverEventArgs>(Model_GameOver);

            viewModel = new CityBuilderViewModel(model);
            viewModel.NewGame += new EventHandler(ViewModel_NewGame);
            viewModel.ExitGame += new EventHandler(ViewModel_ExitGame);

            view = new MainWindow();
            view.DataContext = viewModel;
            view.GameField.MouseDown += new MouseButtonEventHandler(Viewmodel_OnClick);
            view.GameField.MouseMove += new MouseEventHandler(ViewModel_OnHover);
            view.GameField.Loaded += new RoutedEventHandler(View_OnLoaded);
            view.PlacementModeRoadButton.Click += new RoutedEventHandler(Viewmodel_PlacementModeRoadClicked);
            view.PlacementModeResidentialButton.Click += new RoutedEventHandler(Viewmodel_PlacementModeResidentialClicked);
            view.PlacementModeCommercialButton.Click += new RoutedEventHandler(Viewmodel_PlacementModeCommercialClicked);
            view.PlacementModeIndustrialButton.Click += new RoutedEventHandler(Viewmodel_PlacementModeIndustrialClicked);
            view.PlacementModeDestroyButton.Click += new RoutedEventHandler(Viewmodel_PlacementModeDestroyClicked);
            view.PlacementModeFireStationButton.Click += new RoutedEventHandler(Viewmodel_PlacementModeFireStationClicked);
            view.PlacementModePoliceStationButton.Click += new RoutedEventHandler(Viewmodel_PlacementModePoliceStationClicked);
            view.PlacementModeStadiumButton.Click += new RoutedEventHandler(Viewmodel_PlacementModeStadiumClicked);
            view.PlacementModeForestButton.Click += new RoutedEventHandler(Viewmodel_PlacementModeForestClicked);

            view.IncrementTaxByOneButton.Click += new RoutedEventHandler(Viewmodel_IncrementTaxByOneClicked);
            view.DecrementTaxByOneButton.Click += new RoutedEventHandler(Viewmodel_DecrementTaxByOneClicked);
            view.IncrementTaxByPlentyButton.Click += new RoutedEventHandler(Viewmodel_IncrementTaxByPlentyClicked);
            view.DecrementTaxByPlentyButton.Click += new RoutedEventHandler(Viewmodel_DecrementTaxByPlentyClicked);

            view.UpgradeButton.Click += new RoutedEventHandler(View_UpgradeClicked);
            view.DestroyButton.Click += new RoutedEventHandler(View_DestroyClicked);
            view.PlayButton.Click += new RoutedEventHandler(View_PlayGame);
            view.SlowButton.Click += new RoutedEventHandler(View_SlowGame);
            view.FastButton.Click += new RoutedEventHandler(View_FastGame);
            view.Show();

            view.NewGameButton.Click += new RoutedEventHandler(View_NewGameButtonPressed);
            view.LoadGameButton.Click += new RoutedEventHandler(View_LoadGameButtonPressed);
            view.BackToTitleFromLoadGameGridButton.Click += new RoutedEventHandler(View_BackToTitleScreen);
            view.PositiveAreYouSureExitButton.Click += new RoutedEventHandler(View_BackToTitleScreen);
            view.NegativeAreYouSureExitButton.Click += new RoutedEventHandler(View_AreYouSureExitPopup_Dismiss);
            view.ExitGameMenuButton.Click += new RoutedEventHandler(View_AreYouSureExitPopup);
            view.ResumeMenuButton.Click += new RoutedEventHandler(View_ResumeGamePressed);
            view.InGameMenuButton.Click += new RoutedEventHandler(View_CogwheelPressed);
            view.ExitButton.Click += new RoutedEventHandler(ViewModel_ExitGame);
            view.SaveGameMenuButton.Click += new RoutedEventHandler(ViewModel_SaveGame);
            view.LoadGameMenuButton.Click += new RoutedEventHandler(ViewModel_LoadGame); //todo, remove, only for testing functions
            view.LoadGameButton.Click += new RoutedEventHandler(ViewModel_LoadGame);
            view.NegativeAreYouSureConflictButton.Click += new RoutedEventHandler(View_AreYouSureConflictPopup_Dismiss);
            view.PositiveAreYouSureConflictButton.Click += new RoutedEventHandler(View_AreYouSureConlictPopup);
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.75);
            timer.Tick += new EventHandler(Timer_Tick);

            view.GameOverQuitToDesktop_Debt.Click += new RoutedEventHandler(ViewModel_ExitGame);
            view.GameOverQuitToDesktop_Happiness.Click += new RoutedEventHandler(ViewModel_ExitGame);
            view.GameOverBackToTitle_Happiness.Click += new RoutedEventHandler(View_BackToTitleScreen);
            view.GameOverBackToTitle_Debt.Click += new RoutedEventHandler(View_BackToTitleScreen);
        }

        private void Model_GameOver(object? sender, CityBuilder.Model.Events.GameOverEventArgs e)
        {
            if (e.Result == 1)
            {
                view.GameOver_Debt.Visibility = Visibility.Visible;
            }
            else
            {
              view.GameOver_Happiness.Visibility = Visibility.Visible;
            }
        }

        private void View_UpgradeClicked(object sender, RoutedEventArgs e)
        {
            var temp = view.Statwindow.Content.ToString().Split(" ");
            model.UpgradeM(model.modelgd.GameBoard[int.Parse(temp[0]), int.Parse(temp[1])].BuildingID);
            LabelUpdate(int.Parse(temp[0]), int.Parse(temp[1]));
        }

        private void View_DestroyClicked(object sender, RoutedEventArgs e)
        {
            model.DestroyM(choosenX, choosenY);
            LabelUpdate(choosenX, choosenY);
            DrawGameField();
        }
        
        #region Navigation private methods

        private void View_NewGameButtonPressed(object sender, RoutedEventArgs e)
        {
            view.GameGrid.Visibility = Visibility.Visible;
            view.TitleGrid.Visibility = Visibility.Collapsed;

        }
        
        private void View_LoadGameButtonPressed(object sender, RoutedEventArgs e)
        {
            view.LoadGameGrid.Visibility = Visibility.Visible;
            view.TitleGrid.Visibility = Visibility.Collapsed;
        }
        
        private void View_BackToTitleScreen(object sender, RoutedEventArgs e)
        {
            view.GameGrid.Visibility = Visibility.Collapsed;
            view.TitleGrid.Visibility = Visibility.Visible;
            view.LoadGameGrid.Visibility = Visibility.Collapsed;
            view.AreYouSureExitGrid.Visibility = Visibility.Collapsed;
            view.GameOver_Debt.Visibility = Visibility.Collapsed;
            view.GameOver_Happiness.Visibility = Visibility.Collapsed;
        }

        private void View_AreYouSureExitPopup(object sender, RoutedEventArgs e)
        {
            view.InGameMenuGrid.Visibility = Visibility.Collapsed;
            view.AreYouSureExitGrid.Visibility = Visibility.Visible;
        }

        private void View_AreYouSureExitPopup_Dismiss(object sender, RoutedEventArgs e)
        {
            view.AreYouSureExitGrid.Visibility = Visibility.Collapsed;
        }
        private void View_AreYouSureConlictPopup(object sender, RoutedEventArgs e)
        {
            view.AreYouSureConflictGrid.Visibility = Visibility.Collapsed;
        }

        private void View_AreYouSureConflictPopup_Dismiss(object sender, RoutedEventArgs e)
        {
            view.AreYouSureConflictGrid.Visibility = Visibility.Collapsed;
        }

        private void View_ResumeGamePressed(object sender, RoutedEventArgs e)
        {
            view.InGameMenuGrid.Visibility = Visibility.Collapsed;
        }
        
        private void View_CogwheelPressed(object sender, RoutedEventArgs e)
        {
            view.InGameMenuGrid.Visibility = Visibility.Visible;
        }



        #endregion

        private void View_OnLoaded(object sender, RoutedEventArgs e)
        {
            if(view.GameField.Children.Count > 0)
            {
                view.GameField.Children.Clear();
            }else DrawGameField();
        }

        private void DrawGameField()
        {
            int width = 25;
            int height = 25;
            int top;
            int left;
            for (int x = 0; x < 25; x++)
            {
                for (int y = 0; y < 25; y++)
                {
                    Rectangle rec = new Rectangle()
                    {
                        Width = width,
                        Height = height,
                        Stroke = new SolidColorBrush(Colors.Gray),
                        StrokeThickness = 1.5
                    };
                    if (model.modelgd.GameBoard[y, x].Zone == ZoneTypes.None)
                    {

                        switch (model.modelgd.GameBoard[y, x].Type)
                        {
                            case FieldTypes.Land:
                                rec.Fill = new SolidColorBrush(Colors.Green);
                                break;
                            case FieldTypes.Road:
                                rec.Fill = new SolidColorBrush(Colors.Gray);
                                break;
                            case FieldTypes.Bridge:
                                rec.Fill = new SolidColorBrush(Colors.DarkGray);
                                break;
                            case FieldTypes.Water:
                                rec.Fill = new SolidColorBrush(Colors.Blue);
                                break;
                            case FieldTypes.Forest:
                                rec.Fill = new SolidColorBrush(Colors.DarkOliveGreen);
                                break;
                        }
                    }
                    else
                    {
                        switch (model.modelgd.GameBoard[y, x].Zone)
                        {
                            case ZoneTypes.Residential:
                                rec.Fill = new SolidColorBrush(Colors.LightGreen);
                                break;
                            case ZoneTypes.Commercial:
                                rec.Fill = new SolidColorBrush(Colors.Yellow);
                                break;
                            case ZoneTypes.Industrial:
                                rec.Fill = new SolidColorBrush(Colors.LightSkyBlue);
                                break;
                            case ZoneTypes.None:
                                rec.Fill = new SolidColorBrush(Colors.White);
                                break;
                        }
                    }
                    if (model.modelgd.GameBoard[y, x].BuildingID > -1)
                    {
                        switch(model.modelgd.buildings.Find(b => b.Id == model.modelgd.GameBoard[y, x].BuildingID).GetType().Name)
                        {
                            case "House":
                                rec.Fill = new SolidColorBrush(Colors.SaddleBrown);
                                break;
                            case "Commercials":
                                rec.Fill = new SolidColorBrush(Colors.Indigo);
                                break;
                            case "Factory":
                                rec.Fill = new SolidColorBrush(Colors.YellowGreen);
                                break;
                            case "FireStation":
                                rec.Fill = new SolidColorBrush(Colors.DarkRed);
                                break;
                            case "Forest":
                                rec.Fill = new SolidColorBrush(Colors.DarkOliveGreen);
                                break;
                            case "PoliceStation":
                                rec.Fill = new SolidColorBrush(Colors.DarkSlateBlue);
                                break;
                            case "Stadium":
                                rec.Fill = new SolidColorBrush(Colors.HotPink);
                                break;

                        }
                    }
                    left = x * width;
                    top = height * y;
                    rec.MouseEnter += (sender, e) => { rec.Stroke = new SolidColorBrush(Colors.White); };
                    rec.MouseLeave += (sender, e) => { rec.Stroke = new SolidColorBrush(Colors.Gray); };
                    view.GameField.Children.Add(rec);
                    Canvas.SetTop(rec, top);
                    Canvas.SetLeft(rec, left);
                }
            }
        }

        private void LabelUpdate(int X, int Y)
        {
            if (model.modelgd.GameBoard[X, Y].Zone == ZoneTypes.None && model.modelgd.GameBoard[X, Y].BuildingID == -1)
            {
                view.Statwindow.Content = X + " " + Y + " " + model.modelgd.GameBoard[X, Y].Type;
                view.UpgradeButton.Visibility = Visibility.Hidden;
                view.DestroyButton.Visibility = Visibility.Hidden;
                view.BuildingLevel.Visibility = Visibility.Hidden;
                view.BuildingPeople.Visibility = Visibility.Hidden;
                view.BuildingQuality.Visibility = Visibility.Hidden;

            }
            else if(model.modelgd.GameBoard[X, Y].Zone != ZoneTypes.None)
            {
                view.Statwindow.Content = X + " " + Y + " " + model.modelgd.GameBoard[X, Y].Zone;
                view.DestroyButton.Visibility = Visibility.Visible;
                var type = model.modelgd.buildings.Find(i => i.Id == model.modelgd.GameBoard[X, Y].BuildingID);
                if (type is Factory
                    || type is House
                    || type is Commercials
                    )
                {
                    view.BuildingLevel.Visibility = Visibility.Visible;
                    view.BuildingPeople.Visibility = Visibility.Visible;
                    view.BuildingQuality.Visibility = Visibility.Visible;
                    view.BuildingLevel.Content = "Level " + type.Level;
                    view.BuildingPeople.Content = "People: " + type.Occupants + "/" + type.Capacity;
                    view.BuildingQuality.Content = "Quality: " + type.Quality + "%";
                    Debug.WriteLine(type.Level);
                    if (type.Level < 3)
                    {
                        view.UpgradeButton.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        view.UpgradeButton.Visibility = Visibility.Hidden;
                    }
                }

            } else
            {
                view.Statwindow.Content = X + " " + Y + " " + model.modelgd.buildings.Find(b => b.Id == model.modelgd.GameBoard[X, Y].BuildingID).GetType().Name;
                view.DestroyButton.Visibility = Visibility.Visible;
            }
        }
        private void Timer_Tick(object? sender, EventArgs e)
        {
            model.AdvanceTime();
            if (view.GameField.Children.Count > 0)
            {
                view.GameField.Children.Clear();
            }
            DrawGameField();
        }

        private void ViewModel_ExitGame(object? sender, EventArgs e)
        {
            view.Close();
        }
        private void View_PlayGame(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(model.GameSpeed);
            if (timer.IsEnabled)
            {
                timer.Stop();
                model.GameSpeed = 0;
                view.SlowButton.IsEnabled = false;
                view.FastButton.IsEnabled = false;
            }
            else
            {
                model.GameSpeed = 1;
                timer.Start();
                timer.Interval = TimeSpan.FromSeconds(0.75);
                view.SlowButton.IsEnabled = true;
                view.FastButton.IsEnabled = true;
            }
            Debug.WriteLine(model.GameSpeed);
        }
        private void View_FastGame(object sender, RoutedEventArgs e)
        {
            if (timer.IsEnabled)
            {
                if(model.GameSpeed < 3)
                {
                    model.GameSpeed += 1;
                    timer.Interval -= TimeSpan.FromSeconds(0.3);
                }
            }
            Debug.WriteLine(model.GameSpeed);
        }

        private void View_SlowGame(object sender, RoutedEventArgs e)
        {
            if (timer.IsEnabled)
            {
                if (model.GameSpeed > 1)
                {
                    model.GameSpeed -= 1;
                    timer.Interval += TimeSpan.FromSeconds(0.3);
                }
            }
            Debug.WriteLine(model.GameSpeed);
        }

        private void ViewModel_NewGame(object? sender, EventArgs e)
        {
            model.NewGame();
            timer.Start();
        }

        private void ViewModel_LoadGame(object? sender, System.EventArgs e)
        {
            try
            {
                    model.LoadGame("zsatar"); //todo CBD-72

            }
            catch (Exception)
            {
                MessageBox.Show("Loading in failed!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewModel_SaveGame(object? sender, EventArgs e)
        {
            try
            {
                try
                {
                    model.SaveGame("zsatar"); //todo CBD-72
                }
                catch (Exception)
                {
                    MessageBox.Show("Saving game has failed!" + Environment.NewLine + "Wrong path, or library is unwritable", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
            catch
            {
                MessageBox.Show("Saving the file has failed!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }


        private void Viewmodel_OnClick(object? sender, MouseEventArgs e)
        {
            int Y = (int)(e.GetPosition(view.GameField).X * 25 / view.GameField.ActualWidth);
            int X = (int)(e.GetPosition(view.GameField).Y * 25 / view.GameField.ActualHeight);
            choosenX = X;
            choosenY = Y;
            viewModel.OnGameFieldClick(X, Y);
            if (view.GameField.Children.Count > 0)
            {
                view.GameField.Children.Clear();
            }
            LabelUpdate(X,Y);
            DrawGameField();
        }

        private void Viewmodel_PlacementModeRoadClicked(object? sender, EventArgs e)
        {
            viewModel.PlacementModeRoadClicked();
            if (view.GameField.Children.Count > 0)
            {
                view.GameField.Children.Clear();
            }
            DrawGameField();
        }
        private void Viewmodel_PlacementModeIndustrialClicked(object? sender, EventArgs e)
        {
            viewModel.PlacementModeIndustrialClicked();
            if (view.GameField.Children.Count > 0)
            {
                view.GameField.Children.Clear();
            }
            DrawGameField();
        }
        private void Viewmodel_PlacementModeCommercialClicked(object? sender, EventArgs e)
        {
            viewModel.PlacementModeCommercialClicked();
            if (view.GameField.Children.Count > 0)
            {
                view.GameField.Children.Clear();
            }
            DrawGameField();
        }
        private void Viewmodel_PlacementModeResidentialClicked(object? sender, EventArgs e)
        {
            viewModel.PlacementModeResidentialClicked();
            if (view.GameField.Children.Count > 0)
            {
                view.GameField.Children.Clear();
            }
            DrawGameField();
        }
        private void Viewmodel_PlacementModeDestroyClicked(object? sender, EventArgs e)
        {
            viewModel.PlacementModeDestroyClicked();
        }
        private void Viewmodel_PlacementModeFireStationClicked(object? sender, EventArgs e)
        {
            viewModel.PlacementModeFireStationClicked();
        }
        private void Viewmodel_PlacementModePoliceStationClicked(object? sender, EventArgs e)
        {
            viewModel.PlacementModePoliceStationClicked();
        }
        private void Viewmodel_PlacementModeStadiumClicked(object? sender, EventArgs e)
        {
            viewModel.PlacementModeStadiumClicked();
        }
        private void Viewmodel_PlacementModeForestClicked(object? sender, EventArgs e)
        {
            viewModel.PlacementModeForestClicked();
        }

        private void Viewmodel_IncrementTaxByOneClicked(object? sender, EventArgs e)
        {
            viewModel.IncrementTax(1);
        }

        private void Viewmodel_IncrementTaxByPlentyClicked(object? sender, EventArgs e)
        {
            viewModel.IncrementTax(5);
        }

        private void Viewmodel_DecrementTaxByOneClicked(object? sender, EventArgs e)
        {
            viewModel.DecrementTax(1);
        }

        private void Viewmodel_DecrementTaxByPlentyClicked(object? sender, EventArgs e)
        {
            viewModel.DecrementTax(5);
        }

        private void ViewModel_OnHover(object? sender, MouseEventArgs e)
        {
            int Y = (int)(e.GetPosition(view.GameField).X * 25 / view.GameField.ActualWidth);
            int X = (int)(e.GetPosition(view.GameField).Y * 25 / view.GameField.ActualHeight);
            
        }
    }
}
