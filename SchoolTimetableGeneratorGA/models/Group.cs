namespace SchoolTimetableGeneratorGA.models;

public class Group
{
    public int Id { get; set; }
    public string Name { get; set; }

    public Group(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public override string ToString()
    {
        return "Group ID: " + Id + ", Name: " + Name;
    }
}