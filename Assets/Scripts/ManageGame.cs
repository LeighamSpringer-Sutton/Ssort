﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManageGame : MonoBehaviour
{
    // Start is called before the first frame update

    public Datamanager dataManager;

    void Awake()
    {
        dataManager = GetComponent<Datamanager>();
    }
    
   
}
