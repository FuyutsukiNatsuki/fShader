using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace fShader.Editor
{
    // Shared helpers for the Cold Mist wizards: resolving which scene object should
    // host the emitter when the user triggers creation while editing a material.
    internal static class fShaderColdMistShared
    {
        public static bool IsValidIceHost(GameObject go)
        {
            return go != null &&
                   go.GetComponent<MeshRenderer>() != null &&
                   go.GetComponent<MeshFilter>() != null;
        }

        // MeshRenderers in loaded scenes that use the given material and can host an emitter.
        public static List<GameObject> FindIceRenderers(Material material)
        {
            var result = new List<GameObject>();
            if (material == null)
            {
                return result;
            }
            foreach (MeshRenderer renderer in Object.FindObjectsOfType<MeshRenderer>(true))
            {
                if (renderer == null ||
                    renderer.GetComponent<MeshFilter>() == null ||
                    !renderer.gameObject.scene.IsValid())
                {
                    continue;
                }
                if (renderer.sharedMaterials != null && renderer.sharedMaterials.Contains(material))
                {
                    result.Add(renderer.gameObject);
                }
            }
            return result;
        }

        public static string HierarchyPath(GameObject go)
        {
            if (go == null)
            {
                return string.Empty;
            }
            string path = go.name;
            Transform current = go.transform.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }
            return path;
        }

        // Resolves the target host for a material-triggered creation and invokes create.
        // Uses the active selection if valid, otherwise the scene renderers using the material
        // (single -> direct, multiple -> context menu), else shows a guidance dialog.
        public static void CreateForMaterial(
            Material material,
            bool japanese,
            string dialogTitle,
            System.Action<GameObject> create)
        {
            if (IsValidIceHost(Selection.activeGameObject))
            {
                create(Selection.activeGameObject);
                return;
            }

            List<GameObject> candidates = FindIceRenderers(material);
            if (candidates.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    dialogTitle,
                    japanese
                        ? "このマテリアルを使うメッシュがシーンにありません。Ice本体（MeshRenderer + MeshFilter付きGameObject）をシーンに配置し、Hierarchyで選択してから押してください。"
                        : "No mesh in the loaded scene uses this material. Place an Ice object (GameObject with MeshRenderer + MeshFilter) in the scene, select it, then press again.",
                    "OK");
                return;
            }
            if (candidates.Count == 1)
            {
                create(candidates[0]);
                return;
            }

            var menu = new GenericMenu();
            foreach (GameObject candidate in candidates)
            {
                GameObject target = candidate;
                menu.AddItem(new GUIContent(HierarchyPath(target)), false, () => create(target));
            }
            menu.ShowAsContext();
        }
    }
}
