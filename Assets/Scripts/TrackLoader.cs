﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TrackLoader : MonoBehaviour {
    public Camera mainCamera;
    public float trackTime;
    public float timeLeft;
    private int currentTrack = 0;
    public bool started = false;
    private int trackManIterations = 0;
    public int neatIterations = 0;
    private int parity = 0;
    private int populationSize = 20;
    public int geneLength = 20;
    public FI2POP trackPopulation;

    public Optimizer optimizer;
    public GameObject[] trackPrefabs;
    List<GameObject> currentTrackSegements = new List<GameObject>();

	public void Awake()
	{
        trackPopulation = new FI2POP(geneLength, populationSize);
        optimizer.TrialDuration = (trackTime * (trackPopulation.feasable.Count * 2)) + 1;

        loadTrack(trackPopulation.feasable[currentTrack].GetGenes());
	}

    public void startLoader()
    {
        started = true;
    }


	private void FixedUpdate()
	{
        if (!started) return;
        if (trackManIterations > neatIterations) return;
        timeLeft -= Time.deltaTime;
        if(timeLeft <= 0)
        {

            updateCarFitness();
            if (parity % 2 == 1)
            {
                updateTrackFitness();
                currentTrack++;
            }
            parity = ++parity%2;
            timeLeft = trackTime;
            if(currentTrack >= trackPopulation.feasable.Count)
            {
                currentTrack = 0;
                trackPopulation.Evolve();
                optimizer.TrialDuration = (trackTime * (trackPopulation.feasable.Count*2)) + 1;
            } else {
                int[] track = trackPopulation.feasable[currentTrack].GetGenes();
                if(parity % 2 == 1)
                {
                    for (int i = 0; i < track.Length; i++)
                    {
                        if(track[i]%2 ==1 && track[i] < 4)
                        {
                            track[i]--;
                        } else if(track[i] % 2 == 0 && track[i] < 4)
                        {
                            track[i]++;
                        }
                    }
                }
                loadTrack(track);
            }
            trackManIterations++;
        }
	}

    private void updateCarFitness()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Car");
        foreach(GameObject obj in objects)
        {
            obj.GetComponent<CarController>().storeCurrentTrackFitness();
        }
        
    }

    private void updateTrackFitness()
    {
        float fitness = 0f;
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Car");
        foreach (GameObject obj in objects)
        {
            fitness += obj.GetComponent<CarController>().lastProgress;
            float timeLastCompleted = obj.GetComponent<CarController>().lastTimeCompleted;
            fitness += timeLastCompleted <= Mathf.Epsilon ? 0f : (1f - obj.GetComponent<CarController>().lastTimeCompleted);
            obj.GetComponent<CarController>().lastProgress = 0;
            obj.GetComponent<CarController>().lastTimeCompleted = 0;
        }
        fitness /= objects.Length;
        if(trackPopulation.feasable.Count == 0)
        {
            print("oops");
        }
        if (currentTrack > trackPopulation.feasable.Count)
        {
            print("oops");
        }
        trackPopulation.feasable[currentTrack].SetFitness(fitness);
    }

    private void loadTrack(int[] track)
    {
        //0 = hard right, 1 = hard left, 2 = soft right, 3 = soft left, 4 = straight, 5 = end
        //remove old track
        for (int i = 0; i < currentTrackSegements.Count; i++)
        {
            Destroy(currentTrackSegements[i]);
        }
        currentTrackSegements.Clear();

        List<int[]> coords = new List<int[]>();

        int entryDir = 0;

        int[] startingCoords = { 0, 0 };
        coords.Add(startingCoords);
        int id = 0;
        foreach(int trackElemt in track)
        {
            int[] latestCoords = coords[coords.Count-1];
            Quaternion rotation = new Quaternion();
            if(entryDir == 0) rotation = Quaternion.Euler(0f, 0f, 0f);
            else if (entryDir == 1) rotation = Quaternion.Euler(0f,90f,0f);
            else if (entryDir == 2) rotation = Quaternion.Euler(0f, 180f, 0f);
            else if (entryDir == 3) rotation = Quaternion.Euler(0f,270f, 0f);

            GameObject newTrack = Instantiate(trackPrefabs[trackElemt], new Vector3(latestCoords[0]*10, 0, latestCoords[1]*10), rotation);
            for (int i = 0; i < newTrack.transform.GetChild(0).childCount; i++)
            {
                var child = newTrack.transform.GetChild(0).GetChild(i);
                if(child.name == "Road"){
                    child.GetComponent<RoadPiece>().PieceNumber = id;
                }       
            }
                       
            id++;
            currentTrackSegements.Add(newTrack);

            int[] nextCoords = new int[2];

            if(entryDir == 0)
            {
                if (trackElemt == 0 || trackElemt == 2)
                {
                    nextCoords[0] = latestCoords[0] + 1;
                    nextCoords[1] = latestCoords[1];
                    entryDir = 1;
                }
                else if(trackElemt == 1 || trackElemt == 3)
                {
                    nextCoords[0] = latestCoords[0] - 1;
                    nextCoords[1] = latestCoords[1];
                    entryDir = 3;
                    
                } else if (trackElemt == 4 || trackElemt == 5)
                {
                    nextCoords[0] = latestCoords[0];
                    nextCoords[1] = latestCoords[1]+1;
                    entryDir = 0;
                }
            } else if(entryDir == 1)
            {
                if (trackElemt == 0 || trackElemt == 2)
                {
                    nextCoords[0] = latestCoords[0];
                    nextCoords[1] = latestCoords[1]-1;
                    entryDir = 2;
                }
                else if (trackElemt == 1 || trackElemt == 3)
                {
                    nextCoords[0] = latestCoords[0];
                    nextCoords[1] = latestCoords[1]+1;
                    entryDir = 0;

                }
                else if (trackElemt == 4 || trackElemt == 5)
                {
                    nextCoords[0] = latestCoords[0]+1;
                    nextCoords[1] = latestCoords[1];
                    entryDir = 1;
                }
                
            } else if (entryDir == 2)
            { 
                if (trackElemt == 0 || trackElemt == 2)
                {
                    nextCoords[0] = latestCoords[0] -1;
                    nextCoords[1] = latestCoords[1];
                    entryDir = 3;
                }
                else if (trackElemt == 1 || trackElemt == 3)
                {
                    nextCoords[0] = latestCoords[0] + 1;
                    nextCoords[1] = latestCoords[1];
                    entryDir = 1;

                }
                else if (trackElemt == 4 || trackElemt == 5)
                {
                    nextCoords[0] = latestCoords[0];
                    nextCoords[1] = latestCoords[1]-1;
                    entryDir = 2;
                }
            }else if (entryDir == 3)
            { 
                if (trackElemt == 0 || trackElemt == 2)
                {
                    nextCoords[0] = latestCoords[0];
                    nextCoords[1] = latestCoords[1]+1;
                    entryDir = 0;
                }
                else if (trackElemt == 1 || trackElemt == 3)
                {
                    nextCoords[0] = latestCoords[0];
                    nextCoords[1] = latestCoords[1]-1;
                    entryDir = 2;

                }
                else if (trackElemt == 4 || trackElemt == 5)
                {
                    nextCoords[0] = latestCoords[0]-1;
                    nextCoords[1] = latestCoords[1];
                    entryDir = 3;
                }
            }
            coords.Add(nextCoords);
        }
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Car");
        foreach (GameObject obj in objects)
        {
            obj.transform.position = Vector3.zero;
            obj.transform.rotation = Quaternion.identity;
        }
        //todo: Move camera
        int minX = 0;
        int minZ = 0;
        int maxX = 0;
        int maxZ = 0;
        foreach(int[] coord in coords)
        {
            if (coord[0] < minX) minX = coord[0];
            else if (coord[0] > maxX) maxX = coord[0];
            else if (coord[1] < minZ) minX = coord[1];
            else if (coord[1] > maxZ) maxZ = coord[1];
        }
        mainCamera.transform.position = new Vector3((maxX*10)-((Mathf.Abs(maxX-minX)/2)*10), 100, (maxZ*10) - ((Mathf.Abs(maxZ - minZ)/ 2) * 10));
    }

    public static bool Check( TrackChromosome t)
    {
        int[] track = t.GetGenes();
        //0 = hard right, 1 = hard left, 2 = soft right, 3 = soft left, 4 = straight, 5 = end
        //remove old track

        List<int[]> coords = new List<int[]>();

        int entryDir = 0;

        int[] startingCoords = { 0, 0 };
        coords.Add(startingCoords);

        foreach (int trackElemt in track)
        {
            int[] latestCoords = coords[coords.Count - 1];
            int[] nextCoords = new int[2];

            if (entryDir == 0)
            {
                if (trackElemt == 0 || trackElemt == 2)
                {
                    nextCoords[0] = latestCoords[0] + 1;
                    nextCoords[1] = latestCoords[1];
                    entryDir = 1;
                }
                else if (trackElemt == 1 || trackElemt == 3)
                {
                    nextCoords[0] = latestCoords[0] - 1;
                    nextCoords[1] = latestCoords[1];
                    entryDir = 3;

                }
                else if (trackElemt == 4 || trackElemt == 5)
                {
                    nextCoords[0] = latestCoords[0];
                    nextCoords[1] = latestCoords[1] + 1;
                    entryDir = 0;
                }
            }
            else if (entryDir == 1)
            {
                if (trackElemt == 0 || trackElemt == 2)
                {
                    nextCoords[0] = latestCoords[0];
                    nextCoords[1] = latestCoords[1] - 1;
                    entryDir = 2;
                }
                else if (trackElemt == 1 || trackElemt == 3)
                {
                    nextCoords[0] = latestCoords[0];
                    nextCoords[1] = latestCoords[1] + 1;
                    entryDir = 0;

                }
                else if (trackElemt == 4 || trackElemt == 5)
                {
                    nextCoords[0] = latestCoords[0] + 1;
                    nextCoords[1] = latestCoords[1];
                    entryDir = 1;
                }

            }
            else if (entryDir == 2)
            {
                if (trackElemt == 0 || trackElemt == 2)
                {
                    nextCoords[0] = latestCoords[0] - 1;
                    nextCoords[1] = latestCoords[1];
                    entryDir = 3;
                }
                else if (trackElemt == 1 || trackElemt == 3)
                {
                    nextCoords[0] = latestCoords[0] + 1;
                    nextCoords[1] = latestCoords[1];
                    entryDir = 1;

                }
                else if (trackElemt == 4 || trackElemt == 5)
                {
                    nextCoords[0] = latestCoords[0];
                    nextCoords[1] = latestCoords[1] - 1;
                    entryDir = 2;
                }
            }
            else if (entryDir == 3)
            {
                if (trackElemt == 0 || trackElemt == 2)
                {
                    nextCoords[0] = latestCoords[0];
                    nextCoords[1] = latestCoords[1] + 1;
                    entryDir = 0;
                }
                else if (trackElemt == 1 || trackElemt == 3)
                {
                    nextCoords[0] = latestCoords[0];
                    nextCoords[1] = latestCoords[1] - 1;
                    entryDir = 2;

                }
                else if (trackElemt == 4 || trackElemt == 5)
                {
                    nextCoords[0] = latestCoords[0] - 1;
                    nextCoords[1] = latestCoords[1];
                    entryDir = 3;
                }
            }
            foreach (int[] prevCoord in coords)
            {
                if (prevCoord[0] == nextCoords[0] && prevCoord[1] == nextCoords[1]) return false;

            }
            coords.Add(nextCoords);
        }
        return true;
    }


}
