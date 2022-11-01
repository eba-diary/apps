CREATE TABLE [dbo].[DataSourceTokens]
(
    [Id] INT Identity(1,1) NOT NULL, 
    [ParentDataSource_Id] INT NOT NULL, 
    [CurrentToken] NVARCHAR(MAX) NULL, 
    [RefreshToken] NVARCHAR(MAX) NULL, 
    [CurrentTokenExp] DATETIME NULL, 
    [TokenName] VARCHAR(50) NOT NULL, 
    [TokenUrl] VARCHAR(50) NULL, 
    [Scope] VARCHAR(500) NULL, 
    [TokenExp] VARCHAR(50) NULL, 
    PRIMARY KEY ([Id]), 
    CONSTRAINT [FK_DataSourceTokens_DataSource] FOREIGN KEY ([ParentDataSource_Id]) REFERENCES [DataSource]([DataSource_Id]) 
)
