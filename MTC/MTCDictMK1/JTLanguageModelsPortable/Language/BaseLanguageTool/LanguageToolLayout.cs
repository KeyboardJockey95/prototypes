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
    public partial class LanguageTool
    {
        //public static string InflectionsLayoutBreakpoint = "Progressive Perfect Indicative Present Positive";
        //public static string InflectionsLayoutBreakpoint = "Perfect Progressive Indicative Present Positive";
        public static string InflectionsLayoutBreakpoint = null;

        public virtual InflectionsLayout GetAutomatedInflectionsLayout(
            string type,
            string layout,
            LanguageUtilities languageUtilities)
        {
            InflectionsLayout inflectionsLayout = null;

            if (layout != "Automated")
                return inflectionsLayout;

            string key = "InflectionsLayout" + type + "s" + layout + LanguageName;

            inflectionsLayout = new InflectionsLayout(
                key,
                LanguageIDs,
                type,
                layout,
                AutomaticUsePronouns());

            InitializeInflectionsLayout(
                type,
                layout,
                inflectionsLayout,
                languageUtilities);

            switch (type)
            {
                case "Verb":
                    InitializeAutomatedInflectionsLayoutVerb(
                        type,
                        layout,
                        inflectionsLayout);
                    break;
                default:
                    break;
            }

            return inflectionsLayout;
        }

        public virtual bool InitializeInflectionsLayout(
            string type,
            string layout,
            InflectionsLayout inflectionsLayout,
            LanguageUtilities languageUtilities)
        {
            LexicalCategory category = Sense.GetLexicalCategoryFromString(type);
            List<Designator> designators = GetAllCategoryDesignations(category);
            List<BaseString> classifierKeys = new List<BaseString>();
            List<BaseString> classifierValues = new List<BaseString>();
            Dictionary<string, List<BaseString>> classifierKeyValueDictionary = new Dictionary<string, List<BaseString>>();

            bool returnValue = GetClassifierKeysAndValues(
                designators,
                classifierKeys,
                classifierValues,
                classifierKeyValueDictionary,
                true,
                languageUtilities);

            inflectionsLayout.Designators = designators;
            inflectionsLayout.ClassifierKeys = classifierKeys;
            inflectionsLayout.ClassifierValues = classifierValues;
            inflectionsLayout.ClassifierKeyValueDictionary = classifierKeyValueDictionary;

            return returnValue;
        }

        public virtual bool GetClassifierKeysAndValues(
            List<Designator> designators,
            List<BaseString> classifierKeys,
            List<BaseString> classifierValues,
            Dictionary<string, List<BaseString>> classifierKeyValueDictionary,
            bool sortClassifiers,
            LanguageUtilities languageUtilities)
        {
            bool returnValue = true;

            if (designators == null)
                return false;

            foreach (Designator designator in designators)
            {
                foreach (Classifier classifier in designator.Classifications)
                {
                    string key = classifier.KeyString;
                    string value = classifier.Text;

                    BaseString classifierKeyString = classifierKeys.FirstOrDefault(x => x.KeyString == key);

                    if (classifierKeyString == null)
                    {
                        string translatedKey = (languageUtilities != null ? languageUtilities.TranslateUIString(key) : key);
                        classifierKeyString = new BaseString(key, translatedKey);
                        classifierKeys.Add(classifierKeyString);
                    }

                    BaseString classifierValueString = classifierValues.FirstOrDefault(x => x.KeyString == value);

                    if (classifierValueString == null)
                    {
                        string translatedValue = (languageUtilities != null ? languageUtilities.TranslateUIString(value) : value);
                        classifierValueString = new BaseString(value, translatedValue);
                        classifierValues.Add(classifierValueString);
                    }

                    string valueKey = value;
                    List<BaseString> values;
                    BaseString valueItem;

                    if (classifierKeyValueDictionary.TryGetValue(key, out values))
                    {
                        valueItem = values.FirstOrDefault(x => x.KeyString == valueKey);

                        if (valueItem == null)
                        {
                            string translatedValue = (languageUtilities != null ? languageUtilities.TranslateUIString(value) : value);
                            valueItem = new BaseString(valueKey, translatedValue);
                            values.Add(valueItem);
                        }
                    }
                    else
                    {
                        string translatedValue = (languageUtilities != null ? languageUtilities.TranslateUIString(value) : value);
                        valueItem = new BaseString(valueKey, translatedValue);
                        values = new List<BaseString>() { valueItem };
                        classifierKeyValueDictionary.Add(key, values);
                    }
                }
            }

            if (sortClassifiers)
                classifierKeys.Sort(new BaseStringValueComparer());

            return returnValue;
        }

        public virtual List<string> AutomaticRowKeys(string type)
        {
            InflectorTable inflectorTable = InflectorTable(type);
            List<string> rowKeys = null;

            if (inflectorTable != null)
                rowKeys = inflectorTable.AutomaticRowKeys;

            if ((rowKeys == null) || (rowKeys.Count() == 0))
            {
                rowKeys = new List<string>();

                switch (type)
                {
                    case "Verb":
                        rowKeys.Add("Number");
                        break;
                    case "Noun":
                        rowKeys.Add("Number");
                        break;
                    case "Adjective":
                        rowKeys.Add("Number");
                        break;
                    default:
                        break;
                }
            }

            return rowKeys;
        }

        public virtual List<string> AutomaticColumnKeys(string type)
        {
            InflectorTable inflectorTable = InflectorTable(type);
            List<string> columnKeys = null;

            if (inflectorTable != null)
                columnKeys = inflectorTable.AutomaticColumnKeys;

            if ((columnKeys == null) || (columnKeys.Count() == 0))
            {
                columnKeys = new List<string>();

                switch (type)
                {
                    case "Verb":
                        columnKeys.Add("Person");
                        columnKeys.Add("Politeness");
                        columnKeys.Add("Gender");
                        break;
                    case "Noun":
                        columnKeys.Add("Gender");
                        break;
                    case "Adjective":
                        columnKeys.Add("Gender");
                        break;
                    default:
                        break;
                }
            }

            return columnKeys;
        }

        public virtual bool AutomaticUsePronouns()
        {
            return true;
        }

        public virtual List<InflectionsLayoutGroup> MajorGroups(string type)
        {
            InflectorTable inflectorTable = InflectorTable(type);
            List<InflectionsLayoutGroup> majorGroups = null;

            if (inflectorTable != null)
                majorGroups = inflectorTable.CloneMajorGroups();

            return majorGroups;
        }

        public virtual void InitializeAutomatedInflectionsLayoutVerb(
            string type,
            string layout,
            InflectionsLayout inflectionsLayout)
        {
            List<InflectionsLayoutGroup> majorGroups = MajorGroups(type);
            List<InflectionsLayoutGroup> inflectionGroups = new List<InflectionsLayoutGroup>();
            List<string> rowKeys = AutomaticRowKeys(type);
            List<Classifier> rowClassifiers = new List<Classifier>();
            List<string> columnKeys = AutomaticColumnKeys(type);
            List<Classifier> columnClassifiers = new List<Classifier>();
            List<string> cellKeys = new List<string>();
            string groupKey;
            string lastGroupKey = String.Empty;
            string groupTitle;
            string groupHeading;
            string rowKey;
            string columnKey;
            InflectionsLayoutGroup majorGroup = null;
            InflectionsLayoutGroup layoutSubGroup = null;
            InflectionsLayoutGroup group = null;

            if (inflectionsLayout == null)
                return;

            if (inflectionsLayout.Designators == null)
                return;

            List<Designator> designators = inflectionsLayout.Designators;
            Dictionary<string, List<BaseString>> classifierKeyValueDictionary =
                inflectionsLayout.ClassifierKeyValueDictionary;

            if (majorGroups != null)
                inflectionsLayout.Groups = majorGroups;
            else
                inflectionsLayout.Groups = inflectionGroups;

            foreach (Designator designator in designators)
            {
                Designator groupDesignation = new Designator();

                groupKey = String.Empty;
                groupTitle = String.Empty;
                rowKey = String.Empty;
                columnKey = String.Empty;

                foreach (Classifier classifier in designator.Classifications)
                {
                    if (columnKeys.Contains(classifier.KeyString))
                    {
                        if (!String.IsNullOrEmpty(columnKey))
                            columnKey += " ";

                        columnKey += classifier.Text;
                    }
                    else if (rowKeys.Contains(classifier.KeyString))
                    {
                        if (!String.IsNullOrEmpty(rowKey))
                            rowKey += " ";

                        rowKey += classifier.Text;
                    }
                    else if ((classifier.KeyString == "Alternate") || (classifier.KeyString == "Contraction"))
                    {
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(groupKey))
                            groupKey += " ";

                        groupKey += classifier.Text;

                        if (!String.IsNullOrEmpty(groupTitle))
                            groupTitle += " ";

                        groupTitle += classifier.Text;

                        groupDesignation.CopyAndAppendClassification(classifier);
                    }
                }

                groupDesignation.DefaultLabel();

                if ((InflectionsLayoutBreakpoint != null) &&
                        (groupKey == InflectionsLayoutBreakpoint))
                    ApplicationData.Global.PutConsoleMessage(groupKey);

                if (groupKey != lastGroupKey)
                {
                    majorGroup = FindOwningGroup(majorGroups, groupKey);
                    groupHeading = groupTitle;

                    if ((majorGroup != null) && (majorGroup.KeyString == groupKey))
                    {
                        group = inflectionGroups.FirstOrDefault(x => x.KeyString == groupKey);

                        if (group == null)
                        {
                            group = new InflectionsLayoutGroup(groupKey, groupDesignation);
                            inflectionGroups.Add(group);

                            group.AppendHeading(
                                new InflectionsLayoutHeading(
                                    groupHeading,
                                    1,
                                    3));

                            if (!String.IsNullOrEmpty(rowKey))
                                AddGroupRows(
                                    new List<Classifier>(),
                                    0,
                                    rowKeys,
                                    columnKeys,
                                    group,
                                    classifierKeyValueDictionary);
                            else
                                AddGroupRows(
                                    new List<Classifier>(),
                                    0,
                                    null,
                                    columnKeys,
                                    group,
                                    classifierKeyValueDictionary);

                            majorGroup.AppendSubGroup(group);
                        }
                    }
                    else
                    {
                        group = inflectionGroups.FirstOrDefault(x => x.KeyString == groupKey);

                        if (group == null)
                        {
                            group = new InflectionsLayoutGroup(groupKey, groupDesignation);
                            inflectionGroups.Add(group);

                            if (majorGroup != null)
                            {
                                layoutSubGroup = majorGroup.GetSubGroup(groupKey);

                                if (layoutSubGroup != null)
                                    groupHeading = layoutSubGroup.Label;
                            }
                            else
                                layoutSubGroup = null;

                            group.AppendHeading(
                                new InflectionsLayoutHeading(
                                    groupHeading,
                                    1,
                                    3));

                            if (!String.IsNullOrEmpty(rowKey))
                                AddGroupRows(
                                    new List<Classifier>(),
                                    0,
                                    rowKeys,
                                    columnKeys,
                                    group,
                                    classifierKeyValueDictionary);
                            else
                                AddGroupRows(
                                    new List<Classifier>(),
                                    0,
                                    null,
                                    columnKeys,
                                    group,
                                    classifierKeyValueDictionary);
                        }

                        if (majorGroup != null)
                        {
                            if (layoutSubGroup != null)
                                majorGroup.DeleteSubGroup(layoutSubGroup);

                            majorGroup.AppendSubGroup(group);
                        }
                    }
                }

                AddGroupCell(group, designator, columnKeys);

                lastGroupKey = groupKey;
            }

            FixupGroups(majorGroups, inflectionGroups, rowKeys, columnKeys);
        }

        protected InflectionsLayoutGroup FindOwningGroup(
            List<InflectionsLayoutGroup> majorGroups,
            string groupKey)
        {
            InflectionsLayoutGroup bestGroup = null;

            if (majorGroups == null)
                return null;

            foreach (InflectionsLayoutGroup group in majorGroups)
            {
                if (!string.IsNullOrEmpty(group.KeyString) && groupKey.StartsWith(group.KeyString))
                {
                    if (bestGroup != null)
                    {
                        if (group.Label.Length > bestGroup.Label.Length)
                            bestGroup = group;
                    }
                    else
                        bestGroup = group;
                }

                /*
                if (!string.IsNullOrEmpty(group.Label) && groupKey.StartsWith(group.Label))
                {
                    if (bestGroup != null)
                    {
                        if (group.Label.Length > bestGroup.Label.Length)
                            bestGroup = group;
                    }
                    else
                        bestGroup = group;
                }
                */

                if (group.SubGroupCount() != 0)
                {
                    foreach (InflectionsLayoutGroup subGroup in group.SubGroups)
                    {
                        if (!String.IsNullOrEmpty(subGroup.KeyString) && groupKey.StartsWith(subGroup.KeyString))
                        {
                            if (bestGroup != null)
                            {
                                if (group.Label.Length > bestGroup.Label.Length)
                                    bestGroup = group;
                            }
                            else
                                bestGroup = group;
                        }

                        /*
                        if (!String.IsNullOrEmpty(subGroup.Label) && groupKey.StartsWith(subGroup.Label))
                        {
                            if (bestGroup != null)
                            {
                                if (group.Label.Length > bestGroup.Label.Length)
                                    bestGroup = group;
                            }
                            else
                                bestGroup = group;
                        }
                        */
                    }
                }
            }

            return bestGroup;
        }

        protected void AddGroupRows(
            List<Classifier> baseClassifiers,
            int keyIndex,
            List<string> rowKeys,
            List<string> columnKeys,
            InflectionsLayoutGroup group,
            Dictionary<string, List<BaseString>> classifierKeyValueDictionary)
        {
            string key;
            int keyCount = (rowKeys != null ? rowKeys.Count() : 0);
            int valueCount;
            int valueIndex;
            List<BaseString> keyValuesSource;
            List<BaseString> keyValues;
            BaseString keyValue;
            int nextKeyIndex = keyIndex + 1;
            InflectionsLayoutRow row;

            if (rowKeys == null)
            {
                row = new InflectionsLayoutRow();
                group.AddRow(row);
                AddRowColumns(
                    new List<Classifier>(),
                    0,
                    row,
                    columnKeys,
                    classifierKeyValueDictionary);
                return;
            }

            key = rowKeys[keyIndex];

            if (!classifierKeyValueDictionary.TryGetValue(key, out keyValuesSource))
                return;

            keyValues = new List<BaseString>(keyValuesSource);
            keyValues.Add(new BaseString("None", "None"));
            valueCount = keyValues.Count();

            for (valueIndex = 0; valueIndex < valueCount; valueIndex++)
            {
                keyValue = keyValues[valueIndex];
                string value = keyValue.KeyString;

                List<Classifier> classifiers = new List<Classifier>(baseClassifiers);

                Classifier classifier = new Classifier(key, value);
                classifiers.Add(classifier);

                if (nextKeyIndex < keyCount)
                    AddGroupRows(
                        classifiers,
                        nextKeyIndex,
                        rowKeys,
                        columnKeys,
                        group,
                        classifierKeyValueDictionary);
                else
                {
                    Designator designation = new Designator(null, classifiers);
                    row = new InflectionsLayoutRow(designation);
                    group.AddRow(row);
                    AddRowColumns(
                        new List<Classifier>(),
                        0,
                        row,
                        columnKeys,
                        classifierKeyValueDictionary);
                }
            }
        }

        protected void AddRowColumns(
            List<Classifier> baseClassifiers,
            int keyIndex,
            InflectionsLayoutRow row,
            List<string> columnKeys,
            Dictionary<string, List<BaseString>> classifierKeyValueDictionary)
        {
            string key;
            int keyCount = (columnKeys != null ? columnKeys.Count() : 0);
            int valueCount;
            int valueIndex;
            List<BaseString> keyValuesSource;
            List<BaseString> keyValues;
            BaseString keyValue;
            int nextKeyIndex = keyIndex + 1;
            InflectionsLayoutColumn column;

            if (columnKeys == null)
            {
                column = new InflectionsLayoutColumn();
                row.AddColumn(column);
                return;
            }

            key = columnKeys[keyIndex];

            if (!classifierKeyValueDictionary.TryGetValue(key, out keyValuesSource))
                keyValues = new List<BaseString>();
            else
                keyValues = new List<BaseString>(keyValuesSource);

            keyValues.Add(new BaseString("None", "None"));
            valueCount = keyValues.Count();

            for (valueIndex = 0; valueIndex < valueCount; valueIndex++)
            {
                keyValue = keyValues[valueIndex];
                string value = keyValue.KeyString;

                List<Classifier> classifiers = new List<Classifier>(baseClassifiers);

                Classifier classifier = new Classifier(key, value);
                classifiers.Add(classifier);

                if (nextKeyIndex < keyCount)
                    AddRowColumns(
                        classifiers,
                        nextKeyIndex,
                        row,
                        columnKeys,
                        classifierKeyValueDictionary);
                else
                {
                    Designator designation = new Designator(null, classifiers);
                    column = new InflectionsLayoutColumn(designation);
                    row.AddColumn(column);
                }
            }
        }

        protected void FixupGroups(
            List<InflectionsLayoutGroup> majorGroups,
            List<InflectionsLayoutGroup> inflectionGroups,
            List<string> rowKeys,
            List<string> columnKeys)
        {
            foreach (InflectionsLayoutGroup inflectionGroup in inflectionGroups)
            {
                InflectionsLayoutRow columnHeadingsRow = new InflectionsLayoutRow("ColumnHeadings");
                ArrangeColumns(inflectionGroup, columnHeadingsRow, columnKeys);
                SetupRowHeadings(inflectionGroup, rowKeys);
                SetupColumnHeadings(inflectionGroup, columnHeadingsRow, columnKeys);
            }

            // Must come after ArrangeColumns so that the rows are in place.
            if (majorGroups != null)
            {
                foreach (InflectionsLayoutGroup majorGroup in majorGroups)
                    SetupMajorHeading(majorGroup);
            }
        }

        protected void ArrangeColumns(
            InflectionsLayoutGroup group,
            InflectionsLayoutRow columnHeadingsRow,
            List<string> columnKeys)
        {
            int columnCount = group.GetRowIndexed(0).ColumnCount();
            int columnIndex;
            int rowCount = group.RowCount();
            int rowIndex;

            for (rowIndex = rowCount - 1; rowIndex >= 0; rowIndex--)
            {
                InflectionsLayoutRow row = group.GetRowIndexed(rowIndex);
                bool haveCell = false;

                for (columnIndex = 0; columnIndex < columnCount; columnIndex++)
                {
                    InflectionsLayoutColumn column = row.GetColumnIndexed(columnIndex);

                    if (column.Cell != null)
                    {
                        haveCell = true;
                        break;
                    }
                }

                if (!haveCell)
                    group.Rows.RemoveAt(rowIndex);
            }

            rowCount = group.RowCount();

            // Remove empty columns.
            for (columnIndex = columnCount - 1; columnIndex >= 0; columnIndex--)
            {
                bool haveCell = false;

                for (rowIndex = 0; rowIndex < rowCount; rowIndex++)
                {
                    InflectionsLayoutColumn column = group.Rows[rowIndex].Columns[columnIndex];

                    if (column.Cell != null)
                    {
                        haveCell = true;
                        break;
                    }
                }

                if (!haveCell)
                {
                    for (rowIndex = 0; rowIndex < rowCount; rowIndex++)
                        group.Rows[rowIndex].Columns.RemoveAt(columnIndex);
                }
                else
                {
                    InflectionsLayoutRow row = group.GetRowIndexed(0);
                    InflectionsLayoutColumn column = row.GetColumnIndexed(columnIndex);
                    Designator columnDesignation = column.Designation;
                    List<Classifier> classifications = new List<Classifier>();
                    List<InflectionsLayoutHeading> headings = new List<InflectionsLayoutHeading>();

                    foreach (string columnKey in columnKeys)
                    {
                        Classifier classification = columnDesignation.GetClassification(columnKey);
                        string headingText;

                        if (classification != null)
                        {
                            headingText = classification.Text;
                            classifications.Add(new Classifier(classification));
                        }
                        else
                            headingText = String.Empty;

                        InflectionsLayoutHeading heading = new InflectionsLayoutHeading(
                            headingText,
                            1,
                            1);

                        headings.Add(heading);
                    }

                    Designator columnHeadingDesignation = new Designator(null, classifications);
                    InflectionsLayoutCell headingCell = new InflectionsLayoutCell(
                        columnHeadingDesignation,
                        1,
                        1);
                    InflectionsLayoutColumn columnHeadingColumn = new InflectionsLayoutColumn(
                        columnHeadingDesignation);
                    columnHeadingColumn.Cell = headingCell;
                    headingCell.Headings = headings;
                    columnHeadingsRow.InsertCell(0, headingCell);
                    columnHeadingsRow.InsertColumn(0, columnHeadingColumn);
                }
            }

            columnCount = group.GetRowIndexed(0).ColumnCount();

            if ((columnCount > 1) || (rowCount > 1))
            {
                Designator familyDesignator = GetFamilyDesignator(group.GetRowIndexed(0).Columns[columnCount - 1].Designation);
                int familyStartIndex = -1;
                int familyEndIndex = columnCount;

                // Merge columns.
                for (columnIndex = columnCount - 1; columnIndex >= 0; columnIndex--)
                {
                    InflectionsLayoutColumn column = column = group.Rows[0].Columns[columnIndex];

                    if ((columnIndex != 0) && DesignatorMatch(column.Designation, familyDesignator))
                        continue;

                    if (columnIndex == 0)
                        familyStartIndex = 0;
                    else
                        familyStartIndex = columnIndex + 1;

                    if ((familyEndIndex - familyStartIndex) > 1)
                    {
                        int firstNotNoneIndex = -1;
                        bool hasIncompleteColumn = false;
                        int noneRowIndex = -1;
                        int noneColumnIndex = -1;
                        InflectionsLayoutColumn noneColumn = null;

                        for (int index = familyEndIndex - 1; index >= familyStartIndex; index--)
                        {
                            int cellCount = 0;

                            for (rowIndex = 0; rowIndex < rowCount; rowIndex++)
                            {
                                if (group.Rows[rowIndex].Columns[index].Cell != null)
                                    cellCount++;

                                if (group.Rows[rowIndex].Columns[index].Designation.Classifications.Last().Text == "None")
                                {
                                    if (group.Rows[rowIndex].Columns[index].Cell != null)
                                    {
                                        noneColumnIndex = index;
                                        noneRowIndex = rowIndex;
                                        noneColumn = group.Rows[rowIndex].Columns[index];
                                    }
                                }
                                else
                                    firstNotNoneIndex = index;
                            }

                            if (cellCount != rowCount)
                                hasIncompleteColumn = true;
                        }

                        if (hasIncompleteColumn && (noneColumnIndex != -1) && (firstNotNoneIndex != -1))
                        {
                            InflectionsLayoutColumn newNoneColumn = new InflectionsLayoutColumn(noneColumn);
                            newNoneColumn.Cell.ColumnSpan = (familyEndIndex - familyStartIndex) - 1;
                            group.Rows[noneRowIndex].Columns[firstNotNoneIndex] = newNoneColumn;

                            columnHeadingsRow.Columns.RemoveAt(noneColumnIndex);
                            columnHeadingsRow.Cells.RemoveAt(noneColumnIndex);

                            for (rowIndex = 0; rowIndex < rowCount; rowIndex++)
                            {
                                InflectionsLayoutRow row = group.Rows[rowIndex];

                                if (rowIndex == noneRowIndex)
                                {
                                    for (int index = familyEndIndex - 1; index >= familyStartIndex; index--)
                                    {
                                        if (index != firstNotNoneIndex)
                                            row.Columns.RemoveAt(index);
                                    }
                                }
                                else
                                {
                                    for (int index = familyEndIndex - 1; index >= familyStartIndex; index--)
                                    {
                                        if (index == noneColumnIndex)
                                            row.Columns.RemoveAt(index);
                                    }
                                }
                            }
                        }
                    }

                    if (column != null)
                        familyDesignator = GetFamilyDesignator(column.Designation);

                    familyEndIndex = familyStartIndex;
                }
            }

            rowCount = group.RowCount();

            // Copy cells to cells list.
            for (rowIndex = 0; rowIndex < rowCount; rowIndex++)
            {
                InflectionsLayoutRow row = group.GetRowIndexed(rowIndex);

                columnCount = row.ColumnCount();

                for (columnIndex = 0; columnIndex < columnCount; columnIndex++)
                {
                    InflectionsLayoutColumn column = row.GetColumnIndexed(columnIndex);
                    InflectionsLayoutCell cell = column.Cell;

                    if (cell == null)
                        cell = new InflectionsLayoutCell("None");

                    row.AppendCell(cell);
                }
            }

            //if (group.Label == "Indicative Present Simple Positive")
            //    group.Display(null, DisplayDetial.Full, 0);
            //else if (group.Label == "Indicative Positive")
            //    group.Display(null, DisplayDetial.Full, 0);
        }

        protected Designator GetFamilyDesignator(Designator designator)
        {
            List<Classifier> familyClassifiers = new List<Classifier>(designator.Classifications);
            familyClassifiers.RemoveAt(familyClassifiers.Count() - 1);
            Designator familyDesignator = new Designator(null, familyClassifiers);
            return familyDesignator;
        }

        protected void AddGroupCell(
            InflectionsLayoutGroup group,
            Designator designator,
            List<string> columnKeys)
        {
            foreach (InflectionsLayoutRow row in group.Rows)
            {
                if (row.Designation != null)
                {
                    if (DesignatorMatch(designator, row.Designation))
                    {
                        foreach (InflectionsLayoutColumn column in row.Columns)
                        {
                            if (DesignatorMatch(designator, column.Designation))
                            {
                                Designator filteredDesignator = FilterDesignator(designator);
                                column.Cell = new InflectionsLayoutCell(filteredDesignator, 1, 1);
                                return;
                            }
                        }
                    }
                }
            }

            // The cell didn't match any columns, so we add a column.
            foreach (InflectionsLayoutRow row in group.Rows)
            {
                List<Classifier> classifiers = new List<Classifier>();
                foreach (string columnKey in columnKeys)
                    classifiers.Add(new Classifier(columnKey, "None"));
                Designator designation = new Designator(null, classifiers);
                InflectionsLayoutColumn column = new InflectionsLayoutColumn(designation);
                InflectionsLayoutCell cell = new InflectionsLayoutCell(designation, 1, 1);
                column.Cell = cell;
                row.AddColumn(column);
            }
        }

        protected Designator FilterDesignator(Designator designator)
        {
            if (designator == null)
                return null;

            List<Classifier> classifiers = new List<Classifier>();
            
            if (designator.ClassificationCount() != 0)
            {
                foreach (Classifier classifier in designator.Classifications)
                {
                    if ((classifier.KeyString == "Alternate") || (classifier.KeyString == "Contraction"))
                        continue;

                    classifiers.Add(new Classifier(classifier));
                }
            }

            return new Designator(null, classifiers);
        }

        protected bool InflectionMatch(Inflection inflection, Designator designator)
        {
            if (designator == null)
                return true;

            if (designator.Classifications == null)
                return true;

            foreach (Classifier classifier in designator.Classifications)
            {
                if (classifier.Text == "None")
                {
                    Classifier testClassifier = inflection.Designation.GetClassification(classifier.Name);

                    if ((testClassifier != null) && (testClassifier.Text != "None"))
                        return false;
                }
                else
                {
                    if (!inflection.Designation.HasClassificationWith(classifier.Name, classifier.Text))
                        return false;
                }
            }

            return true;
        }

        protected bool DesignatorMatch(Designator designator1, Designator designator2)
        {
            if ((designator1 == null) || (designator2 == null))
                return true;

            if (designator1.Classifications == null)
                return true;

            if (designator2.Classifications == null)
                return true;

            foreach (Classifier classifier in designator2.Classifications)
            {
                if (classifier.Text == "None")
                {
                    Classifier testClassifier = designator1.GetClassification(classifier.Name);

                    if ((testClassifier != null) && (testClassifier.Text != "None"))
                        return false;
                }
                else
                {
                    if (!designator1.HasClassificationWith(classifier.Name, classifier.Text))
                        return false;
                }
            }

            return true;
        }

        protected void SetupMajorHeading(InflectionsLayoutGroup majorGroup)
        {
            if ((majorGroup.SubGroupCount() > 1) && (majorGroup.HeadingsCount() == 0))
            {
                string headingText = majorGroup.Label;
                int rowSpan = 0;
                int columnSpan = 1;

                if (majorGroup.SubGroupCount() != 0)
                {
                    InflectionsLayoutGroup firstSubGroup = majorGroup.GetSubGroupIndexed(0);

                    if (firstSubGroup.RowCount() != 0)
                    {
                        InflectionsLayoutRow firstRow = firstSubGroup.GetRowIndexed(0);

                        if (firstRow.CellCount() > columnSpan)
                            columnSpan = firstRow.CellCount();
                    }
                }

                InflectionsLayoutHeading heading = new InflectionsLayoutHeading(
                    headingText,
                    rowSpan,
                    columnSpan);

                majorGroup.AppendHeading(heading);
            }
        }

        protected void SetupRowHeadings(
            InflectionsLayoutGroup group,
            List<string> rowKeys)
        {
            foreach (InflectionsLayoutRow row in group.Rows)
            {
                if (row.Name != "ColumnHeadings")
                {
                    List<Classifier> classifications = new List<Classifier>();
                    string headingText = String.Empty;

                    foreach (string rowKey in rowKeys)
                    {
                        Classifier classification = row.GetClassification(rowKey);

                        if (classification != null)
                            classifications.Add(new Classifier(classification));
                    }

                    if (classifications.Count() != 0)
                    {
                        Designator designation = new Designator(null, classifications);
                        InflectionsLayoutHeading heading = new InflectionsLayoutHeading(
                            designation.Label,
                            1,
                            1);
                        List<InflectionsLayoutHeading> headings = new List<InflectionsLayoutHeading>() { heading };
                        InflectionsLayoutCell cell = new InflectionsLayoutCell(
                            designation,
                            1,
                            1);

                        cell.Headings = headings;
                        row.InsertCell(0, cell);
                    }
                }
            }
        }

        protected void SetupColumnHeadings(
            InflectionsLayoutGroup group,
            InflectionsLayoutRow columnHeadingsRow,
            List<string> columnKeys)
        {
            if (group.GetRow("ColumnHeadings") == null)
            {
                InflectionsLayoutRow row = group.GetRowIndexed(0);

                if (row.CellCount() == 0)
                    return;

                int columnCount = columnHeadingsRow.CellCount();

                if (columnCount != 0)
                {
                    int familyStart = 0;
                    int familyEnd = 0;
                    InflectionsLayoutCell majorFamilyCell = columnHeadingsRow.Cells[familyStart];
                    int headingCount = majorFamilyCell.Headings.Count();
                    int headingIndex;
                    string majorFamilyText = majorFamilyCell.Headings[0].Text;
                    InflectionsLayoutCell cell;
                    InflectionsLayoutCell familyCell;
                    InflectionsLayoutCell subFamilyCell;
                    InflectionsLayoutCell subFamilyStartCell;
                    InflectionsLayoutCell aCell;
                    InflectionsLayoutHeading familyHeading;
                    InflectionsLayoutHeading subFamilyHeading;
                    InflectionsLayoutHeading subFamilyStartHeading;
                    InflectionsLayoutHeading aHeading;
                    int majorIndex = 0;

                    while (majorIndex < columnCount)
                    {
                        for (; majorIndex < columnCount; majorIndex++)
                        {
                            cell = columnHeadingsRow.Cells[majorIndex];
                            string majorText = cell.Headings[0].Text;

                            if (majorText != majorFamilyText)
                                break;
                        }

                        familyEnd = majorIndex;

                        int familyCount = familyEnd - familyStart;

                        familyCell = columnHeadingsRow.GetCellIndexed(familyStart);
                        familyHeading = familyCell.GetHeadingIndexed(0);
                        familyHeading.ColumnSpan = familyCount;

                        for (int familyIndex = familyStart + 1; familyIndex < familyEnd; familyIndex++)
                        {
                            familyCell = columnHeadingsRow.GetCellIndexed(familyIndex);
                            familyHeading = familyCell.GetHeadingIndexed(0);
                            familyHeading.ColumnSpan = 0;
                            familyHeading.Text = String.Empty;
                        }

                        for (headingIndex = 1; headingIndex < headingCount - 1; headingIndex++)
                        {
                            int subFamilyStart = familyStart;
                            int subFamilyEnd = familyStart;
                            subFamilyStartCell = columnHeadingsRow.GetCellIndexed(subFamilyStart);
                            subFamilyStartHeading = subFamilyStartCell.GetHeadingIndexed(headingIndex);
                            string subFamilyText = subFamilyStartHeading.Text;
                            int subFamilyIndex;

                            for (subFamilyIndex = familyStart; subFamilyIndex < familyEnd; subFamilyIndex++)
                            {
                                subFamilyCell = columnHeadingsRow.GetCellIndexed(subFamilyIndex);
                                subFamilyHeading = subFamilyCell.GetHeadingIndexed(headingIndex);
                                string headingText = subFamilyHeading.Text;

                                if (headingText != subFamilyText)
                                    break;
                            }

                            subFamilyEnd = subFamilyIndex;

                            int subFamilyCount = subFamilyEnd - subFamilyStart;

                            aCell = columnHeadingsRow.GetCellIndexed(subFamilyStart);
                            aHeading = aCell.GetHeadingIndexed(headingIndex);
                            aHeading.ColumnSpan = subFamilyCount;

                            for (int i = subFamilyStart + 1; i < subFamilyEnd; i++)
                            {
                                aCell = columnHeadingsRow.GetCellIndexed(i);
                                aHeading = aCell.GetHeadingIndexed(headingIndex);
                                aHeading.ColumnSpan = 0;
                                aHeading.Text = String.Empty;
                            }

                            subFamilyStart = subFamilyEnd;

                            if (subFamilyStart < columnCount)
                            {
                                subFamilyStartCell = columnHeadingsRow.Cells[subFamilyStart];
                                subFamilyStartHeading = subFamilyStartCell.GetHeadingIndexed(headingIndex);
                                subFamilyText = subFamilyStartHeading.Text;
                            }
                        }

                        familyStart = familyEnd;

                        if (familyEnd < columnCount)
                        {
                            majorFamilyCell = columnHeadingsRow.Cells[familyStart];
                            majorFamilyText = majorFamilyCell.Headings[0].Text;
                        }
                    }

                    // Change "None" headings to empty.
                    // Delete heading rows where all the headings are empty.

                    bool haveNonEmptyHeadingRow = false;

                    for (headingIndex = 0; headingIndex < headingCount; headingIndex++)
                    {
                        bool headingRowEmpty = true;

                        foreach (InflectionsLayoutCell theCell in columnHeadingsRow.Cells)
                        {
                            aHeading = theCell.GetHeadingIndexed(headingIndex);

                            if (aHeading.Text == "None")
                                aHeading.Text = String.Empty;
                            else if (!String.IsNullOrEmpty(aHeading.Text))
                                headingRowEmpty = false;
                        }

                        if (!headingRowEmpty)
                            haveNonEmptyHeadingRow = true;
                        else if (!haveNonEmptyHeadingRow)
                        {
                            foreach (InflectionsLayoutCell theCell in columnHeadingsRow.Cells)
                                theCell.DeleteHeadingIndexed(headingIndex);

                            headingIndex--;
                            headingCount--;
                        }
                    }

                    if (group.RowCount() > 1)
                    {
                        // Insert empty column headings for row heading column.

                        InflectionsLayoutColumn emptyColumn;
                        InflectionsLayoutCell emptyCell = new InflectionsLayoutCell(
                            new Designator(null, new List<Classifier>()),
                            1,
                            1);
                        List<InflectionsLayoutHeading> headings = new List<InflectionsLayoutHeading>(headingCount);

                        bool first = true;

                        for (headingIndex = 0; headingIndex < headingCount; headingIndex++)
                        {
                            InflectionsLayoutHeading heading = new InflectionsLayoutHeading(
                                String.Empty,
                                (first ? headingCount : 0),
                                (first ? 1 : 0));
                            first = false;
                            headings.Add(heading);
                        }

                        emptyCell.Headings = headings;
                        Designator emptyDesignation = new Designator(null, new List<Classifier>());
                        emptyColumn = new InflectionsLayoutColumn(emptyDesignation);
                        emptyColumn.Cell = emptyCell;
                        columnHeadingsRow.InsertColumn(0, emptyColumn);
                        columnHeadingsRow.InsertCell(0, emptyColumn.Cell);
                    }

                    group.InsertRow(0, columnHeadingsRow);
                    //columnHeadingsRow.Display(null, DisplayDetail.Full, 0);
                }
            }
        }
    }
}
