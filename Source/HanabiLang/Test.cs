﻿using System;
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
            DateTime dt = DateTime.Now; 
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

    public class Faker
    {
        private int a;
        public int B => 12345;

        public int BA { get; set; }

        public int c;
        public const string Hi = "Hi";
        public static string Hello => "Hello";
        public Faker()
        {

        }
        public Faker(int a)
        {
            this.a = a;
        }

        public int GetA()
        {
            return this.a;
        }

        public static string Nyan() => "Nyan";
    }

    public enum ENUMMM
    {

    }
}
