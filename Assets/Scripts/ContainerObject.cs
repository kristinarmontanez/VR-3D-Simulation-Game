using System;
using UnityEngine;

// We're using the function OnCollisionEnter, therefore we need to have a collider.
[RequireComponent(typeof(Collider))]
public class ContainerObject : MonoBehaviour
{
    // Action is a delegate - meaning it delegates work for handling some kind of functionality.
    // Inside the brackets <> indicates what parameters are to be used for a listener of the given event.
    // For this example, any function that listens to this event Action, must have the same function signature 
    // as: void (GarnishObject, int);
    // Actions by default return void, Func<T> on the other hand, returns a value other than void.
    // For example, Func<GarnishObject, int> requires a listener to have a signature of the following type:
    // GarnishObject (int);  Meaning, the function returns a GarnishObject and accepts an int.
    public event Action<GarnishObject, int> OnGarnishAdded;

    
    // OnCollisionEnter is a built-in Monobehaviour function which works by listening for collision events on 
    // objects.  Whenever this function is implemented, it's good to require that the object has a collider.
    private void OnCollisionEnter(Collision other)
    {
        // Does the incoming collision object have the component GarnishObject?
        var garnish = other.gameObject.GetComponent<GarnishObject>();
        
        // If it doesn't, there's nothing for us to do.
        if (garnish == null) return;
        
        // If it does, notify any listeners know which GarnishObject we hit and how many of them hit us.
        OnGarnishAdded?.Invoke(garnish, 1);
    }
}