﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.SmartConfig;
using Reusable.SmartConfig.Data;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;

namespace Reusable.Tests.SmartConfig
{
    [TestClass]
    public class FirstSettingFinderTest
    {
        [TestMethod]
        public void TryFindSetting_CanFindSettingByName()
        {
            var provider1 = Mock.Create<ISettingProvider>();
            var provider2 = Mock.Create<ISettingProvider>();
            var provider3 = Mock.Create<ISettingProvider>();

            provider1
                .Arrange(x => x.Read(Arg.IsAny<SettingName>(), Arg.IsAny<Type>(), Arg.IsNull<SettingNameConvention>()))
                .Returns(default(ISetting));

            provider2
                .Arrange(x => x.Read(
                    Arg.Matches<SettingName>(arg => arg == SettingName.Parse("Type.Member")),
                    Arg.IsAny<Type>(),
                    Arg.IsNull<SettingNameConvention>())
                )
                .Returns(Setting.Create("Type.Member", "abc"));

            provider3
                .Arrange(x => x.Read(Arg.IsAny<SettingName>(), Arg.IsAny<Type>(), Arg.IsNull<SettingNameConvention>()))
                .Returns(default(ISetting));

            var settingFinder = new FirstSettingFinder();
            var settingFound = settingFinder.TryFindSetting
            (
                new GetValueQuery(SettingName.Parse("Type.Member"), typeof(string)),
                new[] { provider1, provider2, provider3 },
                out var result
            );

            Assert.IsTrue(settingFound);
            Assert.AreSame(provider2, result.SettingProvider);
            Assert.AreEqual("abc", result.Setting.Value);
        }

        [TestMethod]
        public void TryFindSetting_DoesNotFindNotExistingSetting()
        {
            var provider = Mock.Create<ISettingProvider>();
            provider
                .Arrange(x => x.Read(Arg.IsAny<SettingName>(), Arg.IsAny<Type>(), Arg.IsNull<SettingNameConvention>()))
                .Returns(default(ISetting));

            var settingFinder = new FirstSettingFinder();

            var settingFound = settingFinder.TryFindSetting
            (
                new GetValueQuery(SettingName.Parse("Type.Member"), typeof(string)),
                new[] { provider },
                out var result
            );

            Assert.IsFalse(settingFound);
        }
    }
}