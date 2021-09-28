/*
	Update CLA3048_StandardizeOnUTCTime feature flag value from 0\1 to False\True
*/

Update FeatureEntity
SET Value = 'True'
where KeyCol = 'CLA3048_StandardizeOnUTCTime' and Value = '1'

Update FeatureEntity
SET Value = 'False'
where KeyCol = 'CLA3048_StandardizeOnUTCTime' and Value = '0'