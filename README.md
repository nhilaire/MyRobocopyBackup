# My wrapper around Robocopy

As a developper I need to backup somes files of my laptop, so I created a little program that wraps Robocopy.exe.
The purpose is to read a configuration file, do the backup on an external drive, and send a mail when the process is finished. 
I schedule this task with the windows scheduler and it's ok !

Some things to notice

 - **Mail sending** is done with my Gmail account. You just have to activate two factor authentication and create a password here https://myaccount.google.com/apppasswords
 - I check that my external is available by looking at a specific file (see BackupConfig:TestFilePath)
 - I want to be alerted if the process can't run 5 times