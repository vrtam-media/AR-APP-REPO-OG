/*
 * $Id: VTextInterface.cs 172 2015-03-13 14:05:02Z dirk $
 * 
 * Virtence VFont package
 * Copyright 2014 .. 2016 by Virtence GmbH
 * http://www.virtence.com
 * 
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using Virtence.VText.Extensions;

namespace Virtence.VText.LEGACY
{

	/// <summary>
	/// VText mesh parameter.
	/// 
	/// change requires rebuild of glyph meshes
	/// </summary>

	[System.Serializable]
	public class VTextParameter
	{

		#region EVENTS
		#endregion // EVENTS

		#region FIELDS
		/// <summary>
		/// The depth of the glyphs.
		/// </summary>
		[SerializeField]
		private float m_depth = 0.0f;
		/// <summary>
		/// The bevel frame of the glyphs.
		/// 
		/// range [0..1] where 1 is max factor of 1/10 width of glyph
		/// </summary>
		[SerializeField]
		private float m_bevel = 0.0f;
		/// <summary>
		/// The need tangents property
		/// 
		/// If set, tangents will be generated for Mesh
		/// </summary>
		[SerializeField]
		private bool m_needTangents = false;
		/// <summary>
		/// create backface
		/// 
		/// If set, backface will be generated for Mesh
		/// </summary>
		[SerializeField]
		private bool m_backface = false;
		/// <summary>
		/// crease angle
		/// 
		/// in degree for smoothing sides and bevel.
		/// </summary>
		[SerializeField]
		private float m_crease = 35.0f;
		/// <summary>
		/// quality
		/// 
		/// in percent. range [0..100]
		/// if not in range no change!
		/// </summary>
		[SerializeField]
		private int m_quality = 20;

		/// <summary>
		/// The fontname must specify a font available in StreamingAsset
		/// folder.
		/// Accepted formats are:
		///  - ttf
		///  - otf
		///  - ps (Postscript)
		/// </summary>
		[SerializeField]
		private string m_fontname = string.Empty;

		/// <summary>
		/// The cast shadows property
		/// 
		/// will be passed to children  Mesh Renderer.
		/// </summary>
		[SerializeField]
		private ShadowCastingMode m_shadowCastMode = ShadowCastingMode.On;

		/// <summary>
		/// The receive shadows property
		/// 
		/// will be passed to children Mesh Renderer.
		/// </summary>
		[SerializeField]
		private bool m_receiveShadows = true;

		/// <summary>
		/// The use light probes property
		/// 
		/// will be passed to children Mesh Renderer.
		/// </summary>
		[SerializeField]
		private bool m_useLightProbes = false;
		#endregion // FIELDS

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
				return m_depth;
			}
			set
			{
				float v = (value < 0.0f) ? 0.0f : value;
				if (m_depth != v)
				{
					m_depth = v;

					if ((m_depth - Mathf.Epsilon) < 0)
						Bevel = 0;
				}
			}
		}

		/// <summary>
		/// The crease angle to generate sides and bevel
		/// 
		/// getter setter
		/// range [10..45]
		/// </summary>
		public float Crease
		{
			get
			{
				return m_crease;
			}
			set
			{
				float v = Mathf.Clamp(value, 10f, 45f);
				if (m_crease != v)
				{
					m_crease = v;
				}
			}
		}

		/// <summary>
		/// The bevel frame of the glyphs.
		/// 
		/// getter setter
		/// range [0..1] where 1 is max factor of 1/10 width of glyph
		/// </summary>
		public float Bevel
		{
			get
			{
				return m_bevel;
			}
			set
			{
				float bevel = Mathf.Clamp01(value);
				if ((Depth - Mathf.Epsilon) < 0)
				{
					bevel = 0;
				}

				if (m_bevel != bevel)
				{
					m_bevel = bevel;
				}
			}
		}

		/// <summary>
		/// Quality for tesselation
		/// 
		/// getter setter
		/// in percent range [0..100]
		/// </summary>
		public int Quality
		{
			get
			{
				return m_quality;
			}
			set
			{
				if (m_quality != value)
				{
					m_quality = value;
				}
			}
		}

		/// <summary>
		/// Flag generate backface
		/// 
		/// getter setter
		/// </summary>
		public bool Backface
		{
			get
			{
				return m_backface;
			}
			set
			{
				if (m_backface != value)
				{
					m_backface = value;
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
				return m_needTangents;
			}
			set
			{
				if (m_needTangents != value)
				{
					m_needTangents = value;
				}
			}
		}

		/// <summary>
		/// Fontname
		/// 
		/// getter setter
		/// </summary>
		public string Fontname
		{
			get
			{
				return m_fontname;
			}
			set
			{
				if (m_fontname != value)
				{
					m_fontname = value;
				}
			}
		}

		/// <summary>
		/// shadow casting Mode
		/// </summary>
		public ShadowCastingMode ShadowCastMode
		{
			get
			{
				return m_shadowCastMode;
			}
			set
			{
				m_shadowCastMode = value;
			}
		}

		/// <summary>
		/// flag receive shadows
		/// </summary>
		public bool ReceiveShadows
		{
			get
			{
				return m_receiveShadows;
			}
			set
			{
				m_receiveShadows = value;
			}
		}

		/// <summary>
		/// flag use Lightprobes
		/// </summary>
		public bool UseLightProbes
		{
			get
			{
				return m_useLightProbes;
			}
			set
			{
				m_useLightProbes = value;
			}
		}
		#endregion // PROPERTIES

		#region METHODS
		#endregion // METHODS

	}

	[System.Serializable]
	public class VTextLayout
	{
		//! alignment
		public enum align
		{
			Base,
			Start,
			Center,
			End,
			Block
		};

		#region EVENTS
		#endregion // EVENTS

		#region FIELDS 
		[SerializeField]
		private bool m_horizontal = true;

		[SerializeField]
		private align m_major = align.Base;

		[SerializeField]
		private align m_minor = align.Base;

		[SerializeField]
		private float m_size = 1.0f;

		[SerializeField]
		private float m_spacing = 1.0f;

		[SerializeField]
		private float m_glyphSpacing = 0.0f;

		[SerializeField]
		private AnimationCurve m_curveXZ;

		[SerializeField]
		private AnimationCurve m_curveXY;

		[SerializeField]
		private bool m_orientXZ = false;

		[SerializeField]
		private bool m_orientXY = false;

		[SerializeField]
		private bool m_isCircular = false;

		[SerializeField]
		private float m_startRadius = 0.0f;

		[SerializeField]
		private float m_endRadius = 180.0f;

		[SerializeField]
		private float m_circleRadius = 10.0f;

		[SerializeField]
		private bool m_animateRadius = false;

		[SerializeField]
		private AnimationCurve m_curveRadius;
		#endregion // FIELDS

		#region PROPERTIES
		/// <summary>
		/// Main layout direction.
		/// If false the text will be layout vertical.
		/// </summary>
		public bool Horizontal
		{
			get
			{
				return m_horizontal;
			}
			set
			{
				if (m_horizontal != value)
				{
					m_horizontal = value;
				}
			}
		}

		/// <summary>
		/// The major aligment.
		/// </summary>
		public align Major
		{
			get
			{
				return m_major;
			}
			set
			{
				if (value != m_major)
				{
					m_major = value;					
				}
			}
		}

		/// <summary>
		/// The minor aligment.
		/// </summary>
		public align Minor
		{
			get
			{
				return m_minor;
			}
			set
			{
				if (value != m_minor)
				{
					m_minor = value;
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
				return m_size;
			}
			set
			{
				if (value != m_size)
				{
					m_size = value;					
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
				return m_spacing;
			}
			set
			{
				if (value != m_spacing)
				{
					m_spacing = value;					
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
				return m_glyphSpacing;
			}
			set
			{
				if (value != m_glyphSpacing)
				{
					m_glyphSpacing = value;					
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
				return m_curveXZ;
			}
			set
			{
				m_curveXZ = value;
			}
		}

		/// <summary>
		/// The XY Curve
		/// </summary>
		public AnimationCurve CurveXY
		{
			get
			{
				return m_curveXY;
			}
			set
			{
				m_curveXY = value;
			}
		}

		/// <summary>
		/// adjust orientation for XZ Curve
		/// </summary>
		public bool OrientationXZ
		{
			get
			{
				return m_orientXZ;
			}
			set
			{
				if (value != m_orientXZ)
				{
					m_orientXZ = value;
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
				return m_orientXY;
			}
			set
			{
				if (value != m_orientXY)
				{
					m_orientXY = value;
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
				return m_isCircular;
			}
			set
			{
				if (value != m_isCircular)
				{
					m_isCircular = value;
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
				return m_startRadius;
			}
			set
			{
				m_startRadius = value;
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
				return m_endRadius;
			}
			set
			{
				m_endRadius = value;
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
				return m_circleRadius;
			}
			set
			{
				m_circleRadius = value;
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
				return m_animateRadius;
			}
			set
			{
				m_animateRadius = value;
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
				return m_curveRadius;
			}
			set
			{
				if (value != m_curveRadius)
				{
					m_curveRadius = value;
				}
			}
		}

		#endregion // PROPERTIES

		#region CONSTRUCTORS
		public VTextLayout()
		{
			m_curveXZ = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
			m_curveXY = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
			m_curveRadius = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
		}
		#endregion // CONSTRUCTORS

		#region METHODS
		#endregion // METHODS
	}

	/// <summary>
	/// the physics parameters for VText objects
	/// </summary>
	[System.Serializable]
	public class VTextPhysics
	{
		/// <summary>
		/// Bounding box types
		/// TO BE BACKWARD COMPATIBLE ADD NEW ENTRIES ONLY TO THE END OF THE LIST ... THIS IS SERIALIZED AS AN INTEGER!!!!!
		/// </summary>
		public enum ColliderType
		{
			None,
			Box,
			Mesh,
		};

		#region VARIABLES

		#region COLLIDER
		[SerializeField]
		private ColliderType _colliderType = ColliderType.None;         // the type of the collider which should be added to each glyph

		[SerializeField]
		private PhysicsMaterial _colliderMaterial = null;                // the physics material for the collider

		[SerializeField]
		private bool _colliderIsTrigger = false;                        // determines if this collider is a trigger or not

		[SerializeField]
		private bool _colliderIsConvex = false;                         // determines if this collider is convex or not (used for mesh colliders)

		#endregion // COLLIDER

		#region RIGIDBODY
		[SerializeField]
		private bool _createRigidBody = false;                          // automatically create rigidbodys for each glyph

		[SerializeField]
		private float _rigidbodyMass = 1.0f;                            // the mass of this rigidbody

		[SerializeField]
		private float _rigidbodyDrag = 0.0f;                            // the drag value of this rigidbody

		[SerializeField]
		private float _rigidbodyAngularDrag = 0.05f;                    // the angular drag value of this rigidbody

		[SerializeField]
		private bool _rigidbodyUseGravity = false;                      // use gravity or not for this rigidbody

		[SerializeField]
		private bool _rigidbodyIsKinematic = false;                     // determines if this rigidbody is kinematic or not

		#endregion // RIGIDBODY

		#endregion // VARIABLES

		#region PROPERTIES
		#region COLLIDER
		/// <summary>
		/// the type of collider which is created for each glyph
		/// </summary>
		/// <value>the collider type created for each glyph </value>
		public ColliderType Collider
		{
			get
			{
				return _colliderType;
			}
			set
			{
				_colliderType = value;
			}
		}

		/// <summary>
		/// determines if this collider is a trigger or not
		/// </summary>
		/// <value> true if this collider is setup as a trigger </value>
		public bool ColliderIsTrigger
		{
			get
			{
				return _colliderIsTrigger;
			}
			set
			{
				_colliderIsTrigger = value;
			}
		}

		/// <summary>
		/// determines if this collider is a trigger or not
		/// </summary>
		/// <value> true if this collider is setup as a trigger </value>
		public bool ColliderIsConvex
		{
			get
			{
				return _colliderIsConvex;
			}
			set
			{
				_colliderIsConvex = value;
			}
		}

		/// <summary>
		/// the physics material of the collider
		/// </summary>
		/// <value>the collider type created for each glyph </value>
		public PhysicsMaterial ColliderMaterial
		{
			get
			{
				return _colliderMaterial;
			}
			set
			{
				_colliderMaterial = value;
			}
		}
		#endregion // COLLIDER

		#region RIGIDBODY 
		public bool CreateRigidBody
		{
			get { return _createRigidBody; }
			set
			{
				_createRigidBody = value;
			}
		}

		public float RigidbodyMass
		{
			get { return _rigidbodyMass; }
			set
			{
				_rigidbodyMass = value;
			}
		}

		public float RigidbodyDrag
		{
			get { return _rigidbodyDrag; }
			set
			{
				_rigidbodyDrag = value;
			}
		}

		public float RigidbodyAngularDrag
		{
			get { return _rigidbodyAngularDrag; }
			set
			{
				_rigidbodyAngularDrag = value;
			}
		}
		public bool RigidbodyUseGravity
		{
			get { return _rigidbodyUseGravity; }
			set
			{
				_rigidbodyUseGravity = value;
			}
		}

		public bool RigidbodyIsKinematic
		{
			get { return _rigidbodyIsKinematic; }
			set
			{
				_rigidbodyIsKinematic = value;
			}
		}
		#endregion // RIGIDBODY
		#endregion // PROPERTIES

		#region METHODS
		#endregion // METHODS
	}

	/// <summary>
	/// Additional components for each glyph
	/// </summary>
	[System.Serializable]
	public class VTextAdditionalComponents
	{
		#region VARIABLES
		[SerializeField]
		private GameObject _additionalComponentsObject;                 // a dummy gameobject which holds all components which should be added to each glyph

		#endregion // VARIABLES

		#region PROPERTIES
		public GameObject AdditionalComponentsObject
		{
			get { return _additionalComponentsObject; }

			set
			{
				_additionalComponentsObject = value;
			}
		}
		#endregion // PROPERTIES

		#region METHODS
		#endregion // METHODS
	}

	/// <summary>
	/// Virtence polygon text interface
	/// </summary>
	[ExecuteInEditMode]
	public class VTextInterface : MonoBehaviour
	{
		#region FIELDS
		[SerializeField]
		public VTextParameter parameter;                                // the mesh parameters

		[SerializeField]
		public VTextLayout layout;                                      // the layout parameters

		[SerializeField]
		public VTextPhysics Physics;                                    // the physics parameters

		[SerializeField]
		public VTextAdditionalComponents AdditionalComponents;          // the additional components for each glyp

		/// <summary>
		/// The text to render.
		/// might be overridden by external script for dynamic update.
		/// Line breaks by '\n'
		/// </summary>
		[SerializeField]
		public string RenderText = "Hello world";

		/// <summary>
		/// Select your Materials.
		/// The meshes produced will have valid uv.
		/// </summary>
		public Material[] materials = new Material[3];
		/// <summary>
		/// Workaround for the dynamic Batching error.
		/// </summary>
		[HideInInspector]
		public Material[] usedMaterials = new Material[3];
		#endregion // FIELDS

		#region CONSTRUCTORS
		public VTextInterface()
		{
			parameter = new VTextParameter();
			layout = new VTextLayout();
			Physics = new VTextPhysics();
			AdditionalComponents = new VTextAdditionalComponents();
		}
		#endregion // CONSTRUCTORS

		#region DESTRUCTORS
		~VTextInterface()
		{
		}
		#endregion // DESTRUCTORS

		#region METHODS
		#endregion // METHODS
	}


}