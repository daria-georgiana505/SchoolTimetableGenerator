using GeneticSharp;

namespace SchoolTimetableGeneratorGA.genetic_algorithm;

public class TimetableCrossover: ICrossover
{
    public bool IsOrdered => false;
    
    public IList<IChromosome> Cross(IList<IChromosome> parents)
    {
        var parent1 = parents[0] as TimetableChromosome;
        var parent2 = parents[1] as TimetableChromosome;

        if (parent1 == null || parent2 == null)
        {
            throw new ArgumentException("Expected TimetableChromosome instances.");
        }

        var random = RandomizationProvider.Current;

        int crossoverPoint = random.GetInt(1, parent1.Length - 1);

        var child1 = parent1.CreateNew() as TimetableChromosome;
        var child2 = parent2.CreateNew() as TimetableChromosome;
        
        if (child1 == null || child2 == null)
        {
            throw new InvalidOperationException("Failed to create new TimetableChromosome instances.");
        }

        for (int i = 0; i < parent1.Length; i++)
        {
            if (i < crossoverPoint)
            {
                child1.ReplaceGene(i, parent1.GetGene(i));
                child2.ReplaceGene(i, parent2.GetGene(i));
            }
            else
            {
                child1.ReplaceGene(i, parent2.GetGene(i));
                child2.ReplaceGene(i, parent1.GetGene(i));
            }
        }

        return new List<IChromosome> { child1, child2 };
    }

    public int ParentsNumber => 2;
    public int ChildrenNumber => 2;
    public int MinChromosomeLength => 2;
}