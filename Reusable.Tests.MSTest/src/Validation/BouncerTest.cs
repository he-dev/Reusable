﻿using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Exceptionize;
using Reusable.Flawless;
using Reusable.Utilities.MSTest;

namespace Reusable.Tests.Validation
{
    [TestClass]
    public class BouncerTest
    {
        [TestMethod]
        public void CanEnsureMultipleRules()
        {
            var validator = ExpressValidator.For<Person>(builder =>
            {
                builder.True(p => p.FirstName != null);
                builder.True(p => p.LastName != null);
            });

            var person = new Person { FirstName = "John", LastName = "Doe" };
            Assert.IsTrue(validator.Validate(person).Success);
        }        

        [TestMethod]
        public void CanBlockMultipleRules()
        {
            var validator = ExpressValidator.For<Person>(builder =>
            {
                builder.False(p => p.FirstName == null);
                builder.False(p => p.LastName == null);
            });

            var person = new Person();
            var weelidationResult = validator.Validate(person);
            Assert.AreEqual(1, weelidationResult.Count);
            Assert.AreEqual(2, weelidationResult.False.Count());
            Assert.IsFalse(weelidationResult.Success);
            
        }

        [TestMethod]
        public void IsValidWhenNull_Null_True()
        {
            var validator = ExpressValidator.For<Person>(model => model.Null());
            var person = default(Person);
            Assert.IsTrue(validator.Validate(person).Success);
        }

        [TestMethod]
        public void IsValidWhenNull_NotNull_False()
        {
            var validator = ExpressValidator.For<Person>(model => model.Null());
            var person = new Person();
            Assert.IsFalse(validator.Validate(person).Success);
        }

        [TestMethod]
        public void IsNotValidWhenNull_NotNull_True()
        {
            var validator = ExpressValidator.For<Person>(model => model.NotNull());
            var person = new Person();
            Assert.IsTrue(validator.Validate(person).Success);
        }

        [TestMethod]
        public void IsNotValidWhenNull_Null_False()
        {
            var validator = ExpressValidator.For<Person>(model => model.NotNull());
            var person = default(Person);
            Assert.IsFalse(validator.Validate(person).Success);
        }

        //[TestMethod]
        //public void ValidateWith_MultipleRules_Validations()
        //{
        //    //var age = 5;
        //    //var lastName = "Doe";

        //    var validator = DuckValidator<Person>.Empty
        //        .IsNotValidWhen(p => p.FirstName == null);

        //    var person = new Person();
        //    var context = person.ValidateWith(validator);

        //    Assert.AreEqual("Not((<Person>.FirstName == null))", context.Results.ElementAt(0).Expression);
        //}

        [TestMethod]
        public void ThrowOrDefault_InvalidPerson_PersonValidationException()
        {
            var validator = ExpressValidator.For<Person>(model => model.False(p => p.FirstName == null));
            var person = new Person();

            Assert.That.Throws<DynamicException>(() => validator.Validate(person).Assert(), filter => filter.When(name: "PersonValidationException"));
        }

        public class Person
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }

            public int Age { get; set; }

            public DateTime DayOfBirth { get; set; }
        }
    }
}