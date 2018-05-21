CREATE TABLE [dbo].[FileExtension]
(
	[Extension_Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Extension_NME] NVARCHAR(10) NOT NULL, 
    [Created_DTM] DATETIME NOT NULL, 
    [CreateUser_ID] NVARCHAR(20) NOT NULL
)
