CREATE TABLE [dbo].[Notifications]
(
	[Notification_ID] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Message_DSC] VARCHAR(250) NOT NULL, 
    [CreateUser] VARCHAR(10) NOT NULL, 
    [Start_DTM] DATETIME NOT NULL, 
    [Expire_DTM] DATETIME NOT NULL, 
    [Object_ID] INT NOT NULL, 
    [Severity_TYP] INT NOT NULL, 
    [NotificationType] VARCHAR(20) NOT NULL, 
    [Title] VARCHAR(250) NULL 
)
