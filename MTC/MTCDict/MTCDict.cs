/// <summary>
/// 
/// File: MTCDict.cs
/// 
/// Author: Mike Eldredge
/// 
/// Origination Date 21 Apr 2022
/// 
/// Description:
///     Import a dictionary entry from its native format
///     Instantiate a DictionaryEntry object
///     Export the object as XML
/// 
/// Sample Input
/// 
/// Sample Output
/// 
///     Hello, world!
///     
/// </summary>
///

using JTLanguageModelsPortable.Dictionary;
using System.Xml;

namespace MTCDict
{
    internal class MTCDict
    {
        static void Main(string[] args)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            XmlWriter writer = XmlWriter.Create("MTCDictionary.xml", settings);
            writer.WriteStartElement("JTLanguage");
            writer.WriteAttributeString("Version", "1");

            {
                DictionaryEntry entry = new DictionaryEntry();
                entry.Xml.WriteTo(writer);
            }

            writer.WriteEndElement();

            writer.Close();
        }
    }
}
