using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Content;

namespace JTLanguageModelsPortable.Admin
{
    public enum EditActionType
    {
        None,
        DeleteSentenceRun,
        OverwriteSentenceRun,
        InsertSentenceRun,
        DeleteSentenceRuns,
        OverwriteSentenceRuns,
        InsertSentenceRuns,
        DeleteLanguageItem,
        OverwriteLanguageItem,
        InsertLanguageItem,
        DeleteLanguageItems,
        OverwriteLanguageItems,
        InsertLanguageItems,
        DeleteStudyItem,
        OverwriteStudyItem,
        InsertStudyItem,
        DeleteStudyItems,
        OverwriteStudyItems,
        InsertStudyItems,
        DeleteMediaFile,
        OverwriteMediaFile,
        SetProfile,
        DeleteContent,
        InsertContent,
        DeleteNode,
        InsertNode,
        DeleteTree,
        InsertTree
    };

    public class EditAction : BaseObject
    {
        public EditActionType ActionType { get; set; }
        string StudyItemKey { get; set; }
        int TextRunIndex { get; set; }
        string FileName { get; set; }
        BaseMarkupContainer TreeNodeContentProfile { get; set; }
        List<string> StringKeys { get; set; }
        List<MultiLanguageItem> StudyItems { get; set; }
        MultiLanguageItem StudyItem { get; set; }
        LanguageItem LanguageItem { get; set; }
        LanguageID TextRunLanguageID { get; set; }
        TextRun TextRun { get; set; }

        public EditAction(EditActionType actionType)
        {
            ClearEditAction();
            ActionType = actionType;
        }

        public EditAction(
            EditActionType actionType,
            BaseMarkupContainer treeNodeContentProfile)
        {
            ClearEditAction();
            ActionType = actionType;
            TreeNodeContentProfile = treeNodeContentProfile;
        }

        public EditAction(
            EditActionType actionType,
            List<string> stringKeys)
        {
            ClearEditAction();
            ActionType = actionType;
            StringKeys = stringKeys;
        }

        public EditAction(
            EditActionType actionType,
            List<MultiLanguageItem> studyItems,
            string mediaZipFileName)
        {
            ClearEditAction();
            ActionType = actionType;
            StudyItems = studyItems;
            FileName = mediaZipFileName;
        }

        public EditAction(
            EditActionType actionType,
            string studyItemKey,
            MultiLanguageItem studyItem,
            string mediaZipFileName)
        {
            ClearEditAction();
            ActionType = actionType;
            StudyItemKey = studyItemKey;
            StudyItem = studyItem;
            FileName = mediaZipFileName;
        }

        public EditAction(
            EditActionType actionType,
            string studyItemKey,
            LanguageItem languageItem,
            string mediaZipFileName)
        {
            ClearEditAction();
            ActionType = actionType;
            StudyItemKey = studyItemKey;
            LanguageItem = languageItem;
            FileName = mediaZipFileName;
        }

        public EditAction(
            EditActionType actionType,
            string studyItemKey,
            int textRunIndex,
            LanguageID textRunLanguageID,
            TextRun textRun,
            string mediaZipFileName)
        {
            ClearEditAction();
            ActionType = actionType;
            StudyItemKey = studyItemKey;
            TextRunLanguageID = textRunLanguageID;
            TextRun = textRun;
            FileName = mediaZipFileName;
        }

        public EditAction(XElement element)
        {
            ClearEditAction();
            OnElement(element);
        }

        public EditAction(EditAction other)
        {
            ClearEditAction();
        }

        public EditAction()
        {
            ClearEditAction();
        }

        public void ClearEditAction()
        {
            ActionType = EditActionType.None;
            StudyItemKey = null;
            TextRunIndex = -1;
            FileName = null;
            TreeNodeContentProfile = null;
            StringKeys = null;
            StudyItems = null;
            StudyItem = null;
            LanguageItem = null;
            TextRunLanguageID = null;
            TextRun = null;
        }

        public void CopyEditAction(EditAction other)
        {
            ActionType = other.ActionType;
            StudyItemKey = other.StudyItemKey;
            TextRunIndex = other.TextRunIndex;
            FileName = other.FileName;
            TreeNodeContentProfile = (other.TreeNodeContentProfile != null ? new BaseMarkupContainer(other.TreeNodeContentProfile) : null);
            StringKeys = (other.StringKeys != null ? new List<string>(other.StringKeys) : null);
            StudyItems = MultiLanguageItem.CloneStudyItems(other.StudyItems);
            StudyItem = (other.StudyItem != null ? new MultiLanguageItem(other.StudyItem) : null);
            LanguageItem = (other.LanguageItem != null ? new LanguageItem(other.LanguageItem) : null);
            TextRunLanguageID = (other.TextRunLanguageID != null ? new LanguageID(other.TextRunLanguageID) : null);
            TextRun = (other.TextRun != null ? new TextRun(other.TextRun) : null);
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);

            element.Add(new XElement("ActionType", GetEditActionTypeString(ActionType)));

            if (!String.IsNullOrEmpty(StudyItemKey))
                element.Add(new XElement("StudyItemKey", StudyItemKey));

            if (TextRunIndex != -1)
                element.Add(new XElement("TextRunIndex", TextRunIndex));

            if (!String.IsNullOrEmpty(FileName))
                element.Add(new XAttribute("FileName", FileName));

            if (TreeNodeContentProfile != null)
                element.Add(TreeNodeContentProfile.GetElement("TreeNodeContentProfile"));

            if (StringKeys != null)
            {
                string value = ObjectUtilities.GetStringFromStringList(StringKeys);
                element.Add(new XElement("StringKeys", value));
            }

            if (StudyItems != null)
            {
                XElement studyItemsElement = new XElement("StudyItems");

                foreach (MultiLanguageItem studyItem in StudyItems)
                    studyItemsElement.Add(studyItem.GetElement("StudyItem"));

                element.Add(studyItemsElement);
            }

            if (StudyItem != null)
                element.Add(StudyItem.GetElement("StudyItem"));

            if (LanguageItem != null)
                element.Add(LanguageItem.GetElement("LanguageItem"));

            if (TextRunLanguageID != null)
                element.Add(new XElement("TextRunLanguageCode", TextRunLanguageID.LanguageCultureExtensionCode));

            if (TextRun != null)
                element.Add(TextRun.GetElement("TextRun"));

            return element;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "ActionType":
                    ActionType = GetEditActionType(childElement.Value.Trim());
                    break;
                case "StudyItemKey":
                    StudyItemKey = childElement.Value.Trim();
                    break;
                case "TextRunIndex":
                    TextRunIndex = ObjectUtilities.GetIntegerFromString(childElement.Value.Trim(), -1);
                    break;
                case "FileName":
                    FileName = childElement.Value.Trim();
                    break;
                case "TreeNodeContentProfile":
                    TreeNodeContentProfile = new BaseMarkupContainer(childElement);
                    break;
                case "StringKeys":
                    StringKeys = ObjectUtilities.GetStringListFromString(childElement.Value.Trim());
                    break;
                case "StudyItems":
                    StudyItems = new List<MultiLanguageItem>();
                    foreach (XElement studyItemElement in childElement.Elements())
                        StudyItems.Add(new MultiLanguageItem(studyItemElement));
                    break;
                case "StudyItem":
                    StudyItem = new MultiLanguageItem(childElement);
                    break;
                case "LanguageItem":
                    LanguageItem = new LanguageItem(childElement);
                    break;
                case "TextRunLanguageCode":
                    TextRunLanguageID = new LanguageID(childElement.Value.Trim());
                    break;
                case "TextRun":
                    TextRun = new TextRun(childElement);
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }

        public static string GetEditActionTypeString(EditActionType actionType)
        {
            return actionType.ToString();
        }

        public static EditActionType GetEditActionType(string actionTypeString)
        {
            EditActionType actionType;

            switch (actionTypeString)
            {
                case "None":
                    actionType = EditActionType.None;
                    break;
                case "DeleteSentenceRun":
                    actionType = EditActionType.DeleteSentenceRun;
                    break;
                case "OverwriteSentenceRun":
                    actionType = EditActionType.OverwriteSentenceRun;
                    break;
                case "InsertSentenceRun":
                    actionType = EditActionType.InsertSentenceRun;
                    break;
                case "DeleteSentenceRuns":
                    actionType = EditActionType.DeleteSentenceRuns;
                    break;
                case "OverwriteSentenceRuns":
                    actionType = EditActionType.OverwriteSentenceRuns;
                    break;
                case "InsertSentenceRuns":
                    actionType = EditActionType.InsertSentenceRuns;
                    break;
                case "DeleteLanguageItem":
                    actionType = EditActionType.DeleteLanguageItem;
                    break;
                case "OverwriteLanguageItem":
                    actionType = EditActionType.OverwriteLanguageItem;
                    break;
                case "InsertLanguageItem":
                    actionType = EditActionType.InsertLanguageItem;
                    break;
                case "DeleteLanguageItems":
                    actionType = EditActionType.DeleteLanguageItems;
                    break;
                case "OverwriteLanguageItems":
                    actionType = EditActionType.OverwriteLanguageItems;
                    break;
                case "InsertLanguageItems":
                    actionType = EditActionType.InsertLanguageItems;
                    break;
                case "DeleteStudyItem":
                    actionType = EditActionType.DeleteStudyItem;
                    break;
                case "OverwriteStudyItem":
                    actionType = EditActionType.OverwriteStudyItem;
                    break;
                case "InsertStudyItem":
                    actionType = EditActionType.InsertStudyItem;
                    break;
                case "DeleteStudyItems":
                    actionType = EditActionType.DeleteStudyItems;
                    break;
                case "OverwriteStudyItems":
                    actionType = EditActionType.OverwriteStudyItems;
                    break;
                case "InsertStudyItems":
                    actionType = EditActionType.InsertStudyItems;
                    break;
                case "DeleteMediaFile":
                    actionType = EditActionType.DeleteMediaFile;
                    break;
                case "OverwriteMediaFile":
                    actionType = EditActionType.OverwriteMediaFile;
                    break;
                case "SetProfile":
                    actionType = EditActionType.SetProfile;
                    break;
                case "DeleteContent":
                    actionType = EditActionType.DeleteContent;
                    break;
                case "InsertContent":
                    actionType = EditActionType.InsertContent;
                    break;
                case "DeleteNode":
                    actionType = EditActionType.DeleteNode;
                    break;
                case "InsertNode":
                    actionType = EditActionType.InsertNode;
                    break;
                case "DeleteTree":
                    actionType = EditActionType.DeleteTree;
                    break;
                case "InsertTree":
                    actionType = EditActionType.InsertTree;
                    break;
                default:
                    throw new Exception("Unexpected action type: " + actionTypeString);
            }

            return actionType;
        }
    }
}
