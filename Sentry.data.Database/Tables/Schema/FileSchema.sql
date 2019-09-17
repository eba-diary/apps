CREATE TABLE [dbo].[FileSchema]
(
	[Schema_Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [SchemaEntity_NME] VARCHAR(64) NOT NULL, 
    [Schema_NME] VARCHAR(100) NOT NULL, 
    [Revision_Id] INT NOT NULL, 
    [Revision_NME] VARCHAR(100) NULL, 
    [Created_DTM] DATETIME NOT NULL, 
    [LastUpdatd_DTM] DATETIME NOT NULL, 
    [SchemaStruct_Id] UNIQUEIDENTIFIER NOT NULL, 
    [FileExtension_Id] INT NOT NULL, 
    CONSTRAINT [FK_FileSchema_FielExtension] FOREIGN KEY ([FileExtension_Id]) REFERENCES [FileExtension]([Extension_Id])
)
