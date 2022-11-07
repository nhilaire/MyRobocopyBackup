using System.Reflection;

namespace MyRobocopyBackup
{
    internal class FileLogger : IFileLogger
    {
        private readonly string _pathLog;

        public FileLogger()
        {
            _pathLog = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "log.txt");
        }

        public async Task Log(string message)
        {
            try
            {
                using var sw = new StreamWriter(_pathLog, true);
                await sw.WriteLineAsync(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " => " + message);
            }
            catch (Exception)
            {
            }
        }
    }
}
