using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class Building : MonoBehaviour
{
    public int nb_etages;
    public int type; // 1 for working places, 2 for houses
    private GameObject in_world_object;
    private Vector3 position;
    private Quaternion angle;
    public List<Bonhomme> occupants;
    public List<Bonhomme> people_inside;
    // Start is called before the first frame update
    public void init(int type, GameObject obj, int nb_etages)
    {
        this.type = type;
        this.nb_etages = nb_etages;
        in_world_object = obj;
        if (type == 1)
        {
            in_world_object.transform.GetChild(0).gameObject.transform.localScale = new Vector3(4, 2 * nb_etages, 4);
            in_world_object.transform.GetChild(1).gameObject.transform.localPosition = new Vector3(0, 2 * nb_etages + 0.5f, 3f);
            in_world_object.transform.GetChild(1).gameObject.GetComponent<Light>().color = Color.red;
        }
        if(type == 2)
        {
            in_world_object.transform.GetChild(0).gameObject.transform.localScale = new Vector3(4, nb_etages, 4);
            in_world_object.transform.GetChild(1).gameObject.transform.localPosition = new Vector3(0, 2 * nb_etages + 0.5f, 3f);
            in_world_object.transform.GetChild(1).gameObject.GetComponent<Light>().color = Color.blue;
        }
        in_world_object.transform.GetChild(0).gameObject.transform.localPosition = new Vector3(0, nb_etages, 3f);
        position = in_world_object.transform.GetChild(0).gameObject.transform.position;
        occupants = new List<Bonhomme>();
        people_inside = new List<Bonhomme>();
        SetLightIntensity(0);
    }
    public void DisactiveObject()
    {
        in_world_object.SetActive(false);
    }
    public GameObject getGameobject()
    {
        return in_world_object;
    }
    public Vector3 getPositions()
    {
        return position;
    }
    public static bool isFull(Building building)
    {
        if(building.type == 2)
        {
            return building.occupants.Count >= 4 * building.nb_etages;
        }
        else
        {
            return building.occupants.Count >= 10 * building.nb_etages;
        }
    }
    public bool addNewOccupants(Bonhomme bonhomme)
    {
        if (!isFull(this))
        {
            occupants.Add(bonhomme);
            return true;
        }
        return false;
    }

    public void SetLightIntensity(int intensity)
    {
        in_world_object.transform.GetChild(1).gameObject.GetComponent<Light>().intensity = intensity;
    }
    public void enter_building(Bonhomme bonhomme)
    {
        bonhomme.agent.isStopped = true;
        people_inside.Add(bonhomme);
        bonhomme.inside_building = this;
        Collider collider;
        if (bonhomme.type == 0)
        {
            Renderer[] renderers = bonhomme.gameObject.transform.GetComponentsInChildren<Renderer>();
            collider = bonhomme.gameObject.transform.GetChild(0).GetComponent<Collider>();
            collider.enabled = false;
            foreach (Renderer renderer in renderers)
                renderer.enabled = false;
        }
        else
        {
            Renderer[] renderers = bonhomme.gameObject.transform.GetComponentsInChildren<Renderer>();
            collider = bonhomme.gameObject.transform.GetChild(1).GetComponent<Collider>();
            collider.enabled = false;
            foreach (Renderer renderer in renderers)
                renderer.enabled = false;
        }
        SetLightIntensity(10 * people_inside.Count / occupants.Count);
    }
    public void exit_building(Bonhomme bonhomme)
    {
        if (!people_inside.Remove(bonhomme))
        {
            Debug.Log("Cette personne n'est pas dedans");
            return;
        }
        Collider collider;
        if (bonhomme.type == 0)
        {
            Renderer[] renderers = bonhomme.gameObject.transform.GetChild(0).GetComponentsInChildren<Renderer>();
            collider = bonhomme.gameObject.transform.GetChild(0).GetComponent<Collider>();
            collider.enabled = true;
            foreach (Renderer renderer in renderers)
                renderer.enabled = true;
        }
        else
        {
            Renderer[] renderers = bonhomme.gameObject.transform.GetChild(1).GetComponentsInChildren<Renderer>();
            collider = bonhomme.gameObject.transform.GetChild(1).GetComponent<Collider>();
            collider.enabled = true;
            foreach (Renderer renderer in renderers)
                renderer.enabled = true;
        }
        bonhomme.agent.isStopped = false;
        bonhomme.inside_building = null;
        SetLightIntensity(0);
    }
}
