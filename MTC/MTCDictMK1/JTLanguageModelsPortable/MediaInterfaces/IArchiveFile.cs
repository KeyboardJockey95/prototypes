using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JTLanguageModelsPortable.MediaInterfaces
{
    public delegate bool HandleReadFileDelegate(string filePath, ref string baseDirectory);

    public interface IArchiveFile
    {
        bool Create();
        bool Create(string archivePath);
        bool Create(Stream archiveStream);
        bool AddFile(string filePath, string filePathInArchive);
        bool Extract(string baseDirectory, bool overwrite, HandleReadFileDelegate readFileFunction);
        bool Save(string archivePath);
        bool Save(Stream stream);
        int Count();
        void Close();
        bool Compress(byte[] input, out byte[] output);
        bool Decompress(byte[] input, out byte[] output);
    }
}
