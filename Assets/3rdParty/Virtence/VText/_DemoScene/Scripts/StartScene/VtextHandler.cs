using UnityEngine;
using System;
using UnityEngine.Rendering;

namespace Virtence.VText.Demo
{
	/// <summary>
	///handle vtext changes in the start scene.
	/// </summary>
	public class VtextHandler : MonoBehaviour {
	    #region EVENTS
	    public event EventHandler<GenericEventArgs<string>> FontNameChanged;                        // this will be raised if the used fontname changes
	    public event EventHandler<GenericEventArgs<float>> SizeValueChanged;                        // this will be raised if the size value of the VTextInterface changes
	    public event EventHandler<GenericEventArgs<float>> DepthValueChanged;                       // this will be raised if the size value of the VTextInterface changes
	    public event EventHandler<GenericEventArgs<float>> BevelValueChanged;                       // this will be raised if the bevel value of the VTextInterface changes
	    public event EventHandler<GenericEventArgs<Align>> MajorValueChanged;                       // this will be raised if the major alignment value of the VTextInterface changes	    
        public event EventHandler<GenericEventArgs<LightProbeUsage>> LightProbeUsageChanged;        // this will be raised if the usage of lightprobes of the VTextInterface changes
        #endregion

        public VText[] vti_time = null;
	    public VText vti_textOptions = null;
	    public VText vti_textured = null;
			
	    //heading
	    //private int old_headingValue;

	    // size
	    private float _minSize = 0.1f;
	    private float _maxSize = 1.0f;

	    //depth
	    private float _minDepth = 0.0f;
	    private float _maxDepth = 3.0f;

	    //bevel
	    private float _minBevel = 0.0f;
	    private float _maxBevel = 0.1f;


	    /// <summary>
	    /// Awake this instance.
	    /// </summary>
	    void Start() { 
	        if (Loom.Current == null)
	            Loom.Initialize();

	        vti_textOptions.LayoutParameter.SizeChanged += OnSizeChanged;
	        vti_textOptions.LayoutParameter.MajorChanged += OnMajorLayoutChanged;
	        vti_textOptions.MeshParameter.BevelChanged += OnBevelChanged;
	        vti_textOptions.MeshParameter.DepthChanged += OnDepthChanged;
	        vti_textOptions.RenderParameter.LightProbeUsageChanged += OnLightProbeUsageChanged;
	        vti_textOptions.MeshParameter.FontNameChanged += OnFontNameChanged;

	        foreach (VText vi in vti_time) {
                if (vi != null &&  vi.RenderParameter != null)
                {
                    vi.RenderParameter.LightProbeUsage = LightProbeUsage.BlendProbes;
                }
	        }
            if (vti_textOptions != null) {
                vti_textOptions.RenderParameter.LightProbeUsage = LightProbeUsage.BlendProbes;
            }

            if (vti_textured != null) {
                vti_textured.RenderParameter.LightProbeUsage = LightProbeUsage.BlendProbes;
            }
	        
			
	        // init alignment
	        //old_headingValue = (int) Align.Center;
	        SetAlignment(Align.Center);

	        SetSize(0.4f);                                      // init size
	        SetDepth(0.1f);                                     // init depth
	        SetBevel(0.6f);                                     // init bevel

	        SetFont(vti_textOptions.MeshParameter.FontName);        // init font type
	        if (FontNameChanged != null) {
	            FontNameChanged.Invoke(this, new GenericEventArgs<string>(vti_textOptions.MeshParameter.FontName));
	        }
	    }

        /// <summary>
        /// enable or disable the usage of lightprobes for the vtext objects
        /// </summary>
        /// <param name="SetLightProbes">If set to <c>true</c> enable light probes.</param>
        public void SetLightProbes(LightProbeUsage lp) {
	        foreach (VText vi in vti_time) {
	            vi.RenderParameter.LightProbeUsage = lp;
	        }
	        vti_textOptions.RenderParameter.LightProbeUsage = lp;
	        vti_textured.RenderParameter.LightProbeUsage = lp;
	    }

	    /// <summary>
	    /// change font of vti_textOptions
	    /// </summary>
	    public void SetFont(string fontName) {
	        vti_textOptions.MeshParameter.FontName = fontName;
	//        TransformTxt(vti_textOptions.layout.Major);
	    }

	    /// <summary>
	    /// change size of vti_textOptions
	    /// </summary>
	    public void SetSize(float sizeValue) {        
	        if (vti_textOptions != null) {
	            vti_textOptions.LayoutParameter.Size = _minSize + sizeValue * (_maxSize - _minSize);
	        }
	    }

	    /// <summary>
	    /// change depth of vti_textOptions
	    /// </summary>
	    public void SetDepth(float depthValue) {
	        if (vti_textOptions != null) {                
	            vti_textOptions.MeshParameter.Depth = _minDepth + depthValue * (_maxDepth - _minDepth);
			}
	    }

	    /// <summary>
	    /// change bevel of vti_textOptions
	    /// </summary>
	    public void SetBevel(float bevelValue) {
	        if (vti_textOptions != null) {
	            vti_textOptions.MeshParameter.Bevel = _minBevel + bevelValue * (_maxBevel - _minBevel);
	        }
	    }

	    /// <summary>
	    /// Sets the alignment of the vti_textOptions text
	    /// </summary>
	    /// <param name="alignment">Alignment.</param>
	    public void SetAlignment(Align alignment) {
	        vti_textOptions.LayoutParameter.Major = alignment;
	        TransformTxt(alignment);
	    }

	    /// <summary>
	    /// change transformation of vti_textOptions in dependence of the current alignment to avoid shifts
	    /// </summary>
	    /// <param name="layout">Layout.</param>
	    void TransformTxt(Align alignment) {
	        float width = vti_textOptions.GetBounds().size.x;
	            
	        switch (alignment) {
	        case Align.Base:
	        case Align.Start:
	        case Align.Justified:
	            vti_textOptions.transform.localPosition = new Vector3(-width * 0.25f, vti_textOptions.transform.localPosition.y, vti_textOptions.transform.localPosition.z);
	            break;
	        case Align.Center:
	            vti_textOptions.transform.localPosition = Vector3.zero;
	            break;
	        case Align.End:
	            vti_textOptions.transform.localPosition = new Vector3(width * 0.25f, vti_textOptions.transform.localPosition.y, vti_textOptions.transform.localPosition.z);
	            break;
	        }

	    }

	    #region EVENT HANDLERS
	    /// <summary>
	    /// this is called if the size value of the vtext interface changes 
	    /// </summary>
	    /// <param name="sender">Sender.</param>
	    /// <param name="e">E.</param>
	    void OnSizeChanged (object sender, GenericEventArgs<float> e)
	    {
	        if (SizeValueChanged != null) {
	            
	            float normalizedValue = (e.Value - _minSize) / (_maxSize - _minSize);
	            SizeValueChanged.Invoke(this, new GenericEventArgs<float>(normalizedValue));
	        }
	    }

	    /// <summary>
	    /// this is called if the depth value of the vtext interface changes
	    /// </summary>
	    /// <param name="sender">Sender.</param>
	    /// <param name="e">E.</param>
	    void OnDepthChanged (object sender, GenericEventArgs<float> e)
	    {
	        if (DepthValueChanged != null) {

	            float normalizedValue = (e.Value - _minDepth) / (_maxDepth - _minDepth);
	            DepthValueChanged.Invoke(this, new GenericEventArgs<float>(normalizedValue));
	        }
	    }

	    /// <summary>
	    /// this is called if the bevel value of the vtext interface changes
	    /// </summary>
	    /// <param name="sender">Sender.</param>
	    /// <param name="e">E.</param>
	    void OnBevelChanged (object sender, GenericEventArgs<float> e)
	    {
	        if (BevelValueChanged != null) {

	            float normalizedValue = (e.Value - _minBevel) / (_maxBevel - _minBevel);
	            BevelValueChanged.Invoke(this, new GenericEventArgs<float>(normalizedValue));
	        }
	    }

	    /// <summary>
	    /// this is called if the major layout changes
	    /// </summary>
	    /// <param name="sender">Sender.</param>
	    /// <param name="e">E.</param>
	    void OnMajorLayoutChanged (object sender, GenericEventArgs<Align> e)
	    {
	        if (MajorValueChanged != null) {
	            MajorValueChanged.Invoke(this, e);
	        }
	    }

        /// <summary>
        /// this is called if the light probe usage changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLightProbeUsageChanged(object sender, GenericEventArgs<LightProbeUsage> e)
        {
            if (LightProbeUsageChanged != null)
            {
                LightProbeUsageChanged.Invoke(this, e);
            }
        }

        /// <summary>
        /// this is called if the current used fontname changes
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        void OnFontNameChanged (object sender, GenericEventArgs<string> e)
	    {
	        if (FontNameChanged != null) {
	            FontNameChanged.Invoke(this, e);
	        }
	    }
	    #endregion // EVENT HANDLERS
	}
}