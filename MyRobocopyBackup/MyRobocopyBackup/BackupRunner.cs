using System.Text;

namespace MyRobocopyBackup
{
    public class BackupRunner
    {
        private const string RoboDoc = "<table><tr><td>0</td><td>Aucun fichier n'a été copié. Aucune défaillance ne s’est produite. Aucun fichier n’était incompatibles. Les fichiers existent déjà dans le répertoire de destination ; Par conséquent, l’opération de copie a été ignorée.</td></tr><tr><td>1</td><td>Tous les fichiers ont été copiés avec succès.</td></tr><tr><td>2</td><td>Il existe des fichiers supplémentaires dans le répertoire de destination qui ne sont pas présents dans le répertoire source. Aucun fichier n'a été copié.</td></tr><tr><td>3</td><td>Certains fichiers ont été copiés. Fichiers supplémentaires étaient présents. Aucune défaillance ne s’est produite.</td></tr><tr><td>4</td><td></td></tr><tr><td>5</td><td>Certains fichiers ont été copiés. Certains fichiers étaient incompatibles. Aucune défaillance ne s’est produite.</td></tr><tr><td>6</td><td>Il existe des fichiers et des fichiers qui ne correspondent pas. Aucun fichier n'a été copié, aucune défaillance ne s'est produite. Cela signifie que les fichiers existent déjà dans le répertoire de destination.</td></tr><tr><td>7</td><td>Les fichiers ont été copiés, un fichier incompatible a été rencontré et d'autres supplémentaires étaient déjà présents.</td></tr><tr><td>8</td><td>Plusieurs fichiers n'ont pas été copiés.</td></tr></table><p>Remarque Toute valeur supérieure à 8 indique qu’il y a au moins un échec au cours de l’opération de copie. voir <a href=\"https://support.microsoft.com/fr-fr/help/954404/return-codes-that-are-used-by-the-robocopy-utility-in-windows-server-2\">la documentation</a></p>";

        private readonly IMailSender _mailSender;
        private readonly IFileLogger _fileLogger;
        private readonly BackupConfiguration _backupConfiguration;
        private readonly RobocopyRunner _robocopyRunner;
        private readonly RunControl _runControl;

        public BackupRunner(IMailSender mailSender, IFileLogger fileLogger, BackupConfiguration backupConfiguration,
            RobocopyRunner robocopyRunner, RunControl runControl)
        {
            _mailSender = mailSender;
            _fileLogger = fileLogger;
            _backupConfiguration = backupConfiguration;
            _robocopyRunner = robocopyRunner;
            _runControl = runControl;
        }

        public async Task Run()
        {
            var trace = new StringBuilder();
            try
            {
                if (_runControl.HasRunFivesTimesWithoutSuccess())
                {
                    _runControl.Warn();
                }

                if (CanReachBackup())
                {
                    var paths = GetShuffledPath();
                    foreach (var currentPath in paths)
                    {
                        var commandLine = _robocopyRunner.BuildCommandLine(currentPath);
                        await _fileLogger.Log($"Start copy for {currentPath.PathSource} to {currentPath.PathDestination}");
                        trace.Append($"Start copy for {currentPath.PathSource} to {currentPath.PathDestination} ... <br/>");
                        try
                        {
                            _robocopyRunner.RunRobocopy(trace, commandLine);
                        }
                        catch (Exception ex)
                        {
                            trace.Append($"Exception when executing process {ex} <br/><br/>");
                            await _fileLogger.Log($"Exception when executing process {ex}");
                        }
                    }
                    await _fileLogger.Log("Ended without errors");
                    _runControl.Reset();
                }
                else
                {
                    trace.Append($"Unable to reach test file {_backupConfiguration.TestFilePath}");
                    await _fileLogger.Log($"Unable to reach test file {_backupConfiguration.TestFilePath}");
                    _runControl.IncrementFailure();
                }

            }
            catch (Exception ex)
            {
                trace.Append($"Global error {ex}");
                await _fileLogger.Log($"Global error {ex}");
                _runControl.IncrementFailure();
            }

            trace.Append(RoboDoc);

            _mailSender.SendMail($"End of data backup of {_backupConfiguration.Whom}", trace.ToString());
        }

        private List<PathConfig> GetShuffledPath()
        {
            return _backupConfiguration.Paths.OrderBy(x => Guid.NewGuid()).ToList();
        }

        private bool CanReachBackup() => File.Exists(_backupConfiguration.TestFilePath);
    }
}
