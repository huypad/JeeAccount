/*
   Monday, April 26, 202110:07:02 AM
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
CREATE TABLE dbo.Tbl_Submenu
	(
	Id_row numeric(18, 0) NOT NULL IDENTITY (1, 1),
	Title nvarchar(500) NULL,
	Summary nvarchar(500) NULL,
	AllowPermit numeric(18, 0) NULL,
	GroupName nvarchar(50) NULL,
	Module nvarchar(50) NULL,
	CustemerID numeric(18, 0) NULL,
	Target nvarchar(50) NULL,
	Position int NULL,
	Hienthi bit NULL,
	Soluottruycap int NULL,
	GFunctionID int NULL,
	DependentMenuID numeric(18, 0) NULL,
	ALink nvarchar(500) NULL,
	Icon nvarchar(50) NULL,
	AppLink nvarchar(500) NULL,
	AppIcon nvarchar(50) NULL,
	HTMLUserGuide ntext NULL,
	Phanloai1 int NULL,
	Phanloai2 int NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.Tbl_Submenu ADD CONSTRAINT
	PK_Tbl_Submenu PRIMARY KEY CLUSTERED 
	(
	Id_row
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.Tbl_Submenu SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
select Has_Perms_By_Name(N'dbo.Tbl_Submenu', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.Tbl_Submenu', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.Tbl_Submenu', 'Object', 'CONTROL') as Contr_Per 