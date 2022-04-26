using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Admin;

namespace JTLanguageModelsPortable.Repository
{
    public class UserRecordRepository : BaseRepository<UserRecord>
    {
        public UserRecordRepository(IObjectStore objectStore) : base(objectStore) { }
        public List<string> TeacherRoles = new List<string>(2) { "teacher", "administrator" };

        public List<UserRecord> GetTeachersList(string targetLanguageCode, UserRecord userRecord, UserProfile userProfile)
        {
            List<UserRecord> teachers = null;

            switch (targetLanguageCode)
            {
                case "(my languages)":
                    teachers = GetTeachers(userProfile.LanguageIDs);
                    break;
                case "(target languages)":
                    teachers = GetTeachers(userProfile.TargetLanguageIDs);
                    break;
                case "(host languages)":
                    teachers = GetTeachers(userProfile.HostLanguageIDs);
                    break;
                case null:
                case "":
                case "(any)":
                case "(any language)":
                case "(all languages)":
                    teachers = GetRoleUsers(TeacherRoles);
                    break;
                default:
                    teachers = GetTeachers(LanguageLookup.GetLanguageIDNoAdd(targetLanguageCode));
                    break;
            }
            if (teachers != null)
                teachers.Sort(ObjectUtilities.CompareKeys);
            else
                teachers = new List<UserRecord>();
            int count = teachers.Count();
            int index;
            for (index = count - 1; index >= 0; index--)
            {
                UserRecord teacher = teachers[index];
                if (!teacher.IsHidden)
                    continue;
                if (teacher.UserName == userRecord.UserName)
                    continue;
                if ((teacher.Packages != null) && (userRecord.Packages != null))
                {
                    bool found = false;
                    foreach (string package in userRecord.Packages)
                    {
                        if (teacher.Packages.Contains(package))
                            found = true;
                    }
                    if (found)
                        continue;
                }
                teachers.RemoveAt(index);
            }
            return teachers;
        }

        public List<UserRecord> GetStudents(LanguageID languageID)
        {
            StringMatcher userRoleMatcher = new StringMatcher("student", "UserRole", MatchCode.Exact, 0, 0);
            StringMatcher languageMatcher = new StringMatcher(languageID.LanguageCultureExtensionCode, "LanguagesKey", MatchCode.Contains, 0, 0);
            CompoundMatcher matcher = new CompoundMatcher(new List<Matcher> { userRoleMatcher, languageMatcher }, null, MatchCode.And, 0, 0);
            List<UserRecord> lookupQuery = Query(matcher);
            return lookupQuery;
        }

        public List<UserRecord> GetStudents(List<LanguageID> languageIDs)
        {
            StringMatcher userRoleMatcher = new StringMatcher("student", "UserRole", MatchCode.Exact, 0, 0);
            LanguageIDMatcher languageMatcher = new LanguageIDMatcher(languageIDs, "|", "LanguagesKey", MatchCode.Contains, 0, 0);
            CompoundMatcher matcher = new CompoundMatcher(new List<Matcher> { userRoleMatcher, languageMatcher }, null, MatchCode.And, 0, 0);
            List<UserRecord> lookupQuery = Query(matcher);
            return lookupQuery;
        }

        public List<UserRecord> GetTeachers()
        {
            StringMatcher userRoleMatcher = new StringMatcher(TeacherRoles, "UserRole", MatchCode.Exact, 0, 0);
            List<UserRecord> lookupQuery = Query(userRoleMatcher);
            return lookupQuery;
        }

        public List<UserRecord> GetTeachers(LanguageID languageID)
        {
            StringMatcher userRoleMatcher = new StringMatcher(TeacherRoles, "UserRole", MatchCode.Exact, 0, 0);
            StringMatcher languageMatcher = new StringMatcher(languageID.LanguageCultureExtensionCode, "LanguagesKey", MatchCode.Contains, 0, 0);
            CompoundMatcher matcher = new CompoundMatcher(new List<Matcher> { userRoleMatcher, languageMatcher }, null, MatchCode.And, 0, 0);
            List<UserRecord> lookupQuery = Query(matcher);
            return lookupQuery;
        }

        public List<UserRecord> GetTeachers(List<LanguageID> languageIDs)
        {
            StringMatcher userRoleMatcher = new StringMatcher(TeacherRoles, "UserRole", MatchCode.Exact, 0, 0);
            LanguageIDMatcher languageMatcher = new LanguageIDMatcher(languageIDs, "|", "LanguagesKey", MatchCode.Contains, 0, 0);
            CompoundMatcher matcher = new CompoundMatcher(new List<Matcher> { userRoleMatcher, languageMatcher }, null, MatchCode.And, 0, 0);
            List<UserRecord> lookupQuery = Query(matcher);
            return lookupQuery;
        }

        public List<UserRecord> GetRoleUsers(string role)
        {
            StringMatcher matcher = new StringMatcher(role, "UserRole", MatchCode.Exact, 0, 0);
            List<UserRecord> lookupQuery = Query(matcher);
            return lookupQuery;
        }

        public List<UserRecord> GetRoleUsers(List<string> roles)
        {
            StringMatcher matcher = new StringMatcher(roles, "UserRole", MatchCode.Exact, 0, 0);
            List<UserRecord> lookupQuery = Query(matcher);
            return lookupQuery;
        }

        public List<UserRecord> GetAdministrators()
        {
            return GetRoleUsers("administrator");
        }

        public List<UserRecord> GetUsers(List<string> userNames)
        {
            List<UserRecord> users = new List<UserRecord>(userNames.Count());

            foreach (string userName in userNames)
            {
                UserRecord user = Get(userName);

                if (user != null)
                    users.Add(user);
            }

            return users;
        }

        public List<string> GetUserNames(string role)
        {
            StringMatcher matcher = new StringMatcher(role, "UserRole", MatchCode.Exact, 0, 0);
            List<UserRecord> lookupQuery = Query(matcher);
            List<string> userNames = new List<string>();
            if (lookupQuery != null)
            {
                foreach (UserRecord user in lookupQuery)
                    userNames.Add(user.UserName);
            }
            return userNames;
        }

        public List<string> GetAllUserNames()
        {
            List<UserRecord> userRecords = GetAll();
            List<string> userNames = new List<string>();
            if (userRecords != null)
            {
                foreach (UserRecord user in userRecords)
                    userNames.Add(user.UserName);
            }
            return userNames;
        }
    }

    public class AnonymousUserRecordRepository : BaseRepository<AnonymousUserRecord>
    {
        public AnonymousUserRecordRepository(IObjectStore objectStore) : base(objectStore) { }
    }
}
