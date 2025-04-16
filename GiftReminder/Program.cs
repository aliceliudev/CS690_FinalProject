using GiftReminder.Models;
using GiftReminder.Services;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace GiftReminder
{
    class Program
    {
        private static readonly DataService _dataService = new();
        private static readonly ReminderService _reminderService = new(_dataService);

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            
            // Check reminders on startup
            var upcoming = GetUpcomingReminders(7);
            if (upcoming.Any())
            {
                DisplayReminders(upcoming);
                Console.WriteLine("\nPress any key to continue to main menu...");
                Console.ReadKey();
            }

            ShowMainMenu();
        }

        static void ShowMainMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("🎁 Gift Reminder Console");
                Console.WriteLine("1. Add Contact");
                Console.WriteLine("2. Add Gift");
                Console.WriteLine("3. Check Reminders");
                Console.WriteLine("4. List All Contacts");
                Console.WriteLine("0. Exit");
                Console.Write("\nSelect option: ");

                var input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                        AddContact();
                        break;
                    case "2":
                        AddGift();
                        break;
                    case "3":
                        CheckRemindersMenu();
                        break;
                    case "4":
                        ListContacts();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Invalid option!");
                        Thread.Sleep(500);
                        break;
                }
            }
        }

        static void AddContact()
        {
            Console.Clear();
            Console.WriteLine("➕ Add New Contact");
            Console.WriteLine("------------------");

            var contact = new Contact();

            // Name
            do
            {
                Console.Write("Full Name: ");
                contact.Name = Console.ReadLine() ?? string.Empty;
            } while (!ValidateProperty(contact, nameof(contact.Name)));

            // Date
            do
            {
                Console.Write("Date (MM-DD): ");
                contact.EventDate = Console.ReadLine() ?? string.Empty;
            } while (!ValidateProperty(contact, nameof(contact.EventDate)));

            // Phone
            Console.Write("Phone (optional): ");
            contact.Phone = Console.ReadLine();

            // Occasion Type
            Console.WriteLine("\nOccasion Type:");
            Console.WriteLine("1. Birthday");
            Console.WriteLine("2. Anniversary");
            Console.WriteLine("3. Holiday");
            Console.WriteLine("4. Custom");
            Console.Write("Select [1-4] (default 1): ");
            
            var typeInput = Console.ReadLine();
            contact.OccasionType = typeInput switch
            {
                "2" => OccasionType.Anniversary,
                "3" => OccasionType.Holiday,
                "4" => OccasionType.Custom,
                _ => OccasionType.Birthday
            };

            if (contact.OccasionType != OccasionType.Birthday)
            {
                Console.Write($"{contact.OccasionType} name (optional): ");
                contact.CustomOccasionName = Console.ReadLine();
            }

            // Save
            var contacts = _dataService.LoadContacts();
            contacts.Add(contact);
            _dataService.SaveContacts(contacts);

            Console.WriteLine("\n✅ Contact saved successfully!");
            Thread.Sleep(1500);
        }

        static void AddGift()
        {
            Console.Clear();
            var contacts = _dataService.LoadContacts();
            
            if (!contacts.Any())
            {
                Console.WriteLine("No contacts available. Please add contacts first.");
                Thread.Sleep(1500);
                return;
            }

            Console.WriteLine("🎁 Add Gift");
            Console.WriteLine("-----------");
            Console.WriteLine("Select contact:");

            // Display contacts
            for (int i = 0; i < contacts.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {contacts[i].Name} ({contacts[i].DisplayOccasionName})");
            }

            // Select contact
            int contactIndex;
            do
            {
                Console.Write($"Enter number [1-{contacts.Count}]: ");
            } while (!int.TryParse(Console.ReadLine(), out contactIndex) || contactIndex < 1 || contactIndex > contacts.Count);

            var selectedContact = contacts[contactIndex - 1];

            // Gift details
            var gift = new Gift { ContactName = selectedContact.Name };

            // Description
            do
            {
                Console.Write("\nGift description: ");
                gift.Description = Console.ReadLine() ?? string.Empty;
            } while (string.IsNullOrWhiteSpace(gift.Description));

            // Budget
            Console.Write("Budget (optional): ");
            if (decimal.TryParse(Console.ReadLine(), out decimal budget))
                gift.Budget = budget;

            // Save
            var gifts = _dataService.LoadGifts();
            gifts.Add(gift);
            _dataService.SaveGifts(gifts);

            Console.WriteLine($"\n✅ Gift added for {selectedContact.Name}!");
            Thread.Sleep(1500);
        }

        static void CheckRemindersMenu()
        {
            Console.Clear();
            Console.Write("Check next how many days? (7): ");
            if (!int.TryParse(Console.ReadLine(), out var days) || days <= 0)
                days = 7;

            var reminders = GetUpcomingReminders(days);
            DisplayReminders(reminders);

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        static List<Contact> GetUpcomingReminders(int days)
        {
            return _dataService.LoadContacts()
                .Where(c => c.DaysUntilEvent <= days)
                .OrderBy(c => c.DaysUntilEvent)
                .ToList();
        }

        static void DisplayReminders(List<Contact> contacts)
        {
            Console.Clear();
            Console.WriteLine("🔔 Upcoming Reminders");
            Console.WriteLine("---------------------");

            if (!contacts.Any())
            {
                Console.WriteLine("No upcoming events found.");
                return;
            }

            var gifts = _dataService.LoadGifts();

            foreach (var contact in contacts)
            {
                Console.WriteLine($"\n{contact.Name}'s {contact.DisplayOccasionName}");
                Console.WriteLine($"📅 {contact.NextOccurrence:MMMM d} (in {contact.DaysUntilEvent} days)");
                
                if (!string.IsNullOrEmpty(contact.Phone))
                    Console.WriteLine($"📞 {contact.Phone}");

                var contactGifts = gifts.Where(g => g.ContactName == contact.Name).ToList();
                if (contactGifts.Any())
                {
                    Console.WriteLine("🎁 Planned Gifts:");
                    foreach (var gift in contactGifts)
                    {
                        Console.WriteLine($"- {gift.Description}" + 
                            (gift.Budget.HasValue ? $" (${gift.Budget})" : ""));
                    }
                }
            }
        }

        static void ListContacts()
        {
            Console.Clear();
            var contacts = _dataService.LoadContacts();
            
            Console.WriteLine("📋 All Contacts");
            Console.WriteLine("---------------");
            
            foreach (var contact in contacts)
            {
                Console.WriteLine($"\n{contact.Name}");
                Console.WriteLine($"- {contact.DisplayOccasionName}: {contact.EventDate}");
                Console.WriteLine($"- Phone: {contact.Phone ?? "Not set"}");
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        static bool ValidateProperty(object model, string propertyName, bool showErrors = true)
        {
            var property = model.GetType().GetProperty(propertyName);
            if (property == null) return false;

            var context = new ValidationContext(model) { MemberName = propertyName };
            var results = new List<ValidationResult>();
            var value = property.GetValue(model);
            
            var isValid = Validator.TryValidateProperty(value, context, results);

            if (!isValid && showErrors)
            {
                foreach (var error in results)
                {
                    Console.WriteLine($"❗ {error.ErrorMessage}");
                }
            }

            return isValid;
        }
    }
}