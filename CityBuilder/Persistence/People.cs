using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityBuilder.Persistence
{
    public class People
    {
        private int _homeId;
        //private int _liveRow;
        //private int _liveColumn;
        private int _workplaceId;
        //private int _workRow;
        //private int _workColumn;
        private int _happiness;

        public People(int homeId, int workplaceId)
        {
            _homeId = homeId;
            _workplaceId = workplaceId;
        }

        public int HomeId { get { return _homeId; } set { _homeId = value; } }
        public int WorkplaceId { get { return _workplaceId; } set { _workplaceId = value; } }
        public int Happiness { get { return _happiness; } set { _happiness = value; } }
    }
}
