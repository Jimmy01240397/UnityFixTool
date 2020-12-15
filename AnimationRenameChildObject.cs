using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class AnimationRenameChildObject : EditorWindow
{
    /// <summary>
    /// 需要改变的物体
    /// </summary>
    private GameObject target;

    private string error;

    private int cont = 0;

    private Vector2 scroll;

    private AnimationClip[] ac = new AnimationClip[0];

    private string fromname = "";

    private string toname = "";

    [MenuItem("Window/AnimationFix/Animation Rename Child Object", false,1)]
    static void AnimationRenameChildObjectMethod()
    {
        Rect wr = new Rect(0, 0, 500, 500);
        AnimationRenameChildObject window = (AnimationRenameChildObject)EditorWindow.GetWindowWithRect(typeof(AnimationRenameChildObject), wr, true, "Animation Rename Child Object");
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

        if (fromname == toname)
        {
            error = "Name是一樣的";
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

                    string _name = binding.path.Split('/')[binding.path.Split('/').Length - 1];

                    if(_name == fromname)
                    {
                        string newpath = fromname == "" ? toname : (toname == "" ? binding.path.Replace(fromname + "/", toname) : binding.path.Replace(fromname, toname));
                        Debug.Log("change " + binding.path + " to " + newpath);

                        AnimationCurve curve = AnimationUtility.GetEditorCurve(animation, binding);

                        //remove Old
                        AnimationUtility.SetEditorCurve(animation, binding, null);

                        binding.path = newpath;

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

    GameObject FindInChildren(GameObject obj, string goName)
    {
        Transform objTransform = obj.transform;

        GameObject finded = null;
        Transform findedTransform = objTransform.Find(goName);

        if (findedTransform == null)
        {
            for (int i = 0; i < objTransform.childCount; ++i)
            {
                finded = FindInChildren(objTransform.GetChild(i).gameObject, goName);
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

        EditorGUILayout.LabelField("FromName");

        fromname = EditorGUILayout.TextField(fromname);

        EditorGUILayout.LabelField("ToName");

        toname = EditorGUILayout.TextField(toname);

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
