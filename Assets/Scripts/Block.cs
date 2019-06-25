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
    void Start()
    {

        active = true;
        var bla = new List<Vector3>();

        bla.Add(new Vector3(5,10));
        blocksOnMap = FindObjectsOfType<Block>();
        
        datamanager = FindObjectOfType<Datamanager>();
        spriteRender =  GetComponent<SpriteRenderer>();
        
        var pos = new Vector3(6.6f, 10.74f, blockZValue);
        transform.position = pos;
        spriteRender.sprite = allBocks[Random.Range(0,10)];
        color = spriteRender.color;

       
        
    }

    // Update is called once per frame
    
    void Update()
    {







        if (!active)
        {
            
            return;
        }

       
        
        if (currentTimeOfLowerPortionOfBlockHit != DateTime.MinValue && TimeInSecondsPast(currentTimeOfLowerPortionOfBlockHit, 2) && (lowerWallHit || IgnoreFirstTrue>1))
        {
            
            
            color.a = 1.0f;
            spriteRender.color = color;
            var currentX = transform.position.x;
            var currentY = transform.position.y;
            datamanager.AddToGrid(new Vector3((float)Math.Round(currentX),(float)Math.Round(currentY)));
            
            datamanager.AddToTopsOfBlockData(spriteRender.bounds.max);
            datamanager.AddToBottomsOfBlockData(spriteRender.bounds.min);
            datamanager.AddToBoundsByColumn(transform.position.x,spriteRender.bounds);
            //datamanager.AddToblocksOnMap(gameObject);
            color.a = 1.0f;
            spriteRender.color = color;
            active = false;
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

    private bool BottomBlockHit()
    {

        
        
        if (datamanager.BlockBelowTest(transform.position.x,spriteRender.bounds.min.y) )
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

            if (datamanager.PotentialOverlapDetectedTest(totalAmountToMove,spriteRender.bounds.min.y))
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

            if (datamanager.PotentialOverlapDetectedTest(totalAmountToMove, spriteRender.bounds.min.y))
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

    

    bool PotentialOverlapDetected()
    {   
        
        if (datamanager.PotentialGridOverlap(spriteRender.bounds.min.y) && datamanager.InColumn((float)totalAmountToMove))
        {
            return true;
        }

        return false;
    }
    
}
