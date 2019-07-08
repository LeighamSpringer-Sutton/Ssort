using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Transactions;
using UnityEditor.iOS;
using Random = UnityEngine.Random;
using UnityEngine;

public class Datamanager : MonoBehaviour
{
    // Start is called before the first frame update
    //GENERATE BLOCKS FROM DATAMANAGER TO AVOID DELETING ORIGINAL
    public List<Vector3> postionsOnGrid;
    private List<Vector3> topsOfBlocks;
    
    private List<Block> blocksOnMap;
    private Dictionary<float, List<Bounds>> boundsByColumn;
    private Dictionary<float, List<Bounds>> boundsByRow;
    private Dictionary<float, List<float>> positionOnGridByRow;
    private Dictionary<float, List<float>> positionOnGridByColumn;
    private Dictionary<float, List<int>> solutionByRow;
    private List<float> rows;
    private List<float> columns;
    private List<float> rowsTwoPlaces;
    [SerializeField] private GameObject block;
    private float blockZValue = 2f;
    private List<int[]> upComingEquations;
    private List<string> upComingSigns;
    private float bottomVsTopThreshold = 0.1f;
    private int numberOfBlocksSpawned = 1;
    private int spawnCount = 0;
    private float prevDeletedRow = 0;

    
    void Start()

    {


        
        upComingEquations = new List<int[]>();
        upComingSigns = new List<string>();

        solutionByRow = new Dictionary<float,List<int>>();
        boundsByColumn = new Dictionary<float, List<Bounds>>();
        boundsByRow = new Dictionary<float, List<Bounds>>();
        positionOnGridByRow = new Dictionary<float, List<float>>();
        positionOnGridByColumn = new Dictionary<float, List<float>>();
        CreateRowsDictionary();
        CreateColumnsDictionary();
        CreateSolutionsByRowsDictionary();
        GenerateTwoEquations();
        rows = new List<float> { 2, 4, 7, 9 };
        columns = new List<float> { 2, 4, 7, 9, 12 };
        rowsTwoPlaces = new List<float> { 1.52f, 4.17f,6.83f, 9.49f};

        


        blocksOnMap = new List<Block>();
        topsOfBlocks = new List<Vector3>();
        
        postionsOnGrid = new List<Vector3>();


        CreateNewBlock();

    }


    public void clearMap()
    {
        solutionByRow = new Dictionary<float, List<int>>();
        boundsByColumn = new Dictionary<float, List<Bounds>>();
        boundsByRow = new Dictionary<float, List<Bounds>>();
        positionOnGridByRow = new Dictionary<float, List<float>>();
        positionOnGridByColumn = new Dictionary<float, List<float>>();
        CreateRowsDictionary();
        CreateColumnsDictionary();
        CreateSolutionsByRowsDictionary();
        GenerateTwoEquations();
    }
    public void RemoveDataAfterDeletion(float yPos, float xPos,Vector3 maxBounds,Bounds bounds,int solution)
    {
        var xRound = (float )Math.Round(xPos);
        var yRound = (float)Math.Round(yPos);
        var pointsAsVector = new Vector3(xRound, yRound);

        //tomove
        positionOnGridByRow[yRound].Remove(xRound);
        




        positionOnGridByColumn[xRound].Remove(yRound);
       
        postionsOnGrid.Remove(pointsAsVector);
        
        ///tomove
        topsOfBlocks.Remove(maxBounds);

        //tomove
        if (boundsByColumn[xRound].Count > 0) {

            boundsByColumn[xRound].Remove(bounds);
           
        }
        //tomove
        var blockSolutionIndex = solutionByRow[yRound].IndexOf((int)solution);


        Debug.Log(solution);


        solutionByRow[yRound][blockSolutionIndex] = -1000;


        prevDeletedRow = yPos;



    }


    public void shiftBounds()
    {

    }
    public void ShiftDataDownOnerow(float xPos, float yPos, int solution, Vector3 newPos)
    {


        var xRound = (float)Math.Round(xPos);
        var yRound = (float)Math.Round(yPos);
        var pointsAsVector = new Vector3(xRound, yRound);
        
        var prevDelRowRound = (float)Math.Round(prevDeletedRow);
        if (prevDeletedRow > yPos)
        {
            return;
        }


        //find current row
        //move below one row
        //delete from current row
        var currentIndexRow = rows.IndexOf(yRound);
        var rowBelow = rows[currentIndexRow - 1];
        positionOnGridByRow[rowBelow].Add(xRound);
        positionOnGridByRow[yRound].Remove(xRound);



        ///add the row below, remove the current row
        

        positionOnGridByColumn[xRound].Add(rowBelow);
        positionOnGridByColumn[xRound].Remove(yRound);


        postionsOnGrid.Remove(pointsAsVector);
        postionsOnGrid.Add(newPos);



        var blockSolutionIndex = solutionByRow[yRound].IndexOf((int)solution);


        solutionByRow[rowBelow][blockSolutionIndex] = solutionByRow[yRound][blockSolutionIndex];

        solutionByRow[yRound][blockSolutionIndex] = -1000;


}

    public string GenerateOperand()
    {

        var operands = new string[] { "+", "-", "/", "*" };
        return operands[Random.Range(0, operands.Length - 1)];
    }
    private void GenerateTwoEquations()
    {
        var sign = GenerateOperand();
        var equation = GenerateEquation(sign);
        upComingSigns.Add(sign);
        upComingEquations.Add(equation);


        var sign2 = GenerateOperand();
        var equation2 = GenerateEquation(sign2);
        upComingSigns.Add(sign);
        upComingEquations.Add(equation2);
    }

    public string PullSign()
    {
        var sign = upComingSigns[upComingSigns.Count - 1];
        upComingSigns.RemoveAt(upComingSigns.Count - 1);
        return sign;
    }

    public int[] PullEquation()
    {

       
        var num1 = upComingEquations[upComingEquations.Count - 1][0];
        var num2 = upComingEquations[upComingEquations.Count - 1][1];
        upComingEquations.RemoveAt(upComingEquations.Count - 1);
        if (upComingEquations.Count != 0)
        {
            Debug.Log("upcoming is " + upComingEquations[0][0] + upComingSigns[0] + upComingEquations[0][1]);
        }
               
        else if (upComingEquations.Count == 0)
        {
            GenerateTwoEquations();
            Debug.Log("upcoming is " + upComingEquations[1][0] + upComingSigns[1] + upComingEquations[1][1]);
        }
       ;
        return new int[] { num1, num2 };
    }
    public int[] GenerateEquation(string operand)
    {


        var num1 = Random.Range(1, 3);
        var num2 = Random.Range(1, 3);
        if (operand == "/")
        {

            while (num1 % num2 != 0)
            {
                num2 = Random.Range(1, 12);
            }
        }


        return new int[] { num1, num2 };
    }
    public string PositionTextBasedOffEquation(int equationLength)
    {
        var spaces = "";
        switch (equationLength)
        {
            case 2:

                spaces = "  ";
                break;
            case 3:

                spaces = " ";
                break;

        }
        return spaces;
    }

    public int ComputeSolution(int num1, int num2, string operand)
    {

        int answer = 0;
        switch (operand)
        {
            case "+":
                answer = num1 + num2;
                break;
            case "-":
                answer = num1 - num2;
                break;
            case "*":
                answer = num1 * num2;
                break;
            case "/":
                answer = num1 / num2;
                break;

        }

        return answer;
    }


    public void CreateSolutionsByRowsDictionary()
    {
        
        solutionByRow.Add(2, new List<int> { -1000, -1000, -1000, -1000, -1000 });
        solutionByRow.Add(4, new List<int> { -1000, -1000, -1000, -1000, -1000 });
        solutionByRow.Add(7, new List<int> { -1000, -1000, -1000, -1000, -1000 });
        solutionByRow.Add(9, new List<int> { -1000, -1000, -1000, -1000, -1000 });

    }
    public void CreateRowsDictionary()
    {
        positionOnGridByRow.Add(2, new List<float>());
        positionOnGridByRow.Add(4, new List<float>());
        positionOnGridByRow.Add(7, new List<float>());
        positionOnGridByRow.Add(9, new List<float>());

    }


    public void CreateColumnsDictionary()
    {
        positionOnGridByColumn.Add(2, new List<float>());
        positionOnGridByColumn.Add(4, new List<float>());
        positionOnGridByColumn.Add(7, new List<float>());
        positionOnGridByColumn.Add(9, new List<float>());
        positionOnGridByColumn.Add(12, new List<float>());
    }


    public void AddToSolutionRowsDictionary(float row,float column,int solution)
    {

        //all additons to solutions dictionary must use a switch statement and take the columsn into account
        var columnRounded = (float)Math.Round(column);

        var currentRow = solutionByRow[(float)Math.Round(row)];

        var len  = solutionByRow[(float)Math.Round(row)].Count;

        switch (columnRounded)
        {
            case 2:
                currentRow[0] = solution;
                break;
            case 4:
                currentRow[1] = solution;
                break;
            case 7:
                currentRow[2] = solution;
                break;
            case 9:
                currentRow[3] = solution;
                break;
            case 12:
                currentRow[4] = solution;
                break;
        }


        //Debug.Log(solutionByRow[(float)Math.Round(row)][len - 1]);
    }
    public void AddToColumnsDictionary(Vector3 blockPosition)
    {

        
        positionOnGridByColumn[(float)Math.Round(blockPosition.x)].Add((float)Math.Round(blockPosition.y));

        
    }


    public void AddToRowsDictionary(Vector3 blockPosition)
    {
        
        positionOnGridByRow[(float)Math.Round(blockPosition.y)].Add((float)Math.Round(blockPosition.x));


    }

    public bool CheckForMathes(float row,float xPos)
    {

        var rowRounded = (float)Math.Round(row);

        var currentRowSolutions = solutionByRow[rowRounded];


        if (!currentRowSolutions.Contains(-1000))
        {
            var rowCopy = new List<int>(currentRowSolutions);
            var rowCopyRev = new List<int>(currentRowSolutions);
            rowCopyRev.Sort();
            rowCopyRev.Reverse();
            rowCopy.Sort();

            if (rowCopy.SequenceEqual(currentRowSolutions) || rowCopyRev.SequenceEqual(currentRowSolutions))
            {
                var blocksSpwaned = FindObjectsOfType<Block>();

                foreach (var block in blocksSpwaned)
                {
                    if ((float) Math.Round(block.transform.position.y) == rowRounded)
                    {
                        block.DeleteBlock();
                        
                    }
                }
               
                return true;
            }
            
        }
        
        return false;
    }



    public void ShiftDownOneRow()
    {

        //TO DOO CREATE SHIFTDOWN DATA FUNCTION AND USE MOVE TO TO SHIFT BLOCKS GRACEFULLY
        //bLOCKS ARE MOVED DATA NEEDS TO MOVE AS WELL
        //THEN IMPLEMENT COLUMNS(MAYBE)
        var speed = 1f;
        var step = speed * Time.deltaTime;
        var blocksAlive = FindObjectsOfType<Block>();

        var notActiveBlocksAlive = blocksAlive.Where(b => !b.isActive()).ToList();

        if (notActiveBlocksAlive.Count < 1)
        {
            return;
        }
        
        foreach (var block in notActiveBlocksAlive)
        {

            var rowRouded = (float)Math.Round(block.transform.position.y);
            var fullRound = rowsTwoPlaces.Select(v => (float)Math.Round(v)).ToList();
            var index = fullRound.IndexOf(rowRouded);


            
            if (index > 0)
            {

                var towards = new Vector3(block.transform.position.x, rowsTwoPlaces[index - 1],blockZValue);
                

                block.ShiftBlock(towards);
               
            }
        }


    }
    
   
    
    public void AddToBoundsByColumn(float column, Bounds bounds)
    {
        var col = (float)Math.Round(column);
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

    public bool PotentialOverlapDetected(float movement, float bottom)
    {
        var m = (float)Math.Round(movement);

        if (boundsByColumn.ContainsKey(m))
        {
            var maxBlockInRow = boundsByColumn[m].Max(b => b.max.y);

            return bottom < maxBlockInRow;
        }

        return false;


    }
    public bool BlockBelow(float column, float bottomOfBlock)
    {
        var col = (float)Math.Round(column);
        if (boundsByColumn.ContainsKey(col) && boundsByColumn[col].Count>0)
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

    public void AddToGrid(Vector3 position)
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


   




    public bool BlockBelow(float CurrentBlockBottom)
    {

        var columns = topsOfBlocks.Select(v => CurrentBlockBottom - v.y <= bottomVsTopThreshold).ToList();


        return (CurrentBlockBottom - HighestFloor() <= bottomVsTopThreshold);





        //if in the same column and difference between lower block equals zero start blinker and trigger lower wall being hit
    }

    public bool ColumnEmpty(float row)
    {
        if (positionOnGridByColumn[row].Count == 0)
        {
            return true;
        }
        return false;
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
        var blck = Instantiate(block, new Vector3(6.6f, 10.74f, blockZValue), transform.rotation);
        blck.GetComponent<Block>().OnBlockCreation();
        numberOfBlocksSpawned++;
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


   


    //finish

    
        
    
    
    public float SnapToBottom(float lowerWall, float xPos,float yPos,float bottomOfBlock)
    {

        //use already built in function for checking block below and then use row array to go one index lower for x value

        var xPosR = (float)Math.Round(xPos);
        var yPosR = (float)Math.Round(yPos);
        //Debug.Log(xPosR);
        if (yPosR != lowerWall && ColumnEmpty(xPosR))
        {
            return lowerWall ;
        }
        
        else if (yPos != lowerWall && !ColumnEmpty(xPosR) )
        {

            var currentMaxRow = positionOnGridByColumn[xPosR].Max();
            //Debug.Log(currentMaxRow);
            
            return rowsTwoPlaces[rows.IndexOf(currentMaxRow)+1];
        }
        
        return yPos;
    }
    

    

}
