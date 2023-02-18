using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(FluidContainerContentsManager), typeof(FluidPourHandler))]
public class FluidObject : MonoBehaviour
{
    public event Action<Dictionary<FluidIngredientTemplate, float>> OnFluidChanged;

    [SerializeField] private FluidIngredientTemplate _defaultFluid;
    [Range(0, 1)]
    [SerializeField] private float _defaultFillPercent;
    [Range(0, 1)]
    [SerializeField] private float _currentPercentFill = 0.0f;
    
    // Exposed in Inspector for debugging purposes.
    [Tooltip("This is exposed for debugging purposes, don't set this in the inspector.")]
    [SerializeField] private Color _currentTopColor = Color.clear;
    [SerializeField] private Color _currentSideColor = Color.clear;
    
    private Dictionary<FluidIngredientTemplate, float> _currentContents = new Dictionary<FluidIngredientTemplate, float>();
    
    private FluidContainerContentsManager _containerContentsManager;
    public Color SideColor => _currentSideColor;
    public Color TopColor => _currentTopColor;

    private void OnValidate()
    {
        _containerContentsManager = GetComponent<FluidContainerContentsManager>();
        
        if (Mathf.Abs(_defaultFillPercent - _currentPercentFill) > Mathf.Epsilon)
        {
            _currentPercentFill = _defaultFillPercent;
            _containerContentsManager.SetFillPercent(_currentPercentFill);
        }

        SetColorsFromTemplate();
        
        _containerContentsManager.SetColors(_currentTopColor, _currentSideColor);
    }

    private void SetColorsFromTemplate()
    {
        if (_defaultFluid != null && _currentTopColor != _defaultFluid.TopColor)
            _currentTopColor = _defaultFluid.TopColor;
        
        if (_defaultFluid != null && _currentSideColor != _defaultFluid.SideColor)
            _currentSideColor = _defaultFluid.SideColor;
    }

    private void Awake()
    {
        _containerContentsManager = GetComponent<FluidContainerContentsManager>();
        
        SetColorsFromTemplate();
        if (_defaultFluid != null) 
            _currentContents.Add(_defaultFluid, _defaultFillPercent);
    }

    private void Start()
    {
        _currentTopColor = _defaultFluid == null ? _currentTopColor : _defaultFluid.TopColor;
        _currentSideColor = _defaultFluid == null ? _currentSideColor : _defaultFluid.SideColor;
        _currentPercentFill = _defaultFillPercent;
        
        _containerContentsManager.SetColors(_currentTopColor, _currentSideColor);
        _containerContentsManager.SetFillPercent(_currentPercentFill);
        _containerContentsManager.OnFluidContentsChanged += OnFluidContentsChanged;
    }

    private void OnDestroy()
    {
        _containerContentsManager.OnFluidContentsChanged -= OnFluidContentsChanged;
    }

    private void Update()
    {
        _containerContentsManager.Tick(_currentContents);
    }
    
    private void SumContents(Dictionary<FluidIngredientTemplate, float> incomingContents, float changeAmount)
    {
        foreach(var fluid in incomingContents)
        {
            _currentSideColor = Color.Lerp(_currentSideColor, fluid.Key.SideColor, 0.5f);
            _currentTopColor = Color.Lerp(_currentTopColor, fluid.Key.TopColor, 0.5f);
        }
    }

    private void OnFluidContentsChanged(float newVolumePercent, Dictionary<FluidIngredientTemplate, float> incomingRatios, float changeAmount)
    {
        if (Mathf.Sign(changeAmount) > 0)
            SumContents(incomingRatios, changeAmount);

        // Update the colors and fill percent of the manager so they can change the graphics.
        _currentPercentFill = newVolumePercent;
        _containerContentsManager.SetColors(_currentTopColor, _currentSideColor);
        _containerContentsManager.SetFillPercent(_currentPercentFill);
        
        // Notify any listeners of the change.
        OnFluidChanged?.Invoke(_currentContents);
    }

    public void SetFluidLevels(float percent)
    {
        _currentPercentFill = percent;
        _containerContentsManager.SetFillPercent(_currentPercentFill);
    }

    public void ClearContents()
    {
        _currentContents = new Dictionary<FluidIngredientTemplate, float>();
        _currentTopColor = Color.clear;
        _currentSideColor = Color.clear;
    }

    public void SetColors(Color sideColor, Color topColor)
    {
        _currentSideColor = sideColor;
        _currentTopColor = topColor;
        _containerContentsManager.SetColors(_currentTopColor, _currentSideColor);
    }
}