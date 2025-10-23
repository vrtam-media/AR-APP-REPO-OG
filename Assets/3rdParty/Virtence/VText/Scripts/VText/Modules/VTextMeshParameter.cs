using System;
using UnityEngine;

namespace Virtence.VText
{
    /// <summary>
    /// Changes mostly require glyphs to rebuild
    /// </summary>
    [Serializable]
    public class VTextMeshParameter
    {
        // these events are only for the user

        #region EVENTS

        public event EventHandler<GenericEventArgs<float>> DepthChanged;                            // this is raised if the depth value changes

        public event EventHandler<GenericEventArgs<float>> BevelChanged;                            // this is raised if the bevel value changes
        
        public event EventHandler<GenericEventArgs<bool>> UseFaceUVsChanged;                        // this is raised if the bevels uses face uvs

        public event EventHandler<GenericEventArgs<BevelStyle>> BevelStyleChanged;                  // this is raised if the bevel style changes

        public event EventHandler<GenericEventArgs<AnimationCurve>> BevelProfileChanged;            // this is raised if the bevel profile changes

//        public event EventHandler<GenericEventArgs<int>> BevelProfileTesselationChanged;            // this is raised if the bevel profile tesselation changes

        public event EventHandler<GenericEventArgs<bool>> GenerateTangentsChanged;                  // this is raised if the need tangents value changes

        public event EventHandler<GenericEventArgs<bool>> HasBackfaceChanged;                       // this is raised if the backface value changes

        public event EventHandler<GenericEventArgs<float>> CreaseChanged;                           // this is raised if the creast value changes

        public event EventHandler<GenericEventArgs<float>> ResolutionChanged;                       // this is raised if the tesselation quality value changes

        public event EventHandler<GenericEventArgs<string>> FontNameChanged;                        // this is raised if the font name value changes

        #endregion EVENTS

        #region FIELDS

        /// <summary>
        /// The depth of the glyphs.
        /// </summary>
        [SerializeField]
        private float _depth = 0.0f;

        /// <summary>
        /// The bevel frame of the glyphs.
        ///
        /// range [0..1] where 1 is max factor of 1/10 width of glyph
        /// </summary>
        [SerializeField]
        private float _bevel = 0.0f;

        /// <summary>
        /// The style of the bevel edges
        /// 
        /// Choose from round, flat, chiseled or step
        /// </summary>
        [SerializeField]
        private BevelStyle _bevelStyle = BevelStyle.Round;

        /// <summary>
        /// The profil of bevel edges
        /// </summary>
        [SerializeField] 
        private AnimationCurve _bevelProfile;

        /// <summary>
        /// The profil of bevel edges
        ///
        /// Range >= 1
        /// </summary>

        // TODO: implement me
        //[SerializeField]
        //private int _bevelProfileTesselation;

        /// <summary>
        /// should face uvs be used for bevels
        /// </summary>
        [SerializeField]
        private bool _useFaceUVs;

        /// <summary>
        /// The need tangents property
        ///
        /// If set, tangents will be generated for Mesh
        /// </summary>
        [SerializeField]
        private bool _generateTangents = false;

        /// <summary>
        /// create backface
        ///
        /// If set, backface will be generated for Mesh
        /// </summary>
        [SerializeField]
        private bool _hasBackface = false;

        /// <summary>
        /// crease angle
        ///
        /// in degree for smoothing sides and bevel.
        /// </summary>
        [SerializeField]
        private float _crease = 89.0f;

        /// <summary>
        /// resolution of "round" edges
        ///
        /// in percent. range [0..100]
        /// if not in range no change!
        /// </summary>
        [SerializeField]
        private float _resolution = 0.01f;

        /// <summary>
        /// The fontname must specify a font available in StreamingAsset
        /// folder.
        /// Accepted formats are:
        ///  - ttf
        ///  - otf
        ///  - ps (Postscript)
        /// </summary>
        [SerializeField]
        private string _fontName = string.Empty;

        /// <summary>
        /// The text to render.
        /// might be overridden by external script for dynamic update.
        /// Line breaks by '\n'
        /// </summary>
        [SerializeField]
        private string _text = "VText";

        #region Invalidators

        /// <summary>
        /// if this is true just change something
        /// </summary>
        [SerializeField]
        private bool _modified = true;

        /// <summary>
        /// if this is true rebuild everything
        /// </summary>
        [SerializeField]
        private bool _rebuild = true;

        #endregion Invalidators

        #endregion FIELDS

        #region PROPERTIES

        /// <summary>
        /// The depth of the glyphs.
        ///
        /// getter setter
        /// </summary>
        public float Depth
        {
            get
            {
                return _depth;
            }

            set
            {
                float v = (value < 0.0f) ? 0.0f : value;
                if (_depth != v)
                {
                    if (DepthChanged != null)
                    {
                        DepthChanged.Invoke(this, new GenericEventArgs<float>(v));
                    }

                    _depth = v;
                    _modified = true;
                }
            }
        }

        /// <summary>
        /// The bevel frame of the glyphs.
        ///
        /// getter setter
        /// range >= 0
        /// </summary>
        public float Bevel
        {
            get
            {
                return _bevel;
            }

            set
            {
                float b = (value < 0.0f) ? 0.0f : value;

                if (_bevel != b)
                {
                    if (BevelChanged != null)
                    {
                        BevelChanged.Invoke(this, new GenericEventArgs<float>(b));
                    }

                    _bevel = b;
                    _modified = true;
                }
            }
        }

        public BevelStyle BevelStyle
        {
            get
            {
                return _bevelStyle;
            }

            set
            {
                var newStyle = value;
                if (_bevelStyle != newStyle)
                {
                    if (BevelStyleChanged != null)
                    {
                        BevelStyleChanged.Invoke(this, new GenericEventArgs<BevelStyle>(newStyle));
                    }

                    _bevelStyle = newStyle;
                    _modified = true;
                }
            }
        }

        /// <summary>
        /// Animation Curve for bevel profile.
        ///
        /// getter setter
        /// </summary>

        
        public AnimationCurve BevelProfile
        {
            get
            {
                return _bevelProfile;
            }

            set
            {
                var curve = value;
                if (_bevelProfile != curve)
                {
                    if (BevelProfileChanged != null)
                    {
                        BevelProfileChanged.Invoke(this, new GenericEventArgs<AnimationCurve>(curve));
                    }

                    _bevelProfile = curve;
                    _modified = true;
                }
            }
        }

        /// <summary>
        /// Should bevel uvs continue face uvs.
        ///
        /// getter setter
        /// </summary>
        public bool UseFaceUVs
        {
            get
            {
                return _useFaceUVs;
            }

            set
            {
                var useIt = value;
                if (_useFaceUVs != useIt)
                {
                    if (UseFaceUVsChanged != null)
                    {
                        UseFaceUVsChanged.Invoke(this, new GenericEventArgs<bool>(useIt));
                    }

                    _useFaceUVs = useIt;
                    _modified = true;
                }
            }
        }

        /// <summary>
        /// Quality for tesselation
        ///
        /// getter setter
        /// something bigger zero
        /// </summary>
        public float Resolution
        {
            get
            {
                return _resolution;
            }

            set
            {
                if (_resolution != value)
                {
                    _resolution = value;

                    if (ResolutionChanged != null)
                    {
                        ResolutionChanged.Invoke(this, new GenericEventArgs<float>(_resolution));
                    }

                    _rebuild = true;
                }
            }
        }

        /// <summary>
        /// Flag generate backface
        ///
        /// getter setter
        /// </summary>
        public bool HasBackface
        {
            get
            {
                return _hasBackface;
            }

            set
            {
                if (_hasBackface != value)
                {
                    _hasBackface = value;

                    if (HasBackfaceChanged != null)
                    {
                        HasBackfaceChanged.Invoke(this, new GenericEventArgs<bool>(_hasBackface));
                    }

                    _modified = true;
                }
            }
        }

        /// <summary>
        /// Flag generate tangents
        ///
        /// getter setter
        /// </summary>
        public bool GenerateTangents
        {
            get
            {
                return _generateTangents;
            }

            set
            {
                if (_generateTangents != value)
                {
                    _generateTangents = value;

                    if (GenerateTangentsChanged != null)
                    {
                        GenerateTangentsChanged.Invoke(this, new GenericEventArgs<bool>(_generateTangents));
                    }

                    _modified = true;
                }
            }
        }

        /// <summary>
        /// Fontname
        ///
        /// getter setter
        /// </summary>
        public string FontName
        {
            get
            {
                return _fontName;
            }

            set
            {
                // Debug.Log("set font name: " + value);
                if (_fontName != value)
                {
                    _fontName = value;

                    if (FontNameChanged != null)
                    {
                        FontNameChanged.Invoke(this, new GenericEventArgs<string>(_fontName));
                    }

                    _rebuild = true;
                }
				_rebuild = true;
            }
        }

        public string Text
        {
            get
            {
                return _text;
            }

            set
            {
                if (_text != value)
                {
                    _modified = true;
                    _text = value;                    
                }
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

        /// <summary>
        /// set _rebuild to false if true and return true,
        /// false otherwise.
        /// </summary>
        /// <returns></returns>
        public bool CheckClearRebuild()
        {
            if (_rebuild)
            {
                _modified = _rebuild = false;
                return true;
            }
            return false;
        }

        #endregion METHODS
    }
}