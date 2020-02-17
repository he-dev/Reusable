using System;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using Reusable.Beaver.Policies;
using Reusable.Translucent;
using Reusable.Utilities.JsonNet;
using Xunit;

namespace Reusable.Beaver
{
    public class FeatureToggleTest
    {
        //[Fact]
        public void asdf()
        {
            // var json = TestHelper.Resources.ReadTextFile("Features.json");
            // var c = FeatureConfiguration.FromJson(json);
            // var t = FeatureToggle.FromConfiguration(c);
        }

        [Fact]
        public void Invokes_main_when_enabled()
        {
            var t = new FeatureController(new FeatureToggle(FeaturePolicy.AlwaysOff));
            t.AddOrUpdate("test", FeaturePolicy.AlwaysOn);


            var a = 0;
            var b = 0;

            var c = t.Use("test", () => ++a, () => ++b);
            var d = t.Use("test", () => ++a, () => ++b);

            Assert.Equal(2, a);
            Assert.Equal(0, b);
            Assert.Equal(1, c);
            Assert.Equal(2, d);

            Assert.Equal(FeatureState.Enabled, c.State);
            Assert.IsType<AlwaysOn>(c.Feature.Policy);
            Assert.Equal("test", c.Feature.Name);
        }

        [Fact]
        public void Invokes_fallback_when_disabled()
        {
            var t = new FeatureController(new FeatureToggle(FeaturePolicy.AlwaysOff));
            var a = 0;
            var b = 0;
            var c = t.Use("test", () => ++a, () => ++b);
            var d = t.Use("test", () => ++a, () => ++b);
            Assert.Equal(0, a);
            Assert.Equal(2, b);
            Assert.Equal(1, c);
            Assert.Equal(2, d);
            Assert.Equal(FeatureState.Disabled, c.State);
            Assert.IsType<AlwaysOff>(c.Feature.Policy);
            Assert.Equal("test@Fallback", c.Feature.Name);
        }

        [Fact]
        public void Once_disables_itself_after_first_invoke()
        {
            var t = new FeatureController(new FeatureToggle(FeaturePolicy.AlwaysOff));
            t.AddOrUpdate("test", FeaturePolicy.Once);
            var a = 0;
            var b = 0;
            var c = t.Use("test", () => ++a, () => ++b);
            var d = t.Use("test", () => ++a, () => ++b);
            Assert.Equal(1, a);
            Assert.Equal(1, b);
            Assert.Equal(1, c);
            Assert.Equal(1, d);
            Assert.Equal(FeatureState.Enabled, c.State);
            Assert.Equal(FeatureState.Disabled, d.State);
            Assert.IsType<Once>(c.Feature.Policy);
            Assert.IsType<AlwaysOff>(d.Feature.Policy);
            Assert.Equal("test", c.Feature.Name);
        }

        [Fact]
        public void Ask_requests_permission_to_invoke()
        {
            var q = 0;
            var t = new FeatureController(new FeatureToggle(FeaturePolicy.AlwaysOff));
            t.AddOrUpdate("test", FeaturePolicy.Ask(_ => q++ < 1));

            var m = 0;
            var f = 0;
            var a = t.Use("test", () => ++m, () => ++f);
            var b = t.Use("test", () => ++m, () => ++f);
            Assert.Equal(2, q);
            Assert.Equal(1, m);
            Assert.Equal(1, f);
            Assert.Equal(1, a);
            Assert.Equal(1, b);
            Assert.Equal(FeatureState.Enabled, a.State);
            Assert.Equal(FeatureState.Disabled, b.State);
            Assert.IsType<Ask>(a.Feature.Policy);
            Assert.IsType<Ask>(b.Feature.Policy);
            Assert.Equal("test", a.Feature.Name);
            Assert.Equal("test", b.Feature.Name);
        }

        [Fact]
        public void Throws_when_modifying_locked_feature()
        {
            var t = new FeatureToggle(FeaturePolicy.AlwaysOff);
            t.AddOrUpdate("test", FeaturePolicy.AlwaysOn.Lock());
            //t.SetOrUpdate("test", FeaturePolicy.AlwaysOff);
            Assert.Throws<InvalidOperationException>(() => t["test"].Policy = FeaturePolicy.AlwaysOff);
        }
    }
}