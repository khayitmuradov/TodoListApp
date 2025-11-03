namespace TodoListApp.WebApp.Models;

public class TagItem
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string ColorHex { get; set; } = "#EEEEEE";
}

public class CreateTagRequest
{
    public string Name { get; set; } = string.Empty;
}
