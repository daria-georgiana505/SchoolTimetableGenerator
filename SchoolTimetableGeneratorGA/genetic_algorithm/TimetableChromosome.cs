using GeneticSharp;
using SchoolTimetableGeneratorGA.models;

namespace SchoolTimetableGeneratorGA.genetic_algorithm;

public class TimetableChromosome: ChromosomeBase
{
    public List<Timeslot> Schedule => GetScheduleFromGenes();
    
    private readonly List<int> _courseIds;
    private readonly List<int> _teacherIds;
    private readonly List<int> _roomIds;
    private readonly List<int> _studentGroupIds;
    private readonly List<DayOfWeek> _days;
    private readonly List<(TimeSpan start, TimeSpan end)> _timeSlots;
    
    public TimetableChromosome(int length, List<int> courseIds, List<int> teacherIds, List<int> roomIds, 
        List<int> studentGroupIds, List<DayOfWeek> days, List<(TimeSpan start, TimeSpan end)> timeSlots)
        : base(length)
    {
        _courseIds = courseIds;
        _teacherIds = teacherIds;
        _roomIds = roomIds;
        _studentGroupIds = studentGroupIds;
        _days = days;
        _timeSlots = timeSlots;

        for (var i = 0; i < length; i++)
        {
            ReplaceGene(i, GenerateGene(i));
        }
    }
    
    public override IChromosome Clone()
    {
        var clone = new TimetableChromosome(Length, _courseIds, _teacherIds, _roomIds, _studentGroupIds, _days, _timeSlots);
        for (var i = 0; i < Length; i++)
        {
            clone.ReplaceGene(i, GetGene(i));
        }
        return clone;
    }

    public override Gene GenerateGene(int geneIndex)
    {
        var random = RandomizationProvider.Current;

        var courseId = random.GetInt(_courseIds.Min(), _courseIds.Max() + 1);
        var teacherId = random.GetInt(_teacherIds.Min(), _teacherIds.Max() + 1);
        var roomId = random.GetInt(_roomIds.Min(), _roomIds.Max() + 1);
        var studentGroupId = random.GetInt(_studentGroupIds.Min(), _studentGroupIds.Max() + 1);
        var day = _days[random.GetInt(0, _days.Count)];
        var timeSlot = _timeSlots[random.GetInt(0, _timeSlots.Count)];

        return new Gene(new Timeslot
        (
            courseId,
            teacherId,
            roomId,
            studentGroupId,
            day,
            timeSlot.start,
            timeSlot.end
        ));
    }

    public override IChromosome CreateNew()
    {
        return new TimetableChromosome(Length, _courseIds, _teacherIds, _roomIds, _studentGroupIds, _days, _timeSlots);
    }
    
    private List<Timeslot> GetScheduleFromGenes()
    {
        return GetGenes()
            .Select(g => (Timeslot)g.Value)
            .ToList();
    }
}