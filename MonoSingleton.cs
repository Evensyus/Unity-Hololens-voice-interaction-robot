using UnityEngine;
public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                // ���Ҵ��ڵ�ʵ��
                instance = (T)FindObjectOfType(typeof(T));

                // ���������ʵ�����򴴽�
                if (instance == null)
                {
                    // ��Ҫ����һ����Ϸ�����ٰ��������������ص���Ϸ������
                    var singletonObject = new GameObject();
                    instance = singletonObject.AddComponent<T>();
                    singletonObject.name = typeof(T).ToString() + " (Singleton)";

                    // ��ʵ�������л�����ʱ����
                    DontDestroyOnLoad(singletonObject);
                }
            }

            return instance;
        }
    }
}