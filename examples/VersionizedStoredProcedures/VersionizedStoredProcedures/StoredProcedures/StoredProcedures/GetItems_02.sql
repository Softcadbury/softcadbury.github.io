IF OBJECT_ID('dbo.GetItems', 'P') IS NOT NULL
	DROP PROCEDURE dbo.GetItems
GO

CREATE PROCEDURE dbo.GetItems
	@label VARCHAR(max)

AS
BEGIN
    SET NOCOUNT ON;

	SELECT Id, Label
	FROM Items
	WHERE label = @label

END
GO