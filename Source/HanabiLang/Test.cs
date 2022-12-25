using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testing
{
    public static class Test
    {
        public static object[] PrintLn(object[] arg)
        {
            object[] lines = (object[])arg;
            foreach (object line in lines) 
            {
                Console.WriteLine(line);
            }
            return lines;
        }

        public static Dictionary<object, object> PrintDict(object arg)
        {
            Dictionary<object, object> lines = (Dictionary<object, object>) arg; 
            foreach (var kv in lines)
            {
                Console.WriteLine($"{kv.Key}: {kv.Value}");
            }
            return lines;
        }
    }
}
