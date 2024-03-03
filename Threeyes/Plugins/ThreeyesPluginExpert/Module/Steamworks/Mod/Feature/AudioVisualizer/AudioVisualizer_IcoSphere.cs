using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using Threeyes.Persistent;
using Newtonsoft.Json;
using NaughtyAttributes;
using UnityEngine.Events;
using static Threeyes.Steamworks.AudioVisualizer_IcoSphere.ConfigInfo;
using Threeyes.Core;

namespace Threeyes.Steamworks
{
    /// <summary>
    /// Auto generate icosphere base on config, which can response to audio
    /// 
    /// Ref：Samples/ProBuilder/X.X.X/Runtime Examples/Icosphere FFT
    /// 
    /// PS：
    /// -Waveform：
    ///     -阴影：如果无法显示阴影，则将CastShadows改为TwoSided
    /// -Shell（外壳，也就是最大显示区域）（如玻璃外壳，可空）：
    ///     -半径=sphereRadius + idleExtrusion + maxExtrusion
    ///     
    /// Todo:
    /// -增加非运行模式下初始化默认的Mesh（非必须）及Waveform
    /// -Shell作为外边界：可选是否考虑maxExtrusion，如果是则基于Shell半径+Pivot计算锚点，并且用Event通知外部显隐相关物体（如显隐Shell的MeshRenderer，以及缩放Shell（主要利用其碰撞体））
    /// -不新建Mesh对应的新物体，而是基于已有物体，方便MaterialController引用并修改该材质（因为多个实例，不可能修改同一个材质）（如果不行就引用MaterialController，并手动更新其值）
    /// </summary>
    public class AudioVisualizer_IcoSphere : ConfigurableComponentBase<AudioVisualizer_IcoSphere, SOAudioVisualizer_IcoSphereConfig, AudioVisualizer_IcoSphere.ConfigInfo, AudioVisualizer_IcoSphere.PropertyBag>
    , IHubSystemAudio_RawSampleDataChangedHandler
    , IHubSystemAudio_SpectrumDataChangedHandler
    , IModHandler
    {
        #region Property & Field
        const float twoPi = 6.283185f;
        public IHubSystemAudioManager Manager { get { return ManagerHolder.SystemAudioManager; } }
        Transform TfParent { get { return tfParent ? tfParent : transform; } }
        float parentLossyScale { get { return TfParent.lossyScale.x; } }//父物体的全局缩放，决定Sphere和Waveform的大小

        public Transform tfParent;//The parent of mesh and waveform
        public AnimationCurve frequencyCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));// Optionally weights the frequency amplitude when calculating extrude distance.（PS：因为RuntimeEditor暂不支持曲线，且非必须，所以放在外部）
        public LineRenderer waveform;// A reference to the line renderer that will be used to render the raw waveform.
        public Pivot pivot = Pivot.Center;// The pivot of the Mesh
        /// ToAdd:
        /// -字段：isPivotBaseOnShellSize，

        public Vector3Event onLocalSphereSizeChanged = new Vector3Event();//球（idle状态）的尺寸发生变化
        public Vector3Event onLocalShellSizeChanged = new Vector3Event();//球的外壳尺寸发生变化（idle半径+[可选]最大显示范围，不包括Waveform）

        //Runtime
        public Transform tfShell;//Shell GameObject（外壳，可为其挂载SphereCollider从而处理碰撞检测，可选其半径是否包括maxExtrusion）
        public Transform tfIcoSphere;//IcoSphere GameObject（用于缓存运行时生成的Mesh数据，可以为其提前设置材质）
        ProBuilderMesh m_ProBuilderMesh;
        Mesh m_UnityMesh;// A reference to the MeshFilter.sharedMesh
        float m_FaceLength;
        int waveformPointCount;//Waveform的点数量
        ExtrudedSelection[] m_AnimatedSelections;// All faces that have been extruded       
        Vector3[] m_OriginalVertexPositions, m_DisplacedVertexPositions;// Keep a copy of the original vertex array to calculate the distance from origin.
        #endregion

        #region Unity Method
        protected override void Awake()
        {
            base.Awake();
            Config.actionMeshGenerateSettingChanged += OnMeshGenerateSettingChanged;
            Config.actionWaveformGenerateSettingChanged += OnWaveformGenerateSettingChanged;
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            Config.actionMeshGenerateSettingChanged -= OnMeshGenerateSettingChanged;
            Config.actionWaveformGenerateSettingChanged -= OnWaveformGenerateSettingChanged;
        }
        protected virtual void OnEnable()
        {
            ManagerHolder.SystemAudioManager.Register(this);
        }
        protected virtual void OnDisable()
        {
            ManagerHolder.SystemAudioManager.UnRegister(this);
        }

        void Update()
        {
            //#Ring
            if (Config.showWaveform)
            {
                UpdateWaveformSetting();//更新Gradient

                if (Config.rotateWaveformRing)
                    waveform.transform.Rotate(Config.waveformRotateSpeed /** 360 */* Time.deltaTime, Space.Self);
            }
        }
        #endregion

        #region IModHandler
        public override void OnModInit()
        {
            //初始化需要更新（PS:因为模拟器不会调用以下回调方法，所以要手动调用（这类方法多次调用不影响性能）)
            OnMeshGenerateSettingChanged(PersistentChangeState.Load);
            OnWaveformGenerateSettingChanged(PersistentChangeState.Load);

            base.OnModInit();
        }

        //——以下字段及方法是为了标记指定字段是否被修改，从而在UpdateSetting中调用对应的耗时重建方法——
        bool hasMeshGenerateSettingChanged = false;//Mark has changed
        bool hasWaveformGenerateSettingChanged = false;//Mark has changed
        void OnMeshGenerateSettingChanged(PersistentChangeState persistentChangeState)
        {
            hasMeshGenerateSettingChanged = true;
        }
        void OnWaveformGenerateSettingChanged(PersistentChangeState persistentChangeState)
        {
            hasWaveformGenerateSettingChanged = true;
        }

        /// <summary>
        /// PS：
        /// -调用入口：OnPersistentChanged
        /// </summary>
        public override void UpdateSetting()
        {
            ///当且只有当Mesh相关设置有更改才重新创建
            if (hasMeshGenerateSettingChanged)
                ReGenerateSphere();

            if (hasWaveformGenerateSettingChanged)
                ReGenerateWaveform(Manager.RawSampleCount);

            //Reset State
            hasMeshGenerateSettingChanged = false;
            hasWaveformGenerateSettingChanged = false;
        }
        #endregion

        #region IRuntimeEditable
        protected override void RuntimeEditableRegisterDataEvent()
        {
            base.RuntimeEditableRegisterDataEvent();
            Config.actionMeshGenerateSettingChanged -= OnMeshGenerateSettingChanged;
            Config.actionMeshGenerateSettingChanged += OnMeshGenerateSettingChanged;
            Config.actionWaveformGenerateSettingChanged -= OnWaveformGenerateSettingChanged;
            Config.actionWaveformGenerateSettingChanged += OnWaveformGenerateSettingChanged;
        }
        #endregion

        #region Callback
        public virtual void OnRawSampleDataChanged(float[] rawData)//RawSampleData：更新曲线
        {
            //——Todo：根据 waveformPointCount 计算值
            if (!Config.showWaveform)
                return;

            Vector3 pos = Vector3.zero;
            float fTotalCount = (float)waveformPointCount;//Warning:后续需要使用float进行小数点计算，因此转为float
            for (int waveformpointIndex = 0; waveformpointIndex != waveformPointCount; waveformpointIndex++)
            {
                int rawDataIndex = waveformpointIndex < waveformPointCount - 1 ? waveformpointIndex : 0;//根据waveform点Index计算出对应的rawDataIndex。如果i是最后一点，则确保其值与首位相同，以保证首位相连

                rawDataIndex *= (int)Config.waveformDownSample;//原理：waveform点序号×采样值=rawData对应点的序号

                float travel = Config.waveformRadius + Config.waveformHeight * rawData[rawDataIndex];
                pos.x = Mathf.Cos(waveformpointIndex / fTotalCount * twoPi) * travel;
                pos.z = Mathf.Sin(waveformpointIndex / fTotalCount * twoPi) * travel;
                pos.y = 0f;

                waveform.SetPosition(waveformpointIndex, pos);
            }
        }
        float[] lastFrameSpectrumdata = new float[] { };
        public virtual void OnSpectrumDataChanged(float[] data)//SpectrumData：更新面
        {
            if (m_AnimatedSelections == null)
                return;

            if (lastFrameSpectrumdata.Length != data.Length)//Not set or length changed
            {
                lastFrameSpectrumdata = data;
            }

            //IcoSphere
            //For each face, translate the vertices some distance depending on the frequency range assigned.
            for (int i = 0; i < m_AnimatedSelections.Length; i++)
            {
                float normalizedIndex = (i / m_FaceLength);//Get normal index of face
                int n = (int)(normalizedIndex * Config.fftBounds);
                Vector3 displacement =
                    m_AnimatedSelections[i].normal
                    * ((data[n] + lastFrameSpectrumdata[n]) * .5f)//与上一帧的平均值
                    * (frequencyCurve.Evaluate(normalizedIndex) * .5f + .5f)//利用曲线来决定显示哪一部分的频段（待优化）
                    * Config.maxExtrusion;
                foreach (int t in m_AnimatedSelections[i].indices)
                {
                    m_DisplacedVertexPositions[t] = m_OriginalVertexPositions[t] + displacement;
                }
            }
            m_UnityMesh.vertices = m_DisplacedVertexPositions;// Apply the new extruded vertex positions to the MeshFilter.

            //Cache
            lastFrameSpectrumdata = data;
        }
        #endregion

        #region Init&Update

        #region Editor

        [ContextMenu("EditorGenerateWaveform")]
        void EditorGenerateWaveform()//非运行模式，基于当前配置生成Waveform
        {
            int rawSampleCount = 256;//提供模拟的RawSampleCount
            ReGenerateWaveform(rawSampleCount);
            UpdateWaveformSetting();
        }
        [ContextMenu("EditorGenerateSphere")]
        void EditorGenerateSphere()
        {
            ///Todo:
            ///-仅生成多边形，不缓存到非序列化字段中。不急，可以先用模型代替
            //ReGenerateSphere();
        }
        #endregion

        void ReGenerateSphere()
        {
            //——生成Sphere——
            //Todo：考虑父物体的局部缩放
            m_ProBuilderMesh = ShapeGenerator.GenerateIcosahedron(PivotLocation.Center, Config.sphereRadius, Config.sphereSubdivisions);// Create a new sphere instance.  

            var shell = m_ProBuilderMesh.faces;// Shell is all the faces on the new sphere.

            // Extrude all faces on the sphere by a small amount. The third boolean parameter
            // specifies that extrusion should treat each face as an individual, not try to group
            // all faces together.
            m_ProBuilderMesh.Extrude(shell, Config.extrudeMethod, Config.idleExtrusion);

            // ToMesh builds the mesh positions, submesh, and triangle arrays. Call after adding
            // or deleting vertices, or changing face properties.
            m_ProBuilderMesh.ToMesh();
            m_ProBuilderMesh.Refresh();// Refresh builds the normals, tangents, and UVs.

            m_AnimatedSelections = new ExtrudedSelection[shell.Count];
            // Populate the outsides[] cache. This is a reference to the tops of each extruded column, including
            // copies of the sharedIndices.
            for (int i = 0; i < shell.Count; ++i)
            {
                m_AnimatedSelections[i] = new ExtrudedSelection(m_ProBuilderMesh, shell[i]);
            }
            m_OriginalVertexPositions = m_ProBuilderMesh.positions.ToArray();// Store copy of positions array un-modified          
            m_DisplacedVertexPositions = new Vector3[m_ProBuilderMesh.vertexCount];// displaced_vertices should mirror sphere mesh vertices.
            m_FaceLength = m_AnimatedSelections.Length;

            //tfIcoSphere = m_ProBuilderMesh.transform;
            MeshFilter builderMeshFilter = m_ProBuilderMesh.GetComponent<MeshFilter>();//对应ProBuilderMeshFilter（ProBuilderMesh组件）
            MeshFilter meshFilterIcoSphere = tfIcoSphere.GetComponent<MeshFilter>();
            meshFilterIcoSphere.mesh = builderMeshFilter.sharedMesh;

            m_UnityMesh = meshFilterIcoSphere.sharedMesh;
            m_ProBuilderMesh.gameObject.DestroyAtOnce();//Destroy generated object（会根据是否为运行模式自动删除）

            tfIcoSphere.parent = TfParent;
            tfIcoSphere.localPosition = Vector3.zero;//Init Pos
            tfIcoSphere.localScale = Vector3.one;//Reset Scale
            tfIcoSphere.localRotation = Quaternion.identity;
            //Update Parent Pos
            Vector3 parentLocalPos = pivot == Pivot.Center ? Vector3.zero : Vector3.up * Config.ShellRadius;
            TfParent.localPosition = parentLocalPos;

            //Notify event
            Vector3 shellSize = Vector3.one * (Config.ShellRadius) * 2;
            onLocalSphereSizeChanged.Invoke(Vector3.one * Config.SphereIdleRadius * 2);
            onLocalShellSizeChanged.Invoke(shellSize);

            //——更新Shell——
            if (tfShell)
            {
                MeshRenderer meshRenderer = tfShell.GetComponent<MeshRenderer>();
                if (meshRenderer)//只决定Shell是否显示Mesh，不影响其其他组件的正常运行，如SphereCollider
                    meshRenderer.enabled = Config.showShell;

                tfShell.localScale = shellSize;
            }
        }

        void ReGenerateWaveform(int rawSampleCount)
        {

            waveform.gameObject.SetActive(Config.showWaveform);
            waveform.useWorldSpace = false;//基于局部坐标
            waveform.loop = true;//首尾相连
            waveformPointCount = rawSampleCount;//Waveform点数默认与RawData数组数量一致
            switch (Config.waveformDownSample)//根据设置进行降采样(因为rawData数组的数量一定是2的幂，所以可以直接相除)
            {
                case DownSample.Off:
                    break;
                case DownSample.Half:
                    waveformPointCount /= 2; break;
                case DownSample.Quarter:
                    waveformPointCount /= 4; break;
                case DownSample.Eighth:
                    waveformPointCount /= 8; break;
                default:
                    Debug.LogError($"{Config.waveformDownSample} not define!"); break;
            }
            waveform.positionCount = waveformPointCount;
            InitWaveform();
            //UpdateWaveformSetting();//因为Bug#20240111，导致通过运行时编辑Graident不会回调方法，因此暂时把这段代码挪到Update
        }

        void InitWaveform()
        {
            //Init point positions
            Vector3 vec = Vector3.zero;
            int totalCount = waveform.positionCount;
            float fTotalCount = totalCount;//Warning:后续需要使用float进行小数点计算，因此类型为float
            for (int i = 0; i != totalCount; i++)
            {
                int n = i < totalCount - 1 ? i : 0;
                float travel = Config.waveformRadius;
                vec.x = Mathf.Cos(n / fTotalCount * twoPi) * travel;
                vec.z = Mathf.Sin(n / fTotalCount * twoPi) * travel;
                vec.y = 0f;
                waveform.SetPosition(i, vec);
            }
        }

        [ContextMenu("UpdateWaveformSetting")]
        void UpdateWaveformSetting()
        {
            waveform.colorGradient = Config.waveformGradient;
            waveform.widthMultiplier = Config.waveformWidthMultiplier * parentLossyScale;//用户缩放父物体会同步到Waveform中
        }
        #endregion

        #region Editor Method
#if UNITY_EDITOR
        Color gizmoColor = new Color(1, 1, 1, 0.5f);
        void OnDrawGizmos()
        {
            if (Application.isPlaying)
                return;

            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(TfParent.position, Config.sphereRadius * parentLossyScale);//Draw IcoSphere's shape
        }

        //private void OnValidate()
        //{
        //    if (Application.isPlaying)
        //        return;
        //}
#endif
        #endregion

        #region Define
        [System.Serializable]
        public class ConfigInfo : SerializableComponentConfigInfoBase
        {
            [JsonIgnore] public UnityAction<PersistentChangeState> actionMeshGenerateSettingChanged;//Any Generate field change
            [JsonIgnore] public UnityAction<PersistentChangeState> actionWaveformGenerateSettingChanged;

            /// <summary>
            /// 球Idle的半径
            /// </summary>
            [JsonIgnore] public float SphereIdleRadius { get { return sphereRadius + idleExtrusion; } }

            /// <summary>
            /// 最外圈的半径
            /// </summary>
            [JsonIgnore]
            public float ShellRadius
            {
                get
                {
                    float value = SphereIdleRadius;

                    if (showShell)//仅在显示Shell时包括Shell厚度
                    {
                        value += shellThickness;
                        if (shellSizeIncludeMaxExtrusion)
                            value += maxExtrusion;
                    }
                    return value;
                }
            }

            [Header("Sphere Generate Setting")]
            [Tooltip("The number of subdivisions to give the sphere.")][Range(0, 3)][PersistentValueChanged(nameof(OnMeshGenerateSettingChanged))] public int sphereSubdivisions = 2;
            [Tooltip("The radius of the sphere on instantiation.       ")][Range(0.5f, 1f)][PersistentValueChanged(nameof(OnMeshGenerateSettingChanged))] public float sphereRadius = 0.5f;
            [PersistentValueChanged(nameof(OnMeshGenerateSettingChanged))] public ExtrudeMethod extrudeMethod = ExtrudeMethod.IndividualFaces;
            [Tooltip("How far along the normal should each face be extruded when at idle (no audio input).")][Range(0f, 1f)][PersistentValueChanged(nameof(OnMeshGenerateSettingChanged))] public float idleExtrusion = .01f;
            [Tooltip("The max distance a frequency range will extrude a face.")][Range(0, 1f)][PersistentValueChanged(nameof(OnMeshGenerateSettingChanged))] public float maxExtrusion = 0.1f;

            [PersistentValueChanged(nameof(OnMeshGenerateSettingChanged))] public bool showShell = true;
            [Tooltip("The thickness of the shell")][EnableIf(nameof(showShell))][Range(0, .1f)][PersistentValueChanged(nameof(OnMeshGenerateSettingChanged))] public float shellThickness = .01f;
            [Tooltip("Considering maximum extrusion for shell size")][EnableIf(nameof(showShell))][PersistentValueChanged(nameof(OnMeshGenerateSettingChanged))] public bool shellSizeIncludeMaxExtrusion = true;

            [Header("Sphere Update Setting")]
            [Tooltip("An FFT returns a spectrum including frequencies that are out of human hearing range. This restricts the number of bins used from the spectrum to the lower bounds.")][Range(8, 128)] public int fftBounds = 32;

            [Header("Waveform Generate Setting")]
            [PersistentValueChanged(nameof(OnWaveformSettingGenerateChanged))] public bool showWaveform = true;
            [Tooltip("Downsampling based on audio sources can affect the number of points in the waveform")][EnableIf(nameof(showWaveform))][PersistentValueChanged(nameof(OnWaveformSettingGenerateChanged))] public DownSample waveformDownSample = DownSample.Off;

            [Header("Waveform Update Setting")]
            [EnableIf(nameof(showWaveform))][PersistentValueChanged(nameof(OnWaveformSettingGenerateChanged))] public Gradient waveformGradient = new Gradient();
            [EnableIf(nameof(showWaveform))][PersistentValueChanged(nameof(OnWaveformSettingGenerateChanged))] public float waveformWidthMultiplier = .5f;// The widthMultiplier of the waveform.
            [Tooltip("How far from the sphere center should the waveform be.")][EnableIf(nameof(showWaveform))] public float waveformRadius = .6f;//（PS：因为Update也会使用该字段，所以放在该Header下）
            [Tooltip("The y size of the waveform.")][EnableIf(nameof(showWaveform))] public float waveformHeight = 0.2f;
            [Tooltip("If true, the waveform ring will rotate around self. ")][EnableIf(nameof(showWaveform))] public bool rotateWaveformRing = false;
            [Tooltip("Waveform ring's rotate speed (degrees per second).")][EnableIf(EConditionOperator.And, new string[] { nameof(showWaveform), nameof(rotateWaveformRing) })] public Vector3 waveformRotateSpeed = new Vector3(0f, 3f, 0f);

            [JsonConstructor]
            public ConfigInfo()
            {
                waveformDownSample = DownSample.Off;//避免默认设置为0而导致报错
            }

            void OnMeshGenerateSettingChanged(PersistentChangeState persistentChangeState) { actionMeshGenerateSettingChanged.Execute(persistentChangeState); }
            void OnWaveformSettingGenerateChanged(PersistentChangeState persistentChangeState) { actionWaveformGenerateSettingChanged.Execute(persistentChangeState); }

            /// <summary>
            /// Touse：用于Waveform降采样，减少点的数量
            /// </summary>
            public enum DownSample
            {
                Off = 1,
                Half = 2,//二分之一
                Quarter = 4,//四分之一
                Eighth = 8//八分之一
            };
        }

        public class PropertyBag : ConfigurableComponentPropertyBagBase<AudioVisualizer_IcoSphere, ConfigInfo> { }

        public enum Pivot
        {
            Center,
            LowerCenter,//The Lowest Y Axis
        }

        /// <summary>
        /// This is the container for each extruded column. We'll use it to apply offsets per-extruded face.
        /// </summary>
        struct ExtrudedSelection
        {
            /// <value>
            /// The direction in which to move this selection when animating.
            /// </value>
            public Vector3 normal;

            /// <value>
            /// All vertex indices (including common vertices). "Common" refers to vertices that share a position
            /// but remain discrete.
            /// </value>
            public List<int> indices;

            public ExtrudedSelection(ProBuilderMesh mesh, Face face)
            {
                indices = mesh.GetCoincidentVertices(face.distinctIndexes);
                normal = Math.Normal(mesh, face);
            }
        }
        #endregion
    }
}