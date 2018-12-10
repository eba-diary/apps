CREATE TABLE [dbo].[ApplicationConfiguration]
(
	[AppConfig_Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Application] NVARCHAR(50) NOT NULL, 
    [Options] NVARCHAR(MAX) NOT NULL, 
    [Created] DATETIME NOT NULL, 
    [Modified] DATETIME NOT NULL
)
