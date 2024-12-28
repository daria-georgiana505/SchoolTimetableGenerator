using OfficeOpenXml;
using SchoolTimetableGeneratorGA.genetic_algorithm;

namespace SchoolTimetableGeneratorGA.printer;

public class TimetablePrinter
{
    public static void PrintToExcel(
        TimetableChromosome timetableChromosome, 
        List<DayOfWeek> daysOfWeek, 
        List<(TimeSpan Start, TimeSpan End)> timeslots
    )
    {
        string folderPath = AppDomain.CurrentDomain.BaseDirectory;

        if (folderPath.Contains("bin" + Path.DirectorySeparatorChar + "Debug"))
        {
            folderPath = folderPath.Substring(0, folderPath.LastIndexOf("bin" + Path.DirectorySeparatorChar + "Debug"));
        }

        folderPath = Path.Combine(folderPath, "files");

        Directory.CreateDirectory(folderPath);

        var filePath = Path.Combine(folderPath, "Timetable.xlsx");

        var timetable = new Dictionary<DayOfWeek, List<(TimeSpan Start, TimeSpan End, int CourseId, int TeacherId, int RoomId, int GroupId)>>();

        foreach (var day in daysOfWeek)
        {
            timetable[day] = new List<(TimeSpan, TimeSpan, int, int, int, int)>();
        }

        foreach (var timeslot in timetableChromosome.Schedule)
        {
            timetable[timeslot.Day].Add((
                timeslot.Start, 
                timeslot.End, 
                timeslot.CourseId, 
                timeslot.TeacherId, 
                timeslot.RoomId, 
                timeslot.StudentGroupId));
        }

        var fileInfo = new FileInfo(filePath);
        using (var package = new ExcelPackage(fileInfo))
        {
            var worksheet = package.Workbook.Worksheets["Timetable"];
            if (worksheet != null)
            {
                package.Workbook.Worksheets.Delete("Timetable");
            }

            worksheet = package.Workbook.Worksheets.Add("Timetable");

            worksheet.Cells[1, 1].Value = "Time";
            int colIndex = 2;
            foreach (var day in daysOfWeek)
            {
                worksheet.Cells[1, colIndex].Value = day.ToString();
                colIndex++;
            }

            int rowIndex = 2;
            foreach (var timeslot in timeslots)
            {
                var timeSlotString = $"{timeslot.Item1:hh\\:mm} - {timeslot.Item2:hh\\:mm}";
                worksheet.Cells[rowIndex, 1].Value = timeSlotString;

                colIndex = 2;
                foreach (var day in daysOfWeek)
                {
                    var matchingTimeslots = timetable[day].Where(ts => ts.Start == timeslot.Item1 && ts.End == timeslot.Item2).ToList();

                    if (matchingTimeslots.Any())
                    {
                        var classesDetails = string.Join("\n\n", matchingTimeslots.Select(ts =>
                            $"Course: {ts.CourseId}\nGroup: {ts.GroupId}\nRoom: {ts.RoomId}\nTeacher: {ts.TeacherId}"));
                        
                        worksheet.Cells[rowIndex, colIndex].Value = classesDetails;
                    }
                    else
                    {
                        worksheet.Cells[rowIndex, colIndex].Value = "";
                    }
                    colIndex++;
                }

                rowIndex++;
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            package.Save();
        }

        Console.WriteLine($"Timetable saved to: {filePath}");
    }
}