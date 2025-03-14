using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example
{
    public class Item(string key, string value, DateTime when, int number)
    {
        public string Key { get; set; } = key;
        public string Value { get; set; } = value;
        public DateTime When { get; set; } = when;
        public int Number { get; set; } = number;
    }
}
