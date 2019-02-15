CREATE TABLE [dbo].[Security]
(
	[Security_ID] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [SecurableEntity_NME] VARCHAR(64) NOT NULL, 
    [Created_DTM] DATETIME NOT NULL, 
    [Enabled_DTM] DATETIME NOT NULL, 
    [Removed_DTM] DATETIME NULL, 
    [UpdatedBy_ID] VARCHAR(8) NULL, 
    [CreatedBy_ID] VARCHAR(8) NOT NULL
)
