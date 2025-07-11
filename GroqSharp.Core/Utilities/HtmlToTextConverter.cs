﻿using HtmlAgilityPack;

namespace GroqSharp.Core.Utilities
{
    public static class HtmlToTextConverter
    {
        public static string Convert(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            return doc.DocumentNode.InnerText;
        }
    }
}