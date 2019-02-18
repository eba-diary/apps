CREATE TABLE [dbo].[Permission]
(
	[Permission_ID] INT NOT NULL PRIMARY KEY, 
    [Permission_CDE] VARCHAR(64) NOT NULL, 
    [Permission_NME] VARCHAR(64) NOT NULL, 
    [Permission_DSC] VARCHAR(512) NOT NULL, 
    [SecurableObject_TYP] VARCHAR(32) NOT NULL
)
