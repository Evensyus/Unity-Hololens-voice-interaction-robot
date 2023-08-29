using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class baiduai_02 : MonoBehaviour
{
    
    public static Text RobotTalkBalk;
    //public static string wen;
    public static Text AskText;

    void Start()
    {
        RobotTalkBalk = GameObject.Find("Lunarcom/Terminal/Output/Placeholder").GetComponent<Text>();
        //Utterance.unit_utterance();
    }

    public void Ask()
    {
        AskText = GameObject.Find("Lunarcom/Terminal/OutputText").GetComponent<Text>();
        Utterance.unit_utterance();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public static class AccessToken
    {
        // ����getAccessToken()��ȡ�� access_token�������expires_in ʱ�� ���û���
        // ����tokenʾ��
        public static String TOKEN = "24.adda70c11b9786206253ddb70affdc46.2592000.1493524354.282335-1234567";
        // �ٶ����п�ͨ��Ӧ����Ӧ�õ� API Key ���鿪ͨӦ�õ�ʱ���ѡ����
        private static String clientId = "pzGIlbBor7G5B7XGfdfW3q1u";
        // �ٶ����п�ͨ��Ӧ����Ӧ�õ� Secret Key
        private static String clientSecret = "YGBa3dCOBdpScgZdItl5QYGAH4QgRcL9";
        public static String getAccessToken()
        {
            String authHost = "https://aip.baidubce.com/oauth/2.0/token";
            HttpClient client = new HttpClient();
            List<KeyValuePair<String, String>> paraList = new List<KeyValuePair<string, string>>();
            paraList.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
            paraList.Add(new KeyValuePair<string, string>("client_id", clientId));
            paraList.Add(new KeyValuePair<string, string>("client_secret", clientSecret));
            HttpResponseMessage response = client.PostAsync(authHost, new FormUrlEncodedContent(paraList)).Result;
            String result = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine(result);
            Debug.Log(result+"a");
            //TOKEN = result;
            return result;
        }
    }
    public class Utterance
    {
        // unit�Ի��ӿ�
        public static string unit_utterance()
        {
            JObject jo = (JObject)JsonConvert.DeserializeObject(AccessToken.getAccessToken());
            string token = jo["access_token"].ToString();
            //string message = jo["Message"].ToString();
            // string token = AccessToken.getAccessToken();//token�Ѿ���ȡ
            string host = "https://aip.baidubce.com/rpc/2.0/unit/service/v3/chat?access_token=" + token;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(host);
            request.Method = "post";
            request.ContentType = "application/json";
            request.KeepAlive = true;
            //�ĳ��Լ��ģ�
            //string wen = "���ݽ�������";
            string wen = AskText.text;
            string str = "{\"version\":\"3.0\",\"service_id\":\"S98384\",\"session_id\":\"\",\"log_id\":\"7758521\",\"request\":{\"terminal_id\":\"88888\",\"query\":\"" + wen + "\"}}"; // json��ʽ  S10000 ��Ҫ�Լ��ģ�
            byte[] buffer = Encoding.UTF8.GetBytes(str);
            request.ContentLength = buffer.Length;
            request.GetRequestStream().Write(buffer, 0, buffer.Length);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string result = reader.ReadToEnd();
            Console.WriteLine("�Ի��ӿڷ���:");
            // Debug.Log("________json________result______________________"+result);//��һ��json��ʽ
            Console.WriteLine(result);
            //����json
            /*
            1.Json�ַ���Ƕ�׸�ʽ����
            string jsonText = "{\"beijing\":{\"zone\":\"����\",\"zone_en\":\"haidian\"}}";
            JObject jo = (JObject)JsonConvert.DeserializeObject(jsonText);
            string zone = jo["beijing"]["zone"].ToString();
            string zone_en = jo["beijing"]["zone_en"].ToString();       
            2.Json�ַ��������ʽ����
            string jsonArrayText = "[{'a':'a1','b':'b1'},{'a':'a2','b':'b2'}]"; //"[{'a':'a1','b':'b1'}]��ʹֻ��һ��Ԫ�أ�Ҳ��Ҫ����[]
            string jsonArrayText = "[{\"a\":\"a1\",\"b\":\"b1\"},{\"a\":\"a2\",\"b\":\"b2\"}]";  //����д���ʹ�д��Ч��һ��
            JArray jArray = (JArray)JsonConvert.DeserializeObject(jsonArrayText);//jsonArrayText�����Ǵ�[]�����ʽ�ַ���
            string str = jArray[0]["a"].ToString();
             */
            //Json�ַ���Ƕ�׸�ʽ����
            Debug.Log(result);
            JObject jresult = (JObject)JsonConvert.DeserializeObject(result);
            string sjresult = jresult["result"]["responses"].ToString();
            Debug.Log(sjresult);

            //Json�ַ��������ʽ����
            JArray jArray = (JArray)JsonConvert.DeserializeObject(sjresult);
            string jactions = jArray[0]["actions"].ToString();
            Debug.Log(jactions);

            //Json�ַ��������ʽ����
            JArray jArray1 = (JArray)JsonConvert.DeserializeObject(jactions);
            string jactions1 = jArray1[0]["say"].ToString();
            Debug.Log(jactions1);

            //private Text RobotTalkBalk = GameObject.Find("Lunarcom/Terminal/Output/Out").GetComponent<Text>();
            RobotTalkBalk.text = jactions1;

            //ӽ�죬�ƣ�����������죬��������衣��ë����ˮ�����Ʋ��岨��
            ///����һ�ֽ�����ʽ��///
            /*            
                JObject resultObj = (JObject)JsonConvert.DeserializeObject(result);
                string actions = resultObj["result"]["responses"].ToString();

                JArray sayObj = (JArray)JsonConvert.DeserializeObject(actions);
                string responses = sayObj[0].ToString();
            //���𴦣�
                JObject actionsObj = (JObject)JsonConvert.DeserializeObject(responses);
                string sayobj = actionsObj.GetValue("actions")[0].ToString();

                JObject ac = (JObject)JsonConvert.DeserializeObject(sayobj);
                string acc = ac.GetValue("say").ToString();
            */
            return result;
        }
    }
}

