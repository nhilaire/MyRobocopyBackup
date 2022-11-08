using Newtonsoft.Json;
using System.Reflection;

namespace MyRobocopyBackup
{
    public class RunControl
    {
        private const int NbTimes = 5;

        private readonly IMailSender _mailSender;
        private readonly BackupConfiguration _backupConfiguration;
        private readonly string _pathFile;

        public RunControl(IMailSender mailSender, BackupConfiguration backupConfiguration)
        {
            _mailSender = mailSender;
            _backupConfiguration = backupConfiguration;
            _pathFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "nbtimes.json");
        }

        public bool HasRunFivesTimesWithoutSuccess()
        {
            if (!File.Exists(_pathFile))
            {
                return false;
            }
            var nbTimes = ReadNbtimes();
            return nbTimes?.Nb >= NbTimes;
        }

        public void Warn()
        {
            _mailSender.SendMail($"Warning for {_backupConfiguration.Whom} !", $"The backup task can't complete since {NbTimes} times");
        }

        public void Reset()
        {
            if (File.Exists(_pathFile))
            {
                File.Delete(_pathFile);
            }
        }

        public void IncrementFailure()
        {
            var nbTimes = ReadNbtimes();
            nbTimes.Nb++;
            var contents = JsonConvert.SerializeObject(nbTimes);
            File.WriteAllText(_pathFile, contents);
        }

        private NbTimes ReadNbtimes()
        {
            var json = File.ReadAllText(_pathFile);
            var nbTimes = JsonConvert.DeserializeObject<NbTimes>(json);
            return nbTimes ?? new NbTimes { Nb = 0 };
        }
    }
}
