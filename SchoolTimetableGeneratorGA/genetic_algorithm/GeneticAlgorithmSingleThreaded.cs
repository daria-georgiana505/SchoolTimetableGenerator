using System.Diagnostics;
using GeneticSharp;

namespace SchoolTimetableGeneratorGA.genetic_algorithm;

public class GeneticAlgorithmSingleThreaded : IGeneticAlgorithm
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
    
    public GeneticAlgorithmSingleThreaded(
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
    
    public GeneticAlgorithmSingleThreaded(
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
    
    public void Start()
    {
        _stopwatch.Restart();
        
        this._population.CreateInitialGeneration();
        
        if (this._population.GenerationsNumber == 0)
            throw new InvalidOperationException("The number of generations must be greater than 0.");

        do
        {
            this.EvolveOneGeneration();
        } while (!this._termination.HasReached(this));
        
        _stopwatch.Stop();
        this.TimeEvolving = _stopwatch.Elapsed;
    }
    
    private void EvolveOneGeneration()
    {
        this.EvaluateFitness();
        
        this._population.EndCurrentGeneration();
        
        IList<IChromosome> parents = this.SelectParents();
        IList<IChromosome> offspring = this.PerformCrossover(parents);
        this.MutateAllChromosomes(offspring, _mutationProbability);
        this._population.CreateNewGeneration(this.Reinsert(offspring, parents)); 
    }

    private void EvaluateFitness()
    {
        foreach (var chromosome in this._population.CurrentGeneration.Chromosomes)
        {
            if (!chromosome.Fitness.HasValue)
            {
                chromosome.Fitness = this._fitness.Evaluate(chromosome);
            }
        }
    }

    private IList<IChromosome> PerformCrossover(IList<IChromosome> parents)
    {
        if (parents.Count % 2 != 0)
        {
            throw new InvalidOperationException("Number of parents must be even.");
        }
        
        var crossoverList = new List<IChromosome>();
        
        for (int i = 0; i < parents.Count; i += 2)
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
    
    private IList<IChromosome> SelectParents()
    {
        var parentList = new List<IChromosome>();
        
        for(int i = 0; i < (this._population.MinSize / 2) + 1; i++)
        {
            parentList.AddRange(this._selection.SelectChromosomes(2, this._population.CurrentGeneration));
        }

        return parentList;
    }
    
    private IList<IChromosome> Reinsert(IList<IChromosome> offspring, IList<IChromosome> parents)
    {
        return this._reinsertion.SelectChromosomes(this._population, offspring, parents);
    }

    public int GenerationsNumber => this._population.GenerationsNumber;

    public IChromosome BestChromosome => this._population.BestChromosome;
    public TimeSpan TimeEvolving { get; private set; }
}