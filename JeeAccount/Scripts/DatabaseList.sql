/*
   Monday, May 17, 20214:37:04 PM
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
ALTER TABLE dbo.DatabaseList ADD
	Username nvarchar(500) NULL,
	Password nvarchar(500) NULL
GO
ALTER TABLE dbo.DatabaseList SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.DatabaseList', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.DatabaseList', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.DatabaseList', 'Object', 'CONTROL') as Contr_Per 