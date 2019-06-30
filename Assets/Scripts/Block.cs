using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;
using TMPro;
using UnityEditor;

public class Block : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] private Sprite[] allBocks;
    [SerializeField] private TMPro.TextMeshProUGUI equationText;
    [SerializeField] private Canvas canvas;
    private float amountToMove = 2.57f;
    private float totalAmountToMove;
    private float screenWidth = 10f;
    private float leftMaxWallLocation = 1.52f;
    private float rightMaxWallLocation = 11.81f;
    private bool lowerWallHit = false;
    private bool active = true;
    private DateTime currentTimeOfLowerPortionOfBlockHit;
    private float bottomOfMap = 1.52f;
    private Color color;
    private SpriteRenderer  spriteRender;
    private DateTime blinkTimeStamp ;
    private Datamanager datamanager;
    private float blockZValue = 2f;
    private UnityEngine.Object[] blocksOnMap;
    private float SecondColumnConfirm = 1;
    private int IgnoreFirstTrue =1;
    private bool timeTriggered = false;
    private string textLeftMargin;
    private int solution;
    void Start()
    {
        //if bottom of block not hit keep block falling
        
        //text moving relative to canvas
        active = true;
        
        blocksOnMap = FindObjectsOfType<Block>();
        
        datamanager = FindObjectOfType<Datamanager>();
        spriteRender =  GetComponent<SpriteRenderer>();
        
        var pos = new Vector3(6.6f, 10.74f, blockZValue);

        
        transform.position = pos;
        

        var sign = GenerateOperand();
        var equation = GenerateEquation(sign);
        var num1Str = equation[0].ToString();
        var num2Str = equation[1].ToString();
        var spaces = PositionTextBasedOffEquation(num1Str.Length+num2Str.Length);
        equationText.text = spaces + equation[0] + sign + equation[1]; 
        
        solution = ComputeSolution(equation[0], equation[1],sign);
        spriteRender.sprite = allBocks[Random.Range(0,10)];
        color = spriteRender.color;

       
        
    }

    // Update is called once per frame
    
    void Update()
    {





        equationText.transform.position = transform.position;


        if (!active)
        {
            if (color.a != 1.0)
            {
                color.a = 1.0f;
                spriteRender.color = color;
            }
            return;
        }

        
        if (currentTimeOfLowerPortionOfBlockHit != DateTime.MinValue && TimeInSecondsPast(currentTimeOfLowerPortionOfBlockHit, 2) && (lowerWallHit || IgnoreFirstTrue>1))
        {
            
            
            
            
            
            //datamanager.AddToblocksOnMap(gameObject);
            


            var newyPos = datamanager.SnapToBottom(bottomOfMap, transform.position.x, transform.position.y,spriteRender.bounds.min.y);


            transform.position = new Vector3(transform.position.x, newyPos, blockZValue);

            StoreBlcokData(transform.position.x,transform.position.y);


            datamanager.CheckForMathes(transform.position.y);

            active = false;
            color.a = 1.0f;
            spriteRender.color = color;
            datamanager.CreateNewBlock();

        }
        if (!lowerWallHit && !BottomBlockHit())
        {
            Gravity();
        }
        else if (lowerWallHit || BottomBlockHit() )
        {
            ActiveBlink();
        }
        PlayerKeyListner();



        
        if (BottomBlockHit())
            {
            

            if (!timeTriggered && IgnoreFirstTrue > 1)
            {
                currentTimeOfLowerPortionOfBlockHit = DateTime.Now;
                timeTriggered = true;
                Debug.Log("Hit bottom");
            }
            
        }
            
        
        
        


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


    public int ComputeSolution(int num1,int num2,string operand)
    {

        int answer = 0 ;
        switch(operand)
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


    public string GenerateOperand()
    {

        var operands = new string[] { "+", "-", "/", "*" };
        return operands[Random.Range(0, operands.Length - 1)];
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



    public void StoreBlcokData(float currentX, float currentY)
    {
        datamanager.AddToRowsDictionary(transform.position);
        datamanager.AddToColumnsDictionary(transform.position);
        datamanager.AddToGrid(new Vector3((float)Math.Round(currentX), (float)Math.Round(currentY)));

        datamanager.AddToTopsOfBlockData(spriteRender.bounds.max);
        datamanager.AddToBottomsOfBlockData(spriteRender.bounds.min);
        datamanager.AddToBoundsByColumn(transform.position.x, spriteRender.bounds);
        datamanager.AddToSolutionRowsDictionary(transform.position.y, transform.position.x,solution);
    }

    private bool BottomBlockHit()
    {

        
        
        if (datamanager.BlockBelow(transform.position.x,spriteRender.bounds.min.y) )
        {
            if (IgnoreFirstTrue > 1)
            {
                //CurrentTimeOfLowerPortionOfBlockHit
                
                return true;
            }
            IgnoreFirstTrue++;
            
                
            
        }
        return false;
    }


    private void PlayerKeyListner()
    {
        
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            //double due to each block taking up 2 pixels worth of units





            totalAmountToMove = Mathf.Clamp(transform.position.x-amountToMove, leftMaxWallLocation, max: rightMaxWallLocation);

            


            if (datamanager.GridCellFull((float)Math.Round(totalAmountToMove), (float)Math.Round(transform.position.y)))
            {
                
                return;
            }

            if (datamanager.PotentialOverlapDetected(totalAmountToMove,spriteRender.bounds.min.y))
            {
                Debug.Log("dectected");
                return;
            }
            transform.position = new Vector3(totalAmountToMove, transform.position.y);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {

            //amountToMove = 2.57f;
            totalAmountToMove = Mathf.Clamp(amountToMove + transform.position.x, leftMaxWallLocation, max:rightMaxWallLocation);

            if (datamanager.GridCellFull((float)Math.Round(totalAmountToMove), (float)Math.Round(transform.position.y)))
            {
                
                return;
            }

            if (datamanager.PotentialOverlapDetected(totalAmountToMove, spriteRender.bounds.min.y))
            {
                Debug.Log("dectected");
                return;
            }

            //Debug.Log((float)Math.Round(totalAmountToMove, 1) + "," + (float)Math.Round(transform.position.y, 1) + " is not contained");

                transform.position = new Vector3((float)Math.Round(totalAmountToMove, 1), (float)Math.Round(transform.position.y, 1));
            
            
        }

        
        
        else if (Input.GetButton("Vertical") && transform.position.y > bottomOfMap && !BottomBlockHit())
        {
            
            

            transform.position -= new Vector3(0f, 0.01f);
            
        }
        

    }

    private void Gravity()
    {
        if (transform.position.y > bottomOfMap)
        {
            transform.position -= transform.up * Time.deltaTime;
            
        }
        else
        {
            lowerWallHit = true;
            
            currentTimeOfLowerPortionOfBlockHit = DateTime.Now;

            
        }
        
    }


    

  
    
    bool TimeInSecondsPast(DateTime current,double secondsPast)
    {

        if (DateTime.Now > current + TimeSpan.FromSeconds(secondsPast))
        {
            
            return true;
        }

        return false;
        
    }
    
    void ActiveBlink()
    {


        if (!TimeInSecondsPast(blinkTimeStamp,0.5) && blinkTimeStamp != DateTime.MinValue)
        {
            return;
        }
        if (color.a == 1f)
        {
            
            color.a = 0.4f;
            
        }
        else
        {
            color.a = 1;
        }
        
        blinkTimeStamp = DateTime.Now;
        spriteRender.color = color;
    }

    
   
}
