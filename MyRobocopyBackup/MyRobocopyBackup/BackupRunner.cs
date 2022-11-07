using System.Diagnostics;
using System.Text;

namespace MyRobocopyBackup
{
    public class BackupRunner
    {
        private const string RoboDoc = "<table><tr><td>0</td><td>Aucun fichier n'a été copié. Aucune défaillance ne s’est produite. Aucun fichier n’était incompatibles. Les fichiers existent déjà dans le répertoire de destination ; Par conséquent, l’opération de copie a été ignorée.</td></tr><tr><td>1</td><td>Tous les fichiers ont été copiés avec succès.</td></tr><tr><td>2</td><td>Il existe des fichiers supplémentaires dans le répertoire de destination qui ne sont pas présents dans le répertoire source. Aucun fichier n'a été copié.</td></tr><tr><td>3</td><td>Certains fichiers ont été copiés. Fichiers supplémentaires étaient présents. Aucune défaillance ne s’est produite.</td></tr><tr><td>4</td><td></td></tr><tr><td>5</td><td>Certains fichiers ont été copiés. Certains fichiers étaient incompatibles. Aucune défaillance ne s’est produite.</td></tr><tr><td>6</td><td>Il existe des fichiers et des fichiers qui ne correspondent pas. Aucun fichier n'a été copié, aucune défaillance ne s'est produite. Cela signifie que les fichiers existent déjà dans le répertoire de destination.</td></tr><tr><td>7</td><td>Les fichiers ont été copiés, un fichier incompatible a été rencontré et d'autres supplémentaires étaient déjà présents.</td></tr><tr><td>8</td><td>Plusieurs fichiers n'ont pas été copiés.</td></tr></table><p>Remarque Toute valeur supérieure à 8 indique qu’il y a au moins un échec au cours de l’opération de copie. voir <a href=\"https://support.microsoft.com/fr-fr/help/954404/return-codes-that-are-used-by-the-robocopy-utility-in-windows-server-2\">la documentation</a></p>";

        private readonly IMailSender _mailSender;
        private readonly IFileLogger _fileLogger;
        private readonly BackupConfiguration _backupConfiguration;

        public BackupRunner(IMailSender mailSender, IFileLogger fileLogger, BackupConfiguration backupConfiguration)
        {
            _mailSender = mailSender;
            _fileLogger = fileLogger;
            _backupConfiguration = backupConfiguration;
        }

        public async Task Run()
        {
            var trace = new StringBuilder();
            try
            {
                if (CanReachBackup())
                {
                    var paths = GetShuffledPath();
                    foreach (var currentPath in paths)
                    {
                        var commandLine = BuildCommandLine(currentPath);
                        await _fileLogger.Log($"Start copy for {currentPath.PathSource} to {currentPath.PathDestination}");
                        trace.Append($"Start copy for {currentPath.PathSource} to {currentPath.PathDestination} ... <br/>");
                        try
                        {
                            RunRobocopy(trace, commandLine);
                        }
                        catch (Exception ex)
                        {
                            trace.Append($"Exception when executing process {ex} <br/><br/>");
                            await _fileLogger.Log($"Exception when executing process {ex}");
                        }
                    }
                    await _fileLogger.Log("Ended without errors");
                }
                else
                {
                    trace.Append($"Unable to reach test file {_backupConfiguration.TestFilePath}");
                    await _fileLogger.Log($"Unable to reach test file {_backupConfiguration.TestFilePath}");
                }

            }
            catch (Exception ex)
            {
                trace.Append($"Global error {ex}");
                await _fileLogger.Log($"Global error {ex}");
            }

            trace.Append(RoboDoc);

            _mailSender.SendMail("End of data backup", trace.ToString());
        }

        private static void RunRobocopy(StringBuilder trace, string commandLine)
        {
            var process = Process.Start("robocopy", commandLine);
            process.WaitForExit();
            int result = process.ExitCode;
            trace.Append($"Process ended with return code {result} <br/><br/>");
            Console.WriteLine($"Process ended with return code {result}");
        }

        private string BuildCommandLine(PathConfig currentPath)
        {
            string param = $"{currentPath.PathSource} {currentPath.PathDestination} {_backupConfiguration.CommandLine}";
            if (currentPath.Exclusions != null)
            {
                foreach (var exclusion in currentPath.Exclusions)
                {
                    param += $" /XD \"{exclusion}\"";
                }
            }
            return param;
        }

        private List<PathConfig> GetShuffledPath()
        {
            return _backupConfiguration.Paths.OrderBy(x => Guid.NewGuid()).ToList();
        }

        private bool CanReachBackup() => File.Exists(_backupConfiguration.TestFilePath);
    }
}
