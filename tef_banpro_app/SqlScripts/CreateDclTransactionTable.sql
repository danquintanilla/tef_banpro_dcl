CREATE TABLE dbo.DCL_TRANSACTION
(
	MerchantId NVARCHAR(10) NOT NULL,
	TerminalId NVARCHAR(8) NOT NULL,
	TransactionType NVARCHAR(20) NOT NULL,
	Invoice NVARCHAR(6) NOT NULL,
	ExternalReference NVARCHAR(20) NULL,
	CurrencySymbol NVARCHAR(5) NULL,
	TransactionDate NVARCHAR(8) NULL,
	TransactionTime NVARCHAR(6) NULL,
	ReferenceNumber NVARCHAR(12) NULL,
	AuthorizationNumber NVARCHAR(6) NULL,
	SystemTraceNumber NVARCHAR(6) NULL,
	Amount NVARCHAR(20) NULL,
	TaxAmount NVARCHAR(20) NULL,
	MaskedPan NVARCHAR(30) NULL,
	CardHolderName NVARCHAR(50) NULL,
	Closed BIT NOT NULL DEFAULT(0),
	CreationDate DATETIME NOT NULL DEFAULT(GETDATE()),
	ClosedDate DATETIME NULL,
	CONSTRAINT PK_DCL_TRANSACTION PRIMARY KEY (MerchantId, TerminalId, TransactionType, Invoice)
)
GO