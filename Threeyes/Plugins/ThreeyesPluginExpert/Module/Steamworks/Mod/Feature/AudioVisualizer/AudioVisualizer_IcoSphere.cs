using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using Threeyes.Persistent;
using Newtonsoft.Json;
using System.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine.Events;
using Threeyes.Config;
namespace Threeyes.Steamworks
{
	/// <summary>
	/// Auto generate icosphere base on config, which can response to audio
	/// 
	/// Ref：Samples/ProBuilder/X.X.X/Runtime Examples/Icosphere FFT
	/// </summary>
	public class AudioVisualizer_IcoSphere : ConfigurableComponentBase<SOAudioVisualizer_IcoSphereConfig, AudioVisualizer_IcoSphere.ConfigInfo>
	, IHubSystemAudio_RawSampleDataChangedHandler
	, IHubSystemAudio_SpectrumDataChangedHandler
	, IModHandler
	{
		#region Property & Field
		const float twoPi = 6.283185f;
		public IHubSystemAudioManager Manager { get { return ManagerHolder.SystemAudioManager; } }

		public AnimationCurve frequencyCurve;// Optionally weights the frequency amplitude when calculating extrude distance.
		public LineRenderer waveform;// A reference to the line renderer that will be used to render the raw waveform.

		//Runtime
		ProBuilderMesh m_ProBuilderMesh;
		Mesh m_UnityMesh;// A reference to the MeshFilter.sharedMesh
		Transform m_Transform;//IcoSphere
		float m_FaceLength;
		ExtrudedSelection[] m_AnimatedSelections;// All faces that have been extruded       
		Vector3[] m_OriginalVertexPositions, m_DisplacedVertexPositions;// Keep a copy of the original vertex array to calculate the distance from origin.
		#endregion

		#region Unity Method
		void Awake()
		{
			Config.actionGeneratePersistentChanged += OnGeneratePersistentChanged;
			Config.actionMaterialChanged += OnMaterialChanged;
			Config.actionWaveformSettingChanged += OnWaveformSettingChanged;
		}
		private void OnDestroy()
		{
			Config.actionGeneratePersistentChanged -= OnGeneratePersistentChanged;
			Config.actionMaterialChanged -= OnMaterialChanged;
			Config.actionWaveformSettingChanged -= OnWaveformSettingChanged;
		}

		void Update()
		{
			// Ring rotation
			if (Config.showWaveform && Config.rotateWaveformRing)
			{
				waveform.transform.Rotate(Config.waveformRotateSpeed * 360 * Time.deltaTime, Space.Self);
			}
		}
		#endregion

		#region Callback
		public void OnModInit()
		{
			ReGenerateMesh();//等待PD完成初始化后，读取配置并更新（PS：如果使用的是defaultConfig，因为其不是SO，会导致其PD相关回调无法调用，因此不能依赖OnGeneratePersistentChanged在PersistentChangeState.Load时调用该方法，而是需要手动调用。凡是需要读取初始化值的都需要如此操作）
		}
		public void OnModDeinit() { }
		void OnMaterialChanged(PersistentChangeState persistentChangeState)
		{
			if (persistentChangeState == PersistentChangeState.Load)
				return;
			UpdateMaterial();
		}
		void OnGeneratePersistentChanged(PersistentChangeState persistentChangeState)
		{
			if (persistentChangeState == PersistentChangeState.Load)
				return;
			RuntimeTool.ExecuteOnceInCurFrameAsync(ReGenerateMesh);//Make sure get executed once
		}
		void OnWaveformSettingChanged(PersistentChangeState persistentChangeState)
		{
			if (persistentChangeState == PersistentChangeState.Load)
				return;
			UpdateWaveformSetting();
		}

		float[] lastFrameSpectrumdata = new float[] { };
		public virtual void OnSpectrumDataChanged(float[] data)
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
					* ((data[n] + lastFrameSpectrumdata[n]) * .5f)
					* (frequencyCurve.Evaluate(normalizedIndex) * .5f + .5f)
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

		public virtual void OnRawSampleDataChanged(float[] data)
		{
			if (!Config.showWaveform)
				return;

			///Waveform
			Vector3 vec = Vector3.zero;
			int totalCount = data.Length;
			float fTotalCount = (float)totalCount;//Warning:后续需要使用float进行小数点计算，因此类型为float
			for (int i = 0; i != totalCount; i++)
			{
				if (i == totalCount - 1)//Make sure the rear point connected to head point
				{
					waveform.SetPosition(i, waveform.GetPosition(0));
					return;
				}

				int n = i < fTotalCount - 1 ? i : 0;
				float travel = Config.waveformRadius + Config.waveformHeight * data[n];
				vec.x = Mathf.Cos(n / fTotalCount * twoPi) * travel;
				vec.z = Mathf.Sin(n / fTotalCount * twoPi) * travel;
				vec.y = 0f;

				waveform.SetPosition(i, vec);
			}
		}
		void InitWaveform()
		{
			//Init point positions
			Vector3 vec = Vector3.zero;
			int totalCount = waveform.positionCount;
			float fTotalCount = totalCount;//Warning:后续需要使用float进行小数点计算，因此类型为float
			for (int i = 0; i != totalCount; i++)
			{
				int n = i < fTotalCount - 1 ? i : 0;
				float travel = Config.waveformRadius;
				vec.x = Mathf.Cos(n / fTotalCount * twoPi) * travel;
				vec.z = Mathf.Sin(n / fTotalCount * twoPi) * travel;
				vec.y = 0f;
				waveform.SetPosition(i, vec);
			}
		}
		#endregion

		#region Inner Method
		async void ReGenerateMesh()
		{
			if (m_Transform)
			{
				Destroy(m_Transform.gameObject);//Destroy old mesh
				await Task.Yield();//Wait for destroy completed
			}

			//Todo：考虑父物体的局部缩放
			m_ProBuilderMesh = ShapeGenerator.GenerateIcosahedron(PivotLocation.Center, Config.sphereRadius, Config.sphereSubdivisions);// Create a new sphere.     
			UpdateMaterial();//Assign the default material

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

			m_UnityMesh = m_ProBuilderMesh.GetComponent<MeshFilter>().sharedMesh;
			m_FaceLength = m_AnimatedSelections.Length;
			m_Transform = m_ProBuilderMesh.transform;
			m_Transform.parent = transform;
			m_Transform.localPosition = Vector3.zero;//Reset Pos
			m_Transform.localScale = Vector3.one;//Reset Scale
			m_Transform.localRotation = Quaternion.identity;

			// Build the waveform ring.
			waveform.positionCount = Manager.RawSampleCount;
			InitWaveform();
			//ToAdd:Init Point Position
			UpdateWaveformSetting();
		}
		void UpdateMaterial()
		{
			if (m_ProBuilderMesh)
			{
				m_ProBuilderMesh.GetComponent<MeshRenderer>().sharedMaterial = Config.sphereMaterial;
			}
		}

		[ContextMenu("UpdateWaveformSetting")]
		void UpdateWaveformSetting()
		{
			waveform.gameObject.SetActive(Config.showWaveform);
			waveform.colorGradient = Config.waveformGradient;
			waveform.widthMultiplier = Config.waveformWidthMultiplier;
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
			Gizmos.DrawWireSphere(transform.position, Config.sphereRadius * transform.lossyScale.x);//Draw IcoSphere's shape
		}
#endif
		#endregion

		#region Define
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

		[System.Serializable]
		public class ConfigInfo : SerializableDataBase
		{
			[JsonIgnore] public UnityAction<PersistentChangeState> actionGeneratePersistentChanged;//Any Generate field change
			[JsonIgnore] public UnityAction<PersistentChangeState> actionMaterialChanged;
			[JsonIgnore] public UnityAction<PersistentChangeState> actionWaveformSettingChanged;

			[Header("Generate Setting")]
			[Range(0, 3)] [PersistentValueChanged(nameof(OnGeneratePersistentChanged))] public int sphereSubdivisions = 2;// The number of subdivisions to give the sphere.
			[Range(0.5f, 1f)] [PersistentValueChanged(nameof(OnGeneratePersistentChanged))] public float sphereRadius = 0.5f;// The radius of the sphere on instantiation.       
			[PersistentValueChanged(nameof(OnGeneratePersistentChanged))] public ExtrudeMethod extrudeMethod = ExtrudeMethod.IndividualFaces;
			[Range(0f, 1f)] [PersistentValueChanged(nameof(OnGeneratePersistentChanged))] public float idleExtrusion = .01f;// How far along the normal should each face be extruded when at idle (no audio input).

			[Header("Update Setting")]
			[JsonIgnore] public Material sphereMaterial;//Sphere's Material
			[JsonIgnore] public List<Material> listMaterialPreset = new List<Material>();//Sphere's redefine materials
			[PersistentOption(nameof(listMaterialPreset), nameof(sphereMaterial))] [PersistentValueChanged(nameof(OnMaterialChanged))] public int curMaterialPresetIndex = 0;
			[Range(0, 1f)] public float maxExtrusion = 0.1f;// The max distance a frequency range will extrude a face.
			[Range(8, 128)] public int fftBounds = 32;// An FFT returns a spectrum including frequencies that are out of human hearing range. This restricts the number of bins used from the spectrum to the lower bounds.

			[PersistentValueChanged(nameof(OnWaveformSettingChanged))] public bool showWaveform = true;
			[EnableIf(nameof(showWaveform))] [PersistentValueChanged(nameof(OnWaveformSettingChanged))] public Gradient waveformGradient = new Gradient();
			[EnableIf(nameof(showWaveform))] [PersistentValueChanged(nameof(OnWaveformSettingChanged))] public float waveformWidthMultiplier = .5f;// The widthMultiplier of the waveform.
			[EnableIf(nameof(showWaveform))] public float waveformRadius = .6f;// How far from the sphere should the waveform be.
			[EnableIf(nameof(showWaveform))] public float waveformHeight = 0.2f;// The y size of the waveform.
			[EnableIf(nameof(showWaveform))] public bool rotateWaveformRing = false;// If true, the waveform ring will rotate around self.       
			[EnableIf(EConditionOperator.And, new string[] { nameof(showWaveform), nameof(rotateWaveformRing) })] public Vector3 waveformRotateSpeed = new Vector3(0f, .01f, 0f);//Waveform ring's rotate speed.

			void OnGeneratePersistentChanged(PersistentChangeState persistentChangeState) { actionGeneratePersistentChanged.Execute(persistentChangeState); }
			void OnMaterialChanged(PersistentChangeState persistentChangeState) { actionMaterialChanged.Execute(persistentChangeState); }
			void OnWaveformSettingChanged(PersistentChangeState persistentChangeState) { actionWaveformSettingChanged.Execute(persistentChangeState); }
		}
		#endregion
	}

}