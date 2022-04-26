using System;
using System.Collections.Generic;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Tool;

namespace JTLanguageModelsPortable.Application
{
    public interface IApplicationCookies
    {
        ICookieContainer Container { get; }
        string UserName { get; set; }
        string UserProfileName { get; set; }
        UserProfile UserProfile { get; set; }
        bool ShowFieldHelp { get; set; }
        string CurrentUrl { get; set; }
        /*
        int CoursesTreeKey { get; set; }
        int CoursesGroupKey { get; set; }
        int CoursesNodeKey { get; set; }
        string CoursesContentKey { get; set; }
        string CoursesStudyContentKey { get; set; }
        string CoursesNavState { get; set; }
        string CoursesActiveNode { get; set; }
        int PlansTreeKey { get; set; }
        int PlansGroupKey { get; set; }
        int PlansNodeKey { get; set; }
        string PlansContentKey { get; set; }
        string PlansStudyContentKey { get; set; }
        string PlansNavState { get; set; }
        string PlansActiveNode { get; set; }
        int StudySessionIndex { get; set; }
        */
    }
}
