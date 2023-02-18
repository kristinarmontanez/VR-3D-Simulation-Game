using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DrinkIngredientRequirements
{
    [SerializeField] private IngredientTemplate _ingredient;
    [SerializeField] private float _quantity;

    public IngredientTemplate Ingredient => _ingredient;
    public float Quantity => _quantity;
}


// Template class that defines the "golden" values associated with a drink.
// These are the values that should be used when comparing against a drink made
// by the user.  Again, this class uses the CreateAssetMenu attribute as it inherits
// from ScriptableObject.  This means we can create assets of this type to be stored in the 
// Asset Folder.
[CreateAssetMenu(menuName = "Create Drink", fileName = "Drink", order = 51)]
public class DrinkTemplate : ScriptableObject
{
    // Which type of container does this drink go in?
    [SerializeField] private ContainerTemplate _containerTemplate;
    // Which ingredients does it need?
    [SerializeField] private List<DrinkIngredientRequirements> _ingredientRequirements;
    // Does it need a garnish?
    [SerializeField] private bool _garnishNeeded;

    // Public Accessor for the container.
    public ContainerTemplate ContainerTemplate => _containerTemplate;
    // Public Accessor for the name.
    public string Name => name;
    // Public Accessor for if a garnish is needed..
    public bool GarnishNeeded => _garnishNeeded;

    // Does this Drink Recipe require the given ingredient? 
    public bool IngredientIsInDrink(IngredientTemplate ingredient)
    {
        // If the given ingredient is contained somewhere in the required list, return true.
        foreach(var requirement in _ingredientRequirements)
            if (requirement.Ingredient == ingredient)
                return true;

        // It wasn't found, return false.
        return false;
    }

    
    public DrinkIngredientRequirements FindRequirementEntryOfType(IngredientTemplate template)
    {
        // If the incoming ingredient isn't required, return null. 
        if (!IngredientIsInDrink(template)) return null;
        
        // Find the matching requirement.
        // This is O(n) but I'm not worried about the performance hit as we won't
        // have a very large number of ingredients per drink.
        foreach(var requirement in _ingredientRequirements)
            if (requirement.Ingredient == template)
                return requirement;

        return null;
    }

    public float GetQuantityForIngredient(IngredientTemplate ingredient)
    {
        // See if this drink needs the provided ingredient.
        var requirement = FindRequirementEntryOfType(ingredient);
        
        // If the above value is 'null', it doesn't, therefore return -1.
        if (requirement == null) return -1;

        // It is needed, so return the amount specified in the requirement.
        return requirement.Quantity;
    }
    
    // TODO
    public bool ValidateContainer()
    {
        return false;
    }
    
    // TODO
    public bool ValidateGarnish()
    {
        return false;
    }
    
    // TODO
    public bool ValidateIngredient()
    {
        return false;
    }

    // TODO
    public bool ValidateMixture()
    {
        return false;
    }
}