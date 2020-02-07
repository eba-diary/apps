--*******************************************************************************************************************
--HISTORY FIX SCRIPT TO REMOVE DUPLICATE Dataset_Subscription ENTRIES
--********************************************************************************************************************


--*******************************************************************************************************************
--DELETE DUPLICATE SUBSCRIPTIONS
--2 Rows DELETED
--*******************************************************************************************************************
DELETE Dataset_Subscription
--SELECT Dataset_Subscription.*
FROM Dataset_Subscription Dataset_Subscription
JOIN
(
	SELECT	MAX(Dataset_Subscription.Subscription_ID)			AS Subscription_ID
			,Dataset_Subscription.Dataset_ID					AS Dataset_ID
			,Dataset_Subscription.EventType_ID					AS EventType_ID
			,Dataset_Subscription.SentryOwner_NME				AS SentryOwner_NME

	FROM Dataset_Subscription Dataset_Subscription
	
	GROUP BY	Dataset_Subscription.Dataset_ID
				,Dataset_Subscription.EventType_ID
				,Dataset_Subscription.SentryOwner_NME

	HAVING COUNT(*) > 1

) DATASET_SUBSCRIPTION_VICTIM
	ON	Dataset_Subscription.Subscription_ID				= DATASET_SUBSCRIPTION_VICTIM.Subscription_ID
		AND Dataset_Subscription.Dataset_ID					= DATASET_SUBSCRIPTION_VICTIM.Dataset_ID
		AND Dataset_Subscription.EventType_ID				= DATASET_SUBSCRIPTION_VICTIM.EventType_ID
		AND Dataset_Subscription.SentryOwner_NME			= DATASET_SUBSCRIPTION_VICTIM.SentryOwner_NME




--*******************************************************************************************************************
--DELETE OBSOLETE DATASET SUBSCRIPTIONS to EVENTTYPE where EventType=2 AKA Bundle File Process
--SELECT * FROM EventType EventType WHERE Type_ID = 2
--7 Rows DELETED
--*******************************************************************************************************************
DELETE Dataset_Subscription
--SELECT Dataset_Subscription.*
FROM Dataset_Subscription Dataset_Subscription
JOIN
(
	SELECT	Dataset_Subscription.Subscription_ID			AS Subscription_ID
	
	FROM Dataset_Subscription Dataset_Subscription
	
	JOIN EventType EventType
		ON	Dataset_Subscription.EventType_ID				= EventType.Type_ID

	WHERE EventType.Description = 'Bundle File Process'
	
) DATASET_SUBSCRIPTION_VICTIM
	ON	Dataset_Subscription.Subscription_ID				= DATASET_SUBSCRIPTION_VICTIM.Subscription_ID
