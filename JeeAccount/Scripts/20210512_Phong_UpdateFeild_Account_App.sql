/*
   Wednesday, May 12, 202112:55:00 PM
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
ALTER TABLE dbo.Account_App ADD
	IsActive bit NULL,
	ActivatedDate datetime NULL,
	ActivatedBy numeric(18, 0) NULL,
	InActiveDate datetime NULL,
	InActiveBy numeric(18, 0) NULL,
	IsAdmin bit NULL
GO
ALTER TABLE dbo.Account_App SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.Account_App', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.Account_App', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.Account_App', 'Object', 'CONTROL') as Contr_Per 