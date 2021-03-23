using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ScaleFixer : MonoBehaviour
{
    struct obpos
    {
        public Vector3 pos;
        public List<obpos> subobpos { get; private set; }
        public obpos(Vector3 pos)
        {
            this.pos = pos;
            subobpos = new List<obpos>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        obpos alldata = getpos(transform);

        Vector3 nowScale = Vector3.one;
        Fixing(alldata, nowScale, GetComponent<RectTransform>());

    }

    obpos getpos(Transform transform)
    {
        obpos outdata = new obpos(transform.position);
        for (int i = 0; i < transform.childCount; i++)
        {
            outdata.subobpos.Add(getpos(transform.GetChild(i)));
        }
        return outdata;
    }

    void Fixing(obpos alldata, Vector3 nowScale, RectTransform rectTransform)
    {
        Rect rect = rectTransform.rect;
        Rect prect = rectTransform.parent.GetComponent<RectTransform>().rect;
        Vector2 anchorMax = rectTransform.anchorMax;
        Vector2 anchorMin = rectTransform.anchorMin;
        Vector3 Scale = rectTransform.localScale;

        nowScale = new Vector3(nowScale.x * Scale.x, nowScale.y * Scale.y, nowScale.z * Scale.z);
        if (anchorMax.x == anchorMin.x)
        {
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x * nowScale.x, rectTransform.sizeDelta.y);
        }
        else if (anchorMax.x != anchorMin.x)
        {
            float xsize = rect.size.x * Scale.x;
            rectTransform.offsetMax = new Vector2(-prect.size.x / 2 + xsize, rectTransform.offsetMax.y);
            rectTransform.offsetMin = new Vector2(prect.size.x / 2, rectTransform.offsetMin.y);
        }
        if (anchorMax.y == anchorMin.y)
        {
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y * nowScale.y);
        }
        else if (anchorMax.y != anchorMin.y)
        {
            float ysize = rect.size.y * Scale.y;
            rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, -prect.size.y / 2 + ysize);
            rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, prect.size.y / 2);
        }

        if (rectTransform.GetComponent<Text>())
        {
            rectTransform.GetComponent<Text>().fontSize = (int)(rectTransform.GetComponent<Text>().fontSize * nowScale.x);
        }
        if (rectTransform.GetComponent<TextMeshPro>())
        {
            rectTransform.GetComponent<TextMeshPro>().fontSize = (int)(rectTransform.GetComponent<TextMeshPro>().fontSize * nowScale.x);
        }
        rectTransform.localScale = Vector3.one;

        rectTransform.transform.position = alldata.pos;

        for (int i = 0; i < rectTransform.childCount; i++)
        {
            Fixing(alldata.subobpos[i], nowScale, rectTransform.GetChild(i).GetComponent<RectTransform>());
        }
    }
}
