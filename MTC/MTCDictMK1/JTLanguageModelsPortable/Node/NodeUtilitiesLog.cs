using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Repository;
using JTLanguageModelsPortable.RepositoryInterfaces;
using JTLanguageModelsPortable.Content;
using JTLanguageModelsPortable.Converters;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Tool;
using JTLanguageModelsPortable.Formats;
using JTLanguageModelsPortable.Language;
using JTLanguageModelsPortable.MediaInterfaces;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Markup;
using JTLanguageModelsPortable.Master;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Admin;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Node
{
    public partial class NodeUtilities : ControllerUtilities
    {
        public bool LogStudyItemChange(
            int treeKey,
            int nodeKey,
            string contentKey,
            ContentStudyList studyList,
            EditAction undoEditAction,
            EditAction redoEditAction)
        {
            ChangeLogItem changeLogItem = new ChangeLogItem(
                UserName,
                UserProfile.ProfileName,
                DateTime.UtcNow,
                treeKey,
                nodeKey,
                contentKey,
                studyList.KeyInt,
                undoEditAction,
                redoEditAction);
            if (!Repositories.ChangeLogItems.Add(changeLogItem))
            {
                PutError("Error adding change log item.");
                return false;
            }

            return true;
        }
    }
}
