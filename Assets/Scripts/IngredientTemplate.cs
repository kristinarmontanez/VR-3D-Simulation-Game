using UnityEngine;


public abstract class IngredientTemplate : ScriptableObject
{
    // A public, readonly reference to the name of this container.
    // 'name' is the name of this asset in the asset folder.
    public string Name => name;
}