namespace GiftReminder.Models
{
    public class Gift
    {
        public string ContactName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal? Budget { get; set; }
        public GiftStatus Status { get; set; } = GiftStatus.Planned;
    }

    public enum GiftStatus
    {
        Planned,
        Purchased,
        Wrapped,
        Delivered
    }
}