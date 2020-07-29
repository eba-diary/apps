IF NOT EXISTS (Select * from FileExtension where [Extension_NME] = 'FIXEDWIDTH')
BEGIN
	INSERT INTO FileExtension select 'FIXEDWIDTH', GETDATE(), '072984'
END