using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;



namespace Code.External.Engine.Sqlite
{
    public class MonoBehaviourHelper : MonoBehaviour
    {

        public Action OnGUIEvent;
        public Action OnDestroyEvent;
        public Action OnUpdate;

        private float realTime;
        private float realDeltaTime;

        private static MonoBehaviourHelper _instance;

        void Awake()
        {
            if (!_instance)
            {
                realTime = Time.realtimeSinceStartup;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Update()
        {
            float rt = Time.realtimeSinceStartup;
            realDeltaTime = rt - realTime;
            realTime = rt;
            if (OnUpdate != null)
                OnUpdate();
        }

        void OnDestroy()
        {

            if (OnDestroyEvent != null)
                OnDestroyEvent();
            OnGUIEvent = null;
            OnDestroyEvent = null;
        }

        void OnGUI()
        {
            if (OnGUIEvent != null)
                OnGUIEvent();

        }



        public static MonoBehaviourHelper Instance
        {
            get
            {
                //Debug.Log((_instance == null) + ", _instance null");
                if (_instance)
                    return _instance;

                GameObject go = new GameObject(typeof(MonoBehaviourHelper).Name);

                _instance = go.AddComponent<MonoBehaviourHelper>();
                //_instance.OnGUIEvent += _OnGUIEvent;
                //_instance.OnDestroyEvent += _OnDestroyEvent;

                GameObject.DontDestroyOnLoad(go);
                go.hideFlags = HideFlags.HideInHierarchy;
                UnityEngine.Object.DontDestroyOnLoad(_instance);

                return _instance;
            }
        }


        public static Coroutine StartCoroutine(IEnumerator routine)
        {
            return Instance.StartCoroutineEx(routine);
        }
        public static Coroutine StartCoroutine(IRoutine routine)
        {
            return Instance.StartCoroutineEx(routine);
        }


        static public float RealTime
        {
            get
            {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return Time.realtimeSinceStartup;
#endif

                return Instance.realTime;
            }
        }

        public static float RealDeltaTime
        {
            get
            {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return 0f;
#endif

                return Instance.realDeltaTime;
            }
        }



        //private static void _OnGUIEvent()
        //{

        //    if (OnGUI != null)
        //        OnGUI();
        //}
        //private static void _OnDestroyEvent()
        //{ 
        //    if (OnDestroy != null)
        //        OnDestroy();
        //    OnGUI = null;
        //    OnDestroy = null;

        //}


        private static IEnumerator ExecForeach<T>(T[] part, Func<T, IEnumerator> run)
        {
            if (part == null)
                yield break;
            foreach (T item in part)
            {
                yield return run(item);
            }
        }

        public static IEnumerator Foreach<T>(IEnumerable<T> parts, Func<T, IEnumerator> run, int maxParallel = 3)
        {
            T[][] items;

            var tmp = parts.ToArray();
            if (tmp.Length <= 0)
                yield break;

            maxParallel = Mathf.Clamp(maxParallel, 1, tmp.Length);


            items = new T[maxParallel][];
            int avg = tmp.Length / maxParallel;
            for (int i = 0; i < maxParallel; i++)
            {
                if (i == maxParallel - 1)
                {
                    items[i] = tmp.Skip(avg * i).ToArray();
                }
                else
                {
                    items[i] = tmp.Skip(avg * i).Take(avg).ToArray();
                }
            }


            Coroutine[] coroutines = new Coroutine[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                coroutines[i] = StartCoroutine(ExecForeach(items[i], run));
            }

            foreach (Coroutine c in coroutines)
            {
                yield return c;
            }
        }



        static DateTime startTime;
        static int startFrameCount = -1;
        static int targetFrameCount = 10;
        static float timePerFrame = 1f / targetFrameCount;
        static bool waitNextFrame;


        private static bool UpdateTime()
        {
            DateTime now;
            if (startFrameCount != Time.frameCount)
            {

                now = DateTime.Now;
                if (Time.frameCount == startFrameCount + 1)
                {
                    float frameTime = (float)now.Subtract(startTime).TotalSeconds;
                    frameTime = frameTime - timePerFrame;
                    if (frameTime > 0)
                    {
                        startTime = now.AddSeconds(timePerFrame - frameTime);
                    }
                    else
                    {
                        startTime = now.AddSeconds(timePerFrame + frameTime);
                    }
                }
                else
                {
                    startTime = now.AddSeconds(timePerFrame);
                }

                startFrameCount = Time.frameCount;
                waitNextFrame = false;

            }

            if (!waitNextFrame)
            {
                if (DateTime.Now > startTime)
                {

                    waitNextFrame = true;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        //public static IEnumerator ToNonBlocking(Action run)
        //{
        //    if (UpdateTime())
        //    {
        //        run();
        //        return null;
        //    }
        //    return _ToNonBlocking(run);
        //}

        private static int runCount;

        public static IEnumerator ToNonBlocking(Action run)
        {

            //Debug.Log(runCount++);
            //yield return null;


            while (true)
            {
                if (UpdateTime())
                    break;

                //Debug.Log("blocking frame " + Time.frameCount);
                yield return null;
            }

            run();
            //runCount--;
        }

    }
}