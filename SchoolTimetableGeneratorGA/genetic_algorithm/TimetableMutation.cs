using GeneticSharp;

namespace SchoolTimetableGeneratorGA.genetic_algorithm;

public class TimetableMutation: IMutation
{
    public bool IsOrdered  => false;
    
    public void Mutate(IChromosome chromosome, float probability)
    {
        var timetableChromosome = chromosome as TimetableChromosome;

        if (timetableChromosome == null)
        {
            throw new ArgumentException("Expected a TimetableChromosome.");
        }
        
        var random = RandomizationProvider.Current;
        if (!(random.GetDouble() <= probability))
        {
            return;
        }
        var geneIndex = random.GetInt(0, timetableChromosome.Length);
        timetableChromosome.ReplaceGene(geneIndex, timetableChromosome.GenerateGene(geneIndex));
    }
}