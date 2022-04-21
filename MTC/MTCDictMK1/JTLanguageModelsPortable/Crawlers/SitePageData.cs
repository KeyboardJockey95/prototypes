using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Helpers;
using JTLanguageModelsPortable.MediaInterfaces;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Crawlers
{
    public class SitePageData
    {
        public IDataBuffer DataBuffer { get; set; }

        public SitePageData(IDataBuffer dataBuffer)
        {
            DataBuffer = dataBuffer;
        }

        public SitePageData(byte[] pageData)
        {
            DataBuffer = new MemoryBuffer(pageData);
        }

        public SitePageData(String pageString)
        {
            PageData = ApplicationData.Global.GetBytesFromStringUTF8(pageString);
        }

        public SitePageData()
        {
            DataBuffer = new MemoryBuffer();
        }

        public byte[] PageData
        {
            get
            {
                return DataBuffer.GetAllBytes();
            }
            set
            {
                if (DataBuffer == null)
                    DataBuffer = new MemoryBuffer();
                if (value != null)
                    DataBuffer.SetAllBytes(value);
                else
                    DataBuffer.Delete();
            }
        }

        public string PageString
        {
            get
            {
                if (PageData != null)
                    return ApplicationData.Global.GetStringFromBytesUTF8(PageData);
                return null;
            }
            set
            {
                if (value != null)
                    PageData = ApplicationData.Global.GetBytesFromStringUTF8(value);
                else
                    PageData = null;
            }
        }

        public Stream OpenWriteableStream()
        {
            if (DataBuffer == null)
                return null;
            return DataBuffer.GetWriteableStream();
        }

        public void CloseWriteableStream(Stream stream)
        {
            if (DataBuffer == null)
                return;
            DataBuffer.CompleteWrite(stream);
        }

        public Stream OpenReadableStream()
        {
            if (DataBuffer == null)
                return null;
            return DataBuffer.GetReadableStream();
        }

        public void CloseReadableStream(Stream stream)
        {
            if (DataBuffer == null)
                return;
            DataBuffer.CompleteRead(stream);
        }
    }
}
