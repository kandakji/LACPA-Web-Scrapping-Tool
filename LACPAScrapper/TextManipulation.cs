using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Specialized;

namespace LACPAScrapper
{
    public class TextManipulation
    {
        public static string GetTextFromHTML(string HTML)
        {
            return GetTextFromHTML(HTML, true);
        }

        public static string GetTextFromHTML(string HTML, bool preserveBr)
        {
            if (HTML == null || HTML.Length < 1) return "";

            ////stlyes
            HTML = Regex.Replace(HTML, @"<[sS][tT][yY][lL][eE]>[\s\S]*?</[sS][tT][yY][lL][eE]>", string.Empty);
            //meta tags
            HTML = Regex.Replace(HTML, @"<[mM][eE][tT][aA][\s\S]*?>", string.Empty);
            //link tags
            HTML = Regex.Replace(HTML, @"<[lL][iI][nN][kK][\s\S]*?>", string.Empty);
            ////font tags
            //HTML = Regex.Replace(HTML, @"<[fF][oO][nN][tT] [\s\S]*?>[\s\S]*?</[fF][oO][nN][tT]>", string.Empty);
            //xml tags
            HTML = Regex.Replace(HTML, @"<[xX][mM][lL]>[\s\S]*?</[xX][mM][lL]>", string.Empty);

            if (preserveBr)
            {
                HTML = System.Text.RegularExpressions.Regex.Replace(HTML, "<[bB][rR](.*?)>", "$BR$");
                HTML = System.Text.RegularExpressions.Regex.Replace(HTML, "<[pP][A-Za-z0-9 \"\'=;-]*>", "$SP$");
                HTML = System.Text.RegularExpressions.Regex.Replace(HTML, "</[pP]>", "$EP$");
                HTML = System.Text.RegularExpressions.Regex.Replace(HTML, "<[sS][tT][rR][oO][nN][gG]>", "$SS$");
                HTML = System.Text.RegularExpressions.Regex.Replace(HTML, "</[sS][tT][rR][oO][nN][gG]>", "$ES$");
                HTML = System.Text.RegularExpressions.Regex.Replace(HTML, "<[bB]>", "$SS$");
                HTML = System.Text.RegularExpressions.Regex.Replace(HTML, "</[bB]>", "$ES$");
                HTML = System.Text.RegularExpressions.Regex.Replace(HTML, "<[uU]>", "$SU$");
                HTML = System.Text.RegularExpressions.Regex.Replace(HTML, "</[uU]>", "$EU$");
            }
            else
                HTML = System.Text.RegularExpressions.Regex.Replace(HTML, "<[bB][rR](.*?)>", "$BR$");

            StringBuilder buffer = new StringBuilder(HTML.Length);

            int index = -1, limit = HTML.Length;
            char currentChar = ' ', previousChar = ' ';
            bool insideTagDef = false;
            while (++index < limit)
            {
                currentChar = HTML[index];

                if ((int)currentChar < 32) currentChar = ' ';
                if (currentChar == ' ' && previousChar == ' ') continue;

                // Inside a Tag?
                if (currentChar == '<' && index < limit - 1)
                {
                    char nextChar = HTML[index + 1];
                    if ((int)nextChar < 32) // white space? then this is not a tag start
                    {
                        previousChar = currentChar;
                        buffer.Append(currentChar);
                    }
                    else // it must be a tag start
                    {
                        insideTagDef = true;
                    }
                }
                // Closing a tag
                else if (currentChar == '>')
                {
                    if (insideTagDef)
                    {
                        insideTagDef = false;
                    }
                    else
                    {
                        previousChar = currentChar;
                        buffer.Append(currentChar);
                    }
                }
                // Normal Character
                else if (!insideTagDef)
                {
                    previousChar = currentChar;
                    buffer.Append(currentChar);
                }
            }

            //HTML = Regex.Replace(HTML, "<[^>]*>", string.Empty);
            //HTML.Replace("&nbsp;", " ");
            if (preserveBr)
            {
                buffer = buffer.Replace("$BR$", "<br/>");
                buffer = buffer.Replace("$SP$", "<p>");
                buffer = buffer.Replace("$EP$", "</p>");
                buffer = buffer.Replace("$SS$", "<strong>");
                buffer = buffer.Replace("$ES$", "</strong>");
                buffer = buffer.Replace("$SU$", "<u>");
                buffer = buffer.Replace("$EU$", "</u>");
            }
            else
                buffer = buffer.Replace("$BR$", " ");
            return buffer.ToString();
        }

        public static string GetClearTextFromHTML(string HTML)
        {
            return Regex.Replace(HTML, "<[^>]*>", string.Empty);
        }

        static string CleanWordHtml(string html)
        {
            StringCollection sc = new StringCollection();

            // Get rid of unnecessary tags
            sc.Add(@"<(meta|link|/?o:|/?style|/?div|/?st\d|/?head|/?html|body|/?body|/?span|!\[)[^>]*?>");
            // get rid of unnecessary tag spans (comments and title)
            sc.Add(@"<!--(\w|\W)+?-->");
            sc.Add(@"<title>(\w|\W)+?</title>");
            // Get rid of classes and styles
            sc.Add(@"\s?class=\w+");
            sc.Add(@"\s+style='[^']+'");
            // Get rid of empty paragraph tags
            sc.Add(@"(<[^>]+>)+&nbsp;(</\w+>)+");
            // remove bizarre v: element attached to <img> tag
            sc.Add(@"\s+v:\w+=""[^""]+""");
            // remove extra lines
            sc.Add(@"(\n\r){2,}");
            foreach (string s in sc)
            {
                html = Regex.Replace(html, s, string.Empty, RegexOptions.IgnoreCase);
            }
            return html;
        }

        private string CleanHtml(string html)
        {
            // start by completely removing all unwanted tags     
            html = Regex.Replace(html, @"<[/]?(font|span|xml|del|ins|[ovwxp]:\w+)[^>]*?>", "", RegexOptions.IgnoreCase);
            // then run another pass over the html (twice), removing unwanted attributes     
            html = Regex.Replace(html, @"<([^>]*)(?:class|lang|style|size|face|[ovwxp]:\w+)=(?:'[^']*'|""[^""]*""|[^\s>]+)([^>]*)>", "<$1$2>", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"<([^>]*)(?:class|lang|style|size|face|[ovwxp]:\w+)=(?:'[^']*'|""[^""]*""|[^\s>]+)([^>]*)>", "<$1$2>", RegexOptions.IgnoreCase);
            return html;
        }
    }

}
