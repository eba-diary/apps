CREATE TABLE [HangFire7].[Counter] (
    [Key]      NVARCHAR (100) NOT NULL,
    [Value]    INT            NOT NULL,
    [ExpireAt] DATETIME       NULL
);


GO
CREATE CLUSTERED INDEX [CX_HangFire_Counter]
    ON [HangFire7].[Counter]([Key] ASC);

