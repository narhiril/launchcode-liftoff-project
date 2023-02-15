using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace HouseholdManager.Models
{
    public class Room
    {
        [Key]
        public int Id { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public string Name { get; set; }
        
        [Column(TypeName = "nvarchar(20)")]
        public string Icon { get; set; } = "";

        public ICollection<Mission> Missions { get; set; }

        public Household Household { get; set; }
        public int HouseholdId { get; set; }

        [Range(0, 10, ErrorMessage = "Dirt level must be between 0 and 10.")]
        [DisplayName("Dirt-O-Meter")]
        public int DirtLevel { get; set; } = 0;
    }
}
