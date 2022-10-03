CREATE TABLE [dbo].[SchemaField]
(
	[Field_Id] INT NOT NULL IDENTITY, 
    [Field_NME] VARCHAR(100) NOT NULL, 
    [IsArray] BIT NOT NULL, 
    [OrdinalPosition] INT NULL, 
    [StartPosition] INT NULL, 
    [EndPosition] INT NULL, 
    [CreateDTM] DATETIME NOT NULL, 
    [LastUpdateDTM] DATETIME NOT NULL, 
    [NullableIndicator] BIT NOT NULL, 
    [Type] VARCHAR(50) NOT NULL, 
    [ParentField] INT NULL,
	[Precision] INT NULL, 
    [Scale] INT NULL, 
    [SourceFormat] VARCHAR(50) NULL, 
    [ParentSchemaRevision] INT NOT NULL, 
    [FieldLength] INT NULL, 
    [FieldGuid] UNIQUEIDENTIFIER NULL, 
    [Description] VARCHAR(2000) NULL, 
    [DotNamePath] VARCHAR(1000) NULL, 
    [StructurePosition] VARCHAR(100) NULL, 
    CONSTRAINT [PK_SchemaField] PRIMARY KEY CLUSTERED (Field_Id ASC),
    CONSTRAINT [FK_SchemaField_SchemaField] FOREIGN KEY ([ParentField]) REFERENCES [SchemaField]([Field_Id]), 
    CONSTRAINT [FK_SchemaField_SchemaRevision] FOREIGN KEY ([ParentSchemaRevision]) REFERENCES [SchemaRevision]([SchemaRevision_Id])
)

GO

CREATE INDEX [idx_SchemaField_parent] ON [dbo].[SchemaField] ([ParentField])

GO

CREATE INDEX [idx_SchemaField_parentSchemaRevision] on [dbo].[SchemaField] ([ParentSchemaRevision])
