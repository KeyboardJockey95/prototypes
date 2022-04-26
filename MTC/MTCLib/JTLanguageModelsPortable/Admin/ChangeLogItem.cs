using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Admin
{
    // Key: UserName|ProfileName|DateTime
    public class ChangeLogItem : BaseObjectKeyed
    {
        public string UserName { get; set; }
        public string ProfileName { get; set; }
        public DateTime LogTime { get; set; }
        public int TreeKey { get; set; }
        public int NodeKey { get; set; }
        public string ContentKey { get; set; }
        public int ContentStorageKey { get; set; }
        public EditAction UndoEditAction { get; set; }
        public EditAction RedoEditAction { get; set; }

        public ChangeLogItem(
            string userName,
            string profileName,
            DateTime logTime,
            int treeKey,
            int nodeKey,
            string contentKey,
            int contentStorageKey,
            EditAction undoEditAction,
            EditAction redoEditAction)
        {
            UserName = userName;
            ProfileName = profileName;
            LogTime = logTime;
            TreeKey = treeKey;
            NodeKey = nodeKey;
            ContentKey = contentKey;
            ContentStorageKey = ContentStorageKey;
            UndoEditAction = undoEditAction;
            RedoEditAction = redoEditAction;
        }

        public ChangeLogItem(XElement element)
        {
            ClearChangeLogItem();
            OnElement(element);
        }

        public ChangeLogItem(ChangeLogItem other)
        {
            CopyChangeLogItem(other);
        }

        public ChangeLogItem()
        {
            ClearChangeLogItem();
        }

        public void ClearChangeLogItem()
        {
            UserName = String.Empty;
            ProfileName = String.Empty;
            LogTime = DateTime.MinValue;
            TreeKey = -1;
            NodeKey = -1;
            ContentKey = String.Empty;
            ContentStorageKey = -1;
            UndoEditAction = null;
            RedoEditAction = null;
        }

        public void CopyChangeLogItem(ChangeLogItem other)
        {
            UserName = other.UserName;
            ProfileName = other.ProfileName;
            LogTime = other.LogTime;
            TreeKey = other.TreeKey;
            NodeKey = other.NodeKey;
            ContentKey = other.ContentKey;
            ContentStorageKey = other.ContentStorageKey;
            UndoEditAction = other.UndoEditAction;
            RedoEditAction = other.RedoEditAction;
        }

        public override XElement GetElement(string name)
        {
            XElement element = base.GetElement(name);

            if (!String.IsNullOrEmpty(UserName))
                element.Add(new XElement("UserName", UserName));

            if (!String.IsNullOrEmpty(ProfileName))
                element.Add(new XElement("ProfileName", ProfileName));

            if (LogTime != DateTime.MinValue)
                element.Add(new XElement("LogTime", ObjectUtilities.GetStringFromDateTime(LogTime)));

            if (TreeKey != -1)
                element.Add(new XElement("TreeKey", TreeKey));

            if (NodeKey != -1)
                element.Add(new XElement("NodeKey", NodeKey));

            if (!String.IsNullOrEmpty(ContentKey))
                element.Add(new XAttribute("ContentKey", ContentKey));

            if (ContentStorageKey != -1)
                element.Add(new XElement("ContentStorageKey", ContentStorageKey));

            if (UndoEditAction != null)
                element.Add(UndoEditAction.GetElement("UndoEditAction"));

            if (RedoEditAction != null)
                element.Add(RedoEditAction.GetElement("RedoEditAction"));

            return element;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "UserName":
                    UserName = childElement.Value.Trim();
                    break;
                case "ProfileName":
                    ProfileName = childElement.Value.Trim();
                    break;
                case "LogTime":
                    LogTime = ObjectUtilities.GetDateTimeFromString(childElement.Value.Trim(), DateTime.MinValue);
                    break;
                case "TreeKey":
                    TreeKey = ObjectUtilities.GetIntegerFromString(childElement.Value.Trim(), -1);
                    break;
                case "NodeKey":
                    NodeKey = ObjectUtilities.GetIntegerFromString(childElement.Value.Trim(), -1);
                    break;
                case "ContentKey":
                    ContentKey = childElement.Value.Trim();
                    break;
                case "ContentStorageKey":
                    ContentStorageKey = ObjectUtilities.GetIntegerFromString(childElement.Value.Trim(), -1);
                    break;
                case "UndoEditAction":
                    UndoEditAction = new EditAction(childElement);
                    break;
                case "RedoEditAction":
                    RedoEditAction = new EditAction(childElement);
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }
    }
}
