CREATE TABLE [dbo].[SchemaRevision]
(
	[SchemaRevision_Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [ParentSchema_Id] INT NOT NULL,
	[Revision_NBR] INT NOT NULL,
    [Revision_Name] VARCHAR(50) NULL, 
    [CreatedBy] VARCHAR(8) NOT NULL, 
    [CreatedDTM] DATETIME NOT NULL, 
    [LastUpdatedDTM] DATETIME NOT NULL, 
    [JsonSchemaObject] VARCHAR(MAX) NULL, 
    CONSTRAINT [FK_SchemaRevision_Schema] FOREIGN KEY ([ParentSchema_Id]) REFERENCES [Schema]([Schema_Id])
)

GO

CREATE INDEX [IDX_SchemaRevision_ParentSchema_Id] ON [dbo].[SchemaRevision] ([ParentSchema_Id]) INCLUDE (Revision_NBR)
