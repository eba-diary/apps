CREATE TABLE [dbo].[Submission]
(
	[Submission_ID] INT IDENTITY (1, 1) NOT NULL,
	[Job_ID] INT NOT NULL, 
    [Job_Guid] UNIQUEIDENTIFIER NOT NULL, 
    [Serialized_Job_Options] NVARCHAR(MAX) NULL, 
    [Created] DATETIME NULL, 
    [FlowExecutionGuid] VARCHAR(17) NULL, 
    [RunInstanceGuid] VARCHAR(17) NULL, 
    [ClusterUrl] VARCHAR(100) NULL, 
    CONSTRAINT [FK_Submission_RetrieverJob] FOREIGN KEY ([Job_ID]) REFERENCES [RetrieverJob]([Job_ID])
)
