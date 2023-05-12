using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityBuilder.Model.Events
{
    public class GameOverEventArgs : EventArgs
    {
        private int result;

        public GameOverEventArgs(int result)
        {
            this.result = result;
        }

        public int Result { get { return result; } }
    }
}
