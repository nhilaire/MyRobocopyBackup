namespace MyRobocopyBackup
{
    public interface IMailSender
    {
        void SendMail(string subject, string content);
    }
}