using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Code.External.Engine.Sqlite
{
    public static partial class Extensions
    {
        public static bool AutoIEnumerableAsCoroutine = true;
        public static bool AutoIEnumeratorAsCoroutine = true;

        public static Coroutine StartCoroutineEx(this MonoBehaviour source, IEnumerator routine)
        {
            if (routine == null)
                return source.StartCoroutine(EmptyRoutine);

            return source.StartCoroutine(AsCoroutine(source, routine, true));
        }
        /*  public static Coroutine StartCoroutineEx(this MonoBehaviour source, IRoutine routine)
          {
              IEnumerator r = null;
              if (routine != null)
                  r = routine.Routine;

              return StartCoroutineEx(source, r);
          }*/
        public static Coroutine StartCoroutineEx(this MonoBehaviour source, IDone routine)
        {
            IEnumerator r = null;


            if (routine != null)
            {
                if (routine is IRoutine)
                    r = ((IRoutine)routine).Routine;
                else
                    r = AsCoroutine(routine);
            }

            return StartCoroutineEx(source, r);
        }
        private static readonly IEnumerator EmptyRoutine = MakeEmptyRoutine();
        private static IEnumerator MakeEmptyRoutine()
        {
            yield break;
        }
        private static WaitForEndOfFrame waitFrame = new WaitForEndOfFrame();
        public static bool debug;
        private static IEnumerator AsCoroutine(IDone source)
        {
            if (source == null || source.IsDone)
                yield break;
            while (!source.IsDone)
                yield return null;
        }
        private static IEnumerator AsCoroutine(MonoBehaviour behaviour, IEnumerator source, bool first, int depth = 0)
        {
            object current;
            IEnumerator it = null;

            while (source.MoveNext())
            {
                current = source.Current;

                if (current == null)
                {
                    //default is wait one frame
                    yield return null;
                    continue;
                }

                it = null;


                if (current is IRoutine)
                {
                    IRoutine r = current as IRoutine;
                    it = r.Routine;
                    if (it == null)
                        continue;
                }
                else if (current is IDone)
                {
                    var done = (IDone)current;
                    if (!done.IsDone)
                    {
                        while (!done.IsDone)
                            yield return null;
                    }
                    continue;
                }
                else if ((current is IEnumerator) && AutoIEnumeratorAsCoroutine)
                {
                    it = (IEnumerator)current;
                }
                else if ((current is IEnumerable) && AutoIEnumerableAsCoroutine)
                {
                    it = ((IEnumerable)current).GetEnumerator();
                    if (it == null)
                        continue;
                }


                if (it != null)
                {
                    it = AsCoroutine(behaviour, it, false, depth + 1);

                    if (it.MoveNext())
                    {

                        do
                        {

                            yield return it.Current;
                        } while (it.MoveNext());

                    }

                    continue;
                }

                yield return current;
            }
        }

    }
}
