using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.Content;

namespace JTLanguageModelsPortable.Node
{
    public class ItemWalker<Context>
    {
        public delegate bool VisitTreeDelegate(BaseObjectNodeTree tree, ItemWalker<Context> walker, Context context);
        public delegate bool VisitNodeDelegate(BaseObjectNode node, ItemWalker<Context> walker, Context context);
        public delegate bool VisitContentDelegate(BaseObjectContent content, ItemWalker<Context> walker, Context context);
        public delegate bool VisitDocumentItemDelegate(ContentDocumentItem documentItem, ItemWalker<Context> walker, Context context);
        public delegate bool VisitMediaItemDelegate(ContentMediaItem mediaItem, ItemWalker<Context> walker, Context context);
        public delegate bool VisitStudyListDelegate(ContentStudyList studyList, ItemWalker<Context> walker, Context context);
        public delegate bool VisitStudyItemDelegate(MultiLanguageItem studyitem, ItemWalker<Context> walker, Context context);
        public delegate bool VisitLanguageItemDelegate(LanguageItem languageitem, ItemWalker<Context> walker, Context context);
        public delegate bool VisitSentenceRunDelegate(TextRun sentenceRun, ItemWalker<Context> walker, Context context);
        public delegate bool VisitWordRunDelegate(TextRun wordRun, ItemWalker<Context> walker, Context context);
        public delegate bool VisitMediaRunDelegate(MediaRun mediaRun, ItemWalker<Context> walker, Context context);

        public VisitTreeDelegate VisitTreeFunction;
        public VisitNodeDelegate VisitNodeFunction;
        public VisitContentDelegate VisitContentFunction;
        public VisitDocumentItemDelegate VisitDocumentItemFunction;
        public VisitMediaItemDelegate VisitMediaItemFunction;
        public VisitStudyListDelegate VisitStudyListFunction;
        public VisitStudyListDelegate VisitStudyListPostFunction;
        public VisitStudyItemDelegate VisitStudyItemFunction;
        public VisitLanguageItemDelegate VisitLanguageItemFunction;
        public VisitSentenceRunDelegate VisitSentenceRunFunction;
        public VisitWordRunDelegate VisitWordRunFunction;
        public VisitMediaRunDelegate VisitMediaRunFunction;

        public BaseObjectNodeTree Tree;
        public BaseObjectNode Node;
        public BaseObjectContent Content;
        public ContentDocumentItem DocumentItem;
        public ContentMediaItem MediaItem;
        public ContentStudyList StudyList;
        public MultiLanguageItem StudyItem;
        public LanguageItem LanguageItem;
        public TextRun SentenceRun;
        public TextRun WordRun;
         

        public ItemWalker()
        {
            ClearAll();
        }

        public void ClearAll()
        {
            ClearFunctions();
            ClearCurrent();
        }

        public void ClearFunctions()
        {
            VisitTreeFunction = null;
            VisitNodeFunction = null;
            VisitContentFunction = null;
            VisitDocumentItemFunction = null;
            VisitMediaItemFunction = null;
            VisitStudyListFunction = null;
            VisitStudyListPostFunction = null;
            VisitStudyItemFunction = null;
            VisitLanguageItemFunction = null;
            VisitSentenceRunFunction = null;
            VisitWordRunFunction = null;
            VisitMediaRunFunction = null;
        }

        public void ClearCurrent()
        {
            Tree = null;
            Node = null;
            Content = null;
            DocumentItem = null;
            MediaItem = null;
            StudyList = null;
            StudyItem = null;
            LanguageItem = null;
            SentenceRun = null;
            WordRun = null;
        }

        public bool WalkTree(BaseObjectNodeTree tree, Context context)
        {
            if (tree == null)
                return false;

            BaseObjectNodeTree saveTree = Tree;
            Tree = tree;

            bool returnValue = VisitTree(tree, context);

            if (!WalkNodeChildren(tree, context))
                returnValue = false;

            if (!WalkNodeContent(tree, context))
                returnValue = false;

            Tree = saveTree;

            return returnValue;
        }

        public bool WalkNode(BaseObjectNode node, Context context)
        {
            if (node == null)
                return false;

            BaseObjectNodeTree saveTree = Tree;
            BaseObjectNode saveNode = Node;
            Tree = node.Tree;
            Node = node;

            bool returnValue = VisitNode(node, context);

            if (!WalkNodeChildren(node, context))
                returnValue = false;

            if (!WalkNodeContent(node, context))
                returnValue = false;

            Tree = saveTree;
            Node = saveNode;

            return returnValue;
        }

        public bool WalkNodeChildren(BaseObjectNode node, Context context)
        {
            if (node == null)
                return false;

            bool returnValue = true;

            if (node.HasChildren())
            {
                BaseObjectNodeTree saveTree = Tree;
                BaseObjectNode saveNode = Node;
                Tree = node.Tree;
                Node = node;

                foreach (BaseObjectNode childNode in node.Children)
                {
                    if (!WalkNode(childNode, context))
                        returnValue = false;
                }

                Tree = saveTree;
                Node = saveNode;
            }

            return returnValue;
        }

        public bool WalkNodeContent(BaseObjectNode node, Context context)
        {
            if (node == null)
                return false;

            bool returnValue = true;

            if (node.HasContent())
            {
                BaseObjectNodeTree saveTree = Tree;
                BaseObjectNode saveNode = Node;
                Tree = node.Tree;
                Node = node;

                foreach (BaseObjectContent content in node.ContentChildren)
                {
                    if (!WalkContent(content, context))
                        returnValue = false;
                }

                Tree = saveTree;
                Node = saveNode;
            }

            return returnValue;
        }

        public bool WalkContent(BaseObjectContent content, Context context)
        {
            if (content == null)
                return false;

            BaseObjectContent saveContent = Content;
            Content = content;

            bool returnValue = VisitContent(content, context);

            if (content.ContentStorageDocumentItem != null)
            {
                if (!WalkDocumentItem(content.ContentStorageDocumentItem, context))
                    returnValue = false;

            }

            if (content.ContentStorageMediaItem != null)
            {
                if (!WalkMediaItem(content.ContentStorageMediaItem, context))
                    returnValue = false;

            }

            if (content.ContentStorageStudyList != null)
            {
                if (!WalkStudyList(content.ContentStorageStudyList, context))
                    returnValue = false;

            }

            if (content.HasContent())
            {
                foreach (BaseObjectContent contentChild in content.ContentChildren)
                {
                    if (contentChild.KeyString == content.KeyString)
                        continue;

                    if (!WalkContent(contentChild, context))
                        returnValue = false;
                }
            }

            Content = saveContent;

            return returnValue;
        }

        public bool WalkDocumentItem(ContentDocumentItem documentItem, Context context)
        {
            if (documentItem == null)
                return false;

            ContentDocumentItem saveDocumentItem = DocumentItem;
            DocumentItem = documentItem;

            bool returnValue = VisitDocumentItem(documentItem, context);

            DocumentItem = saveDocumentItem;

            return returnValue;
        }

        public bool WalkMediaItem(ContentMediaItem mediaItem, Context context)
        {
            if (mediaItem == null)
                return false;

            ContentMediaItem saveMediaItem = MediaItem;
            MediaItem = mediaItem;

            bool returnValue = VisitMediaItem(mediaItem, context);

            MediaItem = saveMediaItem;

            return returnValue;
        }

        public bool WalkStudyList(ContentStudyList studyList, Context context)
        {
            if (studyList == null)
                return false;

            ContentStudyList saveStudyList = StudyList;
            StudyList = studyList;

            bool returnValue = VisitStudyList(studyList, context);

            if (studyList.StudyItems != null)
            {
                foreach (MultiLanguageItem studyItem in studyList.StudyItems)
                {
                    if (!WalkStudyItem(studyItem, context))
                        returnValue = false;
                }
            }

            if (!VisitStudyListPost(studyList, context))
                returnValue = false;

            StudyList = saveStudyList;

            return returnValue;
        }

        public bool WalkStudyItem(MultiLanguageItem studyItem, Context context)
        {
            if (studyItem == null)
                return false;

            MultiLanguageItem saveStudyItem = StudyItem;
            StudyItem = studyItem;

            bool returnValue = VisitStudyItem(studyItem, context);

            if (studyItem.LanguageItems != null)
            {
                foreach (LanguageItem languageItem in studyItem.LanguageItems)
                {
                    if (!WalkLanguageItem(languageItem, context))
                        returnValue = false;
                }
            }

            StudyItem = saveStudyItem;

            return returnValue;
        }

        public bool WalkLanguageItem(LanguageItem languageItem, Context context)
        {
            if (languageItem == null)
                return false;

            LanguageItem saveLanguageItem = LanguageItem;
            LanguageItem = languageItem;

            bool returnValue = VisitLanguageItem(languageItem, context);

            if (languageItem.SentenceRuns != null)
            {
                foreach (TextRun sentenceRun in languageItem.SentenceRuns)
                {
                    if (!WalkSentenceRun(sentenceRun, context))
                        returnValue = false;
                }
            }

            if (languageItem.WordRuns != null)
            {
                foreach (TextRun wordRun in languageItem.WordRuns)
                {
                    if (!WalkWordRun(wordRun, context))
                        returnValue = false;
                }
            }

            LanguageItem = saveLanguageItem;

            return returnValue;
        }

        public bool WalkSentenceRun(TextRun sentenceRun, Context context)
        {
            if (sentenceRun == null)
                return false;

            TextRun saveSentenceRun = SentenceRun;
            SentenceRun = sentenceRun;

            bool returnValue = VisitSentenceRun(sentenceRun, context);

            if (sentenceRun.MediaRuns != null)
            {
                foreach (MediaRun mediaRun in sentenceRun.MediaRuns)
                {
                    if (!VisitMediaRun(mediaRun, context))
                        returnValue = false;
                }
            }

            SentenceRun = saveSentenceRun;

            return returnValue;
        }

        public bool WalkWordRun(TextRun wordRun, Context context)
        {
            if (wordRun == null)
                return false;

            TextRun saveWordRun = WordRun;
            WordRun = wordRun;

            bool returnValue = VisitWordRun(wordRun, context);

            if (wordRun.MediaRuns != null)
            {
                foreach (MediaRun mediaRun in wordRun.MediaRuns)
                {
                    if (!VisitMediaRun(mediaRun, context))
                        returnValue = false;
                }
            }

            WordRun = saveWordRun;

            return returnValue;
        }

        public virtual bool VisitTree(BaseObjectNodeTree tree, Context context)
        {
            bool returnValue = true;

            if (VisitTreeFunction != null)
                returnValue = VisitTreeFunction(tree, this, context);

            return returnValue;
        }

        public virtual bool VisitNode(BaseObjectNode node, Context context)
        {
            bool returnValue = true;

            if (VisitNodeFunction != null)
                returnValue = VisitNodeFunction(node, this, context);

            return returnValue;
        }

        public virtual bool VisitContent(BaseObjectContent content, Context context)
        {
            bool returnValue = true;

            if (VisitContentFunction != null)
                returnValue = VisitContentFunction(content, this, context);

            return returnValue;
        }

        public virtual bool VisitDocumentItem(ContentDocumentItem documentItem, Context context)
        {
            bool returnValue = true;

            if (VisitDocumentItemFunction != null)
                returnValue = VisitDocumentItemFunction(documentItem, this, context);

            return returnValue;
        }

        public virtual bool VisitMediaItem(ContentMediaItem mediaItem, Context context)
        {
            bool returnValue = true;

            if (VisitMediaItemFunction != null)
                returnValue = VisitMediaItemFunction(mediaItem, this, context);

            return returnValue;
        }

        public virtual bool VisitStudyList(ContentStudyList studyList, Context context)
        {
            bool returnValue = true;

            if (VisitStudyListFunction != null)
                returnValue = VisitStudyListFunction(studyList, this, context);

            return returnValue;
        }

        public virtual bool VisitStudyListPost(ContentStudyList studyList, Context context)
        {
            bool returnValue = true;

            if (VisitStudyListPostFunction != null)
                returnValue = VisitStudyListPostFunction(studyList, this, context);

            return returnValue;
        }

        public virtual bool VisitStudyItem(MultiLanguageItem studyitem, Context context)
        {
            bool returnValue = true;

            if (VisitStudyItemFunction != null)
                returnValue = VisitStudyItemFunction(studyitem, this, context);

            return returnValue;
        }

        public virtual bool VisitLanguageItem(LanguageItem languageitem, Context context)
        {
            bool returnValue = true;

            if (VisitLanguageItemFunction != null)
                returnValue = VisitLanguageItemFunction(languageitem, this, context);

            return returnValue;
        }

        public virtual bool VisitSentenceRun(TextRun sentenceRun, Context context)
        {
            bool returnValue = true;

            if (VisitSentenceRunFunction != null)
                returnValue = VisitSentenceRunFunction(sentenceRun, this, context);

            return returnValue;
        }

        public virtual bool VisitWordRun(TextRun wordRun, Context context)
        {
            bool returnValue = true;

            if (VisitWordRunFunction != null)
                returnValue = VisitWordRunFunction(wordRun, this, context);

            return returnValue;
        }

        public virtual bool VisitMediaRun(MediaRun mediaRun, Context context)
        {
            bool returnValue = true;

            if (VisitMediaRunFunction != null)
                returnValue = VisitMediaRunFunction(mediaRun, this, context);

            return returnValue;
        }
    }
}
