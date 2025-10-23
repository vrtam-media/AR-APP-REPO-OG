using System;
using UnityEngine;

namespace Virtence.VText
{
    /// <summary>
    /// Layouting of VText
    /// </summary>
    [Serializable]
    public class VTextLayoutParameter
    {
        #region EVENTS

        public event EventHandler<GenericEventArgs<bool>> IsHorizontalLayoutChanged;        // this is raised if the major alignment value changes

        public event EventHandler<GenericEventArgs<float>> SizeChanged;                     // this is raised if the size value changes

        public event EventHandler<GenericEventArgs<Align>> MajorChanged;                    // this is raised if the major alignment value changes

        public event EventHandler<GenericEventArgs<Align>> MinorChanged;                    // this is raised if the minor alignment value changes

        public event EventHandler<GenericEventArgs<float>> LineSpacingChanged;              // this is raised if the line spacing value changes

        public event EventHandler<GenericEventArgs<float>> GlyphSpacingChanged;             // this is raised if the glyph spacing value changes

        #endregion EVENTS

        #region FIELDS

        [SerializeField]
        private bool _isHorizontal = true;

        [SerializeField]
        private Align _major = Align.Base;

        [SerializeField]
        private Align _minor = Align.Base;

        [SerializeField]
        private float _size = 1.0f;

        [SerializeField]
        private float _spacing = 1.0f;

        [SerializeField]
        private float _glyphSpacing = 0.0f;

        [SerializeField]
        private AnimationCurve _curveXZ;

        [SerializeField]
        private AnimationCurve _curveXY;

        [SerializeField]
        private bool _orientXZ = false;

        [SerializeField]
        private bool _orientXY = false;

        [SerializeField]
        private bool _isCircular = false;

        [SerializeField]
        private float _startRadius = 0.0f;

        [SerializeField]
        private float _endRadius = 180.0f;

        [SerializeField]
        private float _circleRadius = 10.0f;

        [SerializeField]
        private bool _animateRadius = false;

        [SerializeField]
        private AnimationCurve _curveRadius;

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

        #endregion FIELDS

        #region PROPERTIES

        /// <summary>
        /// Main layout direction.
        /// If false the text will be layout vertical.
        /// </summary>
        public bool IsHorizontal
        {
            get
            {
                return _isHorizontal;
            }

            set
            {
                if (_isHorizontal != value)
                {
                    _isHorizontal = value;
                    if (IsHorizontalLayoutChanged != null)
                    {
                        IsHorizontalLayoutChanged.Invoke(this, new GenericEventArgs<bool>(_isHorizontal));
                    }
                }

                _modified = true;
            }
        }

        /// <summary>
        /// The major aligment.
        /// </summary>
        public Align Major
        {
            get
            {
                return _major;
            }

            set
            {
                if (value != _major)
                {
                    _major = value;
                    _modified = true;

                    if (MajorChanged != null)
                    {
                        MajorChanged.Invoke(this, new GenericEventArgs<Align>(_major));
                    }
                }
            }
        }

        /// <summary>
        /// The minor aligment.
        /// </summary>
        public Align Minor
        {
            get
            {
                return _minor;
            }

            set
            {
                if (value != _minor)
                {
                    _minor = value;

                    if (MinorChanged != null)
                    {
                        MinorChanged.Invoke(this, new GenericEventArgs<Align>(_minor));
                    }

                    _modified = true;
                }
            }
        }

        /// <summary>
        /// The font size scale factor.
        /// </summary>
        public float Size
        {
            get
            {
                return _size;
            }

            set
            {
                if (value != _size)
                {
                    _size = value;

                    if (SizeChanged != null)
                    {
                        SizeChanged.Invoke(this, new GenericEventArgs<float>(_size));
                    }

                    _rebuild = true;
                }
            }
        }

        /// <summary>
        /// The line spacing factor.
        /// </summary>
        public float Spacing
        {
            get
            {
                return _spacing;
            }

            set
            {
                if (value != _spacing)
                {
                    _spacing = value;
                    if (LineSpacingChanged != null)
                    {
                        LineSpacingChanged.Invoke(this, new GenericEventArgs<float>(_spacing));
                    }
                    _modified = true;
                }
            }
        }

        /// <summary>
        /// The spacing between glyphs
        /// </summary>
        public float GlyphSpacing
        {
            get
            {
                return _glyphSpacing;
            }

            set
            {
                if (value != _glyphSpacing)
                {
                    _glyphSpacing = value;
                    if (GlyphSpacingChanged != null)
                    {
                        GlyphSpacingChanged.Invoke(this, new GenericEventArgs<float>(_glyphSpacing));
                    }
                    _modified = true;
                }
            }
        }

        /// <summary>
        /// The XZ Curve
        /// </summary>
        public AnimationCurve CurveXZ
        {
            get
            {
                return _curveXZ;
            }

            set
            {
                _modified = true;
                _curveXZ = value;
            }
        }

        /// <summary>
        /// The XY Curve
        /// </summary>
        public AnimationCurve CurveXY
        {
            get
            {
                return _curveXY;
            }

            set
            {
                _modified = true;
                _curveXY = value;
            }
        }

        /// <summary>
        /// adjust orientation for XZ Curve
        /// </summary>
        public bool OrientationXZ
        {
            get
            {
                return _orientXZ;
            }

            set
            {
                if (value != _orientXZ)
                {
                    _modified = true;
                    _orientXZ = value;
                }
            }
        }

        /// <summary>
        /// adjust orientation for XY Curve
        /// </summary>
        public bool OrientationXY
        {
            get
            {
                return _orientXY;
            }

            set
            {
                if (value != _orientXY)
                {
                    _modified = true;
                    _orientXY = value;
                }
            }
        }

        /// <summary>
        /// bend the text circular
        /// </summary>
        public bool OrientationCircular
        {
            get
            {
                return _isCircular;
            }

            set
            {
                if (value != _isCircular)
                {
                    _modified = true;
                    _isCircular = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the start radius.
        /// </summary>
        /// <value>The start radius.</value>
        public float StartRadius
        {
            get
            {
                return _startRadius;
            }

            set
            {
                if (value != _startRadius)
                {
                    _modified = true;
                }
                _startRadius = value;
            }
        }

        /// <summary>
        /// Gets or sets the end radius.
        /// </summary>
        /// <value>The end radius.</value>
        public float EndRadius
        {
            get
            {
                return _endRadius;
            }

            set
            {
                if (value != _endRadius)
                {
                    _modified = true;
                }
                _endRadius = value;
            }
        }

        /// <summary>
        /// Gets or sets the radius of the circle.
        /// </summary>
        /// <value>The circle radius.</value>
        public float CircleRadius
        {
            get
            {
                return _circleRadius;
            }

            set
            {
                if (value != _circleRadius)
                {
                    _modified = true;
                }
                _circleRadius = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether radius should be determined by the AnimationCurve CurveRadius
        /// </summary>
        /// <value><c>true</c> if animate radius; otherwise, <c>false</c>.</value>
        public bool AnimateRadius
        {
            get
            {
                return _animateRadius;
            }

            set
            {
                if (value != _animateRadius)
                {
                    _modified = true;
                }
                _animateRadius = value;
            }
        }

        /// <summary>
        /// Gets or sets the CurveRadius Animationcurve.
        /// </summary>
        /// <value>The curve radius.</value>
        public AnimationCurve CurveRadius
        {
            get
            {
                return _curveRadius;
            }

            set
            {
                if (value != _curveRadius)
                {
                    _modified = true;
                    _curveRadius = value;
                }
            }
        }

        #endregion PROPERTIES

        #region CONSTRUCTORS

        public VTextLayoutParameter()
        {
            _curveXZ = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
            _curveXY = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
            _curveRadius = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
        }

        #endregion CONSTRUCTORS

        #region METHODS

        /// <summary>
        /// check if the parameters are modified and reset the modify flag
        /// </summary>
        /// <returns><c>true</c>, if the parameters are modified, <c>false</c> otherwise.</returns>
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


        public void ForceUpdate() {
            _modified = true;
        }
        #endregion METHODS
    }
}