using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityBuilder.Persistence
{
    public interface ICityBuilderDataAccess
    {
        Task<GameData> LoadAsync(String path);
        Task SaveAsync(String path, GameData table);
    }
}
