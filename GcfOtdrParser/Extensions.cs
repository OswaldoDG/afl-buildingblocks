namespace GcfOtdrParser;

using AFL.Luna.Atd.Models.Atd;
using GcfOtdrParser.Models;

public static class Extensions
{

    public static AtdEntry ToAdtEntry(this AtdFile atdFile, string filePath, string fileName, string otdrSharedObjectsPath = null)
    {
        return new AtdEntry
        {
            FilePath = filePath,
            FileName = fileName,
            OtdrSharedObjectsPath = otdrSharedObjectsPath
        };
    }

    public static Repositories.Model.Otdr.AtdEntry ToDbAdtEntry(this AtdFile atdFile, string filePath, string fileName, string otdrSharedObjectsPath = null)
    {
        return new Repositories.Model.Otdr.AtdEntry
        {
            FilePath = filePath,
            FileName = fileName,
            OtdrSharedObjectsPath = otdrSharedObjectsPath
        };
    }
}
