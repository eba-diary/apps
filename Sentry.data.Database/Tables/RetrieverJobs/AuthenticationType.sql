CREATE TABLE [dbo].[AuthenticationType]
(
	[Auth_Id] INT NOT NULL PRIMARY KEY, 
    [AuthType_CDE] NVARCHAR(250) NOT NULL, 
    [Display_NME] NVARCHAR(250) NOT NULL, 
    [Description] NVARCHAR(250) NOT NULL
)
