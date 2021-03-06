﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Exceptionize;
using Reusable.Utilities.MSTest;

namespace Reusable.Tests.Reflection
{
    [TestClass]
    public class DynamicExceptionTest
    {
        [TestMethod]
        public void Create_CanCreateNamedExceptionWithMessage()
        {
            Assert.That.Throws<DynamicException>(
                () => throw DynamicException.Create("TestException", "This is a test."),
                filter => filter.When(name: "^TestException$", message: @"^This is a test\.$")
            );
        }
        
        [TestMethod]
        public void Create_CanAddExceptionSuffixToName()
        {
            Assert.That.Throws<DynamicException>(
                () => throw DynamicException.Create("Test", "This is a test."),
                filter => filter.When(name: "^TestException$", message: @"^This is a test\.$")
            );
        }        
    }
}
