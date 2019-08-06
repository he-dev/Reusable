﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Reusable.Data;
using Reusable.Diagnostics;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Extensions;
using Reusable.OmniLog.Nodes;
using Reusable.OmniLog.Rx;
using Reusable.OmniLog.SemanticExtensions;
using Reusable.OmniLog.SemanticExtensions.Nodes;
//using Reusable.OmniLog.Attachments;
using Reusable.Utilities.NLog.LayoutRenderers;

namespace Reusable.Apps.Examples.OmniLog
{
    using Reusable.OmniLog.v2;

    //using v1 = Reusable.OmniLog;
    //using LoggerTransaction = Reusable.OmniLog.LoggerTransaction;
    //using v2 = Reusable.OmniLog.v2;

    public static class Example
    {
        public static void Run()
        {
            SmartPropertiesLayoutRenderer.Register();

            //var fileProvider = new RelativeResourceProvider(new PhysicalFileProvider(), typeof(Demo).Assembly.Location);

            var loggerFactory = new LoggerFactory
            {
                Nodes =
                {
                    new ConstantNode // Adds constant values to each log-entry
                    {
                        { "Environment", "Demo" },
                        { "Product", "Reusable.Apps.Console" }
                    },
                    new StopwatchNode // --> when activated, adds Elapsed to each logEntry
                    {
                        GetValue = elapsed => elapsed.TotalMilliseconds // This is the default and used here for demonstration purposes
                    }, 
                    new ComputableNode
                    {
                        new Reusable.OmniLog.Computables.Timestamp<DateTimeUtc>() // --> adds Timestamp to each logEntry
                    },
                    new LambdaNode(), // --> adds support for logger.Log(log => log...) overload
                    new CorrelationNode(), // --> adds Correlation object to logEntry
                    new SemanticNode(), // --> copies everything from AbstractionBuilder to logEntry; adds #Dump items for objects
                    new DumpNode(), // --> adds Variable & Dump to logEntry for every main property or KeyValuePair with the #Serializable
                    new SerializationNode(), // --> serializes every #Serializable item in logEntry and adds it as #Property
                    new FilterNode(logEntry => true), // Filters log-entries and short-circuits the pipeline when False
                    new RenameNode // --> changes the name of the property
                    {
                        Changes =
                        {
                            {"Correlation", "Scope"},
                            {"Variable", "Identifier"},
                            {"Dump", "Snapshot"},
                        }
                    },
                    new TransactionNode(), // When activated, buffers logEntry until committed
                    new EchoNode // The final node that sends log-entry to the receivers
                    {
                        Rx =
                        {
                            new NLogRx(),
                            new ConsoleRx()
                        },
                    }
                }
            };

            var logger = loggerFactory.CreateLogger("Demo");
            var logger2 = loggerFactory.CreateLogger("Demo");

            if (logger != logger2) throw new Exception();

            var meta = Abstraction.Layer.Service().Meta(new { asdf = 2 });

            logger.Log(Abstraction.Layer.Service().Routine("SemLogTest").Running());
            logger.Log(Abstraction.Layer.Service().Meta(new { Null = (string)null }));

            // Opening outer-scope.
            //using (logger.UseScope(correlationHandle: "Blub").Routine("MyRoutine").CorrelationContext(new { Name = "OuterScope", CustomerId = 123 }).AttachElapsed())
            using (logger.UseScope(correlationHandle: "Blub"))
            using (logger.UseStopwatch())
            {
                // Logging some single business variable and a message.
                logger.Log(Abstraction.Layer.Business().Variable(new { foo = "bar" }), log => log.Message("Hallo variable!"));
                logger.Log(Abstraction.Layer.Database().Counter(new { RowCount = 7 }));
                logger.Log(Abstraction.Layer.Database().Decision("blub").Because("bluby"));

                // Opening inner-scope.
                using (logger.UseScope())
                using (logger.UseStopwatch())
                {
                    //logger.Log(Abstraction.Layer.Service().().Running());

                    //var correlationIds = logger.Scopes().CorrelationIds<string>().ToList();

                    // Logging an entire object in a single line.
                    //var customer = new { FirstName = "John", LastName = "Doe" };
                    var customer = new Person
                    {
                        FirstName = "John",
                        LastName = null,
                        Age = 123.456,
                        DBNullTest = DBNull.Value,
                        GraduationYears = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 },
                        Nicknames = { "Johny", "Doe" }
                    };
                    logger.Log(Abstraction.Layer.Business().Variable(new { customer }));

                    // Logging multiple variables in a single line.
                    var baz = 123;
                    var qux = "quux";

                    //logger.Log(Abstraction.Layer.Service().Composite(new { multiple = new { baz, qux } }));

                    // Logging action results.
                    logger.Log(Abstraction.Layer.Service().Routine("DoSomething").Running());
                    logger.Log(Abstraction.Layer.Service().Routine("DoSomething").Canceled().Because("No connection."));
                    logger.Log(Abstraction.Layer.Service().Routine("DoSomething").Faulted(), new DivideByZeroException("Cannot divide."));
                    logger.Log(Abstraction.Layer.Service().Decision("Don't do this.").Because("Disabled."));
                }
            }

            using (logger.UseScope(correlationHandle: "Transaction"))
            using (logger.UseStopwatch())
            {
                using (var tran = logger.UseTransaction())
                {
                    logger.Information("This message is not logged.");
                }

                using (var tran = logger.UseTransaction())
                {
                    logger.Information("This message is not logged.");
                    //logger.Information("This message overrides the transaction.", LoggerTransaction.Override);
                }

                using (var tran = logger.UseTransaction())
                {
                    logger.Information("This message is delayed.");
                    logger.Information("This message is delayed too.");
                    //logger.Information("This message overrides the transaction as first.", LoggerTransaction.Override);
                    tran.Commit();
                }
            }
        }
    }
}