/*
   Saturday, May 22, 20217:19:09 AM
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
ALTER TABLE dbo.CustomerList
	DROP CONSTRAINT DF_CustomerList_Status
GO
CREATE TABLE dbo.Tmp_CustomerList
	(
	RowID numeric(18, 0) NOT NULL,
	Code varchar(50) NULL,
	CompanyName nvarchar(500) NULL,
	RegisterName nvarchar(500) NULL,
	Address nvarchar(500) NULL,
	Phone nvarchar(500) NULL,
	RegisterDate datetime NULL,
	Status int NULL,
	Disable bit NULL,
	DeletedDate datetime NULL,
	DeletedBy numeric(18, 0) NULL,
	Note ntext NULL,
	LogoImgURL nvarchar(500) NULL,
	TaxCode nvarchar(500) NULL,
	LinhvucID int NULL,
	SmtpClient nvarchar(50) NULL,
	Port int NULL,
	EmailAddress nvarchar(50) NULL,
	Password nvarchar(500) NULL,
	EnableSSL bit NULL,
	Username nvarchar(50) NULL,
	PartnerID int NULL,
	Gender nvarchar(50) NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_CustomerList SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.Tmp_CustomerList ADD CONSTRAINT
	DF_CustomerList_Status DEFAULT ((1)) FOR Status
GO
IF EXISTS(SELECT * FROM dbo.CustomerList)
	 EXEC('INSERT INTO dbo.Tmp_CustomerList (RowID, Code, CompanyName, RegisterName, Address, Phone, RegisterDate, Status, Disable, DeletedDate, DeletedBy, Note, LogoImgURL, TaxCode, LinhvucID, SmtpClient, Port, EmailAddress, Password, EnableSSL, Username, PartnerID, Gender)
		SELECT RowID, Code, CompanyName, RegisterName, Address, Phone, RegisterDate, Status, Disable, DeletedDate, DeletedBy, Note, LogoImgURL, TaxCode, LinhvucID, SmtpClient, Port, EmailAddress, Password, EnableSSL, Username, PartnerID, Gender FROM dbo.CustomerList WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE dbo.CustomerList
GO
EXECUTE sp_rename N'dbo.Tmp_CustomerList', N'CustomerList', 'OBJECT' 
GO
ALTER TABLE dbo.CustomerList ADD CONSTRAINT
	PK_CustomerList PRIMARY KEY CLUSTERED 
	(
	RowID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT
select Has_Perms_By_Name(N'dbo.CustomerList', 'Object', 'ALTER') as ALT_Per, Has_Perms_By_Name(N'dbo.CustomerList', 'Object', 'VIEW DEFINITION') as View_def_Per, Has_Perms_By_Name(N'dbo.CustomerList', 'Object', 'CONTROL') as Contr_Per 