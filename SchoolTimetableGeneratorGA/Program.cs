
using MPI;
using SchoolTimetableGeneratorGA.printer;

using (new MPI.Environment(ref args))
{
    var world = Communicator.world;

    if (world.Rank == 0)
    {
        Console.WriteLine("--SCHOOL TIMETABLE GENERATOR--");
    }
    MenuPrinter.ShowMenu();
}