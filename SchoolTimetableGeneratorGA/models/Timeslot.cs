namespace SchoolTimetableGeneratorGA.models;

public class Timeslot
{
    public int CourseId { get; set; }
    public int TeacherId { get; set; }
    public int RoomId { get; set; }
    public int StudentGroupId { get; set; }
    public DayOfWeek Day { get; set; }
    public TimeSpan Start { get; set; }
    public TimeSpan End { get; set; }

    public Timeslot(int courseId, int teacherId, int roomId, int studentGroupId, DayOfWeek day, TimeSpan start, TimeSpan end)
    {
        CourseId = courseId;
        TeacherId = teacherId;
        RoomId = roomId;
        StudentGroupId = studentGroupId;
        Day = day;
        Start = start;
        End = end;
    }

    public override string ToString()
    {
        return "Course ID: " + CourseId + " | Teacher ID: " + TeacherId + " | Room ID: " + RoomId + " | Group ID: " + StudentGroupId+ " | Day: " + Day + " | Start Time: " + Start + " | End Time: " + End;
    }
}