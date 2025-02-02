﻿using System;
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
    [SerializeField] private TMPro.TextMeshProUGUI scoreText;
    [SerializeField] private TMPro.TextMeshProUGUI rangeText;
    

    private List<Block> blocksOnMap;
    private Dictionary<float, List<Bounds>> boundsByColumn;
    private Dictionary<float, List<Bounds>> boundsByRow;
    private Dictionary<float, List<float>> positionOnGridByRow;
    private Dictionary<float, List<float>> positionOnGridByColumn;
    private Dictionary<float, List<int>> solutionByRow;
    private Dictionary<float, List<int>> solutionByColumn;
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
    private bool shiftingInProgress = false;
    public int blocksMoved;
    public int blocksToMove = 0;
    public bool rowCleared = false;
    public bool columnsCleared = false;
    public bool shiftingFunctionFinished = false;
    public int gameScore;
    public int gameScoreOnPreviousGeneration;
    private string UpcomingText;
    public int MaxRandNum = 10;
    void Start()

    {


        Debug.Log(MaxRandNum);
        upComingEquations = new List<int[]>();
        upComingSigns = new List<string>();

        solutionByRow = new Dictionary<float, List<int>>();
        solutionByColumn = new Dictionary<float, List<int>>();
        boundsByColumn = new Dictionary<float, List<Bounds>>();
        boundsByRow = new Dictionary<float, List<Bounds>>();
        positionOnGridByRow = new Dictionary<float, List<float>>();
        positionOnGridByColumn = new Dictionary<float, List<float>>();

        CreateRowsDictionary();
        CreateColumnsDictionary();
        CreateSolutionsByRowsDictionary();
        CreateSolutionByColumnDictionary();
        GenerateTwoEquations();
        rows = new List<float> { 2, 4, 7, 9 };
        columns = new List<float> { 2, 4, 7, 9, 12 };
        rowsTwoPlaces = new List<float> { 1.52f, 4.17f, 6.83f, 9.49f };




        blocksOnMap = new List<Block>();
        topsOfBlocks = new List<Vector3>();

        postionsOnGrid = new List<Vector3>();
        scoreText.text = "Score :" + " " + gameScore;
        rangeText.text = "Range :" + "0 - " + MaxRandNum;
        CreateNewBlock();

    }
    void Update()
    {
        if (rowCleared)
        {
            ShiftDownOneRow();
            rowCleared = false;
        }

        if (columnsCleared)
        {
            CreateNewBlock();
            columnsCleared = false;
        }
    }


    public void IncreaseScore(string clearType)
    {

        if (clearType == "row")
        {
            gameScore += 5;
            scoreText.text = "Score :" + " " + gameScore;
            GameScore.gameScore = gameScore;
            return;
        }
        
        gameScore += 4;
        scoreText.text = "Score :" + " " + gameScore;
        GameScore.gameScore = gameScore;



    }

    public void UpdateBlocksMoved()
    {
        blocksMoved++;
    }
    public void ResetBlocksMoved()
    {
        blocksMoved = 0;
    }

    public int GetBlocksMoved()
    {
        return blocksMoved;
    }

    public void UpDateShifting(bool inProgress)
    {
        shiftingInProgress = inProgress;
    }

    public bool CurrentlyShifting()
    {
        return shiftingInProgress;
    }

    public void clearMap()
    {
        solutionByRow = new Dictionary<float, List<int>>();
        solutionByColumn = new Dictionary<float, List<int>>();
        boundsByColumn = new Dictionary<float, List<Bounds>>();
        boundsByRow = new Dictionary<float, List<Bounds>>();
        positionOnGridByRow = new Dictionary<float, List<float>>();
        positionOnGridByColumn = new Dictionary<float, List<float>>();
        CreateRowsDictionary();
        CreateColumnsDictionary();
        CreateSolutionsByRowsDictionary();
        GenerateTwoEquations();

        blocksOnMap = new List<Block>();
        topsOfBlocks = new List<Vector3>();

        postionsOnGrid = new List<Vector3>();

    }
    public void RemoveDataAfterDeletion(float yPos, float xPos, Vector3 maxBounds, Bounds bounds, int solution, bool shiftDelete)
    {
        var xRound = (float)Math.Round(xPos);
        var yRound = (float)Math.Round(yPos);
        var pointsAsVector = new Vector3(xRound, yRound);

        //tomove
        positionOnGridByRow[yRound].Remove(xRound);





        positionOnGridByColumn[xRound].Remove(yRound);

        postionsOnGrid.Remove(pointsAsVector);

        ///tomove
        topsOfBlocks.Remove(maxBounds);

        //tomove
        if (boundsByColumn[xRound].Count > 0)
        {

            boundsByColumn[xRound].Remove(bounds);

        }
        //tomove
        var blockSolutionIndex = solutionByRow[yRound].IndexOf((int)solution);

        var colSolutionIndex = solutionByColumn[xRound].IndexOf((int)solution);
        Debug.Log("The index is " + blockSolutionIndex);

        if (blockSolutionIndex != -1)
        {


            solutionByRow[yRound][blockSolutionIndex] = -1000;
        }
        if (colSolutionIndex != -1)
        {
            solutionByColumn[xRound][colSolutionIndex] = -1000;
        }

        if (!shiftDelete)
        {


            prevDeletedRow = yPos;

        }

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

            UpcomingText = "UPCOMING : " + upComingEquations[0][0].ToString() + upComingSigns[0].ToString() + upComingEquations[0][1].ToString();
            Debug.Log("upcoming is " + upComingEquations[0][0] + upComingSigns[0] + upComingEquations[0][1]);
        }

        else if (upComingEquations.Count == 0)
        {
            GenerateTwoEquations();
            Debug.Log("upcoming is " + upComingEquations[1][0] + upComingSigns[1] + upComingEquations[1][1]);
            UpcomingText = "UPCOMING : " + upComingEquations[1][0].ToString() + upComingSigns[1].ToString() + upComingEquations[1][1].ToString();
        }
       
        return new int[] { num1, num2 };
    }


    public void SetUpcomingText(TMPro.TextMeshProUGUI textToset)
    {
        textToset.text = UpcomingText;
    }

    public void IncreaseMaxRandomNumberRange(int prevScore, int currentScore)
    {
        var prevScoreByTen = Math.Floor(prevScore / 10d);
        var currentScoreByTen = Math.Floor(currentScore / 10d);
        if (currentScoreByTen-prevScoreByTen == 1 && MaxRandNum <100)
        {

            Debug.Log(currentScoreByTen - prevScoreByTen);
            Debug.Log("Working");
            MaxRandNum += 5;
            rangeText.text = "Range :" + " 0 - " + MaxRandNum ;
        }
    }


    public int[] GenerateEquation(string operand)
    {
        IncreaseMaxRandomNumberRange(gameScoreOnPreviousGeneration,gameScore);

        var num1 = Random.Range(1, MaxRandNum);
        var num2 = Random.Range(1, MaxRandNum);
        
        if (operand == "/")
        {

            while (num1 % num2 != 0)
            {
                num2 = Random.Range(1, MaxRandNum);
            }
        }

        gameScoreOnPreviousGeneration = gameScore;
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

    public void CreateSolutionByColumnDictionary()
    {
        solutionByColumn.Add(2, new List<int> { -1000, -1000, -1000, -1000 });
        solutionByColumn.Add(4, new List<int> { -1000, -1000, -1000, -1000 });
        solutionByColumn.Add(7, new List<int> { -1000, -1000, -1000, -1000 });
        solutionByColumn.Add(9, new List<int> { -1000, -1000, -1000, -1000 });
        solutionByColumn.Add(12, new List<int> { -1000, -1000, -1000, -1000 });
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

    public void AddToSolutionColumnDictionary(float row, float column, int solution)
    {
        var rowRounded = (float)Math.Round(row);

        var currentCol = solutionByColumn[(float)Math.Round(column)];



        switch (rowRounded)
        {
            case 2:
                currentCol[0] = solution;
                break;
            case 4:
                currentCol[1] = solution;
                break;
            case 7:
                currentCol[2] = solution;
                break;
            case 9:
                currentCol[3] = solution;
                break;

        }
    }


    public void AddToSolutionRowsDictionary(float row, float column, int solution)
    {

        //all additons to solutions dictionary must use a switch statement and take the columsn into account
        var columnRounded = (float)Math.Round(column);

        var currentRow = solutionByRow[(float)Math.Round(row)];



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



    }
    public void AddToColumnsDictionary(Vector3 blockPosition)
    {


        positionOnGridByColumn[(float)Math.Round(blockPosition.x)].Add((float)Math.Round(blockPosition.y));


    }


    public void AddToRowsDictionary(Vector3 blockPosition)
    {

        positionOnGridByRow[(float)Math.Round(blockPosition.y)].Add((float)Math.Round(blockPosition.x));


    }


   

    public bool CheckForMatches(float row, float xPos)
    {

        var rowRounded = (float)Math.Round(row);
        var colRounded = (float)Math.Round(xPos);

        var currentRowSolutions = solutionByRow[rowRounded];


        var currentColumnolutions = solutionByColumn[colRounded];


        if (!currentRowSolutions.Contains(-1000) || !currentColumnolutions.Contains(-1000))
        {
            var rowCopy = new List<int>(currentRowSolutions);
            var rowCopyRev = new List<int>(currentRowSolutions);


            var colCopy = new List<int>(currentColumnolutions);
            var colCopyRev = new List<int>(currentColumnolutions);

            //sort then revere sort
            rowCopyRev.Sort();
            rowCopyRev.Reverse();
            rowCopy.Sort();


            colCopyRev.Sort();
            colCopyRev.Reverse();
            colCopy.Sort();
            var blocksSpwaned = FindObjectsOfType<Block>();
            var sounds = GetComponents<AudioSource>();

            var rowLen = blocksSpwaned.Where(block => (float)Math.Round(block.transform.position.y) == rowRounded).Count();
            var columnLen = blocksSpwaned.Where(block => (float)Math.Round(block.transform.position.x) == colRounded).Count();
            if ((rowCopy.SequenceEqual(currentRowSolutions) || rowCopyRev.SequenceEqual(currentRowSolutions)) && rowLen==5)
            {
                

                //positionOnGridByColumn[(float)Math.Round(column)].Remove((float)Math.Round(row));
                //positionOnGridByRow[(float)Math.Round(row)].Remove((float)Math.Round(column));

                foreach (var block in blocksSpwaned)
                {

                    var currentRow = (float)Math.Round(block.transform.position.y);
                    var currentColumn = (float)Math.Round(block.transform.position.x);
                    if (currentRow == rowRounded)
                    {


                        block.DeleteBlock();
                        
                    }
                    
                }

                IncreaseScore("row");
                rowCleared = true;
                sounds[0].Play();
                return true;
            }

            //FIRGURE OUT WHY ONE BLOCK IS BEING DELETED FROM TOP OF STACK
            else if (colCopy.SequenceEqual(currentColumnolutions) || colCopyRev.SequenceEqual(currentColumnolutions) && columnLen==4)
            {
                

                //positionOnGridByColumn[(float)Math.Round(column)].Remove((float)Math.Round(row));
                //positionOnGridByRow[(float)Math.Round(row)].Remove((float)Math.Round(column));

                foreach (var block in blocksSpwaned)
                {

                    var currentRow = (float)Math.Round(block.transform.position.y);
                    var currentColumn = (float)Math.Round(block.transform.position.x);
                    Debug.Log(currentColumn);
                    if (currentColumn == colRounded)
                    {


                        block.DeleteBlock();

                    }

                }
                IncreaseScore("column");
                columnsCleared = true;
                sounds[0].Play();
                return true;
            }

        }

        return false;
    }

    public void InplaceSound()
    {
        var sound = GetComponents<AudioSource>();
        sound[1].Play();
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
        var blocksAboveShiftedRow = blocksAlive.Where(b => b.transform.position.y > prevDeletedRow);

        blocksToMove = blocksAboveShiftedRow.Count();
        if (  blocksAboveShiftedRow.Count() ==0)
        {
            Debug.Log("function cancel");
            CreateNewBlock();
            return;
        }


        Debug.Log("Blocs above are " + blocksAboveShiftedRow.Count());
        foreach (var block in blocksAboveShiftedRow)
        {
            Debug.Log("function ran");

            var rowRouded = (float)Math.Round(block.transform.position.y);
            var fullRound = rowsTwoPlaces.Select(v => (float)Math.Round(v)).ToList();
            var index = fullRound.IndexOf(rowRouded);


            Debug.Log(index > 0);
            if (index > 0)
            {
                Debug.Log("shifting is hapeningss");
                var towards = new Vector3(block.transform.position.x, rowsTwoPlaces[index - 1], blockZValue);

                UpDateShifting(true);
                block.ShiftBlock(towards);
                RemoveDataAfterDeletion(block.transform.position.y, block.transform.position.x, block.spriteRender.bounds.max, block.spriteRender.bounds, block.solution, true);



            }
        }

      shiftingFunctionFinished = true;

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

        if (boundsByColumn.ContainsKey(m) && boundsByColumn[m].Count != 0)
        {
            var maxBlockInRow = boundsByColumn[m].Max(b => b.max.y);

            return bottom < maxBlockInRow;
        }

        return false;


    }
    public bool BlockBelow(float column, float bottomOfBlock)
    {
        var col = (float)Math.Round(column);
        if (boundsByColumn.ContainsKey(col) && boundsByColumn[col].Count > 0)
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





    public float SnapToBottom(float lowerWall, float xPos, float yPos, float bottomOfBlock)
    {

        //use already built in function for checking block below and then use row array to go one index lower for x value

        var xPosR = (float)Math.Round(xPos);
        var yPosR = (float)Math.Round(yPos);
        //Debug.Log(xPosR);
        if (yPosR != lowerWall && ColumnEmpty(xPosR))
        {
            return lowerWall;
        }

        else if (yPos != lowerWall && !ColumnEmpty(xPosR))
        {

            var currentMaxRow = positionOnGridByColumn[xPosR].Max();
            //Debug.Log(currentMaxRow);

            return rowsTwoPlaces[rows.IndexOf(currentMaxRow) + 1];
        }

        return yPos;
    }




}