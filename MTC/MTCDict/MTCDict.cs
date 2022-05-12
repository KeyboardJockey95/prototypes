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
using System.Collections.Generic;

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
                                    // the <c> element contains the host language version of the word
                                    case "c":
                                        entry.Key = xmlReader.ReadString();
                                        break;

                                    // the <d> element contains the target language version(s) of the word (comma-separated list), and an optional {gender}
                                    case "d":
                                        string dElementContent = xmlReader.ReadString();
                                        string[] tlGenderedWords = dElementContent.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                                        foreach (string tlGenderedWord in tlGenderedWords)
                                        {
                                            string[] tlGenderedWordComponents = tlGenderedWord.Trim().Split(' ');
                                            ProbableMeaning probableMeaning = new ProbableMeaning() { Meaning = tlGenderedWordComponents[0] };

                                            // Do we have a gender
                                            if (tlGenderedWordComponents.Length > 1)
                                            {
                                                List<LexicalAttribute> lexicalAttributes = new List<LexicalAttribute>();

                                                switch (tlGenderedWordComponents[1])
                                                {
                                                    case "{f}":
                                                        lexicalAttributes.Add(LexicalAttribute.Feminine);
                                                        break;

                                                    case "{m}":
                                                        lexicalAttributes.Add(LexicalAttribute.Masculine);
                                                        break;

                                                    default:
                                                        break;
                                                }

                                                if (lexicalAttributes.Count > 0)
                                                {
                                                    probableMeaning.Attributes = lexicalAttributes;
                                                }
                                            }

                                            synonyms.AddProbableSynonym(probableMeaning);
                                        }

                                        break;

                                    // the <t> element is a composite element, <t>{partOfSpeech} /optionalPronunciation/ (definition) SEE:</t>
                                    // definition may have a nested (parenthetical statement)
                                    case "t":
                                        string tElementContent = xmlReader.ReadString();
                                        string beginDelims = "{/(";
                                        string endDelims = "}/)";
                                        
                                        while (tElementContent.Length > 0)
                                        {
                                            foreach (char beginDelim in beginDelims)
                                            {
                                                if (tElementContent[0] == beginDelim)
                                                {
                                                    // Move past the begin delimiter.  This helps us out especially in the case where both begin and end delimiters are the same
                                                    tElementContent = tElementContent.Substring(1);

                                                    // verify the existence of an end delimiter
                                                    char endDelim = endDelims[beginDelims.IndexOf(beginDelim)];

                                                    int endDelimIndex = tElementContent.IndexOf(endDelim);
                                                    if (endDelimIndex != -1)
                                                    {
                                                        // capture the delimited content
                                                        string delimitedContent = tElementContent.Substring(0, endDelimIndex);

                                                        // advance the string to the last delimiter.  The enclosing while loop will advance past the last delim
                                                        tElementContent = tElementContent.Substring(delimitedContent.Length);
                                                    }
                                                    break;
                                                }
                                            }

                                            if (tElementContent.Length > 0)
                                            {
                                                // Advance one char
                                                tElementContent = tElementContent.Substring(1);
                                            }
                                        }

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
