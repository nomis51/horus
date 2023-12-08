namespace Horus.Models;

public class StoreModel
{
    public string Name { get; }
    public bool IsSeparator => Name == "$separator";
    public bool IsCreateButton => Name == "$createButton";
    public bool IsStore => !IsSeparator && !IsCreateButton;

    public StoreModel(string name)
    {
        Name = name;
    }
}