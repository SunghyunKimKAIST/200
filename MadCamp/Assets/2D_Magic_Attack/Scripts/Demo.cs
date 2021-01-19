using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Demo : MonoBehaviour,IPointerClickHandler {

	public GameObject[] Effects = new GameObject[0];
	public Text EffectText;
	[System.NonSerialized] public static int No;
	private Vector3 clickPosition;
	// Use this for initialization
	void Start () {
		No = 0;
		TextChange();
	}

	public void TextChange(){
		EffectText.text = Effects[No].name;
	}
	
	public void OnPointerClick (PointerEventData eventData)
	{
			clickPosition = Input.mousePosition;
			clickPosition.z = 10f;
			GameObject obj = Instantiate(Effects[No], Camera.main.ScreenToWorldPoint(clickPosition), Effects[No].transform.rotation);
			EffectText.text = Effects[No].name;
			Destroy(obj,3f);
	}
}



IEnumerator SpellStart2()
{
    do
    {
        float oneShoting = 50f;
        float angle = 360 / oneShoting;
        for (int j = 0; j < 10; j++)
        {
            for (int i = 0; i < oneShoting; i++)
            {
                //Debug.Log(i);
                GameObject obj;
                obj = (GameObject)Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                //보스의 위치에 bullet을 생성합니다.
                obj.AddComponent<Rigidbody2D>().gravityScale = 0;
                obj.GetComponent<bullet>().isBossBullet = true;
                obj.GetComponent<Rigidbody2D>().AddForce(new Vector2(500f * Mathf.Cos(Mathf.PI * 2 * i / oneShoting), 500f * Mathf.Sin(Mathf.PI * i * 2 / oneShoting)));
                //Debug.Log(speed*Mathf.Cos(Mathf.PI*2*i/oneShoting));
                obj.transform.Rotate(new Vector3(0f, 0f, 360 * i / oneShoting - 90));
                yield return new WaitForSeconds(0.01f);
            }
        }
        yield return new WaitForSeconds(10f);
    } while (true);