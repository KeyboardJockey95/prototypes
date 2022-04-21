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
        public bool AddCourseToPlan(BaseObjectNodeTree course, Dictionary<int, bool> nodeSelectFlags,
            BaseObjectNodeTree plan, BaseObjectNode parentNode, bool useCourseAsPlan,
            MultiLanguageString planTitle, MultiLanguageString planDescription,
            List<LanguageID> hostLanguageIDs, List<LanguageID> targetLanguageIDs,
            NodeMaster planMaster, List<ObjectReferenceNodeTree> planHeaders)
        {
            bool returnValue = true;

            if (plan == null)
            {
                if (useCourseAsPlan)
                {
                    plan = CloneTree(course, true, true, true, true, planTitle, planDescription,
                        hostLanguageIDs, targetLanguageIDs, nodeSelectFlags, null, planHeaders);

                    if (planMaster != null)
                    {
                        plan.MasterReference = new NodeMasterReference(planMaster);
                        SetupNodeFromMaster(plan);
                    }
                }
                else
                {
                    plan = CreateTree(planTitle, planDescription, "Plan", "Plans", UserName,
                        UILanguageID, hostLanguageIDs, targetLanguageIDs);

                    plan.Directory = "Plan_" + plan.ComposeDirectory();

                    EnsureUniqueTree(plan, planHeaders);

                    if (planMaster != null)
                    {
                        plan.MasterReference = new NodeMasterReference(planMaster);
                        SetupNodeFromMaster(plan);
                    }

                    CloneNode(course, plan, null, true, true, true, true, nodeSelectFlags, null);
                }

                returnValue = AddTree(plan, true);
            }
            else
            {
                CloneNode(course, plan, parentNode, true, true, true, true, nodeSelectFlags, null);
                returnValue = UpdateTree(plan, false, true);
            }

            return returnValue;
        }

        public bool AddGroupsAndLessonsToPlan(BaseObjectNode nodeOrTree, Dictionary<int, bool> nodeSelectFlags,
            BaseObjectNodeTree plan, BaseObjectNode parentNode,
            MultiLanguageString planTitle, MultiLanguageString planDescription,
            List<LanguageID> hostLanguageIDs, List<LanguageID> targetLanguageIDs,
            NodeMaster planMaster, List<ObjectReferenceNodeTree> planHeaders)
        {
            bool returnValue = true;

            if (plan == null)
            {
                plan = CreateTree(planTitle, planDescription, "Plan", "Plans", UserName,
                    UILanguageID, hostLanguageIDs, targetLanguageIDs);

                plan.Directory = "Plan_" + plan.ComposeDirectory();

                EnsureUniqueTree(plan, planHeaders);

                if (planMaster != null)
                {
                    plan.MasterReference = new NodeMasterReference(planMaster);
                    SetupNodeFromMaster(plan);
                }

                CloneNodeChildrenAndContent(nodeOrTree, plan, plan, true, true, true, true, nodeSelectFlags, null);

                returnValue = AddTree(plan, true);
            }
            else
            {
                if (parentNode == null)
                    parentNode = plan;

                CloneNodeChildrenAndContent(nodeOrTree, plan, parentNode, true, true, true, true, nodeSelectFlags, null);
                returnValue = UpdateTree(plan, false, true);
            }

            return returnValue;
        }

        public bool AddNodeToPlan(BaseObjectNode node, Dictionary<int, bool> nodeSelectFlags,
            Dictionary<string, bool> contentSelectFlags,
            BaseObjectNodeTree plan, BaseObjectNode parentNode, bool useAsPlan,
            MultiLanguageString planTitle, MultiLanguageString planDescription,
            List<LanguageID> hostLanguageIDs, List<LanguageID> targetLanguageIDs,
            NodeMaster planMaster, List<ObjectReferenceNodeTree> planHeaders)
        {
            bool returnValue = true;

            if (plan == null)
            {
                plan = CreateTree(planTitle, planDescription, "Plan", "Plans", UserName,
                    UILanguageID, hostLanguageIDs, targetLanguageIDs);

                plan.Directory = "Plan_" + plan.ComposeDirectory();

                EnsureUniqueTree(plan, planHeaders);

                if (planMaster != null)
                {
                    plan.MasterReference = new NodeMasterReference(planMaster);
                    SetupNodeFromMaster(plan);
                }

                if (useAsPlan)
                    CloneNodeChildrenAndContent(node, plan, plan, true, true, true, true,
                        nodeSelectFlags, contentSelectFlags);
                else
                    CloneNode(node, plan, null, true, true, true, true, nodeSelectFlags, contentSelectFlags);

                returnValue = AddTree(plan, true);
            }
            else
            {
                CloneNode(node, plan, parentNode, true, true, true, true, nodeSelectFlags, contentSelectFlags);
                returnValue = UpdateTree(plan, false, true);
            }

            return returnValue;
        }

        public bool AddNodeContentToPlan(BaseObjectNode node,
            Dictionary<string, bool> contentSelectFlags,
            BaseObjectNodeTree plan, BaseObjectNode parentNode,
            MultiLanguageString planTitle, MultiLanguageString planDescription,
            List<LanguageID> hostLanguageIDs, List<LanguageID> targetLanguageIDs,
            NodeMaster planMaster, List<ObjectReferenceNodeTree> planHeaders)
        {
            bool returnValue = true;

            if (plan == null)
            {
                plan = CreateTree(planTitle, planDescription, "Plan", "Plans", UserName,
                    UILanguageID, hostLanguageIDs, targetLanguageIDs);

                plan.Directory = "Plan_" + plan.ComposeDirectory();

                EnsureUniqueTree(plan, planHeaders);

                if (planMaster != null)
                {
                    plan.MasterReference = new NodeMasterReference(planMaster);
                    SetupNodeFromMaster(plan);
                }

                CloneNodeChildrenAndContent(node, plan, parentNode, true, false, true, true,
                    null, contentSelectFlags);

                returnValue = AddTree(plan, true);
            }
            else
            {
                CloneNodeChildrenAndContent(node, plan, parentNode, true, false, true, true,
                    null, contentSelectFlags);

                returnValue = UpdateTree(plan, false, true);
            }

            return returnValue;
        }

        public bool AddContentToPlan(BaseObjectContent sourceContent, Dictionary<string, bool> contentSelectFlags,
            BaseObjectNodeTree plan, BaseObjectNode parentNode, BaseObjectContent parentContent,
            MultiLanguageString planTitle, MultiLanguageString planDescription,
            List<LanguageID> hostLanguageIDs, List<LanguageID> targetLanguageIDs,
            NodeMaster planMaster, List<ObjectReferenceNodeTree> planHeaders)
        {
            bool returnValue = true;

            if (plan == null)
            {
                plan = CreateTree(planTitle, planDescription, "Plan", "Plans", UserName,
                    UILanguageID, hostLanguageIDs, targetLanguageIDs);

                plan.Directory = "Plan_" + plan.ComposeDirectory();

                EnsureUniqueTree(plan, planHeaders);

                if (planMaster != null)
                {
                    plan.MasterReference = new NodeMasterReference(planMaster);
                    SetupNodeFromMaster(plan);
                }

                BaseObjectContent content = CloneContent(sourceContent, plan, plan,
                    true, true, true, contentSelectFlags);

                returnValue = AddTree(plan, true);
            }
            else if (parentContent != null)
            {
                BaseObjectContent content = CloneSubContent(sourceContent, plan, parentContent,
                    true, true, true, contentSelectFlags);

                returnValue = UpdateTree(plan, false, true);
            }
            else
            {
                BaseObjectContent content = CloneContent(sourceContent, plan, parentNode,
                    true, true, true, contentSelectFlags);

                returnValue = UpdateTree(plan, false, true);
            }

            return returnValue;
        }

        public bool AddStudyItemsToPlan(ContentStudyList sourceStudyList,
            List<bool> itemSelectFlags, BaseObjectNodeTree plan, BaseObjectContent planContent,
            ref ContentStudyList planStudyList)
        {
            bool returnValue = true;

            if (planContent.IsReference)
            {
                bool wasConverted;
                string errorMessage;

                if (!ConvertContentToReference(planContent, ref planStudyList, out wasConverted, out errorMessage))
                {
                    Error = errorMessage;
                    return false;
                }

                if (!UpdateTree(plan, false, true))
                    return false;
            }

            returnValue = planStudyList.CloneStudyItemsSelected(sourceStudyList, itemSelectFlags, true);

            if (returnValue)
            {
                if (!UpdateContentStorage(planContent, true))
                    returnValue = false;
            }

            return returnValue;
        }
    }
}
