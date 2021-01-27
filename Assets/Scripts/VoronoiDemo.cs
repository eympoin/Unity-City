using UnityEngine;
using System.Collections.Generic;
using Delaunay;
using Delaunay.Geo;
using UnityEngine.AI;

/*
 TODO:
 batiments différent en fonction de l'occupation (light)
 */
public class VoronoiDemo : MonoBehaviour
{

    public Material land;
    public const int NPOINTS = 400;
    public const int WIDTH = 200;
    public const int HEIGHT = 200;
	public float freqx = 0.02f, freqy = 0.018f, offsetx = 0.43f, offsety = 0.22f;
    public GameObject road;
    public GameObject street;
    public GameObject bureau;
    public GameObject house;
    NavMeshSurface[] surfaces;
    public GameObject plane;
    public int planeSize = 150;

    private List<Vector2> m_points;
	private List<LineSegment> m_edges = null;
	private List<LineSegment> m_spanningTree;
	private List<LineSegment> m_delaunayTriangulation;
	private Texture2D tx;
    Bonhomme[] bonhommes;
    GameObject[] roads;
    List <Building> working_places = new List<Building>();
    List<Building> houses = new List<Building>();
    public NavMeshAgent agent;
    bool LightUp;

    private float [,] createMap() 
    {
        float [,] map = new float[WIDTH, HEIGHT];
        for (int i = 0; i < WIDTH; i++)
            for (int j = 0; j < HEIGHT; j++)
                map[i, j] = Mathf.PerlinNoise(freqx * i + offsetx, freqy * j + offsety);
        return map;
    }

    private int[] Search_density(float[,] map, float p)
    {
        int[] res = new int[] { 0, 0 };
        float density = 0;
        for (int i = 0; i < WIDTH; i++)
        {
            for (int j = 0; j < HEIGHT; j++)
            {
                density += Mathf.Pow(map[i, j], 20);
                if (density > p)
                {
                    res[0] = i;
                    res[1] = j;
                    return res;
                }
            }
        }
        return res;
    }
    private int get_nb_etages(float density)
    {
        if (density < 0.1)
        {
            return Random.Range(9, 11);
        }
        if (density < 0.12)
        {
            return Random.Range(7, 9);
        }
        if (density < 0.16)
        {
            return Random.Range(5, 7);
        }
        if (density < 0.18)
        {
            return Random.Range(4, 6);
        }
        if (density < 0.2)
        {
            return Random.Range(2, 4);
        }
        return Random.Range(2, 3);
    }

    void Start ()
	{
        LightUp = true;

        float[,] map=createMap();
        float[,] map20 = new float[WIDTH,HEIGHT];
        Color[] pixels = createPixelMap(map);

        /* Create random points points */
		
		m_points = new List<Vector2> ();
        float total_density = 0;
        for (int i = 0; i < WIDTH; i++)
        {
            for (int j = 0; j < HEIGHT; j++)
            {
                total_density += Mathf.Pow(map[i, j],20);
                map20[i, j] = Mathf.Pow(map[i, j], 20);
            }
        }
        List<uint> colors = new List<uint> ();
        for (int k = 0; k < NPOINTS; k++) {
            colors.Add((uint)0);
            float p = Random.Range(0, total_density);

            int[] pos = Search_density(map, p);
            Vector2 vec = new Vector2(pos[0], pos[1]); 
			m_points.Add (vec);
            
        }

        /* Generate Graphs */
        Delaunay.Voronoi v = new Delaunay.Voronoi (m_points, colors, new Rect (0, 0, WIDTH, HEIGHT));
		m_edges = v.VoronoiDiagram ();

        roads = new GameObject[m_edges.Count];
        /* Shows Voronoi diagram */
        Color color = Color.blue;
		for (int i = 0; i < m_edges.Count; i++) {
			LineSegment seg = m_edges[i];				
			Vector2 left = (Vector2)seg.p0;
			Vector2 right = (Vector2)seg.p1;
            Vector2 segment = (right - left)/WIDTH*100*planeSize;
            Vector2 middle = Vector2.Lerp(left, right, 0.5f);
            GameObject way;
            float angle = Vector2.SignedAngle(Vector2.right,right-left);
            if (((map[(int)(middle.y + 1000) / 10, (int)(middle.x + 1000) / 10] > 0.16 && map[(int)(middle.y + 1000) / 10, (int)(middle.x + 1000) / 10] < 0.163f)
                || (map[(int)(middle.y + 1000) / 10, (int)(middle.x + 1000) / 10] > 0.1753 && map[(int)(middle.y + 1000) / 10, (int)(middle.x + 1000) / 10] < 0.18f))
                && Vector3.Distance(left, right) < 5)
            {
                way = Instantiate(street, new Vector3(left.y / WIDTH * 10 * planeSize - (5 * planeSize), 0.15f, left.x / HEIGHT * 10 * planeSize - (5 * planeSize)), Quaternion.Euler(0, angle + 90, 0));
            }
            else
            {
                way = Instantiate(road, new Vector3(left.y / WIDTH * 10 * planeSize - (5 * planeSize), 0.15f, left.x / HEIGHT * 10 * planeSize - (5 * planeSize)), Quaternion.Euler(0, angle + 90, 0));
            }
            way.transform.localScale = new Vector3(segment.magnitude, 1, 1);
            roads[i] = way;                  
        }

        for (int i = 0; i < m_edges.Count; i++)
        {
            LineSegment seg = m_edges[i];
            Vector2 left = (Vector2)seg.p0;
            Vector2 right = (Vector2)seg.p1;
            Vector2 segment = (right - left) / WIDTH * 100 * planeSize;
            Vector2 middle = Vector2.Lerp(left, right, 0.5f);
            float angle = Vector2.SignedAngle(Vector2.right, right - left);
            Building building;
            int nb_etages;
            if (map[(int)(middle.y + 1000) / 10, (int)(middle.x + 1000) / 10] < 0.18 && Vector3.Distance(left,right) > 0.5f)
            {
                nb_etages = get_nb_etages(map[(int)(middle.y + 1000) / 10, (int)(middle.x + 1000) / 10]);
                GameObject objBuilding = Instantiate(bureau, new Vector3((middle.y / WIDTH) * 10 * planeSize - (5 * planeSize), 0.1f, (middle.x / HEIGHT) * 10 * planeSize - (5 * planeSize)), Quaternion.Euler(0, angle + 90, 0));
                building = objBuilding.AddComponent(typeof(Building)) as Building;
                building.init(1, objBuilding, nb_etages);
                bool intersected_road = false;
                for (int j = 0; j < m_edges.Count; j++)
                {
                    if (building.getGameobject().transform.GetChild(0).GetComponentInChildren<Collider>().bounds.Intersects(roads[j].GetComponentInChildren<Collider>().bounds))
                    {
                        intersected_road = true;
                        building.DisactiveObject();
                    }
                }
                if (!intersected_road)
                {
                    working_places.Add(building);
                }
            }
            else if (Vector3.Distance(left, right) > 0.5f)
            {
                nb_etages = get_nb_etages(map[(int)(middle.y + 1000) / 10, (int)(middle.x + 1000) / 10]);
                GameObject objBuilding = Instantiate(house, new Vector3((middle.y / WIDTH) * 10 * planeSize - (5 * planeSize), 0.1f, (middle.x / HEIGHT) * 10 * planeSize - (5 * planeSize)), Quaternion.Euler(0, angle + 90, 0));
                building = objBuilding.AddComponent(typeof(Building)) as Building;
                building.init(2, objBuilding, nb_etages);
                bool intersected_road = false;
                for (int j = 0; j < m_edges.Count; j++)
                {
                    if (building.getGameobject().transform.GetChild(0).GetComponentInChildren<Collider>().bounds.Intersects(roads[j].GetComponentInChildren<Collider>().bounds))
                    {
                        intersected_road = true;
                        building.DisactiveObject();
                    }
                }
                if (!intersected_road)
                {
                    houses.Add(building);
                }
            }
            
        }
        plane.GetComponent<NavMeshSurface>().BuildNavMesh();         
        /* Apply pixels to texture */
        tx = new Texture2D(WIDTH, HEIGHT);
        land.SetTexture ("_MainTex", tx);
		tx.SetPixels (pixels);
        //tx.Apply ();
        bonhommes = new Bonhomme[100];
        List<Building> emptyHouses = houses;
        List<Building> emptyWorks = working_places;
        
        emptyHouses.RemoveAll(Building.isFull);
        emptyWorks.RemoveAll(Building.isFull);
        for (int i = 0; i < 100; i++)
        {
            emptyHouses.RemoveAll(Building.isFull);
            emptyWorks.RemoveAll(Building.isFull);

            int NumHouse = Random.Range(0, emptyHouses.Count);
            int NumWork = Random.Range(0, emptyWorks.Count);
            //TODO cas tout est full

            GameObject[] roads = GameObject.FindGameObjectsWithTag("road");
            float minDistHouse = float.MaxValue;
            float minDistWork = float.MaxValue;
            int closestRoadToHouse = 0;
            int closestRoadToWork = 0;
            for (int j = 0; j < roads.Length; j++)
            {
                if (Vector3.Distance(roads[j].transform.position, emptyHouses[NumHouse].getPositions()) < minDistHouse)
                {
                    closestRoadToHouse = j;
                    minDistHouse = Vector3.Distance(roads[j].transform.position, emptyHouses[NumHouse].getPositions());
                }
                if (Vector3.Distance(roads[j].transform.position, emptyWorks[NumWork].getPositions()) < minDistWork)
                {
                    closestRoadToWork = j;
                    minDistWork = Vector3.Distance(roads[j].transform.position, emptyWorks[NumWork].getPositions());
                }
            }
            Vector3 startPos = roads[closestRoadToHouse].transform.position;
            startPos.y = 1.25f;
            NavMeshAgent BonhommeAgent = NavMeshAgent.Instantiate(agent, startPos, Quaternion.identity);
            Bonhomme humain = BonhommeAgent.gameObject.AddComponent(typeof(Bonhomme)) as Bonhomme;
            humain.init(i,0,emptyHouses[NumHouse], emptyWorks[NumWork], BonhommeAgent, roads[closestRoadToHouse].transform.position, roads[closestRoadToWork].transform.position);
            bonhommes[i] = humain;
            emptyHouses[NumHouse].addNewOccupants(humain);
            emptyWorks[NumWork].addNewOccupants(humain);

        }
    }

    public void Update()
    {
        /*GameObject Sun = GameObject.Find("Sun");
        if (Sun.GetComponent<Light>().intensity > 0)
        {
            for (int i = 0; i < houses.Count; i++)
            {
                houses[i].SetLightIntensity(0);
            }
            for (int i = 0; i < working_places.Count; i++)
            {
                working_places[i].SetLightIntensity(0);
            }
            LightUp = false;
        }
        if (Sun.GetComponent<Light>().intensity == 0)
        {
            for (int i = 0; i < houses.Count; i++)
            {
                houses[i].SetLightIntensity(10);
            }
            for (int i = 0; i < working_places.Count; i++)
            {
                working_places[i].SetLightIntensity(10);
            }
            LightUp = true;
        }*/
    }



    /* Functions to create and draw on a pixel array */
    private Color[] createPixelMap(float[,] map)
    {
        Color[] pixels = new Color[WIDTH * HEIGHT];
        for (int i = 0; i < WIDTH; i++)
            for (int j = 0; j < HEIGHT; j++)
            {
                pixels[i * HEIGHT + j] = Color.Lerp(Color.white, Color.black, map[i, j]);
            }
        return pixels;
    }
    private void DrawPoint (Color [] pixels, Vector2 p, Color c) {
		if (p.x<WIDTH&&p.x>=0&&p.y<HEIGHT&&p.y>=0) 
		    pixels[(int)p.x*HEIGHT+(int)p.y]=c;
	}
	// Bresenham line algorithm
	private void DrawLine(Color [] pixels, Vector2 p0, Vector2 p1, Color c) {
		int x0 = (int)p0.x;
		int y0 = (int)p0.y;
		int x1 = (int)p1.x;
		int y1 = (int)p1.y;

		int dx = Mathf.Abs(x1-x0);
		int dy = Mathf.Abs(y1-y0);
		int sx = x0 < x1 ? 1 : -1;
		int sy = y0 < y1 ? 1 : -1;
		int err = dx-dy;
		while (true) {
            if (x0>=0&&x0<WIDTH&&y0>=0&&y0<HEIGHT)
    			pixels[x0*HEIGHT+y0]=c;

			if (x0 == x1 && y0 == y1) break;
			int e2 = 2*err;
			if (e2 > -dy) {
				err -= dy;
				x0 += sx;
			}
			if (e2 < dx) {
				err += dx;
				y0 += sy;
			}
		}
	}
}