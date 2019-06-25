using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Transactions;
using UnityEditor.iOS;
using UnityEngine;

public class Datamanager : MonoBehaviour
{
    // Start is called before the first frame update

    public List<Vector3> postionsOnGrid;
    private List<Vector3> topsOfBlocks;
    private List<Vector3> BottomOfBlocks;
    private List<Block> blocksOnMap;
    private Dictionary<float, List<Bounds>> boundsByColumn;
    private Dictionary<float, List<Bounds>> boundsByRow;
    [SerializeField] private GameObject block;
    private float blockZValue = 2f;
    private float bottomVsTopThreshold = 0.1f;
    private int numberOfBlocksSpawned = 1;
    private int spawnCount =0;
    void Start()

    {
        boundsByColumn = new Dictionary<float, List<Bounds>>();
        boundsByRow = new Dictionary<float, List<Bounds>>();
        blocksOnMap = new List<Block>();
        topsOfBlocks = new List<Vector3>();
        BottomOfBlocks = new List<Vector3>();
        postionsOnGrid = new List<Vector3>();
    }

    public void AddToBoundsByColumn(float column,Bounds bounds)
    {
        var col = (float) Math.Round(column);
        if (!boundsByColumn.ContainsKey(col))
        {
            boundsByColumn.Add(col, new List<Bounds>());
            boundsByColumn[col].Add(bounds);

        }
        else
        {
            boundsByColumn[col].Add(bounds);
        }
        
    }

    public void AddToBoundsByRow(float row, Bounds bounds)
    {
        var r = (float)Math.Round(row);
        if (!boundsByRow.ContainsKey(r))
        {
            boundsByRow.Add(r, new List<Bounds>());
            boundsByRow[r].Add(bounds);

        }
        else
        {
            boundsByRow[r].Add(bounds);
        }

    }

    public bool PotentialOverlapDetectedTest(float movement,float bottom)
    {
        var m = (float)Math.Round(movement);

        if (boundsByColumn.ContainsKey(m))
        {
            var maxBlockInRow = boundsByColumn[m].Max(b => b.max.y);

            return bottom < maxBlockInRow;
        }

        return false;


    }
    public bool BlockBelowTest(float column,float bottomOfBlock)
    {
        var col = (float)Math.Round(column);
        if (boundsByColumn.ContainsKey(col))
        {
            var maxBlockInRow = boundsByColumn[col].Max(b => b.max.y);

            return bottomOfBlock - maxBlockInRow <= bottomVsTopThreshold;
        }

        return false;
        //var columns = topsOfBlocks.Select(v => bottomOfBlock - v.y <= bottomVsTopThreshold).ToList();


    }
    
    // Update is called once per frame
    public bool GridCellFull(float totalX, float y)
    {
        return postionsOnGrid.Contains(new Vector3(totalX, y));

    }

    public void AddToblocksOnMap(Block block)
    {
        blocksOnMap.Add(block);
    }

    public  void AddToGrid(Vector3 position)
    {
        postionsOnGrid.Add(position);
    }

    public void AllData()
    {
        foreach (var v in postionsOnGrid)
        {
            Debug.Log(v);
        }
    }

    public void AddToTopsOfBlockData(Vector3 topOfBlock)
    {
        topsOfBlocks.Add(topOfBlock);
        //topsOfBlocks.RemoveAt(topsOfBlocks.Count - 1);
    }


    public void AddToBottomsOfBlockData(Vector3 BottomOfBlock)
    {
        BottomOfBlocks.Add(BottomOfBlock);
    }

    
    public bool InColumn(float UpcomingX)
    {
        foreach (var v in postionsOnGrid)
        {
            if (Math.Round(v.x) == Math.Round(UpcomingX))
            {
                
                return true;
            }
        }

        return false;
    }

    public bool BlockBelow(float CurrentBlockBottom)
    {

        var columns = topsOfBlocks.Select(v => CurrentBlockBottom -v.y <= bottomVsTopThreshold).ToList();


        return (CurrentBlockBottom -HighestFloor() <= bottomVsTopThreshold);




        
        //if in the same column and difference between lower block equals zero start blinker and trigger lower wall being hit
    }

    public float HighestFloor()
    {
        
        if (topsOfBlocks.Count > 0)
        {
            var columns = topsOfBlocks.Select(v => v.y).ToList();
            return columns.Max();

        }

        return -99;
    }

    public void CreateNewBlock()
    {
        Instantiate(block, new Vector3(6.6f, 10.74f, blockZValue),transform.rotation);
        numberOfBlocksSpawned ++;
        spawnCount++;
    }

    public int BlockCount()
    {
        return numberOfBlocksSpawned;
    }
    public void RemoveFromGrid()
    {
        ///f
    }

    public int spawnValue()
    {
        return spawnCount;
    }
    public void RemoveFromTopsOfBlockData()
    {

    }


    public void RemoveFromBottomOfBlockData()
    {

    }
}
