using CityBuilder.Persistence;
using CityBuilderWPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityBuilderWPF.ViewModel
{
    public class Field : ViewModelBase
    {
        private string _color = null;
      //  private DelegateCommand _stepCommand;
        
     //   public DelegateCommand? StepCommand { 
     //       get {return _stepCommand; } 
     //       set {_stepCommand= value; } 
     //   }

        public Int32 X { get; set; }
        public Int32 Y { get; set; }

        public string Color
        {
            get { return _color; }
            set
            {
                if (_color != value)
                {
                    _color = value;
                    OnPropertyChanged();
                }
            }
        }
       
        public Field(int row, int column, FieldTypes _type, ZoneTypes _zone/*, DelegateCommand stepCommand*/)
        {
            if (_zone == ZoneTypes.None)
            {
                switch (_type)
                {
                    case FieldTypes.Land:
                        Color = "Green";
                        break;
                    case FieldTypes.Road:
                        Color = "Gray";
                        break;
                    case FieldTypes.Bridge:
                        Color = "Yellow";
                        break;
                    case FieldTypes.Water:
                        Color = "Blue";
                        break;
                }
            }
            else
            {
                switch (_zone)
                {
                    case ZoneTypes.Residential:
                        Color = "LightGreen";
                        break;
                    case ZoneTypes.Commercial:
                        Color = "Yellow";
                        break;
                    case ZoneTypes.Industrial:
                        Color = "LightSkyBlue";
                        break;
                    case ZoneTypes.None:
                        Color = "";
                        break;
                }
            }
            
            X = row;
            Y = column;

        }
    }
}
