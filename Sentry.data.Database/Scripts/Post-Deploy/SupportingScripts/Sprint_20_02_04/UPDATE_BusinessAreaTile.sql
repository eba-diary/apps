--*******************************************************************************************************************
--UPDATE Training Hyperlink_URL, Order_SEQ and PL Data Services Business Dashboard Image on PL Landing Page
--select * from BusinessAreaTile order by  Order_SEQ
--********************************************************************************************************************
UPDATE BusinessAreaTile
SET Hyperlink_URL = 'https://confluence.sentry.com/display/PLBI/Personal+Lines+Business+Intelligence+'
WHERE Title_DSC = 'Training'

UPDATE BusinessAreaTile
SET Order_SEQ = 2
WHERE Title_DSC = 'Business Intelligence'

UPDATE BusinessAreaTile
SET Order_SEQ = 3
WHERE Title_DSC = 'PL Data Questions'

UPDATE BusinessAreaTile
SET Order_SEQ = 4
WHERE Title_DSC = 'Training'

UPDATE BusinessAreaTile
SET Order_SEQ = 5, Image_NME = 'Meeting3.jpg'
WHERE Title_DSC = 'PL Data Services Business Dashboard'