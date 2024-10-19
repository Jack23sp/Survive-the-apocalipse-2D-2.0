using UnityEngine;
using UnityEngine.Serialization;

namespace ScriptBoy.Fly2D
{
    [AddComponentMenu(" Script Boy/Fly 2D/ Fly Zone")]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class F2DFlyZone : MonoBehaviour
    {
        #region enums
        private enum CastingMethod
        {
            Circlecast, RaycastAndCirclecast
        }
        #endregion

        #region Variables
        [FormerlySerializedAs("meshRenderer")] [SerializeField] private MeshRenderer m_MeshRenderer;
        [FormerlySerializedAs("meshFilter")] [SerializeField] private MeshFilter m_MeshFilter;


        [FormerlySerializedAs("material")]
        [SerializeField] private Material m_Material;
        public Material material
        {
            get => m_Material;
            set
            {
                m_MeshRenderer.material = m_Material = value;
            }
        }

        [FormerlySerializedAs("sortingLayerID")]
        [SerializeField] private int m_SortingLayerID;
        [FormerlySerializedAs("sortingOrder")]
        [SerializeField] private int m_SortingOrder;


        private Mesh m_Mesh;
        private Vector3[] m_Vertices;
        private Vector2[] m_UV;


        private FlyParticle[] m_Flies;
        public FlyParticle[] flies => m_Flies;

        [FormerlySerializedAs("zoneCenter")]
        [SerializeField] private Transform m_ZoneCenter;
        public Transform zoneCenter
        {
            get => m_ZoneCenter;
            set { m_ZoneCenter = value; }
        }

        [FormerlySerializedAs("zoneRadius")]
        [SerializeField] private float m_ZoneRadius = 2;

        [FormerlySerializedAs("flyCount")]
        [SerializeField] public int m_FlyCount = 100;
        public int flyCount
        {
            get => m_FlyCount;
        }

        [FormerlySerializedAs("flySize")]
        [SerializeField] private float m_FlySize = 0.2f;
        [SerializeField] private int m_Seed;
        [FormerlySerializedAs("timeScale")]
        [SerializeField] private float m_TimeScale = 1;
        private float m_SimulationTime;


        [FormerlySerializedAs("collision")] [SerializeField] private bool m_Collision;
        [FormerlySerializedAs("visualizeCollision")] [SerializeField] private bool m_VisualizeCollision;
        [FormerlySerializedAs("collidesWith")] [SerializeField] private LayerMask m_CollidesWith;
        [FormerlySerializedAs("maxCollisionShapes")] [SerializeField] private int m_MaxCollisionShapes = 50;
        [FormerlySerializedAs("flyRadiusScale")] [SerializeField] private float m_FlyRadiusScale = 0.3f;
        [FormerlySerializedAs("collisionForceScale")] [SerializeField] private float m_CollisionForceScale = 1;
        [FormerlySerializedAs("castingMethod")] [SerializeField] private CastingMethod m_CastingMethod = CastingMethod.Circlecast;


        [FormerlySerializedAs("landing")] [SerializeField] private bool m_Landing;
        [FormerlySerializedAs("landingOn")] [SerializeField] private LayerMask m_LandingOn;
        [FormerlySerializedAs("landingMask")] [SerializeField] private F2DLandingMask m_LandingMask;
        [FormerlySerializedAs("flyingDuration")] [SerializeField] private float m_FlyingDuration = 5;
        [FormerlySerializedAs("landingDuration")] [SerializeField] private float m_LandingDuration = 5;
        [FormerlySerializedAs("landingCheckPerFrame")] [SerializeField] private int m_LandingCheckPerFrame = 30;
        private Collider2D[] m_CollidersForLanding;


        [FormerlySerializedAs("tiles")] [SerializeField] private Vector2Int m_Tiles = new Vector2Int(4, 4);
        private Vector2[] m_AnimationFrameUVs;
        [FormerlySerializedAs("idleFrameCount")] [SerializeField] private int m_IdleFrameCount = 1;
        [FormerlySerializedAs("flyFrameCount")] [SerializeField] private int m_FlyFrameCount = 15;
        [FormerlySerializedAs("framePerSecond")] [SerializeField] private int m_FramePerSecond = 60;
        private float m_AnimationTime;

        #endregion

        #region Unity Functions

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (m_Seed == 0) m_Seed = Random.Range(1000, 9999) + GetHashCode();

            m_FlyCount = Mathf.Clamp(m_FlyCount, 0, 5000000);

            m_ZoneRadius = Mathf.Clamp(m_ZoneRadius, 0, float.MaxValue);
            m_FlySize = Mathf.Clamp(m_FlySize, 0, float.MaxValue);
            m_TimeScale = Mathf.Clamp(m_TimeScale, 0, 100);
            m_FlyingDuration = Mathf.Clamp(m_FlyingDuration, 0, float.PositiveInfinity);
            m_LandingDuration = Mathf.Clamp(m_LandingDuration, 0, float.PositiveInfinity);
            m_MaxCollisionShapes = Mathf.Clamp(m_MaxCollisionShapes, 0, m_FlyCount);

            m_IdleFrameCount = Mathf.Clamp(m_IdleFrameCount, 0, m_Tiles.x * m_Tiles.y);
            m_FlyFrameCount = Mathf.Clamp(m_FlyFrameCount, 0, m_Tiles.x * m_Tiles.y - m_IdleFrameCount);

            if (!m_MeshFilter) m_MeshFilter = GetComponent<MeshFilter>();
            if (!m_MeshRenderer) m_MeshRenderer = GetComponent<MeshRenderer>();
            if (m_Material == null) m_Material = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/ScriptBoy/Fly2D/Materials/FlyMat.mat");

            m_MeshRenderer.material = m_Material;
            m_MeshRenderer.sortingOrder = m_SortingOrder;
            m_MeshRenderer.sortingLayerID = m_SortingLayerID;

            //m_MeshFilter.hideFlags = HideFlags.None;
            //m_MeshRenderer.hideFlags = HideFlags.None;

            m_Tiles.x = Mathf.Clamp(m_Tiles.x, 1, int.MaxValue);
            m_Tiles.y = Mathf.Clamp(m_Tiles.y, 1, int.MaxValue);

            if (m_AnimationFrameUVs == null || m_Tiles.x * m_Tiles.y * 4 != m_AnimationFrameUVs.Length)
            {
                InitializeAnimationFrameUVs();
            }

            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                F2DFlyZoneManager.AddFlyZone_WithoutCreatingInstance(this);
            }
        }
#endif

        private void Awake()
        {
            if (!m_MeshFilter) m_MeshFilter = GetComponent<MeshFilter>();
            if (!m_MeshRenderer) m_MeshRenderer = GetComponent<MeshRenderer>();

            m_MeshRenderer.material = m_Material;
        }

        private void Start()
        {
            InitializeFlyZone();
        }

        private void OnEnable()
        {
            F2DFlyZoneManager.AddFlyZone(this);
        }

        private void OnDisable()
        {
            F2DFlyZoneManager.RemoveFlyZone(this);
        }

        public void Update()
        {
#if UNITY_EDITOR
            if (m_Mesh == null || m_Flies == null || m_Flies.Length != m_FlyCount)
            {
                InitializeParticles();
                InitializeMesh();
                m_MeshFilter.mesh = m_Mesh;
            }
#endif
            _Update();
        }
        #endregion

        #region Init Functions
        private void InitializeFlyZone()
        {
            m_Seed = Random.Range(1000, 9999) + GetHashCode();
            InitializeParticles();
            InitializeAnimationFrameUVs();
            InitializeMesh();
            m_MeshFilter.mesh = m_Mesh;
        }

        private void InitializeParticles()
        {
            m_Flies = new FlyParticle[m_FlyCount];

            Vector3 center = (m_ZoneCenter == null) ? Vector3.zero : m_ZoneCenter.position;
            Vector3 forward = Vector3.forward;
            Vector3 right = Vector3.right;

            Random.State originalRandomState = Random.state;
            for (int i = 0; i < m_FlyCount; i++)
            {
                Random.InitState((int)m_SimulationTime + m_Seed + i);

                float angle = Random.Range(0, 360);
                right.x = Random.Range(0, m_ZoneRadius * 0.25f);

                m_Flies[i].localPosition = center + Quaternion.AngleAxis(angle, forward) * right;
            }
            Random.state = originalRandomState;
        }

        private void InitializeAnimationFrameUVs()
        {
            int frameCount = m_Tiles.x * m_Tiles.y;
            m_AnimationFrameUVs = new Vector2[frameCount * 4];

            float cellWidth = (float)1 / m_Tiles.x;
            float cellHeight = (float)1 / m_Tiles.y;

            int frame = 0;
            for (int v = m_Tiles.y - 1; v >= 0; v--)
            {
                for (int u = 0; u < m_Tiles.x; u++)
                {
                    int i = frame++ * 4;

                    m_AnimationFrameUVs[i] = new Vector2(u * cellWidth, v * cellHeight);
                    m_AnimationFrameUVs[i + 1] = new Vector2(u * cellWidth, v * cellHeight + cellHeight);
                    m_AnimationFrameUVs[i + 2] = new Vector2(u * cellWidth + cellWidth, v * cellHeight + cellHeight);
                    m_AnimationFrameUVs[i + 3] = new Vector2(u * cellWidth + cellWidth, v * cellHeight);
                }
            }
        }

        private void InitializeMesh()
        {
            m_Mesh = new Mesh();
            m_Mesh.hideFlags = HideFlags.DontSaveInBuild;

            m_Vertices = new Vector3[m_FlyCount * 4];
            m_UV = new Vector2[m_FlyCount * 4];
            int[] triangles = new int[m_FlyCount * 6];

            for (int i = 0; i < m_FlyCount; i++)
            {
                Vector3 position = m_Flies[i].localPosition;

                for (int j = 0; j < 4; j++)
                {
                    Vector3 po = Quad.vertices[j];
                    po.x = po.x * m_FlySize + position.x;
                    po.y = po.y * m_FlySize + position.y;
                    m_Vertices[i * 4 + j] = po;
                    m_UV[i * 4 + j] = m_AnimationFrameUVs[j];
                }

                for (int j = 0; j < 6; j++)
                {
                    triangles[i * 6 + j] = Quad.triangles[j] + i * 4;
                }
            }

            m_Mesh.vertices = m_Vertices;
            m_Mesh.uv = m_UV;
            m_Mesh.triangles = triangles;
        }
        #endregion


        private void FreezeTranform()
        {
            transform.position = Vector3.zero;
            transform.localScale = Vector3.one;
            transform.rotation = Quaternion.identity;
        }

        private void UpdateTime()
        {
            float deltaTime = Time.deltaTime * m_TimeScale;
            m_SimulationTime += deltaTime;
            m_AnimationTime += deltaTime * m_FramePerSecond;
        }

        private void _Update()
        {
            FreezeTranform();
            UpdateTime();
            UpdateParticles();
            UpdateMesh();
        }

        #region Fly Functions
        private void UpdateParticles()
        {
            float deltaTime = Time.deltaTime * m_TimeScale;

            Vector3 center = (m_ZoneCenter == null) ? Vector3.zero : m_ZoneCenter.position;
            Vector3 forward = Vector3.forward;
            Vector3 right = Vector3.right;

            float minRadius = m_ZoneRadius * 0.4f;

            Random.State originalRandomState = Random.state;
            for (int i = 0; i < m_FlyCount; i++)
            {
                m_Flies[i].time += deltaTime;

                if (m_Flies[i].landing)
                {
                    if (m_Flies[i].time > m_LandingDuration) m_Flies[i].StopLanding();
                }
                else
                {
                    Random.InitState((int)(m_SimulationTime) + m_Seed + i * 8);

                    float angle = Random.Range(0, 360);
                    right.x = Random.Range(minRadius, m_ZoneRadius);
                    Vector3 randomVector = Quaternion.AngleAxis(angle, forward) * right;

                    Vector3 position = m_Flies[i].localPosition;
                    Vector3 velocity = m_Flies[i].velocity;

                    velocity.x += (center.x - position.x - velocity.x + randomVector.x) * deltaTime;
                    velocity.y += (center.y - position.y - velocity.y + randomVector.y) * deltaTime;

                    position.x += velocity.x * deltaTime;
                    position.y += velocity.y * deltaTime;


                    float zAngle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;

                    m_Flies[i].localPosition = position;
                    m_Flies[i].velocity = velocity;
                    m_Flies[i].localRotation = Quaternion.AngleAxis(zAngle, forward);
                }
            }
            Random.state = originalRandomState;


            if (m_LandingMask == null)
            {
                float sqrRadius = m_ZoneRadius * m_ZoneRadius;
                for (int i = 0; i < m_FlyCount; i++)
                {
                    if (m_Flies[i].landing && (m_Flies[i].Position - center).sqrMagnitude > sqrRadius)
                        m_Flies[i].StopLanding();
                }
            }
            else
            {
                m_LandingMask.UpdateFlies(m_Flies);
            }

            if (m_Landing) CheckLanding();
            if (m_Collision) CheckCollision();
        }

        private void UpdateMesh()
        {
            int order = 0;

            int idleStartIndex = 0;
            int idleEndIndex = idleStartIndex + m_IdleFrameCount - 1;
            int idleFrameIndex = (m_IdleFrameCount == 0) ? 0 : Mod.get((int)m_AnimationTime, m_IdleFrameCount) + idleStartIndex;

            for (int i = 0; i < m_FlyCount; i++)
            {
                if (m_Flies[i].landing)
                {
                    Vector3 position = m_Flies[i].parent.TransformPoint(m_Flies[i].localPosition);
                    Quaternion rotation = m_Flies[i].parent.rotation * m_Flies[i].localRotation;

                    for (int j = 0; j < 4; j++)
                    {
                        Vector3 po = rotation * Quad.vertices[j];
                        po.x = po.x * m_FlySize + position.x;
                        po.y = po.y * m_FlySize + position.y;
                        m_Vertices[order * 4 + j] = po;
                        m_UV[order * 4 + j] = m_AnimationFrameUVs[j + idleFrameIndex * 4];
                    }
                    order++;
                }
            }

            int flyStartIndex = idleEndIndex + 1;
            int flyEndIndex = flyStartIndex + m_FlyFrameCount - 1;
            int flyFrameIndex = (m_FlyFrameCount == 0) ? 0 : Mod.get((int)m_AnimationTime, m_FlyFrameCount) + flyStartIndex;

            for (int i = 0; i < m_FlyCount; i++)
            {
                if (!m_Flies[i].landing)
                {
                    Vector3 position = m_Flies[i].localPosition;
                    Quaternion rotation = m_Flies[i].localRotation;

                    for (int j = 0; j < 4; j++)
                    {
                        Vector3 po = rotation * Quad.vertices[j];
                        po.x = po.x * m_FlySize + position.x;
                        po.y = po.y * m_FlySize + position.y;
                        m_Vertices[order * 4 + j] = po;
                        m_UV[order * 4 + j] = m_AnimationFrameUVs[j + flyFrameIndex * 4];
                    }
                    order++;
                }
            }

            m_Mesh.vertices = m_Vertices;
            m_Mesh.uv = m_UV;
            m_Mesh.RecalculateBounds();
        }
        #endregion

        #region Physics
        private void CheckCollision()
        {
            int layerMask = m_CollidesWith.value;
            float radius = m_FlySize * m_FlyRadiusScale;
            float deltaTime = Time.deltaTime;

            bool onlyCircleCast = m_CastingMethod == CastingMethod.Circlecast;

            for (int i = 0, j = 0; i < m_FlyCount && j < m_MaxCollisionShapes; i++)
            {
                if (!m_Flies[i].landing)
                {
                    float vMagnitude = m_Flies[i].velocity.magnitude;
                    Vector2 po = m_Flies[i].localPosition;

                    RaycastHit2D hit;
                    bool useCircleCast = onlyCircleCast || m_Flies[i].overlaping;

                    if (useCircleCast)
                    {
                        hit = Physics2D.CircleCast(m_Flies[i].localPosition, radius, m_Flies[i].velocity, vMagnitude * 0.5f, layerMask);
                    }
                    else
                    {
                        hit = Physics2D.Raycast(po, m_Flies[i].velocity, vMagnitude * 0.5f, layerMask);
                    }

                    if (hit)
                    {
#if UNITY_EDITOR
                        if (m_VisualizeCollision)
                            Debug.DrawLine(m_Flies[i].localPosition, hit.point, Color.red, Time.deltaTime / m_TimeScale);
#endif

                        Vector2 normal = hit.normal;

                        bool overlaping = hit.distance < float.Epsilon;
                        m_Flies[i].overlaping = overlaping;


                        if (!overlaping || useCircleCast)
                        {
                            m_Flies[i].velocity.x = m_Flies[i].velocity.x + normal.x * m_CollisionForceScale * vMagnitude * deltaTime;
                            m_Flies[i].velocity.y = m_Flies[i].velocity.y + normal.y * m_CollisionForceScale * vMagnitude * deltaTime;
                        }
                    }
                    else
                    {
                        m_Flies[i].overlaping = false;
                    }

                    j++;
                }
            }
        }

        private void CheckLanding()
        {
            if (m_LandingCheckPerFrame > 0)
            {
                Vector2 center = (m_ZoneCenter == null) ? Vector3.zero : m_ZoneCenter.position;
                if (m_CollidersForLanding == null) m_CollidersForLanding = new Collider2D[32];
                int collidersCount = Physics2D.OverlapCircleNonAlloc(center, m_ZoneRadius * 1.5f, m_CollidersForLanding, m_LandingOn);

                int startIndex = (int)Mod.get(m_SimulationTime * m_LandingCheckPerFrame, m_FlyCount);
                for (int i = 0; i < collidersCount; i++)
                {
                    Collider2D collider = m_CollidersForLanding[i];

                    if (collider.isTrigger) continue;

                    for (int j = 0; j < m_LandingCheckPerFrame; j++)
                    {
                        int flyIndex = Mod.get(startIndex + j, m_FlyCount);

                        if (!m_Flies[flyIndex].landing && m_Flies[flyIndex].time > m_FlyingDuration && collider.OverlapPoint(m_Flies[flyIndex].localPosition))
                        {
                            m_Flies[flyIndex].StartLanding(collider.transform);
                        }
                    }
                }
            }
        }
        #endregion

        #region Public Functions
        public void RefreshZone()
        {
            Vector3 center = m_ZoneCenter.position;
            Vector3 forward = Vector3.forward;
            Vector3 right = Vector3.right;

            Random.State originalRandomState = Random.state;
            for (int i = 0; i < m_FlyCount; i++)
            {
                m_Flies[i].time = 0;
                if (m_Flies[i].landing) m_Flies[i].StopLanding();

                Random.InitState((int)m_SimulationTime + m_Seed + i * 10);
                right.x = Random.Range(0, m_ZoneRadius);
                Vector3 randomVector = Quaternion.AngleAxis(UnityEngine.Random.Range(0, 360), forward) * right;
                m_Flies[i].localPosition = center + randomVector;
            }
        }
        #endregion

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            bool selected = UnityEditor.Selection.Contains(gameObject);

            if (m_ZoneCenter)
            {
                UnityEditor.Handles.color = (selected) ? Color.yellow : Color.white;
                UnityEditor.Handles.CircleHandleCap(0, m_ZoneCenter.position, Quaternion.identity, m_ZoneRadius, EventType.Repaint);
            }

            if (m_Flies != null)
            {
                if (selected)
                {
                    if (m_Collision && m_VisualizeCollision)
                    {
                        UnityEditor.Handles.color = new Color(0, 1, 0, 0.2f);
                        for (int i = 0, j = 0; i < m_FlyCount && j < m_MaxCollisionShapes; i++)
                        {
                            if (!m_Flies[i].landing)
                            {
                                UnityEditor.Handles.CircleHandleCap(0, m_Flies[i].Position, Quaternion.identity, m_FlySize * m_FlyRadiusScale, EventType.Repaint);
                                UnityEditor.Handles.DrawLine(m_Flies[i].Position, m_Flies[i].Position + m_Flies[i].velocity * 0.5f);
                                j++;
                            }
                        }
                    }
                }
            }
        }
#endif
    }
}