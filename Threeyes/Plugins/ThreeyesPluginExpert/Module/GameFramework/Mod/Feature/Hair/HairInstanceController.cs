#if USE_UnityHair
using System.Collections;
using UnityEngine;
using System;
using Threeyes.Persistent;
using Newtonsoft.Json;
using UnityEngine.Events;
using NaughtyAttributes;
using Threeyes.Config;
using Unity.DemoTeam.Hair;
using static Unity.DemoTeam.Hair.HairInstance.SettingsStrands;
using static Unity.DemoTeam.Hair.HairSim.SolverSettings;
using static Unity.DemoTeam.Hair.HairInstance;
using static Unity.DemoTeam.Hair.HairSim;
using Threeyes.Core;

namespace Threeyes.GameFramework
{
    public class HairInstanceController : ConfigurableComponentBase<HairInstance, SOHairInstanceControllerConfig, HairInstanceController.ConfigInfo>
, IHubProgramActiveHandler
    {
#region Unity Method
        private void Awake()
        {
            Config.actionPersistentChanged += OnPersistentChanged;
        }
        private void OnDestroy()
        {
            Config.actionPersistentChanged -= OnPersistentChanged;
        }
#endregion

#region Callback

        void OnPersistentChanged(PersistentChangeState persistentChangeState)
        {
            if (persistentChangeState == PersistentChangeState.Load)
                return;
            UpdateSetting();
        }
        public void UpdateSetting()
        {
            SettingsStrands settingsStrands = Comp.strandGroupDefaults.settingsStrands;
            settingsStrands.strandScale = Config.strandScale;
            settingsStrands.strandDiameter = Config.strandDiameter;
            settingsStrands.strandMargin = Config.strandMargin;
            settingsStrands.stagingPrecision = Config.stagingPrecision;
            settingsStrands.staging = Config.staging;
            Comp.strandGroupDefaults.settingsStrands = settingsStrands;

            SolverSettings solverSettings = Comp.strandGroupDefaults.settingsSolver;
            solverSettings.method = Config.method;
            solverSettings.iterations = Config.iterations;
            solverSettings.substeps = Config.substeps;
            solverSettings.stiffness = Config.stiffness;
            solverSettings.kSOR = Config.kSOR;
            solverSettings.damping = Config.damping;
            solverSettings.dampingFactor = Config.dampingFactor;
            solverSettings.dampingInterval = Config.dampingInterval;
            solverSettings.angularDamping = Config.angularDamping;
            solverSettings.angularDampingFactor = Config.angularDampingFactor;
            solverSettings.angularDampingInterval = Config.angularDampingInterval;
            solverSettings.cellPressure = Config.cellPressure;
            solverSettings.cellVelocity = Config.cellVelocity;
            solverSettings.gravity = Config.gravity;
            solverSettings.gravityRotation = Config.gravityRotation;
            solverSettings.boundaryCollision = Config.boundaryCollision;
            solverSettings.boundaryCollisionFriction = Config.boundaryCollisionFriction;
            solverSettings.distance = Config.distance;
            solverSettings.distanceLRA = Config.distanceLRA;
            solverSettings.distanceFTL = Config.distanceFTL;
            solverSettings.distanceFTLDamping = Config.distanceFTLDamping;
            solverSettings.localCurvature = Config.localCurvature;
            solverSettings.localCurvatureMode = Config.localCurvatureMode;
            solverSettings.localCurvatureValue = Config.localCurvatureValue;
            solverSettings.localShape = Config.localShape;
            solverSettings.localShapeMode = Config.localShapeMode;
            solverSettings.localShapeInfluence = Config.localShapeInfluence;
            solverSettings.localShapeBias = Config.localShapeBias;
            solverSettings.localShapeBiasValue = Config.localShapeBiasValue;
            solverSettings.globalPosition = Config.globalPosition;
            solverSettings.globalPositionInfluence = Config.globalPositionInfluence;
            solverSettings.globalPositionInterval = Config.globalPositionInterval;
            solverSettings.globalRotation = Config.globalRotation;
            solverSettings.globalRotationInfluence = Config.globalRotationInfluence;
            solverSettings.globalFade = Config.globalFade;
            solverSettings.globalFadeOffset = Config.globalFadeOffset;
            solverSettings.globalFadeExtent = Config.globalFadeExtent;
            Comp.strandGroupDefaults.settingsSolver = solverSettings;
        }

        //PS:更新激活状态后需要重新激活物体
        public void OnProgramActiveChanged(bool isActive)
        {
            if (isActive)
                ReBuild();
        }

#endregion

        public void ReBuild()
        {
            CoroutineManager.StartCoroutineEx(IEReBuild());
        }
        private IEnumerator IEReBuild()
        {
            gameObject.SetActive(false);
            yield return null;
            gameObject.SetActive(true);
        }
#region Define
        [Serializable]
        [PersistentChanged(nameof(ConfigInfo.OnPersistentChanged))]
        public class ConfigInfo : SerializableDataBase//(PS: default values ref from struct constructor)
        {
            [JsonIgnore] public UnityAction<PersistentChangeState> actionPersistentChanged;

            //ToAdd:对应Strand Group的id，默认是DefaultGroup。后期可用使用多个AC_HairInstanceController来控制对应Group
            //——Strand Group Settings——
            [Header("SettingsStrands")]
            //-Proportions-
            public StrandScale strandScale = StrandScale.Fixed;
            [Range(0.01f, 100.0f), Tooltip("Strand diameter (in millimeters)")]
            public float strandDiameter = 1f;
            [Range(0.0f, 100.0f), Tooltip("Strand margin (in millimeters)")]
            public float strandMargin = 0.0f;
            //-Geometry-
            public bool staging = true;
            [EnableIf(nameof(staging))] [AllowNesting] public StagingPrecision stagingPrecision = StagingPrecision.Half;
            [EnableIf(nameof(staging))] [AllowNesting] [Range(0, 10)] public int stagingSubdivision = 0;


            [Header("SettingsSolver")]
            //-Solver-
            [Tooltip("Constraint solver")]
            public Method method = Method.GaussSeidel;
            [Range(1, 100), Tooltip("Constraint iterations")]
            public int iterations = 3;
            [Range(1, 12), Tooltip("Solver substeps")]
            public int substeps = 1;
            [Range(0.0f, 1.0f), Tooltip("Constraint stiffness")]
            public float stiffness = 1.0f;
            [Range(1.0f, 2.0f), Tooltip("Successive over-relaxation factor")]
            public float kSOR = 1.0f;

            //-Integration-
            [Tooltip("Enable linear damping")]
            public bool damping = false;
            [EnableIf(nameof(damping))]
            [AllowNesting]
            [Range(0.0f, 1.0f), Tooltip("Linear damping factor (fraction of linear velocity to subtract per interval of time)")]
            public float dampingFactor = 0.5f;
            [EnableIf(nameof(damping))]
            [AllowNesting]
            [Tooltip("Interval of time over which to subtract fraction of linear velocity")]
            public TimeInterval dampingInterval = TimeInterval.PerSecond;

            [Tooltip("Enable angular damping")]
            public bool angularDamping = false;
            [EnableIf(nameof(angularDamping))]
            [AllowNesting]
            [Range(0.0f, 1.0f), Tooltip("Angular damping factor (fraction of angular velocity to subtract per interval of time)")]
            public float angularDampingFactor = 0.5f;
            [EnableIf(nameof(angularDamping))]
            [AllowNesting]
            [Tooltip("Interval of time over which to subtract fraction of angular velocity")]
            public TimeInterval angularDampingInterval = TimeInterval.PerSecond;

            [Range(0.0f, 1.0f), Tooltip("Scaling factor for volume pressure impulse")]
            public float cellPressure = 1.0f;
            [Range(0.0f, 1.0f), Tooltip("Scaling factor for volume velocity impulse (where 0 == FLIP, 1 == PIC)")]
            public float cellVelocity = 0.05f;
            [Range(-1.0f, 1.0f), Tooltip("Scaling factor for gravity (Physics.gravity)")]
            public float gravity = 1.0f;
            [Tooltip("Additional rotation of gravity vector (Physics.gravity)")]
            public Vector3 gravityRotation;

            //-Constraints-
            [Tooltip("Enable boundary collision constraint")]
            public bool boundaryCollision = true;
            [EnableIf(nameof(boundaryCollision))]
            [AllowNesting]
            [Range(0.0f, 1.0f), Tooltip("Boundary collision friction")]
            public float boundaryCollisionFriction = 0.5f;

            [Tooltip("Enable particle-particle distance constraint")]
            public bool distance = true;
            [Tooltip("Enable 'long range attachment' distance constraint (root-particle maximum distance)")]
            public bool distanceLRA = true;
            [Tooltip("Enable 'follow the leader' distance constraint (hard particle-particle distance, non-physical)")]
            public bool distanceFTL = false;
            [EnableIf(nameof(distanceFTL))]
            [AllowNesting]
            [Range(0.0f, 1.0f), Tooltip("FTL correction / damping factor")]
            public float distanceFTLDamping = 0.8f;

            [Tooltip("Enable bending curvature constraint")]
            public bool localCurvature = true;
            [EnableIf(nameof(localCurvature))]
            [AllowNesting]
            [Tooltip("Bending curvature constraint mode (=, <, >)")]
            public LocalCurvatureMode localCurvatureMode = LocalCurvatureMode.LessThan;
            [EnableIf(nameof(localCurvature))]
            [AllowNesting]
            [Range(0.0f, 1.0f), Tooltip("Scales to a bend of [0 .. 90] degrees")]
            public float localCurvatureValue = 0.1f;

            [Tooltip("Enable local shape constraint")]
            public bool localShape = true;
            [EnableIf(nameof(localShape))]
            [AllowNesting]
            [Tooltip("Local shape constraint application mode")]
            public LocalShapeMode localShapeMode = LocalShapeMode.Stitched;
            [EnableIf(nameof(localShape))]
            [AllowNesting]
            [Range(0.0f, 1.0f), Tooltip("Local shape constraint influence")]
            public float localShapeInfluence = 1.0f;
            [Tooltip("Enable local shape bias")]
            public bool localShapeBias = true;
            [EnableIf(nameof(localShapeBias))]
            [AllowNesting]
            [Range(0.0f, 1.0f), Tooltip("Local shape bias (skews local solution towards global reference)")]
            public float localShapeBiasValue = 0.5f;

            //-Reference-
            [Tooltip("Enable global position constraint")]
            public bool globalPosition = false;
            [EnableIf(nameof(globalPosition))]
            [AllowNesting]
            [Range(0.0f, 1.0f), Tooltip("Fraction of global position to apply per interval of time")]
            public float globalPositionInfluence = 1.0f;
            [EnableIf(nameof(globalPosition))]
            [AllowNesting]
            [Tooltip("Interval of time over which to apply fraction of global position")]
            public TimeInterval globalPositionInterval = TimeInterval.PerSecond;
            [Tooltip("Enable global rotation constraint")]
            public bool globalRotation = false;
            [EnableIf(nameof(globalRotation))]
            [AllowNesting]
            [Range(0.0f, 1.0f), Tooltip("Global rotation influence")]
            public float globalRotationInfluence = 1.0f;
            [Tooltip("Fade influence of global constraints from root to tip")]
            public bool globalFade = false;
            [EnableIf(nameof(globalFade))]
            [AllowNesting]
            [Range(0.0f, 1.0f), Tooltip("Normalized fade offset (normalized distance from root)")]
            public float globalFadeOffset = 0.1f;
            [EnableIf(nameof(globalFade))]
            [AllowNesting]
            [Range(0.0f, 1.0f), Tooltip("Normalized fade extent (normalized distance from specified offset)")]
            public float globalFadeExtent = 0.2f;

#region Callback
            void OnPersistentChanged(PersistentChangeState persistentChangeState)
            {
                actionPersistentChanged.Execute(persistentChangeState);
            }
#endregion
        }
#endregion
    }
}
#endif