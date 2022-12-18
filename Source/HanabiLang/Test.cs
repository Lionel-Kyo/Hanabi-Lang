using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testing
{
    public static class Test
    {
        public static string[] PrintLn(string[] lines)
        {
            foreach (string line in lines) 
            {
                Console.WriteLine(line);
            }
            return lines;
        }

        public static Dictionary<string, string> PrintDict(Dictionary<string, string> lines)
        {
            foreach (var kv in lines)
            {
                Console.WriteLine($"{kv.Key}: {kv.Value}");
            }
            return lines;
        }
    }
}
