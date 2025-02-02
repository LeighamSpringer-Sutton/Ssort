﻿using System;
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
    //TODO CURRENT BUG BLOCK IS DESTORY ORIGINAL INSTANCE MAKE CLOSE WHEN INSTATIATING INSTEAD OF INSTIATING ORIGINAL**

    [SerializeField] private Sprite[] allBocks;
    [SerializeField] private TMPro.TextMeshProUGUI equationText;
    //[SerializeField] private Canvas canvas;
    [SerializeField] private GameObject particules;
    [SerializeField] public TMPro.TextMeshProUGUI UpComingText;

    private float amountToMove = 2.57f;
    private float totalAmountToMove;
    private float screenWidth = 10f;
    private float leftMaxWallLocation = 1.52f;
    private float rightMaxWallLocation = 11.81f;
    private bool lowerWallHit = false;
    private bool active = false;
    private DateTime currentTimeOfLowerPortionOfBlockHit;
    private float bottomOfMap = 1.52f;
    private Color color;
    private Color textColor;
    public SpriteRenderer spriteRender;
    private DateTime blinkTimeStamp;
    private Datamanager datamanager;
    private float blockZValue = 3f;
    private UnityEngine.Object[] blocksOnMap;
    private float SecondColumnConfirm = 1;
    private int IgnoreFirstTrue = 1;
    private bool timeTriggered = false;
    private string textLeftMargin;
    public int solution;
    private int blockCreated = 0;
    public bool shiftBlock = false;
    private float shiftSpeed = 1.0f;
    private float textZvalue = -2f;
    private Vector3 whereToShift;
    private AudioSource movementSound;
    [SerializeField]private SceneLoader sceneLoader;

   


    //CREATE A METHOD TO SHIFT DOWN ALL DATA BUT PRIOR TO THAT REMOVE ALL BOUNDS BOUNDS CHANGE BASED OFF NEW X AND YS

    public void OnBlockCreation()
    {
        //if bottom of block not hit keep block falling

        //text moving relative to canvas

        //

        movementSound = GetComponents<AudioSource>()[0];
        blocksOnMap = FindObjectsOfType<Block>();

        datamanager = FindObjectOfType<Datamanager>();
        spriteRender = GetComponent<SpriteRenderer>();

        var pos = new Vector3(6.6f, 10.74f, blockZValue);


        transform.position = pos;


        var sign = datamanager.PullSign();


        var equation = datamanager.PullEquation();

        datamanager.SetUpcomingText(UpComingText);
        var num1Str = equation[0].ToString();
        var num2Str = equation[1].ToString();
        var spaces = datamanager.PositionTextBasedOffEquation(num1Str.Length + num2Str.Length);
        equationText.text = spaces + equation[0] + sign + equation[1];

        solution = datamanager.ComputeSolution(equation[0], equation[1], sign);
        var blockColor = allBocks[Random.Range(0, 10)];

        var BlockColorNameSplit = blockColor.name.Split('_');
        var rgbValues = PickParticleColor(BlockColorNameSplit[BlockColorNameSplit.Length-1]);
        spriteRender.sprite = blockColor;




        
        var system = particules.GetComponent<ParticleSystem>();

        var sysMain = system.main;


        sysMain.startColor = new Color(rgbValues[0], rgbValues[1], rgbValues[2], rgbValues[3]);

        
        
        



        color = spriteRender.color;
        textColor = equationText.color;


        active = true;
    }

    public int[] PickParticleColor(string color)
    {
        switch (color)
        {
            case "Black":
                return new int[] { 0,0,255,255};

            case "Blue":
                return new int[] { 60, 149, 248, 255 };
            case "Cyan":
                return new int[] { 122, 252, 255, 255 };
            case "Green":
                return new int[] { 118, 246,110, 255 };
            case "Grey":
                return new int[] { 206, 155, 255, 255 };
            case "Orange":
                return new int[] { 255, 151, 85, 255 };
            case "Pink":
                return new int[] { 253, 159, 224, 255 };
            case "Purple":
                return new int[] { 188, 58, 231, 255 };
            case "Red":
                return new int[] { 255, 64, 116, 255 };
            case "White":
                return new int[] { 255, 255, 255, 255 };
            case "Yellow":
                return new int[] { 255, 244, 134, 255 };
        }
        return new int[] {255, 255, 255, 255 };
    }


    // Update is called once per frame
    public bool ShiftBlockStatus()
    {
        return shiftBlock;
    }

    public void DeleteParticle()
    {
        DestroyObject(particules);
    }
    void Update()
    {










        if (!(shiftBlock && datamanager.shiftingFunctionFinished))
        {
            equationText.transform.position = transform.position;
        }
        





        if (shiftBlock && datamanager.shiftingFunctionFinished)
        {

            
            transform.position = Vector3.MoveTowards(transform.position, whereToShift, shiftSpeed * Time.deltaTime);
            equationText.transform.position = Vector3.MoveTowards(transform.position, whereToShift, shiftSpeed * Time.deltaTime);
            if (whereToShift.y == transform.position.y)
            {

                transform.position = whereToShift;
                
                //equationText.transform.position = whereToShift;
                shiftBlock = false;
                datamanager.blocksMoved++;
                StoreBlcokData(transform.position.x, transform.position.y);
                if (datamanager.blocksMoved == datamanager.blocksToMove)
                {

                    
                    datamanager.CreateNewBlock();
                    datamanager.blocksMoved = 0;
                }
                

            }





        }


        if (!active && !shiftBlock)
        {
            if (color.a != 1.0)
            {
                color.a = 1.0f;

            }
            return;
        }


        if (currentTimeOfLowerPortionOfBlockHit != DateTime.MinValue && TimeInSecondsPast(currentTimeOfLowerPortionOfBlockHit, 2) && (lowerWallHit || IgnoreFirstTrue > 1) && !shiftBlock)
        {



           

           

            var newyPos=0f;
            try
            {
                newyPos = datamanager.SnapToBottom(bottomOfMap, transform.position.x, transform.position.y, spriteRender.bounds.min.y);
            }


            catch{
                

                sceneLoader.LoadNextScene();
                return;
            }


            transform.position = new Vector3(transform.position.x, newyPos, blockZValue);

            StoreBlcokData(transform.position.x, transform.position.y);

            active = false;
            if (datamanager.CheckForMatches(transform.position.y, transform.position.x))
            {

                

                

            }


            else
            {
                datamanager.InplaceSound();
                datamanager.CreateNewBlock();
                color.a = 1.0f;
                spriteRender.color = color;

            }






        }






        else if (isActive())
        {


            if (!lowerWallHit && !BottomBlockHit())
            {
                Gravity();
            }
            else if (lowerWallHit || BottomBlockHit() && !shiftBlock)
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

                }

            }

        }





    }



    public void ShiftBlock(Vector3 whereTo)
    {
        whereToShift = whereTo;

        shiftBlock = true;
    }


    public bool isActive()
    {
        return active;
    }



    private void ShiftAllDataDownOneRow()
    {
        //TODO
    }

    public void DeleteBlock()
    {
        TriggerParticules();
        datamanager.RemoveDataAfterDeletion(transform.position.y, transform.position.x, spriteRender.bounds.max, spriteRender.bounds, solution, false);
        
        DestroyObject(gameObject);

    }


   
    private void TriggerParticules()
    {
        
        var particle = Instantiate(particules, new Vector3(transform.position.x, transform.position.y, 0), transform.rotation);
        
        ParticleSystem parts = particle.GetComponent<ParticleSystem>();
        float totalDuration = parts.duration + parts.startLifetime;
        Destroy(particle, totalDuration);
    }







    public void StoreBlcokData(float currentX, float currentY)
    {
        datamanager.AddToRowsDictionary(transform.position);
        datamanager.AddToColumnsDictionary(transform.position);
        datamanager.AddToGrid(new Vector3((float)Math.Round(currentX), (float)Math.Round(currentY)));

        datamanager.AddToTopsOfBlockData(spriteRender.bounds.max);

        datamanager.AddToBoundsByColumn(transform.position.x, spriteRender.bounds);
        datamanager.AddToSolutionRowsDictionary(transform.position.y, transform.position.x, solution);
        datamanager.AddToSolutionColumnDictionary(transform.position.y, transform.position.x, solution);

    }

    private bool BottomBlockHit()
    {



        if (datamanager.BlockBelow(transform.position.x, spriteRender.bounds.min.y))
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





            totalAmountToMove = Mathf.Clamp(transform.position.x - amountToMove, leftMaxWallLocation, max: rightMaxWallLocation);




            if (datamanager.GridCellFull((float)Math.Round(totalAmountToMove), (float)Math.Round(transform.position.y)))
            {

                return;
            }

            if (datamanager.PotentialOverlapDetected(totalAmountToMove, spriteRender.bounds.min.y))
            {

                return;
            }
            transform.position = new Vector3(totalAmountToMove, transform.position.y);
            movementSound.Play();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {

            //amountToMove = 2.57f;
            totalAmountToMove = Mathf.Clamp(amountToMove + transform.position.x, leftMaxWallLocation, max: rightMaxWallLocation);

            if (datamanager.GridCellFull((float)Math.Round(totalAmountToMove), (float)Math.Round(transform.position.y)))
            {

                return;
            }

            if (datamanager.PotentialOverlapDetected(totalAmountToMove, spriteRender.bounds.min.y))
            {

                return;
            }

            //Debug.Log((float)Math.Round(totalAmountToMove, 1) + "," + (float)Math.Round(transform.position.y, 1) + " is not contained");

            transform.position = new Vector3((float)Math.Round(totalAmountToMove, 1), (float)Math.Round(transform.position.y, 1));
            movementSound.Play();

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






    bool TimeInSecondsPast(DateTime current, double secondsPast)
    {

        if (DateTime.Now > current + TimeSpan.FromSeconds(secondsPast))
        {

            return true;
        }

        return false;

    }

    void ActiveBlink()
    {


        if (!TimeInSecondsPast(blinkTimeStamp, 0.5) && blinkTimeStamp != DateTime.MinValue)
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