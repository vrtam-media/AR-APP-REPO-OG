// ----------------------------------------------------------------------
// File: 			VTextInterfaceToVTextConverter
// Organisation: 	Virtence GmbH
// Department:   	Simulation Development
// Copyright:    	© 2017 Virtence GmbH. All rights reserved
// Author:       	Artur Bullert (artur.bullert@virtence.com)
// ----------------------------------------------------------------------

using System;
using UnityEngine;

namespace Virtence.VText.LEGACY
{
	/// <summary>
	/// 
	/// </summary>
	public class VTextInterfaceToVTextConverter 
	{
		#region EVENTS
		#endregion // EVENTS


		#region CONSTANTS
		#endregion // CONSTANTS


		#region FIELDS
		private VTextInterface _oldVText;
		private VText _newVText;
		#endregion // FIELDS


		#region PROPERTIES
		#endregion // PROPERTIES


		#region CONSTRUCTORS
		#endregion // CONSTRUCTORS


		#region METHODS
		/// <summary>
		/// converts the specified VTextInterface into the new VText
		/// </summary>
		/// <param name="oldVText"></param>
		public void DoConvert(VTextInterface oldVText) {
			if (oldVText == null)
			{
				Debug.LogWarning("old vtextinterface is null");
				return;
			}

			_oldVText = oldVText;

			_newVText = _oldVText.GetComponent<VText>();
			if (_newVText != null)
			{
				Debug.LogWarning(string.Format("There is a new VText-Monobehavior on this gameobject already: '{0}'", _oldVText.name));
				return;
			}
			else
			{
				_newVText = _oldVText.gameObject.AddComponent<VText>();
			}

			UpdateMeshParameters();
			UpdateLayoutParameters();
			UpdateRenderParameters();
			UpdatePhysicParameters();
			UpdateAdditionalComponents();
		}

		/// <summary>
		/// update the mesh paramters 
		/// </summary>
		private void UpdateMeshParameters() {
			_newVText.MeshParameter.FontName = _oldVText.parameter.Fontname;
			_newVText.MeshParameter.Text = _oldVText.RenderText;
			_newVText.MeshParameter.Bevel = _oldVText.parameter.Bevel;
			_newVText.MeshParameter.Depth = _oldVText.parameter.Depth;
			_newVText.MeshParameter.GenerateTangents = _oldVText.parameter.GenerateTangents;
			_newVText.MeshParameter.HasBackface = _oldVText.parameter.Backface;
			_newVText.MeshParameter.Resolution = (float) _oldVText.parameter.Quality / 1000.0f;

			/*
			 * nobody knows:
			_newVText.MeshParameter.UseBevelProfile;
			_newVText.MeshParameter.UseFaceUVs;
			*/
		}

		/// <summary>
		/// update the layout parameters
		/// </summary>
		private void UpdateLayoutParameters() {
			_newVText.LayoutParameter.AnimateRadius = _oldVText.layout.AnimateRadius;
			_newVText.LayoutParameter.CircleRadius = _oldVText.layout.CircleRadius;
			_newVText.LayoutParameter.CurveRadius = _oldVText.layout.CurveRadius;
			_newVText.LayoutParameter.CurveXY = _oldVText.layout.CurveXY;
			_newVText.LayoutParameter.CurveXZ = _oldVText.layout.CurveXZ;
			_newVText.LayoutParameter.EndRadius = _oldVText.layout.EndRadius;
			_newVText.LayoutParameter.GlyphSpacing = _oldVText.layout.GlyphSpacing;
			_newVText.LayoutParameter.IsHorizontal = _oldVText.layout.Horizontal;
			_newVText.LayoutParameter.Major = (Align) _oldVText.layout.Major;
			_newVText.LayoutParameter.Minor = (Align) _oldVText.layout.Minor;
			_newVText.LayoutParameter.OrientationCircular = _oldVText.layout.OrientationCircular;
			_newVText.LayoutParameter.OrientationXY = _oldVText.layout.OrientationXY;
			_newVText.LayoutParameter.OrientationXZ = _oldVText.layout.OrientationXZ;
			_newVText.LayoutParameter.Size = _oldVText.layout.Size;
			_newVText.LayoutParameter.Spacing = _oldVText.layout.Spacing;
			_newVText.LayoutParameter.StartRadius = _oldVText.layout.StartRadius;			
		}

		/// <summary>
		/// update the render parameters
		/// </summary>
		private void UpdateRenderParameters() {
#if UNITY_2018_3_OR_NEWER
			_newVText.RenderParameter.LightProbeUsage = _oldVText.parameter.UseLightProbes ? UnityEngine.Rendering.LightProbeUsage.BlendProbes : UnityEngine.Rendering.LightProbeUsage.Off;
#else
			_newVText.RenderParameter.UseLightProbes = _oldVText.parameter.UseLightProbes;			
#endif
			_newVText.RenderParameter.Materials[(int) GlyphParts.FrontFace] = _oldVText.materials[(int) GlyphParts.FrontFace];
			_newVText.RenderParameter.Materials[(int) GlyphParts.Bevel] = _oldVText.materials[(int) GlyphParts.Bevel];
			_newVText.RenderParameter.Materials[(int) GlyphParts.Side] = _oldVText.materials[(int) GlyphParts.Side];

			_newVText.RenderParameter.ReceiveShadows = _oldVText.parameter.ReceiveShadows;
			_newVText.RenderParameter.ShadowCastMode = _oldVText.parameter.ShadowCastMode;
		}

		/// <summary>
		/// update the physic parameters
		/// </summary>
		private void UpdatePhysicParameters() {
			_newVText.PhysicsParameter.Collider = (Virtence.VText.VTextPhysicsParameter.ColliderType) _oldVText.Physics.Collider;
			_newVText.PhysicsParameter.ColliderIsConvex = _oldVText.Physics.ColliderIsConvex;
			_newVText.PhysicsParameter.ColliderIsTrigger = _oldVText.Physics.ColliderIsTrigger;
			_newVText.PhysicsParameter.ColliderMaterial = _oldVText.Physics.ColliderMaterial;
			_newVText.PhysicsParameter.CreateRigidBody = _oldVText.Physics.CreateRigidBody;
			_newVText.PhysicsParameter.RigidbodyAngularDrag = _oldVText.Physics.RigidbodyAngularDrag;
			_newVText.PhysicsParameter.RigidbodyDrag = _oldVText.Physics.RigidbodyDrag;
			_newVText.PhysicsParameter.RigidbodyIsKinematic = _oldVText.Physics.RigidbodyIsKinematic;
			_newVText.PhysicsParameter.RigidbodyMass = _oldVText.Physics.RigidbodyMass;
			_newVText.PhysicsParameter.RigidbodyUseGravity = _oldVText.Physics.RigidbodyUseGravity;			
		}

		/// <summary>
		/// update the additional components
		/// </summary>
		private void UpdateAdditionalComponents() {
			_newVText.AdditionalComponents.AdditionalComponentsObject = _oldVText.AdditionalComponents.AdditionalComponentsObject;
		}
		#endregion // METHODS


		#region EVENT HANDLERS		
		#endregion // EVENT HANDLERS
	}
}