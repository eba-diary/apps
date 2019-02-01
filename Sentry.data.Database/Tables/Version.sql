CREATE TABLE [dbo].[Version]
(
    [Version_CDE] VARCHAR(50) NOT NULL, 
    [AppliedOn_DTM] DATETIME NOT NULL
    CONSTRAINT PK_Version PRIMARY KEY ([Version_CDE])
)