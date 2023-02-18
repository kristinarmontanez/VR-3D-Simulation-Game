using System.Collections.Generic;
using UnityEngine;


// The 'hub', or managerial class, designed to manage all work done on a drink.
// The drink template, DrinkObject, is created by the SimulationManager who carries
// out all of the detection, reporting any changes here.  The changes made to drinks
// will be judged/scored/processed here.
public class SimulationManager : MonoBehaviour
{
    // The template prefab object we'll instantiate everytime a new drink is to be made.
    [SerializeField] private DrinkObject _drinkObject;

    // The golden value template, or "the right answer", for how to make the current drink selected
    // by the user.
    private DrinkTemplate _goldenValueTemplate;
    
    // The active DrinkObject, or the object that is currently responsible for handling and reporting
    // changes made to the drink that the user is creating.
    private DrinkObject _activeDrinkObject;

    
    // Performs all initialization required at the time of recipe selection.
    // This currently includes storing the golden value, or "the right answer", which will
    // be later accessed when scoring changes made to the drink.
    // This also includes the instantiation of a new template everytime a new recipe starts.
    // This also includes subscribing, or listening, to the DrinkObjects event 'OnGarnishAddedToDrinkObject'.
    // The function that listens to this event is 'OnGarnishAddedToDrinkObject'.  This is where validation occurs based
    // on the correct recipe.
    // Once all initialization is done by the SimulationManager, the DrinkObject is told to initialize all of it's 
    // components - instantiating any GameObjects (models), loading any effects, or calling the initialization of its
    // own collision/detection handlers.
    public void InitializeSimulationOnRecipeSelected(DrinkTemplate drinkTemplate)
    {
        _goldenValueTemplate = drinkTemplate;

        _activeDrinkObject = Instantiate(_drinkObject, transform);
        _activeDrinkObject.OnGarnishAdded += OnGarnishAddedToDrinkObject;
        _activeDrinkObject.OnFluidChanged += OnDrinkObjectLiquidChanged;
        
        _activeDrinkObject.InitializeDrinkObject(drinkTemplate.ContainerTemplate);
    }

    private void OnDrinkObjectLiquidChanged(Dictionary<FluidIngredientTemplate, float> contents)
    {
        Debug.Log("I was told that the Drink Objects fluid contents changed!");
    }

    // Validation of garnish changes done here.
    private void OnGarnishAddedToDrinkObject(GarnishObject garnish, int count)
    {
        Debug.Log($"I was told that the Drink Object got hit by this: {garnish} - and this many {count}!");
    }
}


//TODO
public class PourTracker : MonoBehaviour
{
    private readonly Dictionary<ContainerTemplate, float> _containerTracker = new Dictionary<ContainerTemplate, float>();
    
    public void OnContainerFluidVolumeChanged(ContainerTemplate containerTemplate, float amount)
    {
        if (_containerTracker.ContainsKey(containerTemplate))
            _containerTracker[containerTemplate] += amount;
    }
}