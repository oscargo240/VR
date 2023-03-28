using UnityEngine;
using System.Collections;
using static UnityEditor.PlayerSettings;

public class FlyCamera : MonoBehaviour
{

    /*
    Writen by Windexglow 11-13-10.  Use it, edit it, steal it I don't care.  
    Converted to C# 27-02-13 - no credit wanted.
    Simple flycam I made, since I couldn't find any others made public.  
    Made simple to use (drag and drop, done) for regular keyboard layout  
    wasd : basic movement
    shift : Makes camera accelerate
    space : Moves camera on X and Z axis only.  So camera doesn't gain any height*/
    bool isHolding = false; // Is the player holding a ball?
    GameObject item = null; // Ball GameObject
    float throwForce = 30.0f; // Throwing force
    Vector3 objectPos; // Ball position
    float distance; // Distance between player and ball


    float mainSpeed = 10.0f; //regular speed
    float shiftAdd = 250.0f; //multiplied by how long shift is held.  Basically running
    float maxShift = 1000.0f; //Maximum speed when holdin gshift
    public float camSens = 1f; //How sensitive it with mouse
    private Vector3 lastMouse = new Vector3(255, 255, 255); //kind of in the middle of the screen, rather than at the top (play)
    private float totalRun = 1.0f;

    private float sinceUpdate = 0.2f;
    private bool activeCircle = false;
    public GameObject RedCircle; // Item to be spawned

    void Update()
    {

        // Catch the ball
        if (item != null)
        {
            distance = Vector3.Distance(item.transform.position,
            transform.position);
            if (distance >= 1.5f)
            {
                isHolding = false;
            }
            if (isHolding == true)
            {
                item.GetComponent<Rigidbody>().velocity = Vector3.zero;
                item.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                // The CameraParent becomes the parent of the ball too.
                item.transform.SetParent(transform);
                if (Input.GetKey("z"))
                {
                    // Throw
                    var cam = Camera.main;
                    item.GetComponent<Rigidbody>().AddForce(
                                    cam.transform.forward * throwForce);
                    isHolding = false;

                }
            }
            else
            {
                objectPos = item.transform.position;
                item.transform.SetParent(null);
                item.GetComponent<Rigidbody>().useGravity = true;
                item.transform.position = objectPos;
            }
        }

        //teleport
        RaycastHit hit;
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        // Get the position and direction of the camera
        Vector3 cameraPosition = Camera.main.transform.position;
        Vector3 cameraDirection = Camera.main.transform.forward;

        // Draw a ray that comes from the camera
        Debug.DrawRay(cameraPosition, cameraDirection * 10f, Color.green);


        // Do something with the object that was hit by the raycast
        sinceUpdate -= Time.deltaTime;
        if (Input.GetKey("q") && sinceUpdate < 0.0f)
        {
            sinceUpdate = 0.2f;
            if (activeCircle == false)
            {
                //dibujar la pelota
                Vector3 pos = Camera.main.transform.position + cameraDirection;
                pos.y = 0;
                Instantiate(RedCircle, pos, Quaternion.identity);
                activeCircle = true;
            }
            else
            {
                //recuperar la pelota con la etiqueta "RedCircle"
                GameObject redCircleToDestroy = GameObject.FindGameObjectWithTag("RedCircle");
                Destroy(redCircleToDestroy);
                activeCircle = false;
                Vector3 newPosition = transform.position;
                newPosition.x += cameraDirection.x;
                newPosition.z += cameraDirection.z;
                transform.position = newPosition;
            }
        }

        if (activeCircle)
        {
            GameObject redCircleToDestroy = GameObject.FindGameObjectWithTag("RedCircle");
            Vector3 pos = Camera.main.transform.position + cameraDirection;
            pos.y = 0;
            redCircleToDestroy.transform.position = pos;
        }
        

        //movement
        lastMouse = Input.mousePosition - lastMouse;
        lastMouse = new Vector3(-lastMouse.y * camSens, lastMouse.x * camSens, 0);
        lastMouse = new Vector3(transform.eulerAngles.x + lastMouse.x, transform.eulerAngles.y + lastMouse.y, 0);

        var direction = new Vector3(-Input.GetAxis("Mouse Y"),
            Input.GetAxis("Mouse X"), 0.0f);

        var euler = transform.eulerAngles + direction * camSens;
        transform.eulerAngles = euler;
        lastMouse = Input.mousePosition;
        //Mouse  camera angle done.  

        //Keyboard commands
        float f = 0.0f;
        Vector3 p = GetBaseInput();
        if (p.sqrMagnitude > 0)
        { // only move while a direction key is pressed
            if (Input.GetKey(KeyCode.LeftShift))
            {
                totalRun += Time.deltaTime;
                p = p * totalRun * shiftAdd;
                p.x = Mathf.Clamp(p.x, -maxShift, maxShift);
                p.y = Mathf.Clamp(p.y, -maxShift, maxShift);
                p.z = Mathf.Clamp(p.z, -maxShift, maxShift);
            }
            else
            {
                totalRun = Mathf.Clamp(totalRun * 0.5f, 1f, 1000f);
                p = p * mainSpeed;
            }

            p = p * Time.deltaTime;
            Vector3 newPosition = transform.position;
            if (Input.GetKey(KeyCode.Space))
            { //If player wants to move on X and Z axis only
                transform.Translate(p);
            }
            else
            {
                transform.Translate(p);
                newPosition.x = transform.position.x;
                newPosition.z = transform.position.z;
                transform.position = newPosition;
            }
        }
    }

    void OnCollisionEnter(Collision obj)
    {
        if (obj.gameObject.tag == "Spheres")
        {
            item = obj.gameObject;
            item.transform.position = Camera.main.transform.position
            + Camera.main.transform.forward * 0.75f;
            isHolding = true;
            item.GetComponent<Rigidbody>().useGravity = false;
            item.GetComponent<Rigidbody>().detectCollisions = true;
        }
    }


    private Vector3 GetBaseInput()
    { //returns the basic values, if it's 0 than it's not active.
        Vector3 p_Velocity = new Vector3();
        if (Input.GetKey(KeyCode.W))
        {
            p_Velocity += new Vector3(0, 0, 1);
        }
        if (Input.GetKey(KeyCode.S))
        {
            p_Velocity += new Vector3(0, 0, -1);
        }
        if (Input.GetKey(KeyCode.A))
        {
            p_Velocity += new Vector3(-1, 0, 0);
        }
        if (Input.GetKey(KeyCode.D))
        {
            p_Velocity += new Vector3(1, 0, 0);
        }
        return p_Velocity;
    }
}