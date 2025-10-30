namespace TodoListApp.WebApi.Models;

public class TagModel
{
    public int Id { get; set; }

    public string Name { get; set; } = default!;

    public string ColorHex { get; set; } = default!;
}

public class CreateTagModel
{
    public string Name { get; set; } = default!;
}
