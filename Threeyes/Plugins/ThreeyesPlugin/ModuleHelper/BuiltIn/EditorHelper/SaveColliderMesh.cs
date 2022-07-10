#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
/// <summary>
/// 保存模型的Mesh
/// （适用于保存通过SAMeshBuilder创建的临时Mesh，避免重新打开场景后丢失引用）
/// </summary>
public class SaveColliderMesh : MonoBehaviour
{
    public Transform colliderObject;//挂载脚本的游戏体

    public string folderName;//mesh存放的文件夹名

    [ContextMenu("SaveAsset")]
    public void SaveAsset()
    {   
            Mesh mesh = colliderObject.GetComponent<MeshCollider>().sharedMesh;
            AssetDatabase.CreateAsset(mesh,"Assets/" + folderName + "/" + name + ".asset");
            Debug.Log("Assets/" + folderName + "/" + name + ".asset");
    }
}
#endif