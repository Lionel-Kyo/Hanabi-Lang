using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testing
{
    public static class Test
    {
        public static void PrintLn(string[] lines)
        {
            foreach (string line in lines) 
            {
                Console.WriteLine(line);
            }
        }
    }
}
