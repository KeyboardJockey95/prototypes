using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Language
{
    public partial class MultiLanguageTool : BaseObjectLanguages
    {
        public virtual List<LanguageID> InflectionLanguageIDs
        {
            get
            {
                return LanguageIDs;
            }
        }

        public bool AllowArchaicInflections
        {
            get
            {
                List<LanguageTool> languageTools = LanguageTools;

                foreach (LanguageTool languageTool in languageTools)
                {
                    if (languageTool.AllowArchaicInflections)
                        return true;
                }

                return false;
            }
            set
            {
                List<LanguageTool> languageTools = LanguageTools;

                foreach (LanguageTool languageTool in languageTools)
                    languageTool.AllowArchaicInflections = value;
            }
        }

        public virtual List<Inflection> InflectAnyDictionaryFormAll(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex)
        {
            List<Inflection> masterInflectionList = null;
            List<Inflection> inflectionList;
            List<LanguageTool> languageTools = LanguageTools;

            foreach (LanguageTool languageTool in languageTools)
            {
                inflectionList = languageTool.InflectAnyDictionaryFormAll(
                    dictionaryEntry,
                    ref senseIndex,
                    ref synonymIndex);

                if (inflectionList != null)
                    masterInflectionList = MergeInflectionLists(
                        languageTool,
                        masterInflectionList,
                        inflectionList);
            }

            return masterInflectionList;
        }

        public virtual List<Inflection> InflectAnyDictionaryFormDefault(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex)
        {
            List<Inflection> masterInflectionList = null;
            List<Inflection> inflectionList;
            List<LanguageTool> languageTools = LanguageTools;

            foreach (LanguageTool languageTool in languageTools)
            {
                inflectionList = languageTool.InflectAnyDictionaryFormDefault(
                    dictionaryEntry,
                    ref senseIndex,
                    ref synonymIndex);

                if (inflectionList != null)
                    masterInflectionList = MergeInflectionLists(
                        languageTool,
                        masterInflectionList,
                        inflectionList);
            }

            return masterInflectionList;
        }

        public virtual List<Inflection> InflectAnyDictionaryFormScoped(
            string scope,
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex)
        {
            List<Inflection> masterInflectionList = null;
            List<Inflection> inflectionList;
            List<LanguageTool> languageTools = LanguageTools;

            foreach (LanguageTool languageTool in languageTools)
            {
                inflectionList = languageTool.InflectAnyDictionaryFormScoped(
                    scope,
                    dictionaryEntry,
                    ref senseIndex,
                    ref synonymIndex);

                if (inflectionList != null)
                    masterInflectionList = MergeInflectionLists(
                        languageTool,
                        masterInflectionList,
                        inflectionList);
            }

            return masterInflectionList;
        }

        public virtual List<Inflection> InflectAnyDictionaryFormSelected(
            DictionaryEntry dictionaryEntry,
            int senseIndex,
            int synonymIndex,
            List<Designator> designations)
        {
            List<Inflection> masterInflectionList = null;
            List<Inflection> inflectionList;
            List<LanguageTool> languageTools = LanguageTools;

            foreach (LanguageTool languageTool in languageTools)
            {
                inflectionList = languageTool.InflectAnyDictionaryFormSelected(
                    dictionaryEntry,
                    ref senseIndex,
                    ref synonymIndex,
                    designations);

                if (inflectionList != null)
                    masterInflectionList = MergeInflectionLists(
                        languageTool,
                        masterInflectionList,
                        inflectionList);
            }

            return masterInflectionList;
        }

        public virtual bool InflectAnyDictionaryFormDesignated(
            DictionaryEntry dictionaryEntry,
            int senseIndex,
            int synonymIndex,
            Designator designation,
            out Inflection inflection)
        {
            bool returnValue = true;

            inflection = null;

            List<LanguageTool> languageTools = LanguageTools;

            foreach (LanguageTool languageTool in languageTools)
            {
                Inflection inflection1 = null;

                if (!languageTool.InflectAnyDictionaryFormDesignated(
                        dictionaryEntry,
                        ref senseIndex,
                        ref synonymIndex,
                        designation,
                        out inflection1))
                    returnValue = false;

                inflection = MergeInflections(
                    inflection,
                    inflection1);
            }

            return returnValue;
        }

        public virtual List<Inflection> InflectCategoryDictionaryFormSelected(
            LexicalCategory category,
            DictionaryEntry dictionaryEntry,
            int senseIndex,
            int synonymIndex,
            List<Designator> designators)
        {
            List<Inflection> inflections = null;
            switch (category)
            {
                case LexicalCategory.Verb:
                    inflections = ConjugateVerbDictionaryFormSelected(
                        dictionaryEntry,
                        ref senseIndex,
                        ref synonymIndex,
                        designators);
                    break;
                case LexicalCategory.Adjective:
                    inflections = DeclineAdjectiveDictionaryFormSelected(
                        dictionaryEntry,
                        ref senseIndex,
                        ref synonymIndex,
                        designators);
                    break;
                case LexicalCategory.Noun:
                    inflections = DeclineNounDictionaryFormSelected(
                        dictionaryEntry,
                        ref senseIndex,
                        ref synonymIndex,
                        designators);
                    break;
                default:
                    break;
            }
            return inflections;
        }

        public virtual List<Inflection> ConjugateVerbDictionaryFormAll(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex)
        {
            List<Inflection> masterInflectionList = null;
            List<Inflection> inflectionList;
            List<LanguageTool> languageTools = LanguageTools;

            foreach (LanguageTool languageTool in languageTools)
            {
                inflectionList = languageTool.ConjugateVerbDictionaryFormAll(
                    dictionaryEntry,
                    ref senseIndex,
                    ref synonymIndex);

                if (inflectionList != null)
                    masterInflectionList = MergeInflectionLists(
                        languageTool,
                        masterInflectionList,
                        inflectionList);
            }

            return masterInflectionList;
        }

        public virtual List<Designator> GetAllVerbDesignations()
        {
            List<LanguageTool> languageTools = LanguageTools;
            List<Designator> designators = null;

            if (languageTools.Count() != 0)
            {
                LanguageTool languageTool = languageTools.First();
                designators = languageTool.GetAllVerbDesignations();
            }

            return designators;
        }

        public virtual List<Inflection> ConjugateVerbDictionaryFormDefault(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex)
        {
            List<Inflection> masterInflectionList = null;
            List<Inflection> inflectionList;
            List<LanguageTool> languageTools = LanguageTools;

            foreach (LanguageTool languageTool in languageTools)
            {
                inflectionList = languageTool.ConjugateVerbDictionaryFormDefault(
                    dictionaryEntry,
                    ref senseIndex,
                    ref synonymIndex);

                if (inflectionList != null)
                    masterInflectionList = MergeInflectionLists(
                        languageTool,
                        masterInflectionList,
                        inflectionList);
            }

            return masterInflectionList;
        }

        public virtual List<Designator> GetDefaultVerbDesignations()
        {
            List<LanguageTool> languageTools = LanguageTools;
            List<Designator> designators = null;

            if (languageTools.Count() != 0)
            {
                LanguageTool languageTool = languageTools.First();
                designators = languageTool.GetDefaultVerbDesignations();
            }

            return designators;
        }

        public virtual List<Inflection> ConjugateVerbDictionaryFormSelected(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex,
            List<Designator> designations)
        {
#if true
            List<Inflection> masterInflectionList = new List<Inflection>();
            List<LanguageTool> languageTools = LanguageTools;

            foreach (Designator designation in designations)
            {
                LanguageTool lastLanguageTool = null;
                Inflection inflection = null;

                foreach (LanguageTool languageTool in languageTools)
                {
                    Inflection languageInflection = null;

                    if (!languageTool.ConjugateVerbDictionaryFormDesignated(
                            dictionaryEntry,
                            ref senseIndex,
                            ref synonymIndex,
                            designation,
                            out languageInflection))
                    {
                        if (lastLanguageTool != null)
                        {
                            Designator canonicalDesignation = lastLanguageTool.GetCanonicalDesignation(
                                designation,
                                languageTool);

                            if (canonicalDesignation != null)
                            {
                                if (!languageTool.ConjugateVerbDictionaryFormDesignated(
                                        dictionaryEntry,
                                        ref senseIndex,
                                        ref synonymIndex,
                                        canonicalDesignation,
                                        out languageInflection))
                                    continue;
                            }
                        }
                        else
                            continue;
                    }

                    inflection = MergeInflections(
                        inflection,
                        languageInflection);

                    lastLanguageTool = languageTool;
                }

                if (inflection != null)
                    masterInflectionList.Add(inflection);
            }
#else
            List<Inflection> masterInflectionList = null;
            List<LanguageTool> languageTools = LanguageTools;
            LanguageTool lastLanguageTool = null;

            foreach (LanguageTool languageTool in languageTools)
            {
                /*
                inflectionList = languageTool.ConjugateVerbDictionaryFormSelected(
                    dictionaryEntry,
                    ref senseIndex,
                    ref synonymIndex,
                    designations);
                */

                List<Inflection> inflectionList = new List<Inflection>();

                foreach (Designator designation in designations)
                {
                    Inflection inflection;

                    if (ConjugateVerbDictionaryFormDesignated(
                            dictionaryEntry,
                            ref senseIndex,
                            ref synonymIndex,
                            designation,
                            out inflection))
                        inflectionList.Add(inflection);
                }

                masterInflectionList = MergeInflectionLists(
                    languageTool,
                    masterInflectionList,
                    inflectionList);

                lastLanguageTool = languageTool;
            }
#endif

            return masterInflectionList;
        }

        public virtual bool ConjugateVerbDictionaryFormDesignated(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex,
            Designator designation,
            out Inflection inflection)
        {
            bool returnValue = true;

            inflection = null;

            List<LanguageTool> languageTools = LanguageTools;

            foreach (LanguageTool languageTool in languageTools)
            {
                Inflection inflection1 = null;

                if (!languageTool.ConjugateVerbDictionaryFormDesignated(
                        dictionaryEntry,
                        ref senseIndex,
                        ref synonymIndex,
                        designation,
                        out inflection1))
                    returnValue = false;

                inflection = MergeInflections(
                    inflection,
                    inflection1);
            }

            return returnValue;
        }

        public virtual List<Inflection> DeclineNounDictionaryFormAll(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex)
        {
            List<Inflection> masterInflectionList = null;
            List<Inflection> inflectionList;
            List<LanguageTool> languageTools = LanguageTools;

            foreach (LanguageTool languageTool in languageTools)
            {
                inflectionList = languageTool.DeclineNounDictionaryFormAll(
                    dictionaryEntry,
                    ref senseIndex,
                    ref synonymIndex);

                if (inflectionList != null)
                    masterInflectionList = MergeInflectionLists(
                        languageTool,
                        masterInflectionList,
                        inflectionList);
            }

            return masterInflectionList;
        }

        public virtual List<Designator> GetAllNounDesignations()
        {
            List<LanguageTool> languageTools = LanguageTools;
            List<Designator> designators = null;

            if (languageTools.Count() != 0)
            {
                LanguageTool languageTool = languageTools.First();
                designators = languageTool.GetAllNounDesignations();
            }

            return designators;
        }

        public virtual List<Inflection> DeclineNounDictionaryFormDefault(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex)
        {
            List<Inflection> masterInflectionList = null;
            List<Inflection> inflectionList;
            List<LanguageTool> languageTools = LanguageTools;

            foreach (LanguageTool languageTool in languageTools)
            {
                inflectionList = languageTool.DeclineNounDictionaryFormDefault(
                    dictionaryEntry,
                    ref senseIndex,
                    ref synonymIndex);

                if (inflectionList != null)
                    masterInflectionList = MergeInflectionLists(
                        languageTool,
                        masterInflectionList,
                        inflectionList);
            }

            return masterInflectionList;
        }

        public virtual List<Designator> GetDefaultNounDesignations()
        {
            List<LanguageTool> languageTools = LanguageTools;
            List<Designator> designators = null;

            if (languageTools.Count() != 0)
            {
                LanguageTool languageTool = languageTools.First();
                designators = languageTool.GetDefaultNounDesignations();
            }

            return designators;
        }

        public virtual List<Inflection> DeclineNounDictionaryFormSelected(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex,
            List<Designator> designations)
        {
            List<Inflection> masterInflectionList = null;
            List<Inflection> inflectionList;
            List<LanguageTool> languageTools = LanguageTools;

            foreach (LanguageTool languageTool in languageTools)
            {
                inflectionList = languageTool.DeclineNounDictionaryFormSelected(
                    dictionaryEntry,
                    ref senseIndex,
                    ref synonymIndex,
                    designations);

                if (inflectionList != null)
                    masterInflectionList = MergeInflectionLists(
                        languageTool,
                        masterInflectionList,
                        inflectionList);
            }

            return masterInflectionList;
        }

        public virtual bool DeclineNounDictionaryFormDesignated(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex,
            Designator designation,
            out Inflection inflection)
        {
            bool returnValue = true;

            inflection = null;

            List<LanguageTool> languageTools = LanguageTools;

            foreach (LanguageTool languageTool in languageTools)
            {
                Inflection inflection1 = null;

                if (!languageTool.DeclineNounDictionaryFormDesignated(
                        dictionaryEntry,
                        ref senseIndex,
                        ref synonymIndex,
                        designation,
                        out inflection1))
                    returnValue = false;

                inflection = MergeInflections(
                    inflection,
                    inflection1);
            }

            return returnValue;
        }

        public virtual List<Inflection> DeclineAdjectiveDictionaryFormAll(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex)
        {
            List<Inflection> masterInflectionList = null;
            List<Inflection> inflectionList;
            List<LanguageTool> languageTools = LanguageTools;

            foreach (LanguageTool languageTool in languageTools)
            {
                inflectionList = languageTool.DeclineAdjectiveDictionaryFormAll(
                    dictionaryEntry,
                    ref senseIndex,
                    ref synonymIndex);

                if (inflectionList != null)
                    masterInflectionList = MergeInflectionLists(
                        languageTool,
                        masterInflectionList,
                        inflectionList);
            }

            return masterInflectionList;
        }

        public virtual List<Designator> GetAllAdjectiveDesignations()
        {
            List<LanguageTool> languageTools = LanguageTools;
            List<Designator> designators = null;

            if (languageTools.Count() != 0)
            {
                LanguageTool languageTool = languageTools.First();
                designators = languageTool.GetAllAdjectiveDesignations();
            }

            return designators;
        }

        public virtual List<Inflection> DeclineAdjectiveDictionaryFormDefault(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex)
        {
            List<Inflection> masterInflectionList = null;
            List<Inflection> inflectionList;
            List<LanguageTool> languageTools = LanguageTools;

            foreach (LanguageTool languageTool in languageTools)
            {
                inflectionList = languageTool.DeclineAdjectiveDictionaryFormDefault(
                    dictionaryEntry,
                    ref senseIndex,
                    ref synonymIndex);

                if (inflectionList != null)
                    masterInflectionList = MergeInflectionLists(
                        languageTool,
                        masterInflectionList,
                        inflectionList);
            }

            return masterInflectionList;
        }

        public virtual List<Designator> GetDefaultAdjectiveDesignations()
        {
            List<LanguageTool> languageTools = LanguageTools;
            List<Designator> designators = null;

            if (languageTools.Count() != 0)
            {
                LanguageTool languageTool = languageTools.First();
                designators = languageTool.GetDefaultAdjectiveDesignations();
            }

            return designators;
        }

        public virtual List<Inflection> DeclineAdjectiveDictionaryFormSelected(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex,
            List<Designator> designations)
        {
            List<Inflection> masterInflectionList = null;
            List<Inflection> inflectionList;
            List<LanguageTool> languageTools = LanguageTools;

            foreach (LanguageTool languageTool in languageTools)
            {
                inflectionList = languageTool.DeclineAdjectiveDictionaryFormSelected(
                    dictionaryEntry,
                    ref senseIndex,
                    ref synonymIndex,
                    designations);

                if (inflectionList != null)
                    masterInflectionList = MergeInflectionLists(
                        languageTool,
                        masterInflectionList,
                        inflectionList);
            }

            return masterInflectionList;
        }

        public virtual bool DeclineAdjectiveDictionaryFormDesignated(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex,
            Designator designation,
            out Inflection inflection)
        {
            bool returnValue = true;

            inflection = null;

            List<LanguageTool> languageTools = LanguageTools;

            foreach (LanguageTool languageTool in languageTools)
            {
                Inflection inflection1 = null;

                if (!languageTool.DeclineAdjectiveDictionaryFormDesignated(
                        dictionaryEntry,
                        ref senseIndex,
                        ref synonymIndex,
                        designation,
                        out inflection1))
                    returnValue = false;

                inflection = MergeInflections(
                    inflection,
                    inflection1);
            }

            return returnValue;
        }

        public virtual bool Contract(
            MultiLanguageString uncontracted,
            out List<MultiLanguageString> contracted)
        {
            bool returnValue = true;

            contracted = null;

            List<LanguageTool> languageTools = TargetLanguageTools;

            foreach (LanguageTool languageTool in languageTools)
            {
                List<MultiLanguageString> contracted1 = null;

                if (!languageTool.Contract(
                        uncontracted,
                        out contracted1))
                    returnValue = false;

                if (contracted1 != null)
                {
                    if (contracted != null)
                        contracted.AddRange(contracted1);
                    else
                        contracted = contracted1;
                }
            }

            return returnValue;
        }

        public virtual bool ExpandContraction(
            MultiLanguageString possiblyContracted,
            out List<MultiLanguageString> expanded)
        {
            bool returnValue = true;

            expanded = null;

            List<LanguageTool> languageTools = TargetLanguageTools;

            foreach (LanguageTool languageTool in languageTools)
            {
                List<MultiLanguageString> expanded1 = null;

                if (!languageTool.ExpandContraction(
                        possiblyContracted,
                        out expanded1))
                    returnValue = false;

                if (expanded1 != null)
                {
                    if (expanded != null)
                        expanded.AddRange(expanded1);
                    else
                        expanded = expanded1;
                }
            }

            return returnValue;
        }

        public List<Inflection> MergeInflectionLists(
            LanguageTool languageTool,
            List<Inflection> list1,
            List<Inflection> list2)
        {
            if ((list1 == null) || (list1.Count() == 0))
                return list2;

            if ((list2 == null) || (list2.Count() == 0))
                return list1;

            List<Inflection> mergedInflectionList = new List<Inflection>();
            List<Inflection> bestInflectionList = new List<Inflection>();
            LexicalCategory category = list1.First().Category;

            foreach (Inflection inflection1 in list1)
            {
                Inflection matchedInflection = null;

                bestInflectionList.Clear();

#if true
                string label1 = inflection1.Label;
                List<string> bestLabels2 = GetBestDesignatorMatches(category, inflection1.Label);

                if (bestLabels2 != null)
                {
                    foreach (string label2 in bestLabels2)
                    {
                        Inflection inflection2 = list2.FirstOrDefault(x => x.Label == label2);

                        if (inflection2 == null)
                            continue;

                        bestInflectionList.Add(inflection2);
                    }
                }
#else
                int bestWeight = 0;
                int weight = 0;

                foreach (Inflection inflection2 in list2)
                {
#if true
                    if ((inflection1.Label == "Infinitive Reflexive Singular Second Informal")
                        && (inflection2.Label == "Infinitive Reflexive Singular Second"))
                    {
                        ApplicationData.Global.PutConsoleMessage(inflection1.Label + " + " + inflection2.Label);
                    }
#endif
                    weight = GetInflectionDesignationMatchWeight(inflection1, inflection2);

                    if (weight != 0)
                    {
                        if (weight > bestWeight)
                        {
                            bestInflectionList.Clear();
                            bestInflectionList.Add(inflection2);
                            bestWeight = weight;
                        }
                        else if (weight == bestWeight)
                            bestInflectionList.Add(inflection2);
                    }
                }
#endif

                matchedInflection = inflection1;

                if (bestInflectionList.Count() != 0)
                {
                    foreach (Inflection bestInflection in bestInflectionList)
                        matchedInflection = new Inflection(matchedInflection, bestInflection);
                }

                matchedInflection.Key = inflection1.Key;
                matchedInflection.Designation = new Designator(inflection1.Designation);
                mergedInflectionList.Add(matchedInflection);
            }

            return mergedInflectionList;
        }

        public Inflection MergeInflections(
            Inflection inflection1,
            Inflection inflection2)
        {
            if ((inflection1 == null) && (inflection2 == null))
                return null;

            Inflection mergedInflection = new Inflection(inflection1, inflection2);

            return mergedInflection;
        }

        public List<string> GetBestDesignatorMatches(LexicalCategory category, string label)
        {
            Dictionary<string, List<string>> matchCache;
            List<string> bestLabels = null;
            string key = category.ToString();

            if (_DesignatorMatchCache == null)
                _DesignatorMatchCache = new Dictionary<string, Dictionary<string, List<string>>>();

            if (!_DesignatorMatchCache.TryGetValue(key, out matchCache))
            {
                if (LoadDesignatorMatchCache(category, out matchCache))
                    _DesignatorMatchCache.Add(key, matchCache);
                else
                {
                    if (PrimeDesignatorMatchCache(category, out matchCache))
                    {
                        if (!SaveDesignatorMatchCache(category, matchCache))
                            return null;

                        _DesignatorMatchCache.Add(key, matchCache);
                    }
                }
            }

            if (matchCache == null)
                return bestLabels;

            matchCache.TryGetValue(label, out bestLabels);

            return bestLabels;
        }

        public bool PrimeDesignatorMatchCache(
            LexicalCategory category,
            out Dictionary<string, List<string>> matchCache)
        {
            LanguageTool languageTool1 = TargetLanguageTools.First();
            LanguageTool languageTool2 = HostLanguageTools.First();

            List<Designator> designators1 = languageTool1.GetAllCategoryDesignations(category);
            List<Designator> designators2 = languageTool2.GetAllCategoryDesignations(category);

            matchCache = new Dictionary<string, List<string>>();

            int count1 = designators1.Count();
            int count2 = designators2.Count();
            int index1;
            int index2;

            for (index1 = 0; index1 < count1; index1++)
            {
                Designator designator1 = designators1[index1];
                string label1 = designator1.Label;
                int bestWeight = 0;
                int weight = 0;
                List<string> bestLabels = new List<string>();

                for (index2 = 0; index2 < count2; index2++)
                {
                    Designator designator2 = designators2[index2];
                    string label2 = designator2.Label;

#if false
                    if ((label1 == "Infinitive Reflexive Singular Second Informal")
                        && (label2 == "Infinitive Reflexive Singular Second"))
                    {
                        ApplicationData.Global.PutConsoleMessage(label1 + " + " + label2);
                    }
#endif

                    weight = GetDesignationMatchWeight(designator1, designator2);

                    if (weight != 0)
                    {
                        if (weight > bestWeight)
                        {
                            bestLabels.Clear();
                            bestLabels.Add(label2);
                            bestWeight = weight;
                        }
                        else if (weight == bestWeight)
                            bestLabels.Add(label2);
                    }
                }

                matchCache.Add(label1, bestLabels);
            }

            return true;
        }

        public string ComposeDesignatorMatchCacheFilePath(LexicalCategory category)
        {
            LanguageTool languageTool1 = TargetLanguageTools.First();
            LanguageTool languageTool2 = HostLanguageTools.First();
            LanguageID languageID1 = languageTool1.LanguageID;
            LanguageID languageID2 = languageTool2.LanguageID;
            string languageCode1 = languageID1.SymbolName;
            string languageCode2 = languageID2.SymbolName;
            string categoryCode = category.ToString();
            string fileName = "DesignatorMatchCache_" + languageCode1 + "_" + languageCode2 + "_" + categoryCode + ".txt";
            string filePath = MediaUtilities.ConcatenateFilePath3(
                ApplicationData.LocalDataPath,
                "DesignatorMatch",
                fileName);
            return filePath;
        }

        public bool SaveDesignatorMatchCache(
            LexicalCategory category,
            Dictionary<string, List<string>> matchCache)
        {
            string filePath = ComposeDesignatorMatchCacheFilePath(category);
            List<string> data = new List<string>();

            foreach (KeyValuePair<string, List<string>> kvp in matchCache)
            {
                string line = kvp.Key + ":";

                List<string> labels2 = kvp.Value;
                bool first = true;

                foreach (string label2 in labels2)
                {
                    if (first)
                        line += " " + label2;
                    else
                        line += ", " + label2;
                }

                data.Add(line);
            }

            try
            {
                FileSingleton.DirectoryExistsCheck(filePath);
                FileSingleton.WriteAllLines(filePath, data.ToArray());
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool LoadDesignatorMatchCache(
            LexicalCategory category,
            out Dictionary<string, List<string>> matchCache)
        {
            string filePath = ComposeDesignatorMatchCacheFilePath(category);

            matchCache = new Dictionary<string, List<string>>();

            if (!FileSingleton.Exists(filePath))
                return false;

            string[] data = FileSingleton.ReadAllLines(filePath);

            if (data == null)
                return false;

            foreach (string line in data)
            {
                if (String.IsNullOrEmpty(line))
                    continue;

                string[] major = line.Split(LanguageLookup.Colon);
                List<string> labels2 = new List<string>();

                if (major.Length >= 1)
                {
                    string label1 = major[0].Trim();

                    if (major.Length >= 2)
                    {
                        string rawMinor = major[1].Trim();
                        string[] minor = rawMinor.Split(LanguageLookup.Comma);

                        foreach (string label2 in minor)
                            labels2.Add(label2.Trim());
                    }

                    matchCache.Add(label1, labels2);
                }
            }

            return true;
        }

        public int GetInflectionDesignationMatchWeight(
            Inflection inflection1,
            Inflection inflection2)
        {
            Designator designation1 = inflection1.Designation;
            Designator designation2 = inflection2.Designation;
            int weight;

            if (designation1.Match(designation2))
                weight = 100;
            else
                weight = designation1.GetMatchWeight(designation2);

            return weight;
        }

        public int GetDesignationMatchWeight(
            Designator designation1,
            Designator designation2)
        {
            int weight;

            if (designation1.Match(designation2))
                weight = 100;
            else
                weight = designation1.GetMatchWeight(designation2);

            return weight;
        }

        public bool ExtendHostInflection(Inflection inflection)
        {
            bool returnValue = true;

            foreach (LanguageTool hostTool in _HostLanguageTools)
            {
                if (!hostTool.ExtendInflection(inflection))
                    returnValue = false;
            }

            return returnValue;
        }
    }
}
