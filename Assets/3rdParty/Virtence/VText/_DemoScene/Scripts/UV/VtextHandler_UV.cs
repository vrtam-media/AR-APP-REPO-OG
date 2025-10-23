using UnityEngine;
using System;
using UnityEngine.Rendering;

namespace Virtence.VText.Demo
{
	/// <summary>
	///handle vtext changes in the start scene.
	/// </summary>
	public class VtextHandler_UV : MonoBehaviour {
	    #region EVENTS
	    public event EventHandler<GenericEventArgs<string>> FontNameChanged;                        // this will be raised if the used fontname changes
	    public event EventHandler<GenericEventArgs<float>> SizeValueChanged;                        // this will be raised if the size value of the VTextInterface changes
	    public event EventHandler<GenericEventArgs<float>> DepthValueChanged;                       // this will be raised if the size value of the VTextInterface changes
	    public event EventHandler<GenericEventArgs<float>> BevelValueChanged;                       // this will be raised if the bevel value of the VTextInterface changes
		public event EventHandler<GenericEventArgs<bool>> UseFaceUVValueChanged;                    // this will be raised if the useFaceUV of the VTextInterface changes

		#endregion


		public VText vti_textOptions = null;

			
	    //heading
	    //private int old_headingValue;

	    // size
	    private float _minSize = 0.1f;
	    private float _maxSize = 1.0f;

	    //depth
	    private float _minDepth = 0.0f;
	    private float _maxDepth = 12.0f;

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
	        vti_textOptions.MeshParameter.BevelChanged += OnBevelChanged;
	        vti_textOptions.MeshParameter.DepthChanged += OnDepthChanged;
	        vti_textOptions.MeshParameter.FontNameChanged += OnFontNameChanged;
			vti_textOptions.MeshParameter.UseFaceUVsChanged += OnUseFaceUVsChanged;
	        
            if (vti_textOptions != null) {
                vti_textOptions.RenderParameter.LightProbeUsage = LightProbeUsage.BlendProbes;
            }

	        SetSize(1f);                                      // init size
	        SetDepth(0.1f);                                     // init depth
	        SetBevel(0.6f);                                     // init bevel

	        SetFont(vti_textOptions.MeshParameter.FontName);        // init font type
	        if (FontNameChanged != null) {
	            FontNameChanged.Invoke(this, new GenericEventArgs<string>(vti_textOptions.MeshParameter.FontName));
	        }
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
		/// change useface uvs of vti_textOptions
		/// </summary>
		public void SetUseFaceUVs(bool value)
		{
			if (vti_textOptions != null)
			{
				vti_textOptions.MeshParameter.UseFaceUVs = value;
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

		private void OnUseFaceUVsChanged(object sender, GenericEventArgs<bool> e)
		{
			if (UseFaceUVValueChanged != null)
			{
				UseFaceUVValueChanged.Invoke(this, e);
			}
		}

		#endregion // EVENT HANDLERS
	}
}