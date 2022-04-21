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
        public static string DesignatorBreakpoint = "None";

        public bool UsesImplicitPronouns
        {
            get
            {
                return _UsesImplicitPronouns;
            }
            set
            {
                if (value != _UsesImplicitPronouns)
                {
                    _UsesImplicitPronouns = value;
                    ModifiedFlag = true;
                }
            }
        }

        public List<string> VerbClasses
        {
            get
            {
                return _VerbClasses;
            }
            set
            {
                if (value != _VerbClasses)
                {
                    _VerbClasses = value;
                    ModifiedFlag = true;
                }
            }
        }

        public string VerbStemType
        {
            get
            {
                return _VerbStemType;
            }
            set
            {
                if (value != _VerbStemType)
                {
                    _VerbStemType = value;
                    ModifiedFlag = true;
                }
            }
        }

        public virtual List<Inflection> InflectAnyDictionaryFormAll(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex)
        {
            Sense sense;
            string definition;
            LexicalCategory category;
            List<Inflection> inflections = null;

            if (GetSenseCategoryDefinition(
                    dictionaryEntry,
                    LanguageLookup.English,
                    ref senseIndex,
                    ref synonymIndex,
                    out sense,
                    out category,
                    out definition))
            {
                if (!CanInflectSense(sense))
                {
                    Inflection inflection;
                    Designator designation = new Designator("Dictionary", new Classifier("Special", "Dictionary"));

                    if (GetNonInflectedDesignated(
                            dictionaryEntry,
                            ref senseIndex,
                            ref synonymIndex,
                            designation,
                            out inflection))
                        inflections = new List<Inflection>() { inflection };
                }
                else
                {
                    StemSubstitutionCheck(ref dictionaryEntry, ref sense, ref senseIndex);

                    switch (category)
                    {
                        case LexicalCategory.Verb:
                            inflections = ConjugateVerbDictionaryFormAll(
                                dictionaryEntry,
                                ref senseIndex,
                                ref synonymIndex);
                            break;
                        case LexicalCategory.Adjective:
                            inflections = DeclineAdjectiveDictionaryFormAll(
                                dictionaryEntry,
                                ref senseIndex,
                                ref synonymIndex);
                            break;
                        case LexicalCategory.Noun:
                            inflections = DeclineNounDictionaryFormAll(
                                dictionaryEntry,
                                ref senseIndex,
                                ref synonymIndex);
                            break;
                        case LexicalCategory.Unknown:
                            inflections = DeclineUnknownDictionaryFormAll(
                                dictionaryEntry,
                                ref senseIndex,
                                ref synonymIndex);
                            break;
                    }
                }
            }

            return inflections;
        }

        public virtual List<Inflection> InflectAnyDictionaryFormDefault(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex)
        {
            Sense sense;
            string definition;
            LexicalCategory category;
            List<Inflection> inflections = null;

            if (GetSenseCategoryDefinition(
                    dictionaryEntry,
                    LanguageLookup.English,
                    ref senseIndex,
                    ref synonymIndex,
                    out sense,
                    out category,
                    out definition))
            {
                if (!CanInflectSense(sense))
                {
                    Inflection inflection;
                    Designator designation = new Designator("Dictionary", new Classifier("Special", "Dictionary"));

                    if (GetNonInflectedDesignated(
                            dictionaryEntry,
                            ref senseIndex,
                            ref synonymIndex,
                            designation,
                            out inflection))
                        inflections = new List<Inflection>() { inflection };
                }
                else
                {
                    StemSubstitutionCheck(ref dictionaryEntry, ref sense, ref senseIndex);

                    switch (category)
                    {
                        case LexicalCategory.Verb:
                            inflections = ConjugateVerbDictionaryFormDefault(
                                dictionaryEntry,
                                ref senseIndex,
                                ref synonymIndex);
                            break;
                        case LexicalCategory.Adjective:
                            inflections = DeclineAdjectiveDictionaryFormDefault(
                                dictionaryEntry,
                                ref senseIndex,
                                ref synonymIndex);
                            break;
                        case LexicalCategory.Noun:
                            inflections = DeclineNounDictionaryFormDefault(
                                dictionaryEntry,
                                ref senseIndex,
                                ref synonymIndex);
                            break;
                        case LexicalCategory.Unknown:
                            inflections = DeclineUnknownDictionaryFormDefault(
                                dictionaryEntry,
                                ref senseIndex,
                                ref synonymIndex);
                            break;
                    }
                }
            }

            return inflections;
        }

        public virtual List<Inflection> InflectAnyDictionaryFormScoped(
            string scope,
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex)
        {
            Sense sense;
            string definition;
            LexicalCategory category;
            List<Inflection> inflections = null;

            if (GetSenseCategoryDefinition(
                    dictionaryEntry,
                    LanguageLookup.English,
                    ref senseIndex,
                    ref synonymIndex,
                    out sense,
                    out category,
                    out definition))
            {
                if (!CanInflectSense(sense))
                {
                    Inflection inflection;
                    Designator designation = new Designator("Dictionary", new Classifier("Special", "Dictionary"));

                    if (GetNonInflectedDesignated(
                            dictionaryEntry,
                            ref senseIndex,
                            ref synonymIndex,
                            designation,
                            out inflection))
                        inflections = new List<Inflection>() { inflection };
                }
                else
                {
                    StemSubstitutionCheck(ref dictionaryEntry, ref sense, ref senseIndex);

                    switch (category)
                    {
                        case LexicalCategory.Verb:
                            inflections = ConjugateVerbDictionaryFormScoped(
                                scope,
                                dictionaryEntry,
                                ref senseIndex,
                                ref synonymIndex);
                            break;
                        case LexicalCategory.Adjective:
                            inflections = DeclineAdjectiveDictionaryFormScoped(
                                scope,
                                dictionaryEntry,
                                ref senseIndex,
                                ref synonymIndex);
                            break;
                        case LexicalCategory.Noun:
                            inflections = DeclineNounDictionaryFormScoped(
                                scope,
                                dictionaryEntry,
                                ref senseIndex,
                                ref synonymIndex);
                            break;
                        case LexicalCategory.Unknown:
                            inflections = DeclineUnknownDictionaryFormScoped(
                                scope,
                                dictionaryEntry,
                                ref senseIndex,
                                ref synonymIndex);
                            break;
                    }
                }
            }

            return inflections;
        }

        public virtual List<Inflection> InflectAnyDictionaryFormSelected(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex,
            List<Designator> designations)
        {
            Sense sense;
            string definition;
            LexicalCategory category;
            List<Inflection> inflections = null;

            if (GetSenseCategoryDefinition(
                    dictionaryEntry,
                    LanguageLookup.English,
                    ref senseIndex,
                    ref synonymIndex,
                    out sense,
                    out category,
                    out definition))
            {
                if (!CanInflectSense(sense))
                {
                    Inflection inflection;
                    Designator designation = new Designator("Dictionary", new Classifier("Special", "Dictionary"));

                    if (GetNonInflectedDesignated(
                            dictionaryEntry,
                            ref senseIndex,
                            ref synonymIndex,
                            designation,
                            out inflection))
                        inflections = new List<Inflection>() { inflection };
                }
                else
                {
                    StemSubstitutionCheck(ref dictionaryEntry, ref sense, ref senseIndex);

                    switch (category)
                    {
                        case LexicalCategory.Verb:
                            inflections = ConjugateVerbDictionaryFormSelected(
                                dictionaryEntry,
                                ref senseIndex,
                                ref synonymIndex,
                                designations);
                            break;
                        case LexicalCategory.Adjective:
                            inflections = DeclineAdjectiveDictionaryFormSelected(
                                dictionaryEntry,
                                ref senseIndex,
                                ref synonymIndex,
                                designations);
                            break;
                        case LexicalCategory.Noun:
                            inflections = DeclineNounDictionaryFormSelected(
                                dictionaryEntry,
                                ref senseIndex,
                                ref synonymIndex,
                                designations);
                            break;
                        case LexicalCategory.Unknown:
                            inflections = DeclineUnknownDictionaryFormSelected(
                                dictionaryEntry,
                                ref senseIndex,
                                ref synonymIndex,
                                designations);
                            break;
                    }
                }
            }

            return inflections;
        }

        public virtual bool InflectAnyDictionaryFormDesignated(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex,
            Designator designation,
            out Inflection inflection)
        {
            Sense sense;
            string definition;
            LexicalCategory category;
            bool returnValue = false;

            inflection = null;

            if (GetSenseCategoryDefinition(
                    dictionaryEntry,
                    LanguageLookup.English,
                    ref senseIndex,
                    ref synonymIndex,
                    out sense,
                    out category,
                    out definition))
            {
                if (!CanInflectSense(sense))
                    returnValue = GetNonInflectedDesignated(
                        dictionaryEntry,
                        ref senseIndex,
                        ref synonymIndex,
                        designation,
                        out inflection);
                else
                {
                    StemSubstitutionCheck(ref dictionaryEntry, ref sense, ref senseIndex);

                    switch (category)
                    {
                        case LexicalCategory.Verb:
                            returnValue = ConjugateVerbDictionaryFormDesignated(
                                dictionaryEntry,
                                ref senseIndex,
                                ref synonymIndex,
                                designation,
                                out inflection);
                            break;
                        case LexicalCategory.Adjective:
                            returnValue = DeclineAdjectiveDictionaryFormDesignated(
                                dictionaryEntry,
                                ref senseIndex,
                                ref synonymIndex,
                                designation,
                                out inflection);
                            break;
                        case LexicalCategory.Noun:
                            returnValue = DeclineNounDictionaryFormDesignated(
                                dictionaryEntry,
                                ref senseIndex,
                                ref synonymIndex,
                                designation,
                                out inflection);
                            break;
                        case LexicalCategory.Unknown:
                            returnValue = DeclineUnknownDictionaryFormDesignated(
                                dictionaryEntry,
                                ref senseIndex,
                                ref synonymIndex,
                                designation,
                                out inflection);
                            break;
                    }
                }
            }

            return returnValue;
        }

        public virtual List<List<Designator>> GetAllDesignationsLists()
        {
            List<List<Designator>> designatorLists = new List<List<Designator>>();

            if (CanInflect("Verb"))
                designatorLists.Add(GetAllVerbDesignations());

            if (CanInflect("Adjective"))
                designatorLists.Add(GetAllAdjectiveDesignations());

            if (CanInflect("Noun"))
                designatorLists.Add(GetAllNounDesignations());

            if (CanInflect("Unknown"))
                designatorLists.Add(GetAllUnknownDesignations());

            return designatorLists;
        }

        public virtual List<Designator> GetAllCategoryDesignations(LexicalCategory category)
        {
            List<Designator> designators = null;
            switch (category)
            {
                case LexicalCategory.Verb:
                    designators = GetAllVerbDesignations();
                    break;
                case LexicalCategory.Adjective:
                    designators = GetAllAdjectiveDesignations();
                    break;
                case LexicalCategory.Noun:
                    designators = GetAllNounDesignations();
                    break;
                case LexicalCategory.Unknown:
                    designators = GetAllUnknownDesignations();
                    break;
                default:
                    break;
            }
            return designators;
        }

        public virtual List<Inflection> InflectCategoryDictionaryFormAll(
            LexicalCategory category,
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex)
        {
            List<Inflection> inflections = null;

            switch (category)
            {
                case LexicalCategory.Verb:
                    inflections = ConjugateVerbDictionaryFormAll(
                        dictionaryEntry,
                        ref senseIndex,
                        ref synonymIndex);
                    break;
                case LexicalCategory.Adjective:
                    inflections = DeclineAdjectiveDictionaryFormAll(
                        dictionaryEntry,
                        ref senseIndex,
                        ref synonymIndex);
                    break;
                case LexicalCategory.Noun:
                    inflections = DeclineNounDictionaryFormAll(
                        dictionaryEntry,
                        ref senseIndex,
                        ref synonymIndex);
                    break;
                case LexicalCategory.Unknown:
                    inflections = DeclineUnknownDictionaryFormAll(
                        dictionaryEntry,
                        ref senseIndex,
                        ref synonymIndex);
                    break;
            }

            return inflections;
        }

        public virtual List<Inflection> InflectCategoryDictionaryFormDefault(
            LexicalCategory category,
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex)
        {
            List<Inflection> inflections = null;

            switch (category)
            {
                case LexicalCategory.Verb:
                    inflections = ConjugateVerbDictionaryFormDefault(
                        dictionaryEntry,
                        ref senseIndex,
                        ref synonymIndex);
                    break;
                case LexicalCategory.Adjective:
                    inflections = DeclineAdjectiveDictionaryFormDefault(
                        dictionaryEntry,
                        ref senseIndex,
                        ref synonymIndex);
                    break;
                case LexicalCategory.Noun:
                    inflections = DeclineNounDictionaryFormDefault(
                        dictionaryEntry,
                        ref senseIndex,
                        ref synonymIndex);
                    break;
                case LexicalCategory.Unknown:
                    inflections = DeclineUnknownDictionaryFormDefault(
                        dictionaryEntry,
                        ref senseIndex,
                        ref synonymIndex);
                    break;
            }

            return inflections;
        }

        public virtual List<Inflection> InflectCategoryDictionaryFormSelected(
            LexicalCategory category,
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex,
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
                case LexicalCategory.Unknown:
                    inflections = DeclineUnknownDictionaryFormSelected(
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
            List<Designator> designations = GetAllVerbDesignations();
            List<Inflection> inflections = ConjugateVerbDictionaryFormSelected(
                dictionaryEntry,
                ref senseIndex,
                ref synonymIndex,
                designations);
            return inflections;
        }

        public virtual List<Designator> GetAllVerbDesignations()
        {
            List<Designator> designators = null;
            InflectorTable inflectorTable = InflectorTable("Verb");
            if (inflectorTable != null)
            {
                DesignatorTable designatorTable = inflectorTable.GetDesignatorTable("All");
                if (designatorTable != null)
                    designators = designatorTable.Designators;
            }
            return designators;
        }

        public virtual List<Inflection> ConjugateVerbDictionaryFormDefault(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex)
        {
            List<Designator> designations = GetDefaultVerbDesignations();
            List<Inflection> inflections = ConjugateVerbDictionaryFormSelected(
                dictionaryEntry,
                ref senseIndex,
                ref synonymIndex,
                designations);
            return inflections;
        }

        public virtual List<Designator> GetDefaultVerbDesignations()
        {
            List<Designator> designators = null;
            InflectorTable inflectorTable = InflectorTable("Verb");
            if (inflectorTable != null)
            {
                DesignatorTable designatorTable = inflectorTable.GetDesignatorTable("Default");
                if (designatorTable != null)
                    designators = designatorTable.Designators;
            }
            return designators;
        }

        public virtual List<Inflection> ConjugateVerbDictionaryFormScoped(
            string scope,
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex)
        {
            List<Designator> designations = GetScopedVerbDesignations(scope);
            List<Inflection> inflections = ConjugateVerbDictionaryFormSelected(
                dictionaryEntry,
                ref senseIndex,
                ref synonymIndex,
                designations);
            return inflections;
        }

        public virtual List<Designator> GetScopedVerbDesignations(string scope)
        {
            List<Designator> designators = null;
            InflectorTable inflectorTable = InflectorTable("Verb");
            if (inflectorTable != null)
            {
                DesignatorTable designatorTable = inflectorTable.GetDesignatorTable(scope);
                if (designatorTable != null)
                    designators = designatorTable.Designators;
            }
            return designators;
        }

        public virtual List<Inflection> ConjugateVerbDictionaryFormSelected(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex,
            List<Designator> designations)
        {
            if (designations == null)
                return null;
            List<Inflection> inflections = new List<Inflection>();
            foreach (Designator designation in designations)
            {
                Inflection inflection;
                if (ConjugateVerbDictionaryFormDesignated(
                        dictionaryEntry,
                        ref senseIndex,
                        ref synonymIndex,
                        designation,
                        out inflection))
                    inflections.Add(inflection);
            }
            return inflections;
        }

        public virtual bool ConjugateVerbDictionaryFormDesignated(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex,
            Designator designation,
            out Inflection inflection)
        {
            InflectorTable inflectorTable = InflectorTable("Verb");
            bool returnValue = false;

            inflection = null;

            if (inflectorTable != null)
            {
                returnValue = TableInflect(
                    dictionaryEntry,
                    ref senseIndex,
                    ref synonymIndex,
                    inflectorTable,
                    designation,
                    out inflection);
            }

            return returnValue;
        }

        public virtual List<Inflection> DeclineNounDictionaryFormAll(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex)
        {
            List<Designator> designations = GetAllNounDesignations();
            List<Inflection> inflections = DeclineNounDictionaryFormSelected(
                dictionaryEntry,
                ref senseIndex,
                ref synonymIndex,
                designations);
            return inflections;
        }

        public virtual List<Designator> GetAllNounDesignations()
        {
            List<Designator> designators = null;
            InflectorTable inflectorTable = InflectorTable("Noun");
            if (inflectorTable != null)
            {
                DesignatorTable designatorTable = inflectorTable.GetDesignatorTable("All");
                if (designatorTable != null)
                    designators = designatorTable.Designators;
            }
            return designators;
        }

        public virtual List<Inflection> DeclineNounDictionaryFormDefault(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex)
        {
            List<Designator> designations = GetDefaultNounDesignations();
            List<Inflection> inflections = DeclineNounDictionaryFormSelected(
                dictionaryEntry,
                ref senseIndex,
                ref synonymIndex,
                designations);
            return inflections;
        }

        public virtual List<Designator> GetDefaultNounDesignations()
        {
            List<Designator> designators = null;
            InflectorTable inflectorTable = InflectorTable("Noun");
            if (inflectorTable != null)
            {
                DesignatorTable designatorTable = inflectorTable.GetDesignatorTable("Default");
                if (designatorTable != null)
                    designators = designatorTable.Designators;
            }
            return designators;
        }

        public virtual List<Inflection> DeclineNounDictionaryFormScoped(
            string scope,
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex)
        {
            List<Designator> designations = GetScopedNounDesignations(scope);
            List<Inflection> inflections = DeclineNounDictionaryFormSelected(
                dictionaryEntry,
                ref senseIndex,
                ref synonymIndex,
                designations);
            return inflections;
        }

        public virtual List<Designator> GetScopedNounDesignations(string scope)
        {
            List<Designator> designators = null;
            InflectorTable inflectorTable = InflectorTable("Noun");
            if (inflectorTable != null)
            {
                DesignatorTable designatorTable = inflectorTable.GetDesignatorTable(scope);
                if (designatorTable != null)
                    designators = designatorTable.Designators;
            }
            return designators;
        }

        public virtual List<Inflection> DeclineNounDictionaryFormSelected(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex,
            List<Designator> designations)
        {
            if (designations == null)
                return null;
            List<Inflection> inflections = new List<Inflection>();
            foreach (Designator designation in designations)
            {
                Inflection inflection;
                if (DeclineNounDictionaryFormDesignated(
                        dictionaryEntry,
                        ref senseIndex,
                        ref synonymIndex,
                        designation,
                        out inflection))
                    inflections.Add(inflection);
            }
            return inflections;
        }

        public virtual bool DeclineNounDictionaryFormDesignated(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex,
            Designator designation,
            out Inflection inflection)
        {
            InflectorTable inflectorTable = InflectorTable("Noun");
            bool returnValue = false;

            inflection = null;

            if (inflectorTable != null)
                returnValue = TableInflect(
                    dictionaryEntry,
                    ref senseIndex,
                    ref synonymIndex,
                    inflectorTable,
                    designation,
                    out inflection);

            return returnValue;
        }

        public virtual List<Inflection> DeclineAdjectiveDictionaryFormAll(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex)
        {
            List<Designator> designations = GetAllAdjectiveDesignations();
            List<Inflection> inflections = DeclineAdjectiveDictionaryFormSelected(
                dictionaryEntry,
                ref senseIndex,
                ref synonymIndex,
                designations);
            return inflections;
        }

        public virtual List<Designator> GetAllAdjectiveDesignations()
        {
            List<Designator> designators = null;
            InflectorTable inflectorTable = InflectorTable("Adjective");
            if (inflectorTable != null)
            {
                DesignatorTable designatorTable = inflectorTable.GetDesignatorTable("All");
                if (designatorTable != null)
                    designators = designatorTable.Designators;
            }
            return designators;
        }

        public virtual List<Inflection> DeclineAdjectiveDictionaryFormDefault(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex)
        {
            List<Designator> designations = GetDefaultAdjectiveDesignations();
            List<Inflection> inflections = DeclineAdjectiveDictionaryFormSelected(
                dictionaryEntry,
                ref senseIndex,
                ref synonymIndex,
                designations);
            return inflections;
        }

        public virtual List<Designator> GetDefaultAdjectiveDesignations()
        {
            List<Designator> designators = null;
            InflectorTable inflectorTable = InflectorTable("Adjective");
            if (inflectorTable != null)
            {
                DesignatorTable designatorTable = inflectorTable.GetDesignatorTable("Default");
                if (designatorTable != null)
                    designators = designatorTable.Designators;
            }
            return designators;
        }

        public virtual List<Inflection> DeclineAdjectiveDictionaryFormScoped(
            string scope,
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex)
        {
            List<Designator> designations = GetScopedAdjectiveDesignations(scope);
            List<Inflection> inflections = DeclineAdjectiveDictionaryFormSelected(
                dictionaryEntry,
                ref senseIndex,
                ref synonymIndex,
                designations);
            return inflections;
        }

        public virtual List<Designator> GetScopedAdjectiveDesignations(string scope)
        {
            List<Designator> designators = null;
            InflectorTable inflectorTable = InflectorTable("Adjective");
            if (inflectorTable != null)
            {
                DesignatorTable designatorTable = inflectorTable.GetDesignatorTable(scope);
                if (designatorTable != null)
                    designators = designatorTable.Designators;
            }
            return designators;
        }

        public virtual List<Inflection> DeclineAdjectiveDictionaryFormSelected(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex,
            List<Designator> designations)
        {
            if (designations == null)
                return null;
            List<Inflection> inflections = new List<Inflection>();
            foreach (Designator designation in designations)
            {
                Inflection inflection;
                if (DeclineAdjectiveDictionaryFormDesignated(
                        dictionaryEntry,
                        ref senseIndex,
                        ref synonymIndex,
                        designation,
                        out inflection))
                    inflections.Add(inflection);
            }
            return inflections;
        }

        public virtual bool DeclineAdjectiveDictionaryFormDesignated(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex,
            Designator designation,
            out Inflection inflection)
        {
            InflectorTable inflectorTable = InflectorTable("Adjective");
            bool returnValue = false;

            inflection = null;

            if (inflectorTable != null)
                returnValue = TableInflect(
                    dictionaryEntry,
                    ref senseIndex,
                    ref synonymIndex,
                    inflectorTable,
                    designation,
                    out inflection);

            return returnValue;
        }

        public virtual List<Inflection> DeclineUnknownDictionaryFormAll(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex)
        {
            List<Designator> designations = GetAllUnknownDesignations();
            List<Inflection> inflections = DeclineUnknownDictionaryFormSelected(
                dictionaryEntry,
                ref senseIndex,
                ref synonymIndex,
                designations);
            return inflections;
        }

        public virtual List<Designator> GetAllUnknownDesignations()
        {
            List<Designator> designators = null;
            InflectorTable inflectorTable = InflectorTable("Unknown");
            if (inflectorTable != null)
            {
                DesignatorTable designatorTable = inflectorTable.GetDesignatorTable("All");
                if (designatorTable != null)
                    designators = designatorTable.Designators;
            }
            return designators;
        }

        public virtual List<Inflection> DeclineUnknownDictionaryFormDefault(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex)
        {
            List<Designator> designations = GetDefaultUnknownDesignations();
            List<Inflection> inflections = DeclineUnknownDictionaryFormSelected(
                dictionaryEntry,
                ref senseIndex,
                ref synonymIndex,
                designations);
            return inflections;
        }

        public virtual List<Designator> GetDefaultUnknownDesignations()
        {
            List<Designator> designators = null;
            InflectorTable inflectorTable = InflectorTable("Unknown");
            if (inflectorTable != null)
            {
                DesignatorTable designatorTable = inflectorTable.GetDesignatorTable("Default");
                if (designatorTable != null)
                    designators = designatorTable.Designators;
            }
            return designators;
        }

        public virtual List<Inflection> DeclineUnknownDictionaryFormScoped(
            string scope,
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex)
        {
            List<Designator> designations = GetScopedUnknownDesignations(scope);
            List<Inflection> inflections = DeclineUnknownDictionaryFormSelected(
                dictionaryEntry,
                ref senseIndex,
                ref synonymIndex,
                designations);
            return inflections;
        }

        public virtual List<Designator> GetScopedUnknownDesignations(string scope)
        {
            List<Designator> designators = null;
            InflectorTable inflectorTable = InflectorTable("Unknown");
            if (inflectorTable != null)
            {
                DesignatorTable designatorTable = inflectorTable.GetDesignatorTable(scope);
                if (designatorTable != null)
                    designators = designatorTable.Designators;
            }
            return designators;
        }

        public virtual List<Inflection> DeclineUnknownDictionaryFormSelected(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex,
            List<Designator> designations)
        {
            if (designations == null)
                return null;
            List<Inflection> inflections = new List<Inflection>();
            foreach (Designator designation in designations)
            {
                Inflection inflection;
                if (DeclineUnknownDictionaryFormDesignated(
                        dictionaryEntry,
                        ref senseIndex,
                        ref synonymIndex,
                        designation,
                        out inflection))
                    inflections.Add(inflection);
            }
            return inflections;
        }

        public virtual bool DeclineUnknownDictionaryFormDesignated(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex,
            Designator designation,
            out Inflection inflection)
        {
            InflectorTable inflectorTable = InflectorTable("Unknown");
            bool returnValue = false;

            inflection = null;

            if (inflectorTable != null)
                returnValue = TableInflect(
                    dictionaryEntry,
                    ref senseIndex,
                    ref synonymIndex,
                    inflectorTable,
                    designation,
                    out inflection);

            return returnValue;
        }

        public virtual bool GetNonInflectedDesignated(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex,
            Designator designation,
            out Inflection inflection)
        {
            Sense sense;
            LexicalCategory category;
            string definition;
            bool returnValue = true;

            inflection = new Inflection(
                dictionaryEntry,
                designation,
                TargetLanguageIDs);

            if (GetSenseCategoryDefinition(
                    dictionaryEntry,
                    LanguageLookup.English,
                    ref senseIndex,
                    ref synonymIndex,
                    out sense,
                    out category,
                    out definition))
            {
                foreach (LanguageID targetLanguageID in TargetLanguageIDs)
                {
                    definition = dictionaryEntry.GetDefinition(targetLanguageID, false, false);
                    inflection.SetOutput(targetLanguageID, definition);
                    inflection.SetPronounOutput(targetLanguageID, definition);
                    inflection.SetRegularOutput(targetLanguageID, definition);
                    inflection.SetRegularPronounOutput(targetLanguageID, definition);
                    inflection.SetRoot(targetLanguageID, definition);
                }

                inflection.Category = category;
                inflection.CategoryString = sense.CategoryString;
            }
            else
                returnValue = false;

            return returnValue;
        }

        public virtual bool GetDictionaryFormAndCategories(
            DictionaryEntry dictionaryEntry,
            int senseIndex,
            int synonymIndex,
            bool isTarget,
            InflectorTable inflectorTable,
            out MultiLanguageString dictionaryForm,
            out MultiLanguageString stem,
            out MultiLanguageString compoundFixupPatternMLS,
            out LexicalCategory category,
            out string categoryString,
            out string className,
            out string subClassName)
        {
            Sense sense;
            ProbableMeaning hostMeaning;

            if (!dictionaryEntry.GetMultiLanguageStringAndSense(
                    senseIndex,
                    synonymIndex,
                    "Input",
                    TargetLanguageIDs,
                    out hostMeaning,
                    out sense,
                    out dictionaryForm))
            {
                stem = null;
                compoundFixupPatternMLS = null;
                category = inflectorTable.Category;
                categoryString = null;
                className = null;
                subClassName = null;
                return false;
            }

            CleanDictionaryFormMultiLanguageString(dictionaryForm, out compoundFixupPatternMLS);

            category = sense.Category;

            if (isTarget)
                categoryString = sense.CategoryString;
            else if (hostMeaning != null)
                categoryString = hostMeaning.CategoryString;
            else
                categoryString = sense.CategoryString;

            if (inflectorTable.HasCategoryStringToClassMap())
            {
                if (!inflectorTable.GetClassesFromCategoryString(categoryString, out className, out subClassName))
                {
                    className = null;
                    subClassName = null;
                }
            }
            else
            {
                GetClassesFromCategoryString(
                    categoryString,
                    out className,
                    out subClassName);
            }

            string stemCategoryString;

            stem = GetStem(dictionaryForm, TargetLanguageIDs, out stemCategoryString);

            bool returnValue = FixupDictionaryFormAndCategories(
                dictionaryForm,
                stem,
                inflectorTable.Category,
                ref category,
                ref categoryString,
                ref className,
                ref subClassName);

            return returnValue;
        }

        public virtual bool GetClassesFromCategoryString(
            string categoryString,
            out string className,
            out string subClassName)
        {
            className = null;
            subClassName = null;
            return false;
        }

        public virtual bool FixupDictionaryFormAndCategories(
            MultiLanguageString dictionaryForm,
            MultiLanguageString stem,
            LexicalCategory expectedCategory,
            ref LexicalCategory category,
            ref string categoryString,
            ref string className,
            ref string subClassName)
        {
            return true;
        }

        public virtual bool FixupProbableMeaningCategories(
            ProbableMeaning probableMeaning,
            LanguageID languageID)
        {
            return true;
        }

        public virtual void CleanDictionaryFormMultiLanguageString(
            MultiLanguageString dictionaryForm,
            out MultiLanguageString compoundFixupPatternMLS)
        {
            compoundFixupPatternMLS = null;

            if (dictionaryForm == null)
                return;

            if (dictionaryForm.LanguageStrings == null)
                return;

            foreach (LanguageID languageID in LanguageIDs)
            {
                LanguageString languageString = dictionaryForm.LanguageString(languageID);

                if (languageString == null)
                    continue;

                if (!languageString.HasText())
                    continue;

                string compoundFixupPattern;

                languageString.Text = CleanDictionaryFormString(
                    languageString.Text,
                    out compoundFixupPattern);

                if (!String.IsNullOrEmpty(compoundFixupPattern))
                {
                    if (compoundFixupPatternMLS == null)
                        compoundFixupPatternMLS = new MultiLanguageString("Fixup", LanguageIDs);

                    compoundFixupPatternMLS.SetText(languageID, compoundFixupPattern);
                }
            }
        }

        public virtual string CleanDictionaryFormString(
            string dictionaryForm,
            out string compoundFixupPattern)
        {
            compoundFixupPattern = null;

            if (String.IsNullOrEmpty(dictionaryForm))
                return dictionaryForm;

            dictionaryForm = TextUtilities.FilterAsides(dictionaryForm);
            dictionaryForm = dictionaryForm.Trim();

            return dictionaryForm;
        }

        public virtual bool Contract(
            MultiLanguageString uncontracted,
            out List<MultiLanguageString> contracted)
        {
            contracted = null;
            return false;
        }

        public virtual bool ExpandContraction(
            MultiLanguageString possiblyContracted,
            out List<MultiLanguageString> expanded)
        {
            expanded = null;
            return false;
        }

        public virtual bool HandleStem(
            ref DictionaryEntry dictionaryEntry,
            string remainingText,
            bool wholeWord,
            List<DictionaryEntry> formAndStemDictionaryEntries,
            ref string inflectionText,
            out bool isInflection)
        {
            string stemText = dictionaryEntry.KeyString;
            List<DictionaryEntry> inflectionEntries;
            bool returnValue = false;

            isInflection = false;

            if (GetPossibleInflections(
                dictionaryEntry,
                stemText,
                remainingText,
                wholeWord,
                out inflectionEntries))
            {
                if (inflectionEntries.Count() >= 1)
                {
                    if (formAndStemDictionaryEntries != null)
                    {
                        if (formAndStemDictionaryEntries.FirstOrDefault(x => x.MatchKey(stemText)) == null)
                        {
                            formAndStemDictionaryEntries.Add(dictionaryEntry);
                            GetStemDictionaryFormEntry(dictionaryEntry, formAndStemDictionaryEntries);
                        }
                    }

                    dictionaryEntry = inflectionEntries.First();
                    returnValue = true;
                    inflectionText = dictionaryEntry.KeyString;
                    isInflection = true;
                }
            }

            return returnValue;
        }

        public virtual string GetStem(
            string word,
            LanguageID languageID,
            out string categoryString)
        {
            string classCode;
            string subClassCode;
            string stem = GetStemAndClasses(
                word,
                languageID,
                out categoryString,
                out classCode,
                out subClassCode);
            return stem;
        }

        public virtual MultiLanguageString GetStem(
            MultiLanguageString word,
            List<LanguageID> languageIDs,
            out string categoryString)
        {
            MultiLanguageString multiLanguageString = new MultiLanguageString("Stem", languageIDs);

            categoryString = null;

            foreach (LanguageID languageID in languageIDs)
                multiLanguageString.SetText(
                    languageID,
                    GetStem(
                        word.Text(languageID),
                        languageID,
                        out categoryString));

            return multiLanguageString;
        }

        public virtual string GetStemAndClasses(
            string word,
            LanguageID languageID,
            out string categoryString,
            out string classCode,
            out string subClassCode)
        {
            string stem = null;

            categoryString = null;
            classCode = null;
            subClassCode = null;

            if (!String.IsNullOrEmpty(VerbStemType) && (_VerbClasses != null) && (_VerbClasses.Count() != 0))
            {
                switch (VerbStemType)
                {
                    case "RemoveVerbClass":
                        stem = RemoveEnding(word, VerbClasses, out classCode);
                        categoryString = "v," + classCode;
                        return stem;
                }
            }

            return stem;
        }

        public virtual MultiLanguageString GetStemAndClasses(
            MultiLanguageString word,
            List<LanguageID> languageIDs,
            out string categoryString,
            out string classCode,
            out string subClassCode)
        {
            MultiLanguageString multiLanguageString = new MultiLanguageString("Stem", languageIDs);

            categoryString = null;
            classCode = null;
            subClassCode = null;

            foreach (LanguageID languageID in languageIDs)
            {
                string stem = GetStemAndClasses(
                    word.Text(languageID),
                    languageID,
                    out categoryString,
                    out classCode,
                    out subClassCode);

                multiLanguageString.SetText(
                    languageID,
                    stem);
            }

            return multiLanguageString;
        }

        public string RemoveEnding(
            string word,
            List<string> endings,
            out string suffix)
        {
            string str = null;

            suffix = null;

            foreach (string ending in endings)
            {
                if (word.EndsWith(ending))
                {
                    str = word.Substring(0, word.Length - ending.Length);
                    suffix = ending;
                    break;
                }
            }

            return str;
        }

        public virtual bool GetIrregularStems(
            string type,
            string dictionaryForm,
            string stemForm,
            LanguageID languageID,
            out List<string> irregularStems,
            out string categoryStringTerm)
        {
            bool isIrregular = IsIrregular(type, dictionaryForm, out categoryStringTerm);
            bool returnValue = false;

            irregularStems = null;

            if (isIrregular)
            {
                LexicalCategory category = Sense.GetLexicalCategoryFromString(type);
                string categoryString;
                int senseIndex = -1;
                int synonymIndex = -1;
                DictionaryEntry dictionaryEntry = GetDictionaryEntry(dictionaryForm);

                if (dictionaryEntry == null)
                    return false;

                List<Inflection> inflections = InflectCategoryDictionaryFormAll(
                    category,
                    dictionaryEntry,
                    ref senseIndex,
                    ref synonymIndex);

                if (inflections == null)
                    return false;

                string stem = GetStem(dictionaryForm, languageID, out categoryString);

                foreach (Inflection inflection in inflections)
                {
                    string inflectionRoot = inflection.GetRoot(languageID);

                    if (inflectionRoot != stem)
                    {
                        if (irregularStems == null)
                        {
                            irregularStems = new List<string>() { inflectionRoot };
                            returnValue = true;
                        }
                        else if (!irregularStems.Contains(inflectionRoot))
                        {
                            irregularStems.Add(inflectionRoot);
                            returnValue = true;
                        }
                    }
                }
            }

            return returnValue;
        }

        public virtual bool GetIrregularStems(
            string type,
            DictionaryEntry dictionaryEntry,
            string stemForm,
            LanguageID languageID,
            out List<string> irregularStems,
            out string categoryStringTerm)
        {
            string dictionaryForm = dictionaryEntry.KeyString;
            bool isIrregular = IsIrregular(type, dictionaryForm, out categoryStringTerm);
            bool returnValue = false;

            irregularStems = null;

            if (isIrregular)
            {
                LexicalCategory category = Sense.GetLexicalCategoryFromString(type);
                int senseIndex = -1;
                int synonymIndex = -1;

                if (dictionaryEntry == null)
                    return false;

                List<Inflection> inflections = InflectCategoryDictionaryFormAll(
                    category,
                    dictionaryEntry,
                    ref senseIndex,
                    ref synonymIndex);

                if (inflections == null)
                    return false;

                foreach (Inflection inflection in inflections)
                {
                    string inflectionRoot = inflection.GetRoot(languageID);

                    if (inflectionRoot != stemForm)
                    {
                        if (irregularStems == null)
                        {
                            irregularStems = new List<string>() { inflectionRoot };
                            returnValue = true;
                        }
                        else if (!irregularStems.Contains(inflectionRoot))
                        {
                            irregularStems.Add(inflectionRoot);
                            returnValue = true;
                        }
                    }
                }
            }

            return returnValue;
        }

        public virtual bool IsIrregular(
            string type,
            string dictionaryForm,
            out string categoryStringTerm)
        {
            SemiRegular semiRegular = GetSemiRegular(type, dictionaryForm);
            if (semiRegular != null)
            {
                categoryStringTerm = semiRegular.Name;
                return true;
            }
            else
                categoryStringTerm = null;
            return false;
        }

        public virtual SemiRegular GetSemiRegular(
            string type,
            string dictionaryForm)
        {
            InflectorTable inflectorTable = InflectorTable(type);
            Dictionary<string, SemiRegular> irregularDictionary = inflectorTable.IrregularDictionary;

            if (irregularDictionary != null)
            {
                SemiRegular irregular;

                if (irregularDictionary.TryGetValue(dictionaryForm, out irregular))
                    return irregular;
            }

            List<SemiRegular> semiRegulars = inflectorTable.SemiRegulars;

            if (semiRegulars != null)
            {
                foreach (SemiRegular semiRegularTarget in semiRegulars)
                {
                    if (semiRegularTarget.HasTargets())
                    {
                        if (semiRegularTarget.MatchTarget(dictionaryForm))
                            return semiRegularTarget;
                    }
                }

                foreach (SemiRegular semiRegularCondition in semiRegulars)
                {
                    if (semiRegularCondition.HasTargets())
                        continue;

                    // Don't really know what to do about Post semiregulars.
                    if (semiRegularCondition.Post)
                    {
                        if (PreMatchConditionPost(dictionaryForm, semiRegularCondition))
                            return semiRegularCondition;

                        if (semiRegularCondition.HasDictionaryTargets())
                        {
                            if (semiRegularCondition.MatchDictionaryTarget(dictionaryForm))
                                return semiRegularCondition;
                        }
                    }
                    else
                    {
                        if (semiRegularCondition.MatchCondition(dictionaryForm, this))
                            return semiRegularCondition;
                    }
                }
            }

            return null;
        }

        // Handle trigger functions.
        public virtual bool ModifierTriggerFunction(
            string triggerFunction,
            MultiLanguageString dictionaryForm)
        {
            string triggerFunctionName;
            List<string> triggerArguments;

            ParseTriggerFunctionCall(triggerFunction, out triggerFunctionName, out triggerArguments);

            return ModifierTriggerFunctionCall(
                triggerFunction,
                triggerFunctionName,
                triggerArguments,
                dictionaryForm);
        }

        // Handle trigger function call.
        public virtual bool ModifierTriggerFunctionCall(
            string triggerFunctionCall,
            string triggerFunctionName,
            List<string> triggerArguments,
            MultiLanguageString dictionaryForm)
        {
            switch (triggerFunctionName)
            {
                case "And":
                    foreach (string argument in triggerArguments)
                    {
                        if (!ModifierTriggerFunction(argument, dictionaryForm))
                            return false;
                    }
                    return true;
                case "Or":
                    foreach (string argument in triggerArguments)
                    {
                        if (ModifierTriggerFunction(argument, dictionaryForm))
                            return true;
                    }
                    return false;
                case "Not":
                    foreach (string argument in triggerArguments)
                    {
                        if (!ModifierTriggerFunction(argument, dictionaryForm))
                            return true;
                    }
                    return false;
                case "EndsWith":
                    foreach (LanguageString ls in dictionaryForm.LanguageStrings)
                    {
                        foreach (string triggerArgument in triggerArguments)
                        {
                            if (ls.Text.EndsWith(triggerArgument.ToLower()))
                                return true;
                        }
                    }
                    break;
                case "NotEndsWith":
                    foreach (LanguageString ls in dictionaryForm.LanguageStrings)
                    {
                        foreach (string triggerArgument in triggerArguments)
                        {
                            if (!ls.Text.EndsWith(triggerArgument.ToLower()))
                                return true;
                        }
                    }
                    break;
                case "StartsWith":
                    foreach (LanguageString ls in dictionaryForm.LanguageStrings)
                    {
                        foreach (string triggerArgument in triggerArguments)
                        {
                            if (ls.Text.StartsWith(triggerArgument.ToLower()))
                                return true;
                        }
                    }
                    break;
                case "NotStartsWith":
                    foreach (LanguageString ls in dictionaryForm.LanguageStrings)
                    {
                        foreach (string triggerArgument in triggerArguments)
                        {
                            if (!ls.Text.StartsWith(triggerArgument.ToLower()))
                                return true;
                        }
                    }
                    break;
                case "StartsWithVowel":
                    foreach (LanguageString ls in dictionaryForm.LanguageStrings)
                    {
                        if (StartsWithVowel(ls.Text))
                            return true;
                    }
                    break;
                case "NotStartsWithVowel":
                    foreach (LanguageString ls in dictionaryForm.LanguageStrings)
                    {
                        if (!StartsWithVowel(ls.Text))
                            return true;
                    }
                    break;
                default:
                    throw new Exception("LanguageTool.ModifierTriggerFunctionCall: Unsupported trigger function: " + triggerFunctionCall);
            }

            return false;
        }

        protected virtual void ParseTriggerFunctionCall(
            string triggerfunctionCall,
            out string triggerFunctionName,
            out List<string> triggerArguments)
        {
            int ofs = triggerfunctionCall.IndexOf('(');

            triggerArguments = null;

            if (ofs == -1)
                triggerFunctionName = triggerfunctionCall;
            else
            {
                int endOfs = triggerfunctionCall.LastIndexOf(')');

                if (endOfs == -1)
                    throw new Exception("ParseTriggerFunctionCall: No ending parenthesis.");

                triggerFunctionName = triggerfunctionCall.Substring(0, ofs);
                string argListString = triggerfunctionCall.Substring(ofs + 1, (endOfs - ofs) - 1);
                triggerArguments = TextUtilities.GetArgumentListFromString(argListString);
            }
        }

        public string PullTriggerArgument(
            List<string> triggerArguments,
            int index)
        {
            if ((triggerArguments == null) || (index >= triggerArguments.Count()))
                throw new Exception("PullTriggerArgument: index out of range.");

            return triggerArguments[index];
        }

        public virtual bool PreMatchConditionPost(
            string dictionaryForm,
            SemiRegular semiRegular)
        {
            return false;
        }

        public virtual bool GetPossibleInflections(
            DictionaryEntry stemEntry,
            string stemText,
            string remainingText,
            bool isExactLengthOnly,
            out List<DictionaryEntry> inflectionEntries)
        {
            int stemLength = stemText.Length;
            int fullLength = stemLength + remainingText.Length;
            DictionaryEntry newDictionaryEntry = null;
            int senseIndex = 0;
            List<LexItem> lexItems = null;
            LanguageID stemLanguageID = stemEntry.LanguageID;
            string inflectionText = null;
            bool returnValue = false;

            inflectionEntries = null;

            foreach (Sense sense in stemEntry.Senses)
            {
                if ((sense.Category != LexicalCategory.Stem) && (sense.Category != LexicalCategory.IrregularStem))
                    continue;

                if (lexItems == null)
                {
                    if (isExactLengthOnly)
                    {
                        LexItem item = EndingsTable.ParseExact(remainingText);

                        if (item == null)
                            return false;

                        lexItems = new List<LexItem>() { item };
                    }
                    else
                    {
                        if (!EndingsTable.Parse(remainingText, out lexItems))
                            return false;
                    }
                }

                if (!String.IsNullOrEmpty(sense.CategoryString))
                {
                    string cat = sense.CategoryString.Trim();

                    foreach (LexItem lexItem in lexItems)
                    {
                        if (isExactLengthOnly)
                        {
                            if (stemLength + lexItem.Text.Text(stemLanguageID).Length != fullLength)
                                continue;
                        }

                        if (ProcessCategoryInflections(
                                stemLanguageID,
                                stemText,
                                sense,
                                cat,
                                lexItem,
                                null,
                                ref newDictionaryEntry,
                                out inflectionText))
                        {
                            if (inflectionEntries == null)
                                inflectionEntries = new List<DictionaryEntry>();

                            DictionaryEntry existingEntry = inflectionEntries.FirstOrDefault(
                                x => (x.KeyString == newDictionaryEntry.KeyString) && (x.LanguageID == newDictionaryEntry.LanguageID));

                            if (existingEntry != null)
                            {
                                if (newDictionaryEntry != existingEntry)
                                    existingEntry.MergeEntry(newDictionaryEntry);
                            }
                            else
                                inflectionEntries.Add(newDictionaryEntry);

                            returnValue = true;
                        }
                    }
                }

                senseIndex++;
            }

            return returnValue;
        }

        protected virtual bool ProcessCategoryInflections(
            LanguageID stemLanguageID,
            string stemText,
            Sense sense,
            string cat,
            LexItem lexItem,
            List<DictionaryEntry> formAndStemDictionaryEntries,
            ref DictionaryEntry newDictionaryEntry,
            out string inflectionText)
        {
            List<Designator> designations = lexItem.FindCategoryDesignations(cat);
            List<Inflection> inflections = null;
            DictionaryEntry dictionaryFormEntry;
            int dictionaryFormSenseIndex;
            int dictionaryFormSynonymIndex = 0;

            inflectionText = null;

            if (designations == null)
                return false;

            if (lexItem.Text == null)
                return false;

            inflectionText = stemText.ToLower() + lexItem.Text.Text(stemLanguageID);

                // Verb
            if (cat.StartsWith("v"))
            {
                if ((dictionaryFormEntry = LookupDictionaryFormEntry(
                    sense,
                    out dictionaryFormSenseIndex)) != null)
                {
                    if (formAndStemDictionaryEntries != null)
                        formAndStemDictionaryEntries.Add(dictionaryFormEntry);

                    int senseCount = dictionaryFormEntry.SenseCount;

                    if ((MultiTool != null) && (MultiTool.HostLanguageCount() >= 1))
                    {
                        LanguageID hostLanguageID = MultiTool.HostLanguageIDs.First();

                        for (; dictionaryFormSenseIndex < senseCount; dictionaryFormSenseIndex++)
                        {
                            Sense dfSense = dictionaryFormEntry.GetSenseIndexed(dictionaryFormSenseIndex);
                            LanguageSynonyms dfLanguageSynonyms = dfSense.GetLanguageSynonyms(hostLanguageID);

                            if (dfLanguageSynonyms == null)
                                continue;

                            int synonymCount = dfLanguageSynonyms.SynonymCount;

                            for (dictionaryFormSynonymIndex = 0; dictionaryFormSynonymIndex < synonymCount; dictionaryFormSynonymIndex++)
                            {
                                List<Inflection> dfInflections = MultiTool.ConjugateVerbDictionaryFormSelected(
                                    dictionaryFormEntry,
                                    ref dictionaryFormSenseIndex,
                                    ref dictionaryFormSynonymIndex,
                                    designations);

                                if (dfInflections != null)
                                {
                                    if (inflections == null)
                                        inflections = dfInflections;
                                    else
                                        inflections.AddRange(dfInflections);
                                }
                            }
                            //break;
                        }
                    }
                    else
                    {
                        inflections = ConjugateVerbDictionaryFormSelected(
                                dictionaryFormEntry,
                                ref dictionaryFormSenseIndex,
                                ref dictionaryFormSynonymIndex,
                                designations);
                    }
                }
                else
                    inflections = null;
            }

            if (inflections == null)
                return false;

            foreach (Inflection inflection in inflections)
            {
                if (inflection.GetOutput(stemLanguageID) != inflectionText)
                    continue;

                if ((newDictionaryEntry != null) && (newDictionaryEntry.KeyString == inflectionText))
                {
                    DictionaryEntry additionalEntry = CreateInflectionDictionaryEntry(
                        inflectionText,
                        stemLanguageID,
                        inflection,
                        DefaultInflectionOutputMode);
                    newDictionaryEntry.MergeEntry(additionalEntry);
                }
                else
                    newDictionaryEntry = CreateInflectionDictionaryEntry(
                        inflectionText,
                        stemLanguageID,
                        inflection,
                        DefaultInflectionOutputMode);
            }

            if (newDictionaryEntry == null)
                return false;

            return true;
        }

        public virtual bool LookupSuffix(string text, out LexItem lexItem)
        {
            if (CanDeinflect)
            {
                lexItem = EndingsTable.ParseLongest(text);

                if (lexItem != null)
                    return true;
            }
            else
                lexItem = null;

            return false;
        }

        public DictionaryEntry CreateNumberDictionaryEntry(LanguageID languageID, string text)
        {
            if (languageID == null)
                languageID = TargetLanguageIDs.First();

            DictionaryEntry dictionaryEntry = new DictionaryEntry(text, languageID);

            if (TargetLanguageIDs != null)
            {
                foreach (LanguageID otherLanguageID in TargetLanguageIDs)
                {
                    if (otherLanguageID == languageID)
                        continue;
                    else if (!LanguageLookup.IsAlternateOfLanguageID(otherLanguageID, languageID))
                        continue;

                    LanguageString alternate = new LanguageString(
                        0,
                        otherLanguageID,
                        TranslateNumber(otherLanguageID, text));
                    dictionaryEntry.AddAlternate(alternate);
                }

                if (HostLanguageIDs != null)
                {
                    foreach (LanguageID otherLanguageID in HostLanguageIDs)
                    {
                        ProbableMeaning probableSynonym = new ProbableMeaning(
                            TranslateNumber(otherLanguageID, text),
                            LexicalCategory.Number,
                            null,
                            float.NaN,
                            0,
                            NumberDictionarySourceID);
                        List<ProbableMeaning> probableSynonyms = new List<ProbableMeaning>() { probableSynonym };
                        LanguageSynonyms languageSynonyms = new LanguageSynonyms(otherLanguageID, probableSynonyms);
                        List<LanguageSynonyms> languageSynonymsList = new List<LanguageSynonyms>() { languageSynonyms };
                        Sense sense = new Sense(0, LexicalCategory.Number, null, 0, languageSynonymsList, null);
                        dictionaryEntry.AddSense(sense);
                    }
                }
            }
            else if (UserLanguageIDs != null)
            {
                foreach (LanguageID otherLanguageID in UserLanguageIDs)
                {
                    ProbableMeaning probableSynonym = new ProbableMeaning(
                        TranslateNumber(otherLanguageID, text),
                        LexicalCategory.Number,
                        null,
                        float.NaN,
                        0,
                        NumberDictionarySourceID);
                    List<ProbableMeaning> probableSynonyms = new List<ProbableMeaning>() { probableSynonym };
                    LanguageSynonyms languageSynonyms = new LanguageSynonyms(otherLanguageID, probableSynonyms);
                    List<LanguageSynonyms> languageSynonymsList = new List<LanguageSynonyms>() { languageSynonyms };
                    Sense sense = new Sense(0, LexicalCategory.Number, null, 0, languageSynonymsList, null);
                    dictionaryEntry.AddSense(sense);
                }
            }

            return dictionaryEntry;
        }

        protected string ProcessToken(string pattern, string token, string replacement, string defaultOutput)
        {
            string returnValue;
            if (pattern.Contains(token))
            {
                if (String.IsNullOrEmpty(replacement))
                    returnValue = pattern.Replace(token, String.Empty);
                else
                    returnValue = pattern.Replace(token, " " + replacement + " ");
                returnValue = returnValue.Replace("  ", " ").Trim();
            }
            else
                returnValue = defaultOutput;
            return returnValue;
        }

        public virtual bool TableInflect(
            DictionaryEntry dictionaryEntry,
            ref int senseIndex,
            ref int synonymIndex,
            InflectorTable inflectorTable,
            Designator designator,
            out Inflection inflection)
        {
            bool returnValue = false;

            inflection = null;

            if (dictionaryEntry == null)
                return false;

            if (designator == null)
                return false;

            if (inflectorTable == null)
                return false;

            if (senseIndex == -1)
            {
                if (!SelectDefaultSense(
                        dictionaryEntry,
                        inflectorTable.Category,
                        ref senseIndex,
                        ref synonymIndex))
                    return false;
            }

#if false
            if (designator.Label == "Indicative Past Positive Singular First")
            {
                ApplicationData.Global.PutConsoleMessage(designator.Label);
            }
#endif

            List<Inflector> inflectors = null;
            Inflector inflector = inflectorTable.GetInflector(designator.Label);

            if (inflector == null)
            {
                inflectors = inflectorTable.GetBestInflectors(designator);

                if (inflectors != null)
                {
                    FilterInflectors(inflectors);

                    if (inflectors.Count() == 0)
                        return false;
                    else if (inflectors.Count() == 1)
                        inflector = inflectors[0];
                }
            }

            if (inflector != null)
            {
                returnValue = InflectTargetOrHostFiltered(
                    dictionaryEntry,
                    senseIndex,
                    synonymIndex,
                    inflector,
                    designator,
                    inflectorTable,
                    null,
                    out inflection);
            }
            else if (inflectors != null)
            {
                List<Inflection> inflections = new List<Inflection>();

                returnValue = true;

                foreach (Inflector anInflector in inflectors)
                {
                    Inflection anInflection;
                    bool result = InflectTargetOrHostFiltered(
                        dictionaryEntry,
                        senseIndex,
                        synonymIndex,
                        anInflector,
                        designator,
                        inflectorTable,
                        null,
                        out anInflection);

                    if (result && (anInflection != null))
                        inflections.Add(anInflection);
                    else
                        returnValue = false;
                }

                if (inflections.Count() != 0)
                    inflection = new Inflection(inflections);
            }

            if (inflection != null)
                inflection.Designation = new Designator(designator);

            //if ((inflection != null) && (inflection.Label != designator.Label))
            //    ApplicationData.Global.PutConsoleMessage("Label mismatch: designator: " + designator.Label + " inflection: " + inflection.Label);

            return returnValue;
        }

        public virtual void FilterInflectors(List<Inflector> inflectors)
        {
            if (AllowArchaicInflections)
                return;

            for (int i = inflectors.Count() - 1; i >= 0; i--)
            {
                Inflector inflector = inflectors[i];

                if (inflector.HasClassificationWith("Period", "Archaic"))
                    inflectors.RemoveAt(i);
            }
        }

        public virtual bool SelectDefaultSense(
            DictionaryEntry dictionaryEntry,
            LexicalCategory category,
            ref int senseIndex,
            ref int synonymIndex)
        {
            int senseCount = dictionaryEntry.SenseCount;
            int index;

            for (index = 0; index < senseCount; index++)
            {
                Sense sense = dictionaryEntry.GetSenseIndexed(index);

                if (sense.Category == category)
                {
                    senseIndex = index;

                    if (synonymIndex == -1)
                        synonymIndex = 0;

                    return true;
                }
            }

            return false;
        }

        public virtual bool InflectTargetOrHostFiltered(
            DictionaryEntry dictionaryEntry,
            int senseIndex,
            int synonymIndex,
            Inflector inflector,
            Designator designator,
            InflectorTable inflectorTable,
            string instance,
            out Inflection inflection)
        {
            bool result = false;
            bool wasFiltered = false;

            inflection = null;

            if (inflector.HasTrigger())
            {
                InflectorTrigger inflectorTrigger = inflectorTable.GetInflectorTrigger(inflector.TriggerLabel);

                if (inflectorTrigger != null)
                {
                    if (!inflectorTrigger.MatchTrigger(dictionaryEntry))
                        return false;
                }
            }

            if (inflector.FilterName != null)
                result = InflectFiltered(
                    dictionaryEntry,
                    senseIndex,
                    synonymIndex,
                    inflector,
                    designator,
                    inflectorTable,
                    instance,
                    out inflection,
                    out wasFiltered);

            if (!wasFiltered)
                result = InflectTargetOrHost(
                    dictionaryEntry,
                    senseIndex,
                    synonymIndex,
                    inflector,
                    designator,
                    inflectorTable,
                    instance,
                    out inflection);

            return result;
        }

        public virtual bool InflectFiltered(
            DictionaryEntry dictionaryEntry,
            int senseIndex,
            int synonymIndex,
            Inflector inflector,
            Designator designator,
            InflectorTable inflectorTable,
            string instance,
            out Inflection inflection,
            out bool wasFiltered)
        {
            InflectorFilter inflectorFilter = inflectorTable.GetInflectorFilter(inflector.FilterName);
            bool returnValue = true;

            inflection = null;
            wasFiltered = false;

            if (inflectorFilter == null)
                throw new Exception("Inflector filter not found: " + inflector.FilterName);

            InflectorFilterItem inflectorFilterItem = inflectorFilter.GetInflectorFilterItem(dictionaryEntry.KeyString);

            if (inflectorFilterItem != null)
            {
                if (inflectorFilterItem.ItemType == "Disallow")
                {
                    wasFiltered = true;
                    returnValue = false;
                }
                else if (inflectorFilterItem.ItemType == "Redirection")
                {
                    int filterLanguageIndex = inflectorFilterItem.Input.GetStringIndex(dictionaryEntry.KeyString);

                    if (filterLanguageIndex < 0)
                        filterLanguageIndex = 0;

                    string filterOutputString = inflectorFilterItem.Output.GetIndexedString(filterLanguageIndex);

                    if (String.IsNullOrEmpty(filterOutputString))
                        throw new Exception("Inflector filter output string not found: " + inflector.FilterName);

                    int filteredSenseIndex = -1;
                    int filteredSynonymIndex = -1;
                    DictionaryEntry filteredEntry;

                    if (!String.IsNullOrEmpty(inflectorFilterItem.NewCategoryString))
                    {
                        filteredEntry = new DictionaryEntry(dictionaryEntry);
                        MultiLanguageString filterInputMLS = inflectorFilterItem.Input.GetMultiLanguageString(
                            "Input",
                            TargetLanguageIDs);
                        MultiLanguageString filterOutputMLS = inflectorFilterItem.Output.GetMultiLanguageString(
                            "Output",
                            TargetLanguageIDs);
                        filteredEntry.Retarget(filterInputMLS, filterOutputMLS);
                        filteredEntry.RetargetCategories(
                            inflectorTable.Category,
                            inflectorFilterItem.NewCategoryString);
                    }
                    else
                    {
                        filteredEntry = GetDictionaryEntry(filterOutputString);

                        if (filteredEntry == null)
                            throw new Exception("Inflector filter dictionary entry not found: " + filterOutputString);
                    }

                    if (!SelectDefaultSense(
                            filteredEntry,
                            inflectorTable.Category,
                            ref filteredSenseIndex,
                            ref filteredSynonymIndex))
                    {
                        //throw new Exception("SelectDefaultSense failed on inflector filter dictionary entry: " + filterOutputString);
                        ApplicationData.Global.PutConsoleMessage("Oops");
                        filteredSenseIndex = 0;
                        filteredSynonymIndex = 0;
                    }

                    if (inflectorFilter.ClassificationCount() != 0)
                    {
                        List<Inflector> filteredInflectors = null;
                        Inflector filteredInflector = null;
                        Designator filteredDesignator = new Designator(inflector);

                        foreach (Classifier classifier in inflectorFilter.Classifications)
                            filteredDesignator.DeleteClassification(classifier.Name, classifier.Text);

                        filteredDesignator.DefaultLabel();
                        filteredInflectors = inflectorTable.GetBestInflectors(filteredDesignator);

                        if (filteredInflectors != null)
                        {
                            if (filteredInflectors.Count() == 0)
                                return false;
                            else if (filteredInflectors.Count() == 1)
                                filteredInflector = filteredInflectors[0];
                        }

                        if (filteredInflector != null)
                        {
                            returnValue = InflectTargetOrHost(
                                filteredEntry,
                                filteredSenseIndex,
                                filteredSynonymIndex,
                                filteredInflector,
                                filteredDesignator,
                                inflectorTable,
                                instance,
                                out inflection);
                        }
                        else if (filteredInflectors != null)
                        {
                            List<Inflection> inflections = new List<Inflection>();

                            returnValue = true;

                            foreach (Inflector anInflector in filteredInflectors)
                            {
                                Inflection anInflection;
                                bool result = InflectTargetOrHost(
                                    filteredEntry,
                                    filteredSenseIndex,
                                    filteredSynonymIndex,
                                    anInflector,
                                    filteredDesignator,
                                    inflectorTable,
                                    instance,
                                    out anInflection);

                                if (result && (anInflection != null))
                                    inflections.Add(anInflection);
                                else
                                    returnValue = false;
                            }

                            if (inflections.Count() != 0)
                                inflection = new Inflection(inflections);
                        }

                        if (inflection != null)
                            inflection.Designation = new Designator(designator);
                    }
                    else
                    {
                        returnValue = InflectTargetOrHost(
                            filteredEntry,
                            filteredSenseIndex,
                            filteredSynonymIndex,
                            inflector,
                            designator,
                            inflectorTable,
                            instance,
                            out inflection);
                    }

                    wasFiltered = true;
                }
                else
                    throw new Exception("Unsupported inflector filter item type: " + inflectorFilterItem.ItemType);
            }

            return returnValue;
        }

        public virtual bool InflectTargetOrHost(
            DictionaryEntry dictionaryEntry,
            int senseIndex,
            int synonymIndex,
            Inflector inflector,
            Designator designator,
            InflectorTable inflectorTable,
            string instance,
            out Inflection inflection)
        {
            bool result;

#if false
            if (inflector.Label == "Indicative Past Positive Singular First")
            {
                ApplicationData.Global.PutConsoleMessage(designator.Label);
            }
#endif

            if (IsTargetEntry(dictionaryEntry))
                result = InflectTarget(
                    dictionaryEntry,
                    senseIndex,
                    synonymIndex,
                    inflector,
                    designator,
                    inflectorTable,
                    instance,
                    out inflection);
            else
                result = InflectHost(
                    dictionaryEntry,
                    senseIndex,
                    synonymIndex,
                    inflector,
                    designator,
                    inflectorTable,
                    instance,
                    out inflection);

            return result;
        }

        public virtual bool InflectTarget(
            DictionaryEntry dictionaryEntry,
            int senseIndex,
            int synonymIndex,
            Inflector inflector,
            Designator designator,
            InflectorTable inflectorTable,
            string instance,
            out Inflection inflection)
        {
            inflection = null;

            if (dictionaryEntry == null)
                return false;

            if (senseIndex == -1)
                return false;

            if (synonymIndex == -1)
                return false;

            if (designator == null)
                return false;

            if (inflector == null)
                return false;

            if (inflector.Modifiers == null)
                return false;

            Sense sense = null;
            bool returnValue = false;

            sense = dictionaryEntry.GetSenseIndexed(senseIndex);

            if (sense == null)
                return false;

            if (inflectorTable == null)
                return false;

            if ((DesignatorBreakpoint != "None") && designator.Label.StartsWith(DesignatorBreakpoint))
                ApplicationData.Global.PutConsoleMessage(designator.Label);

            MultiLanguageString dictionaryForm;
            MultiLanguageString stem;
            MultiLanguageString compoundFixupPatternMLS;
            LexicalCategory category;
            string categoryString;
            string className;
            string subClassName;

            if (!GetDictionaryFormAndCategories(
                    dictionaryEntry,
                    senseIndex,
                    synonymIndex,
                    true,
                    inflectorTable,
                    out dictionaryForm,
                    out stem,
                    out compoundFixupPatternMLS,
                    out category,
                    out categoryString,
                    out className,
                    out subClassName))
                return false;

            MultiLanguageString originalStem = null;
            MultiLanguageString originalDictionaryForm = null;

            if (stem == null)
                return false;

            //if (inflector.Label.StartsWith("Progressive"))
            //    ApplicationData.Global.PutConsoleMessage(inflector.Label);

            if (inflector.PreInflector != null)
            {
                originalStem = stem;
                originalDictionaryForm = dictionaryForm;

                if (!ApplyPreInflector(
                        inflector.PreInflector,
                        true,
                        ref dictionaryForm,
                        ref stem,
                        ref category,
                        ref categoryString,
                        ref className,
                        ref subClassName,
                        inflectorTable))
                    return false;
            }

            Modifier bestModifier = GetTargetInflectorModifier(
                inflector,
                category,
                categoryString,
                className,
                subClassName,
                dictionaryForm);

            if (bestModifier != null)
            {
                inflection = new Inflection(
                    dictionaryEntry,
                    designator,
                    TargetLanguageIDs);

                inflection.Category = bestModifier.Category;
                inflection.CategoryString = categoryString;

                Inflector stemInflector = null;
                Modifier stemModifier = null;

                if (!String.IsNullOrEmpty(bestModifier.Stem))
                {
                    stemInflector = inflectorTable.GetStem(bestModifier.Stem);

                    if (stemInflector != null)
                        stemModifier = GetStemInflectorModifier(
                            stemInflector,
                            className,
                            subClassName);
                }

                returnValue = InflectorModify(
                    dictionaryForm,
                    stem,
                    TargetLanguageIDs,
                    inflector,
                    bestModifier,
                    stemInflector,
                    stemModifier,
                    inflectorTable,
                    instance,
                    inflection);

                if (inflector.PostInflector != null)
                {
                    if (!ApplyPostInflector(
                            inflector.PostInflector,
                            true,
                            category,
                            categoryString,
                            className,
                            subClassName,
                            dictionaryForm,
                            inflection,
                            inflectorTable))
                        return false;
                }

                if (originalDictionaryForm != null)
                {
                    // Restore pre-inflected items.
                    inflection.DictionaryForm = originalDictionaryForm;
                    inflection.Root = originalStem;
                }
            }

            return returnValue;
        }

        public virtual bool InflectHost(
            DictionaryEntry dictionaryEntry,
            int senseIndex,
            int synonymIndex,
            Inflector inflector,
            Designator designator,
            InflectorTable inflectorTable,
            string instance,
            out Inflection inflection)
        {
            inflection = null;

#if false
            if (inflector.Label == "Indicative Past Positive Singular First")
            {
                ApplicationData.Global.PutConsoleMessage(designator.Label);
            }
#endif

            if (dictionaryEntry == null)
                return false;

            if (senseIndex == -1)
                return false;

            if (synonymIndex == -1)
                return false;

            if (designator == null)
                return false;

            if (inflector == null)
                return false;

            if (inflector.Modifiers == null)
                return false;

            Sense sense = null;
            bool returnValue = false;

            sense = dictionaryEntry.GetSenseIndexed(senseIndex);

            if (sense == null)
                return false;

            if (inflectorTable == null)
                return false;

            if ((DesignatorBreakpoint != "None") && designator.Label.StartsWith(DesignatorBreakpoint))
                ApplicationData.Global.PutConsoleMessage(designator.Label);

            MultiLanguageString dictionaryForm;
            MultiLanguageString stem;
            MultiLanguageString compoundFixupPatternMLS;
            LexicalCategory category;
            string categoryString;
            string className;
            string subClassName;

            if (!GetDictionaryFormAndCategories(
                    dictionaryEntry,
                    senseIndex,
                    synonymIndex,
                    false,
                    inflectorTable,
                    out dictionaryForm,
                    out stem,
                    out compoundFixupPatternMLS,
                    out category,
                    out categoryString,
                    out className,
                    out subClassName))
                return false;

            MultiLanguageString originalStem = null;
            MultiLanguageString originalDictionaryForm = null;

            if (stem == null)
                return false;

            if (inflector.PreInflector != null)
            {
                originalStem = stem;
                originalDictionaryForm = dictionaryForm;

                if (!ApplyPreInflector(
                        inflector.PreInflector,
                        false,
                        ref dictionaryForm,
                        ref stem,
                        ref category,
                        ref categoryString,
                        ref className,
                        ref subClassName,
                        inflectorTable))
                    return false;
            }

            Modifier bestModifier = GetHostInflectorModifier(
                inflector,
                category,
                className,
                subClassName,
                dictionaryForm);

            if (bestModifier != null)
            {
                inflection = new Inflection(
                    dictionaryEntry,
                    inflector.Designator,
                    TargetLanguageIDs);

                inflection.Category = bestModifier.Category;
                inflection.CategoryString = categoryString;

                Inflector stemInflector = null;
                Modifier stemModifier = null;

                if (!String.IsNullOrEmpty(bestModifier.Stem))
                {
                    stemInflector = inflectorTable.GetStem(bestModifier.Stem);

                    if (stemInflector != null)
                        stemModifier = GetStemInflectorModifier(
                            stemInflector,
                            className,
                            subClassName);
                }

                returnValue = InflectorModify(
                    dictionaryForm,
                    stem,
                    TargetLanguageIDs,
                    inflector,
                    bestModifier,
                    stemInflector,
                    stemModifier,
                    inflectorTable,
                    instance,
                    inflection);

                if (inflector.PostInflector != null)
                {
                    if (!ApplyPostInflector(
                            inflector.PostInflector,
                            false,
                            category,
                            categoryString,
                            className,
                            subClassName,
                            dictionaryForm,
                            inflection,
                            inflectorTable))
                        return false;
                }

                if (originalDictionaryForm != null)
                {
                    // Restore pre-inflected items.
                    inflection.DictionaryForm = originalDictionaryForm;
                    inflection.Root = originalStem;
                }

                if (compoundFixupPatternMLS != null)
                    inflection.ApplyCompoundFixup(compoundFixupPatternMLS);
            }

            return returnValue;
        }

        public virtual bool InflectStem(
            MultiLanguageString dictionaryForm,
            MultiLanguageString stem,
            List<LanguageID> languageIDs,
            Inflector inflector,
            string classCode,
            string subClassCode,
            out MultiLanguageString newStem)
        {
            Modifier modifier = null;
            int languageIndex = 0;
            int languageCount = languageIDs.Count;
            string caseLabel = inflector.Designator.Label;
            bool returnValue = true;

            newStem = new MultiLanguageString(stem);

            foreach (Modifier aModifier in inflector.Modifiers)
            {
                bool match = true;

                if (aModifier.HasAnyClass())
                {
                    if (!aModifier.HasClass(classCode))
                        match = false;
                    else if (aModifier.HasAnySubClass())
                    {
                        if (!aModifier.HasSubClass(subClassCode))
                            match = false;
                    }
                }
                else if (aModifier.HasAnySubClass())
                {
                    if (subClassCode != null)
                    {
                        if (!aModifier.HasSubClass(subClassCode))
                            match = false;
                    }
                }

                if (match)
                {
                    modifier = aModifier;
                    break;
                }
            }

            if (modifier == null)
                return false;

            foreach (LanguageID languageID in languageIDs)
            {
                string space = (LanguageLookup.IsUseSpacesToSeparateWords(languageID) ? " " : "");
                string dictionaryFormText = dictionaryForm.Text(languageID);
                string stemText = stem.Text(languageID);
                string prePronoun = String.Empty;
                string prePronounSpaced = String.Empty;
                string preWords = String.Empty;
                string preWordsSpaced = String.Empty;
                string prefix = String.Empty;
                string suffix = String.Empty;
                string postWords = String.Empty;
                string postWordsSpaced = String.Empty;
                string pronouns = String.Empty;
                string pronounsSpaced = String.Empty;
                string postPronouns = String.Empty;
                string postPronounsSpaced = String.Empty;
                string inflected = String.Empty;
                string outputText = String.Empty;
                string pronounOutputText = String.Empty;

                if (modifier.PrePronoun != null)
                    prePronoun = modifier.PrePronoun.GetIndexedString(languageIndex);

                if (modifier.PreWords != null)
                    preWords = modifier.PreWords.GetIndexedString(languageIndex);

                if (modifier.PostWords != null)
                    postWords = modifier.PostWords.GetIndexedString(languageIndex);

                bool isRegular = true;

                if (!String.IsNullOrEmpty(modifier.Function))
                    returnValue = ApplyFunction(
                        modifier.Function,
                        inflector,
                        modifier,
                        languageID,
                        languageIndex,
                        caseLabel,
                        isRegular,
                        ref dictionaryFormText,
                        ref prePronoun,
                        ref pronouns,
                        ref preWords,
                        ref prefix,
                        ref stemText,
                        ref suffix,
                        ref postWords,
                        ref postPronouns);

                if (modifier.Actions != null)
                    ApplyActions(
                        modifier.Actions,
                        languageIndex,
                        prefix + stemText + suffix,
                        caseLabel,
                        ref dictionaryFormText,
                        ref prePronoun,
                        ref pronouns,
                        ref preWords,
                        ref prefix,
                        ref stemText,
                        ref suffix,
                        ref postWords,
                        ref postPronouns,
                        ref isRegular);

                inflected = prefix + stemText + suffix;

                if (!String.IsNullOrEmpty(preWords))
                    preWordsSpaced = preWords + space;

                if (!String.IsNullOrEmpty(postWords))
                    postWordsSpaced = space + postWords;

                outputText = preWordsSpaced + inflected + postWordsSpaced;

                newStem.SetText(languageID, outputText);

                languageIndex++;
            }

            return returnValue;
        }

        public bool ApplyPreInflector(
            Inflector preInflector,
            bool isTarget,
            ref MultiLanguageString dictionaryForm,
            ref MultiLanguageString stem,
            ref LexicalCategory category,
            ref string categoryString,
            ref string className,
            ref string subClassName,
            InflectorTable inflectorTable)
        {
            Modifier bestModifier;
            Inflection inflection;
            bool returnValue = true;

            if (isTarget)
                bestModifier = GetTargetInflectorModifier(
                    preInflector,
                    category,
                    categoryString,
                    className,
                    subClassName,
                    dictionaryForm);
            else
                bestModifier = GetHostInflectorModifier(
                    preInflector,
                    category,
                    className,
                    subClassName,
                    dictionaryForm);

            if (bestModifier != null)
            {
                inflection = new Inflection(
                    null,
                    null,
                    TargetLanguageIDs);

                inflection.Category = bestModifier.Category;
                inflection.CategoryString = categoryString;

                Inflector stemInflector = null;
                Modifier stemModifier = null;

                if (!String.IsNullOrEmpty(bestModifier.Stem))
                {
                    stemInflector = inflectorTable.GetStem(bestModifier.Stem);

                    if (stemInflector != null)
                        stemModifier = GetStemInflectorModifier(
                            stemInflector,
                            className,
                            subClassName);
                }

                returnValue = InflectorModify(
                    dictionaryForm,
                    stem,
                    TargetLanguageIDs,
                    preInflector,
                    bestModifier,
                    stemInflector,
                    stemModifier,
                    inflectorTable,
                    null,
                    inflection);

                if (returnValue)
                {
                    dictionaryForm = inflection.Output;

                    stem = GetStem(
                        dictionaryForm,
                        TargetLanguageIDs,
                        out categoryString);

                    if (bestModifier.NewCategory != LexicalCategory.Unknown)
                        category = bestModifier.NewCategory;

                    if (!String.IsNullOrEmpty(bestModifier.NewCategoryString))
                        categoryString = bestModifier.NewCategoryString;

                    if (!String.IsNullOrEmpty(bestModifier.NewClass))
                        className = bestModifier.NewClass;

                    if (!String.IsNullOrEmpty(bestModifier.NewSubClass))
                        subClassName = bestModifier.NewSubClass;
                }
            }

            return returnValue;
        }

        public bool ApplyPostInflector(
            Inflector postInflector,
            bool isTarget,
            LexicalCategory category,
            string categoryString,
            string className,
            string subClassName,
            MultiLanguageString dictionaryForm,
            Inflection inflection,
            InflectorTable inflectorTable)
        {
            if (postInflector == null)
                return false;

            if (postInflector.ModifierCount() == 0)
                return false;

            Modifier bestModifier;

            if (isTarget)
                bestModifier = GetTargetInflectorModifier(
                    postInflector,
                    category,
                    categoryString,
                    className,
                    subClassName,
                    dictionaryForm);
            else
                bestModifier = GetHostInflectorModifier(
                    postInflector,
                    category,
                    className,
                    subClassName,
                    dictionaryForm);

            if (bestModifier == null)
                return false;

            if (!InflectionModify(
                    inflection,
                    TargetLanguageIDs,
                    postInflector,
                    bestModifier))
                return false;

            return true;
        }

        public virtual Modifier GetTargetInflectorModifier(
            Inflector inflector,
            LexicalCategory category,
            string categoryString,
            string className,
            string subClassName,
            MultiLanguageString dictionaryForm)
        {
            Modifier bestModifier = null;

            if (inflector.Modifiers != null)
            {
                foreach (Modifier modifier in inflector.Modifiers)
                {
                    bool match = true;

                    if (modifier.Category != category)
                        match = false;
                    else if (modifier.HasAnyClass())
                    {
                        if (className != null)
                        {
                            if (!modifier.HasClass(className))
                                match = false;
                            else if (modifier.HasAnySubClass())
                            {
                                if (!modifier.HasSubClass(subClassName))
                                    match = false;
                            }
                        }
                        else
                        {
                            if (!CategoryStringHasCategoryClass(categoryString, modifier.Class))
                                match = false;
                            else if (modifier.HasAnySubClass())
                            {
                                if (!CategoryStringHasCategorySubClass(categoryString, modifier.SubClass))
                                    match = false;
                            }
                        }
                    }
                    else if (modifier.HasAnySubClass())
                    {
                        if (subClassName != null)
                        {
                            if (!modifier.HasSubClass(subClassName))
                                match = false;
                        }
                        else
                        {
                            if (!CategoryStringHasCategorySubClass(categoryString, modifier.SubClass))
                                match = false;
                        }
                    }

                    if (match)
                    {
                        if (modifier.HasAnyTriggerFunction())
                            match = ModifierTriggerFunction(modifier.TriggerFunction, dictionaryForm);
                    }

                    if (match)
                    {
                        bestModifier = modifier;
                        break;
                    }
                }
            }

            return bestModifier;
        }

        public virtual Modifier GetHostInflectorModifier(
            Inflector inflector,
            LexicalCategory category,
            string classCode,
            string subClassCode,
            MultiLanguageString dictionaryForm)
        {
            Modifier bestModifier = null;

            foreach (Modifier modifier in inflector.Modifiers)
            {
                bool match = true;

                if (modifier.Category != category)
                    match = false;
                else if (modifier.HasAnyClass())
                {
                    if (!modifier.HasClass(classCode))
                        match = false;
                    else if (modifier.HasAnySubClass())
                    {
                        if (!modifier.HasSubClass(subClassCode))
                            match = false;
                    }
                }

                if (match)
                {
                    if (modifier.HasAnyTriggerFunction())
                        match = ModifierTriggerFunction(modifier.TriggerFunction, dictionaryForm);
                }

                if (match)
                {
                    bestModifier = modifier;
                    break;
                }
            }

            return bestModifier;
        }

        public virtual Modifier GetStemInflectorModifier(
            Inflector inflector,
            string classCode,
            string subClassCode)
        {
            Modifier modifier = null;
            string caseLabel = inflector.Designator.Label;

            foreach (Modifier aModifier in inflector.Modifiers)
            {
                bool match = true;

                if (aModifier.HasAnyClass())
                {
                    if (!aModifier.HasClass(classCode))
                        match = false;
                    else if (aModifier.HasAnySubClass())
                    {
                        if (!aModifier.HasSubClass(subClassCode))
                            match = false;
                    }
                }
                else if (aModifier.HasAnySubClass())
                {
                    if (subClassCode != null)
                    {
                        if (!aModifier.HasSubClass(subClassCode))
                            match = false;
                    }
                }

                if (match)
                {
                    modifier = aModifier;
                    break;
                }
            }

            return modifier;
        }

        public bool IsTargetEntry(DictionaryEntry dictionaryEntry)
        {
            if (TargetLanguageIDs.Contains(dictionaryEntry.LanguageID))
                return true;

            return false;
        }

        public virtual bool CategoryStringHasCategoryClass(
            string categoryString,
            string classCode)
        {
            if (!String.IsNullOrEmpty(categoryString))
            {
                string[] parts = categoryString.Split(LanguageLookup.Comma, StringSplitOptions.RemoveEmptyEntries);

                foreach (string part in parts)
                {
                    if (part == classCode)
                        return true;
                }
            }

            return false;
        }

        public virtual bool CategoryStringHasCategoryClass(
            string categoryString,
            List<string> classCodes)
        {
            if (!String.IsNullOrEmpty(categoryString) && (classCodes != null) && (classCodes.Count() != 0))
            {
                string[] parts = categoryString.Split(LanguageLookup.Comma, StringSplitOptions.RemoveEmptyEntries);

                foreach (string part in parts)
                {
                    if (classCodes.Contains(part))
                        return true;
                }
            }

            return false;
        }

        public virtual bool CategoryStringHasCategorySubClass(
            string categoryString,
            string subClassCode)
        {
            if (!String.IsNullOrEmpty(categoryString))
            {
                string[] parts = categoryString.Split(LanguageLookup.Comma, StringSplitOptions.RemoveEmptyEntries);

                foreach (string part in parts)
                {
                    if (part == subClassCode)
                        return true;
                }
            }

            return false;
        }

        public virtual bool CategoryStringHasCategorySubClass(
            string categoryString,
            List<string> subClassCodes)
        {
            if (!String.IsNullOrEmpty(categoryString) && (subClassCodes != null) && (subClassCodes.Count() != 0))
            {
                string[] parts = categoryString.Split(LanguageLookup.Comma, StringSplitOptions.RemoveEmptyEntries);

                foreach (string part in parts)
                {
                    if (subClassCodes.Contains(part))
                        return true;
                }
            }

            return false;
        }

        public virtual bool GetWordClassCategoryStringAndCodes(
            string word,
            LexicalCategory category,
            out string categoryString,
            out string classCode,
            out string subClassCode)
        {
            categoryString = String.Empty;
            classCode = String.Empty;
            subClassCode = String.Empty;
            return false;
        }

        public virtual LexicalCategory GetCategoryFromCategoryString(string categoryString)
        {
            return LexicalCategory.Unknown;
        }

        public virtual string GetCategoryClassFromEntry(DictionaryEntry dictionaryEntry)
        {
            return String.Empty;
        }

        public virtual string GetCategorySubClassFromEntry(DictionaryEntry dictionaryEntry)
        {
            return String.Empty;
        }

        public virtual string GetCategoryClassFromWord(string word)
        {
            return String.Empty;
        }

        public virtual string GetCategorySubClassFromWord(string word)
        {
            return String.Empty;
        }

        public virtual string MergeCategoryStrings(
            string cs1,
            string cs2)
        {
            if (String.IsNullOrEmpty(cs1))
                return cs2;
            else if (String.IsNullOrEmpty(cs2))
                return cs1;

            List<string> parts1 = cs1.Split(LanguageLookup.Comma, StringSplitOptions.RemoveEmptyEntries).ToList();
            List<string> parts2 = cs2.Split(LanguageLookup.Comma, StringSplitOptions.RemoveEmptyEntries).ToList();
            List<string> newParts = ObjectUtilities.ListConcatenateUnique<string>(parts1, parts2);

            return ObjectUtilities.GetStringFromStringList(newParts);
        }

        public virtual bool InflectorModify(
            MultiLanguageString dictionaryForm,
            MultiLanguageString stem,
            List<LanguageID> languageIDs,
            Inflector inflector,
            Modifier modifier,
            Inflector stemInflector,
            Modifier stemModifier,
            InflectorTable inflectorTable,
            string instance,
            Inflection inflection)
        {
            List<SemiRegular> semiRegulars = inflectorTable.SemiRegulars;
            Dictionary<string, SemiRegular> irregularDictionary = inflectorTable.IrregularDictionary;
            int languageIndex = 0;
            int languageCount = languageIDs.Count;
            string caseLabel = inflector.Designator.Label;
            List<MultiLanguageString> contractedOutputList = (inflector.HasAnyPostActions() ? new List<MultiLanguageString>() : null);
            bool returnValue = true;

            foreach (LanguageID languageID in languageIDs)
            {
                string space = (LanguageLookup.IsUseSpacesToSeparateWords(languageID) ? " " : "");
                string dictionaryFormText = dictionaryForm.Text(languageID);
                string stemText = stem.Text(languageID);
                string regularStemText = stemText;
                string prePronoun = String.Empty;
                string prePronounSpaced = String.Empty;
                string preWords = String.Empty;
                string preWordsSpaced = String.Empty;
                string prefix = String.Empty;
                string suffix = String.Empty;
                string regularPrefix = String.Empty;
                string regularSuffix = String.Empty;
                string postWords = String.Empty;
                string postWordsSpaced = String.Empty;
                string pronouns = String.Empty;
                string pronounsSpaced = String.Empty;
                string postPronouns = String.Empty;
                string postPronounsSpaced = String.Empty;
                string inflected = String.Empty;
                string mainInflected = String.Empty;
                string outputText = String.Empty;
                string pronounOutputText = String.Empty;
                string regularInflected = String.Empty;
                string regularOutputText = String.Empty;
                string regularPronounOutputText = String.Empty;

                if (stemModifier != null)
                    ApplyStemModifier(
                        stemInflector,
                        stemModifier,
                        languageID,
                        languageIndex,
                        caseLabel,
                        true,
                        ref dictionaryFormText,
                        ref prePronoun,
                        ref pronouns,
                        ref preWords,
                        ref prefix,
                        ref stemText,
                        ref suffix,
                        ref regularPrefix,
                        ref regularStemText,
                        ref regularSuffix,
                        ref postWords,
                        ref postPronouns);

                if (modifier.PrePronoun != null)
                    prePronoun += modifier.PrePronoun.GetIndexedString(languageIndex);

                if (modifier.PreWords != null)
                    preWords += modifier.PreWords.GetIndexedString(languageIndex);

                if (modifier.Prefix != null)
                {
                    prefix = modifier.Prefix.GetIndexedString(languageIndex) + prefix;
                    regularPrefix = modifier.Prefix.GetIndexedString(languageIndex) + regularPrefix;
                }

                if (modifier.Suffix != null)
                {
                    suffix += modifier.Suffix.GetIndexedString(languageIndex);
                    regularSuffix += modifier.Suffix.GetIndexedString(languageIndex);
                }

                if (modifier.PostWords != null)
                    postWords += modifier.PostWords.GetIndexedString(languageIndex);

                if (inflector.HasAnyPronouns())
                {
                    foreach (LiteralString pronoun in inflector.Pronouns)
                    {
                        string pronounString = pronoun.GetIndexedString(languageIndex);

                        if (!String.IsNullOrEmpty(pronouns))
                            pronouns += "/";

                        pronouns += pronounString;
                    }
                }

                if (inflector.HasAnyPostPronouns())
                {
                    foreach (LiteralString postPronoun in inflector.PostPronouns)
                    {
                        string postPronounString = postPronoun.GetIndexedString(languageIndex);

                        if (!String.IsNullOrEmpty(postPronouns))
                            postPronouns += "/";

                        postPronouns += postPronounString;
                    }
                }

                SemiRegular semiRegularPre = FindSemiRegularPre(
                    semiRegulars,
                    dictionaryForm,
                    stem,
                    instance);
                SemiRegular irregular = null;
                bool isRegular = true;

                if (semiRegularPre != null)
                {
                    //if ((stemText == "have") && (inflector.Label == "Indicative Present Negative Singular Third Masculine"))
                    //    ApplicationData.Global.PutConsoleMessage(inflector.Label);

                    ApplyActions(
                        semiRegularPre.Actions,
                        languageIndex,
                        prefix + stemText + suffix,
                        caseLabel,
                        ref dictionaryFormText,
                        ref prePronoun,
                        ref pronouns,
                        ref preWords,
                        ref prefix,
                        ref stemText,
                        ref suffix,
                        ref postWords,
                        ref postPronouns,
                        ref isRegular);
                    if (isRegular)
                    {
                        regularPrefix = prefix;
                        regularStemText = stemText;
                        regularSuffix = suffix;
                    }
                }

                if ((irregularDictionary != null) &&
                    irregularDictionary.TryGetValue(dictionaryFormText, out irregular))
                {
                    ApplyActions(
                        irregular.Actions,
                        languageIndex,
                        prefix + stemText + suffix,
                        caseLabel,
                        ref dictionaryFormText,
                        ref prePronoun,
                        ref pronouns,
                        ref preWords,
                        ref prefix,
                        ref stemText,
                        ref suffix,
                        ref postWords,
                        ref postPronouns,
                        ref isRegular);
                    if (isRegular)
                    {
                        regularPrefix = prefix;
                        regularStemText = stemText;
                        regularSuffix = suffix;
                    }
                }

                if (!String.IsNullOrEmpty(modifier.Function))
                    returnValue = ApplyFunction(
                        modifier.Function,
                        inflector,
                        modifier,
                        languageID,
                        languageIndex,
                        caseLabel,
                        isRegular,
                        ref dictionaryFormText,
                        ref prePronoun,
                        ref pronouns,
                        ref preWords,
                        ref prefix,
                        ref stemText,
                        ref suffix,
                        ref postWords,
                        ref postPronouns);

                if (modifier.Actions != null)
                    ApplyActions(
                        modifier.Actions,
                        languageIndex,
                        prefix + stemText + suffix,
                        caseLabel,
                        ref dictionaryFormText,
                        ref prePronoun,
                        ref pronouns,
                        ref preWords,
                        ref prefix,
                        ref stemText,
                        ref suffix,
                        ref postWords,
                        ref postPronouns,
                        ref isRegular);

                if (!isRegular)
                {
                    if (!String.IsNullOrEmpty(modifier.Function))
                        returnValue = ApplyFunction(
                            modifier.Function,
                            inflector,
                            modifier,
                            languageID,
                            languageIndex,
                            caseLabel,
                            true,
                            ref dictionaryFormText,
                            ref prePronoun,
                            ref pronouns,
                            ref preWords,
                            ref regularPrefix,
                            ref regularStemText,
                            ref regularSuffix,
                            ref postWords,
                        ref postPronouns);

                    if (modifier.Actions != null)
                        ApplyActions(
                            modifier.Actions,
                            languageIndex,
                            prefix + stemText + suffix,
                            caseLabel,
                            ref dictionaryFormText,
                            ref prePronoun,
                            ref pronouns,
                            ref preWords,
                            ref regularPrefix,
                            ref regularStemText,
                            ref regularSuffix,
                            ref postWords,
                            ref postPronouns,
                            ref isRegular);
                }
                else
                {
                    regularPrefix = prefix;
                    regularStemText = stemText;
                    regularSuffix = suffix;
                }

                inflected = prefix + stemText + suffix;
                regularInflected = regularPrefix + regularStemText + regularSuffix;

                if (((semiRegularPre == null) || !semiRegularPre.NoPost) && ((irregular == null) || !irregular.NoPost))
                {
                    SemiRegular semiRegularPost = FindSemiRegularPost(
                        semiRegulars,
                        dictionaryFormText,
                        prefix,
                        stemText,
                        suffix,
                        inflected,
                        instance);

                    if (semiRegularPost != null)
                    {
                        if (ApplyActions(
                                semiRegularPost.Actions,
                                languageIndex,
                                inflected,
                                caseLabel,
                                ref dictionaryFormText,
                                ref prePronoun,
                                ref pronouns,
                                ref preWords,
                                ref prefix,
                                ref stemText,
                                ref suffix,
                                ref postWords,
                                ref postPronouns,
                                ref isRegular))
                            inflected = prefix + stemText + suffix;
                    }
                }

                if (!String.IsNullOrEmpty(prePronoun))
                    prePronounSpaced = prePronoun + space;

                if (!String.IsNullOrEmpty(preWords))
                    preWordsSpaced = preWords + space;

                if (!String.IsNullOrEmpty(postWords))
                    postWordsSpaced = space + postWords;

                if (!String.IsNullOrEmpty(pronouns))
                    pronounsSpaced = /*"(" +*/ pronouns + /*")" +*/ space;

                if (!String.IsNullOrEmpty(postPronouns))
                    postPronounsSpaced = space + /*"(" +*/ postPronouns /*+ ")"*/;

                mainInflected = preWordsSpaced + inflected + postWordsSpaced;
                outputText = (_UsesImplicitPronouns ? prePronounSpaced : "") + mainInflected + postPronounsSpaced;
                pronounOutputText = prePronounSpaced + pronounsSpaced + mainInflected + postPronounsSpaced;
                regularOutputText = (_UsesImplicitPronouns ? prePronounSpaced : "") + preWordsSpaced + regularInflected + postWordsSpaced + postPronounsSpaced;
                regularPronounOutputText = prePronounSpaced + pronounsSpaced + regularOutputText;

                if (inflector.HasAnyPostActions())
                {
                    foreach (SpecialAction action in inflector.PostActions)
                    {
                        if (ApplyPostAction(
                                action,
                                languageID,
                                languageIndex,
                                inflected,
                                caseLabel,
                                ref dictionaryFormText,
                                ref prePronoun,
                                ref pronouns,
                                ref preWords,
                                ref prefix,
                                ref stemText,
                                ref suffix,
                                ref postWords,
                                ref postPronouns,
                                ref outputText,
                                ref pronounOutputText,
                                ref regularOutputText,
                                ref regularPronounOutputText,
                                ref isRegular,
                                contractedOutputList))
                            inflected = prefix + stemText + suffix;
                    }
                }

                inflection.PrePronoun.SetText(languageID, prePronoun);
                inflection.Pronoun.SetText(languageID, pronouns);
                inflection.PreWords.SetText(languageID, preWords);
                inflection.Prefix.SetText(languageID, prefix);
                inflection.Suffix.SetText(languageID, suffix);
                inflection.PostWords.SetText(languageID, postWords);
                inflection.PostPronoun.SetText(languageID, postPronouns);
                inflection.DictionaryForm.SetText(languageID, dictionaryFormText);

                inflection.MainInflected.SetText(languageID, mainInflected);
                inflection.Output.SetText(languageID, outputText);
                inflection.PronounOutput.SetText(languageID, pronounOutputText);
                inflection.RegularOutput.SetText(languageID, regularOutputText);
                inflection.RegularPronounOutput.SetText(languageID, regularPronounOutputText);
                inflection.Root.SetText(languageID, stemText);

                languageIndex++;
            }

            if ((contractedOutputList != null) && (contractedOutputList.Count() != 0))
                inflection.ContractedOutput = contractedOutputList;

            return returnValue;
        }

        public virtual bool ApplyStemModifier(
            Inflector inflector,
            Modifier modifier,
            LanguageID languageID,
            int languageIndex,
            string caseLabel,
            bool isRegular,
            ref string dictionaryFormText,
            ref string prePronoun,
            ref string pronouns,
            ref string preWords,
            ref string prefix,
            ref string stemText,
            ref string suffix,
            ref string regularPrefix,
            ref string regularStemText,
            ref string regularSuffix,
            ref string postWords,
            ref string postPronouns)
        {
            bool returnValue = false;

            if (modifier.PrePronoun != null)
                prePronoun += modifier.PrePronoun.GetIndexedString(languageIndex);

            if (modifier.PreWords != null)
                preWords += modifier.PreWords.GetIndexedString(languageIndex);

            if (modifier.Prefix != null)
            {
                prefix = modifier.Prefix.GetIndexedString(languageIndex) + prefix;
                regularPrefix = modifier.Prefix.GetIndexedString(languageIndex) + regularPrefix;
            }

            if (modifier.Suffix != null)
            {
                suffix += modifier.Suffix.GetIndexedString(languageIndex);
                regularSuffix += modifier.Suffix.GetIndexedString(languageIndex);
            }

            if (modifier.PostWords != null)
                postWords += modifier.PostWords.GetIndexedString(languageIndex);

            if (!String.IsNullOrEmpty(modifier.Function))
            {
                returnValue = ApplyFunction(
                    modifier.Function,
                    inflector,
                    modifier,
                    languageID,
                    languageIndex,
                    caseLabel,
                    true,
                    ref dictionaryFormText,
                    ref prePronoun,
                    ref pronouns,
                    ref preWords,
                    ref prefix,
                    ref stemText,
                    ref suffix,
                    ref postWords,
                    ref postPronouns);

                if (!isRegular)
                    returnValue = ApplyFunction(
                        modifier.Function,
                        inflector,
                        modifier,
                        languageID,
                        languageIndex,
                        caseLabel,
                        true,
                        ref dictionaryFormText,
                        ref prePronoun,
                        ref pronouns,
                        ref preWords,
                        ref regularPrefix,
                        ref regularStemText,
                        ref regularSuffix,
                        ref postWords,
                        ref postPronouns);
            }

            if (modifier.Actions != null)
                ApplyActions(
                    modifier.Actions,
                    languageIndex,
                    prefix + stemText + suffix,
                    caseLabel,
                    ref dictionaryFormText,
                    ref prePronoun,
                    ref pronouns,
                    ref preWords,
                    ref prefix,
                    ref stemText,
                    ref suffix,
                    ref postWords,
                    ref postPronouns,
                    ref isRegular);

            return returnValue;
        }

        public virtual bool InflectionModify(
            Inflection inflection,
            List<LanguageID> languageIDs,
            Inflector inflector,
            Modifier modifier)
        {
            int languageIndex = 0;
            int languageCount = languageIDs.Count;
            string caseLabel = inflector.Designator.Label;
            bool returnValue = true;

            foreach (LanguageID languageID in languageIDs)
            {
                string space = (LanguageLookup.IsUseSpacesToSeparateWords(languageID) ? " " : "");
                string dictionaryFormText = inflection.DictionaryForm.Text(languageID);
                string stemText = inflection.Root.Text(languageID);
                string regularStemText = stemText;
                string prePronoun = inflection.PrePronoun.Text(languageID);
                string prePronounSpaced = String.Empty;
                string preWords = inflection.PreWords.Text(languageID);
                string preWordsSpaced = String.Empty;
                string prefix = inflection.Prefix.Text(languageID);
                string suffix = inflection.Suffix.Text(languageID);
                string regularPrefix = String.Empty;
                string regularSuffix = String.Empty;
                string postWords = inflection.PostWords.Text(languageID);
                string postWordsSpaced = String.Empty;
                string pronouns = inflection.Pronoun.Text(languageID);
                string pronounsSpaced = String.Empty;
                string postPronouns = inflection.PostPronoun.Text(languageID);
                string postPronounsSpaced = String.Empty;
                string inflected = String.Empty;
                string mainInflected = String.Empty;
                string outputText = String.Empty;
                string pronounOutputText = String.Empty;
                string regularInflected = String.Empty;
                string regularOutputText = String.Empty;
                string regularPronounOutputText = String.Empty;
                bool isRegular = inflection.IsRegular;

                if (modifier.PrePronoun != null)
                    prePronoun += modifier.PrePronoun.GetIndexedString(languageIndex);

                if (modifier.PreWords != null)
                    preWords += modifier.PreWords.GetIndexedString(languageIndex);

                if (modifier.Prefix != null)
                {
                    prefix = modifier.Prefix.GetIndexedString(languageIndex) + prefix;
                    regularPrefix = modifier.Prefix.GetIndexedString(languageIndex) + regularPrefix;
                }

                if (modifier.Suffix != null)
                {
                    suffix += modifier.Suffix.GetIndexedString(languageIndex);
                    regularSuffix += modifier.Suffix.GetIndexedString(languageIndex);
                }

                if (modifier.PostWords != null)
                    postWords += modifier.PostWords.GetIndexedString(languageIndex);

                if (inflector.HasAnyPronouns())
                {
                    foreach (LiteralString pronoun in inflector.Pronouns)
                    {
                        string pronounString = pronoun.GetIndexedString(languageIndex);

                        if (!String.IsNullOrEmpty(pronouns))
                            pronouns += "/";

                        pronouns += pronounString;
                    }
                }

                if (!String.IsNullOrEmpty(modifier.Function))
                    returnValue = ApplyFunction(
                        modifier.Function,
                        inflector,
                        modifier,
                        languageID,
                        languageIndex,
                        caseLabel,
                        isRegular,
                        ref dictionaryFormText,
                        ref prePronoun,
                        ref pronouns,
                        ref preWords,
                        ref prefix,
                        ref stemText,
                        ref suffix,
                        ref postWords,
                        ref postPronouns);

                if (modifier.Actions != null)
                    ApplyActions(
                        modifier.Actions,
                        languageIndex,
                        prefix + stemText + suffix,
                        caseLabel,
                        ref dictionaryFormText,
                        ref prePronoun,
                        ref pronouns,
                        ref preWords,
                        ref prefix,
                        ref stemText,
                        ref suffix,
                        ref postWords,
                        ref postPronouns,
                        ref isRegular);

                if (!isRegular)
                {
                    if (!String.IsNullOrEmpty(modifier.Function))
                        returnValue = ApplyFunction(
                            modifier.Function,
                            inflector,
                            modifier,
                            languageID,
                            languageIndex,
                            caseLabel,
                            true,
                            ref dictionaryFormText,
                            ref prePronoun,
                            ref pronouns,
                            ref preWords,
                            ref regularPrefix,
                            ref regularStemText,
                            ref regularSuffix,
                            ref postWords,
                        ref postPronouns);

                    if (modifier.Actions != null)
                        ApplyActions(
                            modifier.Actions,
                            languageIndex,
                            prefix + stemText + suffix,
                            caseLabel,
                            ref dictionaryFormText,
                            ref prePronoun,
                            ref pronouns,
                            ref preWords,
                            ref regularPrefix,
                            ref regularStemText,
                            ref regularSuffix,
                            ref postWords,
                        ref postPronouns,
                            ref isRegular);
                }
                else
                {
                    regularPrefix = prefix;
                    regularStemText = stemText;
                    regularSuffix = suffix;
                }

                inflected = prefix + stemText + suffix;
                regularInflected = regularPrefix + regularStemText + regularSuffix;

                if (!String.IsNullOrEmpty(prePronoun))
                    prePronounSpaced = prePronoun + space;

                if (!String.IsNullOrEmpty(preWords))
                    preWordsSpaced = preWords + space;

                if (!String.IsNullOrEmpty(postWords))
                    postWordsSpaced = space + postWords;

                if (!String.IsNullOrEmpty(pronouns))
                    pronounsSpaced = /*"(" +*/ pronouns + /*")" +*/ space;

                if (!String.IsNullOrEmpty(postPronouns))
                    postPronounsSpaced = space + /*"(" +*/ postPronouns /*+ ")"*/;

                mainInflected = preWordsSpaced + inflected + postWordsSpaced;
                outputText = (_UsesImplicitPronouns ? prePronounSpaced : "") + mainInflected + postPronounsSpaced;
                pronounOutputText = prePronounSpaced + pronounsSpaced + mainInflected + postPronounsSpaced;
                regularOutputText = (_UsesImplicitPronouns ? prePronounSpaced : "") + preWordsSpaced + regularInflected + postWordsSpaced + postPronounsSpaced;
                regularPronounOutputText = prePronounSpaced + pronounsSpaced + regularOutputText;

                inflection.PrePronoun.SetText(languageID, prePronoun);
                inflection.Pronoun.SetText(languageID, pronouns);
                inflection.PreWords.SetText(languageID, preWords);
                inflection.Prefix.SetText(languageID, prefix);
                inflection.Suffix.SetText(languageID, suffix);
                inflection.PostWords.SetText(languageID, postWords);
                inflection.PostPronoun.SetText(languageID, postPronouns);
                inflection.DictionaryForm.SetText(languageID, dictionaryFormText);

                inflection.MainInflected.SetText(languageID, mainInflected);
                inflection.Output.SetText(languageID, outputText);
                inflection.PronounOutput.SetText(languageID, pronounOutputText);
                inflection.RegularOutput.SetText(languageID, regularOutputText);
                inflection.RegularPronounOutput.SetText(languageID, regularPronounOutputText);
                inflection.Root.SetText(languageID, stemText);

                inflection.IsRegular = isRegular;

                languageIndex++;
            }

            return returnValue;
        }

        public virtual bool ApplyFunction(
            string function,
            Inflector inflector,
            Modifier modifier,
            LanguageID languageID,
            int languageIndex,
            string caseLabel,
            bool isRegular,
            ref string dictionaryFormText,
            ref string prePronoun,
            ref string pronouns,
            ref string preWords,
            ref string prefix,
            ref string stemText,
            ref string suffix,
            ref string postWords,
            ref string postPronoun)
        {
            bool returnValue = true;

            switch (function)
            {
                case null:
                case "":
                case "None":
                    break;
                case "Hide":
                    returnValue = false;
                    break;
                case "Dictionary":
                    returnValue = HandleFunctionDictionaryForm(
                        inflector,
                        modifier,
                        languageID,
                        languageIndex,
                        caseLabel,
                        isRegular,
                        ref dictionaryFormText,
                        ref prePronoun,
                        ref pronouns,
                        ref preWords,
                        ref prefix,
                        ref stemText,
                        ref suffix,
                        ref postWords,
                        ref postPronoun);
                    break;
                case "Stem":
                    break;
                default:
                    returnValue = false;
                    throw new Exception("Unsupported inflection function: " + function);
            }

            return returnValue;
        }

        public virtual bool HandleFunctionDictionaryForm(
            Inflector inflector,
            Modifier modifier,
            LanguageID languageID,
            int languageIndex,
            string caseLabel,
            bool isRegular,
            ref string dictionaryFormText,
            ref string prePronoun,
            ref string pronouns,
            ref string preWords,
            ref string prefix,
            ref string stemText,
            ref string suffix,
            ref string postWords,
            ref string postPronoun)
        {
            int stemOfs = dictionaryFormText.IndexOf(stemText);
            if (stemOfs == -1)
                return false;
            else if (stemOfs > 0)
                prefix = dictionaryFormText.Substring(0, stemOfs);
            if (stemOfs + stemText.Length < dictionaryFormText.Length)
                suffix = dictionaryFormText.Substring(stemOfs + stemText.Length);
            return true;
        }

        public virtual bool HandleModifierStem(
            Inflector inflector,
            Modifier modifier,
            LanguageID languageID,
            int languageIndex,
            string caseLabel,
            bool isRegular,
            ref string dictionaryFormText,
            ref string prePronoun,
            ref string pronouns,
            ref string preWords,
            ref string prefix,
            ref string stemText,
            ref string suffix,
            ref string postWords,
            ref string postPronoun)
        {
            return true;
        }

        public MultiLanguageString CopyMultiLanguageString(
            MultiLanguageString targetMLS,
            MultiLanguageString sourceMLS)
        {
            if (targetMLS == null)
                targetMLS = new Object.MultiLanguageString(sourceMLS);
            else
                targetMLS.CopyText(sourceMLS);

            return targetMLS;
        }

        public virtual SemiRegular FindSemiRegularPre(
            List<SemiRegular> semiRegulars,
            MultiLanguageString dictionaryForm,
            MultiLanguageString stem,
            string instance)
        {
            if (semiRegulars == null)
                return null;

            if (stem == null)
                return null;

            SemiRegular returnValue = null;
            int bestLength = 0;

            foreach (SemiRegular semiRegular in semiRegulars)
            {
                if (semiRegular.Post)
                    continue;

                if (semiRegular.HasTargets())
                {
                    if (semiRegular.MatchTarget(dictionaryForm) && semiRegular.MatchInstance(instance))
                        return semiRegular;
                }
            }

            foreach (SemiRegular semiRegular in semiRegulars)
            {
                if (semiRegular.Post || semiRegular.HasTargets())
                    continue;

                if (semiRegular.MatchCondition(dictionaryForm, this) && semiRegular.MatchInstance(instance))
                {
                    int length = semiRegular.ConditionsLength;

                    if (returnValue != null)
                    {
                        if (length > bestLength)
                        {
                            returnValue = semiRegular;
                            bestLength = length;
                        }
                    }
                    else
                    {
                        returnValue = semiRegular;
                        bestLength = length;
                    }
                }
            }

            return returnValue;
        }

        public virtual SemiRegular FindSemiRegularPost(
            List<SemiRegular> semiRegulars,
            string dictionaryForm,
            string prefix,
            string stem,
            string suffix,
            string inflected,
            string instance)
        {
            if (semiRegulars == null)
                return null;

            if (stem == null)
                return null;

            foreach (SemiRegular semiRegular in semiRegulars)
            {
                if (!semiRegular.Post)
                    continue;

                if (semiRegular.HasTargets())
                {
                    if (semiRegular.MatchTarget(dictionaryForm) && semiRegular.MatchInstance(instance))
                        return semiRegular;
                }
            }

            SemiRegular returnValue = null;
            int bestLength = 0;

            foreach (SemiRegular semiRegular in semiRegulars)
            {
                if (!semiRegular.Post || semiRegular.HasTargets() || !semiRegular.MatchInstance(instance))
                    continue;

                if (semiRegular.MatchConditionPost(
                    dictionaryForm,
                    prefix,
                    stem,
                    suffix,
                    inflected,
                    this))
                {
                    int length = semiRegular.ConditionsLength;

                    if (returnValue != null)
                    {
                        if (length > bestLength)
                        {
                            returnValue = semiRegular;
                            bestLength = length;
                        }
                    }
                    else
                    {
                        returnValue = semiRegular;
                        bestLength = length;
                    }
                }
            }

            return returnValue;
        }

        public virtual bool ApplyActions(
            List<SpecialAction> actions,
            int languageIndex,
            string inflected,
            string caseLabel,
            ref string dictionaryForm,
            ref string prePronoun,
            ref string pronouns,
            ref string preWords,
            ref string prefix,
            ref string stemText,
            ref string suffix,
            ref string postWords,
            ref string postPronoun,
            ref bool isRegular)
        {
            bool returnValue = false;

            if (actions == null)
                return returnValue;

            foreach (SpecialAction action in actions)
            {
                if (!action.MatchCases(caseLabel))
                    continue;

                bool done = false;

                ApplyAction(
                    action,
                    languageIndex,
                    inflected,
                    caseLabel,
                    ref dictionaryForm,
                    ref prePronoun,
                    ref pronouns,
                    ref preWords,
                    ref prefix,
                    ref stemText,
                    ref suffix,
                    ref postWords,
                    ref postPronoun,
                    ref isRegular,
                    ref done,
                    ref returnValue);

                if (done)
                    break;
            }

            return returnValue;
        }

        public virtual bool ApplyAction(
            SpecialAction action,
            int languageIndex,
            string inflected,
            string caseLabel,
            ref string dictionaryForm,
            ref string prePronoun,
            ref string pronouns,
            ref string preWords,
            ref string prefix,
            ref string stemText,
            ref string suffix,
            ref string postWords,
            ref string postPronoun,
            ref bool isRegular,
            ref bool done,
            ref bool returnValue)
        {
            if (action == null)
                return returnValue;

            switch (action.Type)
            {
                // Stem, Pre, Post common actions:

                case "Normal":
                    done = true;
                    returnValue = true;
                    break;

                // Stem inflector actions:

                case "StemRemoveDictionaryEnding":
                    {
                        string input = action.Input.GetIndexedString(languageIndex);
                        if (dictionaryForm.EndsWith(input))
                        {
                            stemText = dictionaryForm.Substring(0, dictionaryForm.Length - input.Length);
                            returnValue = true;
                            done = action.Done;
                            break;
                        }
                    }
                    break;
                case "StemReplaceDictionaryEnding":
                    {
                        string input = action.Input.GetIndexedString(languageIndex);
                        if (dictionaryForm.EndsWith(input))
                        {
                            string output = action.Output.GetIndexedString(languageIndex);
                            stemText = dictionaryForm.Substring(0, dictionaryForm.Length - input.Length) +
                                output;
                            returnValue = true;
                            done = action.Done;
                            break;
                        }
                    }
                    break;

                // Pre actions:

                case "Set":
                    if (action.Output != null)
                    {
                        string output = action.Output.GetIndexedString(languageIndex);
                        prefix = String.Empty;
                        stemText = output;
                        suffix = String.Empty;
                        returnValue = true;
                        done = action.Done;
                        break;
                    }
                    break;
                case "ReplaceStem":
                    if (action.Qualifiers != null)
                    {
                        if (action.Qualifiers.Contains(suffix))
                        {
                            if (action.Output != null)
                            {
                                stemText = action.Output.GetIndexedString(languageIndex);
                                isRegular = action.Regular;
                                returnValue = true;
                                done = action.Done;
                            }
                            else if (action.Stem != null)
                            {
                                stemText = action.Stem.GetIndexedString(languageIndex);
                                isRegular = action.Regular;
                                returnValue = true;
                                done = action.Done;
                            }
                            break;
                        }
                    }
                    else if (action.Input != null)
                    {
                        if (action.Output != null)
                        {
                            stemText = action.Output.GetIndexedString(languageIndex);
                            isRegular = action.Regular;
                            returnValue = true;
                            done = action.Done;
                        }
                        else if (action.Stem != null)
                        {
                            stemText = action.Stem.GetIndexedString(languageIndex);
                            isRegular = action.Regular;
                            returnValue = true;
                            done = action.Done;
                        }
                    }
                    else
                    {
                        stemText = action.Stem.GetIndexedString(languageIndex);
                        isRegular = action.Regular;
                        returnValue = true;
                        done = action.Done;
                        break;
                    }
                    break;
                case "ReplaceInStem":
                    if (action.Qualifiers != null)
                    {
                        if (action.Qualifiers.Contains(suffix))
                        {
                            if ((action.Input != null) && (action.Output != null))
                            {
                                string input = action.Input.GetIndexedString(languageIndex);
                                string output = action.Output.GetIndexedString(languageIndex);
                                if (!String.IsNullOrEmpty(input) && stemText.Contains(input))
                                {
                                    stemText = stemText.Replace(input, output);
                                    isRegular = action.Regular;
                                    returnValue = true;
                                    done = action.Done;
                                }
                                else
                                    throw new Exception("ApplyActions: ReplaceInStem: input empty or stem doesn't contain input.");
                            }
                            else
                                throw new Exception("ApplyActions: ReplaceInStem: No input or output.");
                        }
                    }
                    else if ((action.Input != null) && (action.Output != null))
                    {
                        string input = action.Input.GetIndexedString(languageIndex);
                        string output = action.Output.GetIndexedString(languageIndex);
                        if (!String.IsNullOrEmpty(input) && stemText.Contains(input))
                        {
                            stemText = stemText.Replace(input, output);
                            isRegular = action.Regular;
                            returnValue = true;
                            done = action.Done;
                        }
                        //else
                        //    throw new Exception("ApplyActions: ReplaceInStem: input empty or stem doesn't contain input.");
                    }
                    else
                        throw new Exception("ApplyActions: ReplaceInStem: missing input or output.");
                    break;
                case "AppendToStem":
                    if (action.Qualifiers != null)
                    {
                        if (action.Qualifiers.Contains(suffix))
                        {
                            if (action.Output != null)
                            {
                                string output = action.Output.GetIndexedString(languageIndex);
                                if (!String.IsNullOrEmpty(output))
                                {
                                    stemText = stemText + output;
                                    isRegular = action.Regular;
                                    returnValue = true;
                                    done = action.Done;
                                }
                                else
                                    throw new Exception("ApplyActions: AppendToStem: output empty.");
                            }
                        }
                    }
                    else if (action.Output != null)
                    {
                        string output = action.Output.GetIndexedString(languageIndex);
                        if (!String.IsNullOrEmpty(output))
                        {
                            stemText = stemText + output;
                            isRegular = action.Regular;
                            returnValue = true;
                            done = action.Done;
                        }
                        else
                            throw new Exception("ApplyActions: AppendToStem: output empty.");
                    }
                    else
                        throw new Exception("ApplyActions: AppendToStem: missing output.");
                    break;
                case "ReplaceStemEnding":
                    {
                        if (action.Qualifiers != null)
                        {
                            string stemEnding = action.Input.GetIndexedString(languageIndex);
                            if (action.Qualifiers.Contains(suffix) && stemText.EndsWith(stemEnding))
                            {
                                stemText = stemText.Substring(0, stemText.Length - stemEnding.Length);
                                stemText += action.Output.GetIndexedString(languageIndex);
                                isRegular = action.Regular;
                                returnValue = true;
                                done = action.Done;
                                break;
                            }
                        }
                        else
                        {
                            string stemEnding = action.Input.GetIndexedString(languageIndex);
                            if (stemText.EndsWith(stemEnding))
                            {
                                stemText = stemText.Substring(0, stemText.Length - stemEnding.Length);
                                stemText += action.Output.GetIndexedString(languageIndex);
                                isRegular = action.Regular;
                                returnValue = true;
                                done = action.Done;
                            }
                            break;
                        }
                    }
                    break;
                case "ReplaceStemAndSuffix":
                    if (action.Qualifiers != null)
                    {
                        if (action.Qualifiers.Contains(suffix))
                        {
                            stemText = action.Stem.GetIndexedString(languageIndex);
                            suffix = action.Suffix.GetIndexedString(languageIndex);
                            isRegular = action.Regular;
                            done = true;
                            returnValue = true;
                        }
                    }
                    else
                    {
                        stemText = action.Stem.GetIndexedString(languageIndex);
                        suffix = action.Suffix.GetIndexedString(languageIndex);
                        isRegular = action.Regular;
                        done = true;
                        returnValue = true;
                    }
                    break;
                case "ReplaceInStemAndSuffix":
                    if (action.Qualifiers != null)
                    {
                        if (action.Qualifiers.Contains(suffix))
                        {
                            if ((action.Input != null) && (action.Output != null))
                            {
                                string input = action.Input.GetIndexedString(languageIndex);
                                string output = action.Output.GetIndexedString(languageIndex);
                                if (!String.IsNullOrEmpty(input) && stemText.Contains(input))
                                {
                                    stemText = stemText.Replace(input, output);
                                    suffix = action.Suffix.GetIndexedString(languageIndex);
                                    isRegular = action.Regular;
                                    returnValue = true;
                                    done = action.Done;
                                }
                                //else
                                //    throw new Exception("ApplyActions: ReplaceInStemAndSuffix: input empty or stem doesn't contain input.");
                            }
                            else
                                throw new Exception("ApplyActions: ReplaceInStemAndSuffix: No input or output.");
                        }
                    }
                    if ((action.Input != null) && (action.Output != null))
                    {
                        string input = action.Input.GetIndexedString(languageIndex);
                        string output = action.Output.GetIndexedString(languageIndex);
                        if (!String.IsNullOrEmpty(input) && stemText.Contains(input))
                        {
                            stemText = stemText.Replace(input, output);
                            suffix = action.Suffix.GetIndexedString(languageIndex);
                            isRegular = action.Regular;
                            returnValue = true;
                            done = action.Done;
                        }
                        //else
                        //    throw new Exception("ReplaceInStemAndSuffix: ReplaceInStem: input empty or stem doesn't contain input.");
                    }
                    else
                        throw new Exception("ReplaceInStemAndSuffix: ReplaceInStem: missing input or output.");
                    break;
                case "AppendToStemReplaceSuffix":
                    if (action.Qualifiers != null)
                    {
                        if (action.Qualifiers.Contains(suffix))
                        {
                            if (action.Output != null)
                            {
                                string output = action.Output.GetIndexedString(languageIndex);
                                if (!String.IsNullOrEmpty(output))
                                {
                                    stemText = stemText + output;
                                    suffix = action.Suffix.GetIndexedString(languageIndex);
                                    isRegular = action.Regular;
                                    returnValue = true;
                                    done = action.Done;
                                }
                                //else
                                //    throw new Exception("ApplyActions: AppendToStemReplaceSuffix: output empty.");
                            }
                            if (action.Output != null)
                            {
                                string output = action.Output.GetIndexedString(languageIndex);
                                if (!String.IsNullOrEmpty(output))
                                {
                                    stemText = stemText + output;
                                    suffix = action.Suffix.GetIndexedString(languageIndex);
                                    isRegular = action.Regular;
                                    returnValue = true;
                                    done = action.Done;
                                }
                                else
                                    throw new Exception("ApplyActions: AppendToStemReplaceSuffix: output empty.");
                            }
                        }
                    }
                    else if (action.Output != null)
                    {
                        string output = action.Output.GetIndexedString(languageIndex);
                        if (!String.IsNullOrEmpty(output))
                        {
                            stemText = stemText + output;
                            isRegular = action.Regular;
                            returnValue = true;
                            done = action.Done;
                        }
                        else
                            throw new Exception("ApplyActions: AppendToStem: output empty.");
                    }
                    else
                        throw new Exception("ApplyActions: AppendToStem: missing output.");
                    break;
                case "ReplaceStemClearSuffix":
                    if (action.Qualifiers != null)
                    {
                        if (action.Qualifiers.Contains(suffix))
                        {
                            stemText = action.Stem.GetIndexedString(languageIndex);
                            suffix = "";
                            isRegular = action.Regular;
                            done = true;
                            returnValue = true;
                            break;
                        }
                    }
                    else
                    {
                        stemText = action.Stem.GetIndexedString(languageIndex);
                        suffix = "";
                        isRegular = action.Regular;
                        done = true;
                        returnValue = true;
                        break;
                    }
                    break;
                case "StemEndingClearSuffix":
                    {
                        string input = action.Input.GetIndexedString(languageIndex);
                        if (stemText.EndsWith(input))
                        {
                            string output = action.Output.GetIndexedString(languageIndex);
                            stemText = stemText.Substring(0, stemText.Length - input.Length) +
                                output;
                            suffix = "";
                            isRegular = action.Regular;
                            returnValue = true;
                            done = action.Done;
                            break;
                        }
                    }
                    break;
                case "StemEndingBySuffix":
                    if (action.Qualifiers != null)
                    {
                        if (action.Qualifiers.Contains(suffix))
                        {
                            string input = action.Input.GetIndexedString(languageIndex);
                            string output = action.Output.GetIndexedString(languageIndex);
                            if (stemText.EndsWith(input))
                            {
                                stemText = stemText.Substring(0, stemText.Length - input.Length) +
                                    output;
                                isRegular = action.Regular;
                                returnValue = true;
                                done = action.Done;
                                break;
                            }
                        }
                    }
                    break;
                case "AppendBySuffix":
                    if (action.Qualifiers != null)
                    {
                        if (action.Qualifiers.Contains(suffix))
                        {
                            string output = action.Output.GetIndexedString(languageIndex);
                            stemText = stemText + output;
                            isRegular = action.Regular;
                            returnValue = true;
                            done = action.Done;
                            break;
                        }
                    }
                    break;
                case "SuffixStartBySuffix":
                    if (action.Qualifiers != null)
                    {
                        if (action.Qualifiers.Contains(suffix))
                        {
                            string input = action.Input.GetIndexedString(languageIndex);
                            string output = action.Output.GetIndexedString(languageIndex);
                            if (suffix.StartsWith(input))
                            {
                                suffix = output + suffix.Substring(input.Length);
                                isRegular = action.Regular;
                                returnValue = true;
                                done = action.Done;
                                break;
                            }
                        }
                    }
                    break;
                case "ClearSuffix":
                    {
                        string input = (action.Input != null ? action.Input.GetIndexedString(languageIndex) : String.Empty);
                        if ((input == suffix) || String.IsNullOrEmpty(input))
                        {
                            suffix = "";
                            isRegular = action.Regular;
                            returnValue = true;
                            done = action.Done;
                            break;
                        }
                    }
                    break;
                case "SetSuffix":
                    {
                        suffix = action.Output.GetIndexedString(languageIndex);
                        isRegular = action.Regular;
                        returnValue = true;
                        done = action.Done;
                    }
                    break;
                case "AppendSuffix":
                    {
                        suffix += action.Output.GetIndexedString(languageIndex);
                        isRegular = action.Regular;
                        returnValue = true;
                        done = action.Done;
                    }
                    break;
                case "PrependSuffix":
                    {
                        suffix = action.Output.GetIndexedString(languageIndex) + suffix;
                        isRegular = action.Regular;
                        returnValue = true;
                        done = action.Done;
                    }
                    break;
                case "ReplaceSuffix":
                    {
                        string input = (action.Input != null ? action.Input.GetIndexedString(languageIndex) : String.Empty);
                        if ((input == suffix) || String.IsNullOrEmpty(input))
                        {
                            string output = action.Output.GetIndexedString(languageIndex);
                            suffix = output;
                            isRegular = action.Regular;
                            returnValue = true;
                            done = action.Done;
                            break;
                        }
                    }
                    break;
                case "ReplaceSuffixEnding":
                    {
                        string input = (action.Input != null ? action.Input.GetIndexedString(languageIndex) : String.Empty);
                        if (suffix.EndsWith(input))
                        {
                            string output = action.Output.GetIndexedString(languageIndex);
                            if (suffix.Length > input.Length)
                                suffix = suffix.Substring(0, suffix.Length - input.Length) + output;
                            else
                                suffix = output;
                            isRegular = action.Regular;
                            returnValue = true;
                            done = action.Done;
                            break;
                        }
                    }
                    break;
                case "ReplaceDictionarySuffix":
                    {
                        int stemIndex = dictionaryForm.IndexOf(stemText);
                        if (stemIndex == -1)
                            break;
                        int endingIndex = stemIndex + stemText.Length;
                        string dictionarySuffix = dictionaryForm.Substring(endingIndex);
                        string input = (action.Input != null ? action.Input.GetIndexedString(languageIndex) : String.Empty);
                        if ((input == dictionarySuffix) || String.IsNullOrEmpty(input))
                        {
                            string output = action.Output.GetIndexedString(languageIndex);
                            suffix = output;
                            isRegular = action.Regular;
                            returnValue = true;
                            done = action.Done;
                            break;
                        }
                    }
                    break;
                case "StopAtSuffix":
                    {
                        string input = (action.Input != null ? action.Input.GetIndexedString(languageIndex) : String.Empty);
                        if ((input == suffix) || String.IsNullOrEmpty(input))
                        {
                            isRegular = action.Regular;
                            returnValue = true;
                            done = true;
                            break;
                        }
                    }
                    break;
                case "SetPrefix":
                    {
                        prefix = action.Output.GetIndexedString(languageIndex);
                        isRegular = action.Regular;
                        returnValue = true;
                        done = action.Done;
                    }
                    break;
                case "PrependPrefix":
                    {
                        prefix = action.Output.GetIndexedString(languageIndex) + prefix;
                        isRegular = action.Regular;
                        returnValue = true;
                        done = action.Done;
                    }
                    break;
                case "AppendPrefix":
                    {
                        prefix += action.Output.GetIndexedString(languageIndex);
                        isRegular = action.Regular;
                        returnValue = true;
                        done = action.Done;
                    }
                    break;
                case "ClearPrePronoun":
                    {
                        string input = (action.Input != null ? action.Input.GetIndexedString(languageIndex) : String.Empty);
                        if ((input == prePronoun) || String.IsNullOrEmpty(input))
                        {
                            string output = action.Output.GetIndexedString(languageIndex);
                            prePronoun = "";
                            isRegular = action.Regular;
                            returnValue = true;
                            done = action.Done;
                            break;
                        }
                    }
                    break;
                case "ReplacePrePronoun":
                    {
                        string input = (action.Input != null ? action.Input.GetIndexedString(languageIndex) : String.Empty);
                        if ((input == prePronoun) || String.IsNullOrEmpty(input))
                        {
                            string output = action.Output.GetIndexedString(languageIndex);
                            prePronoun = output;
                            isRegular = action.Regular;
                            returnValue = true;
                            done = action.Done;
                            break;
                        }
                    }
                    break;
                case "ReplaceInPrePronoun":
                    if ((action.Input != null) && (action.Output != null))
                    {
                        string input = action.Input.GetIndexedString(languageIndex);
                        string output = action.Output.GetIndexedString(languageIndex);
                        if (!String.IsNullOrEmpty(input) && prePronoun.Contains(input))
                        {
                            prePronoun = prePronoun.Replace(input, output);
                            isRegular = action.Regular;
                            returnValue = true;
                            done = action.Done;
                        }
                        else
                            throw new Exception("ApplyActions: ReplaceInPrePronoun: input empty or stem doesn't contain input.");
                    }
                    else
                        throw new Exception("ApplyActions: ReplaceInPrePronoun: missing input or output.");
                    break;
                case "ClearPreWords":
                    {
                        string input = (action.Input != null ? action.Input.GetIndexedString(languageIndex) : String.Empty);
                        if ((input == preWords) || String.IsNullOrEmpty(input))
                        {
                            string output = action.Output.GetIndexedString(languageIndex);
                            preWords = "";
                            isRegular = action.Regular;
                            returnValue = true;
                            done = action.Done;
                            break;
                        }
                    }
                    break;
                case "ReplacePreWords":
                    {
                        string input = (action.Input != null ? action.Input.GetIndexedString(languageIndex) : String.Empty);
                        if ((input == preWords) || String.IsNullOrEmpty(input))
                        {
                            string output = action.Output.GetIndexedString(languageIndex);
                            preWords = output;
                            isRegular = action.Regular;
                            returnValue = true;
                            done = action.Done;
                            break;
                        }
                    }
                    break;
                case "ReplaceInPreWords":
                    if ((action.Input != null) && (action.Output != null))
                    {
                        string input = action.Input.GetIndexedString(languageIndex);
                        string output = action.Output.GetIndexedString(languageIndex);
                        if (!String.IsNullOrEmpty(input) && preWords.Contains(input))
                        {
                            preWords = preWords.Replace(input, output);
                            isRegular = action.Regular;
                            returnValue = true;
                            done = action.Done;
                        }
                        else
                            throw new Exception("ApplyActions: ReplaceInPreWords: input empty or stem doesn't contain input.");
                    }
                    else
                        throw new Exception("ApplyActions: ReplaceInPreWords: missing input or output.");
                    break;
                case "ClearPostWords":
                    {
                        string input = (action.Input != null ? action.Input.GetIndexedString(languageIndex) : String.Empty);
                        if ((input == postWords) || String.IsNullOrEmpty(input))
                        {
                            string output = action.Output.GetIndexedString(languageIndex);
                            postWords = "";
                            isRegular = action.Regular;
                            returnValue = true;
                            done = action.Done;
                            break;
                        }
                    }
                    break;
                case "ReplacePostWords":
                    {
                        string input = (action.Input != null ? action.Input.GetIndexedString(languageIndex) : String.Empty);
                        if ((input == postWords) || String.IsNullOrEmpty(input))
                        {
                            string output = action.Output.GetIndexedString(languageIndex);
                            postWords = output;
                            isRegular = action.Regular;
                            returnValue = true;
                            done = action.Done;
                            break;
                        }
                    }
                    break;
                case "ReplaceInPostWords":
                    if ((action.Input != null) && (action.Output != null))
                    {
                        string input = action.Input.GetIndexedString(languageIndex);
                        string output = action.Output.GetIndexedString(languageIndex);
                        if (!String.IsNullOrEmpty(input) && postWords.Contains(input))
                        {
                            postWords = postWords.Replace(input, output);
                            isRegular = action.Regular;
                            returnValue = true;
                            done = action.Done;
                        }
                        else
                            throw new Exception("ApplyActions: ReplaceInPostWords: input empty or stem doesn't contain input.");
                    }
                    else
                        throw new Exception("ApplyActions: ReplaceInPostWords: missing input or output.");
                    break;
                case "ReplacePostWordsBeginning":
                    {
                        string input = (action.Input != null ? action.Input.GetIndexedString(languageIndex) : String.Empty);
                        if (!String.IsNullOrEmpty(input) && postWords.StartsWith(input))
                        {
                            string output = action.Output.GetIndexedString(languageIndex);
                            postWords = output + postWords.Substring(input.Length);
                            isRegular = action.Regular;
                            returnValue = true;
                            done = action.Done;
                            break;
                        }
                    }
                    break;
                case "MergeSuffixAndPostWords":
                    suffix += postWords;
                    postWords = String.Empty;
                    isRegular = action.Regular;
                    returnValue = true;
                    done = action.Done;
                    break;
                case "ClearAndSet":
                    {
                        string output = (action.Output != null ? action.Output.GetIndexedString(languageIndex) : String.Empty);
                        if (!String.IsNullOrEmpty(output))
                        {
                            switch (action.From)
                            {
                                case "PrePronoun":
                                    prePronoun = String.Empty;
                                    break;
                                case "Pronouns":
                                    pronouns = String.Empty;
                                    break;
                                case "PreWords":
                                    preWords = String.Empty;
                                    break;
                                case "Prefix":
                                    prefix = String.Empty;
                                    break;
                                case "StemText":
                                    stemText = String.Empty;
                                    break;
                                case "Suffix":
                                    suffix = String.Empty;
                                    break;
                                case "PostWords":
                                    postWords = String.Empty;
                                    break;
                                case "PostPronoun":
                                    postPronoun = String.Empty;
                                    break;
                                default:
                                    throw new Exception("ApplyAction: Unknown To specifier: " + action.From);
                            }
                            switch (action.To)
                            {
                                case "PrePronoun":
                                    prePronoun = output;
                                    break;
                                case "Pronouns":
                                    pronouns = output;
                                    break;
                                case "PreWords":
                                    preWords = output;
                                    break;
                                case "Prefix":
                                    prefix = output;
                                    break;
                                case "StemText":
                                    stemText = output;
                                    break;
                                case "Suffix":
                                    suffix = output;
                                    break;
                                case "PostWords":
                                    postWords = output;
                                    break;
                                case "PostPronoun":
                                    postPronoun = output;
                                    break;
                                default:
                                    throw new Exception("ApplyAction: Unknown From specifier: " + action.From);
                            }
                            isRegular = action.Regular;
                            returnValue = true;
                            done = action.Done;
                        }
                    }
                    break;
                case "AccentLastOfMultiple":
                    if (GetSyllableCount(stemText) > 1)
                    {
                        AccentLastVowel(ref stemText);
                        isRegular = action.Regular;
                        done = action.Done;
                    }
                    break;

                // Post actions:

                case "First":
                    if (action.Qualifiers != null)
                    {
                        if (action.Qualifiers.Contains(suffix))
                        {
                            string input = action.Input.GetIndexedString(languageIndex);
                            int index = inflected.IndexOf(input);
                            if (index != -1)
                            {
                                string output = action.Output.GetIndexedString(languageIndex);
                                string stemPre = stemText.Substring(0, index);
                                string stemPost = stemText.Substring(index + 1);
                                stemText = stemPre + output + stemPost;
                                returnValue = true;
                                done = action.Done;
                                break;
                            }
                        }
                    }
                    break;
                case "StemAccentedVowel":
                    {
                        char accentedVowel;
                        int index;
                        if (GetAccentedVowel(inflected, out accentedVowel, out index))
                        {
                            string input = action.Input.GetIndexedString(languageIndex);

                            if (accentedVowel == input[0])
                            {
                                index -= prefix.Length;

                                if (index >= 0)
                                {
                                    if (index < stemText.Length)
                                    {
                                        string output = action.Output.GetIndexedString(languageIndex);
                                        string stemPre = stemText.Substring(0, index);
                                        string stemPost = stemText.Substring(index + 1);
                                        stemText = stemPre + output + stemPost;
                                        returnValue = true;
                                        done = action.Done;
                                    }
                                }
                            }
                        }
                    }
                    break;

                default:
                    throw new Exception("Unsupported action type: " + action.Type);
            }

            return returnValue;
        }

        public virtual bool ApplyPostAction(
            SpecialAction action,
            LanguageID languageID,
            int languageIndex,
            string inflected,
            string caseLabel,
            ref string dictionaryForm,
            ref string prePronoun,
            ref string pronouns,
            ref string preWords,
            ref string prefix,
            ref string stemText,
            ref string suffix,
            ref string postWords,
            ref string postPronoun,
            ref string outputText,
            ref string pronounOutputText,
            ref string regularOutputText,
            ref string regularPronounOutputText,
            ref bool isRegular,
            List<MultiLanguageString> contractedOutputList)
        {
            bool returnValue = false;

            if (action == null)
                return returnValue;

            switch (action.Type)
            {
                case "ReplaceInOutput":
                    if ((action.Input != null) && (action.Output != null))
                    {
                        string input = action.Input.GetIndexedString(languageIndex);
                        string output = action.Output.GetIndexedString(languageIndex);
                        if (!String.IsNullOrEmpty(input))
                        {
                            outputText = outputText.Replace(input, output);
                            pronounOutputText = pronounOutputText.Replace(input, output);
                            regularOutputText = regularOutputText.Replace(input, output);
                            regularPronounOutputText = regularPronounOutputText.Replace(input, output);
                            ContractedReplace(contractedOutputList, input, output, languageID);
                            returnValue = true;
                        }
                    }
                    else
                        throw new Exception("ApplyPostAction: ReplaceInPronounOutput: missing input or output.");
                    break;
                case "ReplaceInPronounOutput":
                    if ((action.Input != null) && (action.Output != null))
                    {
                        string input = action.Input.GetIndexedString(languageIndex);
                        string output = action.Output.GetIndexedString(languageIndex);
                        if (!String.IsNullOrEmpty(input))
                        {
                            pronounOutputText = pronounOutputText.Replace(input, output);
                            regularPronounOutputText = regularPronounOutputText.Replace(input, output);
                            ContractedReplace(contractedOutputList, input, output, languageID);
                            returnValue = true;
                        }
                    }
                    else
                        throw new Exception("ApplyPostAction: ReplaceInPronounOutput: missing input or output.");
                    break;
                case "ReplaceForContracted":
                    if ((action.Input != null) && (action.Output != null))
                    {
                        string input = action.Input.GetIndexedString(languageIndex);
                        string output = action.Output.GetIndexedString(languageIndex);
                        if (!String.IsNullOrEmpty(input))
                        {
                            string keySuffix = TextUtilities.MakeWordsFirstLetterUpperCaseAndCollapse(input);
                            ContractedReplaceAndSet(contractedOutputList, "PronounOutput" + keySuffix, input, output, pronounOutputText, languageID);
                            ContractedReplaceAndSet(contractedOutputList, "Output" + keySuffix, input, output, outputText, languageID);
                            ContractedReplaceAndSet(contractedOutputList, "RegularPronounOutput" + keySuffix, input, output, regularPronounOutputText, languageID);
                            ContractedReplaceAndSet(contractedOutputList, "RegularOutput" + keySuffix, input, output, regularOutputText, languageID);
                            returnValue = true;
                        }
                    }
                    else
                        throw new Exception("ApplyPostAction: ReplaceInPronounOutput: missing input or output.");
                    break;

                default:
                    throw new Exception("Unsupported post action type: " + action.Type);
            }

            return returnValue;
        }

        protected void ContractedReplace(
            List<MultiLanguageString> contractedOutputList,
            string input,
            string output,
            LanguageID languageID)
        {
            foreach (MultiLanguageString mls in contractedOutputList)
            {
                LanguageString ls = mls.LanguageString(languageID);

                if (ls != null)
                    ls.Text = ls.Text.Replace(input, output);
            }
        }

        protected void ContractedReplaceAndSet(
            List<MultiLanguageString> contractedOutputList,
            string key,
            string input,
            string output,
            string text,
            LanguageID languageID)
        {
            string newText = text.Replace(input, output);

            if (newText != text)
            {
                MultiLanguageString mls = contractedOutputList.FirstOrDefault(x => x.KeyString == key);

                if (mls != null)
                    mls.SetText(languageID, newText);
                else
                {
                    mls = new MultiLanguageString(key, languageID, newText);
                    contractedOutputList.Add(mls);
                }
            }
        }

        public virtual bool ExpandSpecialInflector(
            InflectorTable inflectorTable,
            InflectorFamily specialInflector,
            List<Inflector> inflectorsSnapshot)
        {
            List<Inflector> sourceInflectors = JTLanguageModelsPortable.Language.InflectorTable.GetInflectorListWithWildCardsStatic(
                specialInflector.ExpandLabels,
                inflectorsSnapshot);
            bool returnValue = true;

            if (!specialInflector.IsIterate())
                return true;

            List<TokenDescriptor> iterators = inflectorTable.GetIterateTokens(specialInflector.Iterate);

            foreach (Inflector sourceInflector in sourceInflectors)
            {
                foreach (TokenDescriptor iterator in iterators)
                {
                    if (!ExpandIteratedSpeciaInflector(
                            inflectorTable,
                            specialInflector,
                            sourceInflector,
                            iterator))
                        returnValue = false;
                }
            }

            return returnValue;
        }

        public virtual bool ExpandIteratedSpeciaInflector(
            InflectorTable inflectorTable,
            InflectorFamily specialInflector,
            Inflector sourceInflector,
            TokenDescriptor iterator)
        {
            Inflector postInflector = null;
            bool returnValue = true;

            foreach (Designator iteratorDesignator in iterator.Designators)
            {
                Inflector inflector = new Inflector(sourceInflector, null, specialInflector, iteratorDesignator);

                if (specialInflector.HasPattern())
                {
                    postInflector = CreateIteratedInflectorFromPattern(
                        specialInflector.Pattern,
                        sourceInflector,
                        iterator);

                    if (postInflector != null)
                    {
                        inflector.AppendPostInflector(postInflector);
                        inflector.TouchAndClearModified();
                    }
                }

                inflectorTable.AppendInflector(inflector);
                inflectorTable.AppendDesignator(inflector.Designator, "All");
            }

            return returnValue;
        }

        public virtual Inflector CreateIteratedInflectorFromPattern(
            List<WordToken> pattern,
            Inflector sourceInflector,
            TokenDescriptor iterator)
        {
            Inflector inflector = new Inflector();
            bool mainDone = false;

            foreach (WordToken wordToken in pattern)
            {
                switch (wordToken.Type)
                {
                    case "Main":
                        {
#if false
                            Modifier modifier = new Modifier();

                            if (wordToken.OperationCount() != 0)
                            {
                                foreach (Operation operation in wordToken.Operations)
                                {
                                    switch (operation.Operator)
                                    {
                                        case "Prepend":
                                            {
                                                Modifier modifier = new Modifier(
                                                    sourceModifier.Category,
                                                    sourceModifier.Class,
                                                    sourceModifier.SubClass,
                                                    sourceModifier.ModifierType);
                                                SpecialAction action = new SpecialAction();
                                                action.Type = "PrependPrefix";
                                                action.Output = iterator.CloneText();
                                                modifier.AppendAction(action);
                                                inflector.AppendModifier(modifier);
                                            }
                                            break;
                                        case "Append":
                                            {
                                                Modifier modifier = new Modifier(
                                                    sourceModifier.Category,
                                                    sourceModifier.Class,
                                                    sourceModifier.SubClass,
                                                    sourceModifier.ModifierType);
                                                SpecialAction action = new SpecialAction();
                                                action = new SpecialAction();
                                                action.Type = "AppendSuffix";
                                                action.Output = iterator.CloneText();
                                                modifier.AppendAction(action);
                                                inflector.AppendModifier(modifier);
                                            }
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
#else
                            foreach (Modifier sourceModifier in sourceInflector.Modifiers)
                            {
                                if (wordToken.OperationCount() != 0)
                                {
                                    foreach (Operation operation in wordToken.Operations)
                                    {
                                        switch (operation.Operator)
                                        {
                                            case "Prepend":
                                                {
                                                    Modifier modifier = new Modifier(
                                                        sourceModifier.Category,
                                                        sourceModifier.Class,
                                                        sourceModifier.SubClass,
                                                        sourceModifier.ModifierType);
                                                    SpecialAction action = new SpecialAction();
                                                    action.Type = "PrependPrefix";
                                                    action.Output = iterator.CloneText();
                                                    modifier.AppendAction(action);
                                                    inflector.AppendModifier(modifier);
                                                }
                                                break;
                                            case "Append":
                                                {
                                                    Modifier modifier = new Modifier(
                                                        sourceModifier.Category,
                                                        sourceModifier.Class,
                                                        sourceModifier.SubClass,
                                                        sourceModifier.ModifierType);
                                                    SpecialAction action = new SpecialAction();
                                                    action = new SpecialAction();
                                                    action.Type = "AppendSuffix";
                                                    action.Output = iterator.CloneText();
                                                    modifier.AppendAction(action);
                                                    inflector.AppendModifier(modifier);
                                                }
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                }
                            }
#endif
                            mainDone = true;
                        }
                        break;
                    case "Iterator":
                        {
#if false
                            Modifier modifier = new Modifier();

                            if (mainDone)
                                modifier.PostWords = iterator.CloneText();
                            else
                                modifier.PreWords = iterator.CloneText();

                            inflector.AppendModifier(modifier);
#else
                            foreach (Modifier sourceModifier in sourceInflector.Modifiers)
                            {
                                Modifier modifier = new Modifier(
                                    sourceModifier.Category,
                                    sourceModifier.Class,
                                    sourceModifier.SubClass,
                                    sourceModifier.ModifierType);

                                if (mainDone)
                                    modifier.PostWords = iterator.CloneText();
                                else
                                    modifier.PreWords = iterator.CloneText();

                                inflector.AppendModifier(modifier);
                            }
#endif
                        }
                        break;
                    default:
                        break;
                }
            }

            return inflector;
        }

        public virtual bool ExpandCompoundInflector(
            InflectorTable inflectorTable,
            InflectorGroup inflectorGroup,
            CompoundInflector compoundInflector)
        {
            if (compoundInflector.Helper != null)
                return ExpandHelperCompoundInflector(
                    inflectorTable,
                    inflectorGroup,
                    compoundInflector);
            else if (compoundInflector.TargetModifierCount() != 0)
                return ExpandModifierCompoundInflector(
                    inflectorTable,
                    inflectorGroup,
                    compoundInflector);

            throw new Exception("CompoundInflector needs either helper or target modifiers.");
        }

        public virtual bool ExpandHelperCompoundInflector(
            InflectorTable inflectorTable,
            InflectorGroup inflectorGroup,
            CompoundInflector compoundInflector)
        {
            string helper = compoundInflector.Helper;
            int helperSenseIndex = compoundInflector.HelperSenseIndex;
            int helperSynonymIndex = compoundInflector.HelperSynonymIndex;
            string helperDestination = compoundInflector.HelperDestination;
            List<int> helperLanguageIndexMap = compoundInflector.HelperLanguageIndexMap;
            DictionaryEntry helperEntry = null;
            LiteralString sep = null;

            helperEntry = inflectorTable.GetHelperEntry(helper);

            if (helperEntry == null)
                helperEntry = GetDictionaryEntry(helper);
            else
            {
                helperSenseIndex = 0;
                helperSynonymIndex = 0;
            }

            if (helperEntry == null)
                return false;

            if (!SelectDefaultSense(
                    helperEntry,
                    inflectorTable.Category,
                    ref helperSenseIndex,
                    ref helperSynonymIndex))
                return false;

            LexicalCategory category = inflectorTable.Category;
            string targetDesignatorLabel = compoundInflector.TargetDesignatorLabel;
            Inflector targetInflector;

            if (inflectorGroup != null)
                targetInflector = inflectorGroup.GetInflector(targetDesignatorLabel);
            else
                targetInflector = inflectorTable.GetInflector(targetDesignatorLabel);

            if (targetInflector == null)
                return false;

            Designator targetDesignator = targetInflector.Designator;
            List<Designator> simpleDesignators;
            List<Designator> compoundDesignators;

            if (inflectorGroup != null)
            {
                simpleDesignators = compoundInflector.CreateSimpleDesignators(inflectorGroup);
                compoundDesignators = compoundInflector.CreateCompoundDesignators(inflectorGroup);
            }
            else
            {
                simpleDesignators = compoundInflector.CreateSimpleDesignators(inflectorTable);
                compoundDesignators = compoundInflector.CreateCompoundDesignators(inflectorTable);
            }

            int designatorCount = simpleDesignators.Count();
            int designatorIndex;

            for (designatorIndex = 0; designatorIndex < designatorCount; designatorIndex++)
            {
                Designator helperDesignator = simpleDesignators[designatorIndex];
                Designator compoundDesignator = compoundDesignators[designatorIndex];
                Inflector simpleInflector;

                if (inflectorGroup != null)
                {
                    if (inflectorGroup.HasInflector(compoundDesignator.Label))
                        continue;
                }
                else
                {
                    if (inflectorTable.HasInflector(compoundDesignator.Label))
                        continue;
                }

                if (inflectorGroup != null)
                    simpleInflector = inflectorGroup.GetBestInflector(helperDesignator);
                else
                    simpleInflector = inflectorTable.GetBestInflector(helperDesignator);

                Inflector helperInflector = simpleInflector;

                if (helperInflector == null)
                    //throw new Exception("Can't find simple helper inflector for " + helperDesignator.Label);
                    continue;

                //if ((compoundInflector.Label == "Perfect") && (helperDesignator.Label == "Indicative Past Negative Contraction Singular First"))
                //    ApplicationData.Global.PutConsoleMessage(helperDesignator.Label);

                if (compoundInflector.ModifierFixups != null)
                {
                    helperInflector = new Inflector(helperInflector);
                    List<Modifier> helperModifiers = helperInflector.Modifiers;
                    string caseLabel = compoundDesignator.Label;

                    foreach (Modifier helperModifier in helperModifiers)
                    {
                        foreach (ModifierFixup modifierFixup in compoundInflector.ModifierFixups)
                        {
                            if (!modifierFixup.MatchCases(caseLabel))
                                continue;

                            modifierFixup.Modify(helperModifier);
                        }
                    }
                }

                Inflection helperInflection;

                if (!InflectTargetOrHostFiltered(
                        helperEntry,
                        helperSenseIndex,           // senseIndex,
                        helperSynonymIndex,         // synonymIndex,
                        helperInflector,
                        helperDesignator,
                        inflectorTable,
                        "Helper",
                        out helperInflection))
                    //throw new Exception("Error inflecting helper for " + helperDesignator.Label);
                    continue;

                compoundInflector.DoHelperActions(helperInflection);

                MultiLanguageString helperOutput = helperInflection.Output;
                List<LiteralString> pronouns = helperInflector.ClonePronouns();
                List<Modifier> modifiers = targetInflector.CloneModifiers();

                if (helperLanguageIndexMap != null)
                {
                    MultiLanguageString source = helperOutput;
                    helperOutput = new MultiLanguageString(helperOutput);

                    int c = helperLanguageIndexMap.Count();
                    int i;

                    for (i = 0; i < c; i += 2)
                    {
                        int di = helperLanguageIndexMap[i];
                        int si = helperLanguageIndexMap[i + 1];

                        if (si != di)
                            helperOutput.SetText(di, source.Text(si));
                    }
                }

                foreach (Modifier modifier in modifiers)
                {
                    LiteralString helperVerbOutput = new LiteralString(helperOutput, TargetLanguageIDs);

                    switch (helperDestination)
                    {
                        default:
                        case null:
                        case "":
                        case "PreWords":
                            modifier.PreWords = helperVerbOutput;
                            break;
                        case "PostWords":
                            modifier.PostWords = helperVerbOutput;
                            break;
                        case "Prefix":
                            modifier.Prefix = helperVerbOutput;
                            break;
                        case "Suffix":
                            modifier.Suffix = helperVerbOutput;
                            break;
                    }

                    Modifier simpleModifier = helperInflector.GetModifier(modifier.Class, modifier.SubClass);

                    if (simpleModifier != null)
                    {
                        if (simpleModifier.PrePronoun != null)
                        {
                            LiteralString simplePrePronoun = new LiteralString(simpleModifier.PrePronoun);
                            LiteralString prePronoun = modifier.PrePronoun;
                            if (prePronoun != null)
                            {
                                if (sep == null)
                                    sep = new LiteralString(LanguageLookup.GetLanguageWordSeparators(LanguageIDs));
                                prePronoun = new LiteralString(simplePrePronoun, sep, prePronoun);
                            }
                            else
                                prePronoun = simplePrePronoun;
                            modifier.PrePronoun = prePronoun;
                        }
                    }
                }

                if (!compoundDesignator.Label.StartsWith(compoundInflector.Label))
                    compoundDesignator.ReorderFromLabel(compoundInflector.Label);

                Inflector inflector = new Inflector(
                    compoundDesignator.Label,
                    compoundDesignator.CloneClassifications(),
                    compoundInflector.Scope,
                    pronouns,
                    modifiers);

                if (compoundInflector.TargetModifiers != null)
                {
                    List<Modifier> targetModifiers = compoundInflector.CloneTargetModifiers();

                    Inflector postInflector = new Inflector(
                        compoundDesignator.Label,
                        compoundDesignator.CloneClassifications(),
                        "All",
                        null,
                        targetModifiers);

                    postInflector.SetDefaultCategory(category);

                    inflector.AppendPostInflector(postInflector);

                    if (targetInflector.PreInflector != null)
                        inflector.InsertPreInflector(targetInflector.ClonePreInflector());
                }

                if (!String.IsNullOrEmpty(compoundInflector.FilterName))
                    inflector.FilterName = compoundInflector.FilterName;

                if (inflectorGroup != null)
                {
                    inflectorGroup.AppendDesignator(inflector.Designator, compoundInflector.Scope);
                    inflectorGroup.AppendCompoundInflectorList(inflector);
                }
                else
                {
                    inflectorTable.AppendDesignator(inflector.Designator, compoundInflector.Scope);
                    inflectorTable.AppendCompoundInflectorList(inflector);
                }

                if (compoundInflector.ContractionInflectorCount() != 0)
                {
                    foreach (Inflector contractionInflector in compoundInflector.ContractionInflectors)
                    {
                        Inflector newInflector = new Inflector(inflector);
                        newInflector.AppendClassifications(contractionInflector.Designator);
                        Inflector postInflector = new Inflector(contractionInflector);
                        postInflector.SetDefaultCategory(category);
                        newInflector.AppendPostInflector(postInflector);
                        newInflector.DefaultLabel();

                        if (inflectorGroup != null)
                        {
                            inflectorGroup.AppendDesignator(newInflector.Designator, compoundInflector.Scope);
                            inflectorGroup.AppendCompoundInflectorList(newInflector);
                        }
                        else
                        {
                            inflectorTable.AppendDesignator(newInflector.Designator, compoundInflector.Scope);
                            inflectorTable.AppendCompoundInflectorList(newInflector);
                        }
                    }
                }

                if (compoundInflector.PostActionCount() != 0)
                {
                    foreach (SpecialAction action in compoundInflector.PostActions)
                    {
                        if (!action.MatchCases(inflector.Label))
                            continue;

                        SpecialAction newAction = new SpecialAction(action);
                        inflector.AppendPostAction(newAction);
                    }
                }
            }

            return true;
        }

        public virtual bool ExpandModifierCompoundInflector(
            InflectorTable inflectorTable,
            InflectorGroup inflectorGroup,
            CompoundInflector compoundInflector)
        {
            if (compoundInflector.TargetModifiers == null)
                return false;

            LexicalCategory category = inflectorTable.Category;
            List<Designator> simpleDesignators;
            List<Designator> compoundDesignators;

            if (inflectorGroup != null)
            {
                simpleDesignators = compoundInflector.CreateSimpleDesignators(inflectorGroup);
                compoundDesignators = compoundInflector.CreateCompoundDesignators(inflectorGroup);
            }
            else
            {
                simpleDesignators = compoundInflector.CreateSimpleDesignators(inflectorTable);
                compoundDesignators = compoundInflector.CreateCompoundDesignators(inflectorTable);
            }

            int designatorCount = simpleDesignators.Count();
            int designatorIndex;

            for (designatorIndex = 0; designatorIndex < designatorCount; designatorIndex++)
            {
                Designator simpleDesignator = simpleDesignators[designatorIndex];
                Designator compoundDesignator = compoundDesignators[designatorIndex];
                Inflector simpleInflector;

                if (inflectorGroup != null)
                    simpleInflector = inflectorGroup.GetBestInflector(simpleDesignator);
                else
                    simpleInflector = inflectorTable.GetBestInflector(simpleDesignator);

                Inflector targetInflector = simpleInflector;

                if (targetInflector == null)
                    //throw new Exception("Can't find simple target inflector for " + compoundInflector.Name);
                    continue;

                if (compoundInflector.ModifierFixups != null)
                {
                    targetInflector = new Inflector(targetInflector);
                    List<Modifier> fixupModifiers = targetInflector.Modifiers;
                    string caseLabel = compoundDesignator.Label;

                    foreach (Modifier fixupModifier in fixupModifiers)
                    {
                        foreach (ModifierFixup modifierFixup in compoundInflector.ModifierFixups)
                        {
                            if (!modifierFixup.MatchCases(caseLabel))
                                continue;

                            modifierFixup.Modify(fixupModifier);
                        }
                    }
                }

                if (!compoundDesignator.Label.StartsWith(compoundInflector.Label))
                    compoundDesignator.ReorderFromLabel(compoundInflector.Label);

                List<Modifier> targetModifiers = compoundInflector.CloneTargetModifiers();

                Inflector inflector = new Inflector(
                    compoundDesignator.Label,
                    compoundDesignator.CloneClassifications(),
                    "All",
                    targetInflector.ClonePronouns(),
                    targetInflector.CloneModifiers());

                Inflector preInflector = new Inflector(
                    compoundDesignator.Label,
                    compoundDesignator.CloneClassifications(),
                    "All",
                    null,
                    targetModifiers);

                preInflector.SetDefaultCategory(category);

                inflector.InsertPreInflector(preInflector);

                if (targetInflector.PostInflector != null)
                    inflector.AppendPostInflector(targetInflector.ClonePostInflector());

                if (!String.IsNullOrEmpty(compoundInflector.FilterName))
                    inflector.FilterName = compoundInflector.FilterName;

                if (inflectorGroup != null)
                {
                    inflectorGroup.AppendDesignator(inflector.Designator, "Both");
                    inflectorGroup.AppendCompoundInflectorList(inflector);
                }
                else
                {
                    inflectorTable.AppendDesignator(inflector.Designator, "Both");
                    inflectorTable.AppendCompoundInflectorList(inflector);
                }

                if (compoundInflector.ContractionInflectorCount() != 0)
                {
                    foreach (Inflector contractionInflector in compoundInflector.ContractionInflectors)
                    {
                        Inflector newInflector = new Inflector(inflector);
                        newInflector.AppendClassifications(contractionInflector.Designator);
                        Inflector postInflector = new Inflector(contractionInflector);
                        postInflector.SetDefaultCategory(category);
                        newInflector.AppendPostInflector(postInflector);
                        newInflector.DefaultLabel();

                        if (inflectorGroup != null)
                        {
                            inflectorGroup.AppendDesignator(newInflector.Designator, "Both");
                            inflectorGroup.AppendCompoundInflectorList(newInflector);
                        }
                        else
                        {
                            inflectorTable.AppendDesignator(newInflector.Designator, "Both");
                            inflectorTable.AppendCompoundInflectorList(newInflector);
                        }
                    }
                }
            }

            return true;
        }

        public virtual Designator GetCanonicalDesignation(
            Designator designation,
            LanguageTool toLanguageTool)
        {
            return null;
        }

        // Handle adding things like reflexive, direct pronouns, indirect pronouns, and other suffixes and prefixes
        // not normally handle in inflecting.
        public virtual bool ExtendInflection(Inflection inflection)
        {
            return false;
        }

        public virtual Inflection GetInflectionCached(
            string inflectedForm,
            string dictionaryForm,
            LexicalCategory category,
            Designator designator,
            int designatorHash)
        {
            Inflection inflection;
            string inflectionKey = inflectedForm + designatorHash.ToString();

            if (InflectionCache.TryGetValue(inflectionKey, out inflection))
                return inflection;

            inflection = GetInflectionNoCache(
                inflectedForm,
                dictionaryForm,
                category,
                designator);

            InflectionCache.Add(inflectionKey, inflection);

            return inflection;
        }

        public virtual Inflection GetInflectionNoCache(
            string inflectedForm,
            string dictionaryForm,
            LexicalCategory category,
            Designator designator)
        {
            Inflection inflection;
            DictionaryEntry dictionaryEntry = GetDictionaryEntry(dictionaryForm);
            int senseIndex = -1;
            int synonymIndex = -1;

            if (dictionaryEntry == null)
                return null;

            if (!InflectAnyDictionaryFormDesignated(
                    dictionaryEntry,
                    ref senseIndex,
                    ref synonymIndex,
                    designator,
                    out inflection))
                return null;

            return inflection;
        }

        protected Dictionary<string, Inflection> _InflectionCache;
        public Dictionary<string, Inflection> InflectionCache
        {
            get
            {
                if (_InflectionCache == null)
                    _InflectionCache = new Dictionary<string, Inflection>();

                return _InflectionCache;
            }
            set
            {
                _InflectionCache = value;
            }
        }
    }
}
