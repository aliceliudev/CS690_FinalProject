
using System;
using System.Collections.Generic;
using GiftReminder.Models;
using GiftReminder.Services;
using Xunit;

namespace GiftReminder.Tests
{
    
    public class ReminderServiceTests
    {
       
        [Fact]
        public void GetUpcomingReminders_Should_Return_Only_Events_Within_DaysAhead()
        {
            var mockService = new MockDataService(new List<Contact>
            {
                new Contact { Name = "Alice", EventDate = DateTime.Today.AddDays(5).ToString("MM-dd") },
                new Contact { Name = "Bob", EventDate = DateTime.Today.AddDays(40).ToString("MM-dd") }
            });

            var reminderService = new ReminderService(mockService);
            var result = reminderService.GetUpcomingReminders(30);

            Assert.Single(result);
            Assert.Equal("Alice", result[0].contact.Name);
            Assert.True(result[0].days <= 30);
        }

        private class MockDataService : DataService
        {
            private readonly List<Contact> _mockContacts;

            public MockDataService(List<Contact> contacts)
            {
                _mockContacts = contacts;
            }

            public override List<Contact> LoadContacts()
            {
                return _mockContacts;
            }
        }
    }
}
