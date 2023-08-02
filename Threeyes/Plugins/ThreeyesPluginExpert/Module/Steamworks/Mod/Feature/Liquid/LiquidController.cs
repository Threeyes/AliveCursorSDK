using NaughtyAttributes;
using Newtonsoft.Json;
using Threeyes.Config;
using Threeyes.Persistent;
using UnityEngine;
using UnityEngine.Events;

namespace Threeyes.Steamworks
{
    /// <summary>
    /// Control liquid shader
    /// 
    /// Ref：
    /// https://www.patreon.com/posts/quick-game-art-18245226
    ///https://pastebin.com/ppbzx7mn
    ///
    /// Warning：
    /// 1. The model must be uniform scaling
    /// 
    /// Howto setup material:
    /// 1. Create Material with shader "Threeyes/SpecialFX/Liquid", attached it to the Model
    /// 2. Set values in [Model] Group, then Invoke SetShaderModelConfig method in ContextMenu
    /// 3.Set material's FillAmount field
    /// </summary>
    public class LiquidController : ConfigurableUpdateComponentBase<Renderer, SOLiquidControllerConfig, LiquidController.ConfigInfo>
, IModHandler
    {
        #region Property & Field
        public float FillAmount { get { return Comp ? Comp.material.GetFloat("_FillAmount") : 0; } set { Comp?.material.SetFloat("_FillAmount", value); } }//(Mainly for runtime modify via UnityEvent)
        protected virtual Material TargetMaterial { get { return Comp ? Comp.material : null; } }//override this if you prefer different material


        [Header("Model Setting")]//PS: Set these value and Invoke SetShaderModelConfig in MenuItem before game started to recalculate the Material propertys
        [Tooltip("The ScaleFactor in model import window")]
        public float modelScaleFactor = 1;
        [Tooltip("The model vertice pos range in Y Axis while the model's rotation is in initial state")]
        public Vector2 modelVerticePosYRange = new Vector2(-0.5f, 0.5f);

        //Runtime
        float Pulse { get { return 2 * Mathf.PI * curWobbleFrequency; } }// make a sine wave of the decreasing wobble
        float maxWobblePerFrame { get { return Config.maxWobble * DeltaTime; } }//(PS:只是单帧上限，不设置总上限，方便实现更自然的晃动效果）
        float curWobbleFrequency = 2f;
        float wobbleSinInputX;
        float wobbleSinInputZ = 10;//Different from X Axis
        float wobbleSinOutputScaleX;
        float wobbleSinOutputScaleZ;
        float curFoamLineWdith;
        Vector3 lastPos;
        Vector3 velocity;
        Vector3 lastVelocity = default;
        Vector3 lastRot;
        Vector3 angularVelocity;
        #endregion

        #region Unity Method
        void Awake()
        {
            Config.actionAppearanceSettingChanged += OnAppearanceSettingChanged;
        }
        void OnDestroy()
        {
            Config.actionAppearanceSettingChanged -= OnAppearanceSettingChanged;
        }
        protected override void UpdateFunc()
        {
            if (!TargetMaterial)
                return;

            base.UpdateFunc();
            velocity = (transform.position - lastPos) / DeltaTime;
            angularVelocity = velocity.sqrMagnitude > 0.001f ? transform.rotation.eulerAngles - lastRot : Vector3.zero;//正在移动=>忽略旋转，否则在两个变量的作用下会导致异常抖动的现象

            //#1 根据瞬间运动方向，调节WaveSine的Input，从而控制液体的偏转方向
            wobbleSinInputX += DeltaTime * Pulse;
            wobbleSinInputZ += DeltaTime * Pulse;
            if (ModifySinInputByVelocity(velocity.x, lastVelocity.x, ref wobbleSinInputX, wobbleSinOutputScaleX))
                lastVelocity.x = velocity.x;
            if (ModifySinInputByVelocity(velocity.z, lastVelocity.z, ref wobbleSinInputZ, wobbleSinOutputScaleZ))
                lastVelocity.z = velocity.z;

            if (velocity.x * lastVelocity.x < 0)//Increase frequency everytime the direction changed
                curWobbleFrequency += Config.wobbleFrequencyIncreaseSpeed * DeltaTime;
            else
                curWobbleFrequency = Mathf.Lerp(curWobbleFrequency, Config.wobbleFrequency, DeltaTime * Config.wobbleRecovery);

            //#2 根据位移和旋转的变化，计算WaveSine的Result缩放(因为#1已经计算了增减区间，所以这里的velocity只需要取正值
            wobbleSinOutputScaleX += Mathf.Clamp((Mathf.Abs(velocity.x) * Config.wobbleIncreaseByMoveSpeed + angularVelocity.z * Config.wobbleIncreaseByRotateSpeed) * maxWobblePerFrame, -maxWobblePerFrame, maxWobblePerFrame);
            wobbleSinOutputScaleZ += Mathf.Clamp((Mathf.Abs(velocity.z) * Config.wobbleIncreaseByMoveSpeed + angularVelocity.x * Config.wobbleIncreaseByRotateSpeed) * maxWobblePerFrame, -maxWobblePerFrame, maxWobblePerFrame);
            wobbleSinOutputScaleX = Mathf.Lerp(wobbleSinOutputScaleX, 0, DeltaTime * Config.wobbleRecovery);
            wobbleSinOutputScaleZ = Mathf.Lerp(wobbleSinOutputScaleZ, 0, DeltaTime * Config.wobbleRecovery);

            //#3 设置Wobble值
            TargetMaterial.SetFloat("_WobbleX", wobbleSinOutputScaleX * Mathf.Sin(wobbleSinInputX));
            TargetMaterial.SetFloat("_WobbleZ", wobbleSinOutputScaleZ * Mathf.Sin(wobbleSinInputZ));

            //#4 设置Foam值
            curFoamLineWdith += (velocity.magnitude * Config.foamIncreaseSpeed - Config.foamDecreaseSpeed) * DeltaTime;//PS: angularVelocity会导致瞬间增加的Bug，暂时i不考虑
            curFoamLineWdith = Mathf.Clamp(curFoamLineWdith, Config.rangeFoam.x, Config.rangeFoam.y);
            TargetMaterial.SetFloat("_FoamLineWidth", curFoamLineWdith);//Foam Line Width

            //#5 每帧都保证与物体缩放值同步
            TargetMaterial.SetFloat("_GlobalScale", transform.lossyScale.y * modelScaleFactor);

            // Save Last Data
            lastPos = transform.position;
            lastRot = transform.rotation.eulerAngles;
        }
        #endregion

        #region Callback
        ///ToAdd：检查Config的Appearance并更新Shader
        public void OnModInit()
        {
            UpdateAppearanceSetting();

            //Init
            curWobbleFrequency = Config.wobbleFrequency;
        }
        public void OnModDeinit() { }
        void OnAppearanceSettingChanged(PersistentChangeState persistentChangeState)
        {
            if (persistentChangeState == PersistentChangeState.Load)
                return;
            RuntimeTool.ExecuteOnceInCurFrameAsync(UpdateAppearanceSetting);
        }
        void UpdateAppearanceSetting()
        {
            if (!Config.isOverrideAppearance)
                return;
            if (!TargetMaterial)
                return;

            if (TargetMaterial.GetTexture("_MainTex") != Config.MainTexture)
            {
                TargetMaterial.SetTexture("_MainTex", Config.MainTexture);
            }
            TargetMaterial.SetColor("_Color", Config.mainColor);
            TargetMaterial.SetColor("_TopColor", Config.topColor);
            TargetMaterial.SetColor("_FoamColor", Config.foamColor);
            TargetMaterial.SetColor("_RimColor", Config.rimColor);
            TargetMaterial.SetFloat("_RimPower", Config.rimPower);
        }
        #endregion

        #region Utility
        /// <summary>
        /// 根据位移变化，尝试更改Sin的Input
        /// （ToUpdate: 优化算法）
        /// </summary>
        /// <param name="sinInput"></param>
        /// <returns>是否已经修改</returns>
        static bool ModifySinInputByVelocity(float velocityValue, float lastVelocityValue, ref float sinInput, float wobbleAdd)
        {
            /// Wobble原理：
            ///     1.以标准的一个周期SineWave为例，Input为时间，Output为当前对应的Wobble值，移动或旋转物体会缩放其 振幅（Amplitude）（如果物体反向运动则对Input进行偏移）
            ///     2. Sine示意图(https://www.mathopenref.com/trigsinewaves.html#:~:text=The%20frequency%20of%20a%20sine%20wave%20is%20the,the%20frequency%20is%20about%20one%20cycle%20per%20second.)
            ///     
            //PS：左右移动时的瞬间波纹应该是相反的
            //PS:是pulse * time控制波动，如果方向相反，应该立即进行位移偏转
            //移动Sin的x值到其平行位置，使其从递增改为递减

            sinInput = Mathf.Repeat(sinInput, 2 * Mathf.PI);//将Input限定在在一个2π周期内的对应值
            int inIncreasingArea = sinInput <= Mathf.PI / 2 || sinInput >= 1.5f * Mathf.PI ? 1 : -1;//检查当前输出值是否在增加区域，对应Input值范围为：[0,Pi/2] 或 [Pi*3/2,2*Pi]

            //如果进行偏移（从静止开始移动或反方向移动）:修改对应的sin当前值，该值会修改Liquid的偏向
            if (wobbleAdd == 0 || lastVelocityValue * wobbleAdd < 0)//从静止开始移动||与上次的位移方向相反       
            {
                //如果移动方向与水的加速度方向相反：使其加速度相同（物体右移 对应 水顺时针旋转/加速）
                if (Mathf.Abs(velocityValue) > 0.001f && velocityValue * inIncreasingArea < 0)
                {
                    //变换sin wave中Input的值，把使其x值偏移但是y值保持不变
                    if (sinInput <= Mathf.PI)//[0,PI]范围内
                    {
                        sinInput = Mathf.PI - sinInput;
                    }
                    else //[PI,2PI]
                    {
                        sinInput = 3 * Mathf.PI - sinInput;
                    }
                }
                return true;
            }
            return false;
        }
        #endregion

        #region ContextMenu 
        [ContextMenu("SetShaderModelConfig")]
        public void SetShaderModelConfig()//Use this to (Execute before game start)
        {
            if (Application.isPlaying)
                return;

            ///PS:
            ///1.虽然可以通过代码主动获取Vertice的范围，但是由用户设置可以更自由地限制显示区域，避免模型带有外壳导致溢出的情况

            //Remap the pos range to [-0.5f,0.5f]
            float middlePoint = (modelVerticePosYRange.y + modelVerticePosYRange.x) / 2;
            float scale = 0.5f / (modelVerticePosYRange.y - middlePoint);

            //PS:
            //  Renderer.sharedMaterial:Modifying sharedMaterial will change the appearance of all objects using this material, and change material settings that are stored in the project too.
            //  Renderer.material:If the material is used by any other renderers, this will clone the shared material and start using it from now on.This function automatically instantiates the materials and makes them unique to this renderer.
            Comp.sharedMaterial.SetFloat("_GlobalScale", transform.lossyScale.y * modelScaleFactor);//PS:默认是Y轴，所以模型只能是统一缩放。Todo：以后升级为不同轴向
            Comp.sharedMaterial.SetFloat("_PosOffset", -middlePoint);
            Comp.sharedMaterial.SetFloat("_PosScale", scale);
        }
        #endregion

        #region Define

        [System.Serializable]
        [PersistentChanged(nameof(ConfigInfo.OnPersistentChanged))]
        public class ConfigInfo : SerializableDataBase
        {
            [JsonIgnore] public UnityAction<PersistentChangeState> actionPersistentChanged;
            [JsonIgnore] public UnityAction<PersistentChangeState> actionAppearanceSettingChanged;

            public Texture MainTexture { get { return externalMainTexture ? externalMainTexture : defaultMainTexture; } }

            //——Runtime Motion——
            [Header("Foam")]
            [Tooltip("Foam range")]
            public Vector2 rangeFoam = new Vector2(0, 0.1f);
            [Tooltip("The foam increase speed")]
            public float foamIncreaseSpeed = 0.005f;
            [Tooltip("The foam decrease speed")]
            public float foamDecreaseSpeed = 0.2f;

            [Header("Wobble")]
            [Tooltip("Max increase wobble per second")]
            public float maxWobble = 1f;
            [Tooltip("How much the movement affects wobble")]
            public float wobbleIncreaseByMoveSpeed = 0.15f;
            [Tooltip("How much the rotation affects wobble")]
            public float wobbleIncreaseByRotateSpeed = 0.2f;
            [Tooltip("Base frequency of the sine wave")]
            public float wobbleFrequency = 2f;
            [Tooltip("How much the frequency increased everytime the motion has changed to opposite direction")]
            public float wobbleFrequencyIncreaseSpeed = 1f;
            [Tooltip("How fast the wobble reset to origin state")]
            public float wobbleRecovery = 0.8f;

            //——Appearance——
            [Header("Appearance (Color or texture")]
            public bool isOverrideAppearance = false;
            [EnableIf(nameof(isOverrideAppearance))] [AllowNesting] [JsonIgnore] public Texture defaultMainTexture;
            [EnableIf(nameof(isOverrideAppearance))] [AllowNesting] [ReadOnly] [JsonIgnore] public Texture externalMainTexture;
            [EnableIf(nameof(isOverrideAppearance))] [AllowNesting] [PersistentAssetFilePath(nameof(externalMainTexture), true)] [PersistentValueChanged(nameof(OnAppearanceSettingChanged))] public string externalMainTextureFilePath;
            [EnableIf(nameof(isOverrideAppearance))] [AllowNesting] [ColorUsage(true, true)] [PersistentValueChanged(nameof(OnAppearanceSettingChanged))] public Color mainColor = Color.white;
            [EnableIf(nameof(isOverrideAppearance))] [AllowNesting] [ColorUsage(true, true)] [PersistentValueChanged(nameof(OnAppearanceSettingChanged))] public Color topColor = Color.green;
            [EnableIf(nameof(isOverrideAppearance))] [AllowNesting] [ColorUsage(true, true)] [PersistentValueChanged(nameof(OnAppearanceSettingChanged))] public Color foamColor = Color.gray;
            [EnableIf(nameof(isOverrideAppearance))] [AllowNesting] [ColorUsage(true, true)] [PersistentValueChanged(nameof(OnAppearanceSettingChanged))] public Color rimColor = Color.cyan;
            [EnableIf(nameof(isOverrideAppearance))] [AllowNesting] [Range(0, 10)] [PersistentValueChanged(nameof(OnAppearanceSettingChanged))] public float rimPower = 1;

            [HideInInspector] [JsonIgnore] [PersistentDirPath] public string PersistentDirPath;

            #region Callback
            void OnPersistentChanged(PersistentChangeState persistentChangeState)
            {
                actionPersistentChanged.Execute(persistentChangeState);
            }
            void OnAppearanceSettingChanged(PersistentChangeState persistentChangeState)
            {
                actionAppearanceSettingChanged.Execute(persistentChangeState);
            }
            #endregion
        }

        #endregion
    }
}