
using GeneticSharp;
using SchoolTimetableGeneratorGA.genetic_algorithm;
using SchoolTimetableGeneratorGA.models;
using SchoolTimetableGeneratorGA.printer;
using SchoolTimetableGeneratorGA.test_data;

Console.WriteLine("--SCHOOL TIMETABLE GENERATOR--");

List<Course> courses = TestDataGenerator.GenerateCourses(6);
List<Group> groups = TestDataGenerator.GenerateGroups(7);
List<Room> rooms = TestDataGenerator.GenerateRooms(3);
List<Teacher> teachers = TestDataGenerator.GenerateTeachers(8);
List<DayOfWeek> daysOfWeek = new List<DayOfWeek>
    { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };
List<(TimeSpan, TimeSpan)> timeslots = TestDataGenerator.GenerateTimeslots(8, 14);

IChromosome adamChromosome = new TimetableChromosome(
    courses.Count() * groups.Count(),
    courses.Select(c => c.Id).ToList(),
    teachers.Select(t => t.Id).ToList(),
    rooms.Select(r => r.Id).ToList(),
    groups.Select(g => g.Id).ToList(),
    daysOfWeek,
    timeslots
    ).CreateNew();
Population population = new Population(
    50, 100, adamChromosome
);
IFitness fitness = new TimetableFitness();
// ISelection selection = new EliteSelection();
ISelection selection = new TournamentSelection();
ICrossover crossover = new TimetableCrossover();
IMutation mutation = new TimetableMutation();
GeneticAlgorithmWithTasks ga = new GeneticAlgorithmWithTasks(
    population, fitness, selection, crossover, mutation, adamChromosome
);
ga.Start();

TimetableChromosome result = ga.BestChromosome as TimetableChromosome;

TimetablePrinter.PrintToExcel(result, daysOfWeek, timeslots);

Console.WriteLine("Press any key to exit...");
Console.ReadKey();