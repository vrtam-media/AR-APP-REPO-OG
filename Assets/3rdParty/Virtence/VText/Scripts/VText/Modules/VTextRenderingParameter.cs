using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Virtence.VText
{
    /// <summary>
    /// Changes modify MeshRenderer of the glyphs
    /// </summary>
    [Serializable]
    public class VTextRenderParameter
    {
        // these events are only for the user

        #region EVENTS

        public event EventHandler<GenericEventArgs<ShadowCastingMode>> ShadowCastingModeChanged;    // this is raised if the shadow casting mode value changes

        public event EventHandler<GenericEventArgs<bool>> ReceiveShadowsChanged;                    // this is raised if the receive shadows value changes

#if UNITY_2018_3_OR_NEWER

        public event EventHandler<GenericEventArgs<LightProbeUsage>> LightProbeUsageChanged;        // this is raised if the lightprobe usage value changes

#else

        public event EventHandler<GenericEventArgs<bool>> UseLightProbesChanged;                    // this is raised if the use lightprobes value changes

#endif
        public event EventHandler<GenericEventArgs<bool>> ApplyLayerSettingsChanged;                // this is raised if the apply layer settings value changes
        
        #endregion EVENTS

        #region EXPOSED

        /// <summary>
        /// Select your Materials.
        /// Materials[0] for front faces
        /// Materials[1] for bevel faces
        /// Materials[2] for side faces
        /// </summary>
        [SerializeField]
        private Material[] _materials;

        #endregion EXPOSED

        #region FIELDS

        /// <summary>
        /// The cast shadows property
        ///
        /// will be passed to children  Mesh Renderer.
        /// </summary>
        [SerializeField]
        private ShadowCastingMode _shadowCastMode = ShadowCastingMode.On;

        /// <summary>
        /// The receive shadows property
        ///
        /// will be passed to children Mesh Renderer.
        /// </summary>
        [SerializeField]
        private bool _receiveShadows = true;

        /// <summary>
        /// The use light probes property
        ///
        /// will be passed to children Mesh Renderer.
        /// </summary>
#if UNITY_2018_3_OR_NEWER

        [SerializeField]
        private LightProbeUsage _lightProbeUsage = LightProbeUsage.Off;

#else
        /// <summary>
        /// The use light probes property
        ///
        /// will be passed to children Mesh Renderer.
        [SerializeField]
        private bool _useLightProbes = false;
#endif

        /// <summary>
        /// The apply layer settings property
        ///
        /// will be passed to children. ... if set to true the children will have the same layer as the VText object ... if not it will use "default" 
        /// </summary>
        [SerializeField]
        private bool _applyLayerSettings = true;

        #region Invalidators

        /// <summary>
        /// if this is true just change something
        /// </summary>
        [SerializeField]
        private bool _modified = true;

        #endregion Invalidators

        #endregion FIELDS

        #region PROPERTIES
        /// <summary>
        /// Select your Materials.
        /// Materials[0] for front faces
        /// Materials[1] for bevel faces
        /// Materials[2] for side faces
        /// </summary>
        public Material[] Materials
        {
            get
            {
                if (_materials != null)
                {
                    return _materials;
                }
                else
                {
                    _materials = new Material[3];
                }
                // set default material
                var mat = new Material(Shader.Find("Standard"));
                if (_materials[0] == null)
                {
                    _materials[0] = mat;
                }

                if (_materials[1] == null)
                {
                    _materials[1] = mat;
                }

                if (_materials[2] == null)
                {
                    _materials[2] = mat;
                }
                return _materials;
            }

            set
            {
                if(value == null)
                {
                    Debug.LogWarning("Given Value is null. Ignoring set.");
                    return;
                }
                if(value.Length != 3)
                {
                    Debug.LogWarning("Given array length is != 3. Ignoring set.");
                    return;
                }
                _materials = value;

                _modified = true;
            }
        }

        /// <summary>
        /// shadow casting Mode
        /// </summary>
        public ShadowCastingMode ShadowCastMode
        {
            get
            {
                return _shadowCastMode;
            }

            set
            {
                _shadowCastMode = value;

                if (ShadowCastingModeChanged != null)
                {
                    ShadowCastingModeChanged.Invoke(this, new GenericEventArgs<ShadowCastingMode>(_shadowCastMode));
                }

                _modified = true;
            }
        }

        /// <summary>
        /// flag receive shadows
        /// </summary>
        public bool ReceiveShadows
        {
            get
            {
                return _receiveShadows;
            }

            set
            {
                _receiveShadows = value;

                if (ReceiveShadowsChanged != null)
                {
                    ReceiveShadowsChanged.Invoke(this, new GenericEventArgs<bool>(_receiveShadows));
                }

                _modified = true;
            }
        }

#if UNITY_2018_3_OR_NEWER

        /// <summary>
        /// enum of LightProbeUsage
        /// </summary>
        public LightProbeUsage LightProbeUsage
        {
            get
            {
                return _lightProbeUsage;
            }

            set
            {
                _lightProbeUsage = value;

                if (LightProbeUsageChanged != null)
                {
                    LightProbeUsageChanged.Invoke(this, new GenericEventArgs<LightProbeUsage>(_lightProbeUsage));
                }

                _modified = true;
            }
        }

#else
        /// <summary>
        /// flag use Lightprobes
        /// </summary>
        public bool UseLightProbes
        {
            get
            {
                return _useLightProbes;
            }

            set
            {
                _useLightProbes = value;

                if (UseLightProbesChanged != null)
                {
                    UseLightProbesChanged.Invoke(this, new GenericEventArgs<bool>(_useLightProbes));
                }

                _modified = true;
            }
        }
#endif

        /// <summary>
        /// flag apply layer settings
        /// </summary>
        public bool ApplyLayerSettings
        {
            get
            {
                return _applyLayerSettings;
            }

            set
            {
                _applyLayerSettings = value;

                if (ApplyLayerSettingsChanged != null)
                {
                    ApplyLayerSettingsChanged.Invoke(this, new GenericEventArgs<bool>(_applyLayerSettings));
                }

                _modified = true;
            }
        }

        #endregion PROPERTIES

        #region METHODS

        /// <summary>
        /// set _modified to false if true and return true,
        /// false otherwise.
        /// </summary>
        /// <returns></returns>
        public bool CheckClearModified()
        {
            if (_modified)
            {
                _modified = false;
                return true;
            }
            return false;
        }

        #endregion METHODS
    }
}