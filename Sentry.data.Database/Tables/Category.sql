CREATE TABLE [dbo].[Category] (
    [Id]             INT           IDENTITY (1, 1) NOT NULL,
    [Name]           VARCHAR (255) NULL,
    [ParentCategory] INT           NULL,
    [Color] VARCHAR(50) NOT NULL, 
    CONSTRAINT [PK_Category] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Category_Category] FOREIGN KEY ([ParentCategory]) REFERENCES [dbo].[Category] ([Id])
);

