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

            var parser = new HtmlParser();
            var document = parser.ParseDocument(html);

            foreach (IElement element in document.QuerySelectorAll("p"))
            {
                text += element.TextContent;
            }

            // Ссылка
            foreach (IElement element in document.QuerySelectorAll("a"))
            {
                string href = element.GetAttribute("href");
                if (href.Contains("zoom.us"))
                {
                    link += href;
                }

            }

            string[] textArr = text.Split(' ');

            if (textArr.Length > 1)
            {

                Console.WriteLine("Length: " + textArr.Length);

                //Тип
                //int i = textArr.ToList().IndexOf("Лекция");
                //if (i == -1)
                //{
                //    i = textArr.ToList().IndexOf("занятие");
                //    type = textArr[i - 1] + " " + textArr[i];
                //}
                //else
                //{
                //    type = textArr[i];
                //}


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
            else
            {
                return null;
            }
        }
    }
}
