using System.Diagnostics;
using GeneticSharp;

namespace SchoolTimetableGeneratorGA.genetic_algorithm;

public class GeneticAlgorithmWithTasks: IGeneticAlgorithm
{
    private IPopulation _population;
    private IFitness _fitness;
    private ISelection _selection;
    private ICrossover _crossover;
    private IMutation _mutation;
    private float _crossoverProbability;
    private float _mutationProbability;
    private IReinsertion _reinsertion;
    private ITermination _termination;
    private Stopwatch _stopwatch = new Stopwatch();
    
    public GeneticAlgorithmWithTasks(
        IPopulation population,
        IFitness fitness,
        ISelection selection,
        ICrossover crossover,
        float crossoverProbability,
        IMutation mutation,
        float mutationProbability)
    {
        this._population = population;
        this._fitness = fitness;
        this._selection = selection;
        this._crossover = crossover;
        this._mutation = mutation;
        this._crossoverProbability = crossoverProbability;
        this._mutationProbability = mutationProbability;
        this.TimeEvolving = TimeSpan.Zero;
        this._reinsertion = new ElitistReinsertion();
        this._termination = new FitnessStagnationTermination(100);
    }
    
    public GeneticAlgorithmWithTasks(
        IPopulation population,
        IFitness fitness,
        ISelection selection,
        ICrossover crossover,
        IMutation mutation)
    {
        this._population = population;
        this._fitness = fitness;
        this._selection = selection;
        this._crossover = crossover;
        this._mutation = mutation;
        this._crossoverProbability = 0.75f;
        this._mutationProbability = 0.2f;
        this.TimeEvolving = TimeSpan.Zero;
        this._reinsertion = new ElitistReinsertion();
        this._termination = new FitnessStagnationTermination(100);
    }
    
    public async Task Start()
    {
        _stopwatch.Restart();
        
        this._population.CreateInitialGeneration();
        
        if (this._population.GenerationsNumber == 0)
            throw new InvalidOperationException("The number of generations must be greater than 0.");

        do
        {
            await this.EvolveOneGeneration();
        } while (!this._termination.HasReached(this));
        
        _stopwatch.Stop();
        this.TimeEvolving = _stopwatch.Elapsed;
    }
    
    private async Task EvolveOneGeneration()
    {
        await this.EvaluateFitness();
        
        this._population.EndCurrentGeneration();
        
        IList<IChromosome> parents = await this.SelectParents();
        IList<IChromosome> offspring = await this.PerformCrossover(parents);
        await this.MutateAllChromosomes(offspring, _mutationProbability);
        this._population.CreateNewGeneration(this.Reinsert(offspring, parents)); 
    }

    private async Task EvaluateFitness()
    {
        var fitnessTasks = this._population.CurrentGeneration.Chromosomes
            .Where(c => !c.Fitness.HasValue)
            .Select(c => Task.Run(() => c.Fitness = this._fitness.Evaluate(c)));
        
        await Task.WhenAll(fitnessTasks);
    }

    private async Task<IList<IChromosome>> PerformCrossover(IList<IChromosome> parents)
    {
        if (parents.Count % 2 != 0)
        {
            throw new InvalidOperationException("Number of parents must be even.");
        }
        
        var crossoverTasks = new List<Task<IList<IChromosome>>>();
        
        for (int i = 0; i < parents.Count; i += 2)
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
            .Select(c => Task.Run(() => this._mutation.Mutate(c, mutationProbability)));
        
        await Task.WhenAll(mutationTasks);
    }
    
    private async Task<IList<IChromosome>> SelectParents()
    {
        var parentTasks = new List<Task<IList<IChromosome>>>();
        
        for(int i = 0; i < (this._population.MinSize / 2) + 1; i++)
        {
            parentTasks.Add(Task.Run(() => this._selection.SelectChromosomes(2, this._population.CurrentGeneration)));
        }
        
        await Task.WhenAll(parentTasks);
        
        return parentTasks.SelectMany(t => t.Result).ToList();
    }
    
    private IList<IChromosome> Reinsert(IList<IChromosome> offspring, IList<IChromosome> parents)
    {
        return this._reinsertion.SelectChromosomes(this._population, offspring, parents);
    }

    public int GenerationsNumber => this._population.GenerationsNumber;

    public IChromosome BestChromosome => this._population.BestChromosome;
    public TimeSpan TimeEvolving { get; private set; }
}