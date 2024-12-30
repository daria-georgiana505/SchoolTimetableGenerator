using System.Text;

namespace SchoolTimetableGeneratorGA.printer;

public class PerformanceMeasurementPrinter
{
    public static void LogPerformanceMetrics(
        string filename,
        int teacherCount,
        int courseCount,
        int groupCount,
        int roomCount,
        int timeslotCount,
        TimeSpan executionTime,
        double bestChromosomeFitness)
    {
        string folderPath = AppDomain.CurrentDomain.BaseDirectory;

        if (folderPath.Contains("bin" + Path.DirectorySeparatorChar + "Debug"))
        {
            folderPath = folderPath.Substring(0, folderPath.LastIndexOf("bin" + Path.DirectorySeparatorChar + "Debug"));
        }

        folderPath = Path.Combine(folderPath, "files");

        Directory.CreateDirectory(folderPath);

        string logFilePath = Path.Combine(folderPath, filename);
        
        StringBuilder logContent = new StringBuilder();
        logContent.AppendLine($"-- PERFORMANCE METRICS #{DateTime.Now} --");
        logContent.AppendLine($"Number of Teachers: {teacherCount}");
        logContent.AppendLine($"Number of Courses: {courseCount}");
        logContent.AppendLine($"Number of Groups: {groupCount}");
        logContent.AppendLine($"Number of Rooms: {roomCount}");
        logContent.AppendLine($"Number of Timeslots: {timeslotCount}");
        logContent.AppendLine($"Execution Time: {executionTime}");
        logContent.AppendLine($"Best chromosome fitness: {bestChromosomeFitness}");
        logContent.AppendLine("---------------------------------------------\n");

        File.AppendAllText(logFilePath, logContent.ToString());
    }
}