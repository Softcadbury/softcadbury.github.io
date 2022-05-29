IF OBJECT_ID('dbo.GetItems', 'P') IS NOT NULL
	DROP PROCEDURE dbo.GetItems
GO

CREATE PROCEDURE dbo.GetItems

AS
BEGIN
    SET NOCOUNT ON;

	SELECT Id
	FROM Items

END
GO