using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Car : MonoBehaviour, ICandidate
{
    
    UnityEvent carSpawned;
    RaceTrackManager rm;
    Rigidbody2D rb;
    float forwardSpeed = 0;
    float maxSpeed = 20;
    float acceleration = 5f;
    float lookAhead = 15;
    public int points = 0;
    float rotationSpeed = 100;
    int notMoved = 0;
    NeuralNetwork nn;
    List<GameObject> collisions = new List<GameObject>();
    public double[] Gene { get; set; }
    public bool Dead { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void rotateLeft()
    {
        rb.SetRotation(rb.rotation + rotationSpeed * Time.deltaTime);
    }

    void rotateRight()
    {
        rb.SetRotation(rb.rotation - rotationSpeed * Time.deltaTime);
    }
    void accelerate()
    {
        if(forwardSpeed>=maxSpeed)
        {
            forwardSpeed = maxSpeed;
            return;
        }
        forwardSpeed += acceleration * Time.deltaTime;
    }
    void decelerate()
    {
        if (forwardSpeed <= 0)
        {
            notMoved += 1;
            forwardSpeed = 0;
            return;
        }
        notMoved = 0;
        forwardSpeed -= 2* acceleration * Time.deltaTime;
    }
    (float front, float left, float right) checkSensors()
    {

        float left = 1;
        float right = 1;
        float front = 1;

        Vector2 point = rb.GetRelativePoint(new Vector2(0, forwardSpeed * Time.deltaTime));
        Vector2 forwardSensor = rb.GetRelativePoint(new Vector2(0, lookAhead));
        Vector2 leftSensor = rb.GetRelativePoint(new Vector2(-lookAhead/1.5f, lookAhead));
        Vector2 rightSensor = rb.GetRelativePoint(new Vector2(lookAhead/1.5f, lookAhead));
        forwardSensor = forwardSensor - (Vector2)transform.position;
        leftSensor = leftSensor - (Vector2)transform.position;
        rightSensor = rightSensor - (Vector2)transform.position;


        transform.position = point;
        RaycastHit2D hitFront = Physics2D.Raycast(transform.position, forwardSensor, lookAhead, 1 << 6);
        if (hitFront)
        {
            forwardSensor = (hitFront.point - (Vector2)transform.position);
            front = forwardSensor.magnitude/ lookAhead;
        }
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, leftSensor, lookAhead, 1 << 6);
        if (hitLeft)
        {
            leftSensor = (hitLeft.point - (Vector2)transform.position);
            left = leftSensor.magnitude/ lookAhead;
        }
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, rightSensor, lookAhead, 1 << 6);
        if (hitRight)
        {
            rightSensor = (hitRight.point - (Vector2)transform.position);
            right = rightSensor.magnitude / lookAhead;
        }
        Debug.DrawRay(transform.position, leftSensor, Color.red);
        Debug.DrawRay(transform.position, forwardSensor, Color.red);
        Debug.DrawRay(transform.position, rightSensor, Color.red);

        return (front, left, right);
    }
    void manualControl()
    {
        if (Input.GetKey("up"))
        {
            accelerate();
        }
        if (Input.GetKey("right"))
        {
            rotateRight();
        }
        if (Input.GetKey("left"))
        {
            rotateLeft();
        }
        if (Input.GetKey("down"))
        {
            decelerate();
        }
    }
    void Update()
    {
        if(Dead)
        {
            return;
        }
        manualControl();
        (float front, float left, float right) = checkSensors();

        double[] input = new double[] { front, left, right , forwardSpeed/maxSpeed };
        double[] output = nn.ForwardPass(input);

        double acc = output[0];
        double dec = output[1];
        double turnL = output[2];
        double turnR = output[3];
        if(acc > 0.5 && acc > dec)
        {
            notMoved = 0;
            accelerate();
        } else if(dec > 0.5 && dec > acc)
        {
            decelerate();
        }
        else
        {
            if(forwardSpeed==0)
            {
                notMoved += 1;
            }

        }
        if (notMoved > 20)
        {
            Dead = true;
        }
        if (turnL > 0.5 && turnL > turnR)
        {
            rotateLeft();
        } else if(turnR > 0.5 && turnR > turnL)
        {
            rotateRight();
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "checkpoint")
        {
            if(collisions.Contains(collision.gameObject))
            {
                Dead = true;
            }
            collisions.Add(collision.gameObject);
            points += 1;
        }
        else
        {
            Dead = true;
        }
    }

    public float EvaluateFitness()
    {
        return points;
    }

    public void InitCandidate(double[] gene)
    {
        Dead = false;
        rm = GameObject.Find("RaceTrackManager").GetComponent<RaceTrackManager>();
        gameObject.transform.position = rm.start;
        nn = new NeuralNetwork(4, 1, 6, 4);
        if (gene == null)
        {
            nn.BuildLayers();
            Gene = nn.Weights;
        }
        else
        {
            Gene = gene;
            nn.SetWeights(Gene);
            nn.BuildLayers();
        }
    }

    public GameObject getGameObject()
    {
        return gameObject;
    }
}
