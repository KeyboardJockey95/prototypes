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
using JTLanguageModelsPortable.Object;
using System.Xml;

namespace MTCDict
{
    internal class MTCDict
    {
        static void Main(string[] args)
        {
            using (XmlWriter xmlWriter = XmlWriter.Create("MTCDictionary.xml", new XmlWriterSettings { Indent = true }))
            using (XmlReader xmlReader = XmlReader.Create("en-es.xml"))
            {
                xmlWriter.WriteStartElement("JTLanguage");
                xmlWriter.WriteAttributeString("Version", "1");

                LanguageID fromLanguageID = new LanguageID();
                LanguageID toLanguageID = new LanguageID();

                while (xmlReader.Read())
                {
                    if ((xmlReader.NodeType == XmlNodeType.Element) && (xmlReader.Name == "dic"))
                    {
                        fromLanguageID.FromString(xmlReader.GetAttribute("from"));
                        toLanguageID.FromString(xmlReader.GetAttribute("to"));
                        break;
                    }
                }

                while (xmlReader.Read())
                {
                    if ((xmlReader.NodeType == XmlNodeType.Element) && (xmlReader.Name == "w"))
                    {
                        DictionaryEntry entry = new DictionaryEntry { LanguageID = fromLanguageID };

                        Sense sense = new Sense();
                        LanguageSynonyms synonyms = new LanguageSynonyms { LanguageID = toLanguageID };
                        sense.AddLanguageSynonyms(synonyms);

                        entry.AddSense(sense);

                        while (xmlReader.Read())
                        {
                            if (xmlReader.NodeType == XmlNodeType.Element)
                            {
                                switch (xmlReader.Name)
                                {
                                    case "c":
                                        entry.Key = xmlReader.ReadString();
                                        break;

                                    case "d":
                                        synonyms.AddProbableSynonym(new ProbableMeaning { Meaning = xmlReader.ReadString() });
                                        break;

                                    default:
                                        break;
                                }
                            }

                            if ((xmlReader.NodeType == XmlNodeType.EndElement) && (xmlReader.Name == "w"))
                            {
                                break;
                            }
                        }

                        entry.Xml.WriteTo(xmlWriter);
                    }
                }

                xmlWriter.WriteEndElement();
            }
            // xmlWriter will automatically close
        }
    }
}
