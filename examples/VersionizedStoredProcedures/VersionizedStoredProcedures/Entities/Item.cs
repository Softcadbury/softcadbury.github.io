namespace VersionizedStoredProcedures.Entities;

/// <summary>
/// Simple entity that will represent the Items table in SQL server
/// </summary>
public class Item
{
    public int Id { get; set; }

    public string Label { get; set; }

    public Item(string label)
    {
        Label = label;
    }
}