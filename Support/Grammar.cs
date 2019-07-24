using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace HakeemTestV4.SupportClasses
{
    //Class to Camelcase word
    public class Grammar
    {
        public static string Capitalise(string word)
        {
            string[] words = word.Split(' ');
            string output = "";
            foreach (string x in words)
            {
                if (Char.IsNumber(x.First()))
                {
                    output += x + "  ";
                }
                else if (x == "and" || x == "of" || x == "for" || x == "with")
                {
                    output += x + "  ";
                }
                else if (x == "it")
                {
                    output += "IT";
                }
                else
                {
                    output += x.First().ToString().ToUpper() + x.Substring(1) + "  ";
                }
            }
            return output;
        }

        public static int Capitalise(int word)
        {
            return word;
        }
    }
}