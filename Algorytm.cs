using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Projekt_ZP
{
    public class Algorytm
    {
        private List<int> instance; 
        private Random random;
        private int _populationSize, _generationCount, _tournamentSize;
        private double _crossingOver, _mutation;
        private int _maxStagnationGenerations;

        public Algorytm(List<int> instance, int populationSize, int generationCount,
                      double crossingOver, double mutation, int tournamentSize, int maxGenerations)
        {
            this.instance = instance;
            _populationSize = populationSize;
            _generationCount = generationCount;
            _crossingOver = crossingOver;
            _mutation = mutation;
            _tournamentSize = tournamentSize;
            random = new Random();
            _maxStagnationGenerations = maxGenerations;
        }


        public int FitnessFunction(List<int> individual)
        {
            HashSet<int> differences = new HashSet<int>();
            bool allInInstance = true;

            for (int i = 0; i < individual.Count; i++)
            {
                for (int j = i + 1; j < individual.Count; j++)
                {
                    int diff = Math.Abs(individual[i] - individual[j]);
                    differences.Add(diff);
                    if (!instance.Contains(diff)) allInInstance = false;
                }
            }

            int missing = instance.Count(d => !differences.Contains(d));
            double fitness = (1.5 * differences.Count) - (2.0 * missing) + (allInInstance ? 10 : 0);
            return (int)Math.Max(fitness, 0);
        }



        public List<List<int>> Tournament(List<List<int>> population, int tournamentSize)
        {
            List<List<int>> selected = new List<List<int>>();
            while (selected.Count < population.Count)
            {
                var tournament = population.OrderBy(x => random.Next()).Take(tournamentSize);
                selected.Add(tournament.OrderByDescending(ind => FitnessFunction(ind)).First());
            }
            return selected;
        }

        public List<List<int>> CrossingOver(List<List<int>> population, double crossoverRate)
        {
            List<List<int>> newPopulation = new List<List<int>>();
            for (int i = 0; i < population.Count; i += 2)
            {
                var parent1 = population[i];
                var parent2 = population[(i + 1) % population.Count];

                if (random.NextDouble() < crossoverRate && parent1.Count > 1 && parent2.Count > 1)
                {
                    int point1 = random.Next(1, parent1.Count - 1);
                    int point2 = random.Next(1, parent2.Count - 1);

                    var child1 = parent1.Take(point1).Concat(parent2.Skip(point2)).Distinct().ToList();
                    var child2 = parent2.Take(point2).Concat(parent1.Skip(point1)).Distinct().ToList();

                    newPopulation.Add(child1);
                    newPopulation.Add(child2);
                }
                else
                {
                    newPopulation.Add(parent1);
                    newPopulation.Add(parent2);
                }
            }
            return newPopulation.Take(population.Count).ToList();
        }

        public List<List<int>> Mutation(List<List<int>> population, double mutationRate)
        {
            int maxD = instance.Max();
            foreach (var individual in population)
            {
                if (random.NextDouble() < mutationRate)
                {
                    int idx = random.Next(individual.Count);
                    individual[idx] = random.Next(0, maxD + 1);
                    individual.Sort();
                }
            }
            return population;
        }

        public async Task<List<int>> RunGeneticAlgorithmAsync(IProgress<(int Progress, List<int> CurrentBest, List<int> OverallBest)> progress, CancellationToken cancellationToken)
        {
            List<List<int>> population = InitializePopulation();
            List<int> bestSolution = null;
            int stagnationCounter = 0;
            int maxStagnation = _maxStagnationGenerations;
            int maxGenerations = _generationCount;
            int bestGeneration = 0;
            double bestFitness = double.MinValue;
            Stopwatch algorithmTimer = new Stopwatch();

            algorithmTimer.Start();

            for (int gen = 0; gen < maxGenerations; gen++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var generationTimer = Stopwatch.StartNew();

                population = Tournament(population, _tournamentSize);
                population = CrossingOver(population, _crossingOver);
                population = Mutation(population, _mutation);

                var currentBest = population.OrderByDescending(FitnessFunction).First();
                double currentFitness = FitnessFunction(currentBest);

                if (currentFitness > bestFitness)
                {
                    bestSolution = new List<int>(currentBest);
                    bestFitness = currentFitness;
                    stagnationCounter = 0;
                    bestGeneration = gen + 1;

                    Console.WriteLine($"\n=== NOWE NAJLEPSZE ROZWIĄZANIE ===");
                    Console.WriteLine($"Generacja: {bestGeneration}");
                    Console.WriteLine($"Czas: {algorithmTimer.Elapsed.TotalSeconds:F2}s");
                    Console.WriteLine($"Fitness: {bestFitness}");
                    Console.WriteLine($"Rozwiązanie: {string.Join(" ", bestSolution.OrderBy(x => x))}");
                }
                else
                {
                    stagnationCounter++;
                }

                if (maxStagnation > 0 && stagnationCounter >= maxStagnation)
                {
                    Console.WriteLine($"\n=== PRZERWANO Z POWODU STAGNACJI ===");
                    Console.WriteLine($"Osiągnięto limit {maxStagnation} generacji bez poprawy");
                    Console.WriteLine($"Ostatnia poprawa w generacji: {bestGeneration}");
                    break;
                }

                progress?.Report((
                    Progress: (gen + 1) * 100 / maxGenerations,
                    CurrentBest: currentBest,
                    OverallBest: bestSolution
                ));

                
                Console.WriteLine($"Gen {gen + 1,4} | Fitness: {currentFitness,7:F1} | Czas generacji: {generationTimer.Elapsed.TotalMilliseconds,4}ms");
                generationTimer.Stop();

                await Task.Delay(1);
            }

            algorithmTimer.Stop();

            Console.WriteLine($"\n=== PODSUMOWANIE ===");
            Console.WriteLine($"Całkowity czas: {algorithmTimer.Elapsed.TotalSeconds:F2}s");
            Console.WriteLine($"Liczba generacji: {(stagnationCounter >= maxStagnation ? bestGeneration + stagnationCounter : maxGenerations)}");
            Console.WriteLine($"Najlepsze rozwiązanie (gen {bestGeneration}):");
            Console.WriteLine($"Fitness: {bestFitness}");
            Console.WriteLine($"Elementy: {string.Join(" ", bestSolution.OrderBy(x => x))}");

            return bestSolution;
        }


        private List<List<int>> InitializePopulation()
        {
            int maxD = instance.Max();
            return Enumerable.Range(0, _populationSize).Select(_ =>
            {
                var ind = new List<int> { 0, maxD };
                for (int i = 0; i < random.Next(1, 5); i++)
                    ind.Add(random.Next(1, maxD));
                return ind.Distinct().OrderBy(x => x).ToList();
            }).ToList();
        }
    }
}

