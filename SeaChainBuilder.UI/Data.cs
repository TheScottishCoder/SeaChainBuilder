using System;
using System.Collections.Generic;
using System.Text;

namespace SeaChainBuilder.UI
{
    public class Data
    {
        public Data(int h = 0, int w = 0)
        {
            height = h;
            width = w;
        }

        public int height { get; set; }
        public int width { get; set; }

        public string[,] _map;
    }
}
