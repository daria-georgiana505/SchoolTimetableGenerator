namespace SchoolTimetableGeneratorGA.models;

public class Room
{
    public int Id { get; set; }
    private string Name { get; set; }

    public Room(int id, string name)
    {
        Id = id;
        Name = name;
    }
    
    public override string ToString()
    {
        return "Room ID: " + Id + ", Name: " + Name;
    }
}