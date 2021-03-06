﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FI2POP
{

    public static int pop_size;
    public static int chromosome_length;
    public List<TrackChromosome> pop;

    public List<TrackChromosome> feasable = new List<TrackChromosome>();
    public List<TrackChromosome> unfeasable = new List<TrackChromosome>();

    public FI2POP(int track_length, int _pop_size)
    {
        pop = new List<TrackChromosome>();
        pop_size = _pop_size;
        chromosome_length = track_length;

        for (int i = 0; i < pop_size; i++)
        {
            pop.Add(new TrackChromosome(chromosome_length));
        }
        bool hasFeasibilePop = false;
        while (!hasFeasibilePop)
        {
            hasFeasibilePop = FeasibilityCheck();
            if (!hasFeasibilePop)
            {
                pop.Clear();
                for (int i = 0; i < pop_size; i++)
                {
                    pop.Add(new TrackChromosome(chromosome_length));
                }
            }
        }

    }

    public void SetIsFeasable(bool isFeasable, int index)
    {
        pop[index].SetFeasable(isFeasable);
    }

    public void SetFitness(int fitness, int index)
    {
        pop[index].SetFitness(fitness);
    }


    public bool FeasibilityCheck()
    {

        // check feasibility
        foreach(TrackChromosome c in pop)
        {
            c.SetFeasable(TrackLoader.Check(c));
        }

        for (int i = 0; i < pop_size; i++)
        {
            if (pop[i].IsFeasable())
            {
                feasable.Add(pop[i]);
            }
            else
            {
                unfeasable.Add(pop[i]);
            }
        }
        return feasable.Count > 0;
    }

    public void Evolve()
    {


        int from_feasable = pop_size / 2;
        int from_unfeasable = pop_size - from_feasable;

        List<TrackChromosome> new_pop = new List<TrackChromosome>();
        new_pop.AddRange(NewPopulation(feasable, from_feasable));
        new_pop.AddRange(NewPopulation(unfeasable, from_unfeasable));

        pop.Clear();
        pop.AddRange(new_pop);

        feasable.Clear();
        unfeasable.Clear();

        bool hasFeasibilePop = false;
        while (!hasFeasibilePop)
        {
            hasFeasibilePop = FeasibilityCheck();
            if (!hasFeasibilePop)
            {
                pop.Clear();
                for (int i = 0; i < pop_size; i++)
                {
                    pop.Add(new TrackChromosome(chromosome_length));
                }
            }
        }

    }

    private TrackChromosome Selection(List<TrackChromosome> chrs)
    {
        TrackChromosome s = chrs[Random.Range(0, chrs.Count)];
        int tournament_size = 4;

        for (int i = 1; i < tournament_size; i++)
        {
            TrackChromosome new_s = chrs[Random.Range(0, chrs.Count)];
            if (s.GetFitness() < new_s.GetFitness())
            {
                s = new_s;
            }
        }
        return s;
    }

    private List<TrackChromosome> NewPopulation(List<TrackChromosome> old_pop, int new_size)
    {
        List<TrackChromosome> new_pop = new List<TrackChromosome>();


        while (new_pop.Count != new_size)
        {
            TrackChromosome c1 = Selection(old_pop);
            TrackChromosome c2 = Selection(old_pop);

            List<TrackChromosome> crossed = TrackChromosome.Crossover(c1, c2);

            new_pop.Add(crossed[0]);

            if (new_pop.Count < new_size)
            {
                new_pop.Add(crossed[1]);
            }
        }

        for (int i = 0; i < new_size; i++)
        {
            new_pop[i].Mutate();
        }

        return new_pop;
    }

}
