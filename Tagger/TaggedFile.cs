using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tagger
{
    public class TaggedFile
    {

        string FileName;

        private Dictionary<string, Tag> Tags = new Dictionary<string, Tag>();

        public void Save()
        {

            using (FlacLibSharp.FlacFile flac = new FlacLibSharp.FlacFile(FileName))
            {
                // First get the existing VorbisComment (if any)
                var comment = flac.VorbisComment;
                if (comment == null)
                {
                    // Create a new vorbis comment metadata block
                    comment = new FlacLibSharp.VorbisComment();
                    // Add it to the flac file
                    flac.Metadata.Add(comment);
                }

                //Remove the ones to be removed
                foreach (var remTag in DeletedTags.Keys)
                {
                    if (comment.ContainsField(remTag))
                    {
                        comment.Remove(remTag);
                    }
                }

                //Update the ones to be updated
                foreach (var updTag in ChangedTags.Keys)
                {
                    if (comment.ContainsField(updTag))
                    {
                        comment.Remove(updTag);
                    }
                    var values = Tags[updTag].GetSaveValues;
                    comment.Add(updTag, values);
                }

                //Add ones to be added
                foreach (var addTag in AddedTags.Keys)
                {
                    if (comment.ContainsField(addTag))
                    {
                        //This shouldn't ever happen, but just in case.
                        comment.Remove(addTag);
                    }
                    comment.Add(addTag, Tags[addTag].GetSaveValues);

                }

                // Write the changes back to the FLAC file
                flac.Save();
            }
        }

        public TaggedFile(string fileName)
        {
            this.FileName = fileName;
            char[] delim = new char[] { ';' };
            using (FlacLibSharp.FlacFile file = new FlacLibSharp.FlacFile(fileName))
            {
                var vorbisComment = file.VorbisComment;
                if (vorbisComment != null)
                {
                    
                    foreach (var tag in vorbisComment)
                    {
                        List<string> values = new List<string>();
                        foreach (var value in tag.Value)
                        {
                            //Console.WriteLine("{0}: {1}", tag.Key, tag.Value);
                            foreach (string part in value.Split(delim))
                            {
                                values.Add(part.Trim());
                            }
                            
                        }
                        Tags.Add(tag.Key.ToLower(), new Tag(tag.Key.ToLower(), values));

                    }
                }
            }
            
        }

        public List<string> GetTag(string key)
        {
            if (Tags.ContainsKey(key.ToLower())) return Tags[key.ToLower()].values;
            return new List<string>();
        }

        public List<string> GetTagOrigValue(string key)
        {
            if (Tags.ContainsKey(key.ToLower())) return Tags[key.ToLower()].origValues;
            return new List<string>();
        }

        public void SetTag(string key, List<string> values)
        {
            if (!Tags.ContainsKey(key.ToLower())) Tags.Add(key.ToLower(), new Tag(key.ToLower()));
            Tags[key.ToLower()].values = values;
        }

        public void RemoveTag(string key)
        {
            if (Tags.ContainsKey(key.ToLower())) Tags[key.ToLower()].values = new List<string>();
        }

        public Dictionary<string, List<string>> AllTags
        {
            get
            {
                Dictionary<string, List<string>> tags = new Dictionary<string, List<string>>();
                foreach (string tagKey in Tags.Keys)
                {
                    tags.Add(tagKey, Tags[tagKey].values);
                }
                return tags;
            }
        }


        public Dictionary<string, List<string>> ChangedTags
        {
            get
            {
                Dictionary<string, List<string>> changed = new Dictionary<string, List<string>>();
                foreach (Tag tag in Tags.Values)
                {
                    if (tag.Changed) changed.Add(tag.Key, tag.values);
                }
                return changed;
            }
        }

        public Dictionary<string, List<string>> AddedTags
        {
            get
            {
                Dictionary<string, List<string>> added = new Dictionary<string, List<string>>();
                foreach (Tag tag in Tags.Values)
                {
                    if (tag.Added) added.Add(tag.Key, tag.values);
                }
                return added;
            }
        }

        public Dictionary<string, List<string>> DeletedTags
        {
            get
            {
                Dictionary<string, List<string>> deleted = new Dictionary<string, List<string>>();
                foreach (Tag tag in Tags.Values)
                {
                    if (tag.Deleted) deleted.Add(tag.Key, tag.values);
                }
                return deleted;
            }
        }


    }

    public class Tag
    {
        private readonly string[] singleTagsOnly = { "ALBUM", "ALBUMARTIST", "ALBUM ARTIST", "EPOQUE", "ORCHESTRA" };
        public List<string> origValues = new List<string>();
        public List<string> values = new List<string>();
        public string Key;

        public bool IsSingleLineTag
        {
            get
            {
                foreach (string s in singleTagsOnly)
                {
                    if (Key.ToUpper() == s) return true;
                }
                return false;
                
            }
        }

        public List<string> GetSaveValues
        {
            get
            {
                if (IsSingleLineTag)
                {
                    return new List<string>(new string[] { string.Join("; ", values.ToArray<string>()) });
                }
                else
                {
                    return values;
                }
                    
            }
        }

        internal Tag (string key, List<string> values)
        {
            this.Key = key;
            this.origValues = values;
            this.values = values;
        }

        public Tag(string key)
        {
            this.Key = key;
            
        }

       
        public bool Changed
        {
            get {
                if (origValues.Count > 0 && origValues.Count != values.Count) return true;
                for (int idx = 0; idx < origValues.Count; idx++)
                {
                    if (origValues[idx] != values[idx]) return true;
                }
                return false;
            }
        }
        public bool Added
        {
            get
            {
                return (origValues.Count == 0 && values.Count != 0);
            }
        }

        public bool Deleted
        {
            get
            {
                return (values.Count == 0 && origValues.Count != 0);
            }
        }
    }
}
