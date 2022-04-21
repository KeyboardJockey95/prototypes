using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Application;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.ObjectInterfaces;

namespace JTLanguageModelsPortable.Application
{
    public class ApplicationMenu : MenuItem
    {
        public IApplicationCookies Cookies;
        public UserRecord UserRecord;
        public UserProfile UserProfile;
        public ILanguageTranslator Translator;
        public LanguageUtilities LanguageUtilities;
        public MenuItem HomeMenu;
        public MenuItem LessonsMenu;
        public MenuItem PlansMenu;
        public MenuItem ToolMenu;
        public MenuItem ResourcesMenu;
        public MenuItem TeacherMenu;
        public MenuItem AdminMenu;
        public MenuItem HelpMenu;

        public ApplicationMenu(IApplicationCookies cookies,
            UserRecord userRecord, UserProfile userProfile, ILanguageTranslator translator,
            LanguageUtilities languageUtilities)
        {
            Cookies = cookies;
            UserRecord = userRecord;
            UserProfile = userProfile;
            Translator = translator;
            LanguageUtilities = languageUtilities;

            Key = "Application";
            ParentKey = String.Empty;
            Type = MenuItemType.ApplicationMenu;
            Text = S(ApplicationData.ApplicationName);
            Action = "MainMenu";
            Controller = "Menu";
            IsDisplayed = true;
            IsEnabled = true;

            InitializeApplicationMenu();
        }

        public void InitializeApplicationMenu()
        {
            ChildItems = new List<MenuItem>();
            ChildItems.Add(CreateHomeMenu());
            ChildItems.Add(CreateLessonsMenu());
            ChildItems.Add(CreatePlansMenu());
            ChildItems.Add(CreateToolMenu());
            ChildItems.Add(CreateResourcesMenu());
            ChildItems.Add(CreateTeacherMenu());
            ChildItems.Add(CreateAdminMenu());
            ChildItems.Add(CreateHelpMenu());
        }

        public void UpdateApplicationMenu()
        {
            UpdateHomeMenu();
            UpdateLessonsMenu();
            UpdatePlansMenu();
            UpdateToolMenu();
            UpdateResourcesMenu();
            UpdateTeacherMenu();
            UpdateAdminMenu();
            UpdateHelpMenu();
        }

        public MenuItem CreateHomeMenu()
        {
            bool isMobile = (ApplicationData.IsMobileVersion || ApplicationData.IsTestMobileVersion);
            bool isUser = ((UserRecord != null) && !UserRecord.IsAnonymous());
            bool isIOS = ApplicationData.IsIOS;
            List<MenuItem> menuItems = new List<MenuItem>();
            menuItems.Add(
                new MenuItem(
                    "Languages", "Home", MenuItemType.LeafItem, S("Languages"), "Languages", "Home",
                    "HomeLanguages", "Select the languages for the current profile.", true, true, null));
            menuItems.Add(
                new MenuItem(
                    "Teachers", "Home", MenuItemType.LeafItem, S("Teachers"), "Teachers", "Home",
                    "HomeTeachers", "Select the teachers whose courses you want to see.", true, true, null));
            menuItems.Add(
                new MenuItem(
                    "ManageCourses", "Home", MenuItemType.LeafItem, S("Manage Courses"),
                    "ManageCourses", "Lessons", "LessonsManageCourses",
                    "Download, update, or delete courses on your phone.", isMobile, true, null));
            menuItems.Add(
                new MenuItem(
                    "ManageVocabulary", "Home", MenuItemType.LeafItem, S("Manage Vocabulary"),
                    "ManageVocabulary", "Home", "HomeManageVocabulary",
                    "Manage vocabulary from text study.", isUser || isMobile, true, null));
            menuItems.Add(
                new MenuItem(
                    "Profile", "Home", MenuItemType.LeafItem, S("Profile"), "Profile", "Home",
                    "HomeProfile",
                    "Select, add, or delete a profile, which is the set of languages in use"
                        + " and the associated context.", isUser || isMobile, true, null));
            menuItems.Add(
                new MenuItem(
                    "Account", "Home", MenuItemType.LeafItem, S("Account"), "Account", "Home",
                    "HomeAccount", "Edit your account information here.", isUser, true, null));
            menuItems.Add(
                new MenuItem(
                    "Settings", "Home", MenuItemType.LeafItem, S("Settings"), "Settings", "Home",
                    "HomeSettings", "Manage some overall settings.", true, true, null));
            menuItems.Add(
                new MenuItem(
                    "Maintenance", "Home", MenuItemType.LeafItem, S("Maintenance"), "Maintenance", "Home",
                    "HomeMaintenance", "Perform some maintenance.", true, true, null));
            menuItems.Add(
                new MenuItem(
                    "Donate", "Home", MenuItemType.LeafItem, S("Donate"), "Donate", "Home",
                    "HomeDonate", "Donate to help offset costs.", !isIOS, true, null));
            MenuItem subMenu = new MenuItem(
                "Home", Key, MenuItemType.SubMenu, S("Home"), "HomeMenu", "Menu",
                "MainHome", "Display the \"Home\" menu, which displays menu items related to your account or profile overall.",
                true, true, menuItems);
            HomeMenu = subMenu;
            return subMenu;
        }

        public void UpdateHomeMenu()
        {
        }

        public MenuItem CreateLessonsMenu()
        {
            List<MenuItem> menuItems = new List<MenuItem>();
            bool hasCourse = (GetUserOptionInteger("CoursesTreeKey", 0) > 0 ? true : false);
            bool hasGroup = (GetUserOptionInteger("CoursesGroupKey", 0) > 0 ? true : false);
            bool hasLesson = (GetUserOptionInteger("CoursesNodeKey", 0) > 0 ? true : false);
            bool hasContent = !String.IsNullOrEmpty(GetUserOptionString("CoursesContentKey"));
            bool hasStudyContent = !String.IsNullOrEmpty(GetUserOptionString("CoursesStudyContentKey"));
            menuItems.Add(
                new MenuItem(
                    "Courses", "Lessons", MenuItemType.LeafItem, S("Courses"), "Courses", "Lessons",
                    "LessonsCourses", "Display the courses available with respect to selected languages and teachers.",
                    true, true, null));
            menuItems.Add(
                new MenuItem(
                    "Course", "Lessons", MenuItemType.LeafItem, S("Course"), "Course", "Lessons",
                    "LessonsCourse", "Go to the course in your most recently visit path, if any."
                        + "If gray, it means no path has been stored yet in the current profile.",
                    true, hasCourse, null));
            menuItems.Add(
                new MenuItem(
                    "Group", "Lessons", MenuItemType.LeafItem, S("Group"), "Group", "Lessons",
                    "LessonsGroup", "Go to the lesson group in your most recently visit path, if any."
                        + "If gray, it means no path has been stored yet in the current profile.",
                    true, hasGroup, null));
            menuItems.Add(
                new MenuItem(
                    "Lesson", "Lessons", MenuItemType.LeafItem, S("Lesson"), "Lesson", "Lessons",
                    "LessonsLesson", "Go to the lesson in your most recently visit path, if any."
                        + "If gray, it means no path has been stored yet in the current profile.",
                    true, hasLesson, null));
            menuItems.Add(
                new MenuItem(
                    "Content", "Lessons", MenuItemType.LeafItem, S("Content"), "Content", "Lessons",
                    "LessonsContent", "Go to the lesson content item in your most recently visit path, if any."
                        + "If gray, it means no path has been stored yet in the current profile.",
                    true, hasContent, null));
            menuItems.Add(
                new MenuItem(
                    "Study", "Lessons", MenuItemType.LeafItem, S("Study"), "Study", "Lessons",
                    "LessonsStudy", "Go to the study page for the lesson content in your most recently visit path, if any."
                        + "If gray, it means no path has been stored yet in the current profile, or the content is not a study list.",
                    true, hasStudyContent, null));
            MenuItem subMenu = new MenuItem(
                "Lessons", Key, MenuItemType.SubMenu, S("Lessons"), "LessonsMenu", "Menu",
                "MainLessons", "Display the \"Lessons\" menu, which displays menu items for jumping to items in your last visited lesson hierarchy path.",
                true, true, menuItems);
            LessonsMenu = subMenu;
            return subMenu;
        }

        public void UpdateLessonsMenu()
        {
            bool hasCourse = (GetUserOptionInteger("CoursesTreeKey", 0) > 0 ? true : false);
            bool hasGroup = (GetUserOptionInteger("CoursesGroupKey", 0) > 0 ? true : false);
            bool hasLesson = (GetUserOptionInteger("CoursesNodeKey", 0) > 0 ? true : false);
            bool hasContent = !String.IsNullOrEmpty(GetUserOptionString("CoursesContentKey"));
            bool hasStudyContent = !String.IsNullOrEmpty(GetUserOptionString("CoursesStudyContentKey"));
            LessonsMenu.FindChild("Course").IsEnabled = hasCourse;
            LessonsMenu.FindChild("Group").IsEnabled = hasGroup;
            LessonsMenu.FindChild("Lesson").IsEnabled = hasLesson;
            LessonsMenu.FindChild("Content").IsEnabled = hasContent;
            LessonsMenu.FindChild("Study").IsEnabled = hasStudyContent;
        }

        public MenuItem CreatePlansMenu()
        {
            List<MenuItem> menuItems = new List<MenuItem>();
            bool isAnonymous = ((UserRecord == null) || UserRecord.IsAnonymous());
            bool isDisplay = !isAnonymous;
            bool hasPlan = (GetUserOptionInteger("PlansTreeKey", 0) > 0 ? true : false);
            bool hasGroup = (GetUserOptionInteger("PlansGroupKey", 0) > 0 ? true : false);
            bool hasLesson = (GetUserOptionInteger("PlansNodeKey", 0) > 0 ? true : false);
            bool hasContent = !String.IsNullOrEmpty(GetUserOptionString("PlansContentKey"));
            bool hasStudyContent = !String.IsNullOrEmpty(GetUserOptionString("PlansStudyContentKey"));
            menuItems.Add(
                new MenuItem(
                    "Plans", "Plans", MenuItemType.LeafItem, S("Plans"), "PlanList", "Plans",
                    "PlansPlans", "Display the plans you've created previously, with respect to selected languages.",
                    true, true, null));
            menuItems.Add(
                new MenuItem(
                    "Plan", "Plans", MenuItemType.LeafItem, S("Plan"), "Plan", "Plans",
                    "PlansPlan", "Go to the plan in your most recently visit plan path, if any."
                        + "If gray, it means no plan path has been stored yet in the current profile.",
                    true, hasPlan, null));
            menuItems.Add(
                new MenuItem(
                    "Group", "Plans", MenuItemType.LeafItem, S("Group"), "Group", "Plans",
                    "LessonsGroup", "Go to the lesson group in your most recently visit plan path, if any."
                        + "If gray, it means no plan path has been stored yet in the current profile.",
                    true, hasGroup, null));
            menuItems.Add(
                new MenuItem(
                    "Lesson", "Plans", MenuItemType.LeafItem, S("Lesson"), "Lesson", "Plans",
                    "LessonsLesson", "Go to the lesson in your most recently visit plan path, if any."
                        + "If gray, it means no plan path has been stored yet in the current profile.",
                    true, hasLesson, null));
            menuItems.Add(
                new MenuItem(
                    "Content", "Plans", MenuItemType.LeafItem, S("Content"), "Content", "Plans",
                    "LessonsContent", "Go to the lesson content in your most recently visit plan path, if any."
                        + "If gray, it means no plan path has been stored yet in the current profile.",
                    true, hasContent, null));
            menuItems.Add(
                new MenuItem(
                    "Study", "Plans", MenuItemType.LeafItem, S("Study"), "Study", "Plans",
                    "LessonsStudy", "Go to the study page for the lesson content in your most recently visit plan path, if any."
                        + "If gray, it means no plan path has been stored yet in the current profile, or the content is not a study list.",
                    true, hasStudyContent, null));
            MenuItem subMenu = new MenuItem(
                "Plans", Key, MenuItemType.SubMenu, S("Plans"), "PlansMenu", "Menu",
                "MainPlans", "Display the \"Plans\" menu, which displays menu items for jumping to items in your last visited plan hierarchy path.",
                isDisplay, true, menuItems);
            PlansMenu = subMenu;
            return subMenu;
        }

        public void UpdatePlansMenu()
        {
            bool hasPlan = (GetUserOptionInteger("PlansTreeKey", 0) > 0 ? true : false);
            bool hasGroup = (GetUserOptionInteger("PlansGroupKey", 0) > 0 ? true : false);
            bool hasLesson = (GetUserOptionInteger("PlansNodeKey", 0) > 0 ? true : false);
            bool hasContent = !String.IsNullOrEmpty(GetUserOptionString("PlansContentKey"));
            bool hasStudyContent = !String.IsNullOrEmpty(GetUserOptionString("PlansStudyContentKey"));
            PlansMenu.FindChild("Plan").IsEnabled = hasPlan;
            PlansMenu.FindChild("Group").IsEnabled = hasGroup;
            PlansMenu.FindChild("Lesson").IsEnabled = hasLesson;
            PlansMenu.FindChild("Content").IsEnabled = hasContent;
            PlansMenu.FindChild("Study").IsEnabled = hasStudyContent;
        }

        public MenuItem CreateToolMenu()
        {
            List<MenuItem> menuItems = new List<MenuItem>();
            bool isUser = ((UserRecord != null) && !UserRecord.IsAnonymous());
            bool isDisplayToolMenu = ((UserProfile != null) && UserProfile.HasAnyTargetLanguages() && isUser) ||
                ApplicationData.IsMobileVersion || ApplicationData.IsTestMobileVersion;
            bool hasSession = (GetUserOptionInteger("StudySessionIndex", 0) > 0 ? true : false);
            menuItems.Add(
                new MenuItem(
                    "Default", "Tool", MenuItemType.LeafItem, S("Default"), "Default", "Tool",
                    "ToolDefault", "Display a page where you can edit the default tool profile used for a tool page."
                        + " A tool profile is a collection of settings and configurations used in a study tool."
                        + " For example, it stores the current item selection algorithm used in the flash card tool.",
                    true, true, null));
            menuItems.Add(
                new MenuItem(
                    "Profiles", "Tool", MenuItemType.LeafItem, S("Profiles"), "ToolProfilesView", "Tool",
                    "ToolProfiles", "Display and manage the tool profiles you've defined.",
                    true, true, null));
            menuItems.Add(
                new MenuItem(
                    "Profile", "Tool", MenuItemType.LeafItem, S("Profile"), "ToolProfileView", "Tool",
                    "ToolProfile", "Display or edit the current tool profile.",
                    true, hasSession, null));
            menuItems.Add(
                new MenuItem(
                    "Configuration", "Tool", MenuItemType.LeafItem, S("Configuration"), "Configuration", "Tool",
                    "ToolConfiguration", "Display or edit the current tool configuration."
                        + "A tool configuration is a collection of settings stored in a tool profile."
                        + " For example, it specifies what is displayed in the flash tool card sides.",
                    true, hasSession, null));
            MenuItem subMenu = new MenuItem(
                "Tool", Key, MenuItemType.SubMenu, S("Tool"), "ToolMenu", "Menu",
                "MainTool", "Display the \"Tool\" menu, which displays menu items for managing the study tool profiles and configurations.",
                isDisplayToolMenu, true, menuItems);
            ToolMenu = subMenu;
            return subMenu;
        }

        public void UpdateToolMenu()
        {
            bool hasSession = (GetUserOptionInteger("StudySessionIndex", 0) > 0 ? true : false);
            ToolMenu.FindChild("Profile").IsEnabled = hasSession;
            ToolMenu.FindChild("Configuration").IsEnabled = hasSession;
        }

        public MenuItem CreateResourcesMenu()
        {
            bool isDisplayed = UserRecord.IsAdministrator();
            List<MenuItem> menuItems = new List<MenuItem>();
            menuItems.Add(
                new MenuItem(
                    "Dictionary", "Resources", MenuItemType.LeafItem, S("Dictionary"), "DictionarySearch", "Resources",
                    "ResourcesDictionary", "Displays a dictionary for the languages of the current profile.",
                    true, true, null));
            menuItems.Add(
                new MenuItem(
                    "Inflect", "Resources", MenuItemType.LeafItem, S("Inflect"), "InflectWord", "Resources",
                    "ResourcesInflect", "Displays an inflector/conjugator for the languages of the current profile.",
                    true, true, null));
            menuItems.Add(
                new MenuItem(
                    "Translate", "Resources", MenuItemType.LeafItem, S("Translate"), "Translate", "Resources",
                    "ResourcesTranslate", "Displays a translator for the languages of the current profile.",
                    true, true, null));
            menuItems.Add(
                new MenuItem(
                    "Passage", "Resources", MenuItemType.LeafItem, S("Passage"), "Passage", "Resources",
                    "ResourcesPassage", "Displays a passage study tool for the languages of the current profile.",
                    true, true, null));
            menuItems.Add(
                new MenuItem(
                    "Grammar", "Resources", MenuItemType.LeafItem, S("Grammar"), "Grammar", "Resources",
                    "ResourcesGrammar", "Displays a grammar reference for the target language of the current profile.",
                    isDisplayed, true, null));
            menuItems.Add(
                new MenuItem(
                    "Blogs", "Resources", MenuItemType.LeafItem, S("Blogs"), "Blogs", "Resources",
                    "ResourcesBlogs", "Displays blogs related to the target languages of the current profile.",
                    isDisplayed, true, null));
            menuItems.Add(
                new MenuItem(
                    "Forums", "Resources", MenuItemType.LeafItem, S("Forums"), "Forums", "Resources",
                    "ResourcesForums", "Display a forum where you can ask questions and discuss various topics.",
                    true, true, null));
            menuItems.Add(
                new MenuItem(
                    "TextChat", "Resources", MenuItemType.LeafItem, S("Text Chat"), "TextChat", "Resources",
                    "ResourcesTextChat", "Text chat with other students and teachers.",
                    isDisplayed, true, null));
            menuItems.Add(
                new MenuItem(
                    "VoiceChat", "Resources", MenuItemType.LeafItem, S("Voice Chat"), "VoiceChat", "Resources",
                    "ResourcesVoiceChat", "Voice chat with other students and teachers.",
                    isDisplayed, true, null));
            menuItems.Add(
                new MenuItem(
                    "Links", "Resources", MenuItemType.LeafItem, S("Links"), "Links", "Resources",
                    "ResourcesLinks", "Display useful links for learning languages.",
                    isDisplayed, true, null));
            MenuItem subMenu = new MenuItem(
                "Resources", Key, MenuItemType.SubMenu, S("Resources"), "ResourcesMenu", "Menu",
                "MainResources", "Display the \"Resources\" menu, which displays menu items for other language-learning resources.",
                true, true, menuItems);
            ResourcesMenu = subMenu;
            return subMenu;
        }

        public void UpdateResourcesMenu()
        {
        }

        public MenuItem CreateTeacherMenu()
        {
            bool isUser = ((UserRecord != null) && !UserRecord.IsAnonymous());
            bool isTeacherOrAdministrator = ApplicationData.IsEditVersion && isUser &&
                ((UserRecord != null) && (UserRecord.IsTeacher() || UserRecord.IsAdministrator()));
            bool isMobile = ApplicationData.IsMobileVersion || ApplicationData.IsTestMobileVersion;
            List<MenuItem> menuItems = new List<MenuItem>();
            menuItems.Add(
                new MenuItem(
                    "Languages", "Teacher", MenuItemType.LeafItem, S("Languages"), "TeacherLanguages", "Teacher",
                    "TeacherLanguages", "Manage the languages supported.  You can add new languages here.",
                    isTeacherOrAdministrator, true, null));
            menuItems.Add(
                new MenuItem(
                    "Masters", "Teacher", MenuItemType.LeafItem, S("Masters"), "Masters", "Teacher",
                    "TeacherMasters", "Manage your lesson master templates.",
                    true, true, null));
            menuItems.Add(
                new MenuItem(
                    "Markups", "Teacher", MenuItemType.LeafItem, S("Markups"), "Markups", "Teacher",
                    "TeacherMarkups", "Manage your markup templates."
                        + " A markup template defines a layout for a content, lesson, group, course, or plan page"
                        + " using a markup language extending HTML."
                        + " It also can be used to define the audio layout for generated audio media or the automated lesson mechanism.",
                    true, true, null));
            menuItems.Add(
                new MenuItem(
                    "Text", "Teacher", MenuItemType.LeafItem, S("Text"), "Text", "Teacher",
                    "TeacherText", "Edit or translate page text items.",
                    isTeacherOrAdministrator, true, null));
            menuItems.Add(
                new MenuItem(
                    "Strings", "Teacher", MenuItemType.LeafItem, S("Strings"), "Strings", "Teacher",
                    "TeacherStrings", "Edit or translate user interface labels or messages.",
                    isTeacherOrAdministrator, true, null));
            menuItems.Add(
                new MenuItem(
                    "Audio", "Teacher", MenuItemType.LeafItem, S("Audio"), "Audio", "Teacher",
                    "TeacherAudio", "Edit dictionary audio.",
                    isTeacherOrAdministrator, true, null));
            MenuItem subMenu = new MenuItem(
                "Teacher", Key, MenuItemType.SubMenu, S("Teacher"), "TeacherMenu", "Menu",
                "MainTeacher", "Display the \"Teacher\" menu, which displays menu items for managing languages, lesson masters, markup templates, and page text and strings.",
                isUser, true, menuItems);
            TeacherMenu = subMenu;
            return subMenu;
        }

        public void UpdateTeacherMenu()
        {
        }

        public MenuItem CreateAdminMenu()
        {
            bool isAdministrator = (UserRecord != null ? UserRecord.IsAdministrator() : false);
            bool isDisplayed = false;
            List<MenuItem> menuItems = new List<MenuItem>();
            menuItems.Add(
                new MenuItem(
                    "Members", "Admin", MenuItemType.LeafItem, S("Members"), "Members", "Admin",
                    "AdminMembers", "Mangage the users and teacher database.",
                    isAdministrator, true, null));
            menuItems.Add(
                new MenuItem(
                    "Backup", "Admin", MenuItemType.LeafItem, S("Backup"), "Backup", "Admin",
                    "AdminBackup", "Backup items from database.",
                    isAdministrator, true, null));
            menuItems.Add(
                new MenuItem(
                    "Restore", "Admin", MenuItemType.LeafItem, S("Restore"), "Restore", "Admin",
                    "AdminRestore", "Restore items to database.",
                    isAdministrator, true, null));
            menuItems.Add(
                new MenuItem(
                    "Database", "Admin", MenuItemType.LeafItem, S("Database"), "Database", "Admin",
                    "AdminDatabase", "Manage the overall database.",
                    isAdministrator, true, null));
            menuItems.Add(
                new MenuItem(
                    "Files", "Admin", MenuItemType.LeafItem, S("Files"), "Files", "Admin",
                    "AdminFiles", "Manage files.",
                    isAdministrator, true, null));
            menuItems.Add(
                new MenuItem(
                    "Log", "Admin", MenuItemType.LeafItem, S("Log"), "Log", "Admin",
                    "AdminLog", "Display or manage a log of activity.",
                    isAdministrator, true, null));
            menuItems.Add(
                new MenuItem(
                    "Times", "Admin", MenuItemType.LeafItem, S("Times"), "Times", "Admin",
                    "AdminTimes", "Display system timings.",
                    isDisplayed, true, null));
            menuItems.Add(
                new MenuItem(
                    "Counts", "Admin", MenuItemType.LeafItem, S("Counts"), "Counts", "Admin",
                    "AdminCounts", "Display system counts.",
                    isDisplayed, true, null));
            menuItems.Add(
                new MenuItem(
                    "Settings", "Admin", MenuItemType.LeafItem, S("Settings"), "Settings", "Admin",
                    "AdminSettings", "Manage some system settings.",
                    isDisplayed, true, null));
            menuItems.Add(
                new MenuItem(
                    "DemoUrl", "Admin", MenuItemType.LeafItem, S("DemoUrl"), "DemoUrl", "Admin",
                    "AdminDemoUrl", "Format a URL for a reference to a lesson path with included context.",
                    isDisplayed, true, null));
            menuItems.Add(
                new MenuItem(
                    "Fixup", "Admin", MenuItemType.LeafItem, S("Fixup"), "Fixup", "Admin",
                    "AdminFixup", "Do some fixups related to development.",
                    true, true, null));
            menuItems.Add(
                new MenuItem(
                    "MediaTest", "Admin", MenuItemType.LeafItem, S("MediaTest"), "MediaTest", "Admin",
                    "AdminMediaTest", "A place-holder for a media-related test.",
                    isDisplayed, true, null));
            menuItems.Add(
                new MenuItem(
                    "JavaTest", "Admin", MenuItemType.LeafItem, S("JavaTest"), "JavaTest", "Admin",
                    "AdminJavaTest", "A place-holder for a java test.",
                    isDisplayed, true, null));
            MenuItem subMenu = new MenuItem(
                "Admin", Key, MenuItemType.SubMenu, S("Admin"), "AdminMenu", "Menu",
                "MainAdmin", "Display the \"Admin\" menu, which displays menu items for system administrators.",
                isAdministrator, true, menuItems);
            AdminMenu = subMenu;
            return subMenu;
        }

        public void UpdateAdminMenu()
        {
        }

        public MenuItem CreateHelpMenu()
        {
            List<MenuItem> menuItems = new List<MenuItem>();
            menuItems.Add(
                new MenuItem(
                    "Welcome", "Help", MenuItemType.LeafItem, S("Welcome"), "Welcome", "Help",
                    "HelpWelcome", "Displays an introduction to JTLanguage.", true, true, null));
            menuItems.Add(
                new MenuItem(
                    "WhatsNew", "Help", MenuItemType.LeafItem, S("What's New"), "WhatsNew", "Help",
                    "HelpWhatsNew", "Displays information about the current version.", true, true, null));
            menuItems.Add(
                new MenuItem(
                    "WhatsAhead", "Help", MenuItemType.LeafItem, S("What's Ahead"), "WhatsAhead", "Help",
                    "HelpWhatsAhead", "Displays information about possible future plans.", true, true, null));
            menuItems.Add(
                new MenuItem(
                    "Browse", "Help", MenuItemType.LeafItem, S("Browse"), "Browse", "Help",
                    "HelpBrowse", "Display a table of contents for help information.",
                    true, true, null));
            menuItems.Add(
                new MenuItem(
                    "Contact", "Help", MenuItemType.LeafItem, S("Contact"), "Contact", "Help",
                    "HelpContact", "Display a page for contacting the administrator.",
                    true, true, null));
            menuItems.Add(
                new MenuItem(
                    "About", "Help", MenuItemType.LeafItem, S("About"), "About", "Help",
                    "HelpAbout", "Display a page with version and system attribution information.",
                    true, true, null));
            menuItems.Add(
                new MenuItem(
                    "TermsAndConditions", "Help", MenuItemType.LeafItem, S("Terms and Conditions"),
                    "TermsAndConditions", "Help", "HelpTermsAndConditions",
                     "Display a page with our legal terms and conditions.",
                     true, true, null));
            menuItems.Add(
                new MenuItem(
                    "PrivacyPolicy", "Help", MenuItemType.LeafItem, S("Privacy Policy"),
                    "PrivacyPolicy", "Help", "HelpPrivacyPolicy",
                     "Display a page with our privacy policy.",
                     true, true, null));
            menuItems.Add(
                new MenuItem(
                    "Test", "Help", MenuItemType.LeafItem, S("Test"), "Test", "Help",
                    "HelpTest", "A place-holder for a temporary development test.",
                    ApplicationData.IsTestMobileVersion, true, null));
            MenuItem subMenu = new MenuItem(
                "Help", Key, MenuItemType.SubMenu, S("Help"), "HelpMenu", "Menu",
                "MainHelp", "Display the \"Help\" menu, which displays menu items for help and system information and communication.",
                true, true, menuItems);
            HelpMenu = subMenu;
            return subMenu;
        }

        public void UpdateHelpMenu()
        {
        }

        public string S(string str)
        {
            return LanguageUtilities.TranslateUIString(str);
        }

        public string GetUserOptionString(string key, string defaultValue = null)
        {
            string value = UserProfile.GetUserOptionString(key, defaultValue);
            return value;
        }

        public int GetUserOptionInteger(string key, int defaultValue = 0)
        {
            int value = UserProfile.GetUserOptionInteger(key, defaultValue);
            return value;
        }
    }
}
