﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Testing;
using Reusable.Testing.Validations;

namespace Reusable.FluentValidation.Tests.Testing.Validations
{
    [TestClass]
    public class ExceptionValidationTests
    {
        [TestMethod]
        public void Throws_True()
        {
            new Action(() => { throw new DivideByZeroException(); }).Verify().Throws<DivideByZeroException>();
        }

        [TestMethod]
        [ExpectedException(typeof(VerificationException))]
        public void Throws_False()
        {
            new Action(() => { throw new DivideByZeroException(); }).Verify().Throws<InvalidOperationException>();
        }
    }
}