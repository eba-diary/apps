CREATE TABLE [dbo].[JobHistory]
(
	[History_Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Job_ID] INT NOT NULL, 
    [State] NVARCHAR(100) NOT NULL, 
    [LivyAppId] NVARCHAR(MAX) NULL, 
    [LivyDriverLogUrl] NVARCHAR(MAX) NULL, 
    [LivySparkUiUrl] NVARCHAR(MAX) NULL, 
    [LogInfo] NVARCHAR(MAX) NULL, 
    [Created] DATETIME NOT NULL, 
    [Modified] DATETIME NOT NULL, 
    [BatchId] INT NOT NULL, 
    [ActiveInd] BIT NOT NULL, 
    [Job_Guid] UNIQUEIDENTIFIER NULL, 
    [Submission] INT NULL, 
    [ClusterUrl] VARCHAR(100) NULL, 
    CONSTRAINT [FK_JobHistory_RetrieverJob] FOREIGN KEY ([Job_ID]) REFERENCES [RetrieverJob]([Job_ID])
)
GO

create index IDX_JobHistoryNeedsValidation on JobHistory(ActiveInd) include (BatchId) with(online =on)