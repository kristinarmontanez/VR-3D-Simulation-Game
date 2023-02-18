using System.Collections.Generic;
using UnityEngine;

public interface IFluidContainer
{
    float FillHeightPosition { get; }
    void ModifyFluidContents(float amountToChange, Dictionary<FluidIngredientTemplate, float> fluidRatiosToAdd);
}