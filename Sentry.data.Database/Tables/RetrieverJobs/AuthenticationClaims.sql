CREATE TABLE [dbo].[AuthenticationClaims]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [DataSource_Id] INT NOT NULL, 
    [Name] CHAR(50) NOT NULL, 
    [Value] VARCHAR(1000) NOT NULL, 
    [Token_Id] INT NULL, 
    CONSTRAINT [FK_AuthenticationClaims_DataSource] FOREIGN KEY ([DataSource_Id]) REFERENCES [DataSource]([DataSource_Id]),
    CONSTRAINT [FK_AuthenticationClaims_DataSourceTokens] FOREIGN KEY ([Token_Id]) REFERENCES [DataSourceTokens]([Id])
)
