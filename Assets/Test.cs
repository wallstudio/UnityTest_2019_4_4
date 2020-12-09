using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        DynamicTest();
    }

    void Execute()
    {
        Debug.Log("ExecuteLog!");
    }

    public object DynamicTest()
    {
        dynamic value = (this);
        value.Execute(); 
        return value;
    }
}
