/*
   Saturday, July 31, 202110:34:26 PM
   User: jeeaccount
   Server: 202.143.110.154,1443
   Database: JeeAccount
   Application: 
*/

/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.AccountList ADD
	JobtitleID int NULL
GO
ALTER TABLE dbo.AccountList SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.AccountList', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.AccountList', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.AccountList', 'Object', 'CONTROL') as Contr_Per 