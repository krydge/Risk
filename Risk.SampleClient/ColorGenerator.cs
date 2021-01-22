using System;
using System.Collections.Generic;
using System.Linq;

namespace Risk.SampleClient
{
    public class ColorGenerator
    {
        public ColorGenerator()
        {
            colors = new Dictionary<string, string>();
        }

        Queue<string> palette = new Queue<string>(new []{
            "#FFFFFF",
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

        private Dictionary<string, string> colors;

        public string GetColor(string token)
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


