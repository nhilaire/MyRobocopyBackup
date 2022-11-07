using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyRobocopyBackup;

var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", true, true)
    .AddJsonFile($"appsettings.{environmentName}.json", true, true)
    .Build();

var mailConfiguration = new MailSettingsConfiguration();
config.GetSection("MailSettings").Bind(mailConfiguration);
var backupConfiguration = new BackupConfiguration();
config.GetSection("BackupConfig").Bind(backupConfiguration);

var serviceProvider = new ServiceCollection()
    .AddTransient(_ => mailConfiguration)
    .AddTransient(_ => backupConfiguration)
    .AddTransient<IFileLogger, FileLogger>()
    .AddTransient<IMailSender, MailSender>()
    .AddTransient<BackupRunner>()
    .BuildServiceProvider();

using var scope = serviceProvider.CreateScope();
await scope.ServiceProvider.GetRequiredService<BackupRunner>().Run();
