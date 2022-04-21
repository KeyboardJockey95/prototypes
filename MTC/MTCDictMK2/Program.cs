using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JTLanguageModelsPortable.Dictionary;
using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;
using System.IO;

namespace MTCDictMK2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DictionaryEntry entry = new DictionaryEntry();
            entry.Key = "foo";
            //Debug.write

            //Debug.Write(entry.ToString());


            using (StreamWriter stream = File.CreateText("C:\\MTC\\OutputText.xml"))
            {
                XElement element = entry.Xml;
                element.Save(stream);
            }
        }
    }
}
