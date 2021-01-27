using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Bonhomme : MonoBehaviour
{
    public Building house;
    public Building work;
    public NavMeshAgent agent;
    public Vector3 HouseSpawn;
    public Vector3 WorkSpawn;
    public Building inside_building;
    public int index;
    public float dist;
    public int type; //0 for car, 1 for 

    public void init(int index, int type,Building house,Building work,NavMeshAgent agent, Vector3 HouseSpawn, Vector3 WorkSpawn)
    {
        this.house = house;
        this.work = work;
        this.HouseSpawn = HouseSpawn;
        this.WorkSpawn = WorkSpawn;
        this.agent = agent;
        this.index = index;
        this.type = type;
        agent.SetDestination(WorkSpawn);
        agent.isStopped = false;
    }

    // Update is called once per frame
    void Update()
        {
        NavMeshHit hit;
        NavMesh.SamplePosition(agent.transform.position, out hit, 1.0f, NavMesh.AllAreas);
        //Debug.Log(hit.mask);
        if(hit.mask == 32 && type == 0)
        {
            changeLocomotion();
        }
        if (hit.mask == 8 && type == 1)
        {
            changeLocomotion();
        }
        DayNightCycle day = GameObject.Find("Plane").GetComponent(typeof(DayNightCycle)) as DayNightCycle;
        if(day.currentTimeOfDay > 0.23f && day.currentTimeOfDay < 0.7f && inside_building != work) //if day, go to work
        {
            if(inside_building == house)
            {

                house.exit_building(this);
            }
            agent.destination = WorkSpawn;
        }
        if (day.currentTimeOfDay > 0.7f || day.currentTimeOfDay < 0.23f && !inside_building != house) //if night go to house
        {
            if (inside_building == work)
            {
                work.exit_building(this);
            }
            agent.destination = HouseSpawn;
        }
        dist = Vector3.Distance(agent.destination, agent.gameObject.transform.position);
        if (Vector3.Distance(agent.destination, agent.gameObject.transform.position) < 0.3 && day.currentTimeOfDay > 0.7f || day.currentTimeOfDay < 0.23f && inside_building == null) //if arrived to house, enter the building
        { 
            house.enter_building(this);
        }
        if (Vector3.Distance(agent.destination, agent.gameObject.transform.position) < 0.3 && day.currentTimeOfDay > 0.23f && day.currentTimeOfDay < 0.7f && inside_building == null)
        {
            work.enter_building(this);
        }
    }
    void changeLocomotion()
    {
        type = 1 - type;
        if(type == 1)
        {
            gameObject.transform.GetChild(0).GetComponent<Collider>().enabled = false; //deactivate old locomotion
            Renderer[] old_renderers = gameObject.transform.GetChild(0).GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in old_renderers)
                renderer.enabled = false;

            gameObject.transform.GetChild(1).GetComponent<Collider>().enabled = true;
            Renderer[] renderers = gameObject.transform.GetChild(1).GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
                renderer.enabled = true;
            agent.speed = 3.5f;
        }
        else
        {
            gameObject.transform.GetChild(1).GetComponent<Collider>().enabled = false; //deactivate old locomotion
            Renderer[] old_renderers = gameObject.transform.GetChild(1).GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in old_renderers)
                renderer.enabled = false;

            gameObject.transform.GetChild(0).GetComponent<Collider>().enabled = true;
            Renderer[] renderers = gameObject.transform.GetChild(0).GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
                renderer.enabled = true;
            agent.speed = 10;
        }
    }
}
