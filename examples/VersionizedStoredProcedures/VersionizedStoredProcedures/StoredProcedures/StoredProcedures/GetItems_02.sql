CREATE OR ALTER PROCEDURE dbo.GetItems
	@label VARCHAR(max)
AS
BEGIN
    SET NOCOUNT ON;

	SELECT Id, Label
	FROM Items
	WHERE label = @label
END
GO