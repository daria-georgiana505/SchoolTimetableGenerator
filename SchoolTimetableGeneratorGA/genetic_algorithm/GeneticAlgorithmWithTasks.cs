using System.Diagnostics;
using GeneticSharp;

namespace SchoolTimetableGeneratorGA.genetic_algorithm;

public class GeneticAlgorithmWithTasks: IGeneticAlgorithm
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
    
    public GeneticAlgorithmWithTasks(
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
    
    public GeneticAlgorithmWithTasks(
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
    
    public async Task Start()
    {
        _stopwatch.Restart();
        
        _population.CreateInitialGeneration();
        
        if (_population.GenerationsNumber == 0)
            throw new InvalidOperationException("The number of generations must be greater than 0.");

        do
        {
            await EvolveOneGeneration();
        } while (!_termination.HasReached(this));
        
        _stopwatch.Stop();
        TimeEvolving = _stopwatch.Elapsed;
    }
    
    private async Task EvolveOneGeneration()
    {
        await EvaluateFitness();
        
        _population.EndCurrentGeneration();
        
        var parents = await SelectParents();
        var offspring = await PerformCrossover(parents);
        await MutateAllChromosomes(offspring, _mutationProbability);
        _population.CreateNewGeneration(Reinsert(offspring, parents)); 
    }

    private async Task EvaluateFitness()
    {
        var fitnessTasks = _population.CurrentGeneration.Chromosomes
            .Where(c => !c.Fitness.HasValue)
            .Select(c => Task.Run(() => c.Fitness = _fitness.Evaluate(c)));
        
        await Task.WhenAll(fitnessTasks);
    }

    private async Task<IList<IChromosome>> PerformCrossover(IList<IChromosome> parents)
    {
        if (parents.Count % 2 != 0)
        {
            throw new InvalidOperationException("Number of parents must be even.");
        }
        
        var crossoverTasks = new List<Task<IList<IChromosome>>>();
        
        for (var i = 0; i < parents.Count; i += 2)
        {
            var parent1 = parents[i];
            var parent2 = parents[i + 1];

            crossoverTasks.Add(Task.Run(() =>
                _crossover.Cross(new List<IChromosome> { parent1, parent2 })
            ));
        }
        
        await Task.WhenAll(crossoverTasks);
        
        return crossoverTasks.SelectMany(t => t.Result).ToList();
    }
    
    private async Task MutateAllChromosomes(IList<IChromosome> chromosomes, float mutationProbability)
    {
        var mutationTasks = chromosomes
            .Select(c => Task.Run(() => _mutation.Mutate(c, mutationProbability)));
        
        await Task.WhenAll(mutationTasks);
    }
    
    private async Task<IList<IChromosome>> SelectParents()
    {
        var parentTasks = new List<Task<IList<IChromosome>>>();
        
        for(var i = 0; i < (_population.MinSize / 2) + 1; i++)
        {
            parentTasks.Add(Task.Run(() => _selection.SelectChromosomes(2, _population.CurrentGeneration)));
        }
        
        await Task.WhenAll(parentTasks);
        
        return parentTasks.SelectMany(t => t.Result).ToList();
    }
    
    private IList<IChromosome> Reinsert(IList<IChromosome> offspring, IList<IChromosome> parents)
    {
        return _reinsertion.SelectChromosomes(_population, offspring, parents);
    }

    public int GenerationsNumber => _population.GenerationsNumber;

    public IChromosome BestChromosome => _population.BestChromosome;
    public TimeSpan TimeEvolving { get; private set; }
}