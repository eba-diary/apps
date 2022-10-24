CREATE TABLE [dbo].[DataSourceTokens]
(
    [Id] INT NOT NULL, 
    [ParentDataSource_Id] INT NOT NULL, 
    [CurrentToken] VARCHAR(50) NULL, 
    [RefreshToken] VARCHAR(50) NULL, 
    [CurrentTokenExp] DATETIME NULL, 
    [TokenName] VARCHAR(50) NOT NULL, 
    [TokenUrl] VARCHAR(50) NULL, 
    [Scope] NCHAR(50) NULL, 
    [TokenExp] VARCHAR(50) NULL, 
    PRIMARY KEY ([Id]), 
    CONSTRAINT [FK_DataSourceTokens_DataSource] FOREIGN KEY ([ParentDataSource_Id]) REFERENCES [DataSource]([DataSource_Id]) 
)
