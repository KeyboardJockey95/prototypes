using System;
using System.Collections.Generic;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Tool;

namespace JTLanguageModelsPortable.Application
{
    public class ApplicationCookies : IApplicationCookies
    {
        private readonly ICookieContainer Cookies;

        public ApplicationCookies(ICookieContainer cookies)
        {
            Cookies = cookies;
        }

        public ICookieContainer Container
        {
            get
            {
                return Cookies;
            }
        }

        /* Save for example.
        public string AnonymousID
        {
            get { return Cookies.GetValue("AnonymousID"); }
            set { Cookies.SetValue("AnonymousID", value, DateTime.UtcNow.AddDays(365)); }
        }
        */

        public string UserName
        {
            get { return Cookies.GetValue("UserName"); }
            set { Cookies.SetValue("UserName", value, DateTime.UtcNow.AddDays(365)); }
        }

        public string UserProfileName
        {
            get { return Cookies.GetValue("UserProfileName"); }
            set { Cookies.SetValue("UserProfileName", value, DateTime.UtcNow.AddDays(365)); }
        }

        public UserProfile UserProfile
        {
            get
            {
                try
                {
                    string data = Cookies.RawCookieGetValue("UserProfile");
                    if (!String.IsNullOrEmpty(data))
                    {
                        UserProfile userProfile = new UserProfile();
                        byte[] binData = ObjectUtilities.GetByteArrayFromDataString(data);
                        userProfile.BinaryData = binData;
                        return userProfile;
                    }
                }
                catch
                {
                    Cookies.RawCookieSetValue("UserProfile", String.Empty, DateTime.UtcNow.AddDays(365));
                }
                return null;
            }
            set
            {
                string data;
                if (value != null)
                {
                    byte[] binData = value.BinaryData;
                    data = ObjectUtilities.GetDataStringFromByteArray(binData, true);
                }
                else
                    data = "";
                Cookies.RawCookieSetValue("UserProfile", data, DateTime.UtcNow.AddDays(365));
            }
        }

        public bool ShowFieldHelp
        {
            get { return Cookies.GetValue("ShowFieldHelp") == "true"; }
            set { Cookies.SetValue("ShowFieldHelp", (value ? "true" : "false"), DateTime.UtcNow.AddDays(365)); }
        }

        public string CurrentUrl
        {
            get { return Cookies.GetValue("CurrentUrl"); }
            set { Cookies.SetValue("CurrentUrl", value, DateTime.UtcNow.AddDays(365)); }
        }

        /*
        public int CoursesTreeKey
        {
            get { return Cookies.GetValue<int>("CoursesTreeKey"); }
            set { Cookies.SetValue("CoursesTreeKey", value, DateTime.UtcNow.AddDays(365)); }
        }

        public int CoursesGroupKey
        {
            get { return Cookies.GetValue<int>("CoursesGroupKey"); }
            set { Cookies.SetValue("CoursesGroupKey", value, DateTime.UtcNow.AddDays(365)); }
        }

        public int CoursesNodeKey
        {
            get { return Cookies.GetValue<int>("CoursesNodeKey"); }
            set { Cookies.SetValue("CoursesNodeKey", value, DateTime.UtcNow.AddDays(365)); }
        }

        public string CoursesContentKey
        {
            get { return Cookies.GetValue("CoursesContentKey"); }
            set { Cookies.SetValue("CoursesContentKey", value, DateTime.UtcNow.AddDays(365)); }
        }

        public string CoursesStudyContentKey
        {
            get { return Cookies.GetValue("CoursesStudyContentKey"); }
            set { Cookies.SetValue("CoursesStudyContentKey", value, DateTime.UtcNow.AddDays(365)); }
        }

        public string CoursesNavState
        {
            get { return Cookies.GetValue("CoursesNavState"); }
            set { Cookies.SetValue("CoursesNavState", value, DateTime.UtcNow.AddDays(365)); }
        }

        public string CoursesActiveNode
        {
            get { return Cookies.GetValue("CoursesActiveNode"); }
            set { Cookies.SetValue("CoursesActiveNode", value, DateTime.UtcNow.AddDays(365)); }
        }

        public int PlansTreeKey
        {
            get { return Cookies.GetValue<int>("PlansTreeKey"); }
            set { Cookies.SetValue("PlansTreeKey", value, DateTime.UtcNow.AddDays(365)); }
        }

        public int PlansGroupKey
        {
            get { return Cookies.GetValue<int>("PlansGroupKey"); }
            set { Cookies.SetValue("PlansGroupKey", value, DateTime.UtcNow.AddDays(365)); }
        }

        public int PlansNodeKey
        {
            get { return Cookies.GetValue<int>("PlansNodeKey"); }
            set { Cookies.SetValue("PlansNodeKey", value, DateTime.UtcNow.AddDays(365)); }
        }

        public string PlansContentKey
        {
            get { return Cookies.GetValue("PlansContentKey"); }
            set { Cookies.SetValue("PlansContentKey", value, DateTime.UtcNow.AddDays(365)); }
        }

        public string PlansStudyContentKey
        {
            get { return Cookies.GetValue("PlansStudyContentKey"); }
            set { Cookies.SetValue("PlansStudyContentKey", value, DateTime.UtcNow.AddDays(365)); }
        }

        public string PlansNavState
        {
            get { return Cookies.GetValue("PlansNavState"); }
            set { Cookies.SetValue("PlansNavState", value, DateTime.UtcNow.AddDays(365)); }
        }

        public string PlansActiveNode
        {
            get { return Cookies.GetValue("PlansActiveNode"); }
            set { Cookies.SetValue("PlansActiveNode", value, DateTime.UtcNow.AddDays(365)); }
        }

        public int StudySessionIndex
        {
            get { return Cookies.GetValue<int>("StudySessionIndex"); }
            set { Cookies.SetValue("StudySessionIndex", value, DateTime.UtcNow.AddDays(365)); }
        }
        */
    }
}
