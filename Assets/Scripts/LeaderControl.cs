using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderControl : MonoBehaviour
{
    //Leader Attributes
    private Vector3 targetPosition;
    Vector3 lookAtTarget;
    Quaternion leaderRot;
    [Range(0, 40)]
    public float speed = 30f;
    [Range(0, 20)]
    public float rotSpeed = 10f;
    public bool moving = false;
    //public List<Vector3> movePositionList;
    public Collider[] unitsInsideArea;
    int layerMask;
    public float arrivalRadius = 1;
    public float separationRadius_min = 2;

    public float unitSize;
    public float unitSpace;
    public float allUnitsSpaces;
    public float arrivalSpace;

    // Start is called before the first frame update
    void Start()
    {
        unitSize = 1;
        unitSpace = (unitSize + separationRadius_min / 2)* (unitSize + separationRadius_min / 2);
        layerMask= 1<< 8;
    }

    // Update is called once per frame
    void Update()
    {
        //unitSize of cube=1
        //unitSpace = (unitSize + separaration_radius_min/2)^2
        //arrivalSpace = Pi*arrivalRadius^2
        
        if (Input.GetMouseButton(1))
        {
            SetTargetPosition();
        }
        if (moving)
        {
            arrivalRadius = 1;
            Move();
        }
        else
        {
            unitsInsideArea = Physics.OverlapSphere(transform.position, arrivalRadius, layerMask);
            allUnitsSpaces = unitSpace * unitsInsideArea.Length;
            arrivalSpace = Mathf.PI * (arrivalRadius * arrivalRadius);

            if (allUnitsSpaces >= (arrivalSpace-(unitsInsideArea.Length*2)))
            {
                arrivalRadius+=5;
            }

        }


        Debug.DrawRay(transform.position, transform.forward * (arrivalRadius), Color.red);
        Debug.DrawRay(transform.position, -transform.forward * (arrivalRadius), Color.red);
        Debug.DrawRay(transform.position, transform.right * (arrivalRadius), Color.red);
        Debug.DrawRay(transform.position, -transform.right * (arrivalRadius), Color.red);
        

    }

    public void SetTargetPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit rayHit;

        if (Physics.Raycast(ray, out rayHit, Mathf.Infinity))
        {
            targetPosition = new Vector3(rayHit.point.x,rayHit.point.y+0.5f,rayHit.point.z);
            lookAtTarget = new Vector3(targetPosition.x - transform.position.x, transform.position.y, targetPosition.z - transform.position.z);
            leaderRot = Quaternion.LookRotation(lookAtTarget);
            moving = true;
        }
    }

    void Move()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, leaderRot, rotSpeed * Time.deltaTime);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
        {
            moving = false;
        }
    }


}
