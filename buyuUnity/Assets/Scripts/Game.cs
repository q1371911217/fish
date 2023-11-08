using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum eGameState
{
    Wait,
    Gaming,
    GetOver,
    boomOver,
}

public class Record
{
    string time;
    string mul;
    float win;
    public Record(string time, string mul, float win)
    {
        this.time = time;
        this.mul = mul ;
        this.win = win;
    }

    public string getTime()
    {
        return time;
    }

    public string getMul()
    {
        return mul;
    }

    public float getWin()
    {
        return win;
    }

}

public class Game : MonoBehaviour {

    [SerializeField]
    AudioSource audioSource;

    [SerializeField]
    List<AudioClip> clipList;

    [SerializeField]
    Text lblTopCoin, lblBetCoin;
    [SerializeField]
    InputField inputMul, inputCoin;
    [SerializeField]
    Transform svContent;
    [SerializeField]
    GameObject itemPref;
    [SerializeField]
    Text lblPower;
    [SerializeField]
    List<GameObject> mulRecordGoList;

    [SerializeField]
    GameObject btnGet, btnBet, btnVolum;

    [SerializeField]
    GameObject getAnim, notGetAnim;
    [SerializeField]
    Image playerImg;

    [SerializeField]
    GameObject helpLayer, page1,page2;

    [SerializeField]
    Text lblGet;


    float addRate = 0.1f;


    public static bool isLoad = false;

    float curMul = 3;//倍数默认3  1.01 - 1000
    float curCoin = 10000.00f; // 金币 
    int curBet = 1; // 1 - coin

    int betMin = 1;
    int coinMin = 1000;
    float coinDefault = 10000.00f;
    int recordMax = 30;
    float curPower;

    float startPower = 1.00f;    

    eGameState eState;

    public static bool volumOpen = true;

    List<Record> recordList = new List<Record>();
    List<string> mulRecordList = new List<string>();

    void Awake()
    {
        init();
    }

    void init()
    {
        inputMul.onEndEdit.AddListener((value) =>
        {
            if (string.IsNullOrEmpty(value))
                value = "3";

            if (!value.Contains("."))
            {
                curMul = float.Parse(value);
            }
            else
            {
                float tmp = float.Parse(value);
                curMul = Mathf.FloorToInt(tmp * 100) / 100f;
            }
            if (curMul < 1.01)
                curMul = 1.01f;
            else if (curMul > 1000)
                curMul = 1000;
            inputMul.text = curMul.ToString();
        });

        //inputMul.onValueChanged.AddListener((value)=>
        //{
        //    if (string.IsNullOrEmpty(value))
        //        value = "3";
           
        //    if(!value.Contains("."))
        //    {
        //        curMul = float.Parse(value);
        //    }
        //    else
        //    {    
        //        float tmp = float.Parse(value);
        //        curMul = Mathf.FloorToInt(tmp * 100) / 100f;
        //    }
        //    if (curMul < 1.01)
        //        curMul = 1.01f;
        //    else if (curMul > 1000)
        //        curMul = 1000;
        //    inputMul.text = curMul.ToString();
        //});

        inputCoin.onValueChanged.AddListener((value) =>
        {
            if (string.IsNullOrEmpty(value))
                value = "1";
            curBet = int.Parse(value);
            if (curBet < betMin)
                curBet = betMin;
            else if (curBet > curCoin)
                curBet = Mathf.FloorToInt(curCoin);
            inputCoin.text = curBet.ToString();
        });

        inputMul.text = curMul.ToString() ;
        inputCoin.text = curBet.ToString();

        lblTopCoin.text = string.Format("{0:N2}", curCoin);
        lblBetCoin.text = string.Format("{0:N2}", curCoin);

        eState = eGameState.Wait;
    }

    void updateMoney(float addCoin)
    {
        curCoin += addCoin;
        if (curCoin < coinMin)
            curCoin = coinDefault;
        lblTopCoin.text = string.Format("{0:N2}", curCoin);
        lblBetCoin.text = string.Format("{0:N2}", curCoin);
    }

    IEnumerator updateShow()
    {
        yield return new WaitForSeconds(1);

        playerImg.enabled = true;
        getAnim.SetActive(false);
        notGetAnim.SetActive(false);

        lblPower.text = "";
        //btnBet.gameObject.SetActive(true);
        //btnGet.gameObject.SetActive(false);
        btnGet.transform.localPosition = new Vector3(5000, 5000, 0);// gameObject.SetActive(false);
        btnBet.transform.localPosition = new Vector3(565, 3.814f, 0);// gameObject.SetActive(true);

        eState = eGameState.Wait;
    }

    void updateRecord()
    {
        for(int i = 0; i < recordList.Count;i++)
        {
            Transform cell;
            if(i < svContent.childCount)
            {
                cell = svContent.GetChild(i);
            }
            else
            {
                cell = GameObject.Instantiate(itemPref).transform;
                cell.gameObject.SetActive(true);
                cell.name = i.ToString();
                cell.SetParent(svContent);
                cell.localPosition = Vector3.zero;
                cell.localRotation = Quaternion.identity;
                cell.localScale = Vector3.one;
            }
            cell.Find("lblTime").GetComponent<Text>().text = recordList[i].getTime();
            cell.Find("lblCoin").GetComponent<Text>().text = recordList[i].getWin().ToString("F2");
            cell.Find("bgMul/Text").GetComponent<Text>().text = recordList[i].getMul();
        }
    }

    void updateMulRecord()
    {
        for(int i = 0; i < mulRecordList.Count; i++)
        {
            mulRecordGoList[i].SetActive(true);
            mulRecordGoList[i].transform.Find("lblMul").GetComponent<Text>().text = mulRecordList[i];
        }
    }

    int sec = 0;

    IEnumerator gameStart()
    {
        //yield return new WaitForSeconds(1);
        sec = 0;
        addRate = 0.1f;
        while (eState == eGameState.Gaming)
        {            
            if(UnityEngine.Random.Range(0, 101) < 5)//boom
            {
                eState = eGameState.boomOver;
                settle();
            }
            else
            {
                sec++;
                if(sec == 9)
                {
                    addRate += 0.1f;// (float)Math.Floor(curPower) / 10;
                    sec = 0;
                }
                    
                curPower += addRate;
                lblPower.text = string.Format("{0:N2}X", curPower);
                lblGet.text = ((float)Math.Ceiling(1000 * curPower * curBet) / 1000).ToString("F2");
                if (Math.Ceiling(curPower * 100) >= Math.Ceiling(curMul * 100))
                {
                    eState = eGameState.GetOver;
                    settle();
                    break;
                }
                else
                yield return new WaitForSeconds(0.1f);
            }
            
        }
    }

    void settle()
    {
        playerImg.enabled = false;
        if(eState == eGameState.boomOver)
        {
            audioSource.PlayOneShot(clipList[3]);
            notGetAnim.SetActive(true);

            string aa = string.Format("{0:N2}X", curPower);
            mulRecordList.Add(aa);
            if(mulRecordList.Count > 6)
            {
                mulRecordList.RemoveAt(0);
            }

            updateMulRecord();

        }
        else if(eState == eGameState.GetOver)
        {
            audioSource.PlayOneShot(clipList[2]);
            getAnim.SetActive(true);
            float win = (float)Math.Ceiling(1000 * curPower * curBet) / 1000;
            updateMoney(win);
            Record record = new Record(System.DateTime.Now.ToString("HH:mm:ss"), string.Format("X{0:N2}", curPower), win);
            recordList.Insert(0,record);
            if(recordList.Count > recordMax)
            {
                recordList.RemoveAt(0);
            }
            updateRecord();
        }

        StartCoroutine(updateShow());
    }

    public void onBtnClick(string btnName)
    {
        if (btnName == "btnHelp")
        {
            audioSource.PlayOneShot(clipList[0]);
            helpLayer.SetActive(true);
            page2.SetActive(false);
            page1.SetActive(true);
        }
        else if (btnName == "btnVolum")
        {
            audioSource.PlayOneShot(clipList[0]);
            volumOpen = !volumOpen;
            audioSource.volume = volumOpen ? 1 : 0;
            btnVolum.transform.Find("spDisable").gameObject.SetActive(!volumOpen);
        }
        else if (btnName == "btnHome")
        {
            audioSource.PlayOneShot(clipList[0]);
            SceneManager.LoadSceneAsync("LoginScene");
        }
        else if (btnName == "btnHalf")
        {
            audioSource.PlayOneShot(clipList[1]);
            if (eState != eGameState.Wait) return;
            curBet = Mathf.FloorToInt(curBet / 2);
            if (curBet < betMin)
            {
                curBet = betMin;
            }
            inputCoin.text = curBet.ToString();
        }
        else if (btnName == "btnDouble")
        {
            audioSource.PlayOneShot(clipList[1]);
            if (eState != eGameState.Wait) return;
            curBet = curBet * 2;
            if (curBet > curCoin)
                curBet = Mathf.FloorToInt(curCoin);
            inputCoin.text = curBet.ToString();
        }
        else if (btnName == "btnBet")
        {
            audioSource.PlayOneShot(clipList[0]);
            if (eState != eGameState.Wait) return;
            eState = eGameState.Gaming;
            curPower = startPower;
            lblPower.text = string.Format("{0:N2}X", curPower);
            updateMoney(-curBet);
            btnBet.transform.localPosition = new Vector3(5000, 5000, 0);// gameObject.SetActive(false);
            btnGet.transform.localPosition = new Vector3(565, 3.814f, 0);// gameObject.SetActive(true);

            StartCoroutine(gameStart());
        }
        else if(btnName == "btnGet")
        {
            audioSource.PlayOneShot(clipList[0]);
            if (eState == eGameState.Gaming)
            {
                StopCoroutine(gameStart());
                eState = eGameState.GetOver;
                settle();
            }
        }
        else if(btnName == "btnNext")
        {
            audioSource.PlayOneShot(clipList[0]);
            page1.SetActive(false);
            page2.SetActive(true);
        }
        else if(btnName == "btnHelpStart")
        {
            audioSource.PlayOneShot(clipList[0]);
            helpLayer.SetActive(false);
        }
        else if(btnName == "btnMax")
        {
            audioSource.PlayOneShot(clipList[1]);
            if (eState != eGameState.Wait) return;
            curBet = Mathf.FloorToInt(curCoin);
            inputCoin.text = curBet.ToString();
        }
        else if(btnName == "btnMin")
        {
            audioSource.PlayOneShot(clipList[1]);
            if (eState != eGameState.Wait) return;
            curBet = 1;
            inputCoin.text = curBet.ToString();
        }
    }
}
