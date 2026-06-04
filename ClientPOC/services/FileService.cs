namespace ClientPOC.services;

public class FileService
{
    public static List<FileInfo> GetFilesRecursive(string path)
    {
        var fileList = new List<FileInfo>();
        // Agregar todos los archivos del directorio actual
        fileList.AddRange(Directory.GetFiles(path).ToList().Select(f => new FileInfo(f)));

        // Obtener todos los subdirectorios y buscar recursivamente
        foreach (var directory in Directory.GetDirectories(path))
        {
            fileList.AddRange(GetFilesRecursive(directory));
        }

        return fileList;
    }

    public static void LogTimeDiff(string proc, DateTime started)
    {
        File.AppendAllText("times.txt", $"{proc}\t{(DateTime.Now- started).TotalMilliseconds}\r\n");
    }
}
