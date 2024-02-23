using Newtonsoft.Json;
using Threeyes.Persistent;
using UnityEngine;

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
    /// 
    /// PS:
    /// -因为FillAmount可能是运行时更新，所以暂不存到ConfigInfo中，或者增加是否为DynamicFillAmount等相关字段
    /// </summary>
    public class LiquidController : ConfigurableComponentBase<Renderer, LiquidController, SOLiquidControllerConfig, LiquidController.ConfigInfo, LiquidController.PropertyBag>
, IModHandler
    {
        #region Property & Field

        /// <summary>
        /// Runtime FillAmount，
        /// 
        /// PS：
        /// -This property is mainly for runtime modify other Sources (Such as UnityEvent），This field's value will not change ConfigInfo.startFillAmount or to be saved/serialized（因为该属性主要用于运行时更改FillAmount（如根据时间回调更新），所以不适合存储到Config中及序列化）
        /// </summary>
        public float FillAmount { get { return Comp ? Comp.material.GetFloat("_FillAmount") : 0; } set { SetShaderProperty_FillAmount(value); } }

        /// <summary>
        /// 在startFillAmount的基础上额外的数值
        /// 
        /// 适用于：
        /// -在原基础上进行偏移（如音频响应）
        /// </summary>
        /// <param name="delta"></param>
        public void SetDeltaFillAmount(float delta)
        {
            //PS：基于ConfigInfo.startFillAmount进行计算
            float finalFillAmount = Mathf.Clamp01(Config.baseFillAmount + delta);
            SetShaderProperty_FillAmount(finalFillAmount);
        }

        /// <summary>
        /// 填充剩余空间，通过传入归一化值，可以填充（1-Config.startFillAmount）的值，用于
        /// </summary>
        /// <param name="fillPercent"></param>
        public void FillRemainSpace(float fillPercent)
        {
            fillPercent = Mathf.Clamp01(fillPercent);
            float leftPercent = Mathf.Clamp01(1 - Config.baseFillAmount);//剩余位填充空间
            SetShaderProperty_FillAmount(Config.baseFillAmount + leftPercent * fillPercent);
        }
        void SetShaderProperty_FillAmount(float value)
        {
            Comp?.material.SetFloat("_FillAmount", value);
        }

        protected virtual Material TargetMaterial { get { return Comp ? Comp.material : null; } }//override this if you prefer different material

        public Transform TfMotionSource { get { return tfMotionSource ? tfMotionSource : transform; } }
        public Transform TfLiquidSource { get { return Comp ? Comp.transform : transform; } }

        public Transform tfMotionSource;//The target to calculate offset
        [Header("Model Setting")]//PS: Set these value and Invoke SetShaderModelConfig in MenuItem before game started to recalculate the Material propertys
        [Tooltip("The ScaleFactor in model import window")]
        public float modelScaleFactor = 1;
        [Tooltip("The model vertice pos range in Y Axis while the model's rotation is in initial state")]
        public Vector2 modelVerticePosYRange = new Vector2(-0.5f, 0.5f);//当模型在初始旋转状态时，模型所有点的Y轴区间范围，用于计算液体的显示区域

        //Runtime
        float Pulse { get { return 2 * Mathf.PI * curWobbleFrequency; } }// make a sine wave of the decreasing wobble
        float maxWobblePerFrame { get { return Config.maxWobble * DeltaTime; } }//(PS:只是单帧上限，不设置总上限，方便实现更自然的晃动效果）
        float curWobbleFrequency = 0;//当前的晃动频率，值越大晃动越频繁
        float wobbleXSinInput = 0;
        float wobbleZSinInput = 0;//Different from X Axis
        float wobbleXSinOutputScale = 0;
        float wobbleZSinOutputScale = 0;
        float curFoamLineWdith = 0;
        Vector3 deltaVelocity;
        Vector3 deltaAngularVelocity;
        Vector3 lastPos;
        public Vector3 lastVelocity = Vector3.zero;
        Vector3 lastRot;
        #endregion

        #region Unity Method
        protected float DeltaTime { get { return Time.deltaTime; } }
        protected virtual void LateUpdate()
        {
            if (!TargetMaterial)
                return;
            if (DeltaTime == 0)//避免因为时间停止导致velocity等值因为除0变为Null
                return;

            deltaVelocity = (TfMotionSource.position - lastPos) / DeltaTime;
            deltaAngularVelocity = deltaVelocity.sqrMagnitude > 0.001f ? TfMotionSource.rotation.eulerAngles - lastRot : Vector3.zero;//正在移动=>忽略旋转，否则在两个变量的作用下会导致异常抖动的现象

            //#0【Frequency】 如果移动朝向发生变化，则增加晃动频率Frequency；否则逐渐恢复原状
            if (deltaVelocity.x * lastVelocity.x < 0)//Increase frequency everytime the direction changed
            {
                curWobbleFrequency += Config.wobbleFrequencyIncreaseSpeed * DeltaTime;
            }
            else
            {
                curWobbleFrequency = Mathf.Lerp(curWobbleFrequency, Config.wobbleFrequency, DeltaTime * Config.wobbleRecovery);
            }

            //#1【Direction】 根据瞬间运动方向，调节WaveSine的Input，从而控制液体的偏转方向
            wobbleXSinInput += DeltaTime * Pulse;
            wobbleZSinInput += DeltaTime * Pulse;
            if (ModifySinInput(deltaVelocity.x, lastVelocity.x, ref wobbleXSinInput, wobbleXSinOutputScale))
                lastVelocity.x = deltaVelocity.x;
            if (ModifySinInput(deltaVelocity.z, lastVelocity.z, ref wobbleZSinInput, wobbleZSinOutputScale))
                lastVelocity.z = deltaVelocity.z;

            //#2【Y Value】 根据位移和旋转的差值，计算WaveSine的Y轴值缩放(因为#1已经计算了增减区间，所以这里的velocity只需要取正值）
            wobbleXSinOutputScale += Mathf.Clamp(
                (Mathf.Abs(deltaVelocity.x) * Config.wobbleIncreaseByMoveSpeed +
                Mathf.Abs(deltaAngularVelocity.z) * Config.wobbleIncreaseByRotateSpeed) * maxWobblePerFrame,
                -maxWobblePerFrame, maxWobblePerFrame);
            wobbleZSinOutputScale += Mathf.Clamp(
                (Mathf.Abs(deltaVelocity.z) * Config.wobbleIncreaseByMoveSpeed +
                Mathf.Abs(deltaAngularVelocity.x) * Config.wobbleIncreaseByRotateSpeed) * maxWobblePerFrame,
                -maxWobblePerFrame, maxWobblePerFrame);
            wobbleXSinOutputScale = Mathf.Lerp(wobbleXSinOutputScale, 0, DeltaTime * Config.wobbleRecovery);
            wobbleZSinOutputScale = Mathf.Lerp(wobbleZSinOutputScale, 0, DeltaTime * Config.wobbleRecovery);

            //#3 设置Wobble值
            TargetMaterial.SetFloat("_WobbleX", wobbleXSinOutputScale * Mathf.Sin(wobbleXSinInput));
            TargetMaterial.SetFloat("_WobbleZ", wobbleZSinOutputScale * Mathf.Sin(wobbleZSinInput));

            //#4 设置Foam值
            curFoamLineWdith += (deltaVelocity.magnitude * Config.foamIncreaseSpeed - Config.foamDecreaseSpeed) * DeltaTime;//PS: angularVelocity会导致瞬间增加的Bug，暂时i不考虑
            curFoamLineWdith = Mathf.Clamp(curFoamLineWdith, Config.rangeFoam.x, Config.rangeFoam.y);
            TargetMaterial.SetFloat("_FoamLineWidth", curFoamLineWdith);//Foam Line Width

            //#5 确保材质与物体的全局缩放值同步
            TargetMaterial.SetFloat("_GlobalScale", TfLiquidSource.lossyScale.y * modelScaleFactor);

            // Save Last Data
            lastPos = TfMotionSource.position;
            lastRot = TfMotionSource.rotation.eulerAngles;
            lastVelocity = deltaVelocity;
        }
        #endregion

        #region IModHandler
        public override void UpdateSetting()
        {
            //Init
            if (curWobbleFrequency == 0)
                curWobbleFrequency = Config.wobbleFrequency;
        }
        #endregion

        #region Utility
        /// <summary>
        /// 根据位移变化，尝试更改Sin的Input
        /// （ToUpdate: 优化算法）
        /// </summary>
        /// <param name="sinInput"></param>
        /// <returns>是否已经修改（移动方向与水的加速度方向相反）</returns>
        static bool ModifySinInput(float velocityValue, float lastVelocityValue, ref float sinInput, float lastWobbleScale)
        {
            /// Wobble原理：
            ///     1.以标准的一个周期SineWave为例，Input为时间，Output为当前对应的Wobble值，移动或旋转物体会缩放其 振幅（Amplitude）（如果物体反向运动则对Input进行偏移）
            ///     2. Sine示意图(https://www.mathopenref.com/trigsinewaves.html#:~:text=The%20frequency%20of%20a%20sine%20wave%20is%20the,the%20frequency%20is%20about%20one%20cycle%20per%20second.)
            ///     
            //PS：左右移动时的瞬间波纹应该是相反的
            //PS:是pulse * time控制波动，如果方向相反，应该立即进行位移偏转
            //移动Sin的x值到其平行位置，使其从递增改为递减

            sinInput = Mathf.Repeat(sinInput, 2 * Mathf.PI);//将Input限定在在一个2π周期内的对应值
            int inIncreasingArea = sinInput <= 0.5f * Mathf.PI || sinInput >= 1.5f * Mathf.PI ? 1 : -1;//检查当前输出值是否在增加区域，对应Input值范围为：[0,Pi/2] 或 [Pi*3/2,2*Pi]

            //如果进行偏移（从静止开始移动或反方向移动）:修改对应的sin当前值，该值会修改Liquid的偏向
            if (lastWobbleScale == 0 || lastVelocityValue * velocityValue < 0)//从静止开始移动||与上次的位移方向相反       
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
#if UNITY_EDITOR
        [ContextMenu("CalculateModelVerticeRange")]
        public void CalculateModelVerticeRange()
        {
            ///计算模型点的区间
            if (!Comp)
            {
                Debug.LogError("Please set comp!");
                return;
            }

            MeshFilter meshFilter = Comp.GetComponent<MeshFilter>();
            if (meshFilter)
            {
                modelVerticePosYRange = new Vector2(float.MaxValue, float.MinValue);//初始化为两个最不可能的值，避免因为用户手动设置导致无法匹配

                //查找所有顶点的Y轴最大/最小值
                foreach (Vector3 point in meshFilter.sharedMesh.vertices)//Note that this method returns the vertices in local space, not in world space.
                {
                    if (point.y < modelVerticePosYRange.x)//最小值
                        modelVerticePosYRange.x = point.y;
                    else if (point.y > modelVerticePosYRange.y)
                        modelVerticePosYRange.y = point.y;
                }
                UnityEditor.EditorUtility.SetDirty(this);// mark as dirty, so the change will be save into scene file
            }
        }

        /// <summary>
        /// Use config data to setup shader (Only executed before game start)
        /// </summary>
        [ContextMenu("SetShaderModelConfig")]
        public void SetShaderModelConfig()
        {
            if (Application.isPlaying)
                return;

            ///PS:
            ///-既可以通过CalculateModelVerticeRange主动获取Vertice的范围，也可以由用户限制显示区域，避免模型带有外壳导致溢出的情况

            //Remap the pos range to [-0.5f,0.5f]（将点重映射到区间内）
            float middlePoint = (modelVerticePosYRange.y + modelVerticePosYRange.x) / 2;
            float scale = 0.5f / (modelVerticePosYRange.y - middlePoint);

            //PS:
            //  Renderer.sharedMaterial:Modifying sharedMaterial will change the appearance of all objects using this material, and change material settings that are stored in the project too.
            //  Renderer.material:If the material is used by any other renderers, this will clone the shared material and start using it from now on.This function automatically instantiates the materials and makes them unique to this renderer.
            Comp.sharedMaterial.SetFloat("_GlobalScale", transform.lossyScale.y * modelScaleFactor);//PS:默认是Y轴，所以模型只能是统一缩放。Todo：以后升级为不同轴向
            Comp.sharedMaterial.SetFloat("_PosOffset", -middlePoint);
            Comp.sharedMaterial.SetFloat("_PosScale", scale);
        }
#endif
        #endregion

        #region Define

        [System.Serializable]
        public class ConfigInfo : SerializableComponentConfigInfoBase
        {
            [Header("Common")]
            [Tooltip("The default liquid fill amount")] [Range(0, 1)] public float baseFillAmount = 0.5f;//

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

            [HideInInspector] [JsonIgnore] [PersistentDirPath] public string PersistentDirPath;
        }

        public class PropertyBag : ConfigurableComponentPropertyBagBase<LiquidController, ConfigInfo> { }
        #endregion
    }
}