/* Copyright (2021) Fractality LLC - All Rights Reserved
 * 
 * Unauthorized copying of this file, via any medium is strictly prohibited
 * Proprietary and confidential
 */

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