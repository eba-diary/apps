CREATE TABLE [dbo].[Tag]
(
	[TagId] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Name] VARCHAR(100) NOT NULL, 
    [Created] DATETIME NOT NULL, 
    [CreatedBy] CHAR(10) NOT NULL, 
    [Description] VARCHAR(250) NULL, 
    [TagGroupId] INT NOT NULL, 
    CONSTRAINT [FK_Tag_TagGroup] FOREIGN KEY ([TagGroupId]) REFERENCES [TagGroup]([TagGroupId])
)
