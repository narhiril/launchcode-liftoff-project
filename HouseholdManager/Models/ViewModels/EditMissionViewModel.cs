using HouseholdManager.Areas.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HouseholdManager.Models.ViewModels
{
    public class EditMissionViewModel
    {
        public EditMissionViewModel() { }

        public EditMissionViewModel(Mission mission)
        {
            Id = mission.Id;
            MemberId = mission.MemberId;
            Name = mission.Name;
            Point = mission.Point;
            DueDate = mission.DueDate;
            RoomId = mission.RoomId;
            Completed = mission.Completed;
        }

        public EditMissionViewModel(int id, Mission mission)
        {
            Id = id;
            MemberId = mission.MemberId;
            Name = mission.Name;
            Point = mission.Point;
            DueDate = mission.DueDate;
            RoomId = mission.RoomId;
            Completed = mission.Completed;
        }

        public int? Id { get; set; }

        [Required(ErrorMessage = "Name of mission is required.")]
        [DisplayName("Mission Name")]
        public string? Name { get; set; }

        [DisplayName("Room")]
        public int? RoomId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Point value must be greater than zero and no more than five.")]
        [DisplayName("Difficulty")]
        public int Point { get; set; }

        [BindProperty, DataType(DataType.Date)]
        [DisplayName("Due Date")]
        [Required]
        public DateTime DueDate { get; set; }

        public bool Completed { get; set; } = false;

        public string? MemberId { get; set; }

    }
}
