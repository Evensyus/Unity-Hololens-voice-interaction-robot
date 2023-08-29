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
                // 查找存在的实例
                instance = (T)FindObjectOfType(typeof(T));

                // 如果不存在实例，则创建
                if (instance == null)
                {
                    // 需要创建一个游戏对象，再把这个单例组件挂载到游戏对象上
                    var singletonObject = new GameObject();
                    instance = singletonObject.AddComponent<T>();
                    singletonObject.name = typeof(T).ToString() + " (Singleton)";

                    // 让实例不在切换场景时销毁
                    DontDestroyOnLoad(singletonObject);
                }
            }

            return instance;
        }
    }
}