﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.FluentValidation;
using Reusable.FluentValidation.Validations;

namespace SmartUtilities.Tests.Unit.Frameworks.InlineValidation.Validations
{
    [TestClass]
    public class BooleanValidationTests
    {
        [TestMethod]
        public void IsTrue_True()
        {
            "foo".Validate().IsTrue(x => x.Length == 3);
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void IsTrue_False()
        {
            "foo".Validate().IsTrue(x => x.Length == 1);
        }

        [TestMethod]
        public void IsFalse_False()
        {
            "foo".Validate().IsFalse(x => x.Length == 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void IsFalse_True()
        {
            "foo".Validate().IsFalse(x => x.Length == 3);
        }
    }
}
