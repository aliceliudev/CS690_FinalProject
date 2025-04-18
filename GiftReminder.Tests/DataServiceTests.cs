
using System;
using System.Collections.Generic;
using System.IO;
using GiftReminder.Models;
using GiftReminder.Services;
using Xunit;

namespace GiftReminder.Tests
{
    public class DataServiceTests
    {
        private readonly string testDataPath = Path.Combine(Path.GetTempPath(), "GiftReminderTestData_" + Guid.NewGuid());
        [Fact]
        public void SaveContacts_Should_Write_And_Read_Back_Correctly()
        {
            if (Directory.Exists(testDataPath))
                Directory.Delete(testDataPath, true);
            Directory.CreateDirectory(testDataPath);

            var service = new DataServiceMock(testDataPath);
            var contacts = new List<Contact>
            {
                new Contact { Name = "Tom", EventDate = "12-25", OccasionType = OccasionType.Holiday }
            };

            service.SaveContacts(contacts);
            var result = service.LoadContacts();

            Assert.Single(result);
            Assert.Equal("Tom", result[0].Name);
        }

        private class DataServiceMock : DataService
        {
            public DataServiceMock(string path)
            {
                var contactsField = typeof(DataService).GetField("_contactsPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var giftsField = typeof(DataService).GetField("_giftsPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var dataField = typeof(DataService).GetField("_dataDir", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                contactsField?.SetValue(this, Path.Combine(path, "contacts.csv"));
                giftsField?.SetValue(this, Path.Combine(path, "gifts.csv"));
                dataField?.SetValue(this, path);

                typeof(DataService).GetMethod("InitializeDataFiles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(this, null);
            }
        }
    }
}
