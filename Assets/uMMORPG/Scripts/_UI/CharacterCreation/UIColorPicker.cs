using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIColorPicker : MonoBehaviour
{
	public static UIColorPicker singleton;
	public bool isDraggable = false;
	public Color pickedColor = Color.white;
	public GameObject pickerObj;
	[Range(0, 255)]
	public int alphaTolerancy = 0;

	void Start()
	{
		Reset();
	}

    public void Reset()
    {
		if (!singleton) singleton = this;
		Color actColor = GetComponent<Image>().sprite.texture.GetPixel(Mathf.RoundToInt((pickerObj.transform.position.x - transform.position.x) * (1 / GetComponent<RectTransform>().localScale.x) * (1 / GetComponentInParent<Canvas>().scaleFactor)), Mathf.RoundToInt((pickerObj.transform.position.y - transform.position.y) * (1 / GetComponent<RectTransform>().localScale.y) * (1 / GetComponentInParent<Canvas>().scaleFactor)) + GetComponent<Image>().sprite.texture.height);
		if (actColor.a >= ((255 - alphaTolerancy) / 255f))
		{
			pickedColor = actColor;
		}
	}
    public void OnEnter()
	{
		isDraggable = true;
	}

	public void OnExit()
	{
		isDraggable = false;
	}

	public void OnClick()
	{
		if (isDraggable)
		{
			Color actColor = GetComponent<Image>().sprite.texture.GetPixel(Mathf.RoundToInt((Input.mousePosition.x - transform.position.x) * (1 / GetComponent<RectTransform>().localScale.x) * (1 / GetComponentInParent<Canvas>().scaleFactor)), Mathf.RoundToInt((Input.mousePosition.y - transform.position.y) * (1 / GetComponent<RectTransform>().localScale.y) * (1 / GetComponentInParent<Canvas>().scaleFactor)) + GetComponent<Image>().sprite.texture.height);
			if (actColor.a >= ((255 - alphaTolerancy) / 255f))
			{
				pickedColor = actColor;
				pickerObj.transform.position = Input.mousePosition;
			}
		}
	}

}
