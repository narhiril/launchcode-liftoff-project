using System.ComponentModel;

namespace HouseholdManager.Models.ViewModels
{
    public class RoomDetailViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public List<Mission> Missions { get; set; }

        [DisplayName("Dirt-O-Meter")]
        public int DirtLevel { get; set; }


        public RoomDetailViewModel()
        {
            Id = 0;
            Name = "";
            Icon = "";
            Missions = new List<Mission>();
            DirtLevel = 0;
        }

        public RoomDetailViewModel(Room room, List<Mission> missions)
        {
            Id = room.Id;
            Name = room.Name;
            Icon = room.Icon;
            Missions = missions;
            DirtLevel = room.DirtLevel;
        }
    }
}
