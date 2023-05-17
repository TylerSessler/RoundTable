using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEditor;

public class credits : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] float beginPosition;
    [SerializeField] float endPosition;
    [SerializeField] TextMeshProUGUI creditText;

    RectTransform moveRect;


    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("ScrollText");

        moveRect = gameObject.GetComponent<RectTransform>();
        StartCoroutine(ScrollText());

    }
    public void StartScrollText()
    {

    }

    IEnumerator ScrollText()
    {
        while (moveRect.localPosition.y < endPosition)
        {
            moveRect.Translate(Vector3.up * speed * Time.deltaTime);
            yield return null;
        }
    }
}
