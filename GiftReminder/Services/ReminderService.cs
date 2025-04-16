using GiftReminder.Models;

namespace GiftReminder.Services
{
    public class ReminderService
    {
        private readonly DataService _dataService;

        public ReminderService(DataService dataService)
        {
            _dataService = dataService;
        }

        public List<(Contact contact, int days)> GetUpcomingReminders(int daysAhead)
        {
            var contacts = _dataService.LoadContacts();
            return contacts
                .Where(c => c.DaysUntilEvent >= 0 && c.DaysUntilEvent <= daysAhead)
                .Select(c => (c, c.DaysUntilEvent))
                .OrderBy(x => x.DaysUntilEvent)
                .ToList();
        }
    }
}