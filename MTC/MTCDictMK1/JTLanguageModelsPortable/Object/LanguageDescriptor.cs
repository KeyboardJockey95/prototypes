using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Admin;

namespace JTLanguageModelsPortable.Object
{
    public class LanguageDescriptor : BaseObject
    {
        public string Name { get; set; }

        public LanguageID LanguageID { get; set; }

        public bool Show { get; set; }

        public List<string> HiddenKeys { get; set; }

        public bool Used { get; set; }

        public string PreferredFontFamily { get; set; }

        public string PreferredFontSizeText { get; set; }
        public string PreferredFontSizeList { get; set; }
        public string PreferredFontSizeFlash { get; set; }
        public string PreferredFontSizeSubtitles { get; set; }

        public static string[] Names = new string[]
            { "Host", "Target" };
        public static string[] AllNames = new string[]
            { "UI", "Host", "Target" };
        public static string[] TargetNames = new string[]
            { "Target" };

        public LanguageDescriptor(string name, LanguageID languageID, bool show, bool used = true,
            string preferredFontFamily = "", string preferredFontSizeText = "", string preferredFontSizeList = "",
            string preferredFontSizeFlash = "", string preferredFontSizeSubtitles = "")
        {
            Name = name;
            LanguageID = languageID;
            Show = show;
            HiddenKeys = null;
            Used = used;
            PreferredFontFamily = preferredFontFamily;
            PreferredFontSizeText = preferredFontSizeText;
            PreferredFontSizeList = preferredFontSizeList;
            PreferredFontSizeFlash = preferredFontSizeFlash;
            PreferredFontSizeSubtitles = preferredFontSizeSubtitles;
        }

        public LanguageDescriptor(XElement element)
        {
            OnElement(element);
        }

        public LanguageDescriptor(LanguageDescriptor other)
        {
            Copy(other);
        }

        public LanguageDescriptor()
        {
            ClearLanguageDescriptor();
        }

        public override void Clear()
        {
            base.Clear();
            ClearLanguageDescriptor();
        }

        public void ClearLanguageDescriptor()
        {
            Name = null;
            LanguageID = null;
            Show = true;
            HiddenKeys = null;
            Used = false;
            PreferredFontFamily = String.Empty;
            PreferredFontSizeText = String.Empty;
            PreferredFontSizeList = String.Empty;
            PreferredFontSizeFlash = String.Empty;
            PreferredFontSizeSubtitles = String.Empty;
        }

        public void Copy(LanguageDescriptor other)
        {
            Name = other.Name;
            LanguageID = other.LanguageID;
            Show = other.Show;
            if (other.HiddenKeys != null)
                HiddenKeys = new List<string>(other.HiddenKeys);
            else
                HiddenKeys = null;
            Used = other.Used;
            PreferredFontFamily = other.PreferredFontFamily;
            PreferredFontSizeText = other.PreferredFontSizeText;
            PreferredFontSizeList = other.PreferredFontSizeList;
            PreferredFontSizeFlash = other.PreferredFontSizeFlash;
            PreferredFontSizeSubtitles = other.PreferredFontSizeSubtitles;
        }

        public string LanguageCultureExtensionCode
        {
            get
            {
                if (LanguageID != null)
                    return LanguageID.LanguageCultureExtensionCode;
                return String.Empty;
            }
        }

        public string DisplayName
        {
            get
            {
                switch (Name)
                {
                    case "Host":
                        return "Host";
                    case "Target":
                        return "Target";
                    case "TargetAlternate1":
                        return "Target Alternate 1";
                    case "TargetAlternate2":
                        return "Target Alternate 2";
                    case "TargetAlternate3":
                        return "Target Alternate 3";
                    default:
                        return Name;
                }
            }
        }

        public string TokenAbbreviation
        {
            get
            {
                switch (Name)
                {
                    case "Host":
                        return "H";
                    case "Target":
                        return "T";
                    case "TargetAlternate1":
                        return "A";
                    case "TargetAlternate2":
                        return "A";
                    case "TargetAlternate3":
                        return "A";
                    default:
                        if (!String.IsNullOrEmpty(Name))
                            return Name.Substring(0, 1);
                        else
                            return "";
                }
            }
        }

        public bool GetShowState(string key)
        {
            if (String.IsNullOrEmpty(key))
                return Show;
            else if (HiddenKeys == null)
                return true;
            else if (HiddenKeys.Contains(key))
                return false;
            return true;
        }

        public void SetShowState(string key, bool show)
        {
            if (!show)
            {
                if (HiddenKeys == null)
                    HiddenKeys = new List<string>() { key };
                else
                {
                    if (!HiddenKeys.Contains(key))
                        HiddenKeys.Add(key);
                }
            }
            else
            {
                if (HiddenKeys != null)
                {
                    if (HiddenKeys.Contains(key))
                    {
                        if (HiddenKeys.Count() == 1)
                            HiddenKeys = null;
                        else
                            HiddenKeys.Remove(key);
                    }
                }
            }
        }

        public void ClearShowStates()
        {
            Show = true;
            HiddenKeys = null;
        }

        public string PreferredFontStyleTextAttribute
        {
            get
            {
                string fontStyle = PreferredFontStyleTextStyle;
                if (!String.IsNullOrEmpty(fontStyle))
                    fontStyle = "style=\"" + fontStyle + "\"";
                return fontStyle;
            }
            private set {}
        }

        public string PreferredFontStyleListAttribute
        {
            get
            {
                string fontStyle = PreferredFontStyleListStyle;
                if (!String.IsNullOrEmpty(fontStyle))
                    fontStyle = "style=\"" + fontStyle + "\"";
                return fontStyle;
            }
            private set { }
        }

        public string PreferredFontStyleFlashAttribute
        {
            get
            {
                string fontStyle = PreferredFontStyleFlashStyle;
                if (!String.IsNullOrEmpty(fontStyle))
                    fontStyle = "style=\"" + fontStyle + "\"";
                return fontStyle;
            }
            private set { }
        }

        public string PreferredFontStyleTextStyle
        {
            get
            {
                string fontStyle = "";
                if (!String.IsNullOrEmpty(PreferredFontFamily))
                    fontStyle += "font-family:" + PreferredFontFamily + ";";
                if (!String.IsNullOrEmpty(PreferredFontSizeText))
                    fontStyle += "font-size:" + PreferredFontSizeText + "pt;";
                return fontStyle;
            }
            private set { }
        }

        public string PreferredFontStyleListStyle
        {
            get
            {
                string fontStyle = "";
                if (!String.IsNullOrEmpty(PreferredFontFamily))
                    fontStyle += "font-family:" + PreferredFontFamily + ";";
                if (!String.IsNullOrEmpty(PreferredFontSizeList))
                    fontStyle += "font-size:" + PreferredFontSizeList + "pt;";
                return fontStyle;
            }
            private set { }
        }

        public string PreferredFontStyleFlashStyle
        {
            get
            {
                string fontStyle = "";
                if (!String.IsNullOrEmpty(PreferredFontFamily))
                    fontStyle += "font-family:" + PreferredFontFamily + ";";
                if (!String.IsNullOrEmpty(PreferredFontSizeFlash))
                    fontStyle += "font-size:" + PreferredFontSizeFlash + "pt;";
                return fontStyle;
            }
            private set { }
        }

        public string PreferredFontStyleSubtitlesStyle
        {
            get
            {
                string fontStyle = "";
                if (!String.IsNullOrEmpty(PreferredFontFamily))
                    fontStyle += "font-family:" + PreferredFontFamily + ";";
                if (!String.IsNullOrEmpty(PreferredFontSizeSubtitles))
                    fontStyle += "font-size:" + PreferredFontSizeSubtitles + "pt;";
                return fontStyle;
            }
            private set { }
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);
            if (Name != null)
                element.Add(new XAttribute("Name", Name));
            if ((LanguageID != null) && (LanguageID.LanguageCultureExtensionCode != null))
                element.Add(new XAttribute("LanguageID", LanguageID.LanguageCultureExtensionCode));
            element.Add(new XAttribute("Show", Show.ToString()));
            if ((HiddenKeys != null) && (HiddenKeys.Count() != 0))
                element.Add(new XAttribute("HiddenKeys", ObjectUtilities.GetStringFromStringList(HiddenKeys)));
            element.Add(new XAttribute("Used", Used.ToString()));
            if (!String.IsNullOrEmpty(PreferredFontFamily))
                element.Add(new XAttribute("PreferredFontFamily", PreferredFontFamily));
            if (!String.IsNullOrEmpty(PreferredFontSizeText))
                element.Add(new XAttribute("PreferredFontSizeText", PreferredFontSizeText));
            if (!String.IsNullOrEmpty(PreferredFontSizeList))
                element.Add(new XAttribute("PreferredFontSizeList", PreferredFontSizeList));
            if (!String.IsNullOrEmpty(PreferredFontSizeFlash))
                element.Add(new XAttribute("PreferredFontSizeFlash", PreferredFontSizeFlash));
            if (!String.IsNullOrEmpty(PreferredFontSizeSubtitles))
                element.Add(new XAttribute("PreferredFontSizeSubtitles", PreferredFontSizeSubtitles));
            return element;
        }

        public override bool OnAttribute(XAttribute attribute)
        {
            string attributeValue = attribute.Value.Trim();

            switch (attribute.Name.LocalName)
            {
                case "Name":
                    Name = attributeValue;
                    break;
                case "LanguageID":
                    LanguageID = LanguageLookup.GetLanguageIDNoAdd(attributeValue);
                    break;
                case "Show":
                    if (!String.IsNullOrEmpty(attributeValue))
                        Show = Convert.ToBoolean(attributeValue);
                    else
                        Show = false;
                    break;
                case "HiddenKeys":
                    HiddenKeys = ObjectUtilities.GetStringListFromString(attributeValue);
                    break;
                case "Used":
                    if (!String.IsNullOrEmpty(attributeValue))
                        Used = Convert.ToBoolean(attributeValue);
                    else
                        Used = false;
                    break;
                case "PreferredFontFamily":
                    PreferredFontFamily = attributeValue;
                    break;
                case "PreferredFontSizeText":
                    PreferredFontSizeText = attributeValue;
                    break;
                case "PreferredFontSizeList":
                    PreferredFontSizeList = attributeValue;
                    break;
                case "PreferredFontSizeFlash":
                    PreferredFontSizeFlash = attributeValue;
                    break;
                case "PreferredFontSizeSubtitles":
                    PreferredFontSizeSubtitles = attributeValue;
                    break;
                case "PreferredFontSize":   // Legacy
                    PreferredFontSizeText = attributeValue;
                    break;
                default:
                    return base.OnAttribute(attribute);
            }

            return true;
        }

        public static LanguageID SafeLanguageIDFromLanguageDescriptors(List<LanguageDescriptor> languageDescriptors, string name, LanguageID defaultLanguageID)
        {
            if (languageDescriptors == null)
                return defaultLanguageID;

            LanguageDescriptor languageDescriptor = languageDescriptors.FirstOrDefault(x => x.Name == name);

            if (languageDescriptor == null)
                return defaultLanguageID;

            if (languageDescriptor.LanguageID == null)
                return defaultLanguageID;

            return languageDescriptor.LanguageID;
        }

        public static LanguageDescriptor LanguageDescriptorFromLanguageID(string name, LanguageID languageID)
        {
            return new LanguageDescriptor(name, languageID, true,
                ((languageID != null) && (languageID != LanguageLookup.Any) && !String.IsNullOrEmpty(languageID.LanguageCultureExtensionCode)), null);
        }

        public static LanguageDescriptor LanguageDescriptorFromLanguageCode(string name, string languageCode)
        {
            LanguageID languageID = LanguageLookup.GetLanguageIDNoAdd(languageCode);
            return LanguageDescriptorFromLanguageID(name, languageID);
        }

        public static LanguageDescriptor LanguageDescriptorFromLanguageID(LanguageID languageID, List<LanguageID> languageIDs)
        {
            int index = languageIDs.IndexOf(languageID);
            if (index == -1)
                return null;
            if (index >= Names.Count())
                return null;
            return new LanguageDescriptor(Names[index], languageID, true, true, null);
        }

        public static LanguageDescriptor LanguageDescriptorFromLanguageName(string name, List<LanguageID> languageIDs)
        {
            if (languageIDs == null)
                return null;

            int index;
            int count = Names.Count();
            LanguageID languageID;

            for (index = 0; index < count; index++)
            {
                if (Names[index] == name)
                {
                    if (index > languageIDs.Count())
                    {
                        if (languageIDs.Count() > 0)
                            languageID = languageIDs[0];
                        else
                            languageID = LanguageLookup.English;
                    }
                    else
                        languageID = languageIDs[index];

                    return new LanguageDescriptor(Names[index], languageID, true, true, null);
                }
            }

            return null;
        }

        public static List<LanguageDescriptor> LanguageDescriptorsFromLanguageIDs(List<LanguageID> languageIDs)
        {
            if (languageIDs == null)
                return new List<LanguageDescriptor>();
            int index = 0;
            int endIndex = Names.Count() - 1;
            List<LanguageDescriptor> languageDescriptors = new List<LanguageDescriptor>(languageIDs.Count());
            foreach (LanguageID languageID in languageIDs)
            {
                languageDescriptors.Add(LanguageDescriptorFromLanguageID(Names[index], languageID));
                if (index < endIndex)
                    index++;
            }
            return languageDescriptors;
        }

        public static List<LanguageDescriptor> LanguageDescriptorsFromLanguageIDs(string name, List<LanguageID> languageIDs)
        {
            if (languageIDs == null)
                return new List<LanguageDescriptor>();
            List<LanguageDescriptor> languageDescriptors = new List<LanguageDescriptor>(languageIDs.Count());
            foreach (LanguageID languageID in languageIDs)
                languageDescriptors.Add(LanguageDescriptorFromLanguageID(name, languageID));
            return languageDescriptors;
        }

        public static List<LanguageDescriptor> LanguageDescriptorsFromLanguageIDs(string[] names, List<LanguageID> languageIDs)
        {
            if ((languageIDs == null) || (languageIDs.Count() == 0) || (names == null) || (names.Count() == 0))
                return new List<LanguageDescriptor>();
            int index = 0;
            int endIndex = languageIDs.Count() - 1;
            List<LanguageDescriptor> languageDescriptors = new List<LanguageDescriptor>(languageIDs.Count());
            foreach (LanguageID languageID in languageIDs)
            {
                string name;
                if (index < names.Count())
                    name = names[index];
                else
                    name = names.First();
                languageDescriptors.Add(LanguageDescriptorFromLanguageID(name, languageID));
                if (index < endIndex)
                    index++;
            }
            return languageDescriptors;
        }

        public static List<LanguageID> LanguageIDsFromLanguageDescriptors(List<LanguageDescriptor> languageDescriptors)
        {
            if (languageDescriptors == null)
                return new List<LanguageID>();
            List<LanguageID> languageIDs = new List<LanguageID>(languageDescriptors.Count());
            foreach (LanguageDescriptor languageDescriptor in languageDescriptors)
            {
                if (languageDescriptor.Used && (languageDescriptor.LanguageID != null) &&
                        !languageIDs.Contains(languageDescriptor.LanguageID))
                    languageIDs.Add(languageDescriptor.LanguageID);
            }
            return languageIDs;
        }

        public static LanguageDescriptor CopyLanguageDescriptor(LanguageDescriptor languageDescriptor)
        {
            if (languageDescriptor == null)
                return null;

            return new LanguageDescriptor(languageDescriptor);
        }

        public static List<LanguageDescriptor> CopyLanguageDescriptors(List<LanguageDescriptor> languageDescriptors)
        {
            if (languageDescriptors == null)
                return null;

            List<LanguageDescriptor> list = new List<LanguageDescriptor>(languageDescriptors.Count());

            foreach (LanguageDescriptor languageDescriptor in languageDescriptors)
            {
                list.Add(new LanguageDescriptor(languageDescriptor));
            }

            return list;
        }

        public static List<LanguageDescriptor> CopyLanguageDescriptors(List<LanguageDescriptor> languageDescriptors, string name)
        {
            if (languageDescriptors == null)
                return null;

            List<LanguageDescriptor> list = new List<LanguageDescriptor>(languageDescriptors.Count());

            foreach (LanguageDescriptor languageDescriptor in languageDescriptors)
            {
                list.Add(new LanguageDescriptor(name, languageDescriptor.LanguageID, languageDescriptor.Show, languageDescriptor.Used,
                    languageDescriptor.PreferredFontFamily, languageDescriptor.PreferredFontSizeText,
                    languageDescriptor.PreferredFontSizeList, languageDescriptor.PreferredFontSizeFlash, languageDescriptor.PreferredFontSizeSubtitles));
            }

            return list;
        }

        public static List<LanguageDescriptor> CopyLanguageDescriptors(List<LanguageDescriptor> languageDescriptors,
            List<LanguageID> masterLanguageIDs)
        {
            if (languageDescriptors == null)
                return null;

            List<LanguageDescriptor> list = new List<LanguageDescriptor>(languageDescriptors.Count());

            foreach (LanguageDescriptor languageDescriptor in languageDescriptors)
            {
                if ((masterLanguageIDs == null) || !masterLanguageIDs.Contains(languageDescriptor.LanguageID))
                    continue;

                list.Add(new LanguageDescriptor(languageDescriptor));
            }

            return list;
        }

        public static List<LanguageDescriptor> CopyLanguageDescriptors(List<LanguageDescriptor> languageDescriptors,
            List<LanguageID> masterLanguageIDs, string name)
        {
            if (languageDescriptors == null)
                return null;

            List<LanguageDescriptor> list = new List<LanguageDescriptor>(languageDescriptors.Count());

            foreach (LanguageDescriptor languageDescriptor in languageDescriptors)
            {
                if ((masterLanguageIDs == null) || !masterLanguageIDs.Contains(languageDescriptor.LanguageID))
                    continue;

                list.Add(new LanguageDescriptor(name, languageDescriptor.LanguageID, languageDescriptor.Show, languageDescriptor.Used,
                    languageDescriptor.PreferredFontFamily, languageDescriptor.PreferredFontSizeText,
                    languageDescriptor.PreferredFontSizeList, languageDescriptor.PreferredFontSizeFlash,
                    languageDescriptor.PreferredFontSizeSubtitles));
            }

            return list;
        }

        public static List<LanguageDescriptor> FilterDuplicates(List<LanguageDescriptor> list, bool favorLater)
        {
            List<LanguageDescriptor> newList = new List<LanguageDescriptor>(list.Count());

            foreach (LanguageDescriptor languageDescriptor in list)
            {
                LanguageDescriptor duplicate = newList.FirstOrDefault(x => (x.LanguageID == languageDescriptor.LanguageID) /*&& (x.Name == languageDescriptor.Name)*/);

                if (duplicate != null)
                {
                    if (favorLater)
                        newList.Remove(duplicate);
                    else
                        continue;
                }

                newList.Add(languageDescriptor);
            }

            return newList;
        }

        public int Compare(LanguageDescriptor other)
        {
            if (other == null)
                return 1;

            int returnValue = ObjectUtilities.CompareStrings(Name, other.Name);

            if (returnValue != 0)
                return returnValue;

            returnValue = ObjectUtilities.CompareLanguageIDs(LanguageID, other.LanguageID);

            return returnValue;
        }

        public static int CompareLanguageDescriptorLists(List<LanguageDescriptor> list1, List<LanguageDescriptor> list2)
        {
            if ((list1 == null) || (list1.Count() == 0))
            {
                if ((list2 == null) || (list2.Count() == 0))
                    return 0;

                return -1;
            }
            else if ((list2 == null) || (list2.Count() == 0))
                return 1;
            else
            {
                int index;

                for (index = 0; ; index++)
                {
                    if ((index >= list1.Count()) || (index >= list2.Count()))
                    {
                        if (list1.Count() == list2.Count())
                            return 0;
                        else
                            return list1.Count - list2.Count();
                    }

                    LanguageDescriptor languageDescriptor = list1[index];

                    if (list2.Count(x => (x.Name == languageDescriptor.Name) && (x.LanguageID == languageDescriptor.LanguageID)) == 0)
                        return -1;
                }
            }
        }

        public static void AddAdditionalLanguageDescriptors(List<LanguageDescriptor> languageDescriptors,
            List<LanguageID> languageIDs, UserProfile userProfile)
        {
            if ((languageDescriptors == null) || (languageIDs == null))
                return;

            int index;
            int count = languageIDs.Count();

            for (index = 0; index < count; index++)
            {
                LanguageID languageID = languageIDs[index];

                LanguageDescriptor languageDescriptor = languageDescriptors.FirstOrDefault(x => x.Used && (x.LanguageID == languageID));

                if (languageDescriptor == null)
                {
                    languageDescriptor = userProfile.GetLanguageDescriptor("Display", languageID);

                    if (languageDescriptor == null)
                        languageDescriptor = new LanguageDescriptor("Display", languageID, true, true);

                    languageDescriptors.Add(languageDescriptor);
                }
            }
        }

        public static void AddAlternateLanguageDescriptors(List<LanguageDescriptor> languageDescriptors)
        {
            if (languageDescriptors == null)
                return;

            int index;
            int count = languageDescriptors.Count();

            for (index = 0; index < count; index++)
            {
                LanguageDescriptor languageDescriptor = languageDescriptors[index];

                if (!languageDescriptor.Used || (languageDescriptor.LanguageID == null))
                    continue;

                List<LanguageID> alternates = LanguageLookup.GetAlternateLanguageIDs(languageDescriptor.LanguageID);

                if (alternates == null)
                    continue;

                foreach (LanguageID alternate in alternates)
                {
                    if (languageDescriptors.FirstOrDefault(x => x.LanguageID == alternate) == null)
                        languageDescriptors.Add(LanguageDescriptorFromLanguageID(languageDescriptor.Name, alternate));
                }
            }
        }

        public static List<LanguageDescriptor> GetTargetAndHostLanguageDescriptorsFromLanguageIDs(
            List<LanguageID> targetLanguageIDs,
            List<LanguageID> hostLanguageIDs)
        {
            List<LanguageDescriptor> returnValue = new List<LanguageDescriptor>();

            if (targetLanguageIDs != null)
            {
                foreach (LanguageID languageID in targetLanguageIDs)
                    returnValue.Add(new LanguageDescriptor("Target", languageID, true));
            }

            if (hostLanguageIDs != null)
            {
                foreach (LanguageID languageID in hostLanguageIDs)
                    returnValue.Add(new LanguageDescriptor("Host", languageID, true));
            }

            return returnValue;
        }

        public static List<LanguageDescriptor> GetLanguageDescriptorsFromLanguageIDs(List<LanguageID> languageIDs, string name)
        {
            List<LanguageDescriptor> returnValue = new List<LanguageDescriptor>();
            if (languageIDs != null)
            {
                foreach (LanguageID languageID in languageIDs)
                    returnValue.Add(new LanguageDescriptor(name, languageID, true));
            }
            return returnValue;
        }

        public static void GetLanguageIDsFromLanguageDescriptors(List<LanguageDescriptor> languageDescriptors,
            out LanguageID uiLanguageID, out List<LanguageID> targetLanguageIDs, out List<LanguageID> hostLanguageIDs)
        {
            uiLanguageID = null;
            targetLanguageIDs = new List<LanguageID>();
            hostLanguageIDs = new List<LanguageID>();

            if (languageDescriptors == null)
                return;

            foreach (LanguageDescriptor languageDescriptor in languageDescriptors)
            {
                switch (languageDescriptor.Name)
                {
                    case "Target":
                        targetLanguageIDs.Add(languageDescriptor.LanguageID);
                        break;
                    case "Host":
                        hostLanguageIDs.Add(languageDescriptor.LanguageID);
                        break;
                    case "UI":
                        if (uiLanguageID == null)
                            uiLanguageID = languageDescriptor.LanguageID;
                        break;
                }
            }

            if (uiLanguageID == null)
                uiLanguageID = hostLanguageIDs.FirstOrDefault();

            if (uiLanguageID == null)
                uiLanguageID = LanguageLookup.English;

            if (hostLanguageIDs.Count() == 0)
                hostLanguageIDs.Add(uiLanguageID);
        }

        public static void GetLanguageIDsFromLanguageDescriptors(List<LanguageDescriptor> languageDescriptors,
            out List<LanguageID> languageIDs)
        {
            languageIDs = new List<LanguageID>();

            if (languageDescriptors == null)
                return;

            foreach (LanguageDescriptor languageDescriptor in languageDescriptors)
            {
                if (languageDescriptor.Used)
                    languageIDs.Add(languageDescriptor.LanguageID);
            }
        }

        public static List<LanguageDescriptor> GetLanguageDescriptorListIntersection(List<LanguageDescriptor> list1, List<LanguageDescriptor> list2)
        {
            List<LanguageDescriptor> list = new List<LanguageDescriptor>();
            if ((list1 == null) || (list1.Count() == 0))
            {
                if ((list2 == null) || (list2.Count() == 0))
                    return list;

                return list;
            }
            else if ((list2 == null) || (list2.Count() == 0))
                return list;
            else
            {
                foreach (LanguageDescriptor ld in list1)
                {
                    if (list2.FirstOrDefault(x => (x.Name == ld.Name) && (x.LanguageID == ld.LanguageID)) != null)
                        list.Add(new LanguageDescriptor(ld));
                }
                return list;
            }
        }

        public static List<LanguageDescriptor> OrderUIAndHostLanguageDescriptorsFirst(
            List<LanguageDescriptor> languageDescriptors, LanguageID uiLanguageID, List<LanguageID> hostLanguageIDs)
        {
            if (languageDescriptors == null)
                return null;
            List<LanguageDescriptor> newLanguageDescriptors = new List<LanguageDescriptor>(languageDescriptors);
            if (hostLanguageIDs != null)
            {
                int count = hostLanguageIDs.Count();
                int index;
                for (index = count - 1; index >= 0; index--)
                {
                    LanguageID languageID = hostLanguageIDs[index];
                    LanguageDescriptor languageDescriptor =
                        newLanguageDescriptors.FirstOrDefault(x => x.LanguageID == languageID);
                    if (languageDescriptor != null)
                    {
                        newLanguageDescriptors.Remove(languageDescriptor);
                        newLanguageDescriptors.Insert(0, languageDescriptor);
                    }
                }
            }
            if (uiLanguageID != null)
            {
                LanguageDescriptor languageDescriptor =
                    newLanguageDescriptors.FirstOrDefault(x => x.LanguageID == uiLanguageID);
                if (languageDescriptor != null)
                {
                    newLanguageDescriptors.Remove(languageDescriptor);
                    newLanguageDescriptors.Insert(0, languageDescriptor);
                }
            }
            return newLanguageDescriptors;
        }

        public static int GetVisibleLanguageDescriptorCount(List<LanguageDescriptor> languageDescriptors)
        {
            if (languageDescriptors == null)
                return 0;
            int count = 0;
            foreach (LanguageDescriptor languageDescriptor in languageDescriptors)
            {
                if (languageDescriptor.Show && languageDescriptor.Used)
                    count++;
            }
            return count;
        }

        public static List<string> GetLanguageCultureExtensionCodes(List<LanguageDescriptor> languageDescriptors)
        {
            List<string> list = new List<string>();

            if (languageDescriptors != null)
            {
                foreach (LanguageDescriptor languageDescriptor in languageDescriptors)
                    list.Add(languageDescriptor.LanguageCultureExtensionCode);
            }

            return list;
        }

        public static Dictionary<string, bool> GetLanguageFlagsDictionaryFromStringList(
            List<string> flagNames, List<LanguageDescriptor> languageDescriptors)
        {
            if (flagNames == null)
                return null;

            if (languageDescriptors == null)
                return null;

            Dictionary<string, bool> flagDictionary = new Dictionary<string, bool>();

            foreach (string flagName in flagNames)
            {
                LanguageDescriptor languageDescriptor = languageDescriptors.FirstOrDefault(x => x.LanguageCultureExtensionCode == flagName);
                flagDictionary.Add(flagName, languageDescriptor.Show);
            }

            return flagDictionary;
        }

        public static LanguageDescriptor GetIthLanguageDescriptor(
            List<LanguageDescriptor> languageDescriptors,
            string name,
            int ith)
        {
            int count = 0;
            if (languageDescriptors == null)
                return null;
            foreach (LanguageDescriptor languageDescriptor in languageDescriptors)
            {
                if (languageDescriptor.Name == name)
                {
                    if (count++ == ith)
                        return languageDescriptor;
                }
            }

            return null;
        }

        public static List<LanguageDescriptor> GetExtendedLanguageDescriptorsFromLanguageDescriptors(List<LanguageDescriptor> languageDescriptors)
        {
            List<LanguageDescriptor> extendedLanguageDescriptors = new List<LanguageDescriptor>();

            foreach (LanguageDescriptor languageDescriptor in languageDescriptors)
            {
                List<LanguageID> familyLanguageIDs = LanguageLookup.GetLanguagePlusAlternateLanguageIDs(languageDescriptor.LanguageID);

                foreach (LanguageID familyLanguageID in familyLanguageIDs)
                {
                    if (extendedLanguageDescriptors.FirstOrDefault(x => (x.LanguageID == familyLanguageID) && (x.Name == languageDescriptor.Name)) == null)
                    {
                        LanguageDescriptor ld = languageDescriptors.FirstOrDefault(x => (x.LanguageID == familyLanguageID) && (x.Name == languageDescriptor.Name));

                        if (ld != null)
                            extendedLanguageDescriptors.Add(new LanguageDescriptor(ld));
                        else
                            extendedLanguageDescriptors.Add(new LanguageDescriptor(languageDescriptor.Name, familyLanguageID, true, true));
                    }
                }
            }

            return extendedLanguageDescriptors;
        }
    }
}
