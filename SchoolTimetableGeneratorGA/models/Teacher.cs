using System.Diagnostics.CodeAnalysis;

namespace SchoolTimetableGeneratorGA.models;

public class Teacher
{
    public int Id { get; set; }
    private string FirstName { get; set; }
    private string LastName { get; set; }
    // public int DesiredNumberOfWorkingHours { get; set; }

    [SuppressMessage("ReSharper", "ConvertToPrimaryConstructor")]
    public Teacher(int id, string firstName, string lastName)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
    }
    
    public override string ToString()
    {
        return "Teacher ID: " + Id + ", First Name: " + FirstName + ", Last Name: " + LastName;
    }
}