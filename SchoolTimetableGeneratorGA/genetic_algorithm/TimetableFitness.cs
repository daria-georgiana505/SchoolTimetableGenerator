using GeneticSharp;
using SchoolTimetableGeneratorGA.models;

namespace SchoolTimetableGeneratorGA.genetic_algorithm;

public class TimetableFitness: IFitness
{
    public double Evaluate(IChromosome chromosome)
    {
        var timetable = chromosome as TimetableChromosome;
        if (timetable == null)
        {
            throw new ArgumentException("Expected TimetableChromosome instance.");
        }

        double fitness = 1000;
        int penalty = 0;
        
        var schedule = timetable.Schedule;
        
        //HARD CONSTRAINT: Check if a teacher has overlapping timeslots
        var teacherTimeslotConflicts = schedule
            .GroupBy(t => t.TeacherId)
            .SelectMany(group => CheckOverlappingTimeslots(group.ToList()));
        penalty += teacherTimeslotConflicts.Count() * 10;
        
        //HARD CONSTRAINT: Check if a student group has overlapping timeslots
        var studentGroupTimeslotConflicts = schedule
            .GroupBy(t => t.StudentGroupId)
            .SelectMany(group => CheckOverlappingTimeslots(group.ToList()));
        penalty += studentGroupTimeslotConflicts.Count() * 10;
        
        //HARD CONSTRAINT: Check if a room has overlapping timeslots
        var roomTimeslotConflicts = schedule
            .GroupBy(t => t.RoomId)
            .SelectMany(group => CheckOverlappingTimeslots(group.ToList()));
        penalty += roomTimeslotConflicts.Count() * 10;
        
        //SOFT CONSTRAINT: The number of working hours for teachers are evenly spread
        var teachersWorkingHours = schedule
            .GroupBy(t => t.TeacherId)
            .Select(group => ComputeNumberOfWorkingHours(group.ToList()))
            .ToList();
        if (teachersWorkingHours.Count > 1)
        {
            penalty += teachersWorkingHours.Max() - teachersWorkingHours.Min();
        }
        
        //SOFT CONSTRAINT: Minimize gaps between scheduled courses for teachers
        var teacherGaps = schedule
            .GroupBy(t => t.TeacherId)
            .Sum(group => CalculateGapsInSchedule(group.ToList()));
        penalty += teacherGaps;
        
        //SOFT CONSTRAINT: Minimize gaps between scheduled courses for student groups
        var studentGaps = schedule
            .GroupBy(t => t.StudentGroupId)
            .Sum(group => CalculateGapsInSchedule(group.ToList()));
        penalty += studentGaps;
        
        fitness -= penalty;
        
        return Math.Max(0, fitness);
    }
    
    private IEnumerable<Timeslot> CheckOverlappingTimeslots(List<Timeslot> timeslots)
    {
        var conflicts = new List<Timeslot>();

        var sortedSlots = timeslots.OrderBy(t => t.Day).ThenBy(t => t.Start).ToList();

        for (int i = 0; i < sortedSlots.Count - 1; i++)
        {
            var current = sortedSlots[i];
            var next = sortedSlots[i + 1];

            if (current.Day == next.Day && current.End > next.Start)
            {
                conflicts.Add(current);
                conflicts.Add(next);
            }
        }

        return conflicts.Distinct();
    }
    
    private int CalculateGapsInSchedule(List<Timeslot> timeslots)
    {
        int totalGaps = 0;

        var sortedSlots = timeslots.OrderBy(t => t.Day).ThenBy(t => t.Start).ToList();

        for (int i = 0; i < sortedSlots.Count - 1; i++)
        {
            var current = sortedSlots[i];
            var next = sortedSlots[i + 1];

            if (current.Day == next.Day && current.End < next.Start)
            {
                totalGaps += (int)(next.Start - current.End).TotalMinutes;
            }
        }

        return totalGaps;
    }

    private int ComputeNumberOfWorkingHours(List<Timeslot> timeslots)
    {
        return timeslots.Sum(t => (int)(t.End - t.Start).TotalHours);
    }
}