using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Node;
using JTLanguageModelsPortable.RepositoryInterfaces;

namespace JTLanguageModelsPortable.Content
{
    public class MultiLanguageItemReference : ContentItemReference<MultiLanguageItem>
    {
        public MultiLanguageItemReference(object key, object itemKey, object contentKey, string nodeContentKey,
            object nodeKey, object treeKey, BaseObjectNode node, BaseObjectNodeTree tree, MultiLanguageItem item)
            : base(key, itemKey, contentKey, nodeContentKey, nodeKey, treeKey, node, tree, item)
        {
        }

        public MultiLanguageItemReference(object key, object itemKey, object contentKey, string nodeContentKey,
            object nodeKey, object treeKey, MultiLanguageItem item)
            : base(key, itemKey, contentKey, nodeContentKey, nodeKey, treeKey, item)
        {
        }

        public MultiLanguageItemReference(object key, object contentKey, string nodeContentKey,
                object nodeKey, MultiLanguageItem item)
            : base(key, item.Key, contentKey, nodeContentKey, nodeKey, null, item)
        {
        }

        public MultiLanguageItemReference(MultiLanguageItemReference other)
            : base(other)
        {
        }

        public MultiLanguageItemReference(XElement element)
            : base(element)
        {
        }

        public MultiLanguageItemReference()
        {
        }

        public void ResolveReference(IMainRepository mainRepository,
            ref Dictionary<object, ContentStudyList> studyListCache,
            ref Dictionary<object, BaseObjectNodeTree> treeCache)
        {
            if (((_Item == null) || (_Tree == null)) && (_ItemKey != null))
            {
                if (_ContentKey != null)
                {
                    BaseObjectNodeTree tree = null;
                    BaseObjectNode node = null;
                    BaseObjectContent content = null;
                    ContentStudyList studyList = null;

                    if (studyListCache != null)
                    {
                        if (!studyListCache.TryGetValue(_ContentKey, out studyList))
                        {
                            studyList = mainRepository.StudyLists.Get(_ContentKey);

                            if (studyList != null)
                                studyListCache.Add(studyList.Key, studyList);
                        }
                    }
                    else
                    {
                        studyListCache = new Dictionary<object, ContentStudyList>();
                        studyList = mainRepository.StudyLists.Get(_ContentKey);

                        if (studyList != null)
                            studyListCache.Add(studyList.Key, studyList);
                    }

                    if (studyList != null)
                    {
                        _Item = studyList.GetStudyItem(_ItemKey);

                        content = studyList.Content;

                        if (content == null)
                        {
                            if (_TreeKey != null)
                            {
                                if (treeCache != null)
                                {
                                    if (!treeCache.TryGetValue(_TreeKey, out tree))
                                    {
                                        tree = mainRepository.ResolveReference("Courses", null, _TreeKey) as BaseObjectNodeTree;
                                        if (tree != null)
                                            treeCache.Add(tree.Key, tree);
                                    }
                                }
                                else
                                {
                                    treeCache = new Dictionary<object, BaseObjectNodeTree>();
                                    tree = mainRepository.ResolveReference("Courses", null, _TreeKey) as BaseObjectNodeTree;

                                    if (tree != null)
                                        treeCache.Add(tree.Key, tree);
                                }
                                _Tree = tree;
                            }

                            if (tree != null)
                            {
                                if (_NodeKey != null)
                                {
                                    node = tree.GetNode(_NodeKey);

                                    if (node != null)
                                        node.Tree = tree;

                                    _Node = node;
                                }
                            }

                            if (_NodeContentKey != null)
                            {
                                if (node != null)
                                {
                                    content = node.GetContent(_NodeContentKey);

                                    if (content != null)
                                    {
                                        content.Node = node;
                                        studyList.Content = content;
                                    }
                                }
                            }
                        }
                        else
                        {
                            _Node = content.Node;
                            _Tree = content.Tree;
                        }
                    }
                }
            }
        }

        public bool SaveReference(IMainRepository mainRepository)
        {
            return false;
        }

        public bool UpdateReference(IMainRepository mainRepository)
        {
            return false;
        }

        public bool UpdateReferenceCheck(IMainRepository mainRepository)
        {
            return false;
        }

        public void ClearReference()
        {
            _Item = null;
        }

        public static int CompareMultiLanguageItemReference(MultiLanguageItemReference object1, MultiLanguageItemReference object2)
        {
            if (((object)object1 == null) && ((object)object2 == null))
                return 0;
            if ((object)object1 == null)
                return -1;
            if ((object)object2 == null)
                return 1;
            return object1.Compare(object2);
        }

        public static int CompareMultiLanguageItemReferenceLists(List<MultiLanguageItemReference> list1, List<MultiLanguageItemReference> list2)
        {
            return ObjectUtilities.CompareTypedObjectLists<MultiLanguageItemReference>(list1, list2);
        }
    }
}
