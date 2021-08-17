using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using WorkUtilities.Domain.Models;

namespace WorkUtilities.Domain.Services.Package
{
    public class FilePackagerService
    {
        public byte[] BuildPackage(List<InMemoryFile> files)
        {
            byte[] archiveFile;
            using (var archiveStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, true))
                {
                    foreach (var file in files)
                    {
                        var zipArchiveEntry = archive.CreateEntry(Path.Combine(file.Basepath, file.Name) + file.Extension, CompressionLevel.Fastest);

                        using var zipStream = zipArchiveEntry.Open();
                        zipStream.Write(file.ContentData, 0, file.ContentData.Length);
                    }
                }

                archiveFile = archiveStream.ToArray();
            }

            return archiveFile;
        }

        public InMemoryFile BuildFile(string basePath, string filename, string extension, string value)
        {
            byte[] bytes = null;
            using (var ms = new MemoryStream())
            {
                TextWriter tw = new StreamWriter(ms);
                tw.Write(value);
                tw.Flush();
                ms.Position = 0;
                bytes = ms.ToArray();
            }

            return new InMemoryFile
            {
                Basepath = basePath,
                Name = filename,
                Extension = extension,
                ContentText = value,
                ContentData = bytes
            };
        }
    }
}
