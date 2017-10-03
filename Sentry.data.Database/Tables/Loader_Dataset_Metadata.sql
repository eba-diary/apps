CREATE TABLE [dbo].[Loader_Dataset_Metadata]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [Search_Name] CHAR(255) NULL, 
    [Search_Regex] CHAR(50) NULL, 
    [Dataset_Name] CHAR(255) NOT NULL, 
    [Category_ID] INT NOT NULL, 
    [Prefix] CHAR(50) NULL, 
    [Sufix] CHAR(50) NULL, 
    [Description] VARCHAR(4096) NOT NULL, 
    [Create_User] VARCHAR(128) NULL, 
    [Owner_ID] INT NULL, 
    [Frequency] INT NOT NULL, 
    [Notification_Email] VARCHAR(4096) NULL, 
    CONSTRAINT [FK_Loader_Dataset_Metadata_To_Category] FOREIGN KEY ([Category_ID]) REFERENCES [Category]([Id])
)
