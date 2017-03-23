using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Baka_Tsuki_Downloader
{
    class Tag
    {
        public string name;
        public List<Tag> innerTags;
        public Dictionary<string, string> attributes;
        public string content;

        public Tag()
        {
            innerTags = new List<Tag>();
            attributes = new Dictionary<string, string>();
        }

        private string FindFirst(string attribute, Tag tag)
        {
            if (tag.attributes.ContainsKey(attribute))
            {
                return tag.attributes[attribute];
            }
            foreach (Tag t in tag.innerTags)
            {
                string s = FindFirst(attribute, t);
                if (s != null)
                {
                    return s;
                }
            }
            return null;
        }

        /// <summary>
        /// Depth First search of the tag tree, but stops as soon as the first matching element has been found
        /// </summary>
        /// <param name="attribute">The search key</param>
        /// <returns>The value of the firt attribute that was equal to the parameter. Returns null on failure</returns>
        public string FindFirst(string attribute)
        {
            return FindFirst(attribute, this);
        }

        private void Find(string attribute, List<string> list, Tag tag)
        {
            if (tag.attributes.ContainsKey(attribute))
            {
                list.Add(tag.attributes[attribute]);
            }
            foreach (Tag t in tag.innerTags)
            {
                Find(attribute, list, t);
            }
        }

        /// <summary>
        /// Depth First search of the tag tree
        /// </summary>
        /// <param name="attribute">The search key</param>
        /// <returns>A list containing all values belonging to the searched attribute</returns>
        public List<string> Find(string attribute)
        {
            List<string> result = new List<string>();

            this.Find(attribute, result, this);

            return result;
        }

        private void Find(string attribute, string value, List<Tag> list, Tag tag)
        {
            if (tag.attributes.ContainsKey(attribute))
            {
                if (tag.attributes[attribute].Equals(value))
                    list.Add(tag);
            }
            foreach (Tag t in tag.innerTags)
            {
                Find(attribute, value, list, t);
            }
        }

        /// <summary>
        /// Depth First search of the tag tree
        /// </summary>
        /// <param name="attribute">The search key attribute</param>
        /// <param name="value">The search key value</param>
        /// <returns>A list containg all tags that had the attribute="value"</returns>
        public List<Tag> Find(string attribute, string value)
        {
            List<Tag> result = new List<Tag>();

            this.Find(attribute, value, result, this);

            return result;
        }


        private Tag FindFirst(string attribute, string value, Tag tag)
        {
            if (tag.attributes.ContainsKey(attribute))
            {
                if (tag.attributes[attribute].Equals(value))
                    return tag;
            }
            foreach (Tag t in tag.innerTags)
            {
                Tag foundTag = FindFirst(attribute, value, t) ;
                if (foundTag != null)
                    return foundTag;
            }
            return null;
        }

        /// <summary>
        /// Depth First search of the tag tree. Stops at the first match
        /// </summary>
        /// <param name="attribute">The search key attribute</param>
        /// <param name="value">The search key value</param>
        /// <returns>Returns the first tag for which attribute="value"</returns>
        public Tag FindFirst(string attribute, string value)
        {
            return FindFirst(attribute, value, this);
        }
    }
}
