using HouseholdManager.Data.API;
using HouseholdManager.Models;

namespace HouseholdManager.Data.Interfaces
{
    public interface IRequestIcons
    {
        public abstract Task<List<Icon>> PopulateIcons();
    }
}
