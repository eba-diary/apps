/********************************************************************************************************************
--INSERT records into BusinessArea_Subscription to auto subscribe people
--select * from EventType
--select * from BusinessArea_Subscription
--delete from BusinessArea_Subscription where Subscription_ID = 48

select SentryOwner_NME 
from #BusinessArea_Subscription
group by SentryOwner_NME
--having count(*) > 1;


*********************************************************************************************************************/


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
SELECT	1, 27, 1, '072186'	UNION ALL
SELECT  1, 27, 1,'067233' UNION ALL   --  JessicaAndreae
SELECT  1, 27, 1,'070361' UNION ALL   --  PeterAnhalt
SELECT  1, 27, 1,'078102' UNION ALL   --  ChadBalsiger
SELECT  1, 27, 1,'077081' UNION ALL   --  JenniferBarthels
SELECT  1, 27, 1,'069306' UNION ALL   --  BenjaminBehnke
SELECT  1, 27, 1,'081716' UNION ALL   --  DylanBernklau
SELECT  1, 27, 1,'083549' UNION ALL   --  ErikBock
SELECT  1, 27, 1,'072495' UNION ALL   --  MarkBonk
SELECT  1, 27, 1,'081058' UNION ALL   --  ErinBretzman
SELECT  1, 27, 1,'070199' UNION ALL   --  AmyBrunner
SELECT  1, 27, 1,'079295' UNION ALL   --  MaxCarlson
SELECT  1, 27, 1,'075435' UNION ALL   --  StephanieCranney
SELECT  1, 27, 1,'070969' UNION ALL   --  MichaelDietry
SELECT  1, 27, 1,'078573' UNION ALL   --  DavidDoty
SELECT  1, 27, 1,'082545' UNION ALL   --  RogerFrazier
SELECT  1, 27, 1,'068551' UNION ALL   --  JoshuaGarbe
SELECT  1, 27, 1,'083094' UNION ALL   --  Gaustad Dave											
SELECT  1, 27, 1,'066004' UNION ALL   --  BrookeGibson


SELECT  1, 27, 1,'080550' UNION ALL   --  Gladem Jenny
SELECT  1, 27, 1,'070367' UNION ALL   --  DawnGold
SELECT  1, 27, 1,'056144' UNION ALL   --  JohnGundersen
SELECT  1, 27, 1,'070556' UNION ALL   --  LeahHermanson
SELECT  1, 27, 1,'067573' UNION ALL   --  Hinchcliffe Mike
SELECT  1, 27, 1,'070363' UNION ALL   --  StevenHornacek
SELECT  1, 27, 1,'070992' UNION ALL   --  BethHoward
SELECT  1, 27, 1,'082657' UNION ALL   --  KyleJastromski
SELECT  1, 27, 1,'070423' UNION ALL   --  MatthewJensema
SELECT  1, 27, 1,'070321' UNION ALL   --  Kautzer Rick
SELECT  1, 27, 1,'077688' UNION ALL   --  JasonLam
SELECT  1, 27, 1,'081621' UNION ALL   --  DevonMaier
SELECT  1, 27, 1,'070358' UNION ALL   --  Marsden Steve
SELECT  1, 27, 1,'081259' UNION ALL   --  DylanMcCorkle
SELECT  1, 27, 1,'083721' UNION ALL   --  MichaelMcCoy
SELECT  1, 27, 1,'082744' UNION ALL   --  MichaelMilbourn


SELECT  1, 27, 1,'070100' UNION ALL   --  JasonMillar
SELECT  1, 27, 1,'074276' UNION ALL   --  JasonMills
SELECT  1, 27, 1,'082976' UNION ALL   --  ScottMueller
SELECT  1, 27, 1,'077128' UNION ALL   --  LucasMuzynoski
SELECT  1, 27, 1,'074481' UNION ALL   --  BrentNewport
SELECT  1, 27, 1,'083870' UNION ALL   --  GeraldPace
SELECT  1, 27, 1,'078723' UNION ALL   --  WilliamPeck
SELECT  1, 27, 1,'083172' UNION ALL   --  StephenPelkofer
SELECT  1, 27, 1,'082123' UNION ALL   --  Peterson Joey
SELECT  1, 27, 1,'080456' UNION ALL   --  MarinaRaymond
SELECT  1, 27, 1,'082494' UNION ALL   --  JennaRifner
SELECT  1, 27, 1,'084498' UNION ALL   --  StephenRifner
SELECT  1, 27, 1,'073055' UNION ALL   --  PeterSampson
SELECT  1, 27, 1,'079012' UNION ALL   --  SarahSchneider


SELECT  1, 27, 1,'078973' UNION ALL   --  BenjaminSchuelke
SELECT  1, 27, 1,'078877' UNION ALL   --  AustinSchulz
SELECT  1, 27, 1,'082930' UNION ALL   --  ZacharySeeman
SELECT  1, 27, 1,'083188' UNION ALL   --  Shaffer Brady
SELECT  1, 27, 1,'082396' UNION ALL   --  StephanieSpencer
SELECT  1, 27, 1,'083607' UNION ALL   --  KaylaStaniszewski
SELECT  1, 27, 1,'079430' UNION ALL   --  DanielleSuch
SELECT  1, 27, 1,'079711' UNION ALL   --  DavidSullivan
SELECT  1, 27, 1,'070617' UNION ALL   --  CurtisTemplin
SELECT  1, 27, 1,'052092' UNION ALL   --  TracieTer Maat
SELECT  1, 27, 1,'071395' UNION ALL   --  KyleTkachuk
SELECT  1, 27, 1,'070557' UNION ALL   --  EthanVaade
SELECT  1, 27, 1,'070229' UNION ALL   --  MonicaVann
SELECT  1, 27, 1,'083379' UNION ALL   --  JoshuaWagner
SELECT  1, 27, 1,'080074' UNION ALL   --  RaychelWatters
SELECT  1, 27, 1,'071872' UNION ALL   --  Wong Michelle
SELECT  1, 27, 1,'071011' UNION ALL   --  RobertYeiser
SELECT  1, 27, 1,'083934' UNION ALL   --  Ann-MarieZahn
SELECT  1, 27, 1,'079787'    --  Zeske Katie
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