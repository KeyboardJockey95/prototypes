using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Media;

namespace JTLanguageModelsPortable.Admin
{
    public class AnonymousUserRecord : UserRecord
    {
        public AnonymousUserRecord(
                string userName,
                string realName,
                string email,
                string userRole,
                UserStanding standing,
                LanguageDescriptor uiLanguageDescriptor,
                List<LanguageDescriptor> hostLanguageDescriptors,
                List<LanguageDescriptor> targetLanguageDescriptors,
                string profileImageFileName,
                MultiLanguageString title,
                MultiLanguageString description,
                MultiLanguageString aboutMe,
                bool isMinor)
            : base(userName, realName, email, userRole, standing,
                uiLanguageDescriptor, hostLanguageDescriptors, targetLanguageDescriptors, profileImageFileName,
                title, description, aboutMe, isMinor)
        {
        }

        public AnonymousUserRecord(
                string userName,
                List<LanguageID> targetLanguageIDs,
                List<LanguageID> hostLanguageIDs,
                LanguageID uiLanguageID)
            : base(userName, targetLanguageIDs, hostLanguageIDs, uiLanguageID)
        {
        }

        public AnonymousUserRecord(string userName)
            : base(userName)
        {
            ClearUserRecord();
        }

        public AnonymousUserRecord(UserRecord other)
            : base(other)
        {
        }

        public AnonymousUserRecord(XElement element)
            : base(element)
        {
        }

        public AnonymousUserRecord()
        {
        }

        public override bool IsAnonymous()
        {
            return true;
        }

        public override string UserNameOrGuest
        {
            get
            {
                return "Guest";
            }
        }
    }
}
