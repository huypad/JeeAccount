/*
   Saturday, July 31, 20212:28:32 PM
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
CREATE TABLE dbo.JobtitleList
	(
	RowID int NOT NULL IDENTITY (1, 1),
	CustomerID int NOT NULL,
	JobtitleName nvarchar(500) NULL,
	CreatedDate datetime NULL,
	CreatedBy nvarchar(50) NULL,
	LastModified datetime NULL,
	ModifiedBy nvarchar(50) NULL,
	Disable bit NULL,
	DeletedDate datetime NULL,
	DeletedBy nvarchar(50) NULL,
	Note nvarchar(2000) NULL,
	IsActive bit NULL,
	ActiveDate datetime NULL,
	ActiveBy nvarchar(50) NULL,
	DeActiveDate datetime NULL,
	DeActiveBy nvarchar(50) NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.JobtitleList ADD CONSTRAINT
	PK_JobtitleList PRIMARY KEY CLUSTERED 
	(
	RowID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.JobtitleList SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.JobtitleList', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.JobtitleList', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.JobtitleList', 'Object', 'CONTROL') as Contr_Per 