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
                // Write the root element
                xmlWriter.WriteStartElement("JTLanguage");
                xmlWriter.WriteAttributeString("Version", "1");

                // capture the 'from' and 'to' language IDs
                LanguageID fromLanguageID = new LanguageID();
                LanguageID toLanguageID = new LanguageID();
                while (xmlReader.Read())
                {
                    // there had better be a <dic> element or this loop will eat the whole file
                    if ((xmlReader.NodeType == XmlNodeType.Element) && (xmlReader.Name == "dic"))
                    {
                        fromLanguageID.FromString(xmlReader.GetAttribute("from"));
                        toLanguageID.FromString(xmlReader.GetAttribute("to"));
                        break;
                    }
                }

                // Main loop for reading all the <w> (word?) entries
                while (xmlReader.Read())
                {
                    if ((xmlReader.NodeType == XmlNodeType.Element) && (xmlReader.Name == "w"))
                    {
                        // Found the beginning of a <w> (word) entry.  Create the entry DictionaryEntry object that we will populate
                        // and use for output and set the fromLanguageID on the entry.
                        DictionaryEntry entry = new DictionaryEntry { LanguageID = fromLanguageID };

                        // The DictionaryEntry is composed of a series of Sense entries.  In this case there is only one Sense per DictionaryEntry
                        Sense sense = new Sense();
                        entry.AddSense(sense);

                        // The Sense element contains a LanguageSynonyms element which is a list of ProbableSynonyms elements.
                        LanguageSynonyms synonyms = new LanguageSynonyms { LanguageID = toLanguageID };
                        sense.AddLanguageSynonyms(synonyms);

                        while (xmlReader.Read())
                        {
                            // capture and process the attributes of the word (<w>) element
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

                                continue;
                            }

                            // detect the end of the word (</w>) element.  Note that this depends on there not being any nested <w> elements
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
