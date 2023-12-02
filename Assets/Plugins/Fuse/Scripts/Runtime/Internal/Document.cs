using System;

namespace Fuse
{
    /// <summary>
    /// Represents a mark-up for documentation menu.
    /// </summary>
    public class Document : Attribute
    {
        public readonly string Category;
        public readonly string Body;

        public Document(string category, string body)
        {
            Category = category;
            Body = body;
        }
    }
}