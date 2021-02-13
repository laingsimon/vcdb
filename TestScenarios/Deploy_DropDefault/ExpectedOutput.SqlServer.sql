ALTER TABLE [dbo].[Person]
DROP CONSTRAINT [DF__Person__Id__/[A-F0-9]{8}/]
GO
ALTER TABLE [dbo].[Person]
DROP CONSTRAINT [DF_Person_DefaultName]
GO
