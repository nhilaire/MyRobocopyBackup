public class MailSettingsConfiguration
{
    public string From { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string Dest { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
}

public class BackupConfiguration
{
    public string Whom { get; set; } = string.Empty;
    public string TestFilePath { get; set; }
    public string CommandLine { get; set; }
    public List<PathConfig> Paths { get; set; }
}

public class PathConfig
{
    public string PathSource { get; set; }
    public string PathDestination { get; set; }
    public string[] Exclusions { get; set; }
}