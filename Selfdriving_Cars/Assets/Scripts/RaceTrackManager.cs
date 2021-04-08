using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RaceTrackManager : MonoBehaviour
{

    public UnityEvent trackReady;
    public int raceTrackWidth = 100;
    public int trackWidth = 5;
    public int raceTrackHeight = 50;
    public GameObject raceTrackControlPoint;
    public GameObject carPrefab;
    public Layer wallLayer;
    List<GameObject> created = new List<GameObject>();
    List<GameObject> cps = new List<GameObject>();
    public List<GameObject> correctOrder = new List<GameObject>();
    public Vector3 start;
    public Quaternion startRot;
    bool first = true;
    // Start is called before the first frame update
    void GenerateMap()
    {
        DeleteCreatedObjects();
        GenerateControlPoints();
        (List<GameObject> newCps, Vector3 pos) = GeneratePolygonCollider(correctOrder, true);
        GeneratePolygonCollider(newCps);
        deletePoints(newCps);
        newCps.Clear();
        deletePoints(cps);
        cps.Clear();
        correctOrder.Clear();
        start = pos;
        Debug.Log("Set start");
        if(!first)
        {
            GameObject.Find("GAManager").GetComponent<GAManager>().NewEpisode();
        }
        first = false;

    }

    void Start()
    {
        GenerateMap();
        GameObject.Find("GAManager").GetComponent<GAManager>().StartAlgorithm();
        GameObject.Find("GAManager").GetComponent<GAManager>().episode_ended.AddListener(GenerateMap);
    }

    void DeleteCreatedObjects()
    {
        for(int i=created.Count-1;i>=0;i--)
        {
            Destroy(created[i]);
        }
        created.Clear();
    }
    // Update is called once per frame
    void Update()
    {

    }

    void deletePoints(List<GameObject> list)
    {
        for(int i=list.Count-1; i>=0;i--)
        {
            Destroy(list[i]);

        }
    }
    void GenerateControlPoints()
    {
        int sections = (int)(raceTrackWidth / 12.5);
        for (int i = 0; i < sections; i++)
        {
            float x, y;
            cps.Add(Instantiate(raceTrackControlPoint));
            if (i % 2 == 0)
            {
                y = Random.Range(0.0f, raceTrackHeight / 2);
            }
            else
            {
                y = Random.Range(-raceTrackHeight / 2, 0.0f);
            }
            x = Random.Range(raceTrackWidth / 2 - 12.5f * (sections - i), raceTrackWidth / 2 - 12.5f * (sections - i) + 12.5f);
            cps[i].transform.position = new Vector2(x, y);
        }

        correctOrder.Add(cps[0]);
        for (int i = 1; i < cps.Count; i=i+2)
        {
            correctOrder.Add(cps[i]);
        }
        for (int i = cps.Count - 2 + cps.Count % 2; i > 0; i = i - 2)
        {
            correctOrder.Add(cps[i]);
        }
    }

    void drawLine(Vector3 start, Vector3 end)
    {
        drawLine(start, end, Color.white, true);
    }
    void drawLine(Vector3 start, Vector3 end, Color color, bool collide = false, bool wall = true)
    {
        List < Vector2 > list = new List<Vector2>();
        list.Add(new Vector3(0,0,0));
        list.Add(end-start);
        GameObject myLine = new GameObject();
        created.Add(myLine);
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        if (collide)
        {
            myLine.AddComponent<EdgeCollider2D>();
            EdgeCollider2D edgeCollider = myLine.GetComponent<EdgeCollider2D>();
            edgeCollider.SetPoints(list);
            edgeCollider.isTrigger = true;
            if(wall)
            {
                myLine.tag = "wall";
                myLine.layer = 6;
            }
            else
            {
                myLine.tag = "checkpoint";
            }
        }

        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.SetWidth(0.1f, 0.1f);
        lr.SetColors(color, color);
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }

    (List<GameObject>, Vector3) GeneratePolygonCollider(List<GameObject> controlPoints, bool newControls=false)
    {
        List<(Vector2, Vector2, bool)> lines = new List<(Vector2, Vector2, bool)>();

        for (int i=0; i<controlPoints.Count;i++) {
            if (i == controlPoints.Count - 1)
            {
                drawLine(controlPoints[i].transform.position, controlPoints[0].transform.position);
                lines.Add((controlPoints[i].transform.position, controlPoints[0].transform.position, false));

            }
            else
            {
                drawLine(controlPoints[i].transform.position, controlPoints[i + 1].transform.position);
                lines.Add((controlPoints[i].transform.position, controlPoints[i + 1].transform.position, i < controlPoints.Count / 2 + 1));
            }
        }

        List<GameObject> newControlpoints = new List<GameObject>();
        Vector3 start = new Vector3(0, 0, 0);
        if (newControls)
        {
            for (int i = 0; i < lines.Count; i++)
            {

                (Vector2 b, Vector2 c, bool isDown) = lines[i];
                Vector2 a;
                bool isDownSecond;
                if (i == 0)
                {
                    (a, _, isDownSecond) = lines[lines.Count - 1];
                }
                else
                {
                    (a, _, isDownSecond) = lines[i - 1];
                }


                Vector2 bc = c - b;
                bc = bc.normalized;
                Vector2 ba = a - b;
                ba = ba.normalized;
                Vector2 perpB;
                Color color;
                if (isDown && isDownSecond)
                {
                    color = new Color(255, 0, 0);
                    if ((b + bc + ba).y < b.y)
                    {
                        perpB = b + (bc + ba).normalized;
                    }
                    else
                    {
                        perpB = b - (bc + ba).normalized;
                    }
                }
                else if ((isDown && !isDownSecond) || (isDownSecond && !isDown))
                {
                    color = new Color(0, 255, 0);
                    perpB = b - (bc + ba).normalized;
                }
                else
                {
                    color = new Color(0, 0, 255);
                    if ((b + bc + ba).y > b.y)
                    {
                        perpB = b + (bc + ba).normalized;
                    }
                    else
                    {
                        perpB = b - (bc + ba).normalized;
                    }
                }
                //drawLine(b, perpB, color);



                Vector2 perpBt = perpB - b;

                Vector2 bcr = new Vector2(bc.y, -bc.x);
                Vector2 bar = new Vector2(-ba.y, ba.x);


                Vector2 newPosition = b + 0.01f * bar;
                while ((newPosition - b).magnitude < trackWidth)
                {
                    newPosition += 0.01f * bar;
                }
                if ((c - newPosition).magnitude > (c-b).magnitude)
                {
                    newControlpoints.Add(Instantiate(raceTrackControlPoint));
                    newControlpoints[newControlpoints.Count-1].transform.position = newPosition;
                }

                newPosition = b + 0.01f * perpBt;
                while ((newPosition - b).magnitude < trackWidth)
                {
                    newPosition += 0.01f * perpBt;
                }

                if (newControls)
                {
                    newControlpoints.Add(Instantiate(raceTrackControlPoint));
                    newControlpoints[newControlpoints.Count - 1].transform.position = newPosition;
                }


                newPosition = b + 0.01f * bcr;
                while ((newPosition - b).magnitude < trackWidth)
                {
                    newPosition += 0.01f * bcr;
                }

                if ((a - newPosition).magnitude > (a - b).magnitude)
                {
                    newControlpoints.Add(Instantiate(raceTrackControlPoint));
                    newControlpoints[newControlpoints.Count - 1].transform.position = newPosition;
                }

                Vector3 startPoint = (b - (b - c) / 2f);
                Vector3 endPoint = (b - (b - c) / 2f) + (newPosition - b);
                if (i == 0)
                {
                    drawLine(startPoint, endPoint, Color.red);
                    start = (endPoint - startPoint) * 0.5f + startPoint;
                    startPoint = (b - (b - c) / 3f);
                    endPoint = (b - (b - c) / 3f) + (newPosition - b);
                    drawLine(startPoint, endPoint, Color.green, true, false);
                    startPoint = (b - (b - c) / 3f * 2);
                    endPoint = (b - (b - c) / 3f * 2) + (newPosition - b);
                    drawLine(startPoint, endPoint, Color.green, true, false);
                }
                else
                {
                    startPoint = (b - (b - c) / 3f);
                    endPoint = (b - (b - c) / 3f) + (newPosition - b);
                    drawLine(startPoint, endPoint, Color.green, true, false);
                    startPoint = (b - (b - c) / 3f*2);
                    endPoint = (b - (b - c) / 3f*2) + (newPosition - b);
                    drawLine(startPoint, endPoint, Color.green, true, false);
                }


                //Vector2 perpBt = perpB - b;

                //drawLine(b, b + new Vector2(-perpBt.y, perpBt.x),Color.black);
            }
        }
        return (newControlpoints, start);
    }



}
