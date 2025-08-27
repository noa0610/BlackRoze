using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HPManager : MonoBehaviour
{
    [SerializeField] private Image HPBar;
    [SerializeField] private TextMeshProUGUI HPText;
    [SerializeField] private float Max_HP = 100;
    private float S_HP;
    void Start()
    {
        HPBar.fillAmount = 1;
        S_HP = Max_HP;
        HPText.text = S_HP.ToString() + "/" + Max_HP.ToString();
    }


    void Update()
    {

        HPText.text = S_HP.ToString() + "/" + Max_HP.ToString();

        //!スペースキーを押すとダメージを受ける
        if (Input.GetKeyDown(KeyCode.Space))
        {
            int damage = Random.Range(1, 99);
            S_HP -= damage;
            HPText.text = S_HP.ToString() + "/" + Max_HP.ToString();
            HPBar.fillAmount = S_HP / Max_HP;
            Debug.Log("ダメージを受けた！" + damage + "のダメージ！");
            
        }
        if (S_HP <= 0)
        {
            S_HP = 0;
            Debug.Log("HPが0になった！");
        }

        //*エンターキーを押すと回復する
        if (Input.GetKeyDown(KeyCode.Return))
        {
            int heal = Random.Range(1, 99);
            S_HP += heal;
            Debug.Log("回復した！" + heal + "回復！");
            HPText.text = S_HP.ToString() + "/" + Max_HP.ToString();
            HPBar.fillAmount = S_HP / Max_HP;
        }
        if (S_HP >= Max_HP)
        {
            S_HP = Max_HP;
            Debug.Log("HPが最大値になった！");
        }

    }
    
}
