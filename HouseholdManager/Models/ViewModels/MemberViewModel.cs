using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace HouseholdManager.Models.ViewModels
{
    public class MemberViewModel
    {
        //This should never be called, but a view model has to have 
        //a parameterless constructor
        public MemberViewModel() 
        {
            MemberId = string.Empty;
            HouseholdId= -1;
        }

        public MemberViewModel(Member member)
        {
            MemberId = member.Id;
            MemberDisplayName = member.DisplayName;
            HouseholdId = member.HouseholdId ?? -1;
            HouseholdName = member.Household?.Name ?? string.Empty;
            Icon = member.Icon;
            HouseholdIcon = member.Household?.Icon ?? string.Empty;
            MemberType = member.MemberType;
        }

        public string MemberId { get; set; }
        public string MemberType { get; set; } = "Member";
        public string Icon { get; set; } = string.Empty;
        public int HouseholdId { get; set; }

        public string HouseholdName { get; set; } = string.Empty;
        public string HouseholdIcon { get; set; } = string.Empty;

        public string MemberDisplayName { get; set; } = string.Empty;

    }
}
