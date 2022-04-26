using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace JTLanguageModelsPortable.ObjectInterfaces
{
    public delegate bool VisitMedia(
        List<string> mediaFiles,
        object content,
        object ownerObject,
        string filePath,
        string mimeType);

    public enum DisplayDetail
    {
        Lite,
        Diagnostic,
        Full,
        Xml
    }

    public interface IBaseObject
    {
        void Clear();
        IBaseObject Clone();
        XElement GetElement(string name);
        void OnElement(XElement element);
        XElement Xml { get; set; }
        string StringData { get; set; }
        byte[] BinaryData { get; set; }
        void CollectReferences(List<IBaseObjectKeyed> references, List<IBaseObjectKeyed> externalSavedChildren,
            List<IBaseObjectKeyed> externalNonSavedChildren, Dictionary<int, bool> intSelectFlags,
            Dictionary<string, bool> stringSelectFlags, Dictionary<string, bool> itemSelectFlags,
            Dictionary<string, bool> languageSelectFlags, List<string> mediaFiles, VisitMedia visitFunction);
        void Display(string label, DisplayDetail detail, int indent);
    }
}
