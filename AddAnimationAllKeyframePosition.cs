using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class AddAnimationAllKeyframePosition : EditorWindow
{
    /// <summary>
    /// 需要改变的物体
    /// </summary>
    private GameObject target;

    private string error;

    private int cont = 0;

    private Vector2 scroll;

    private Vector3 Add = new Vector3();

    private AnimationClip[] ac = new AnimationClip[0];

    [MenuItem("Window/AnimationFix/Add Animation All Keyframe Position", false,1)]
    static void AddAnimationAllKeyframePositionMethod()
    {
        Rect wr = new Rect(0, 0, 500, 500);
        AddAnimationAllKeyframePosition window = (AddAnimationAllKeyframePosition)EditorWindow.GetWindowWithRect(typeof(AddAnimationAllKeyframePosition), wr, true, "Add Animation All Keyframe Position");
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

                    if (binding.path.Split('/')[binding.path.Split('/').Length - 1] == root.name && binding.propertyName.Split('.')[0] == "m_LocalPosition")
                    {
                        AnimationCurve curve = AnimationUtility.GetEditorCurve(animation, binding);

                        Keyframe[] keyframes = curve.keys;
                        for (int ii = 0; ii < keyframes.Length;ii++)
                        {
                            float ve = keyframes[ii].value;
                            keyframes[ii].value += binding.propertyName.Split('.')[1] == "x" ? Add.x : (binding.propertyName.Split('.')[1] == "y" ? Add.y : (binding.propertyName.Split('.')[1] == "z" ? Add.z : 0));
                            Debug.Log("change Keyframe " + binding.propertyName + ":" + ve + " to " + keyframes[ii].value);
                        }
                        AnimationUtility.SetEditorCurve(animation, binding, null);

                        curve.keys = keyframes;

                        AnimationUtility.SetEditorCurve(animation, binding, curve);
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

    void OnGUI()
    {
        EditorGUILayout.LabelField("TargetRoot");
        target = EditorGUILayout.ObjectField(target, typeof(GameObject), true) as GameObject;

        Add = EditorGUILayout.Vector3Field("AddPosition", Add);

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
