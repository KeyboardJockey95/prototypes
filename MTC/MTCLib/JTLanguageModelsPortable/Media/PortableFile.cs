using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.MediaInterfaces;

namespace JTLanguageModelsPortable.Media
{
    public class PortableFile : IPortableFile
    {
        public static PortableFile Singleton;
        public virtual void AppendAllText(string path, string contents) { }
        public virtual void AppendAllText(string path, string contents, Encoding encoding) { }
        public virtual StreamWriter AppendText(string path) { return null; }
        public virtual void Copy(string sourceFileName, string destFileName) { }
        public virtual void Copy(string sourceFileName, string destFileName, bool overwrite) { }
        public virtual Stream Create(string path) { return null; }
        public virtual StreamWriter CreateText(string path) { return null; }
        public virtual void Delete(string path) { }
        public virtual bool Exists(string path) { return false; }
        public virtual long FileSize(string path) { return -1L; }
        public virtual void Move(string sourceFileName, string destFileName) { }
        public virtual Stream Open(string path, PortableFileMode mode) { return null; }
        public virtual Stream Open(string path, PortableFileMode mode, PortableFileAccess access) { return null; }
        public virtual Stream OpenRead(string path) { return null; }
        public virtual StreamReader OpenText(string path) { return null; }
        public virtual StreamReader OpenText(string path, Encoding encoding) { return null; }
        public virtual Stream OpenWrite(string path) { return null; }
        public virtual byte[] ReadAllBytes(string path) { return null; }
        public virtual string ReadAllText(string path) { return null; }
        public virtual string ReadAllText(string path, Encoding encoding) { return null; }
        public virtual string[] ReadAllLines(string path) { return null; }
        public virtual string[] ReadAllLines(string path, Encoding encoding) { return null; }
        public virtual void WriteAllBytes(string path, byte[] bytes) { }
        public virtual void WriteAllText(string path, string contents) { }
        public virtual void WriteAllText(string path, string contents, Encoding encoding) { }
        public virtual void WriteAllLines(string path, string[] contents) { }
        public virtual void WriteAllLines(string path, string[] contents, Encoding encoding) { }
        public virtual void Close(Stream stream) { }
        public virtual void Close(StreamReader streamReader) { }
        public virtual void Close(StreamWriter streamWriter) { }
        public virtual bool DirectoryExistsCheck(string path) { return false; }
        public virtual bool DirectoryExists(string path) { return false; }
        public virtual void CreateDirectory(string path) { }
        public virtual void RenameDirectory(string oldPath, string newPath) { }
        public virtual void DeleteDirectory(string path) { }
        public virtual bool DeleteEmptyDirectory(string path) { return false; }
        public virtual void CopyDirectory(string sourceDirectoryPath, string destDirectoryPath) { }
        public virtual void CopyDirectory(string sourceDirectoryPath, string destDirectoryPath, bool overwrite) { }
        public virtual void MoveDirectory(string sourceDirectoryPath, string destDirectoryPath) { }
        public virtual void MoveDirectory(string sourceDirectoryPath, string destDirectoryPath, bool overwrite) { }
        public virtual List<string> GetFiles(string path) { return new List<string>();  }
        public virtual List<string> GetDirectories(string path) { return new List<string>(); }
        public virtual string GetTemporaryFileName(string fileExtension) { return String.Empty; }
        public virtual IArchiveFile Archive() { return null; }
        public virtual long GetFileSize(string path) { return -1L; }
        public virtual DateTime GetFileDate(string path) { return DateTime.MinValue; }
        public virtual long GetDirectorySize(string path, List<string> extensions, bool recurse) { return -1L; }
    }
}
