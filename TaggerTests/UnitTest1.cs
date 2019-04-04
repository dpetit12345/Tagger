using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;
namespace TaggerTests
{
    [TestClass]
    public class TaggerTests
    {

        [TestMethod]
        public void TestSave()
        {
            Tagger.TaggedFile taggedFile = new Tagger.TaggedFile(@"E:\Music\Adrian Boult\1992 - Boult conducts Holst\01-01-Holst_ A Fugal Overture, for orchestra, Op. 40-1, H. 151.flac");
            taggedFile.RemoveTag("NEWTAG1");
            var existingTags = taggedFile.AllTags;

            List<string> origTagNames = new List<string>(existingTags.Keys);

            var values = new List<string>();
            values.Add("First Value");
            values.Add("Second Value");
            taggedFile.SetTag("NEWTAG1", values);
            taggedFile.Save();

            taggedFile = new Tagger.TaggedFile(@"E:\Music\Adrian Boult\1992 - Boult conducts Holst\01-01-Holst_ A Fugal Overture, for orchestra, Op. 40-1, H. 151.flac");
            Assert.IsTrue(taggedFile.GetTag("NEWTAG1").Count == 2, "Did not read back 2 values of added tag.");
            Assert.IsTrue(taggedFile.GetTag("NEWTAG1")[0] == "First Value", "Did not read back 2 values of added tag.");
            Assert.IsTrue(taggedFile.GetTag("NEWTAG1")[1] == "Second Value", "Did not read back 2 values of added tag.");
            Assert.IsTrue(origTagNames.Count == taggedFile.AllTags.Count - 1, "INcorrect tag count after add");

            values = new List<string>();
            values.Add("New First Value");
            values.Add("New Second Value");
            taggedFile.SetTag("NEWTAG1", values);
            taggedFile.Save();

            taggedFile = new Tagger.TaggedFile(@"E:\Music\Adrian Boult\1992 - Boult conducts Holst\01-01-Holst_ A Fugal Overture, for orchestra, Op. 40-1, H. 151.flac");
            Assert.IsTrue(taggedFile.GetTag("NEWTAG1").Count == 2, "Did not read back 2 values of added tag.");
            Assert.IsTrue(taggedFile.GetTag("NEWTAG1")[0] == "New First Value", "Did not read back 2 values of added tag.");
            Assert.IsTrue(taggedFile.GetTag("NEWTAG1")[1] == "New Second Value", "Did not read back 2 values of added tag.");
            Assert.IsTrue(origTagNames.Count == taggedFile.AllTags.Count - 1, "INcorrect tag count after change");

            taggedFile.RemoveTag("NEWTAG1");
            taggedFile.Save();

            taggedFile = new Tagger.TaggedFile(@"E:\Music\Adrian Boult\1992 - Boult conducts Holst\01-01-Holst_ A Fugal Overture, for orchestra, Op. 40-1, H. 151.flac");
            Assert.IsTrue(taggedFile.GetTag("NEWTAG1").Count == 0, "Read back deleted tag");
            Assert.IsTrue(origTagNames.Count == taggedFile.AllTags.Count, "INcorrect tag count after delete");


        }
        [TestMethod]
        public void TestChangingTagsValues()
        {

            Tagger.TaggedFile taggedFile = new Tagger.TaggedFile(@"E:\Music\Adrian Boult\1992 - Boult conducts Holst\01-01-Holst_ A Fugal Overture, for orchestra, Op. 40-1, H. 151.flac");

            Assert.IsTrue(taggedFile.AddedTags.Count == 0);
            Assert.IsTrue(taggedFile.ChangedTags.Count == 0);
            Assert.IsTrue(taggedFile.DeletedTags.Count == 0);

            List<string> values = new List<string>();
            values.Add("new value");
            taggedFile.SetTag("New Tag", values);
            Assert.IsTrue(taggedFile.GetTag("New Tag").Count == 1, "Too many values on tag");
            Assert.IsTrue(taggedFile.GetTag("New Tag")[0]== "new value", "Wrong Value on tag");
            Assert.IsTrue(taggedFile.AddedTags.Count == 1, "Number of added tags is wrong");
            Assert.IsTrue(taggedFile.ChangedTags.Count == 0);
            Assert.IsTrue(taggedFile.DeletedTags.Count == 0);

            try
            {
                taggedFile.RemoveTag("THIS TAG DOESNT EXIST");
            }
            catch
            {
                Assert.Fail("Removing tag that didnt exist failed");
            }

            try
            {
                taggedFile.RemoveTag("COMPOSER");
                Assert.IsTrue(taggedFile.GetTag("Composer").Count == 0, "Tag still has value after being removed.");
                Assert.IsTrue(taggedFile.DeletedTags.Count == 1);
            }
            catch
            {
                Assert.Fail("Removing tag that did exist failed");
            }

            List<string> compvalues = new List<string>();
            compvalues.Add("New Composer Value");
            compvalues.Add("Another composer");
            taggedFile.SetTag("ComPoser", compvalues);
            Assert.IsTrue(taggedFile.GetTag("composer").Count == 2, "invalid number of values");
            Assert.IsTrue(taggedFile.GetTag("composer")[0] == "New Composer Value", "wrong values");
            Assert.IsTrue(taggedFile.GetTag("composer")[1] == "Another composer", "wrong values");

            Assert.IsTrue(taggedFile.AddedTags.Count == 1);
            Assert.IsTrue(taggedFile.ChangedTags.Count == 1);
            Assert.IsTrue(taggedFile.DeletedTags.Count == 0);

            var artist = taggedFile.GetTag("artists");
            int initCount = artist.Count;

            artist.Add("Another artist");
            
            taggedFile.SetTag("artists", artist);
            Assert.IsTrue(taggedFile.GetTag("ARTISTS").Count == initCount + 1);

            taggedFile.SetTag("ALBUM", new List<string>(taggedFile.GetTag("Album")));
            Assert.IsTrue(taggedFile.ChangedTags.Count == 1);
            var alist = taggedFile.GetTag("AlBUM");
            alist[0] = "NEW VALUE";
            taggedFile.SetTag("Album", alist);
            Assert.IsTrue(taggedFile.ChangedTags.Count == 2);


        }
    }
}
