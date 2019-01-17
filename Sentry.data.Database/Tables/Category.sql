CREATE TABLE [dbo].[Category] (
    [Id]             INT          NOT NULL,
    [Name]           VARCHAR (255) NULL,
    [ParentCategory] INT           NULL,
    [Color] VARCHAR(50) NOT NULL, 
    [Object_TYP] CHAR(3) NULL, 
    [AbbreviatedName] VARCHAR(10) NULL, 
    CONSTRAINT [PK_Category] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Category_Category] FOREIGN KEY ([ParentCategory]) REFERENCES [dbo].[Category] ([Id])
);

