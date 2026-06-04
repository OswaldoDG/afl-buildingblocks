namespace GcfOtdrParser.Models;

using AFL.Luna.Atd.Models.Atd;

public class AtdEntry : AtdFile
{
    public string FilePath { get; set; }

    public string FileName { get; set; }

    public string OtdrSharedObjectsPath { get; set; }
}
