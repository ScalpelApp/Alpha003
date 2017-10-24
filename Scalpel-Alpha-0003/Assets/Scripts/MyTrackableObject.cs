using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class MyTrackableObject : MonoBehaviour {

    private string mName;
    private int state;

    public MyTrackableObject( string _name, int _state )
    {
        this.mName = _name;
        this.state = _state;
    }

    public int State
    {
        get { return this.state; }
        set { this.state = value; }
    }
}
