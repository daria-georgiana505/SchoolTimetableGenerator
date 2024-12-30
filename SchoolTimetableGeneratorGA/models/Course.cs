namespace SchoolTimetableGeneratorGA.models;

public class Course
{
    public int Id { get; set; }
    private string Name { get; set; }

    public Course(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public override string ToString()
    {
        return "Course ID: " + Id + ", Name: " + Name;
    }
}