
using CsvHelper;
using CsvHelper.Configuration;
using GiftReminder.Models;
using System.Globalization;
using System.ComponentModel.DataAnnotations;

namespace GiftReminder.Services
{
    public class DataService
    {
        private readonly string _dataDir;
        private readonly string _contactsPath;
        private readonly string _giftsPath;
        private const string ContactsHeader = "Name|Date|Phone|OccasionType|CustomOccasionName";
        private const string GiftsHeader = "ContactName|Description|Budget|Status";

        public DataService()
        {
            _dataDir = Path.Combine(Directory.GetCurrentDirectory(), "Data");
            Directory.CreateDirectory(_dataDir);
            _contactsPath = Path.Combine(_dataDir, "contacts.csv");
            _giftsPath = Path.Combine(_dataDir, "gifts.csv");
            InitializeDataFiles();
        }

        private void InitializeDataFiles()
        {
            if (!File.Exists(_contactsPath))
                File.WriteAllText(_contactsPath, ContactsHeader + Environment.NewLine);
            if (!File.Exists(_giftsPath))
                File.WriteAllText(_giftsPath, GiftsHeader + Environment.NewLine);
        }

        public virtual List<Contact> LoadContacts()
        {
            try
            {
                if (new FileInfo(_contactsPath).Length == 0)
                    return new List<Contact>();

                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = "|",
                    MissingFieldFound = null,
                    HeaderValidated = null,
                    BadDataFound = context =>
                    {
                        var rowNumber = context.Context.Parser?.Row.ToString() ?? "unknown";
                        Console.WriteLine($"Bad data in row {rowNumber}: {context.RawRecord}");
                    }
                };

                using var reader = new StreamReader(_contactsPath);
                using var csv = new CsvReader(reader, config);
                csv.Context.RegisterClassMap<ContactMap>();

                return csv.GetRecords<Contact>()
                    .Where(contact => Validator.TryValidateObject(contact,
                        new ValidationContext(contact), null, true))
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error loading contacts: {ex.Message}");
                CreateBackup();
                return new List<Contact>();
            }
        }

        public void SaveContacts(List<Contact> contacts)
        {
            try
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = "|",
                    HasHeaderRecord = true
                };

                var tempPath = Path.GetTempFileName();
                using (var writer = new StreamWriter(tempPath))
                using (var csv = new CsvWriter(writer, config))
                {
                    csv.Context.RegisterClassMap<ContactMap>();
                    csv.WriteRecords(contacts);
                }

                if (File.Exists(_contactsPath))
                    File.Delete(_contactsPath);

                File.Move(tempPath, _contactsPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error saving contacts: {ex.Message}");
                CreateBackup();
                throw;
            }
        }

        public List<Gift> LoadGifts()
        {
            try
            {
                if (new FileInfo(_giftsPath).Length == 0)
                    return new List<Gift>();

                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = "|",
                    MissingFieldFound = null,
                    HeaderValidated = null
                };

                using var reader = new StreamReader(_giftsPath);
                using var csv = new CsvReader(reader, config);
                return csv.GetRecords<Gift>()
                    .Where(g => !string.IsNullOrWhiteSpace(g.ContactName))
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error loading gifts: {ex.Message}");
                return new List<Gift>();
            }
        }

        public void SaveGifts(List<Gift> gifts)
        {
            try
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = "|",
                    HasHeaderRecord = true
                };

                var tempPath = Path.GetTempFileName();
                using (var writer = new StreamWriter(tempPath))
                using (var csv = new CsvWriter(writer, config))
                {
                    csv.WriteRecords(gifts);
                }

                if (File.Exists(_giftsPath))
                    File.Delete(_giftsPath);

                File.Move(tempPath, _giftsPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error saving gifts: {ex.Message}");
                throw;
            }
        }

        public void CreateBackup()
        {
            try
            {
                var backupDir = Path.Combine(_dataDir, "Backups");
                Directory.CreateDirectory(backupDir);
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupPath = Path.Combine(backupDir, $"contacts_backup_{timestamp}.csv");

                if (File.Exists(_contactsPath))
                {
                    File.Copy(_contactsPath, backupPath);
                    Console.WriteLine($"✅ Created backup: {backupPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Backup failed: {ex.Message}");
            }
        }

        private sealed class ContactMap : ClassMap<Contact>
        {
            public ContactMap()
            {
                Map(m => m.Name).Name("Name");
                Map(m => m.EventDate).Name("Date").TypeConverter(new CustomDateConverter());
                Map(m => m.Phone).Name("Phone").Optional();
                Map(m => m.OccasionType).Name("OccasionType")
                .TypeConverterOption.EnumIgnoreCase()
                .Default(OccasionType.Custom);
                Map(m => m.CustomOccasionName).Name("CustomOccasionName").Optional();
            }
        }
    }

    public class CustomDateConverter : CsvHelper.TypeConversion.ITypeConverter
    {
        public object? ConvertFromString(string? text, CsvHelper.IReaderRow row, CsvHelper.Configuration.MemberMapData memberMapData)
        {
            if (string.IsNullOrWhiteSpace(text)) return "01-01";
            return text.Trim();
        }

        public string? ConvertToString(object? value, CsvHelper.IWriterRow row, CsvHelper.Configuration.MemberMapData memberMapData)
        {
            return value?.ToString() ?? "01-01";
        }
    }
}
