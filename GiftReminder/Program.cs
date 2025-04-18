using GiftReminder.Models;
using GiftReminder.Services;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace GiftReminder
{
    public class Program
    {  
        public static bool IsTesting = false;
        private static readonly DataService _dataService = new();
        private static readonly ReminderService _reminderService = new(_dataService);

       public static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            
            // Check reminders on startup
            var reminders = _reminderService.GetUpcomingReminders(7);
            if (reminders.Any())
            {
                DisplayReminders(reminders);
                Console.WriteLine("\nPress any key to continue to main menu...");
                Console.ReadKey();
            }

            ShowMainMenu();
        }

        public static void ShowMainMenu()
        {  if (IsTesting)
                return;
            while (true)
            {
                TryClear();
                Console.WriteLine("🎁 Gift Reminder Console");
                Console.WriteLine("1. Add Contact");
                Console.WriteLine("2. Add Gift");
                Console.WriteLine("3. Check Reminders");
                Console.WriteLine("4. List All Contacts");
                Console.WriteLine("5. Debug Date Report");
                Console.WriteLine("0. Exit");
                Console.Write("\nSelect option: ");

                switch (Console.ReadLine())
                {
                    case "1": AddContact(); break;
                    case "2": AddGift(); break;
                    case "3": CheckRemindersMenu(); break;
                    case "4": ListContacts(); break;
                    case "5": _reminderService.ValidateReminderDates();
                              Console.WriteLine("\nPress any key to continue...");
                              Console.ReadKey();
                              break;
                    case "0": return;
                    default: Console.WriteLine("Invalid option!"); 
                             Thread.Sleep(500); 
                             break;
                }
            }
        }
    public static void TryClear()
        {
            try { Console.Clear(); }
            catch { /* ignore in test or non-console environments */ }
        }

        static void AddContact()
        {
            TryClear();
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
            TryClear();
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
            TryClear();
            Console.Write("Check next how many days? (7): ");
            if (!int.TryParse(Console.ReadLine(), out var days) || days <= 0)
                days = 7;

            var reminders = _reminderService.GetUpcomingReminders(days);
            DisplayReminders(reminders);

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        static void DisplayReminders(List<(Contact contact, int days)> reminders)
        {
            TryClear();
            Console.WriteLine("🔔 Upcoming Reminders");
            Console.WriteLine("---------------------");
            Console.WriteLine($"Today is {DateTime.Today:MMMM d, yyyy}\n");

            if (!reminders.Any())
            {
                Console.WriteLine("No upcoming events found.");
                return;
            }

            var gifts = _dataService.LoadGifts();

            foreach (var (contact, days) in reminders)
            {
                Console.WriteLine($"\n{contact.Name}'s {contact.DisplayOccasionName}");
                Console.WriteLine($"📅 Date: {contact.NextOccurrence:MMMM d} (in {days} days)");
                
                if (!string.IsNullOrEmpty(contact.Phone))
                    Console.WriteLine($"📞 Contact: {contact.Phone}");

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
            TryClear();
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