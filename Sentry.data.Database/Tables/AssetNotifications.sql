CREATE TABLE [dbo].[AssetNotifications]
(
	[Notification_ID] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Message_DSC] VARCHAR(250) NOT NULL, 
    [CreateUser] VARCHAR(10) NOT NULL, 
    [Start_DTM] DATETIME NOT NULL, 
    [Expire_DTM] DATETIME NOT NULL, 
    [DataAsset_ID] INT NOT NULL, 
    [Severity_TYP] INT NOT NULL, 
    CONSTRAINT [FK_AssetNotifications_DataAsset] FOREIGN KEY ([DataAsset_ID]) REFERENCES [DataAsset]([DataAsset_ID])
)
