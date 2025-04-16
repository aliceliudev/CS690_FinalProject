using CsvHelper;
using CsvHelper.Configuration;
using GiftReminder.Models;
using System.Globalization;
using System.ComponentModel.DataAnnotations;

namespace GiftReminder.Services
{
    public class DataService
    {
        private readonly string _dataDir = Path.Combine(Directory.GetCurrentDirectory(), "Data");
        private readonly string _contactsPath;
        private readonly string _giftsPath;

        public DataService()
        {
            _contactsPath = Path.Combine(_dataDir, "contacts.csv");
            _giftsPath = Path.Combine(_dataDir, "gifts.csv");
            Directory.CreateDirectory(_dataDir);
        }

        public List<Contact> LoadContacts()
        {
            if (!File.Exists(_contactsPath)) return new List<Contact>();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = "|",
                MissingFieldFound = null,
                HeaderValidated = null
            };

            using var reader = new StreamReader(_contactsPath);
            using var csv = new CsvReader(reader, config);
            csv.Context.RegisterClassMap<ContactMap>();

            try
            {
                return csv.GetRecords<Contact>()
                    .Where(c => Validator.TryValidateObject(c, new ValidationContext(c), null, true))
                    .ToList();
            }
            catch
            {
                CreateBackup();
                return new List<Contact>();
            }
        }

        public void SaveContacts(List<Contact> contacts)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = "|",
                HasHeaderRecord = true
            };

            using var writer = new StreamWriter(_contactsPath);
            using var csv = new CsvWriter(writer, config);
            csv.Context.RegisterClassMap<ContactMap>();
            csv.WriteRecords(contacts);
        }

        public List<Gift> LoadGifts()
        {
            if (!File.Exists(_giftsPath)) return new List<Gift>();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = "|"
            };

            using var reader = new StreamReader(_giftsPath);
            using var csv = new CsvReader(reader, config);
            return csv.GetRecords<Gift>().ToList();
        }

        public void SaveGifts(List<Gift> gifts)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = "|",
                HasHeaderRecord = true
            };

            using var writer = new StreamWriter(_giftsPath);
            using var csv = new CsvWriter(writer, config);
            csv.WriteRecords(gifts);
        }

        public void CreateBackup()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupPath = Path.Combine(_dataDir, $"contacts_backup_{timestamp}.csv");
            
            if (File.Exists(_contactsPath))
                File.Copy(_contactsPath, backupPath);
        }

        private sealed class ContactMap : ClassMap<Contact>
        {
            public ContactMap()
            {
                Map(m => m.Name).Name("Name");
                Map(m => m.EventDate).Name("Birthday");
                Map(m => m.Phone).Name("Phone").Optional();
                Map(m => m.OccasionType).Name("OccasionType").Default(OccasionType.Birthday);
                Map(m => m.CustomOccasionName).Name("CustomName").Optional();
            }
        }
    }
}