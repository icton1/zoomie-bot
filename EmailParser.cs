using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System.Text.RegularExpressions;

namespace GimmeTheZoomBot
{
    static public class EmailParser
    {
        static HtmlParser parser = new HtmlParser();
        static public string ParseMail(string html)
        {
            string result = string.Empty;
            string text = string.Empty;
            string type = string.Empty;
            string id = "Идендификатор: ";
            string link = "Ссылка: ";
            string tutor = "Преподаватель:";
            string discipline = "Дисциплина:";
            string time = "Время:";
            string pass = "Пароль:";

            var document = parser.ParseDocument(html);

            // Текст сообщения
            text = GetFullEmailText(text, document);

            // Ссылка
            link = GetLinkToZoom(link, document);

            string[] textArr = text.Split(' ');

            if (textArr.Length > 1)
            {
                return SetMessage(out result, text, ref id, link, ref tutor, ref discipline, ref time, ref pass, textArr);
            }
            else
            {
                return null;
            }
        }

        static public string ParseMailsByDay(string html, DateTime date)
        {
            string result = string.Empty;
            string text = string.Empty;
            string type = string.Empty;
            string id = "Идендификатор: ";
            string link = "Ссылка: ";
            string tutor = "Преподаватель:";
            string discipline = "Дисциплина:";
            string time = "Время:";
            string pass = "Пароль:";

            var document = parser.ParseDocument(html);

            // Текст сообщения
            text = GetFullEmailText(text, document);

            // Ссылка
            link = GetLinkToZoom(link, document);

            string[] textArr = text.Split(' ');

            DateTime messageDate = GetTimeFromEmail(text);

            if (textArr.Length > 1 &&  messageDate.Date == date.Date)
            {
                return SetMessage(out result, text, ref id, link, ref tutor, ref discipline, ref time, ref pass, textArr);
            }
            else
            {
                return null;
            }
        }

        private static string SetMessage(out string result, string text, ref string id, string link, ref string tutor, ref string discipline, ref string time, ref string pass, string[] textArr)
        {
            //Идентификатор
            id += textArr[textArr.Length - 3].TrimEnd(',');

            // Пароль
            pass += textArr[textArr.Length - 1];

            // Дисциплина
            int pos1 = text.IndexOf("дисциплине") + "дисциплине".Length;
            int pos2 = text.Substring(pos1).IndexOf(',');

            discipline += text.Substring(pos1, pos2);

            // Преподаватель
            pos1 = text.IndexOf("преподаватель") + "преподаватель".Length;
            pos2 = text.Substring(pos1).IndexOf(',');

            tutor += text.Substring(pos1, pos2);

            // Время
            pos1 = text.IndexOf("состоится") + "состоится".Length;
            time += text.Substring(pos1, 19);

            result = Regex.Replace(discipline.Replace("\n", " "), @"\s+", " ") + "\n" +
                Regex.Replace(tutor.Replace("\n", " "), @"\s+", " ") + "\n" +
                Regex.Replace(time.Replace("\n", " "), @"\s+", " ") + "\n" + id.Replace("\n", "") + "\n" +
                pass.Replace("\n", "") + "\n" + link;

            return result;
        }

        private static DateTime GetTimeFromEmail(string text)
        {
            int i = text.IndexOf("состоится") + "состоится".Length;
            string timeStr = text.Substring(i, 19).Trim().Replace("\n", " ") + ":00.0";

            DateTime time;

            if(DateTime.TryParse(timeStr, out time))
            {
                return time;
            }



            return DateTime.MinValue;
        }

        private static string GetFullEmailText(string text, IHtmlDocument document)
        {
            foreach (IElement element in document.QuerySelectorAll("p"))
            {
                text += element.TextContent;
            }

            return text;
        }

        private static string GetLinkToZoom(string link, IHtmlDocument document)
        {
            foreach (IElement element in document.QuerySelectorAll("a"))
            {
                string href = element.GetAttribute("href");
                if (href.Contains("zoom.us"))
                {
                    link += href;
                }
            }

            return link;
        }
    }
}
