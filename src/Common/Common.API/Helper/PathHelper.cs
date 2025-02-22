namespace Common.API.Helper;
public static class PathHelper
{
    public static string GetRootDirectory()
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var directoryInfo = new DirectoryInfo(baseDirectory);

        while (directoryInfo != null && !directoryInfo.Name.Equals("src", StringComparison.OrdinalIgnoreCase))
        {
            directoryInfo = directoryInfo.Parent;
        }

        return directoryInfo?.FullName;
    }
}
