using UnityEngine;

namespace Virtence.VText
{
    /// <summary>
    /// the physics parameters for VText objects
    /// </summary>
    [System.Serializable]
    public class VTextPhysicsParameter
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

        [HideInInspector]
        private bool _modified = false;                                 // determine if the physics parameters are changed or not

        #region COLLIDER

        [SerializeField]
        private ColliderType _colliderType = ColliderType.None;         // the type of the collider which should be added to each glyph

        [SerializeField]
        private PhysicsMaterial _colliderMaterial = null;                // the physics material for the collider

        [SerializeField]
        private bool _colliderIsTrigger = false;                        // determines if this collider is a trigger or not

        [SerializeField]
        private bool _colliderIsConvex = false;                         // determines if this collider is convex or not (used for mesh colliders)

        #endregion COLLIDER

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

        #endregion RIGIDBODY

        #endregion VARIABLES

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
                if (value != _colliderType)
                {
                    _modified = true;
                }
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
                if (value != _colliderIsTrigger)
                {
                    _modified = true;
                }
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
                if (value != _colliderIsConvex)
                {
                    _modified = true;
                }
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
                if (value != _colliderMaterial)
                {
                    _modified = true;
                }
                _colliderMaterial = value;
            }
        }

        #endregion COLLIDER

        #region RIGIDBODY

        public bool CreateRigidBody
        {
            get
            {
                return _createRigidBody;
            }

            set
            {
                if (value != _createRigidBody)
                {
                    _modified = true;
                }
                _createRigidBody = value;
            }
        }

        public float RigidbodyMass
        {
            get
            {
                return _rigidbodyMass;
            }

            set
            {
                if (value != _rigidbodyMass)
                {
                    _modified = true;
                }
                _rigidbodyMass = value;
            }
        }

        public float RigidbodyDrag
        {
            get
            {
                return _rigidbodyDrag;
            }

            set
            {
                if (value != _rigidbodyDrag)
                {
                    _modified = true;
                }
                _rigidbodyDrag = value;
            }
        }

        public float RigidbodyAngularDrag
        {
            get
            {
                return _rigidbodyAngularDrag;
            }

            set
            {
                if (value != _rigidbodyAngularDrag)
                {
                    _modified = true;
                }
                _rigidbodyAngularDrag = value;
            }
        }

        public bool RigidbodyUseGravity
        {
            get
            {
                return _rigidbodyUseGravity;
            }

            set
            {
                if (value != _rigidbodyUseGravity)
                {
                    _modified = true;
                }
                _rigidbodyUseGravity = value;
            }
        }

        public bool RigidbodyIsKinematic
        {
            get
            {
                return _rigidbodyIsKinematic;
            }

            set
            {
                if (value != _rigidbodyIsKinematic)
                {
                    _modified = true;
                }
                _rigidbodyIsKinematic = value;
            }
        }

        #endregion RIGIDBODY

        #endregion PROPERTIES

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

        #endregion METHODS
    }
}