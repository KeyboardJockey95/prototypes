using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Formats
{
    public class FormatMulti : Format
    {
        private List<string> _Names = null;
        private List<string> _Types = null;
        public IMainRepository Repositories { get; set; }
        public Format Format { get; set; }

        public FormatMulti(string name, string type, string owner, IMainRepository repositories)
            : base(name, type, owner)
        {
            Repositories = repositories;
            Format = null;
        }

        public FormatMulti()
        {
            Format = null;
        }

        public virtual Format GetFormat(string name, string type)
        {
            Format format = null;

            switch (type)
            {
                case "CEDict":
                    format = new FormatCEDict(name, Owner, Repositories.Dictionary);
                    break;
                case "EDict":
                    format = new FormatEDict(name, Owner, Repositories.Dictionary);
                    break;
                case "Text":
                case "XML":
                default:
                    throw new ModelException("Format type " + type + " not supported yet.");
            }

            return format;
        }

        public override void DeleteFirst()
        {
            base.DeleteFirst();

            if (Format == null)
                Format = GetFormat(Name, Type);

            if (Format != null)
                Format.DeleteFirst();
        }

        public override void Read(Stream stream)
        {
            ItemCount = 0;

            if (Format == null)
                Format = GetFormat(Name, Type);

            if (Format != null)
            {
                Format.Read(stream);
                ItemCount = Format.ItemCount;
            }
        }

        public override void Write(Stream stream)
        {
            ItemCount = 0;

            if (Format == null)
                Format = GetFormat(Name, Type);

            if (Format != null)
            {
                Format.Write(stream);
                ItemCount = Format.ItemCount;
            }
        }

        public override float GetProgress()
        {
            if (Format == null)
                Format = GetFormat(Name, Type);

            if (Format != null)
                return Format.GetProgress();

            return 1.0f;
        }

        public override string GetProgressMessage()
        {
            if (Format == null)
                Format = GetFormat(Name, Type);

            if (Format != null)
                return Format.GetProgressMessage();

            return null;
        }

        public List<string> Names
        {
            get
            {
                if (_Names == null)
                {
                    _Names = new List<string>()
                    {
                        "Dictionary"
                    };
                }

                return _Names;
            }

            set
            {
                _Names = value;
            }
        }

        public List<string> Types
        {
            get
            {
                if (_Types == null)
                {
                    _Types = new List<string>()
                    {
                        "CEDict"
                    };
                }

                return _Types;
            }

            set
            {
                _Types = value;
            }
        }

        public static new bool IsSupportedStatic(string importExport, string componentName, string capability)
        {
            return false;
        }

        public override bool IsSupportedVirtual(string importExport, string componentName, string capability)
        {
            if (Format != null)
                return Format.IsSupportedVirtual(importExport, componentName, capability);

            return IsSupportedStatic(importExport, componentName, capability);
        }

        public static new string TypeStringStatic { get { return "Multi"; } }

        public override string TypeStringVirtual { get { return TypeStringStatic; } }
    }
}
