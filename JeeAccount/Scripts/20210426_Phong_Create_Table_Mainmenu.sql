/*
   Monday, April 26, 202110:02:08 AM
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
CREATE TABLE dbo.Mainmenu
	(
	Id_row numeric(18, 0) NOT NULL IDENTITY (1, 1),
	Title nvarchar(500) NULL,
	Summary nvarchar(500) NULL,
	Code nvarchar(500) NULL,
	CustemerID numeric(18, 0) NULL,
	Target nvarchar(50) NULL,
	Position int NULL,
	Visible bit NULL,
	Icon nvarchar(50) NULL,
	ALink nvarchar(500) NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.Mainmenu ADD CONSTRAINT
	PK_Mainmenu PRIMARY KEY CLUSTERED 
	(
	Id_row
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.Mainmenu SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.Mainmenu', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.Mainmenu', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.Mainmenu', 'Object', 'CONTROL') as Contr_Per 