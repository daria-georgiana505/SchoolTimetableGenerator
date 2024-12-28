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
        this._reinsertion = (IReinsertion) new ElitistReinsertion();
        this._termination = (ITermination) new FitnessStagnationTermination(10);
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
        this._mutationProbability = 0.1f;
        this.TimeEvolving = TimeSpan.Zero;
        this._reinsertion = (IReinsertion) new ElitistReinsertion();
        this._termination = (ITermination) new FitnessStagnationTermination(10);
    }

    public void Start()
    {
        this._population.CreateInitialGeneration();
        
        if (this._population.GenerationsNumber == 0)
            throw new InvalidOperationException("The number of generations must be greater than 0.");
        
        this.EvaluateFitness();

        do
        {
            this.EvolveOneGeneration();
        } while (!this._termination.HasReached(this));
    }
    
    private void EvolveOneGeneration()
    {
        IList<IChromosome> parents = this.SelectParents();
        IList<IChromosome> offspring = _crossover.Cross(parents);
        this.MutateAllChromosomes(offspring, _mutationProbability);
        this._population.CreateNewGeneration(this.Reinsert(offspring, parents)); 
        this.EndCurrentGeneration();
    }
    
    private void EndCurrentGeneration()
    {
        this.EvaluateFitness();
        this._population.EndCurrentGeneration();
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
    
    private void MutateAllChromosomes(IList<IChromosome> chromosomes, float mutationProbability)
    {
        foreach (var chromosome in chromosomes)
        {
            _mutation.Mutate(chromosome, mutationProbability);
        }
    }
    
    private IList<IChromosome> SelectParents()
    {
        return this._selection.SelectChromosomes(this._population.MinSize, this._population.CurrentGeneration);
    }
    
    private IList<IChromosome> Reinsert(IList<IChromosome> offspring, IList<IChromosome> parents)
    {
        return this._reinsertion.SelectChromosomes(this._population, offspring, parents);
    }

    public int GenerationsNumber => this._population.GenerationsNumber;

    public IChromosome BestChromosome => this._population.BestChromosome;
    public TimeSpan TimeEvolving { get; private set; }
}