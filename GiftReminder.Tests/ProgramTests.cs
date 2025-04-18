using System;
using System.IO;
using Xunit;

namespace GiftReminder.Tests
{
    public class ProgramTests
    {
        [Fact]
        public void Program_Should_Not_Throw_On_Main()
        {
          
            var input = new StringReader("0\n");
            Console.SetIn(input);

            var output = new StringWriter();
            Console.SetOut(output);

        
            var ex = Record.Exception(() => Program.Main(Array.Empty<string>()));
            Assert.Null(ex);
            
        }
    }
}
