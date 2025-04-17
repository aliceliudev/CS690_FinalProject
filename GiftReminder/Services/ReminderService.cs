using GiftReminder.Models;
using System;
using System.Collections.Generic;
using System.Linq;

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
            try
            {
                var today = DateTime.Today;
                var contacts = _dataService.LoadContacts();

                return contacts
                    .Select(c => (contact: c, days: c.DaysUntilEvent))
                    .Where(x => x.days >= 0 && x.days <= daysAhead)
                    .OrderBy(x => x.days)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking reminders: {ex.Message}");
                return new List<(Contact, int)>();
            }
        }

        public void ValidateReminderDates()
        {
            var contacts = _dataService.LoadContacts();
            Console.WriteLine("\n[DEBUG] Date Validation Report");
            Console.WriteLine("-----------------------------");
            Console.WriteLine($"Today is {DateTime.Today:yyyy-MM-dd}");

            foreach (var contact in contacts)
            {
                Console.WriteLine(
                    $"{contact.Name.PadRight(15)} " +
                    $"Event: {contact.EventDate.PadRight(5)} " +
                    $"Next: {contact.NextOccurrence:yyyy-MM-dd} " +
                    $"(in {contact.DaysUntilEvent.ToString().PadLeft(3)} days) " +
                    $"{(contact.DaysUntilEvent <= 7 ? "⚠️" : "")}");
            }
        }
    }
}