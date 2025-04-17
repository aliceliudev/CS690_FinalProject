
using System;
using GiftReminder.Models;
using Xunit;

namespace GiftReminder.Tests
{
    public class ContactTests
    {
        [Fact]
        public void DaysUntilEvent_ShouldBe_Zero_For_Today()
        {
            var today = DateTime.Today;
            var contact = new Contact { EventDate = today.ToString("MM-dd") };
            Assert.Equal(0, contact.DaysUntilEvent);
        }

        [Fact]
        public void DaysUntilEvent_ShouldBe_Correct_For_Future_Date()
        {
            var contact = new Contact { EventDate = DateTime.Today.AddDays(5).ToString("MM-dd") };
            Assert.Equal(5, contact.DaysUntilEvent);
        }

        [Fact]
        public void DisplayOccasionName_Should_Include_Custom_Name()
        {
            var contact = new Contact
            {
                OccasionType = OccasionType.Anniversary,
                CustomOccasionName = "Wedding"
            };
            Assert.Equal("Wedding Anniversary", contact.DisplayOccasionName);
        }
    }
}
