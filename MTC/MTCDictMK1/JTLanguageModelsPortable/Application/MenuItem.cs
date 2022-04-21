using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTLanguageModelsPortable.Application
{
    public enum MenuItemType
    {
        LeafItem,
        SeparatorItem,
        SubMenu,
        ApplicationMenu,
        Unknown
    }

    public class MenuItem
    {
        public string Key { get; set; }
        public string ParentKey { get; set; }
        public MenuItemType Type { get; set; }
        public string Text { get; set; }
        public string Action { get; set; }
        public string Controller { get; set; }
        public string ImageName { get; set; }
        public string Help { get; set; }
        public bool IsDisplayed { get; set; }
        public bool IsEnabled { get; set; }
        public List<MenuItem> ChildItems { get; set; }

        public MenuItem(
            string key,
            string parentKey,
            MenuItemType type,
            string text,
            string action,
            string controller,
            string imageName,
            string help,
            bool isDisplayed,
            bool isEnabled,
            List<MenuItem> childItems)
        {
            Key = key;
            ParentKey = parentKey;
            Type = type;
            Text = text;
            Action = action;
            Controller = controller;
            ImageName = imageName;
            Help = help;
            IsDisplayed = isDisplayed;
            IsEnabled = isEnabled;
            ChildItems = childItems;
        }

        public MenuItem(MenuItem other)
        {
            Key = other.Key;
            ParentKey = other.ParentKey;
            Type = other.Type;
            Text = other.Text;
            Action = other.Action;
            Controller = other.Controller;
            ImageName = other.ImageName;
            Help = other.Help;
            IsDisplayed = other.IsDisplayed;
            IsEnabled = other.IsEnabled;
            ChildItems = other.ChildItems;
        }

        public MenuItem()
        {
            Key = String.Empty;
            ParentKey = String.Empty;
            Type = MenuItemType.Unknown;
            Text = String.Empty;
            Action = String.Empty;
            Controller = String.Empty;
            ImageName = String.Empty;
            Help = String.Empty;
            IsDisplayed = true;
            IsEnabled = true;
            ChildItems = null;
        }

        public string Path
        {
            get
            {
                if (!String.IsNullOrEmpty(ParentKey))
                    return ParentKey + "|" + Key;
                else
                    return Key;
            }
        }

        public string HelpKey
        {
            get
            {
                return Controller + Action + "Help";
            }
        }

        public MenuItem FindChild(string key)
        {
            if (ChildItems != null)
                return ChildItems.FirstOrDefault(x => x.Key == key);
            return null;
        }
    }
}
