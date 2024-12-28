using GeneticSharp;
using SchoolTimetableGeneratorGA.models;

namespace SchoolTimetableGeneratorGA.test_data;

public class TestDataGenerator
{
    public static List<Course> GenerateCourses(int count)
    {
        var courses = new List<Course>();
        for (int i = 1; i <= count; i++)
        {
            courses.Add(new Course(i, $"Course {i}"));
        }
        return courses;
    }

    public static List<Group> GenerateGroups(int count)
    {
        var groups = new List<Group>();
        for (int i = 1; i <= count; i++)
        {
            groups.Add(new Group(i, $"Group {i}"));
        }
        return groups;
    }

    public static List<Room> GenerateRooms(int count)
    {
        var rooms = new List<Room>();
        for (int i = 1; i <= count; i++)
        {
            rooms.Add(new Room(i, $"Room {i}"));
        }
        return rooms;
    }

    public static List<Teacher> GenerateTeachers(int count)
    {
        var teachers = new List<Teacher>();
        for (int i = 1; i <= count; i++)
        {
            teachers.Add(new Teacher(i, $"Teacher {i} First Name", $"Teacher {i} Last Name"));
        }
        return teachers;
    }

    public static List<(TimeSpan, TimeSpan)> GenerateTimeslots(int startHour, int endHour)
    {
        var timeslots = new List<(TimeSpan, TimeSpan)>();
        for (int hour = startHour; hour < endHour; hour++)
        {
            TimeSpan startTime = TimeSpan.FromHours(hour);
            TimeSpan endTime = TimeSpan.FromHours(hour + 1);
            timeslots.Add((startTime, endTime));
        }
        return timeslots;
    }
}
