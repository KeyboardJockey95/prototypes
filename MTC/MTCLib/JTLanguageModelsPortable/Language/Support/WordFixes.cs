using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Language
{
    public class WordFixes : BaseObject
    {
        public HashSet<string> CompoundsToSeparate;
        public HashSet<string> CompoundsToNotSeparate;
        public List<string> EmbeddedConversions;
        public List<string> FullConversions;

        public WordFixes(XElement element)
        {
            OnElement(element);
        }

        public WordFixes()
        {
            CompoundsToSeparate = null;
            CompoundsToNotSeparate = null;
            EmbeddedConversions = null;
            FullConversions = null;
        }

        public bool IsCompoundToSeparate(string text)
        {
            if (CompoundsToSeparate != null)
                return CompoundsToSeparate.Contains(text);
            return false;
        }

        public bool AddCompoundToSeparate(string compound)
        {
            if (CompoundsToSeparate == null)
            {
                CompoundsToSeparate = new HashSet<string>();
                CompoundsToSeparate.Add(compound);
            }
            else if (!CompoundsToSeparate.Contains(compound))
                CompoundsToSeparate.Add(compound);

            if (IsCompoundToNotSeparate(compound))
                RemoveCompoundToNotSeparate(compound);

            return true;
        }

        public bool AddCompoundsToSeparate(List<string> compounds)
        {
            if (compounds != null)
            {
                foreach (string compound in compounds)
                    AddCompoundToSeparate(compound);
            }

            return true;
        }

        public bool RemoveCompoundToSeparate(string compound)
        {
            if (CompoundsToSeparate == null)
                return false;

            if (CompoundsToSeparate.Contains(compound))
                CompoundsToSeparate.Remove(compound);
            else
                return false;

            return true;
        }

        public bool IsCompoundToNotSeparate(string text)
        {
            if (CompoundsToNotSeparate != null)
                return CompoundsToNotSeparate.Contains(text);
            return false;
        }

        public bool AddCompoundToNotSeparate(string compound)
        {
            if (CompoundsToNotSeparate == null)
            {
                CompoundsToNotSeparate = new HashSet<string>();
                CompoundsToNotSeparate.Add(compound);
            }
            else if (!CompoundsToNotSeparate.Contains(compound))
                CompoundsToNotSeparate.Add(compound);

            if (IsCompoundToSeparate(compound))
                RemoveCompoundToSeparate(compound);

            return true;
        }

        public bool AddCompoundsToNotSeparate(List<string> compounds)
        {
            if (compounds != null)
            {
                foreach (string compound in compounds)
                    AddCompoundToNotSeparate(compound);
            }

            return true;
        }

        public bool RemoveCompoundToNotSeparate(string compound)
        {
            if (CompoundsToNotSeparate == null)
                return false;

            if (CompoundsToNotSeparate.Contains(compound))
                CompoundsToNotSeparate.Remove(compound);
            else
                return false;

            return true;
        }

        public string Convert(string text)
        {
            string returnValue = text;

            if (EmbeddedConversions != null)
            {
                int count = EmbeddedConversions.Count();
                int index;

                for (index = 0; index < count; index += 2)
                    returnValue = returnValue.Replace(EmbeddedConversions[index], EmbeddedConversions[index + 1]);
            }

            if (FullConversions != null)
            {
                int count = FullConversions.Count();
                int index;

                for (index = 0; index < count; index += 2)
                {
                    string conversionFrom = FullConversions[index];

                    if (returnValue == conversionFrom)
                    {
                        returnValue = FullConversions[index + 1];
                        break;
                    }
                }
            }

            return returnValue;
        }

        public void AddEmbeddedConversion(string from, string to)
        {
            if (EmbeddedConversions == null)
                EmbeddedConversions = new List<string>();

            EmbeddedConversions.Add(from);
            EmbeddedConversions.Add(to);
        }

        public void AddEmbeddedConversions(List<string> conversions)
        {
            if (EmbeddedConversions == null)
                EmbeddedConversions = new List<string>();

            EmbeddedConversions.AddRange(conversions);
        }

        public void AddFullConversion(string from, string to)
        {
            if (FullConversions == null)
                FullConversions = new List<string>();

            FullConversions.Add(from);
            FullConversions.Add(to);
        }

        public void AddFullConversions(List<string> conversions)
        {
            if (FullConversions == null)
                FullConversions = new List<string>();

            FullConversions.AddRange(conversions);
        }

        public override XElement GetElement(string name)
        {
            XElement element = new XElement(name);

            if ((CompoundsToSeparate != null) && (CompoundsToSeparate.Count() != 0))
                element.Add(
                    new XElement(
                        "CompoundsToSeparate",
                        "\n    " + TextUtilities.GetStringFromStringHashSetDelimited(CompoundsToSeparate, ",\n    ") + "\n  "));

            if ((CompoundsToNotSeparate != null) && (CompoundsToNotSeparate.Count() != 0))
                element.Add(
                    new XElement(
                        "CompoundsToNotSeparate",
                        "\n    " + TextUtilities.GetStringFromStringHashSetDelimited(CompoundsToNotSeparate, ",\n    ") + "\n  "));

            if (EmbeddedConversions != null)
                element.Add(
                    new XElement(
                        "EmbeddedConversions",
                        "\n    " + TextUtilities.GetStringFromStringListDelimited(EmbeddedConversions, ",\n    ") + "\n  "));

            if (FullConversions != null)
                element.Add(
                    new XElement(
                        "FullConversions",
                        "\n    " + TextUtilities.GetStringFromStringListDelimited(FullConversions, ",\n    ") + "\n  "));

            return element;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "CompoundsToSeparate":
                    CompoundsToSeparate = TextUtilities.GetStringHashSetFromString(childElement.Value.Trim());
                    break;
                case "CompoundsToNotSeparate":
                    CompoundsToNotSeparate = TextUtilities.GetStringHashSetFromString(childElement.Value.Trim());
                    break;
                case "EmbeddedConversions":
                    EmbeddedConversions = TextUtilities.GetStringListFromString(childElement.Value.Trim());
                    break;
                case "FullConversions":
                    FullConversions = TextUtilities.GetStringListFromString(childElement.Value.Trim());
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public static string GetFilePath(BaseMarkupContainer optionContainer, LanguageID targetLanguageID, LanguageID uiLanguageID)
        {
            string wordFixesKey = optionContainer.GetInheritedOptionValue("WordFixesKey");
            string wordFixesPath = GetFilePath(wordFixesKey, targetLanguageID);
            return wordFixesPath;
        }

        public static string GetFilePath(string key, LanguageID targetLanguageID)
        {
            string wordFixesName;
            if (targetLanguageID != null)
            {
                string languageSymbol = targetLanguageID.SymbolName;
                wordFixesName = key + "_" + languageSymbol + ".xml";
            }
            else
                wordFixesName = key + ".xml";
            string wordFixesPath = MediaUtilities.ConcatenateFilePath(ApplicationData.LocalDataPath, "WordFixes");
            wordFixesPath = MediaUtilities.ConcatenateFilePath(wordFixesPath, wordFixesName);
            return wordFixesPath;
        }

        public static bool CreateAndLoad(
            BaseMarkupContainer optionContainer,
            LanguageID targetLanguageID,
            LanguageID uiLanguageID,
            out WordFixes wordFixes)
        {
            string filePath = GetFilePath(optionContainer, targetLanguageID, uiLanguageID);
            return CreateAndLoad(filePath, out wordFixes);
        }

        public static bool CreateAndLoad(string filePath, out WordFixes wordFixes)
        {
            wordFixes = new WordFixes();
            return wordFixes.Load(filePath);
        }

        public bool Load(BaseMarkupContainer optionContainer, LanguageID targetLanguageID, LanguageID uiLanguageID)
        {
            string filePath = GetFilePath(optionContainer, targetLanguageID, uiLanguageID);
            return Load(filePath);
        }

        public bool Load(string filePath)
        {
            if (!FileSingleton.Exists(filePath))
                return false;

            try
            {
                XElement element = ApplicationData.Global.LoadXml(filePath);

                if (element != null)
                    OnElement(element);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public bool Save(BaseMarkupContainer optionContainer, LanguageID targetLanguageID, LanguageID uiLanguageID)
        {
            string filePath = GetFilePath(optionContainer, targetLanguageID, uiLanguageID);
            return Save(filePath);
        }

        public bool Save(string filePath)
        {
            try
            {
                XElement element = Xml;
                FileSingleton.DirectoryExistsCheck(filePath);
                ApplicationData.Global.SaveXml(element, filePath);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
