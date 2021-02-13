ALTER TABLE [dbo].[Person]
DROP CONSTRAINT [CK__Person__Name__/[A-F0-9]{8}/]
GO
ALTER TABLE [dbo].[Person]
DROP CONSTRAINT [CK_Person_IdNotTooBig]
GO
