using System.ComponentModel.DataAnnotations;

namespace GiftReminder.Models
{
    public class Contact
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^(0[1-9]|1[0-2])-(0[1-9]|[12][0-9]|3[01])$")]
        public string EventDate { get; set; } = string.Empty;

        public string? Phone { get; set; }

        [Required]
        public OccasionType OccasionType { get; set; } = OccasionType.Birthday;

        public string? CustomOccasionName { get; set; }

        public int DaysUntilEvent
        {
            get
            {
                var today = DateTime.Today;
                var eventDate = DateTime.ParseExact(EventDate, "MM-dd", null);
                var nextDate = new DateTime(today.Year, eventDate.Month, eventDate.Day);
                
                if (nextDate < today) 
                    nextDate = nextDate.AddYears(1);
                
                return (nextDate - today).Days;
            }
        }

        public DateTime NextOccurrence => DateTime.Today.AddDays(DaysUntilEvent);

        public string DisplayOccasionName => 
            OccasionType switch
            {
                OccasionType.Birthday => "Birthday",
                OccasionType.Anniversary when !string.IsNullOrEmpty(CustomOccasionName) 
                    => $"{CustomOccasionName} Anniversary",
                OccasionType.Anniversary => "Anniversary",
                _ => CustomOccasionName ?? OccasionType.ToString()
            };
    }

    public enum OccasionType
    {
        Birthday,
        Anniversary,
        Holiday,
        Custom
    }
}