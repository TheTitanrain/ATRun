using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ATRun.Tests
{
    public class AutorunEntryTests
    {
        [Fact]
        public void Constructor_SetsAllProperties()
        {
            var entry = new AutorunEntry("MyApp", @"C:\app.exe", AutorunHive.CurrentUser);

            Assert.Equal("MyApp", entry.Name);
            Assert.Equal(@"C:\app.exe", entry.FullPath);
            Assert.Equal(AutorunHive.CurrentUser, entry.Hive);
        }

        [Fact]
        public void Record_Equality_SameValues_AreEqual()
        {
            var a = new AutorunEntry("MyApp", @"C:\app.exe", AutorunHive.CurrentUser);
            var b = new AutorunEntry("MyApp", @"C:\app.exe", AutorunHive.CurrentUser);

            Assert.Equal(a, b);
            Assert.True(a == b);
        }

        [Fact]
        public void Record_Equality_DifferentHive_NotEqual()
        {
            var a = new AutorunEntry("MyApp", @"C:\app.exe", AutorunHive.CurrentUser);
            var b = new AutorunEntry("MyApp", @"C:\app.exe", AutorunHive.LocalMachine);

            Assert.NotEqual(a, b);
        }

        [Fact]
        public void Record_Equality_DifferentPath_NotEqual()
        {
            var a = new AutorunEntry("MyApp", @"C:\app.exe", AutorunHive.CurrentUser);
            var b = new AutorunEntry("MyApp", @"C:\other.exe", AutorunHive.CurrentUser);

            Assert.NotEqual(a, b);
        }

        [Fact]
        public void Record_Equality_DifferentName_NotEqual()
        {
            var a = new AutorunEntry("MyApp", @"C:\app.exe", AutorunHive.CurrentUser);
            var b = new AutorunEntry("OtherApp", @"C:\app.exe", AutorunHive.CurrentUser);

            Assert.NotEqual(a, b);
        }

        [Fact]
        public void With_OverridesHive_PreservesOtherFields()
        {
            var original = new AutorunEntry("MyApp", @"C:\app.exe", AutorunHive.CurrentUser);
            var modified = original with { Hive = AutorunHive.LocalMachine };

            Assert.Equal("MyApp", modified.Name);
            Assert.Equal(@"C:\app.exe", modified.FullPath);
            Assert.Equal(AutorunHive.LocalMachine, modified.Hive);
        }

        [Fact]
        public void With_OverridesPath_PreservesOtherFields()
        {
            var original = new AutorunEntry("MyApp", @"C:\app.exe", AutorunHive.CurrentUser);
            var modified = original with { FullPath = @"C:\new.exe" };

            Assert.Equal("MyApp", modified.Name);
            Assert.Equal(@"C:\new.exe", modified.FullPath);
            Assert.Equal(AutorunHive.CurrentUser, modified.Hive);
        }

        [Fact]
        public void SortByName_CaseInsensitive_SortsCorrectly()
        {
            var entries = new List<AutorunEntry>
            {
                new("zoom",  @"C:\zoom.exe",  AutorunHive.CurrentUser),
                new("Alpha", @"C:\alpha.exe", AutorunHive.CurrentUser),
                new("beta",  @"C:\beta.exe",  AutorunHive.CurrentUser),
            };

            var sorted = entries.OrderBy(e => e.Name, StringComparer.CurrentCultureIgnoreCase).ToList();

            Assert.Equal("Alpha", sorted[0].Name);
            Assert.Equal("beta",  sorted[1].Name);
            Assert.Equal("zoom",  sorted[2].Name);
        }

        [Fact]
        public void SortByName_MixedHives_SortsIgnoringHive()
        {
            var entries = new List<AutorunEntry>
            {
                new("Zoom",  @"C:\zoom.exe",  AutorunHive.LocalMachine),
                new("Alpha", @"C:\alpha.exe", AutorunHive.CurrentUser),
                new("Beta",  @"C:\beta.exe",  AutorunHive.LocalMachine),
            };

            var sorted = entries.OrderBy(e => e.Name, StringComparer.CurrentCultureIgnoreCase).ToList();

            Assert.Equal("Alpha", sorted[0].Name);
            Assert.Equal("Beta",  sorted[1].Name);
            Assert.Equal("Zoom",  sorted[2].Name);
        }

        [Fact]
        public void SortByName_EmptyList_ReturnsEmpty()
        {
            var entries = new List<AutorunEntry>();

            var sorted = entries.OrderBy(e => e.Name, StringComparer.CurrentCultureIgnoreCase).ToList();

            Assert.Empty(sorted);
        }
    }
}
