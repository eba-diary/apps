CREATE TABLE [dbo].[Schema]
(
	[Schema_Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [SchemaEntity_NME] VARCHAR(64) NOT NULL, 
    [Schema_NME] VARCHAR(100) NOT NULL, 
	[CreatedBy] VARCHAR(8) NOT NULL,
    [Created_DTM] DATETIME NOT NULL, 
    [LastUpdatd_DTM] DATETIME NOT NULL, 
    [FileExtension_Id] INT NULL, 
    CONSTRAINT [FK_Schema_FileExtension] FOREIGN KEY ([FileExtension_Id]) REFERENCES [FileExtension]([Extension_Id]) 
)
