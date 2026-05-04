using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckerScript : MonoBehaviour
{
    public List<StopScript> stopScripts;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider c)
    {
        CarAIController controller = c.GetComponent<CarAIController>();

        if(controller)
        {
            // Debug.Log(
            //     $"Checker '{gameObject.name}' triggered by car '{controller.gameObject.name}' " +
            //     $"on layer '{LayerMask.LayerToName(c.gameObject.layer)}' " +
            //     $"at position {c.transform.position}"
            // );

            for(int i = 0; i < stopScripts.Count; i++)
                stopScripts[i].stop = true;
        }
    }

    void OnTriggerExit(Collider c)
    {
        CarAIController controller = c.GetComponent<CarAIController>();

        if(controller)
        {
            for(int i = 0; i < stopScripts.Count; i++)
                stopScripts[i].stop = false;
        }
    }
}
