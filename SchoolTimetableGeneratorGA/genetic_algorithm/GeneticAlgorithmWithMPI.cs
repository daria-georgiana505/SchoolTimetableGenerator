using System.Diagnostics;
using GeneticSharp;
using MessagePack;
using MPI;

namespace SchoolTimetableGeneratorGA.genetic_algorithm;


[MessagePackObject]
public partial class GeneticAlgorithmWithMPI: IGeneticAlgorithm
{
    [Key(0)]
    private IPopulation _population;
    [Key(1)]
    private IFitness _fitness;
    [Key(2)]
    private ISelection _selection;
    [Key(3)]
    private ICrossover _crossover;
    [Key(4)]
    private IMutation _mutation;
    [Key(5)]
    private float _crossoverProbability;
    [Key(6)]
    private float _mutationProbability;
    [Key(7)]
    private IReinsertion _reinsertion;
    [Key(8)]
    private ITermination _termination;
    [IgnoreMember]
    private Stopwatch _stopwatch = new Stopwatch();

    public GeneticAlgorithmWithMPI()
    {
    }
    
    public GeneticAlgorithmWithMPI(
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
    
    public GeneticAlgorithmWithMPI(
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
        if (Communicator.world.Rank == 0)
        {
            this.MasterThread();
        }
        else
        {
            this.WorkerThread();
        }
    }

    private void MasterThread()
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

        for (int workerRank = 1; workerRank < Communicator.world.Size; workerRank++)
        {
            byte[] serializedData = MessagePackSerializer.Serialize(new List<IChromosome>());
            Communicator.world.Send(serializedData, workerRank, 1);
        }
    }

    private void WorkerThread()
    {
        while (true)
        {
            byte[] serializedDataSubpopulation = Communicator.world.Receive<byte[]>(0, 1);
            IList<IChromosome> chromosomes = MessagePackSerializer.Deserialize<IList<IChromosome>>(serializedDataSubpopulation);
            if (chromosomes.Count == 0) 
                break;

            IList<double> computedFitnessesFromWorker = this.EvaluateFitness(chromosomes);
            
            byte[] serializedDataFitnesses = MessagePackSerializer.Serialize(computedFitnessesFromWorker);
            Communicator.world.Send(serializedDataFitnesses, 0, 2);
        }
    }

    private IList<IChromosome> ReturnListOfChromosomesForEachWorker(int rank, int numberChromosomesPerWorker)
    {
        int startIndex = rank * numberChromosomesPerWorker;
        int endIndex = Math.Min(_population.CurrentGeneration.Chromosomes.Count, (rank + 1) * numberChromosomesPerWorker);
        return _population.CurrentGeneration.Chromosomes
            .Skip(startIndex)
            .Take(endIndex - startIndex)
            .ToList();
    }

    private void SetComputedFitnessesFromWorkersToTheirAssignedSubpopulation(int rank, int numberChromosomesPerWorker, IList<double> computedFitnesses)
    {
        int startIndex = rank * numberChromosomesPerWorker;
        for (int i = 0; i < computedFitnesses.Count; i++)
        {
            _population.CurrentGeneration.Chromosomes[startIndex + i].Fitness = computedFitnesses[i];
        }
    }
    
    private void EvolveOneGeneration()
    {
        int numberChromosomesPerWorker = (_population.CurrentGeneration.Chromosomes.Count + Communicator.world.Size - 1) / Communicator.world.Size;
        for (int workerRank = 1; workerRank < Communicator.world.Size; workerRank++)
        {
            IList<IChromosome> subPopulation = this.ReturnListOfChromosomesForEachWorker(workerRank, numberChromosomesPerWorker);
            byte[] serializedDataSubpopulation = MessagePackSerializer.Serialize(subPopulation);
            Communicator.world.Send(serializedDataSubpopulation, workerRank, 1);
        }

        IList<IChromosome> subPopulationForMasterProcess = ReturnListOfChromosomesForEachWorker(0, numberChromosomesPerWorker);
        IList<double> masterComputedFitnesses = EvaluateFitness(subPopulationForMasterProcess);
        this.SetComputedFitnessesFromWorkersToTheirAssignedSubpopulation(0, numberChromosomesPerWorker, masterComputedFitnesses);
        
        for (int workerRank = 1; workerRank < Communicator.world.Size; workerRank++)
        {
            byte[] serializedDataFitnesses = Communicator.world.Receive<byte[]>(workerRank, 2);
            IList<double> workerComputedFitnesses = MessagePackSerializer.Deserialize<IList<double>>(serializedDataFitnesses);
            this.SetComputedFitnessesFromWorkersToTheirAssignedSubpopulation(workerRank, numberChromosomesPerWorker, workerComputedFitnesses);
        }
        
        this._population.EndCurrentGeneration();
        
        IList<IChromosome> parents = this.SelectParents();
        IList<IChromosome> offspring = this.PerformCrossover(parents);
        this.MutateAllChromosomes(offspring, _mutationProbability);
        this._population.CreateNewGeneration(this.Reinsert(offspring, parents)); 
    }
    
    private IList<double> EvaluateFitness(IList<IChromosome> chromosomes)
    {
        IList<double> computedFitnesses = new List<double>();
        foreach (var chromosome in chromosomes)
        {
            if (!chromosome.Fitness.HasValue)
            {
                computedFitnesses.Add(this._fitness.Evaluate(chromosome));
            }
            else
            {
                computedFitnesses.Add(chromosome.Fitness.Value);
            }
        }
        return computedFitnesses;
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
    
    [IgnoreMember]
    public int GenerationsNumber => this._population.GenerationsNumber;

    [IgnoreMember]
    public IChromosome BestChromosome => this._population.BestChromosome;
    
    [IgnoreMember]
    public TimeSpan TimeEvolving { get; private set; }
}