--*******************************************************************************************************************
--INSERT records into BusinessArea_Subscription to auto subscribe people
--select * from EventType
--select * from BusinessArea_Subscription
--delete from BusinessArea_Subscription where Subscription_ID = 48
--********************************************************************************************************************


/*******************************************************************************************************************************************
#BusinessArea_Subscription to hold all people taht should be auto subscribed
******************************************************************************************************************************************/		
IF OBJECT_ID('tempDB..#BusinessArea_Subscription', 'U') IS NOT NULL BEGIN 	DROP TABLE #BusinessArea_Subscription END
CREATE TABLE #BusinessArea_Subscription
(
	BusinessArea_ID int NOT NULL,
	EventType_ID int NOT NULL,
	Interval_ID int NOT NULL,
	SentryOwner_NME varchar(128) NOT NULL
);
INSERT INTO #BusinessArea_Subscription
(
	BusinessArea_ID,
	EventType_ID,
	Interval_ID,
	SentryOwner_NME
)
SELECT	1, 27, 1, '072186'	--UNION ALL
--SELECT	1, 27, 1, 'XXXXX'
;


/*******************************************************************************************************************************************
-MERGE people into current BusinessArea_Subscription table
-if they already have an entry for Critical AKA EventType_ID=27 then it will update them to instant
******************************************************************************************************************************************/		
MERGE	BusinessArea_Subscription			AS TARGET
USING	#BusinessArea_Subscription			AS SOURCE 
		ON		TARGET.BusinessArea_ID		= SOURCE.BusinessArea_ID
				AND TARGET.EventType_ID		= SOURCE.EventType_ID	
				AND TARGET.SentryOwner_NME	= SOURCE.SentryOwner_NME
				 

WHEN	MATCHED AND TARGET.Interval_ID <> SOURCE.Interval_ID
THEN	UPDATE 
		SET TARGET.Interval_ID = SOURCE.Interval_ID

WHEN	NOT MATCHED BY TARGET 
THEN	INSERT 	
		(
			BusinessArea_ID,
			EventType_ID,
			Interval_ID,
			SentryOwner_NME
		)
		VALUES 
		(
			SOURCE.BusinessArea_ID
			,SOURCE.EventType_ID
			,SOURCE.Interval_ID
			,SOURCE.SentryOwner_NME
		)
;