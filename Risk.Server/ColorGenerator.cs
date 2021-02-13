using System;
using System.Collections.Generic;
using System.Linq;

namespace Risk.Server
{
    public static class ColorGenerator
    {
        static ColorGenerator()
        {
            colors = new Dictionary<string, string>();
        }

        static Queue<string> palette = new Queue<string>(new []{
            "#FFE6E6",
            "#4682B4",
            "#5F9EA0",
            "#6495ED",
            "#6A5ACD",
            "#808080",
            "#8FBC8F",
            "#A9A9A9",
            "#C0C0C0",
            "#FFAAAA",
            "#D46A6A",
            "#801515",
            "#0066FF",
            "#00FFFF",
            "#3399FF",
            "#33FFFF",
            "#9900FF",
            "#9966FF",
            "#9999FF",
            "#99FFFF",
            "#CC33FF",
            "#CCFFFF",
            "#FFCCFF",
            "#550000"
        });

        private static Dictionary<string, string> colors;

        public static string GetColor(string token)
        {
            if(!colors.ContainsKey(token))
            {
                if(palette.Any())
                {
                    colors.Add(token, palette.Dequeue());
                }
                else
                {
                    colors.Add(token, "FFFFFF");
                }
            }

            return colors[token];
        }
    }
}


