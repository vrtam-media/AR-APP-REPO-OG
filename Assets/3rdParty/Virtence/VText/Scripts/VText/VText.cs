using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Virtence.VText.Extensions;

namespace Virtence.VText
{
    /// <summary>
    /// Main Frontend of VText
    /// </summary>
    [ExecuteInEditMode]
    public sealed class VText : MonoBehaviour
    {

        #region EVENT HANDLERS
        /// <summary>
        /// Event thrown when VText is done rendering and layouting
        /// </summary>
        public event EventHandler TextRenderingFinished;

        /// <summary>
        /// event which is thrown if the font loading failed
        /// </summary>
        public event EventHandler<GenericEventArgs<string>> FontLoadingFailed;
        #endregion // EVENT HANDLERS

        #region EXPOSED

        // the layout parameters
        [SerializeField]
        public VTextLayoutParameter LayoutParameter = new VTextLayoutParameter();

        // the mesh parameters
        [SerializeField]
        public VTextMeshParameter MeshParameter = new VTextMeshParameter();

        // the render parameters
        [SerializeField]
        public VTextRenderParameter RenderParameter = new VTextRenderParameter();

        // the physics parameters
        [SerializeField]
        public VTextPhysicsParameter PhysicsParameter = new VTextPhysicsParameter();

        // the additional components for each glyph
        [SerializeField]
        public VTextAdditionalComponents AdditionalComponents = new VTextAdditionalComponents();

        #endregion EXPOSED

        #region FIELDS

        private VTextBuilder _builder = null;                   // stupid sexy text builder

        private List<MonoBehaviour> m_changeListener = null;    // for additional components

        /// <summary>
        /// the list of component-types on the glyphs which should not be overwritten or deleted
        /// </summary>
        private List<Type> _componentsToKeep = new List<Type>() {
            typeof(Transform),
            typeof(Renderer),
            typeof(MeshFilter),
            typeof(Rigidbody),
            typeof(Collider),
        };

        #endregion FIELDS

        #region CONSTRUCTOR

        public VText()
        {
            MeshParameter = new VTextMeshParameter();
            RenderParameter = new VTextRenderParameter();
            LayoutParameter = new VTextLayoutParameter();
            PhysicsParameter = new VTextPhysicsParameter();
            AdditionalComponents = new VTextAdditionalComponents();
        }

        #endregion CONSTRUCTOR

        #region PROPERTIES

        /// <summary>
        /// our mighty text builder.
        /// </summary>
        private VTextBuilder _textBuilder
        {
            get
            {
                return _builder = _builder ?? new VTextBuilder(transform, MeshParameter, RenderParameter, LayoutParameter);
            }
        }

        #endregion PROPERTIES

        #region METHODS

        private void Start()
        {
            _textBuilder.FontLoadingFailed += OnFontLoadingFailed;
            Rebuild();
        }

        private void Update()
        {
            CheckUpdate();
        }

        public void CheckUpdate()
        {
            bool isRebuild = MeshParameter.CheckClearRebuild();
            isRebuild |= LayoutParameter.CheckClearRebuild();
            if (isRebuild)
            {
                Rebuild();
                return;
            }
            bool isModified = MeshParameter.CheckClearModified();
            isModified |= LayoutParameter.CheckClearModified();
            isModified |= RenderParameter.CheckClearModified();
            if (isModified)
            {
                Modify();
            }
        }

        /// <summary>
        /// convinience function to set the text
        /// </summary>
        /// <param name="text"></param>
        public void SetText(string text) {
            if (MeshParameter != null) {
                MeshParameter.Text = text;
            }
        }

        /// <summary>
        /// Rebuild complete stuff
        /// </summary>
        public void Rebuild()
        {
			Action<bool> finished = success =>
			{
				if (success)
				{
					UpdatePhysics();
					UpdateAdditionalComponents();
					if (null != m_changeListener)
					{
						foreach (var mb in m_changeListener)
						{
							mb.SendMessage("VTextChanged");
						}
					}
					TextRenderingFinished?.Invoke(this, null);
				}
				else {
                    //Rebuild();
                    //Debug.Log(string.Format("Could not read font '{0}'", MeshParameter.FontName));
					CheckUpdate();
				}
			};
            _textBuilder.Rebuild(finished);
        }

        /// <summary>
        /// Update existing children
        /// </summary>
        public void Modify()
        {
			Action<bool> finished = success =>
			{
				if (success)
				{
                    UpdatePhysics();
                    UpdateAdditionalComponents();
                    if (null != m_changeListener)
					{
						foreach (var mb in m_changeListener)
						{
							mb.SendMessage("VTextChanged");
						}
                    }
                    TextRenderingFinished?.Invoke(this, null);
                }
			};
			_textBuilder.Modify(finished);
        }

        #region physics

        /// <summary>
        /// create or remove the rigidbody component
        /// </summary>
        /// <param name="t"> the transform to change</param>
        private void CreateRigidbody(Transform t)
        {
            if (PhysicsParameter.CreateRigidBody)
            {
                Rigidbody rigidBody = t.GetComponent<Rigidbody>();
                if (rigidBody == null)
                {
                    rigidBody = t.gameObject.AddComponent<Rigidbody>();
                }
                rigidBody.useGravity = PhysicsParameter.RigidbodyUseGravity;
                rigidBody.mass = PhysicsParameter.RigidbodyMass;
                rigidBody.linearDamping = PhysicsParameter.RigidbodyDrag;
                rigidBody.angularDamping = PhysicsParameter.RigidbodyAngularDrag;
                rigidBody.isKinematic = PhysicsParameter.RigidbodyIsKinematic;
            }
            else
            {
#if UNITY_EDITOR
                DestroyImmediate(t.GetComponent<Rigidbody>());
#else
                Destroy(t.GetComponent<Rigidbody>());
#endif
            }
        }

        /// <summary>
        /// create or remove the collider on the specified transform
        /// </summary>
        /// <param name="t"> the transform to modify </param>
        private void CreateCollider(Transform t)
        {
            switch (PhysicsParameter.Collider)
            {
                case VTextPhysicsParameter.ColliderType.None:
                    RemoveCollider(t);
                    break;

                case VTextPhysicsParameter.ColliderType.Box:
                    RemoveCollider(t);
                    BoxCollider bc = t.gameObject.AddComponent<BoxCollider>();
                    bc.material = PhysicsParameter.ColliderMaterial;
                    bc.isTrigger = PhysicsParameter.ColliderIsTrigger;
                    break;

                case VTextPhysicsParameter.ColliderType.Mesh:
                    RemoveCollider(t);
                    MeshCollider mc = t.gameObject.AddComponent<MeshCollider>();
                    mc.material = PhysicsParameter.ColliderMaterial;
                    mc.convex = PhysicsParameter.ColliderIsConvex;
                    if (mc.convex)
                    {
                        mc.isTrigger = PhysicsParameter.ColliderIsTrigger;
                    }
                    else
                    {
                        mc.isTrigger = false;
                    }
                    break;
            }
        }

        /// <summary>
        /// Removes the collider from the specified transform.
        /// </summary>
        /// <param name="t">T.</param>
        private void RemoveCollider(Transform t)
        {
            Collider[] colliders = t.GetComponents<Collider>();
            foreach (Collider c in colliders)
            {
#if UNITY_EDITOR
                DestroyImmediate(t.GetComponent<Collider>());
#else
                Destroy(t.GetComponent<Collider>());
#endif
            }
        }

        /// <summary>
        /// update the physics aspects of the vtext
        /// </summary>
        public void UpdatePhysics()
        {
			if (this == null)
				return;

            for (int i = 0; i < transform.childCount; i++)
            {
                Transform t = transform.GetChild(i);

                CreateRigidbody(t);
                CreateCollider(t);
            }
        }

        #endregion physics

        #region additionalcomponents

        public void RegisterListener(MonoBehaviour go)
        {
            if (null == m_changeListener)
            {
                m_changeListener = new List<MonoBehaviour>();
            }
            m_changeListener.Add(go);
        }

        public void UnRegisterListener(MonoBehaviour go)
        {
            if (null != m_changeListener)
            {
                if (m_changeListener.Contains(go))
                {
                    m_changeListener.Remove(go);
                }
            }
        }

        /// <summary>
        /// update the additional components
        /// </summary>
        public void UpdateAdditionalComponents()
        {
            if (AdditionalComponents.AdditionalComponentsObject != null)
            {
                // foreach glyph
                for (int i = 0; i < transform.childCount; i++)
                {
                    Transform t = transform.GetChild(i);

                    // remove all existing MonoBehaviours of the glyph
                    Component[] components = t.GetComponents<Component>();
                    foreach (Component c in components)
                    {
                        if (_componentsToKeep.Any(type => type.IsAssignableFrom(c.GetType())))
                        {
                            continue;
                        }

                        if (Application.isPlaying)
                        {
                            Destroy(c);
                        }
                        else
                        {
                            DestroyImmediate(c);
                        }
                    }

                    // now add copies of the new behaviours
                    Component[] componentsToAdd = AdditionalComponents.AdditionalComponentsObject.GetComponents<Component>();
                    foreach (Component c in componentsToAdd)
                    {
                        if (_componentsToKeep.Any(type => type.IsAssignableFrom(c.GetType())))
                        {
                            continue;
                        }
                        t.gameObject.AddComponentClone(c);
                    }
                }
            }
        }

        #endregion additionalcomponents

        #region HELPER_FUNCIONS
        public Bounds GetBounds() {
            if (this == null)
                return new Bounds();

            Bounds result = new Bounds();
            MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();


            if (null != renderers && renderers.Length > 0)
            {
                foreach (Renderer r in renderers) {
                    result.Encapsulate(r.bounds);
                }
            }
            return result;
        }
        #endregion // HELPER_FUNCTIONS

        #endregion METHODS

        #region EVENTHANDLER

        /// <summary>
        /// this is called if the specified font could not be loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFontLoadingFailed(object sender, GenericEventArgs<string> e)
        {
            FontLoadingFailed?.Invoke(this, new GenericEventArgs<string>(e.Value));
        }

        #endregion // EVENTHANDLER
    }
}