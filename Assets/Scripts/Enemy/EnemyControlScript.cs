﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EnemyControlScript : MonoBehaviour
{
    public float moveSpeed;
    public GameObject[] notAffected;
    public GameObject[] wayPoints;
    public bool isCaptured;
    private Vector3 capturedTransform;
    public bool isLeaving;
    private int nextWayPoint;
    private bool isActive = true;
    public int hotelSpace;
    public int cardDegrees;
    private int waveEntered;
    public bool isLast;

    // Use this for initialization
    void Start()
    {

    }

    void Awake()
    {
        nextWayPoint = 0;
        isCaptured = false;
        isLast = false;
    }

    // Update is called once per frame
    void Update()
    {
    }

    //public IEnumerator moveTowardsNext()
    //{
    //    while (!isCaptured && isActive && Vector3.Distance(transform.position, wayPoints[nextWayPoint].transform.position) > 0.1)
    //    {
    //        transform.GetComponent<Rigidbody2D>().velocity = ((wayPoints[nextWayPoint].transform.position - transform.position).normalized * moveSpeed);
    //        yield return null;
    //    }
    //}

    public void moveTowardsNext()
    {
        
        transform.GetComponent<Rigidbody2D>().velocity = ((wayPoints[nextWayPoint].transform.position - transform.position).normalized * moveSpeed);
    }

    public IEnumerator moveTowardsAttractor(Transform attractor)
    {
        Debug.Log("movetowards Attractor");
        while (isCaptured && Vector3.Distance(transform.position, attractor.position) > 0.4)
        {
            transform.position = Vector3.MoveTowards(transform.position, attractor.position, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }

    public IEnumerator moveTowardsExit(Vector3 exit)
    {
        Debug.Log("movetowards exit");

        while (Vector3.Distance(transform.position, exit) > 0.01)
        {
            transform.position = Vector3.MoveTowards(transform.position, exit, moveSpeed * Time.deltaTime);
            yield return null;
        }

        isLeaving = false;

        moveTowardsNext();
    }

    public IEnumerator derez(float timeSpentIn, BAttraction Attractor)
    {
        //gameObject.GetComponent<Renderer>().enabled = false;
        Attractor.currCapacity++;

        ClickToBuild UIcontrol = GameObject.Find("PlayerController").GetComponent<ClickToBuild>();
        UIcontrol.currMoney += Attractor.moneyEarned;

        yield return new WaitForSeconds(timeSpentIn);
        
      
        Attractor.currCapacity--;
        //gameObject.GetComponent<Renderer>().enabled = 
        isLeaving = true;
        isCaptured = false;
        yield return moveTowardsExit(capturedTransform);

    }

    void OnTriggerEnter2D(Collider2D other)
    {

        if (other.transform.tag == "Attraction Collider" )
        {
            GameObject Attractor = other.transform.parent.gameObject;

            waveEntered = GameObject.Find("SpawnPoint").GetComponent<SpawnControlScript>().waveNumber;

            if (Attractor.GetComponent<BAttraction>().currCapacity < Attractor.GetComponent<BAttraction>().maxCapacity)
            {
                transform.GetComponent<Rigidbody2D>().velocity = new Vector3(0, 0, 0);
                isCaptured = true;
                capturedTransform = transform.position;
                //other.gameObject.GetComponent<Rigidbody2D>().velocity = new Vector3(0, 0, 0);
                StartCoroutine(moveTowardsAttractor(Attractor.transform));
            } 
        }
        else if(other.transform.tag == "Attraction" )
        {
            BAttraction Attractor = other.transform.gameObject.GetComponent<BAttraction>();
            StartCoroutine(derez(Attractor.timeSpentIn, Attractor));
            //energy -= Attractor.energySubtraction;

            //if(energy <= 0)
            //{
            //    if(energy < 0)
            //    {
            //        energy = 0;
            //    }
            //    transform.gameObject.GetComponent<Renderer>().material.color = Color.grey;
            //}
        }
        else if (other.transform.tag == "Waypoint")
        {
            if(!isLeaving)
            {
                ++nextWayPoint;
                moveTowardsNext();
            }
        }
        else if (other.transform.tag == "Final")
        {
                ++nextWayPoint;
            moveTowardsNext();
        }
        else if(other.transform.tag == "Town")
        {
            //Debug.Log("good1");
            if (isLast)
            {
                GameObject.Find("StartButton").GetComponent<Button>().interactable = true;
                isLast = false;
            }

            TownControlScript control = other.transform.gameObject.GetComponent <TownControlScript>();
            control.trashCount += hotelSpace;
            if(control.trashCount > control.trashCapacity)
            {
                SceneManager.LoadScene("gameOverScene");
            }
            isActive = false;
            Destroy(this.gameObject);

            
        }
        else if(other.transform.tag == "Hotel")
        {
            isActive = false;
            Destroy(this.gameObject);
        }
    }
}
