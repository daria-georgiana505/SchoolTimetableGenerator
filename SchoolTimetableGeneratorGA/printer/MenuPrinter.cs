using GeneticSharp;
using SchoolTimetableGeneratorGA.genetic_algorithm;
using SchoolTimetableGeneratorGA.models;
using SchoolTimetableGeneratorGA.test_data;

namespace SchoolTimetableGeneratorGA.printer;

public class MenuPrinter
{
    public static void ShowMenu()
    {
        while(true)
            PrintMenu();
    }
    
    private static void PrintMenu()
    {
        Console.WriteLine();
        Console.WriteLine("~ MENU ~");
        Console.WriteLine();
        PrintMenuOptions();
        Console.WriteLine();
        Console.WriteLine("Select an option: ");
        var option = Convert.ToInt32(Console.ReadLine());
        Console.WriteLine();
        ExecuteSelectedMenuOption(option);
    }

    private static void PrintMenuOptions()
    {
        Console.WriteLine("0. Exit");
        Console.WriteLine("1. Single threaded genetic algorithm");
        Console.WriteLine("2. Genetic algorithm using tasks");
        Console.WriteLine("3. Genetic algorithm using MPI");
    }

    private static void ExecuteSelectedMenuOption(int option)
    {
        switch (option)
        {
            case 1:
            case 2:
                PrintSubmenu(option);
                break;
            case 3:
                // PrintSubmenu(option);
                Console.WriteLine("To be implemented...");
                break;
            case 0:
                Console.WriteLine("Exiting the program...");
                Environment.Exit(0);
                break;
            default:
                Console.WriteLine("Invalid option");
                break;
        }
    }

    private static void PrintSubmenuOptions()
    {
        Console.WriteLine("0. Exit");
        Console.WriteLine("1. Run algorithm with 6 courses, 7 groups, 3 rooms, 8 teachers and 6 timeslots");
        Console.WriteLine("2. Run algorithm with 10 courses, 12 groups, 5 rooms, 15 teachers and 10 timeslots");
        Console.WriteLine("3. Run algorithm with custom data");
    }

    private static void PrintSubmenu(int menuOption)
    {
        PrintSubmenuOptions();
        Console.WriteLine();
        Console.WriteLine("Select an option: ");
        var option = Convert.ToInt32(Console.ReadLine());
        Console.WriteLine();
        ExecuteSelectedSubmenuOption(option, menuOption);
    }

    private static void ExecuteSelectedSubmenuOption(int option, int menuOption)
    {
        List<Course> courses;
        List<Group> groups;
        List<Room> rooms;
        List<Teacher> teachers;
        List<DayOfWeek> daysOfWeek = Enum.GetValues<DayOfWeek>().ToList();
        List<(TimeSpan, TimeSpan)> timeslots;

        switch (option)
        {
            case 1:
                (courses, groups, rooms, teachers, timeslots) = GenerateTestData(6, 7, 3, 8, 8, 14);
                RunSelectedGeneticAlgorithm(menuOption, courses, groups, rooms, teachers, timeslots, daysOfWeek);
                break;
            case 2:
                (courses, groups, rooms, teachers, timeslots) = GenerateTestData(10, 12, 5, 15, 8, 18);
                RunSelectedGeneticAlgorithm(menuOption, courses, groups, rooms, teachers, timeslots, daysOfWeek);
                break;
            case 3:
                var (coursesCount, groupsCount, roomsCount, teachersCount, timeslotsStartHour, timeslotsEndHour) = ReadCustomTestDataFromUser();
                (courses, groups, rooms, teachers, timeslots) = GenerateTestData(coursesCount, groupsCount, roomsCount, teachersCount, timeslotsStartHour, timeslotsEndHour);
                RunSelectedGeneticAlgorithm(menuOption, courses, groups, rooms, teachers, timeslots, daysOfWeek);
                break;
            case 0:
                return;
            default:
                Console.WriteLine("Invalid option");
                break;
        }
    }

    private static (int coursesCount, int groupsCount, int roomsCount, int teachersCount, int timeslotsStartHour, int timeslotsEndHour) ReadCustomTestDataFromUser()
    {
        static int GetInput(string prompt)
        {
            Console.WriteLine(prompt);
            return Convert.ToInt32(Console.ReadLine());
        }

        var coursesCount = GetInput("Input number of courses: ");
        var groupsCount = GetInput("Input number of groups: ");
        var roomsCount = GetInput("Input number of rooms: ");
        var teachersCount = GetInput("Input number of teachers: ");
        var timeslotsStartHour = GetInput("Input starting hour for timeslots: ");
        var timeslotsEndHour = GetInput("Input ending hour for timeslots: ");

        return (coursesCount, groupsCount, roomsCount, teachersCount, timeslotsStartHour, timeslotsEndHour);
    }


    private static (List<Course>, List<Group>, List<Room>, List<Teacher>, List<(TimeSpan, TimeSpan)>) GenerateTestData(int coursesCount, int groupsCount, int roomsCount, int teachersCount, int timeslotsStartHour, int timeslotsEndHour)
    {
        var courses = TestDataGenerator.GenerateCourses(coursesCount);
        var groups = TestDataGenerator.GenerateGroups(groupsCount);
        var rooms = TestDataGenerator.GenerateRooms(roomsCount);
        var teachers = TestDataGenerator.GenerateTeachers(teachersCount);
        var timeslots = TestDataGenerator.GenerateTimeslots(timeslotsStartHour, timeslotsEndHour);

        return (courses, groups, rooms, teachers, timeslots);
    }

    private static void RunSelectedGeneticAlgorithm(int menuOption, List<Course> courses, List<Group> groups, List<Room> rooms, List<Teacher> teachers, List<(TimeSpan, TimeSpan)> timeslots, List<DayOfWeek> daysOfWeek)
    {
        switch (menuOption)
        {
            case 1:
                RunGeneticAlgorithmSingleThreaded(courses, groups, rooms, teachers, timeslots, daysOfWeek);
                break;
            case 2:
                RunGeneticAlgorithmWithTasks(courses, groups, rooms, teachers, timeslots, daysOfWeek);
                break;
            case 3:
                RunGeneticAlgorithmWithMPI(courses, groups, rooms, teachers, timeslots, daysOfWeek);
                break;
            default:
                Console.WriteLine("Invalid option");
                break;
        }
    }
    
    private static (Population population, IFitness fitness, ISelection selection, ICrossover crossover, IMutation mutation) InitializeGeneticAlgorithm(
        List<Course> courses, 
        List<Group> groups, 
        List<Room> rooms, 
        List<Teacher> teachers, 
        List<(TimeSpan, TimeSpan)> timeslots, 
        List<DayOfWeek> daysOfWeek)
    {
        var adamChromosome = new TimetableChromosome(
            courses.Count * groups.Count,
            courses.Select(c => c.Id).ToList(),
            teachers.Select(t => t.Id).ToList(),
            rooms.Select(r => r.Id).ToList(),
            groups.Select(g => g.Id).ToList(),
            daysOfWeek,
            timeslots
        ).CreateNew();

        Population population = new Population(500, 1000, adamChromosome);
        IFitness fitness = new TimetableFitness();
        ISelection selection = new TournamentSelection();
        ICrossover crossover = new TimetableCrossover();
        IMutation mutation = new TimetableMutation();

        return (population, fitness, selection, crossover, mutation);
    }

    private static void RunGeneticAlgorithmSingleThreaded(List<Course> courses, List<Group> groups, List<Room> rooms, List<Teacher> teachers, List<(TimeSpan, TimeSpan)> timeslots, List<DayOfWeek> daysOfWeek)
    {
        var (population, fitness, selection, crossover, mutation) = InitializeGeneticAlgorithm(
            courses, groups, rooms, teachers, timeslots, daysOfWeek);

        GeneticAlgorithmSingleThreaded ga = new GeneticAlgorithmSingleThreaded(population, fitness, selection, crossover, mutation);
        ga.Start();

        const string filename = "SingleThreadPerformanceLog.txt";
        HandleTimetableResult(
            filename,
            ga.BestChromosome as TimetableChromosome, 
            ga.TimeEvolving, 
            teachers, 
            courses, 
            groups, 
            rooms, 
            timeslots, 
            daysOfWeek);
    }
    
    private static void RunGeneticAlgorithmWithMPI(List<Course> courses, List<Group> groups, List<Room> rooms, List<Teacher> teachers, List<(TimeSpan, TimeSpan)> timeslots, List<DayOfWeek> daysOfWeek)
    {
        var (population, fitness, selection, crossover, mutation) = InitializeGeneticAlgorithm(
            courses, groups, rooms, teachers, timeslots, daysOfWeek);

        //Initialization and execution of genetic algorithm
        
        //Write results to log and Excel files
    }

    private static void RunGeneticAlgorithmWithTasks(List<Course> courses, List<Group> groups, List<Room> rooms, List<Teacher> teachers, List<(TimeSpan, TimeSpan)> timeslots, List<DayOfWeek> daysOfWeek)
    {
        var (population, fitness, selection, crossover, mutation) = InitializeGeneticAlgorithm(
            courses, groups, rooms, teachers, timeslots, daysOfWeek);

        GeneticAlgorithmWithTasks ga = new GeneticAlgorithmWithTasks(population, fitness, selection, crossover, mutation);
        ga.Start().Wait();

        var filename = "TaskMethodPerformanceLog.txt";
        HandleTimetableResult(
            filename,
            ga.BestChromosome as TimetableChromosome, 
            ga.TimeEvolving, 
            teachers, 
            courses, 
            groups, 
            rooms, 
            timeslots, 
            daysOfWeek);
    }
    
    private static void HandleTimetableResult(
        string filename,
        TimetableChromosome? result, 
        TimeSpan executionTime, 
        List<Teacher> teachers, 
        List<Course> courses, 
        List<Group> groups, 
        List<Room> rooms, 
        List<(TimeSpan, TimeSpan)> timeslots, 
        List<DayOfWeek> daysOfWeek)
    {
        if (result != null && result.Fitness != null)
        {
            Console.WriteLine("Best chromosome fitness: " + result.Fitness);
            Console.WriteLine("Execution time: " + executionTime);
            PerformanceMeasurementPrinter.LogPerformanceMetrics(
                filename,
                teachers.Count,
                courses.Count,
                groups.Count,
                rooms.Count,
                timeslots.Count,
                executionTime,
                result.Fitness.Value);
            Console.WriteLine($"Performance measurement saved to log file: {filename}");
            TimetablePrinter.PrintToExcel(result, daysOfWeek, timeslots);
            Console.WriteLine("Timetable saved");
        }
        else
        {
            Console.WriteLine("Timetable chromosome generation failed.");
        }
    }
}