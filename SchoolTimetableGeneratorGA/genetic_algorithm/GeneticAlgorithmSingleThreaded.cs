using System.Diagnostics;
using GeneticSharp;

namespace SchoolTimetableGeneratorGA.genetic_algorithm;

public class GeneticAlgorithmSingleThreaded : IGeneticAlgorithm
{
    private readonly IPopulation _population;
    private readonly IFitness _fitness;
    private readonly ISelection _selection;
    private readonly ICrossover _crossover;
    private readonly IMutation _mutation;
    private float _crossoverProbability;
    private readonly float _mutationProbability;
    private readonly IReinsertion _reinsertion;
    private readonly ITermination _termination;
    private readonly Stopwatch _stopwatch = new Stopwatch();
    
    public GeneticAlgorithmSingleThreaded(
        IPopulation population,
        IFitness fitness,
        ISelection selection,
        ICrossover crossover,
        float crossoverProbability,
        IMutation mutation,
        float mutationProbability)
    {
        _population = population;
        _fitness = fitness;
        _selection = selection;
        _crossover = crossover;
        _mutation = mutation;
        _crossoverProbability = crossoverProbability;
        _mutationProbability = mutationProbability;
        TimeEvolving = TimeSpan.Zero;
        _reinsertion = new ElitistReinsertion();
        _termination = new FitnessStagnationTermination(100);
    }
    
    public GeneticAlgorithmSingleThreaded(
        IPopulation population,
        IFitness fitness,
        ISelection selection,
        ICrossover crossover,
        IMutation mutation)
    {
        _population = population;
        _fitness = fitness;
        _selection = selection;
        _crossover = crossover;
        _mutation = mutation;
        _crossoverProbability = 0.75f;
        _mutationProbability = 0.2f;
        TimeEvolving = TimeSpan.Zero;
        _reinsertion = new ElitistReinsertion();
        _termination = new FitnessStagnationTermination(100);
    }
    
    public void Start()
    {
        _stopwatch.Restart();
        
        _population.CreateInitialGeneration();
        
        if (_population.GenerationsNumber == 0)
            throw new InvalidOperationException("The number of generations must be greater than 0.");

        do
        {
            EvolveOneGeneration();
        } while (!_termination.HasReached(this));
        
        _stopwatch.Stop();
        TimeEvolving = _stopwatch.Elapsed;
    }
    
    private void EvolveOneGeneration()
    {
        EvaluateFitness();
        
        _population.EndCurrentGeneration();
        
        var parents = SelectParents();
        var offspring = PerformCrossover(parents);
        MutateAllChromosomes(offspring, _mutationProbability);
        _population.CreateNewGeneration(Reinsert(offspring, parents)); 
    }

    private void EvaluateFitness()
    {
        foreach (var chromosome in _population.CurrentGeneration.Chromosomes)
        {
            if (!chromosome.Fitness.HasValue)
            {
                chromosome.Fitness = _fitness.Evaluate(chromosome);
            }
        }
    }

    private List<IChromosome> PerformCrossover(IList<IChromosome> parents)
    {
        if (parents.Count % 2 != 0)
        {
            throw new InvalidOperationException("Number of parents must be even.");
        }
        
        var crossoverList = new List<IChromosome>();
        
        for (var i = 0; i < parents.Count; i += 2)
        {
            var parent1 = parents[i];
            var parent2 = parents[i + 1];

            crossoverList.AddRange(_crossover.Cross(new List<IChromosome> { parent1, parent2 }));
        }
        
        return crossoverList;
    }
    
    private void MutateAllChromosomes(IList<IChromosome> chromosomes, float mutationProbability)
    {
        foreach (var chromosome in chromosomes)
        {
            _mutation.Mutate(chromosome, mutationProbability);
        }
    }
    
    private List<IChromosome> SelectParents()
    {
        var parentList = new List<IChromosome>();
        
        for(var i = 0; i < (_population.MinSize / 2) + 1; i++)
        {
            parentList.AddRange(_selection.SelectChromosomes(2, _population.CurrentGeneration));
        }

        return parentList;
    }
    
    private IList<IChromosome> Reinsert(IList<IChromosome> offspring, IList<IChromosome> parents)
    {
        return _reinsertion.SelectChromosomes(_population, offspring, parents);
    }

    public int GenerationsNumber => _population.GenerationsNumber;
    public IChromosome BestChromosome => _population.BestChromosome;
    public TimeSpan TimeEvolving { get; private set; }
}