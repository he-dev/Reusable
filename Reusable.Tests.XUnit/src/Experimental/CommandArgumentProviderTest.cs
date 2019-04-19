using System.Collections.Generic;
using System.ComponentModel;
using Reusable.Commander;
using Reusable.Commander.Annotations;
using Reusable.Commander.Services;
using Xunit;

namespace Reusable.Tests.XUnit.Experimental
{
    /*
    public class UriSchemaAttribute:Attribute{}
    
    ResourcePrefix("test") // override
    ResourcePrefix(null) // disable
    // inherit by default
    ResourceScheme("setting")
    ResourceName("alias", Convention = TypeMember)
    ResourceProvider("alias")
     
      
     */

    public class CommandArgumentProviderTest
    {
        [Fact]
        public void Blub()
        {
            var commandLine = new CommandLine
            {
                { "files", "first.txt" },
                { "files", "second.txt" },
                { "build", "debug" },
                { "canWrite" },
                { "canBuild" },
            };

            var cmdln = new CommandLineReader<ITestParameter>(new CommandArgumentProvider(commandLine));

            var actualFiles = cmdln.GetItem(x => x.Files);
            var actualBuild = cmdln.GetItem(x => x.Build);
            var canWrite = cmdln.GetItem(x => x.CanWrite);
            var canBuild = cmdln.GetItem(x => x.CanBuild);
            //var async = cmdln.GetItem(x => x.Async);

            Assert.Equal(new[] { "first.txt", "second.txt" }, actualFiles);
            Assert.Equal("debug", actualBuild);
            Assert.Equal(true, canWrite);
            Assert.Equal(true, canBuild);
            //Assert.Equal(true, async);
        }        
        
        internal interface ITestParameter : ICommandParameter
        {
            [Alias("f")]
            List<string> Files { get; }

            string Build { get; }

            bool CanWrite { get; }

            [DefaultValue(true)]
            bool CanBuild { get; }
        }
    }
}