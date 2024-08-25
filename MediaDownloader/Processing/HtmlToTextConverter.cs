using HtmlAgilityPack;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;

namespace MediaDownloader
{
    internal class HtmlToTextConverter
    {
        public static string ConvertHtmlToPlainText(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            StringBuilder plainText = new StringBuilder();

            foreach (var node in doc.DocumentNode.ChildNodes)
            {
                ConvertNodeToPlainText(node, plainText);
            }

            // Replace multiple consecutive empty lines with a single empty line
            string formattedText = Regex.Replace(plainText.ToString(), @"^\s+$[\r\n]*", Environment.NewLine, RegexOptions.Multiline).Trim();

            // Replace \n with \r\n for proper display in TextBox
            formattedText = formattedText.Replace("\n", "\r\n");

            return formattedText;
        }

        private static void ConvertNodeToPlainText(HtmlNode node, StringBuilder plainText)
        {
            if (node.Name == "br")
            {
                plainText.AppendLine();
            }
            else if (node.Name == "p")
            {
                plainText.AppendLine();
                plainText.AppendLine(WebUtility.HtmlDecode(node.InnerText.Trim()));
                plainText.AppendLine();
            }
            else if (node.Name == "a")
            {
                var linkText = WebUtility.HtmlDecode(node.InnerText.Trim());
                var href = node.GetAttributeValue("href", "");
                plainText.AppendLine($"{linkText}");
            }
            else
            {
                if (node.HasChildNodes)
                {
                    foreach (var child in node.ChildNodes)
                    {
                        ConvertNodeToPlainText(child, plainText);
                    }
                }
                else
                {
                    plainText.Append(WebUtility.HtmlDecode(node.InnerText));
                }
            }
        }
    }
}
