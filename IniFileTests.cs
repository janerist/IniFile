using System.IO;
using System.Linq;
using NUnit.Framework;

namespace YourNamespace
{
    [TestFixture]
    public class IniFileTests
    {
        [Test]
        public void TestLoad()
        {
            var iniFile = new IniFile(new StringReader("[Foo]\nbar=1\nbaz=qux\n[Lol]\nrofl = copter"));
            Assert.That(iniFile.Section("Foo").Get("bar"), Is.EqualTo("1"));
            Assert.That(iniFile.Section("Foo").Get("baz"), Is.EqualTo("qux"));

            Assert.That(iniFile.Section("Lol").Get("rofl"), Is.EqualTo("copter"));
        }

        [Test]
        public void TestCreatesSection()
        {
            var iniFile = new IniFile(new StringReader("[Foo]\nbar=1\nbaz=qux"));
            var category = iniFile.Section("foo");
            Assert.That(category, Is.Not.Null);
            Assert.That(category.Count, Is.EqualTo(0));
        }

        [Test]
        public void TestKeyNotFound()
        {
            var iniFile = new IniFile(new StringReader("[Foo]\nbar=1\nbaz=qux"));
            Assert.That(iniFile.Section("Foo").Get("nope"), Is.Null);
        }

        [Test]
        public void TestOverwrites()
        {
            var iniFile = new IniFile(new StringReader("[Foo]\ndupe=1\ndupe=qux"));
            Assert.That(iniFile.Section("Foo").Get("dupe"), Is.EqualTo("qux"));
        }

        [Test]
        public void TestSkipsComments()
        {
            var iniFile = new IniFile(new StringReader("[Foo]\n;comment\nbar=1\nbaz=qux"));
            Assert.That(iniFile.Count(), Is.EqualTo(1));
            Assert.That(iniFile.First().Count, Is.EqualTo(2));
        }

        [Test]
        public void TestMergesSectionsWithSameName()
        {
            var iniFile = new IniFile(new StringReader("[Foo]\nbar=1\nbaz=qux\n[Foo]\nyes=true\n[Foo2]\nno=false"));
            Assert.That(iniFile.Count(), Is.EqualTo(2));
            Assert.That(iniFile.First().Count, Is.EqualTo(3));
            Assert.That(iniFile.Section("Foo").Get("yes"), Is.EqualTo("true"));
        }

        [Test]
        public void TestRemoveSection()
        {
            var iniFile = new IniFile(new StringReader("[Foo]\nbar=1\nbaz=qux\n[Foo]\nyes=true\n[Foo2]\nno=false"));
            iniFile.RemoveSection("Foo2");
            Assert.That(iniFile.Count(), Is.EqualTo(1));
        }

        [Test]
        public void TestRemoveProperty()
        {
            var iniFile = new IniFile(new StringReader("[Foo]\nbar=1\nbaz=qux\n[Foo]\nyes=true\n[Foo2]\nno=false"));

            Assert.That(iniFile.Section("Foo").Get("baz"), Is.Not.Null);
            iniFile.Section("Foo").Set("baz", null);
            Assert.That(iniFile.Section("Foo").Get("baz"), Is.Null);

            Assert.That(iniFile.Section("Foo").Get("bar"), Is.Not.Null);
            iniFile.Section("Foo").Set("bar", "   ");
            Assert.That(iniFile.Section("Foo").Get("bar"), Is.Null);

            Assert.That(iniFile.Section("Foo2").Get("no"), Is.Not.Null);
            iniFile.Section("Foo2").RemoveProperty("no");
            Assert.That(iniFile.Section("Foo2").Get("no"), Is.Null);
        }

        [Test]
        public void TestSave()
        {
            var tempFilename = Path.GetTempFileName();
            var iniFile = new IniFile();
            iniFile.Section("Foo").Comment = "This is foo";
            iniFile.Section("Foo").Set("bar", "1");
            iniFile.Section("Foo").Set("baz", "qux", comment: "bazy baz");
            iniFile.Save(tempFilename);

            Assert.That(File.ReadAllText(tempFilename), Is.EqualTo("# This is foo\r\n[Foo]\r\nbar=1\r\n# bazy baz\r\nbaz=qux\r\n\r\n"));
        }

        [Test]
        public void TestPadding()
        {
            var tempFilename = Path.GetTempFileName();
            var iniFile = new IniFile {WriteSpacingBetweenNameAndValue = true};
            iniFile.Section("Foo").Set("bar", "1");
            iniFile.Save(tempFilename);

            Assert.That(File.ReadAllText(tempFilename), Is.EqualTo("[Foo]\r\nbar = 1\r\n\r\n"));
        }

        [Test]
        public void TestDoNotWriteSectionIfEmpty()
        {
            var tempFilename = Path.GetTempFileName();
            var iniFile = new IniFile();
            iniFile.Section("Foo").Set("bar", "1");
            iniFile.Section("Foo2");
            iniFile.Save(tempFilename);

            Assert.That(File.ReadAllText(tempFilename), Is.EqualTo("[Foo]\r\nbar=1\r\n\r\n"));
        }

        [Test]
        public void TestSplitsOnFirstEqualSign()
        {
            var iniFile = new IniFile(new StringReader("[Foo]\nbar = -i 3 -f 5 s=3 -n=9 -f=bar"));
            Assert.That(iniFile.Section("Foo").Get("bar"), Is.EqualTo("-i 3 -f 5 s=3 -n=9 -f=bar"));
        }

        [Test]
        public void TestChangeCommentChar()
        {
            var tempFilename = Path.GetTempFileName();
            var iniFile = new IniFile();
            iniFile.Section("Foo").Comment = "Foo comment";
            iniFile.Section("Foo").Set("bar", "1", "Bar comment");
            
            iniFile.Save(tempFilename);

            Assert.That(File.ReadAllText(tempFilename), Is.EqualTo("# Foo comment\r\n[Foo]\r\n# Bar comment\r\nbar=1\r\n\r\n"));

            iniFile.CommentChar = ';';
            iniFile.Save(tempFilename);
            Assert.That(File.ReadAllText(tempFilename), Is.EqualTo("; Foo comment\r\n[Foo]\r\n; Bar comment\r\nbar=1\r\n\r\n"));
        }
    }
}