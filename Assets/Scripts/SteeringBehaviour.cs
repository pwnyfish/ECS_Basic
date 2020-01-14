using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringBehaviour : MonoBehaviour
{

    //currently unused variables
    private bool moveTo = false;
    private RaycastHit mouseHit;


    SteeringManager steeringManager;
    LeaderControl leader;
    public float maxSteeringForce = 1;
    public Rigidbody rigidbody;
    public GameObject[] unitList;

    public float separationBuildUp;
    public float arrivalSlowDown;
    private Boolean arrived = false;

    private List<Vector3> behaviourForceList = new List<Vector3>();



    //Customisable Behaviour Settings
    private float see_ahead = 15;
    private float max_speed = 15;
    private float sep_speed = 10;
    private float max_avoidance = 20;
    private float separation_radius_min = 2;
    private float separation_radius_max = 10;
    private float alignment_radius = 4;
    
    //unitSize of cube=1
    //unitSpace = (unitSize + separaration_radius_min/2)^2
    //arrivalSpace = Pi*arrivalRadius^2
 

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        steeringManager = GameObject.FindObjectOfType(typeof(SteeringManager)) as SteeringManager;
        leader = GameObject.FindObjectOfType(typeof(LeaderControl)) as LeaderControl;
        unitList = steeringManager.unitList;
        
    }

    // Update is called once per frame
    void Update()
    {
        //smooth out the intial seperation force with a wind up time
        separationBuildUp += Time.deltaTime;
        separationBuildUp =  Mathf.Clamp(separationBuildUp, 0f, 1f);

        if (Input.GetMouseButtonDown(1))
        {
            moveTo = true;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            Debug.DrawRay(ray.origin, mouseHit.point, Color.red, 0.5f);

            if (Physics.Raycast(ray, out mouseHit, Mathf.Infinity))
            {
                //target = mouseHit.point;
                //does nothing atm
            }
        }
    }

    private void FixedUpdate()
    {
        if (Vector3.Distance(leader.transform.position, transform.position) < leader.arrivalRadius)
        {
            arrived = true;
        }
        else
        {
            arrived = false;
        }

        if (!arrived)
        {
            behaviourForceList.Add(doArrival(leader.transform.position, leader.arrivalRadius, max_speed));
        }
        behaviourForceList.Add(doSeparation(separation_radius_min, separation_radius_max, sep_speed,max_speed));
        //behaviourForceList.Add(doAlignment(alignment_radius, max_speed));
        //behaviourForceList.Add(doAvoidance(see_ahead, max_avoidance));
        ApplySteering(behaviourForceList);

        behaviourForceList.Clear();

    }
    void OnDrawGizmos()
    {

    }


    private Vector3 doArrival(Vector3 target, float slowingRadius, float speed)
    {
        Vector3 averagePosition = Vector3.zero;
        Vector3 steering;
        // Calculate the desired velocity
        Vector3 desired_velocity;
        desired_velocity = target - transform.position;
        
        float distance = Vector3.Magnitude(desired_velocity);

        // Check the distance to detect whether the character
        // is inside the slowing area
        if (distance < slowingRadius)
        {
            // Inside the slowing area
            desired_velocity = desired_velocity.normalized * speed * (distance / slowingRadius);
        }
        else
        {
            // Outside the slowing area.
            desired_velocity = desired_velocity.normalized * speed;
        }
        // Set the steering based on this
        steering=desired_velocity - rigidbody.velocity;
        return steering;
    }


    private Vector3 doSeparation(float separation_radius_min, float separation_radius_max, float sepSpeed,float max_velocity)
    {
        float velocity_magnitude = rigidbody.velocity.magnitude / max_velocity;
        float separation_radius = Mathf.Lerp(separation_radius_min, separation_radius_max, velocity_magnitude);

        Vector3 steering;
        Vector3 desired_velocity = Vector3.zero;

        List<GameObject> crowd = new List<GameObject>(unitList);
        List<GameObject> crowdNeighbors = new List<GameObject>();
        float sqrDist = 0;

        Debug.DrawRay(transform.position, transform.forward * (separation_radius / 2), Color.green);
        Debug.DrawRay(transform.position, -transform.forward * (separation_radius / 2), Color.green);
        Debug.DrawRay(transform.position, transform.right * (separation_radius / 2), Color.green);
        Debug.DrawRay(transform.position, -transform.right * (separation_radius / 2), Color.green);

        // Get all crow units in neighbor hood
        for (int i = 0; i < crowd.Count; i++)
        {
            GameObject crowdUnit = crowd[i];

            if (crowdUnit != null)
            {
                if (crowdUnit.gameObject != this.gameObject)
                {
                    sqrDist = (crowdUnit.transform.position - transform.position).sqrMagnitude;

                    if (sqrDist < separation_radius * separation_radius)
                    {
                        crowdNeighbors.Add(crowdUnit);
                    }
                }
            }
        }

        if (crowdNeighbors.Count < 1)
        {
            separationBuildUp = 0;
            return Vector3.zero;
        }

        Vector3 force;
        // Calculate separation from neighbor hood
        for (int i = 0; i < crowdNeighbors.Count; i++)
        {
            force = transform.position - crowdNeighbors[i].transform.position;
            force *= 1 - Mathf.Min(force.sqrMagnitude / (separation_radius * separation_radius), 1);
            desired_velocity += force;
        }
        
        desired_velocity /= crowdNeighbors.Count;

        desired_velocity = desired_velocity.normalized * sepSpeed*separationBuildUp;
        steering = desired_velocity - rigidbody.velocity;
        return steering;
    }

    private Vector3 doAlignment(float alignment_radius, float speed)
    {
        Vector3 steering;
        Vector3 desired_velocity = Vector3.zero;

        int neighborCount = 0;

        foreach (GameObject unit in unitList)
        {
            if (unit != null)
            {
                if (unit != this && Vector3.Distance(unit.transform.position, this.transform.position) < alignment_radius)
                {

                    //desired_velocity += unit.GetComponent<Rigidbody>().velocity;
                    desired_velocity += unit.transform.forward;
                    neighborCount++;
                }
            }
        }

        if (neighborCount != 0)
        {
            desired_velocity /= neighborCount;
        }

        desired_velocity = desired_velocity.normalized * speed;
        steering = desired_velocity - rigidbody.velocity;
        return steering;
    }


    private Vector3 doAvoidance(float see_ahead, float max_force)
    {
        Transform mostThreatening = null;
        //SphereCollider collider;
        RaycastHit hitAhead;
        Vector3 avoidanceForce = Vector3.zero;
        Vector3 ahead;
        Vector3 ahead2;
        ahead = transform.position + rigidbody.velocity.normalized * see_ahead;
        ahead2 = transform.position + rigidbody.velocity.normalized * see_ahead * 0.5f;
        int layerMask = 1 << 8;
        layerMask = ~layerMask;
        //avoidanceForce=v_ahead - obstacle_center

        if (Physics.Raycast(transform.position, transform.forward, out hitAhead, ahead.magnitude, layerMask))
        {
            //hit an obstacle
            Debug.Log("hit obstacle");
            mostThreatening = hitAhead.transform;
            Debug.DrawLine(transform.position, hitAhead.point, Color.red);
        }
        else
        {
            Debug.Log("no obstacle hit");
        }

        if (mostThreatening != null)
        {
            if (mostThreatening.tag == "Obstacle")
            {
                Debug.Log(mostThreatening.tag);

                avoidanceForce = ahead - mostThreatening.position;
                avoidanceForce = avoidanceForce.normalized * max_force;
                //collider = mostThreatening.GetComponent<SphereCollider>();
            }

        }
        else
        {
            avoidanceForce = Vector3.zero; // nullify the avoidance force
        }

        return avoidanceForce;
    }

    private void ApplySteering(List<Vector3> behaviourForceList)
    {
        // Get steering force average
        Vector3 steeringForceAverage = Vector3.zero;

        foreach (Vector3 force in behaviourForceList)
        {
            steeringForceAverage += force;
        }

        steeringForceAverage.y = 0;
        steeringForceAverage = Vector3.ClampMagnitude(steeringForceAverage, maxSteeringForce);
        rigidbody.velocity += steeringForceAverage;

        Debug.DrawRay(transform.position, rigidbody.velocity, Color.blue);

        // Update rotation
        if (rigidbody.velocity.sqrMagnitude > 1)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(rigidbody.velocity), Time.deltaTime * 5);
        }
    }


}


