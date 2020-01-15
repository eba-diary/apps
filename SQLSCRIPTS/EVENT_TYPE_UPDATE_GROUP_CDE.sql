select * from Dataset
select * from EventType
select * from BusinessArea
select * from BusinessArea_Subscription where SentryOwner_NME = '072186'
select * from Dataset_Subscription where SentryOwner_NME = '072186' and Dataset_ID = 195;
select * from IntervalType



/**********************************************************************************************************************************

**********************************************************************************************************************************/

/*
--STEP 1:  UPDATE EXISTING rows
UPDATE EventType
SET Group_CDE = 'DATASET'


--STEP 2:	INSERT ROW FOR NOTIFICATION
INSERT INTO EventType
SELECT	MAX(Type_ID) + 1		AS 'Type_ID'
		,'Notification'			AS 'Description'
		,1						AS 'Severity'
		,1						AS 'Display_IND'
		,'BUSINESSAREA'			AS 'Group_CDE'
FROM EventType
*/


INSERT INTO BusinessArea_Subscription
SELECT	1				AS 'Subscription_ID'
		,1				AS 'BusinessArea_ID'
		,27				AS 'EventType_ID'
		,1				AS 'Interval_ID'
		,'072186'		AS 'SentryOwner_NME'