CREATE TABLE [dbo].[Asset]
(
	[Asset_ID] INT IDENTITY(1,1) NOT NULL, 
    [SaidKey_CDE] CHAR(4) NOT NULL, 
    [Security_ID] UNIQUEIDENTIFIER NOT NULL, 
    CONSTRAINT [PK_Asset] PRIMARY KEY ([Asset_ID]),
    CONSTRAINT [AK1_Asset] UNIQUE ([SaidKey_CDE]),
    CONSTRAINT [FK_Asset_Security] FOREIGN KEY ([Security_ID]) REFERENCES [Security]([Security_ID])
)
