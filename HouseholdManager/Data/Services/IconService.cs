using HouseholdManager.Data.API;
using HouseholdManager.Data.Interfaces;
using HouseholdManager.Models;

namespace HouseholdManager.Data.Services
{
    public class IconService : IRequestIcons
    {
        public async Task<List<Icon>> PopulateIcons()
        {
            IconRequestor req = new IconRequestor();
            return await req.GetIconsFromApi();
        }
    }
}
