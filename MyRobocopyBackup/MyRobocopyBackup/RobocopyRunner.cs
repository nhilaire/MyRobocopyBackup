using System.Diagnostics;
using System.Text;

namespace MyRobocopyBackup
{
    public class RobocopyRunner
    {
        private readonly BackupConfiguration _backupConfiguration;

        public RobocopyRunner(BackupConfiguration backupConfiguration)
        {
            _backupConfiguration = backupConfiguration;
        }

        public void RunRobocopy(StringBuilder trace, string commandLine)
        {
            var process = Process.Start("robocopy", commandLine);
            process.WaitForExit();
            int result = process.ExitCode;
            trace.Append($"Process ended with return code {result} <br/><br/>");
            Console.WriteLine($"Process ended with return code {result}");
        }

        public string BuildCommandLine(PathConfig currentPath)
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
    }
}
