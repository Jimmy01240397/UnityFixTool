using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class FixAnimationPath : EditorWindow
{
    /// <summary>
    /// 需要改变的物体
    /// </summary>
    private GameObject target;

    private string error;

    private int cont = 0;

    private Vector2 scroll;

    private AnimationClip[] ac = new AnimationClip[0];

    [MenuItem("Window/AnimationFix/Fix Animation Path", false,1)]
    static void FixAnimationPathMethod()
    {
        Rect wr = new Rect(0, 0, 500, 500);
        FixAnimationPath window = (FixAnimationPath)EditorWindow.GetWindowWithRect(typeof(FixAnimationPath), wr, true, "Fix Animation");
        window.Show();
    }

    bool DoFix()
    {
        //AnimationClip ac = Selection.activeObject as AnimationClip;
        {
            List<AnimationClip> animations = new List<AnimationClip>(ac);
            if (animations.Contains(null))
            {
                error = "AnimationClip缺失";
                return false;
            }
        }
        if (target == null)
        {
            error = "Target丢失";
            return false;
        }

        GameObject root = target;
        int a = 0;
        //获取所有绑定的EditorCurveBinding(包含path和propertyName)
        foreach (AnimationClip animation in ac)
        {
            if (animation != null)
            {
                EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(animation);

                for (int i = 0; i < bindings.Length; ++i)
                {
                    EditorCurveBinding binding = bindings[i];

                    Keyframe[] keyframes = AnimationUtility.GetEditorCurve(animation, binding).keys;
                    foreach(Keyframe keyframe in keyframes)
                    {
                        Debug.Log("Keyframe " + binding.propertyName + ":" + keyframe.value);
                    }

                    Transform bindTransform = root.transform.Find(binding.path);
                    if (!bindTransform)
                    {
                        GameObject bindObj = FindInChildren(root, binding.path);
                        if (bindObj)
                        {
                            string newPath = AnimationUtility.CalculateTransformPath(bindObj.transform, root.transform);
                            Debug.Log("change " + binding.path + " to " + newPath);

                            AnimationCurve curve = AnimationUtility.GetEditorCurve(animation, binding);

                            //remove Old
                            AnimationUtility.SetEditorCurve(animation, binding, null);

                            binding.path = newPath;

                            AnimationUtility.SetEditorCurve(animation, binding, curve);
                        }
                    }
                }
            }
            else
            {
                Debug.Log("animation[" + a + "] == null");
            }
            a++;
        }
        return true;
    }

    GameObject FindInChildren(GameObject obj, string goName)
    {
        Transform objTransform = obj.transform;

        GameObject finded = null;
        Transform findedTransform = objTransform.Find(goName.Split('/')[goName.Split('/').Length - 1]);

        if (findedTransform == null)
        {
            for (int i = 0; i < objTransform.childCount; ++i)
            {
                finded = FindInChildren(objTransform.GetChild(i).gameObject, goName.Split('/')[goName.Split('/').Length - 1]);
                if (finded)
                {
                    return finded;
                }
            }
            return null;
        }
        return findedTransform.gameObject;
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("TargetRoot");
        target = EditorGUILayout.ObjectField(target, typeof(GameObject), true) as GameObject;

        List<AnimationClip> animationClips = new List<AnimationClip>();

        EditorGUILayout.LabelField("AnimationClipCount");
        
        cont = EditorGUILayout.IntField(cont);

        if (GUILayout.Button("GetSize", GUILayout.Width(200)))
        {
            Array.Resize<AnimationClip>(ref ac, cont);
        }

        EditorGUILayout.LabelField("AnimationClip");
        scroll = GUILayout.BeginScrollView(scroll);
        for (int i = 0; i < ac.Length; i++)
        {
            animationClips.Add(EditorGUILayout.ObjectField(ac[i], typeof(AnimationClip), true) as AnimationClip);
        }
        GUILayout.EndScrollView();

        ac = animationClips.ToArray();

        if (GUILayout.Button("Fix", GUILayout.Width(200)))
        {
            if (this.DoFix())
            {
                this.ShowNotification(new GUIContent("Change Complete"));
            }
            else
            {
                this.ShowNotification(new GUIContent("Change Error " + error));
            }
        }
    }
}
