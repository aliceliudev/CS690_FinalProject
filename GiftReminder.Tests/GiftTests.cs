
using GiftReminder.Models;
using Xunit;

namespace GiftReminder.Tests
{
    public class GiftTests
    {
        [Fact]
        public void Gift_Should_Hold_Data_Correctly()
        {
            var gift = new Gift
            {
                ContactName = "John",
                Description = "Watch",
                Budget = 100,
                Status = GiftStatus.Planned
            };

            Assert.Equal("John", gift.ContactName);
            Assert.Equal("Watch", gift.Description);
            Assert.Equal(100, gift.Budget);
            Assert.Equal(GiftStatus.Planned, gift.Status);
        }
    }
}
