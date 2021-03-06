﻿using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Commander;
using Reusable.Exceptionizer;
using Reusable.Reflection;
using Reusable.Utilities.MSTest;

namespace Reusable.Tests.Commander.Integration
{
    using static Helper;

    [TestClass]
    public class ExceptionIntegrationTest
    {
        // It's no longer possible to register any type so this test is irrelevant now.
        // [TestMethod]
        // public void ctor_CommanderModule_ctor_InvalidCommandType_Throws()
        // {
        //     Assert.That.Throws<DynamicException>(
        //         () => new CommanderModule(new[] {typeof(string)}),
        //         filter => filter.WhenName("CommandTypeException")
        //     );
        // }

        [TestMethod]
        public void DisallowDuplicateCommandNames()
        {
            Assert.That.Throws<DynamicException>(
                () =>
                {
                    var bags = new BagTracker();
                    using (CreateContext(
                        commands => commands
                            .Add("c", Track<SimpleBag>(bags))
                            .Add("c", Track<SimpleBag>(bags))
                    ))
                    {
                    }
                },
                filter => filter.When(name: "^RegisterCommand"),
                inner => inner.When(name: "^DuplicateCommandName")
            );
        }

        [TestMethod]
        public void DisallowDuplicateParameterNames()
        {
            Assert.That.Throws<DynamicException>(
                () =>
                {
                    var bags = new BagTracker();
                    using (CreateContext(
                        commands => commands
                            .Add("c", Track<BagWithDuplicateParameter>(bags))
                    ))
                    {
                    }
                },
                filter => filter.When(name: "^RegisterCommand"),
                inner => inner.When(name: "^DuplicateParameterName")
            );
        }

        [TestMethod]
        public void DisallowNonSequentialParameterPosition()
        {
            Assert.That.Throws<DynamicException>(
                () =>
                {
                    var bags = new BagTracker();
                    using (CreateContext(
                        commands => commands
                            .Add("c", Track<BagWithInvalidParameterPosition>(bags))
                    ))
                    {
                    }
                },
                filter => filter.When(name: "^RegisterCommand"),
                inner => inner.When(name: "^ParameterPositionException")
            );
        }

        [TestMethod]
        public void DisallowUnsupportedParameterType()
        {
            Assert.That.Throws<DynamicException>(
                () =>
                {
                    var bags = new BagTracker();
                    using (CreateContext(
                        commands => commands
                            .Add("c", Track<BagWithUnsupportedParameterType>(bags))
                    ))
                    {
                    }
                },
                filter => filter.When(name: "^RegisterCommand"),
                inner => inner.When(name: "^UnsupportedParameterTypeException")
            );
        }

        [TestMethod]
        public void DisallowCommandLineWithoutCommandName()
        {
            Assert.That.Throws<DynamicException>(
                () =>
                {
                    var bags = new BagTracker();
                    using (var context = CreateContext(
                        commands => commands
                            .Add("c", Track<SimpleBag>(bags))
                    ))
                    {
                        context.Executor.ExecuteAsync<object>("-a", default).GetAwaiter().GetResult();
                    }
                },
                filter => filter.When(name: "^InvalidCommandLine")
            );
        }

        [TestMethod]
        public void DisallowNonExistingCommandName()
        {
            Assert.That.Throws<DynamicException>(
                () =>
                {
                    var bags = new BagTracker();
                    using (var context = CreateContext(
                        commands => commands
                            .Add("c", Track<SimpleBag>(bags))
                    ))
                    {
                        context.Executor.ExecuteAsync<object>("b", default).GetAwaiter().GetResult();
                    }
                },
                filter => filter.When(name: "^InvalidCommandLine")
            );
        }

        [TestMethod]
        public void Throws_when_required_parameter_not_specified()
        {
            var exception = default(Exception);
            
            using (var context = CreateContext(
                commands => commands
                    .Add("c", ExecuteNoop<BagWithRequiredValue>()), inner => { exception = inner; }
            ))
            {
                Assert.ThrowsException<TaskCanceledException>(() => context.Executor.ExecuteAsync<object>("c", default).GetAwaiter().GetResult());
                Assert.IsNotNull(exception);
            }
        }

        [TestMethod]
        public void DisallowCommandLineWithMissingPositionalParameter()
        {
            Assert.That.Throws<DynamicException>(
                () =>
                {
                    using (var context = CreateContext(
                        commands => commands.Add("c", ExecuteNoop<BagWithPositionalValues>()),
                        (ExecuteExceptionCallback)(ex => throw ex)
                    ))
                    {
                        context.Executor.ExecuteAsync<object>("c 3", default).GetAwaiter().GetResult();
                    }
                }
                //filter => filter.When(name: "^ParameterMapping")
            );
        }
    }
}