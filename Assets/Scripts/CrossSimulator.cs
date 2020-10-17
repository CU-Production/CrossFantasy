using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossSimulator : MonoBehaviour
{
    private LineGenerator_MonteCarlo _generator;
    private List<Line> _allLines;
    private float _timeCounter;
    private float _genTimeCounter;

    private void Awake()
    {
        _allLines = new List<Line>();
        _generator = new LineGenerator_MonteCarlo();
        _timeCounter = 0;
        _genTimeCounter = 0;
    }

    private void FixedUpdate()
    {
        _timeCounter += Time.fixedDeltaTime;
        _genTimeCounter += Time.fixedDeltaTime;

        RemoveEndLines();
        SimulateAll();
       
        if (_genTimeCounter > Configs.GEN_INTERVAL)
        {
            _genTimeCounter = 0;
            for(int i = 0;i < Configs.GEN_MULTIPLIER; i++)
            {
                //_generator.GenerateLineOnce(_allLines, _timeCounter,Random.value>0.5?SourceDir.East:SourceDir.West)
                //_generator.GenerateLineOnce(_allLines, _timeCounter, DecideDir());
                Line tmpLine = _generator.GenerateLineOnce(_allLines, _timeCounter, DecideDir());
                if (tmpLine != null)
                {
                    _allLines.Add(tmpLine);
                }
            }        
        }
    }

    private void RemoveEndLines()
    {
        for (int i = 0; i < _allLines.Count; i++)
        {
            if (_allLines[i].IsEnd(_timeCounter))
            {
                _allLines[i].EndSimulate();
                _allLines.RemoveAt(i);
                i--;
                Debug.Log("now lines : " + _allLines.Count.ToString());
            }
        }
        return;
    }

    private void SimulateAll()
    {
        foreach (Line line in _allLines)
        {
            line.Simulate(_timeCounter);
        }
    }

    private SourceDir DecideDir()
    {
        Dictionary<SourceDir, int> dic = new Dictionary<SourceDir, int>();
        dic[SourceDir.North] = 0;
        dic[SourceDir.East] = 0;
        dic[SourceDir.South] = 0;
        dic[SourceDir.West] = 0;
        foreach(Line line in _allLines)
        {
            dic[line.Dir]++;
        }
        int minCount = int.MaxValue;
        SourceDir minDir = SourceDir.North;
        foreach(KeyValuePair<SourceDir,int> kv in dic)
        {
            if (kv.Value < minCount)
            {
                minCount = kv.Value;
                minDir = kv.Key;
            }         
        }
        return minDir;
    }
}
