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
    public partial class LanguageTool : BaseObjectLanguage
    {
        public virtual bool ExternalConjugatorHasNegative
        {
            get
            {
                return false;
            }
        }

        public virtual bool ExternalConjugatorHasModal
        {
            get
            {
                return false;
            }
        }

        public virtual bool GenerateInflectors(
            InflectorTable inflectorTable,
            InflectorDescription inflectorDescription,
            string word,
            LanguageID languageID,
            Dictionary<string, string> inflectionDictionary,
            bool isIrregular)
        {
            LexicalCategory category;
            string categoryString;
            string classCode;
            string subClassCode;

            //if ((word == "ser") || (word == "SER"))
            //    ApplicationData.Global.PutConsoleMessage(word);

            string stem = GetStemAndClasses(word, languageID, out categoryString, out classCode, out subClassCode);
            bool returnValue;

            // In some case (i.e. Spanish "ir") there is no stem.
            //if (String.IsNullOrEmpty(stem))
            //    throw new Exception("Failed to get stem of: " + word);

            category = GetCategoryFromCategoryString(categoryString);

            if (category == LexicalCategory.Unknown)
                category = inflectorTable.Category;

            if (!isIrregular)
                returnValue = GenerateRegularInflectors(
                    inflectorTable,
                    inflectorDescription,
                    word,
                    languageID,
                    inflectionDictionary,
                    category,
                    categoryString,
                    classCode,
                    subClassCode,
                    stem);
            else
                returnValue = GenerateIrregularInflectors(
                    inflectorTable,
                    inflectorDescription,
                    word,
                    languageID,
                    inflectionDictionary,
                    category,
                    categoryString,
                    classCode,
                    subClassCode,
                    stem);

            return returnValue;
        }

        public virtual bool GenerateRegularInflectors(
            InflectorTable inflectorTable,
            InflectorDescription inflectorDescription,
            string word,
            LanguageID languageID,
            Dictionary<string, string> inflectionDictionary,
            LexicalCategory category,
            string categoryString,
            string classCode,
            string subClassCode,
            string stem)
        {
            bool returnValue = true;

            foreach (InflectorFamily inflectorFamily in inflectorDescription.InflectorFamilies)
            {
                if (!GenerateRegularInflectorFamily(
                        inflectorTable,
                        inflectorDescription,
                        inflectionDictionary,
                        word,
                        languageID,
                        inflectorFamily,
                        category,
                        categoryString,
                        classCode,
                        subClassCode,
                        stem))
                    returnValue = false;
            }

            return returnValue;
        }

        public virtual bool GenerateRegularInflectorFamily(
            InflectorTable inflectorTable,
            InflectorDescription inflectorDescription,
            Dictionary<string, string> inflectionDictionary,
            string word,
            LanguageID languageID,
            InflectorFamily inflectorFamily,
            LexicalCategory category,
            string categoryString,
            string classCode,
            string subClassCode,
            string stem)
        {
            bool returnValue = true;

            switch (inflectorFamily.InflectionSource)
            {
                case "Implicit":
                    returnValue = GenerateRegularInflectorImplicit(
                        inflectorTable,
                        inflectorDescription,
                        word,
                        languageID,
                        inflectorFamily,
                        category,
                        categoryString,
                        classCode,
                        subClassCode,
                        stem);
                    break;
                case "Manual":
                    returnValue = GenerateRegularInflectorManual(
                        inflectorTable,
                        inflectorDescription,
                        word,
                        languageID,
                        inflectorFamily,
                        category,
                        categoryString,
                        classCode,
                        subClassCode,
                        stem);
                    break;
                case "Compound":
                    returnValue = GenerateRegularInflectorCompound(
                        inflectorTable,
                        inflectorDescription,
                        word,
                        languageID,
                        inflectorFamily,
                        category,
                        categoryString,
                        classCode,
                        subClassCode,
                        stem);
                    break;
                case "Inherited":
                    returnValue = GenerateRegularInflectorInherited(
                        inflectorTable,
                        inflectorDescription,
                        inflectionDictionary,
                        word,
                        languageID,
                        inflectorFamily,
                        category,
                        categoryString,
                        classCode,
                        subClassCode,
                        stem);
                    break;
                case "Expanded":
                    returnValue = GenerateRegularInflectorExpanded(
                        inflectorTable,
                        inflectorDescription,
                        inflectionDictionary,
                        word,
                        languageID,
                        inflectorFamily,
                        category,
                        categoryString,
                        classCode,
                        subClassCode,
                        stem);
                    break;
                case "External":
                    returnValue = GenerateRegularInflectorExternal(
                        inflectorTable,
                        inflectorDescription,
                        inflectionDictionary,
                        word,
                        languageID,
                        inflectorFamily,
                        category,
                        categoryString,
                        classCode,
                        subClassCode,
                        stem);
                    break;
                default:
                    throw new Exception("GenerateRegularInflectorFamily: Unexpected inflection source: source=" +
                        inflectorFamily.InflectionSource +
                        "family=" +
                        inflectorFamily.Label);
            }

            return returnValue;
        }

        public virtual bool GenerateRegularInflectorImplicit(
            InflectorTable inflectorTable,
            InflectorDescription inflectorDescription,
            string word,
            LanguageID languageID,
            InflectorFamily inflectorFamily,
            LexicalCategory category,
            string categoryString,
            string classCode,
            string subClassCode,
            string stem)
        {
            throw new Exception("GenerateRegularInflectorImplicit: Implicit source used, but this function not overridden: " +
                    inflectorFamily.Label);
        }

        public virtual bool GenerateRegularInflectorManual(
            InflectorTable inflectorTable,
            InflectorDescription inflectorDescription,
            string word,
            LanguageID languageID,
            InflectorFamily inflectorFamily,
            LexicalCategory category,
            string categoryString,
            string classCode,
            string subClassCode,
            string stem)
        {
            if (inflectorFamily.InflectorCount() == 0)
                throw new Exception("GenerateRegularInflectorManual: Inflector family has no inflectors: " +
                    inflectorFamily.Label);

            if (inflectorTable.HasInflectorFamilyList(inflectorFamily.Label))
                return true;

            InflectorFamily tableInflectorFamily = new InflectorFamily(inflectorFamily);
            inflectorTable.AppendInflectorFamilyList(tableInflectorFamily);

            return true;
        }

        public virtual bool GenerateRegularInflectorCompound(
            InflectorTable inflectorTable,
            InflectorDescription inflectorDescription,
            string word,
            LanguageID languageID,
            InflectorFamily inflectorFamily,
            LexicalCategory category,
            string categoryString,
            string classCode,
            string subClassCode,
            string stem)
        {
            bool returnValue = true;

            if (inflectorFamily.IsIterate())
            {
                bool hasPolarity = inflectorFamily.HasAnyPolarizer() || inflectorFamily.HasClassification("Polarity");
                int polarityCount = (hasPolarity ? 2 : 1);
                List<TokenDescriptor> iterators = inflectorDescription.GetIterateTokens(inflectorFamily.Iterate);

                for (int polarityIndex = 0; polarityIndex < polarityCount; polarityIndex++)
                {
                    foreach (TokenDescriptor iteratorDescriptor in iterators)
                    {
                        if (!GenerateRegularInflectorCompoundWithIterator(
                                inflectorTable,
                                inflectorDescription,
                                word,
                                languageID,
                                inflectorFamily,
                                iteratorDescriptor,
                                category,
                                categoryString,
                                classCode,
                                subClassCode,
                                stem,
                                hasPolarity,
                                (polarityIndex == 1 ? true : false)))
                            returnValue = false;
                    }
                }
            }
            else
            {
                if (!GenerateRegularInflectorCompoundNoPronouns(
                        inflectorTable,
                        inflectorDescription,
                        word,
                        languageID,
                        inflectorFamily,
                        category,
                        categoryString,
                        classCode,
                        subClassCode,
                        stem))
                    returnValue = false;
            }

            return returnValue;
        }

        public virtual bool GenerateRegularInflectorCompoundNoPronouns(
            InflectorTable inflectorTable,
            InflectorDescription inflectorDescription,
            string word,
            LanguageID languageID,
            InflectorFamily inflectorFamily,
            LexicalCategory category,
            string categoryString,
            string classCode,
            string subClassCode,
            string stem)
        {
            string familyLabel = inflectorFamily.Label;
            string label;
            Designator designator;
            InflectorFamily tableInflectorFamily;
            Inflector inflector = null;
            List<LiteralString> iteratorStrings = null;
            Modifier modifier = null;
            List<Modifier> modifiers = null;

            label = familyLabel;
            designator = new Designator(inflectorFamily);

            tableInflectorFamily = inflectorTable.GetInflectorFamilyList(familyLabel);

            if (tableInflectorFamily != null)
                inflector = tableInflectorFamily.GetInflector(label);

            if (inflector == null)
            {
                if (!CreateModifierFromPattern(
                        inflectorDescription,
                        inflectorTable,
                        inflectorFamily,
                        null,
                        category,
                        classCode,
                        subClassCode,
                        stem,
                        inflectorFamily.Pattern,
                        false,
                        false,
                        out modifier))
                    return false;

                modifiers = new List<Modifier>() { modifier };

                inflector = new Inflector(
                    label,
                    designator.Classifications,
                    "All",
                    iteratorStrings,
                    modifiers);

                if (tableInflectorFamily == null)
                {
                    tableInflectorFamily = new InflectorFamily(inflectorFamily);
                    inflectorTable.AppendInflectorFamilyList(tableInflectorFamily);
                }

                tableInflectorFamily.AppendInflector(inflector);
                // Have to add non-saved inflector too.
                inflectorTable.AppendInflector(inflector);
            }
            else
            {
                modifier = inflector.GetModifier(classCode, subClassCode);

                if (modifier != null)
                {
                    if (!VerifyModifierFromPattern(
                            inflectorDescription,
                            inflectorTable,
                            inflectorFamily,
                            null,
                            category,
                            classCode,
                            subClassCode,
                            stem,
                            inflectorFamily.Pattern,
                            false,
                            false,
                            modifier))
                        return false;
                }
                else
                {
                    if (!CreateModifierFromPattern(
                            inflectorDescription,
                            inflectorTable,
                            inflectorFamily,
                            null,
                            category,
                            classCode,
                            subClassCode,
                            stem,
                            inflectorFamily.Pattern,
                            false,
                            false,
                            out modifier))
                        return false;

                    inflector.AppendModifier(modifier);
                }
            }

            return true;
        }

        public virtual bool GenerateRegularInflectorCompoundWithIterator(
            InflectorTable inflectorTable,
            InflectorDescription inflectorDescription,
            string word,
            LanguageID languageID,
            InflectorFamily inflectorFamily,
            TokenDescriptor iteratorDescriptor,
            LexicalCategory category,
            string categoryString,
            string classCode,
            string subClassCode,
            string stem,
            bool hasPolarity,
            bool isNegative)
        {
            string familyLabel = inflectorFamily.Label;
            string label;
            string subLabel;
            Designator designator;
            InflectorFamily tableInflectorFamily;
            Inflector inflector = null;
            List<LiteralString> iteratorStrings = null;
            Modifier modifier = null;
            List<Modifier> modifiers = null;

            foreach (Designator subDesignator in iteratorDescriptor.Designators)
            {
                if (inflectorFamily.HasExclude(subDesignator))
                    continue;

                Designator familyDesignator;
                Designator nonPolarizedDesignator = new Designator(inflectorFamily, subDesignator, Designator.CombineCode.Union);

                nonPolarizedDesignator.DeleteFirstClassification("Polarity");
                nonPolarizedDesignator.DefaultLabel();

                string nonPolarizedLabel = nonPolarizedDesignator.Label;

                if (hasPolarity)
                {
                    if (isNegative)
                    {
                        familyDesignator = new Designator(inflectorFamily);
                        familyDesignator.AppendClassification("Polarity", "Negative");
                    }
                    else
                    {
                        familyDesignator = new Designator(inflectorFamily);
                        familyDesignator.AppendClassification("Polarity", "Positive");
                    }
                }
                else
                    familyDesignator = inflectorFamily;

                designator = new Designator(familyDesignator, subDesignator, Designator.CombineCode.Union);
                label = designator.Label;
                subLabel = subDesignator.Label;

                tableInflectorFamily = inflectorTable.GetInflectorFamilyList(familyLabel);

                if (tableInflectorFamily != null)
                    inflector = tableInflectorFamily.GetInflector(label);
                else
                    inflector = null;

                if (inflector == null)
                {
                    if (inflectorFamily.IsIterate())
                        iteratorStrings = new List<LiteralString>() { iteratorDescriptor.Text };
                    else
                        iteratorStrings = null;

                    if (!CreateModifierFromPattern(
                            inflectorDescription,
                            inflectorTable,
                            inflectorFamily,
                            subLabel,
                            category,
                            classCode,
                            subClassCode,
                            stem,
                            inflectorFamily.Pattern,
                            hasPolarity,
                            isNegative,
                            out modifier))
                        return false;

                    modifiers = new List<Modifier>() { modifier };

                    inflector = new Inflector(
                        label,
                        designator.Classifications,
                        "All",
                        iteratorStrings,
                        modifiers);

                    if (tableInflectorFamily == null)
                    {
                        tableInflectorFamily = new InflectorFamily(inflectorFamily);
                        inflectorTable.AppendInflectorFamilyList(tableInflectorFamily);
                    }

                    tableInflectorFamily.AppendInflector(inflector);
                    // Have to add non-saved inflector too.
                    inflectorTable.AppendInflector(inflector);
                }
                else
                {
                    if (!inflector.HasPronoun(iteratorDescriptor.Text))
                        inflector.AppendPronoun(iteratorDescriptor.Text);

                    modifier = inflector.GetModifier(classCode, subClassCode);

                    if (modifier != null)
                    {
                        if (!VerifyModifierFromPattern(
                                inflectorDescription,
                                inflectorTable,
                                inflectorFamily,
                                subLabel,
                                category,
                                classCode,
                                subClassCode,
                                stem,
                                inflectorFamily.Pattern,
                                hasPolarity,
                                isNegative,
                                modifier))
                            return false;
                    }
                    else
                    {
                        if (!CreateModifierFromPattern(
                                inflectorDescription,
                                inflectorTable,
                                inflectorFamily,
                                subLabel,
                                category,
                                classCode,
                                subClassCode,
                                stem,
                                inflectorFamily.Pattern,
                                hasPolarity,
                                isNegative,
                                out modifier))
                            return false;

                        inflector.AppendModifier(modifier);
                    }
                }
            }

            return true;
        }

        public virtual bool GenerateRegularInflectorInherited(
            InflectorTable inflectorTable,
            InflectorDescription inflectorDescription,
            Dictionary<string, string> inflectionDictionary,
            string word,
            LanguageID languageID,
            InflectorFamily inflectorFamily,
            LexicalCategory category,
            string categoryString,
            string classCode,
            string subClassCode,
            string stem)
        {
            bool returnValue = GenerateRegularInflectorExternal(
                inflectorTable,
                inflectorDescription,
                inflectionDictionary,
                word,
                languageID,
                inflectorFamily,
                category,
                categoryString,
                classCode,
                subClassCode,
                stem);
            return returnValue;
        }

        public virtual bool GenerateRegularInflectorExpanded(
            InflectorTable inflectorTable,
            InflectorDescription inflectorDescription,
            Dictionary<string, string> inflectionDictionary,
            string word,
            LanguageID languageID,
            InflectorFamily inflectorFamily,
            LexicalCategory category,
            string categoryString,
            string classCode,
            string subClassCode,
            string stem)
        {
            bool returnValue = true;

            if (inflectorFamily.ExpandLabels == null)
                throw new Exception("Expanded inflector family has no \"Expand\" elements.");

            foreach (string expandLabel in inflectorFamily.ExpandLabels)
            {
                if (TextUtilities.IsWildCardPattern(expandLabel))
                {
                    int inflectorFamilyCount = inflectorDescription.InflectorFamilyCount();
                    int inflectorFamilyIndex;
                    int count = 0;

                    for (inflectorFamilyIndex = 0; inflectorFamilyIndex < inflectorFamilyCount; inflectorFamilyIndex++)
                    {
                        InflectorFamily inheritFamily = inflectorDescription.GetInflectorFamilyIndexed(inflectorFamilyIndex);

                        if (!TextUtilities.IsWildCardTextMatch(expandLabel, inheritFamily.Label, false))
                            continue;

                        InflectorFamily expandedFamily = new InflectorFamily(inflectorFamily);

                        expandedFamily.InsertClassifications(inheritFamily);
                        expandedFamily.InflectionSource = "Inherit";
                        expandedFamily.Inherit = inheritFamily.Label;
                        expandedFamily.DefaultLabel();

                        bool result = GenerateRegularInflectorExternal(
                            inflectorTable,
                            inflectorDescription,
                            inflectionDictionary,
                            word,
                            languageID,
                            expandedFamily,
                            category,
                            categoryString,
                            classCode,
                            subClassCode,
                            stem);

                        if (!result)
                            returnValue = false;

                        count++;
                    }

                    if (count == 0)
                        throw new Exception("Mp onherited families found for \"expandLabel\".");
                }
                else
                {
                    InflectorFamily inheritFamily = inflectorTable.GetInflectorFamilyList(expandLabel);

                    if (inheritFamily == null)
                        throw new Exception("Inherited family \"expandLabel\" not found.");

                    InflectorFamily expandedFamily = new InflectorFamily(inflectorFamily);

                    expandedFamily.InsertClassifications(inheritFamily);
                    expandedFamily.InflectionSource = "Inherit";
                    expandedFamily.Inherit = expandLabel;
                    expandedFamily.DefaultLabel();

                    bool result = GenerateRegularInflectorExternal(
                        inflectorTable,
                        inflectorDescription,
                        inflectionDictionary,
                        word,
                        languageID,
                        expandedFamily,
                        category,
                        categoryString,
                        classCode,
                        subClassCode,
                        stem);

                    if (!result)
                        returnValue = false;
                }
            }

            return returnValue;
        }

        public virtual bool GenerateRegularInflectorExternal(
            InflectorTable inflectorTable,
            InflectorDescription inflectorDescription,
            Dictionary<string, string> inflectionDictionary,
            string word,
            LanguageID languageID,
            InflectorFamily inflectorFamily,
            LexicalCategory category,
            string categoryString,
            string classCode,
            string subClassCode,
            string stem)
        {
            bool returnValue = true;

            if (inflectorFamily.IsIterate())
            {
                bool hasPolarity = false;
                int polarityCount = 1;

                if (!inflectorFamily.HasClassification("Polarity"))
                {
                    hasPolarity = inflectorFamily.HasAnyPolarizer();
                    polarityCount = (hasPolarity ? 2 : 1);
                }

                List<TokenDescriptor> iterators = inflectorDescription.GetIterateTokens(inflectorFamily.Iterate);

                for (int polarityIndex = 0; polarityIndex < polarityCount; polarityIndex++)
                {
                    foreach (TokenDescriptor iteratorDescriptor in iterators)
                    {
                        if (!GenerateRegularInflectorExternalWithIterator(
                                inflectorTable,
                                inflectorDescription,
                                inflectionDictionary,
                                word,
                                languageID,
                                inflectorFamily,
                                iteratorDescriptor,
                                category,
                                categoryString,
                                classCode,
                                subClassCode,
                                stem,
                                hasPolarity,
                                (polarityIndex == 1 ? true : false)))
                            returnValue = false;
                    }
                }
            }
            else
            {
                if (!GenerateRegularInflectorExternalNoPronouns(
                        inflectorTable,
                        inflectorDescription,
                        inflectionDictionary,
                        word,
                        languageID,
                        inflectorFamily,
                        category,
                        categoryString,
                        classCode,
                        subClassCode,
                        stem))
                    returnValue = false;
            }

            return returnValue;
        }

        public virtual bool GenerateRegularInflectorExternalNoPronouns(
            InflectorTable inflectorTable,
            InflectorDescription inflectorDescription,
            Dictionary<string, string> inflectionDictionary,
            string word,
            LanguageID languageID,
            InflectorFamily inflectorFamily,
            LexicalCategory category,
            string categoryString,
            string classCode,
            string subClassCode,
            string stem)
        {
            string familyLabel = inflectorFamily.Label;
            string label;
            string inflectionValue;
            Designator designator;
            InflectorFamily tableInflectorFamily;
            Inflector inflector = null;
            List<LiteralString> iteratorStrings = null;
            Modifier modifier = null;
            List<Modifier> modifiers = null;
            List<BaseString> components = null;

            label = familyLabel;
            designator = new Designator(inflectorFamily);

            tableInflectorFamily = inflectorTable.GetInflectorFamilyList(familyLabel);

            if (!inflectionDictionary.TryGetValue(familyLabel, out inflectionValue))
            {
                if (inflectorFamily.IsInherit())
                {
                    if (!inflectionDictionary.TryGetValue(inflectorFamily.Inherit, out inflectionValue))
                    {
                        string parentLabel = inflectorFamily.Inherit;

                        if (!inflectionDictionary.TryGetValue(parentLabel, out inflectionValue))
                        {
                            string message = "GenerateRegularInflectorExternalNoPronouns: Inherited inflectionValue not found." +
                                " label=" + label + " word=" + word;
                            ApplicationData.Global.PutConsoleMessage(message);
                            return true;
                        }
                    }
                }
                else if (inflectorFamily.InflectorCount() == 0)
                {
                    string message = "GenerateRegularInflectorExternalNoPronouns: inflectionValue not found." +
                        " label=" + label + " word=" + word;
                    ApplicationData.Global.PutConsoleMessage(message);
                    return true;
                }
                else if (tableInflectorFamily == null)
                {
                    tableInflectorFamily = new InflectorFamily(inflectorFamily);
                    inflectorTable.AppendInflectorFamilyList(tableInflectorFamily);
                    return true;
                }
                else
                    return true;
            }

            string[] multipleParts = inflectionValue.Split(LanguageLookup.Comma);
            int instanceCount = multipleParts.Length;

            for (int instanceIndex = 0; instanceIndex < instanceCount; instanceIndex++)
            {
                string instanceValue = multipleParts[instanceIndex];
                Designator instanceDesignator;

                if (instanceIndex == 0)
                    instanceDesignator = designator;
                else
                {
                    instanceDesignator = new Designator(designator);
                    instanceDesignator.AppendClassification("Alternate", "Alternate" + instanceIndex.ToString());
                    instanceDesignator.DefaultLabel();
                }

                label = instanceDesignator.Label;

                if (tableInflectorFamily != null)
                    inflector = tableInflectorFamily.GetInflector(label);

                if (!ParseExternalPatternComponents(
                        instanceValue,
                        inflectorFamily.ExternalPattern,
                        inflectorFamily.AlternateExternalPattern,
                        out components))
                    return false;

                if (inflector == null)
                {
                    if (!CreateModifierFromComponents(
                            category,
                            classCode,
                            subClassCode,
                            stem,
                            components,
                            null,
                            inflectorFamily.Pattern,
                            false,
                            false,
                            out modifier))
                        return false;

                    modifiers = new List<Modifier>() { modifier };

                    inflector = new Inflector(
                        label,
                        instanceDesignator.Classifications,
                        "All",
                        iteratorStrings,
                        modifiers);

                    if (tableInflectorFamily == null)
                    {
                        tableInflectorFamily = new InflectorFamily(inflectorFamily);
                        inflectorTable.AppendInflectorFamilyList(tableInflectorFamily);
                    }

                    if (instanceIndex == 0)
                    {
                        tableInflectorFamily.AppendInflector(inflector);
                        // Have to add non-saved inflector too.
                        inflectorTable.AppendInflector(inflector);
                    }
                    else
                    {
                        tableInflectorFamily.InsertAlternateInflector(inflector, designator);
                        // Have to add non-saved inflector too.
                        inflectorTable.InsertAlternateInflector(inflector, designator);
                    }
                }
                else
                {
                    modifier = inflector.GetModifier(classCode, subClassCode);

                    if (modifier != null)
                    {
                        if (!VerifyModifierFromComponents(
                                category,
                                classCode,
                                subClassCode,
                                stem,
                                components,
                                null,
                                inflectorFamily.Pattern,
                                false,
                                false,
                                modifier))
                            return false;
                    }
                    else
                    {
                        if (!CreateModifierFromComponents(
                                category,
                                classCode,
                                subClassCode,
                                stem,
                                components,
                                null,
                                inflectorFamily.Pattern,
                                false,
                                false,
                                out modifier))
                            return false;

                        inflector.AppendModifier(modifier);
                    }
                }
            }

            return true;
        }

        public virtual bool GenerateRegularInflectorExternalWithIterator(
            InflectorTable inflectorTable,
            InflectorDescription inflectorDescription,
            Dictionary<string, string> inflectionDictionary,
            string word,
            LanguageID languageID,
            InflectorFamily inflectorFamily,
            TokenDescriptor iteratorDescriptor,
            LexicalCategory category,
            string categoryString,
            string classCode,
            string subClassCode,
            string stem,
            bool hasPolarity,
            bool isNegative)
        {
            string familyLabel = inflectorFamily.Label;
            string label;
            string inflectionValue;
            Designator designator;
            InflectorFamily tableInflectorFamily;
            Inflector inflector = null;
            List<LiteralString> iteratorStrings = null;
            Modifier modifier = null;
            List<Modifier> modifiers = null;
            List<BaseString> components = null;

            tableInflectorFamily = inflectorTable.GetInflectorFamilyList(familyLabel);

            foreach (Designator subDesignator in iteratorDescriptor.Designators)
            {
                if (inflectorFamily.HasExclude(subDesignator))
                    continue;

                Designator familyDesignator;
                Designator nonPolarizedDesignator;

                nonPolarizedDesignator = new Designator(inflectorFamily, subDesignator, Designator.CombineCode.Union);

                nonPolarizedDesignator.DeleteFirstClassification("Polarity");
                nonPolarizedDesignator.DefaultLabel();

                string nonPolarizedLabel = nonPolarizedDesignator.Label;

                if (hasPolarity)
                {
                    if (isNegative)
                    {
                        familyDesignator = new Designator(inflectorFamily);

                        if (inflectorFamily.HasClassification("Polarity"))
                            inflectorFamily.DeleteFirstClassification("Polarity");

                        familyDesignator.AppendClassification("Polarity", "Negative");
                    }
                    else
                    {
                        familyDesignator = new Designator(inflectorFamily);

                        if (inflectorFamily.HasClassification("Polarity"))
                            inflectorFamily.DeleteFirstClassification("Polarity");

                        familyDesignator.AppendClassification("Polarity", "Positive");
                    }
                }
                else
                    familyDesignator = inflectorFamily;

                designator = new Designator(familyDesignator, subDesignator, Designator.CombineCode.Union);
                string fullLabel = designator.Label;

                if (!inflectionDictionary.TryGetValue(nonPolarizedLabel, out inflectionValue))
                {
                    if (!inflectionDictionary.TryGetValue(fullLabel, out inflectionValue))
                    {
                        if (inflectorFamily.IsInherit())
                        {
                            if (!inflectionDictionary.TryGetValue(inflectorFamily.Inherit, out inflectionValue))
                            {
                                string parentLabel = fullLabel.Replace(familyLabel, inflectorFamily.Inherit);

                                if (!inflectionDictionary.TryGetValue(parentLabel, out inflectionValue))
                                {
                                    parentLabel = TextUtilities.RemoveFirstWord(fullLabel);

                                    if (!inflectionDictionary.TryGetValue(parentLabel, out inflectionValue))
                                    {
                                        string message = "GenerateRegularInflectorExternalWithIterator: Inherited inflectionValue not found." +
                                            " parentLabel=" + parentLabel + " word=" + word;
                                        ApplicationData.Global.PutConsoleMessage(message);
                                        continue;
                                    }
                                }
                            }
                        }
                        else if (inflectorFamily.InflectorCount() == 0)
                        {
                            string message = "GenerateRegularInflectorExternalWithIterator: inflectionValue not found. familyLabel=" + familyLabel +
                                " nonPolarizedLabel=" + nonPolarizedLabel + " word=" + word;
                            ApplicationData.Global.PutConsoleMessage(message);
                            continue;
                        }
                        else if (tableInflectorFamily == null)
                        {
                            tableInflectorFamily = new InflectorFamily(inflectorFamily);
                            inflectorTable.AppendInflectorFamilyList(tableInflectorFamily);
                            continue;
                        }
                        else
                            continue;
                    }
                }

                string[] multipleParts = inflectionValue.Split(LanguageLookup.Comma);
                int instanceCount = multipleParts.Length;

                for (int instanceIndex = 0; instanceIndex < instanceCount; instanceIndex++)
                {
                    string instanceValue = multipleParts[instanceIndex];
                    Designator instanceDesignator;

                    if (instanceIndex == 0)
                        instanceDesignator = designator;
                    else
                    {
                        instanceDesignator = new Designator(designator);
                        instanceDesignator.AppendClassification("Alternate", "Alternate" + instanceIndex.ToString());
                        instanceDesignator.DefaultLabel();
                    }

                    label = instanceDesignator.Label;

                    if (tableInflectorFamily != null)
                        inflector = tableInflectorFamily.GetInflector(label);
                    else
                        inflector = null;

                    if (!ParseExternalPatternComponents(
                            instanceValue,
                            inflectorFamily.ExternalPattern,
                            inflectorFamily.AlternateExternalPattern,
                            out components))
                        return false;

                    if (inflectorFamily.IsIterate())
                        iteratorStrings = new List<LiteralString>() { iteratorDescriptor.Text };
                    else
                        iteratorStrings = null;

                    if (inflector == null)
                    {
                        if (!CreateModifierFromComponents(
                                category,
                                classCode,
                                subClassCode,
                                stem,
                                components,
                                iteratorStrings,
                                inflectorFamily.Pattern,
                                hasPolarity,
                                isNegative,
                                out modifier))
                            return false;

                        modifiers = new List<Modifier>() { modifier };

                        inflector = new Inflector(
                            label,
                            instanceDesignator.Classifications,
                            "All",
                            (inflectorFamily.Iterate == "SubjectPronouns" ? iteratorStrings : null),
                            modifiers);

                        if (tableInflectorFamily == null)
                        {
                            tableInflectorFamily = new InflectorFamily(inflectorFamily);
                            inflectorTable.AppendInflectorFamilyList(tableInflectorFamily);
                        }

                        if (instanceIndex == 0)
                        {
                            tableInflectorFamily.AppendInflector(inflector);
                            // Have to add non-saved inflector too.
                            inflectorTable.AppendInflector(inflector);
                        }
                        else
                        {
                            tableInflectorFamily.InsertAlternateInflector(inflector, designator);
                            // Have to add non-saved inflector too.
                            inflectorTable.InsertAlternateInflector(inflector, designator);
                        }
                    }
                    else
                    {
                        if (inflectorFamily.Iterate == "SubjectPronouns")
                        {
                            if (inflectorFamily.HasAnyPronoun() && !inflector.HasPronoun(iteratorDescriptor.Text))
                                inflector.AppendPronoun(iteratorDescriptor.Text);
                        }

                        modifier = inflector.GetModifier(classCode, subClassCode);

                        if (modifier != null)
                        {
                            if (!VerifyModifierFromComponents(
                                    category,
                                    classCode,
                                    subClassCode,
                                    stem,
                                    components,
                                    iteratorStrings,
                                    inflectorFamily.Pattern,
                                    hasPolarity,
                                    isNegative,
                                    modifier))
                                return false;
                        }
                        else
                        {
                            if (!CreateModifierFromComponents(
                                    category,
                                    classCode,
                                    subClassCode,
                                    stem,
                                    components,
                                    iteratorStrings,
                                    inflectorFamily.Pattern,
                                    hasPolarity,
                                    isNegative,
                                    out modifier))
                                return false;

                            inflector.AppendModifier(modifier);
                        }
                    }
                }
            }

            return true;
        }

        private static string[] NumberClassifiers =
        {
            "Singular",
            "Plural"
        };

        private static string[] GenderClassifiers =
        {
            "Masculine",
            "Feminine"
        };

        private static string[] NumberGenderClassifiers =
        {
            "Singular",
            "Plural",
            "Masculine",
            "Feminine"
        };

        // This assumes the inflector table is already populated with regular inflectors.
        public virtual bool GenerateIrregularInflectors(
            InflectorTable inflectorTable,
            InflectorDescription inflectorDescription,
            string word,
            LanguageID languageID,
            Dictionary<string, string> inflectionDictionary,
            LexicalCategory category,
            string categoryString,
            string classCode,
            string subClassCode,
            string stem)
        {
            //if (word == "dire")
            //    ApplicationData.Global.PutConsoleMessage("dire");

            foreach (InflectorFamily inflectorFamily in inflectorTable.InflectorFamilyList)
            {
                foreach (Inflector inflector in inflectorFamily.Inflectors)
                {
                    GenerateIrregularFamilyInflector(
                        inflectorTable,
                        inflectorDescription,
                        word,
                        languageID,
                        inflectionDictionary,
                        category,
                        categoryString,
                        classCode,
                        subClassCode,
                        stem,
                        inflectorFamily,
                        inflector);
                }
            }

            return true;
        }

        public virtual bool GenerateIrregularFamilyInflector(
            InflectorTable inflectorTable,
            InflectorDescription inflectorDescription,
            string word,
            LanguageID languageID,
            Dictionary<string, string> inflectionDictionary,
            LexicalCategory category,
            string categoryString,
            string classCode,
            string subClassCode,
            string stem,
            InflectorFamily inflectorFamily,
            Inflector inflector)
        {
            string familyLabel = inflectorFamily.Label;
            string label = inflector.Label;
            string externalLabel = label;
            string inflectionValue;

            //if ((word == "ser") && label.StartsWith("Indicative Present Positive Singular First"))
            //    ApplicationData.Global.PutConsoleMessage("soy " + label);

            if (!inflectionDictionary.TryGetValue(externalLabel, out inflectionValue))
            {
                string nonPolarizedLabel = externalLabel.Replace(" Positive", "").Replace(" Negative", "").Replace(" Alternate1", "").Replace(" Alternate2", "");

                if (!inflectionDictionary.TryGetValue(nonPolarizedLabel, out inflectionValue))
                {
                    if (inflectorFamily.IsInherit())
                    {
                        if (!inflectionDictionary.TryGetValue(inflectorFamily.Inherit, out inflectionValue))
                        {
                            string parentLabel = nonPolarizedLabel.Replace(familyLabel, inflectorFamily.Inherit);

                            if (!inflectionDictionary.TryGetValue(parentLabel, out inflectionValue))
                                return false;
                        }
                    }
                    else
                        return false;
                }
            }

            string[] partsMultiple = inflectionValue.Split(LanguageLookup.Comma);
            int multipleCount = partsMultiple.Length;
            int multipleIndex;

            if (label.Contains("Alternate1"))
                multipleIndex = 1;
            else if (label.Contains("alternate2"))
                multipleIndex = 2;
            else
                multipleIndex = 0;

            if (multipleIndex >= multipleCount)
                return false;

            string str = partsMultiple[multipleIndex];
            string[] partsPipe = str.Split(LanguageLookup.Bar);
            string irregularInflection = partsPipe[0];
            string prefix;
            string suffix;
            string regularFullInflection;
            string regularInflection;
            bool returnFlag = GetQuickInflectionParts(
                word,
                label,
                inflectorTable,
                classCode,
                subClassCode,
                inflector,
                out regularFullInflection,
                out regularInflection,
                out prefix,
                out stem,
                out suffix);

            if (!returnFlag)
                return false;

            if (irregularInflection == regularInflection)
                return true;

            string externalConjugation = irregularInflection;
            string externalPattern;
            List<BaseString> externalComponents;

            if (inflectorFamily.HasExternalPattern)
                externalPattern = inflectorFamily.ExternalPattern;
            else
                externalPattern = "%m";

            if (!ParseExternalPatternComponents(
                externalConjugation,
                externalPattern,
                inflectorFamily.AlternateExternalPattern,
                out externalComponents))
            {
                throw new Exception("Error parsing external conjugation \"" + externalConjugation + "\" for \"" + label + "\".");
            }

            if (!String.IsNullOrEmpty(inflectorFamily.Iterate))
            {
                WordToken mainToken = inflectorFamily.Pattern.FirstOrDefault(x => x.Type == "Main");

                if (mainToken.Operations != null)
                {
                    BaseString mainComponent = externalComponents.FirstOrDefault(x => x.KeyString == "Main");
                    List<TokenDescriptor> iterators = inflectorDescription.GetIterateTokens(inflectorFamily.Iterate);
                    TokenDescriptor iterator = iterators.FirstOrDefault(x => x.Designators.FirstOrDefault(y => label.Contains(y.Label)) != null);

                    if (iterator == null)
                        throw new Exception("GenerateIrregularFamilyInflector: Iterator not found.");

                    mainComponent.Text = InflectFromPattern(
                        mainComponent.Text,
                        mainToken.Operations,
                        iterator);
                }
            }

            if (!GenerateIrregularInflector(
                    inflectorTable,
                    inflectorDescription,
                    word,
                    languageID,
                    inflectionDictionary,
                    category,
                    categoryString,
                    classCode,
                    subClassCode,
                    prefix,
                    stem,
                    suffix,
                    label,
                    inflectorFamily,
                    inflector,
                    regularFullInflection,
                    regularInflection,
                    externalComponents))
                return false;

            return true;
        }

        // This assumes the inflector table is already populated with regular inflectors.
        public virtual bool GenerateIrregularInflectorsOld(
            InflectorTable inflectorTable,
            InflectorDescription inflectorDescription,
            string word,
            LanguageID languageID,
            Dictionary<string, string> inflectionDictionary,
            LexicalCategory category,
            string categoryString,
            string classCode,
            string subClassCode,
            string stem)
        {
            if (word == "dire")
                ApplicationData.Global.PutConsoleMessage("dire");

            foreach (KeyValuePair<string, string> kvp in inflectionDictionary)
            {
                string label = kvp.Key;
                string externalLabel = label;
                string raw = kvp.Value;

                //if ((DesignatorBreakpoint != "None") && label.StartsWith(DesignatorBreakpoint))
                //    ApplicationData.Global.PutConsoleMessage(label);

                //if (!raw.Contains("|irregular"))
                //    continue;

                string[] partsMultiple = raw.Split(LanguageLookup.Comma);
                // Ignore alternats for now.
                //int multipleCount = partsMultiple.Length;
                int multipleCount = 1;
                int multipleIndex;

                for (multipleIndex = 0; multipleIndex < multipleCount; multipleIndex++)
                {
                    string str = partsMultiple[multipleIndex];
                    string[] partsPipe = str.Split(LanguageLookup.Bar);
                    string irregularInflection = partsPipe[0];
                    string prefix;
                    string suffix;
                    string regularFullInflection;
                    string regularInflection;
                    Inflector inflector;
                    bool returnFlag = GetQuickInflectionAndParts(
                        word,
                        label,
                        inflectorTable,
                        classCode,
                        subClassCode,
                        out regularFullInflection,
                        out regularInflection,
                        out prefix,
                        out stem,
                        out suffix,
                        out inflector);

                    // Try adding in "Positive".
                    if (!returnFlag)
                    {
                        string testLabel = label.Replace("Singular", "Positive Singular").Replace("Plural", "Positive Plural");
                        returnFlag = GetQuickInflectionAndParts(
                            word,
                            testLabel,
                            inflectorTable,
                            classCode,
                            subClassCode,
                            out regularFullInflection,
                            out regularInflection,
                            out prefix,
                            out stem,
                            out suffix,
                            out inflector);
                        if (returnFlag)
                            label = testLabel;
                    }

                    // Try adding in Number+Gender (for latin past participles).
                    if (!returnFlag && !TextUtilities.ContainsWholeWordFromList(label, NumberGenderClassifiers))
                    {
                        string testLabel = label + " Singular Masculine";
                        returnFlag = GetQuickInflectionAndParts(
                            word,
                            testLabel,
                            inflectorTable,
                            classCode,
                            subClassCode,
                            out regularFullInflection,
                            out regularInflection,
                            out prefix,
                            out stem,
                            out suffix,
                            out inflector);
                        if (returnFlag)
                            label = testLabel;
                    }

                    if (!returnFlag)
                    {
                        continue;
                        //throw new Exception("GenerateIrregularInflectors: Failed to get regular inflection: word=" +
                        //    word +
                        //    " label=" + label);
                    }

                    InflectorFamily inflectorFamily = inflectorTable.GetInflectorFamilyFromInflectorLabel(label);

                    if (inflectorFamily == null)
                    {
                        throw new Exception("GenerateIrregularInflectors: Failed to get inflection family: word=" +
                            word +
                            " label=" + label);
                    }

                    if (irregularInflection == regularInflection)
                    {
                        continue;
                        //throw new Exception("GenerateIrregularInflectors: Irregular not different from regular: irregularInflection=" +
                        //    irregularInflection);
                    }

                    string externalConjugation = irregularInflection;
                    string externalPattern;
                    List<BaseString> externalComponents;

                    if (inflectorFamily.HasExternalPattern)
                        externalPattern = inflectorFamily.ExternalPattern;
                    else
                        externalPattern = "%m";

                    if (inflectorFamily.IterateIrregulars)
                    {
                        List<Inflector> inflectors = inflectorFamily.Inflectors;
                        int inflectorCount = inflectors.Count();
                        int inflectorIndex;

                        for (inflectorIndex = 0; inflectorIndex < inflectorCount; inflectorIndex++)
                        {
                            Inflector inflectorIterator = inflectors[inflectorIndex];

                            label = inflectorIterator.Label;

                            if (!ParseExternalPatternComponents(
                                externalConjugation,
                                externalPattern,
                                inflectorFamily.AlternateExternalPattern,
                                out externalComponents))
                            {
                                throw new Exception("Error parsing external conjugation \"" + externalConjugation + "\" for \"" + label + "\".");
                            }

                            BaseString mainComponent = externalComponents.FirstOrDefault(x => x.KeyString == "Main");
                            WordToken mainToken = inflectorFamily.Pattern.FirstOrDefault(x => x.Type == "Main");
                            TokenDescriptor iterator = inflectorDescription.GetIterateTokens(inflectorFamily.Iterate)[inflectorIndex];

                            mainComponent.Text = InflectFromPattern(
                                mainComponent.Text,
                                mainToken.Operations,
                                iterator);

                            GetQuickInflectionParts(
                                word,
                                label,
                                inflectorTable,
                                classCode,
                                subClassCode,
                                inflectorIterator,
                                out regularFullInflection,
                                out regularInflection,
                                out prefix,
                                out stem,
                                out suffix);

                            if (!GenerateIrregularInflector(
                                    inflectorTable,
                                    inflectorDescription,
                                    word,
                                    languageID,
                                    inflectionDictionary,
                                    category,
                                    categoryString,
                                    classCode,
                                    subClassCode,
                                    prefix,
                                    stem,
                                    suffix,
                                    inflectorIterator.Label,
                                    inflectorFamily,
                                    inflectorIterator,
                                    regularFullInflection,
                                    regularInflection,
                                    externalComponents))
                                return false;
                        }
                    }
                    else
                    {
                        if (!ParseExternalPatternComponents(
                            externalConjugation,
                            externalPattern,
                            inflectorFamily.AlternateExternalPattern,
                            out externalComponents))
                        {
                            throw new Exception("Error parsing external conjugation \"" + externalConjugation + "\" for \"" + label + "\".");
                        }

                        if (!GenerateIrregularInflector(
                                inflectorTable,
                                inflectorDescription,
                                word,
                                languageID,
                                inflectionDictionary,
                                category,
                                categoryString,
                                classCode,
                                subClassCode,
                                prefix,
                                stem,
                                suffix,
                                label,
                                inflectorFamily,
                                inflector,
                                regularFullInflection,
                                regularInflection,
                                externalComponents))
                            return false;
                    }
                }
            }

            return true;
        }

        // This assumes the inflector table is already populated with regular inflectors.
        public virtual bool GenerateIrregularInflector(
            InflectorTable inflectorTable,
            InflectorDescription inflectorDescription,
            string word,
            LanguageID languageID,
            Dictionary<string, string> inflectionDictionary,
            LexicalCategory category,
            string categoryString,
            string classCode,
            string subClassCode,
            string prefix,
            string stem,
            string suffix,
            string label,
            InflectorFamily inflectorFamily,
            Inflector inflector,
            string regularFullInflection,
            string regularInflection,
            List<BaseString> externalComponents)
        {
            List<BaseString> localComponents;
            List<WordToken> localPattern = inflectorFamily.Pattern;

            if ((localPattern == null) || (localPattern.Count() == 0))
                localPattern = new List<WordToken>() { new WordToken("Main", null, null) };

            if (!GetInflectionComponents(
                regularFullInflection,
                regularInflection,
                inflectorFamily.Pattern,
                LanguageID,
                out localComponents))
            {
                throw new Exception("Error getting local components from \"" + regularFullInflection + "\" for \"" + label + "\".");
            }

            int localCount = localComponents.Count();
            int externalCount = externalComponents.Count();
            Condition condition = new Condition(label, null, null, null);
            List<Condition> conditions = new List<Condition>() { condition };
            int patternIndex;
            int patternCount = localPattern.Count();

            if (externalCount > localCount)
            {
                if (externalComponents.FirstOrDefault(x => x.KeyString == "ReflexivePronoun") != null)
                {
                    if (externalCount - 1 > localCount)
                        throw new Exception("CompareComponents: External count exceeds local count: local=" +
                            ObjectUtilities.GetStringFromObjectList<BaseString>(localComponents) +
                            " external=" + ObjectUtilities.GetStringFromObjectList<BaseString>(externalComponents));
                }
                else
                    throw new Exception("CompareComponents: External count exceeds local count: local=" +
                        ObjectUtilities.GetStringFromObjectList<BaseString>(localComponents) +
                        " external=" + ObjectUtilities.GetStringFromObjectList<BaseString>(externalComponents));
            }

            if (!ExternalConjugatorHasNegative)
            {
                if (label.Contains("Negative"))
                    return true;

                Inflector negativeInflector = GetNegativeInflector(inflector, inflectorTable);

                if (negativeInflector != null)
                {
                    string negativeLabel = negativeInflector.Label;
                    Condition negativeCondition = new Condition(negativeLabel, null, null, null);
                    conditions.Add(negativeCondition);
                }
            }

            SemiRegular irregular = null;

            for (patternIndex = 0; patternIndex < patternCount; patternIndex++)
            {
                WordToken patternToken = localPattern[patternIndex];
                BaseString localComponent = localComponents.FirstOrDefault(x => x.KeyString == patternToken.Type);
                BaseString externalComponent = externalComponents.FirstOrDefault(x => x.KeyString == patternToken.Type);

                if (localComponent == null)
                    continue;

                if (externalComponent == null)
                    continue;

                if (localComponent.Text == externalComponent.Text)
                    continue;

                if (irregular == null)
                {
                    irregular = inflectorTable.GetSemiRegular(word);

                    if (irregular == null)
                    {
                        irregular = new SemiRegular(
                            word,
                            new List<LiteralString>() { new LiteralString(word) },
                            new List<SpecialAction>(),
                            false,
                            true,
                            null);

                        inflectorTable.AppendSemiRegular(irregular);
                    }
                }

                switch (localComponent.KeyString)
                {
                    case "Main":
                        ProcessIrregularMain(
                            word,
                            inflectorTable,
                            label,
                            externalComponent.Text,
                            prefix,
                            stem,
                            suffix,
                            irregular,
                            conditions);
                        break;
                    case "Helper":
                        ProcessIrregularHelper(
                            word,
                            label,
                            localComponent.Text,
                            externalComponent.Text,
                            1,
                            irregular,
                            conditions);
                        break;
                    case "Pronoun":
                        continue;
                    case "ReflexivePronoun":
                        continue;
                    case "ImplicitPronoun":
                        continue;
                    case "Polarizer":
                        if (!ExternalConjugatorHasNegative)
                            continue;
                        break;
                    case "Modal":
                        if (!ExternalConjugatorHasModal)
                            continue;
                        break;
                }
            }

            return true;
        }

        protected string InflectFromPattern(
            string baseInflected,
            List<Operation> operations,
            TokenDescriptor tokenDescriptor)
        {
            string inflection = baseInflected;

            foreach (Operation operation in operations)
            {
                switch (operation.Operator)
                {
                    case "Inherit":
                        break;
                    case "TruncateEnd":
                        {
                            int truncateCount = operation.OperandInteger(0);
                            inflection = inflection.Substring(0, inflection.Length - truncateCount);
                        }
                        break;
                    case "Append":
                        switch (operation.Operand)
                        {
                            case "Iterator":
                                if (tokenDescriptor != null)
                                {
                                    string tokenText = tokenDescriptor.Text.GetIndexedString(0);
                                    inflection += tokenText;
                                }
                                break;
                            default:
                                throw new Exception("Unexpected Append operation operand: " + operation.Operand);
                        }
                        break;
                    default:
                        throw new Exception("Unexpected operation: " + operation.ToString());
                }
            }

            return inflection;
        }

        protected virtual bool ProcessIrregularMain(
            string word,
            InflectorTable inflectorTable,
            string label,
            string irregularInflection,
            string prefix,
            string stem,
            string suffix,
            SemiRegular irregular,
            List<Condition> conditions)
        {
            bool replacePrefix = false;
            string irregularPrefix = String.Empty;
            bool replaceStem = false;
            string irregularStem = String.Empty;
            bool replaceSuffix = false;
            string irregularSuffix = String.Empty;
            string stemRemainder = irregularInflection;
            int irregularCount = 0;
            int ofs;

            if (!String.IsNullOrEmpty(prefix))
            {
                if (!irregularInflection.StartsWith(prefix))
                {
                    replacePrefix = true;
                    ofs = stemRemainder.IndexOf(stem);
                    if (ofs != -1)
                    {
                        irregularPrefix = stemRemainder.Substring(0, ofs);
                        stemRemainder = stemRemainder.Substring(ofs);
                    }
                    irregularCount++;
                }
                else
                {
                    ofs = prefix.Length;
                    stemRemainder = stemRemainder.Substring(ofs);
                }
            }

            if (!String.IsNullOrEmpty(suffix))
            {
                if (!irregularInflection.EndsWith(suffix))
                {
                    replaceSuffix = true;
                    ofs = stemRemainder.IndexOf(stem);
                    if (ofs != -1)
                    {
                        ofs += stem.Length;
                        irregularSuffix = stemRemainder.Substring(ofs);
                        stemRemainder = stemRemainder.Substring(0, ofs);
                    }
                    irregularCount++;
                }
                else
                    stemRemainder = stemRemainder.Substring(0, stemRemainder.Length - suffix.Length);
            }

            if (stemRemainder != stem)
            {
                replaceStem = true;
                irregularStem = stemRemainder;
                irregularCount++;
            }

            if (irregularCount == 0)
                throw new Exception("GenerateIrregularInflectors: No diffs found: irregularInflection=" +
                    irregularInflection);

            string actionType = null;
            List<SpecialAction> actions = irregular.Actions;
            SpecialAction action;
            LiteralString inputLiteral = null;
            LiteralString outputLiteral = null;
            LiteralString stemLiteral = null;
            LiteralString suffixLiteral = null;

            if (replacePrefix)
            {
                actionType = "SetPrefix";
                outputLiteral = new LiteralString(irregularPrefix);
            }

            if (replaceStem && replaceSuffix)
            {
                string stemPatternSubString;
                string stemReplacementSubString;
                if (irregularStem.StartsWith(stem))
                {
                    actionType = "AppendToStemReplaceSuffix";
                    stemReplacementSubString = irregularStem.Substring(stem.Length);
                    outputLiteral = new LiteralString(stemReplacementSubString);
                }
                else if (TextUtilities.ComputeSimpleReplacement(
                    stem,
                    irregularStem,
                    out stemPatternSubString,
                    out stemReplacementSubString))
                {
                    actionType = "ReplaceInStemAndSuffix";
                    inputLiteral = new LiteralString(stemPatternSubString);
                    outputLiteral = new LiteralString(stemReplacementSubString);
                }
                else
                {
                    actionType = "ReplaceInStemAndSuffix";
                    inputLiteral = new LiteralString(stem);
                    outputLiteral = new LiteralString(irregularStem);
                }
                suffixLiteral = new LiteralString(irregularSuffix);
            }
            else if (replaceSuffix)
            {
                actionType = "SetSuffix";
                outputLiteral = new LiteralString(irregularSuffix);
            }
            else if (replaceStem)
            {
                string stemPatternSubString;
                string stemReplacementSubString;
                if (irregularStem.StartsWith(stem))
                {
                    actionType = "AppendToStem";
                    stemReplacementSubString = irregularStem.Substring(stem.Length);
                    outputLiteral = new LiteralString(stemReplacementSubString);
                }
                else if (TextUtilities.ComputeSimpleReplacement(
                    stem,
                    irregularStem,
                    out stemPatternSubString,
                    out stemReplacementSubString))
                {
                    actionType = "ReplaceInStem";
                    inputLiteral = new LiteralString(stemPatternSubString);
                    outputLiteral = new LiteralString(stemReplacementSubString);
                }
                else
                {
                    actionType = "ReplaceInStem";
                    inputLiteral = new LiteralString(stem);
                    outputLiteral = new LiteralString(irregularStem);
                }
            }

            if ((suffixLiteral != null) && (suffixLiteral.ToString() == "ei,credetti"))
                ApplicationData.Global.PutConsoleMessage("ei,credetti");

            action = new SpecialAction(
                actionType,
                inputLiteral,
                outputLiteral,
                null,
                stemLiteral,
                suffixLiteral,
                null,
                null,
                null,
                conditions,
                false,
                false);
            actions.Add(action);

            return true;
        }

        protected virtual bool ProcessIrregularHelper(
            string word,
            string label,
            string localHelper,
            string externalHelper,
            int position,
            SemiRegular irregular,
            List<Condition> conditions)
        {
            string actionType = null;
            List<SpecialAction> actions = irregular.Actions;
            SpecialAction action;
            LiteralString inputLiteral = new Language.LiteralString(localHelper);
            LiteralString outputLiteral = new LiteralString(externalHelper);

            switch (position)
            {
                case 0:
                    actionType = "ReplaceInPrePronoun";
                    break;
                case 1:
                    actionType = "ReplaceInPreWords";
                    break;
                case 2:
                    actionType = "ReplaceInPostPronoun";
                    break;
                default:
                    throw new Exception("Unexpected position.");
            }

            action = new SpecialAction(
                actionType,
                inputLiteral,
                outputLiteral,
                null,
                null,
                null,
                null,
                null,
                null,
                conditions,
                false,
                false);
            actions.Add(action);

            return true;
        }

        public bool CompactSemiIrregulars(InflectorTable inflectorTable)
        {
            List<SemiRegular> semiRegulars = inflectorTable.SemiRegulars;
            bool returnValue = true;

            if (semiRegulars == null)
                return true;

            int semiRegularIndex;

            for (semiRegularIndex = 0; semiRegularIndex < semiRegulars.Count(); semiRegularIndex++)
            {
                SemiRegular semiRegular = semiRegulars[semiRegularIndex];

                if (!CompactSemiIrregular(semiRegulars, semiRegularIndex))
                    returnValue = false;
            }

            return returnValue;
        }

        public bool CompactSemiIrregular(List<SemiRegular> semiRegulars, int semiRegularIndex)
        {
            List<string> compacted = new List<string>();
            SemiRegular semiRegularBase = semiRegulars[semiRegularIndex];
            bool returnValue = true;

            if (!semiRegularBase.HasTargets())
            {
                ApplicationData.Global.PutConsoleMessage("Base SemiRegular doesn't have targets: " + semiRegularBase.ToString());
                return true;
            }

            if (semiRegularBase.HasDictionaryTargets())
            {
                ApplicationData.Global.PutConsoleMessage("Base SemiRegular has dictionary targets: " + semiRegularBase.ToString());
                return true;
            }

            if (semiRegularBase.HasConditions())
            {
                ApplicationData.Global.PutConsoleMessage("Base SemiRegular has conditions: " + semiRegularBase.ToString());
                return true;
            }

            for (semiRegularIndex++; semiRegularIndex < semiRegulars.Count(); semiRegularIndex++)
            {
                SemiRegular semiRegularTest = semiRegulars[semiRegularIndex];

                if (semiRegularTest.MatchActions(semiRegularBase))
                {
                    if (!semiRegularTest.HasTargets())
                    {
                        ApplicationData.Global.PutConsoleMessage("SemiRegular doesn't have targets: " + semiRegularTest.ToString());
                        continue;
                    }

                    if (semiRegularTest.HasDictionaryTargets())
                    {
                        ApplicationData.Global.PutConsoleMessage("SemiRegular has dictionary targets: " + semiRegularTest.ToString());
                        continue;
                    }

                    if (semiRegularTest.HasConditions())
                    {
                        ApplicationData.Global.PutConsoleMessage("SemiRegular has conditions: " + semiRegularTest.ToString());
                        continue;
                    }

                    semiRegularBase.AddTargets(semiRegularTest.Targets);
                    semiRegulars.RemoveAt(semiRegularIndex);
                    semiRegularIndex--;

                    if (compacted.Count() == 0)
                        compacted.Add(semiRegularBase.Targets[0].GetIndexedString(0));

                    compacted.Add(semiRegularTest.Targets[0].GetIndexedString(0));
                }
            }

            if (compacted.Count() != 0)
            {
                ApplicationData.Global.PutConsoleMessage("Compacted irregular targets: " +
                    ObjectUtilities.GetStringFromStringList(compacted));
            }

            return returnValue;
        }

        public Inflector GetNegativeInflector(
            Inflector positiveInflector,
            InflectorTable inflectorTable)
        {
            string positiveLabel = positiveInflector.Label;
            string negativeLabel;

            if (positiveLabel.Contains("Positive"))
                negativeLabel = positiveLabel.Replace("Positive", "Negative");
            else
            {
                int ofs = positiveLabel.IndexOf("Singular");

                if (ofs != -1)
                    ofs = positiveLabel.IndexOf("Plural");

                if (ofs == -1)
                    return null;

                negativeLabel = positiveLabel.Insert(ofs, "Negative ");
            }

            Inflector negativeInflector = inflectorTable.GetInflector(negativeLabel);

            return negativeInflector;
        }

        public bool ParseExternalPatternComponents(
            string value,
            string pattern,
            string alternatePattern,
            out List<BaseString> components)
        {
            BaseString component;
            string[] multipleParts = value.Split(LanguageLookup.Comma);

            // Ignore alternates for now.
            value = multipleParts[0];

            components = new List<BaseString>();

            if (String.IsNullOrEmpty(pattern))
            {
                component = new BaseString("Main", value);
                components.Add(component);
            }
            else if (TextUtilities.CountChars(pattern, '%') == 1)
            {
                component = new BaseString(GetComponentType(pattern), value);
                components.Add(component);
            }
            else
            {
                string[] patternParts = pattern.Split(LanguageLookup.Space);

                string[] valueParts = value.Split(LanguageLookup.Space);
                int count = patternParts.Length;
                int index;

                if (valueParts.Length != count)
                {
                    if (valueParts.Length == 1)
                    {
                        component = new BaseString("Main", value);
                        components.Add(component);
                    }
                    else if (!String.IsNullOrEmpty(alternatePattern))
                    {
                        string[] alternatePatternParts = alternatePattern.Split(LanguageLookup.Space);
                        int alternateCount = alternatePatternParts.Length;

                        if (valueParts.Length == alternateCount)
                        {
                            for (index = 0; index < alternateCount; index++)
                            {
                                string patternPart = alternatePatternParts[index];
                                string valuePart = valueParts[index];

                                component = new BaseString(GetComponentType(patternPart), valuePart);
                                components.Add(component);
                            }
                        }
                        else
                            throw new Exception("ParseExternalPatternComponents: Pattern token count doesn't match value part count: pattern=" +
                                pattern + " value=" + value);
                    }
                    else
                        throw new Exception("ParseExternalPatternComponents: Pattern token count doesn't match value part count: pattern=" +
                            pattern + " value=" + value);
                }
                else
                {
                    for (index = 0; index < count; index++)
                    {
                        string patternPart = patternParts[index];
                        string valuePart = valueParts[index];

                        component = new BaseString(GetComponentType(patternPart), valuePart);
                        components.Add(component);
                    }
                }
            }

            return true;
        }

        public virtual bool GetInflectionComponents(
            Inflection inflection,
            List<WordToken> familyPattern,
            LanguageID languageID,
            out List<BaseString> components)
        {
            BaseString component;

            components = new List<BaseString>();

            string value = inflection.GetOutput(languageID);
            string mainWord = inflection.GetMainWord(languageID);

            if (familyPattern == null)
            {
                component = new BaseString("Main", mainWord);
                components.Add(component);
            }
            else
            {
                string[] valueParts = value.Split(LanguageLookup.Space);
                int patternCount = familyPattern.Count();
                int patternIndex;
                int valueIndex = 0;

                if (valueIndex >= valueParts.Length)
                    throw new Exception("GetInflectionComponents: Value count exceeds pattern length: pattern=" +
                        ObjectUtilities.GetStringFromObjectList<WordToken>(familyPattern) + " value=" + value);

                for (patternIndex = 0; patternIndex < patternCount; patternIndex++)
                {
                    WordToken wordToken = familyPattern[patternIndex];

                    if (valueIndex >= valueParts.Length)
                        throw new Exception("GetInflectionComponents: Value index exceeds length: pattern=" +
                            ObjectUtilities.GetStringFromObjectList<WordToken>(familyPattern) + " value=" + value);

                    string valuePart = valueParts[valueIndex];

                    switch (wordToken.Type)
                    {
                        case "Main":
                            break;
                        case "Helper":
                            break;
                        case "Pronoun":
                            continue;
                        case "ReflexivePronoun":
                            continue;
                        case "ImplicitPronoun":
                            continue;
                        case "Polarizer":
                            if (valuePart != wordToken.Word.GetIndexedString(0))
                                continue;
                            break;
                        case "Modal":
                            if (patternIndex == 0)
                            {
                                component = new BaseString(wordToken.Type, inflection.GetPrePronoun(languageID));
                                components.Add(component);
                                continue;
                            }
                            else if (patternIndex == patternCount - 1)
                            {
                                component = new BaseString(wordToken.Type, inflection.GetPostWords(languageID));
                                components.Add(component);
                                continue;
                            }
                            break;
                    }

                    component = new BaseString(wordToken.Type, valuePart);
                    components.Add(component);
                    valueIndex++;
                }
            }

            return true;
        }

        public virtual bool GetInflectionComponents(
            string value,
            string mainWord,
            List<WordToken> familyPattern,
            LanguageID languageID,
            out List<BaseString> components)
        {
            BaseString component;

            components = new List<BaseString>();

            if (familyPattern == null)
            {
                component = new BaseString("Main", mainWord);
                components.Add(component);
            }
            else
            {
                string[] valueParts = value.Split(LanguageLookup.Space);
                int patternCount = familyPattern.Count();
                int patternIndex;
                int valueIndex = 0;

                if (valueIndex >= valueParts.Length)
                    throw new Exception("GetInflectionComponents: Value count exceeds pattern length: pattern=" +
                        ObjectUtilities.GetStringFromObjectList<WordToken>(familyPattern) + " value=" + value);

                for (patternIndex = 0; patternIndex < patternCount; patternIndex++)
                {
                    WordToken wordToken = familyPattern[patternIndex];

                    if (valueIndex >= valueParts.Length)
                        throw new Exception("GetInflectionComponents: Value index exceeds length: pattern=" +
                            ObjectUtilities.GetStringFromObjectList<WordToken>(familyPattern) + " value=" + value);

                    string valuePart = valueParts[valueIndex];

                    switch (wordToken.Type)
                    {
                        case "Main":
                            break;
                        case "Helper":
                            break;
                        case "Pronoun":
                            continue;
                        case "ReflexivePronoun":
                            continue;
                        case "ImplicitPronoun":
                            continue;
                        case "Polarizer":
                            if (wordToken.Word != null)
                            {
                                if (valuePart != wordToken.Word.GetIndexedString(0))
                                    continue;
                            }
                            break;
                        case "Modal":
                            break;
                    }

                    component = new BaseString(wordToken.Type, valuePart);
                    components.Add(component);
                    valueIndex++;
                }
            }

            return true;
        }

        public string GetComponentType(string patternToken)
        {
            string type;

            switch (patternToken)
            {
                case "%m":
                    type = "Main";
                    break;
                case "%h":
                    type = "Helper";
                    break;
                case "%p":
                    type = "Pronoun";
                    break;
                case "%r":
                    type = "ReflexivePronoun";
                    break;
                case "%n":
                    type = "Polarizer";
                    break;
                case "%o":
                    type = "Modal";
                    break;
                default:
                    throw new Exception("GetComponentType: Unexpected pattern token: " + patternToken);
            }

            return type;
        }

        public bool CreateModifierFromPattern(
            InflectorDescription inflectorDescription,
            InflectorTable inflectorTable,
            InflectorFamily inflectorFamily,
            string subLabel, 
            LexicalCategory category,
            string classCode,
            string subClassCode,
            string stem,
            List<WordToken> pattern,
            bool hasPolarity,
            bool isNegative,
            out Modifier modifier)
        {
            modifier = new Modifier();
            modifier.Category = category;
            modifier.Class = new List<string>() { classCode };

            if (!String.IsNullOrEmpty(subClassCode))
                modifier.SubClass = new List<string>() { subClassCode };

            bool mainDone = false;
            bool pronounDone = false;
            string polarityWord = String.Empty;
            int helperOrdinal = 1;

            if ((pattern == null) || (pattern.Count() == 0))
                pattern = new List<WordToken>() { new WordToken("Main", null, null) };

            foreach (WordToken wordToken in pattern)
            {
                string type = wordToken.Type;
                string value;

                if (wordToken.Word != null)
                    value = wordToken.Word.StringListString;
                else
                    value = String.Empty;

                switch (type)
                {
                    case "Main":
                        {
                            Inflector inflector = inflectorTable.GetInflector(wordToken.Label);
                            if (inflector == null)
                                inflector = inflectorTable.GetInflectorStartsWith(wordToken.Label);
                            if (inflector == null)
                                throw new Exception("CreateModifierFromPattern: No inflection for: " + wordToken.Label);
                            Modifier mainModifier;
                            if (!String.IsNullOrEmpty(subClassCode))
                                mainModifier = inflector.GetModifier(classCode, subClassCode);
                            else
                                mainModifier = inflector.GetModifier(classCode);
                            if (mainModifier.Prefix != null)
                                modifier.Prefix = new LiteralString(mainModifier.Prefix);
                            if (mainModifier.Suffix != null)
                                modifier.Suffix = new LiteralString(mainModifier.Suffix);
                            // FixMe: Need support for irregular.
                            mainDone = true;
                        }
                        break;
                    case "Helper":
                        {
                            string helperLabel = wordToken.Label;
                            string label = wordToken.Label;
                            if (helperOrdinal == 1)
                            {
                                if (hasPolarity)
                                {
                                    if (isNegative)
                                        label += " Negative";
                                    else
                                        label += " Positive";
                                }
                                if (!String.IsNullOrEmpty(subLabel))
                                {
                                    label += " " + subLabel;
                                    helperLabel += " " + subLabel;
                                }
                            }
                            string word = wordToken.Word.GetIndexedString(0);
                            string helperInflection = inflectorDescription.GetHelperInflectionSimple(
                                word,
                                helperLabel);
                            if (String.IsNullOrEmpty(helperInflection))
                                helperInflection = GetQuickInflection(
                                    word,
                                    label,
                                    inflectorTable,
                                    classCode,
                                    subClassCode,
                                    false);
                            if (String.IsNullOrEmpty(helperInflection))
                                helperInflection = GetQuickInflection(
                                    word,
                                    helperLabel,
                                    inflectorTable,
                                    classCode,
                                    subClassCode,
                                    false);
                            if (String.IsNullOrEmpty(helperInflection))
                                throw new Exception("CreateModifierFromPattern: Inflection for word \"" + word + "\" label \"" + label + "\" failed.");
                            SetModifierWord(modifier, helperInflection, pronounDone, mainDone);
                            helperOrdinal++;
                        }
                        break;
                    case "Pronoun":
                        pronounDone = true;
                        break;
                    case "ReflexivePronoun":
                        break;
                    case "Polarizer":
                        if (hasPolarity)
                        {
                            polarityWord = wordToken.Word.GetIndexedString(0);
                            if (isNegative && wordToken.HasClassificationWith("Polarity", "Negative"))
                                SetModifierWord(modifier, polarityWord, pronounDone, mainDone);
                            else if (!isNegative && wordToken.HasClassificationWith("Polarity", "Positive"))
                                SetModifierWord(modifier, polarityWord, pronounDone, mainDone);
                        }
                        break;
                    case "Modal":
                        SetModifierWord(modifier, wordToken.Word.GetIndexedString(0), pronounDone, mainDone);
                        break;
                }
            }

            return true;
        }

        public bool CreateModifierFromComponents(
            LexicalCategory category,
            string classCode,
            string subClassCode,
            string stem,
            List<BaseString> components,
            List<LiteralString> iteratorStrings,
            List<WordToken> pattern,
            bool hasPolarity,
            bool isNegative,
            out Modifier modifier)
        {
            modifier = new Modifier();
            modifier.Category = category;
            modifier.Class = new List<string>() { classCode };

            if (!String.IsNullOrEmpty(subClassCode))
                modifier.SubClass = new List<string>() { subClassCode };

            bool mainDone = false;
            bool pronounDone = false;

            if ((pattern == null) || (pattern.Count() == 0))
                pattern = new List<WordToken>() { new WordToken("Main", null, null) };

            foreach (WordToken wordToken in pattern)
            {
                string type = wordToken.Type;
                string value;
                BaseString component = components.FirstOrDefault(x => x.KeyString == type);

                if (wordToken.Word != null)
                    value = wordToken.Word.StringListString;
                else
                    value = String.Empty;

                if (component == null)
                    component = new BaseString(type, value);

                switch (type)
                {
                    case "Main":
                        {
                            string inflection = component.Text;

                            if (wordToken.OperationCount() != 0)
                            {
                                foreach (Operation operation in wordToken.Operations)
                                {
                                    switch (operation.Operator)
                                    {
                                        case "Inherit":
                                            if (operation.Operand != "Main")
                                            {
                                                BaseString source = components.FirstOrDefault(x => x.KeyString == operation.Operand);
                                                if (source == null)
                                                    throw new Exception("Can't find WordToken Inherit source: " + operation.Operand);
                                                inflection = source.Text;
                                            }
                                            break;
                                        case "TruncateEnd":
                                            {
                                                int truncateCount = operation.OperandInteger(0);
                                                inflection = inflection.Substring(0, inflection.Length - truncateCount);
                                            }
                                            break;
                                        case "Append":
                                            switch (operation.Operand)
                                            {
                                                case "Iterator":
                                                    if ((iteratorStrings != null) && (iteratorStrings.Count() != 0))
                                                    {
                                                        LiteralString pronounString = iteratorStrings[0];
                                                        string pronounText = pronounString.GetIndexedString(0);
                                                        inflection += pronounText;
                                                    }
                                                    break;
                                                default:
                                                    throw new Exception("Unexpected Append operation operand: " + operation.Operand);
                                            }
                                            break;
                                        default:
                                            throw new Exception("Unexpected operation: " + operation.ToString());
                                    }
                                }
                            }
                            if (!inflection.StartsWith(stem))
                            {
                                int ofs = inflection.IndexOf(stem);

                                if (ofs == -1)
                                    throw new Exception("CreateModifier: Inflection doesn't contain stem: inflection=" +
                                        inflection + " stem=" + stem);

                                string prefix = inflection.Substring(0, ofs);
                                modifier.Prefix = new LiteralString(prefix);

                                inflection = inflection.Substring(ofs);
                            }

                            string suffix = inflection.Substring(stem.Length);
                            modifier.Suffix = new LiteralString(suffix);
                            mainDone = true;
                        }
                        break;
                    case "Helper":
                        SetModifierWord(modifier, component.Text, pronounDone, mainDone);
                        break;
                    case "Pronoun":
                        pronounDone = true;
                        break;
                    case "ReflexivePronoun":
                        break;
                    case "Polarizer":
                        if (hasPolarity)
                        {
                            if (isNegative && wordToken.HasClassificationWith("Polarity", "Negative"))
                                SetModifierWord(modifier, component.Text, pronounDone, mainDone);
                            else if (!isNegative && wordToken.HasClassificationWith("Polarity", "Positive"))
                                SetModifierWord(modifier, component.Text, pronounDone, mainDone);
                        }
                        else
                            SetModifierWord(modifier, component.Text, pronounDone, mainDone);
                        break;
                    case "Modal":
                        SetModifierWord(modifier, component.Text, pronounDone, mainDone);
                        break;
                    default:
                        throw new Exception("Unexpected pattern word token type: " + type);
                }
            }

            return true;
        }

        protected void SetModifierWord(
            Modifier modifier,
            string word,
            bool isPronounDone,
            bool isMainDone)
        {
            LiteralString wordString = new LiteralString(word);
            LiteralString previous;

            if (isMainDone)
            {
                previous = modifier.PostWords;

                if (previous != null)
                {
                    LiteralString sep = (LanguageLookup.IsUseSpacesToSeparateWords(LanguageID) ?
                        new Language.LiteralString(" ") : null);
                    wordString = LiteralString.Concatenate(previous, sep, wordString);
                }

                modifier.PostWords = wordString;
            }
            else if (isPronounDone)
            {
                previous = modifier.PreWords;

                if (previous != null)
                {
                    LiteralString sep = (LanguageLookup.IsUseSpacesToSeparateWords(LanguageID) ?
                        new Language.LiteralString(" ") : null);
                    wordString = LiteralString.Concatenate(previous, sep, wordString);
                }

                modifier.PreWords = wordString;
            }
            else
            {
                previous = modifier.PrePronoun;

                if (previous != null)
                {
                    LiteralString sep = (LanguageLookup.IsUseSpacesToSeparateWords(LanguageID) ?
                        new Language.LiteralString(" ") : null);
                    wordString = LiteralString.Concatenate(previous, sep, wordString);
                }

                modifier.PrePronoun = wordString;
            }
        }

        public bool VerifyModifierFromPattern(
            InflectorDescription inflectorDescription,
            InflectorTable inflectorTable,
            InflectorFamily inflectorFamily,
            string subLabel,
            LexicalCategory category,
            string classCode,
            string subClassCode,
            string stem,
            List<WordToken> pattern,
            bool hasPolarity,
            bool isNegative,
            Modifier modifier)
        {
            Modifier newModifier;
            string message = String.Empty;

            if (!CreateModifierFromPattern(
                    inflectorDescription,
                    inflectorTable,
                    inflectorFamily,
                    subLabel,
                    category,
                    classCode,
                    subClassCode,
                    stem,
                    pattern,
                    hasPolarity,
                    isNegative,
                    out newModifier))
                message += "Error creating new modifier.\n";

            if (modifier.Category != newModifier.Category)
                message += "Category different: old=" + modifier.Category.ToString() + " new=" + newModifier.Category.ToString() + "\n";

            if (newModifier.ClassDisplay() != modifier.ClassDisplay())
                message += "Class different: old=" + modifier.ClassDisplay() + " new=" + newModifier.ClassDisplay() + "\n";

            if (newModifier.SubClassDisplay() != modifier.SubClassDisplay())
                message += "Subclass different: old=" + modifier.SubClassDisplay() + " new=" + newModifier.SubClassDisplay() + "\n";

            if (newModifier.PreWords != modifier.PreWords)
                message += "Prewords different: old=" + modifier.PreWords.ToString() + " new=" + newModifier.PreWords.ToString() + "\n";

            if (newModifier.Prefix != modifier.Prefix)
                message += "Prefix different: old=" + modifier.Prefix.ToString() + " new=" + newModifier.Prefix.ToString() + "\n";

            if (newModifier.Suffix != modifier.Suffix)
                message += "Suffix different: old=" + modifier.Suffix.ToString() + " new=" + newModifier.Suffix.ToString() + "\n";

            if (newModifier.PostWords != modifier.PostWords)
                message += "PostWords different: old=" + modifier.PostWords.ToString() + " new=" + newModifier.PostWords.ToString() + "\n";

            if (!String.IsNullOrEmpty(message))
                throw new Exception(message);

            return true;
        }

        public bool VerifyModifierFromComponents(
            LexicalCategory category,
            string classCode,
            string subClassCode,
            string stem,
            List<BaseString> components,
            List<LiteralString> iteratorStrings,
            List<WordToken> pattern,
            bool hasPolarity,
            bool isNegative,
            Modifier modifier)
        {
            Modifier newModifier;
            string message = String.Empty;

            if (!CreateModifierFromComponents(
                    category,
                    classCode,
                    subClassCode,
                    stem,
                    components,
                    iteratorStrings,
                    pattern,
                    hasPolarity,
                    isNegative,
                    out newModifier))
                message += "Error creating new modifier.\n";

            if (modifier.Category != newModifier.Category)
                message += "Category different: old=" + modifier.Category.ToString() + " new=" + newModifier.Category.ToString() + "\n";

            if (newModifier.ClassDisplay() != modifier.ClassDisplay())
                message += "Class different: old=" + modifier.ClassDisplay() + " new=" + newModifier.ClassDisplay() + "\n";

            if (newModifier.SubClassDisplay() != modifier.SubClassDisplay())
                message += "Subclass different: old=" + modifier.SubClassDisplay() + " new=" + newModifier.SubClassDisplay() + "\n";

            if (newModifier.PreWords != modifier.PreWords)
                message += "Prewords different: old=" + modifier.PreWords.ToString() + " new=" + newModifier.PreWords.ToString() + "\n";

            if (newModifier.Prefix != modifier.Prefix)
                message += "Prefix different: old=" + modifier.Prefix.ToString() + " new=" + newModifier.Prefix.ToString() + "\n";

            if (newModifier.Suffix != modifier.Suffix)
                message += "Suffix different: old=" + modifier.Suffix.ToString() + " new=" + newModifier.Suffix.ToString() + "\n";

            if (newModifier.PostWords != modifier.PostWords)
                message += "PostWords different: old=" + modifier.PostWords.ToString() + " new=" + newModifier.PostWords.ToString() + "\n";

            if (!String.IsNullOrEmpty(message))
                throw new Exception(message);

            return true;
        }

        public string GetQuickInflection(
            string word,
            string label,
            InflectorTable inflectorTable,
            string classCode,
            string subClassCode,
            bool isMainOnly)
        {
            Inflector inflector = inflectorTable.GetInflector(label);

            if (inflector == null)
                return null;

            Modifier modifier = inflector.GetModifier(classCode, subClassCode);

            if (modifier == null)
                return null;

            string categoryString;
            string stem = GetStem(word, LanguageID, out categoryString);

            if (String.IsNullOrEmpty(stem))
                return null;

            string inflection = String.Empty;

            if (!isMainOnly && (modifier.PreWords != null))
                inflection += modifier.PreWords.GetIndexedString(0) + " ";

            if (modifier.Prefix != null)
                inflection += modifier.Prefix.GetIndexedString(0);

            inflection += stem;

            if (modifier.Suffix != null)
                inflection += modifier.Suffix.GetIndexedString(0);

            if (!isMainOnly && (modifier.PostWords != null))
                inflection += " " + modifier.PostWords.GetIndexedString(0);

            return inflection;
        }

        public bool GetQuickInflectionAndParts(
            string word,
            string label,
            InflectorTable inflectorTable,
            string classCode,
            string subClassCode,
            out string fullInflection,
            out string mainInflection,
            out string prefix,
            out string stem,
            out string suffix,
            out Inflector inflector)
        {
            inflector = inflectorTable.GetInflector(label);

            if (inflector == null)
            {
                fullInflection = null;
                mainInflection = null;
                prefix = null;
                stem = null;
                suffix = null;
                inflector = null;
                return false;
            }

            return GetQuickInflectionParts(
                word,
                label,
                inflectorTable,
                classCode,
                subClassCode,
                inflector,
                out fullInflection,
                out mainInflection,
                out prefix,
                out stem,
                out suffix);
        }

        public bool GetQuickInflectionParts(
            string word,
            string label,
            InflectorTable inflectorTable,
            string classCode,
            string subClassCode,
            Inflector inflector,
            out string fullInflection,
            out string mainInflection,
            out string prefix,
            out string stem,
            out string suffix)
        {
            inflector = inflectorTable.GetInflector(label);

            fullInflection = String.Empty;
            mainInflection = prefix = stem = suffix = String.Empty;

            if (inflector == null)
                return false;

            string categoryString;
            stem = GetStem(word, LanguageID, out categoryString);

            // May be "ir".
            //if (String.IsNullOrEmpty(stem))
            //    return false;

            Modifier modifier = inflector.GetModifier(classCode, subClassCode);

            if (modifier == null)
                return false;

            if (modifier.Prefix != null)
            {
                prefix = modifier.Prefix.GetIndexedString(0);
                mainInflection += prefix;
            }

            mainInflection += stem;

            if (modifier.Suffix != null)
            {
                suffix = modifier.Suffix.GetIndexedString(0);
                mainInflection += suffix;
            }

            bool isSpaced = LanguageLookup.IsUseSpacesToSeparateWords(LanguageID);

            if (modifier.PrePronoun != null)
                fullInflection += modifier.PrePronoun.GetIndexedString(0);

            if (modifier.PreWords != null)
            {
                if (!String.IsNullOrEmpty(fullInflection) && isSpaced)
                    fullInflection += " ";

                fullInflection += modifier.PreWords.GetIndexedString(0);
            }

            if (!String.IsNullOrEmpty(fullInflection) && isSpaced)
                fullInflection += " ";

            fullInflection += mainInflection;

            if (modifier.PostWords != null)
            {
                if (!String.IsNullOrEmpty(fullInflection) && isSpaced)
                    fullInflection += " ";

                fullInflection += modifier.PostWords.GetIndexedString(0);
            }

            return true;
        }

        public virtual bool FixupInflectionDescriptions(
            InflectorDescription inflectorDescription,
            Dictionary<string, string> inflectionDictionary)
        {
#if false
            if (inflectorDescription.ExternalPronounExpansions != null)
            {
                Dictionary<string, TokenDescriptor> subjectPronounsByLabel = inflectorDescription.SubjectProunounsByDescriptorLabel;
                List<KeyValuePair<string, string>> newEntries = new List<KeyValuePair<string, string>>();

                foreach (KeyValuePair<string, string> kvpInflection in inflectionDictionary)
                {
                    string inflectionLabel = kvpInflection.Key;
                    string inflection = kvpInflection.Value;

                    foreach (KeyValuePair<string, TokenDescriptor> kvpPronoun in subjectPronounsByLabel)
                    {
                        string pronounLabel = kvpPronoun.Key;
                        TokenDescriptor iteratorDescriptor = kvpPronoun.Value;
                        string pronoun = iteratorDescriptor.KeyString;

                        if (kvpInflection.Key.Contains(pronounLabel))
                        {
                            List<TokenDescriptor> expandedPronouns = inflectorDescription.GetExternalPronounExpansions(pronoun);

                            if (expandedPronouns != null)
                            {
                                foreach (TokenDescriptor expandedPronoun in expandedPronouns)
                                {
                                    foreach (Designator expandedPronounDesignator in expandedPronoun.Designators)
                                    {
                                        string expandedPronounLabel = expandedPronounDesignator.Label;
                                        string newInflectionLabel = inflectionLabel.Replace(pronounLabel, expandedPronounLabel);
                                        KeyValuePair<string, string> kvpNewInflection = new KeyValuePair<string, string>(
                                            newInflectionLabel,
                                            inflection);
                                        newEntries.Add(kvpNewInflection);
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (KeyValuePair<string, string> newInflection in newEntries)
                    inflectionDictionary.Add(newInflection.Key, newInflection.Value);
            }
#endif

            return true;
        }

        public bool MergeInflectionTableDescription(
            InflectorDescription inflectorDescription,
            InflectorTable inflectorTable)
        {
            inflectorTable.InflectorTableDocumentation = inflectorDescription.InflectorTableDocumentation;

            if (inflectorDescription.CompoundInflectors != null)
                inflectorTable.AppendCompoundInflectors(inflectorDescription.CompoundInflectors);

            if (inflectorDescription.HelperEntries != null)
                inflectorTable.AppendHelperEntries(inflectorDescription.HelperEntries);

            inflectorTable.ClassKeys = inflectorDescription.ClassKeys;
            inflectorTable.StemType = inflectorDescription.StemType;
            inflectorTable.SubjectPronouns = inflectorDescription.SubjectPronouns;
            inflectorTable.ReflexivePronouns = inflectorDescription.ReflexivePronouns;
            inflectorTable.DirectPronouns = inflectorDescription.DirectPronouns;
            inflectorTable.IndirectPronouns = inflectorDescription.IndirectPronouns;
            inflectorTable.GenderSuffixes = inflectorDescription.GenderSuffixes;
            inflectorTable.SpecialInflectors = inflectorDescription.SpecialInflectors;

            inflectorTable.EndingsVersion = inflectorDescription.EndingsVersion;
            inflectorTable.EndingsSources = inflectorDescription.EndingsSources;
            inflectorTable.InflectorFilterList = inflectorDescription.InflectorFilterList;
            inflectorTable.AutomaticRowKeys = inflectorDescription.AutomaticRowKeys;
            inflectorTable.AutomaticColumnKeys = inflectorDescription.AutomaticColumnKeys;
            inflectorTable.MajorGroups = inflectorDescription.MajorGroups;
            inflectorTable.DesignationTranslations = inflectorDescription.DesignationTranslations;
            inflectorTable.CategoryStringToClassMap = inflectorDescription.CategoryStringToClassMap;

            inflectorTable.FindAndAppendEndingsSources();

            return true;
        }
    }
}
