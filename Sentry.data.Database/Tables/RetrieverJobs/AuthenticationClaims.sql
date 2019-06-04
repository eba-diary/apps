CREATE TABLE [dbo].[AuthenticationClaims]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [DataSource_Id] INT NOT NULL, 
    [Name] CHAR(50) NOT NULL, 
    [Value] VARCHAR(1000) NOT NULL, 
    CONSTRAINT [FK_AuthenticationClaims_DataSource] FOREIGN KEY ([DataSource_Id]) REFERENCES [DataSource]([DataSource_Id])
)
