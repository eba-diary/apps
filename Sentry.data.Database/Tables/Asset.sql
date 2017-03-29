CREATE TABLE [dbo].[Asset] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [Version]       INT            NOT NULL,
    [Name]          VARCHAR (255)  NULL,
    [Description]   VARCHAR (255)  NULL,
    CONSTRAINT [PK_Asset] PRIMARY KEY CLUSTERED ([Id] ASC)
);
