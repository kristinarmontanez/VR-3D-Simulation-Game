using System;
using System.Collections.Generic;
using UnityEngine;

public class FluidContainerContentsManager : MonoBehaviour, IFluidContainer
{
    // Shader property IDs
    private static readonly int FillPercentID = Shader.PropertyToID("_FillPercent");
    private static readonly int SideColorID = Shader.PropertyToID("_SideColor");
    private static readonly int TopColorID = Shader.PropertyToID("_TopColor");

    public event Action<float, Dictionary<FluidIngredientTemplate, float>, float> OnFluidContentsChanged;
    
    [Header("Debug Toggles")]
    [SerializeField] private bool _renderDebug;
    [SerializeField] private bool _continuousFill;

    [Space(5)]
    [Header("Child Fluid Renderer")]
    [SerializeField] private MeshRenderer _fluidRenderer;
    
    [Space(5)]
    [Header("Fluid Parameters")]
    [SerializeField] private Transform _pourPoint;
    [SerializeField] private float _pourPointRadius;
    [SerializeField] private float _minPourSpeed;
    [SerializeField] private float _maxPourSpeed;
    
    // Private parameters handling various aspects of the positioning and behavior of the fluid.
    public float FillPercent => _fillPercent;
    private float _fillPercent;
    private float _currentPourSpeed;
    public float FillHeightPosition => _fillHeightPosition.y;
    private float _fillHeight;
    private Vector3 _fillHeightPosition;

    // Required Components
    private Material _fluidMaterial;
    private FluidPourHandler _fluidPourHandler;


    private void ValidateComponents(bool isEditor)
    {
        _fluidPourHandler ??= GetComponent<FluidPourHandler>();
        _fluidMaterial = isEditor ? _fluidRenderer.sharedMaterial : _fluidRenderer.material;
    }
    
    private void OnValidate()
    {
        ValidateComponents(Application.isEditor);
        _fluidPourHandler.SetGradientColors(_fluidMaterial.GetColor(SideColorID), _fluidMaterial.GetColor(TopColorID));
        _fluidPourHandler.SetGradientWidth(_pourPointRadius, _pourPointRadius / 2.0f);
    }

    private void Awake()
    {
        _fluidPourHandler = GetComponent<FluidPourHandler>();
        _fluidMaterial = _fluidRenderer.material;
    }
    
        
    public void Tick(Dictionary<FluidIngredientTemplate, float> currentContents)
    {
        if (_fluidMaterial == null) return;

        if (_continuousFill)
            _fillPercent = 1.0f;
        
        TickPour(currentContents);
    }
    
    public void ModifyFluidContents(float amountToChange, Dictionary<FluidIngredientTemplate, float> fluidRatiosToAdd)
    {
        _fillPercent = Mathf.Clamp01(_fillPercent + amountToChange);
        OnFluidContentsChanged?.Invoke(_fillPercent, fluidRatiosToAdd, amountToChange);
    }

    public void SetColors(Color topColor, Color sideColor)
    {
        ValidateComponents(Application.isEditor);
        
        _fluidMaterial.SetColor(TopColorID, topColor);
        _fluidMaterial.SetColor(SideColorID, sideColor);
        _fluidPourHandler.SetGradientColors(sideColor, topColor);
        _fluidPourHandler.SetGradientWidth(_pourPointRadius, _pourPointRadius / 2.0f);
    }

    public void SetFillPercent(float newVolumePercent)
    {
        ValidateComponents(Application.isEditor);
        
        _fillPercent = newVolumePercent;
        _fluidMaterial.SetFloat(FillPercentID, newVolumePercent);
    }
    
    private void TickPour(Dictionary<FluidIngredientTemplate, float> currentContents)
    {
        _fluidPourHandler.SetGradientColors(_fluidMaterial.GetColor(SideColorID), _fluidMaterial.GetColor(TopColorID));
        _fluidPourHandler.SetGradientWidth(_pourPointRadius, _pourPointRadius / 2.0f);
        // Update the materials property for fill percent.
        var currentFill = _fluidMaterial.GetFloat(FillPercentID);
        if (Mathf.Abs(currentFill - _fillPercent) > Mathf.Epsilon)
            _fluidMaterial.SetFloat(FillPercentID, _fillPercent);

        // Remap the fill percent to the size of the bounds.
        _fillHeight = MathUtility.Remap(_fillPercent, 0.0f, 1.0f, -transform.localScale.y, transform.localScale.y);
        // Determine the fill position relative to the transform.
        _fillHeightPosition = new Vector3(transform.position.x, transform.position.y + _fillHeight, transform.position.z);
        // What's the angle of our pour?  The higher the angle, the greater the value of pourT.
        var pourT = Mathf.Abs(Vector3.Dot((_pourPoint.position - _fillHeightPosition).normalized, Vector3.up));
        
        // Calculate pour speed relative to angle.
        _currentPourSpeed = Mathf.Lerp(_minPourSpeed, _maxPourSpeed, pourT);
        
        // Given our current pour speed and fill percent, how long will it take to drain?
        var drainTime = _fillPercent / _currentPourSpeed;

        // If the difference between the fillPositions y and pour points y is less than or equal to the radius of the opening,
        // we should start pouring.  We should also always pour if the fill position is strictly greater higher than the pour points
        // opening.
        var shouldPour = ShouldPour(_fillHeightPosition);
        _fluidPourHandler.SetActive(shouldPour);
        if (!shouldPour) return;

        var pourDirection = (_pourPoint.position - _fillHeightPosition).normalized;
        // Draw the stream and distribute fluid to receiving containers.
        _fluidPourHandler.Pour(_pourPoint.position, pourDirection, _currentPourSpeed, drainTime, currentContents, _fluidMaterial.GetColor(SideColorID));

        // Update the fill percent based on the pour speed.
        var amountToReduce = -Time.deltaTime * _currentPourSpeed;
        ModifyFluidContents(amountToReduce, currentContents);
    }
    

    private bool ShouldPour(Vector3 fillPosition)
    {
        var inRangeOfRadius = Mathf.Abs(fillPosition.y - _pourPoint.position.y) <= _pourPointRadius;
        var isAbovePourPoint = fillPosition.y >= _pourPoint.position.y;
        var hasFluid = _fillPercent > 0f;

        return (inRangeOfRadius || isAbovePourPoint) && hasFluid;
    }

    
    private void OnDrawGizmos()
    {
        if (!_renderDebug) return;
        var height = MathUtility.Remap(_fillPercent, 0.0f, 1.0f, -transform.localScale.y, transform.localScale.y);
        var fillPosition = new Vector3(transform.position.x, transform.position.y + height, transform.position.z);
        
        Gizmos.color = ShouldPour(fillPosition) ? Color.green : Color.red;
        Gizmos.DrawWireSphere(_pourPoint.position, _pourPointRadius);
        Gizmos.DrawLine(fillPosition, _pourPoint.position);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(fillPosition, 0.1f);

        _fluidPourHandler?.RenderDebugStream();
    }
}