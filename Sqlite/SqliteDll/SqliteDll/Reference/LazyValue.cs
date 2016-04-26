using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Code.External.Engine.Sqlite
{
    public class LazyValue<T> : IRoutine
    {

        protected T value;
        protected bool isDone;
        protected IEnumerator routine;
        protected Func<T> valueFactory;
        protected bool isLoading;


        public LazyValue(Func<T> valueFactory)
            : this(valueFactory, null)
        {
        }
        public LazyValue(Func<T> valueFactory, IEnumerator routine)
        {


            if (valueFactory == null)
                throw new ArgumentNullException("valueFactory");

            this.routine = routine;
            this.valueFactory = valueFactory;
        }

        public T Value
        {
            get
            {
                if (!isDone)
                {
                    if (routine != null)
                        throw new MemberAccessException("routine not null, Value未初始化");
                    value = valueFactory();
                    isDone = true;
                }
                return value;
            }
        }


        #region ICoroutine 成员

        public bool IsDone
        {
            get { return isDone; }
        }


        public IEnumerator Routine
        {
            get
            {
                return ToRoutine();
            }
        }

        private IEnumerator ToRoutine()
        {
            if (isDone)
                yield break;
            //if (Extensions.debug)
            //{
            //    Debug.Log("lazval isloading:" + isLoading);
            //}
            if (isLoading)
            {
                //可多等待
                //WaitForEndOfFrame waitFrame = new WaitForEndOfFrame();
                //while (this.isLoading)
                //yield return waitFrame;
                while (isLoading)
                    yield return null;
                yield break;
            }

            isLoading = true;

            if (routine != null)
            {
                while (true)
                {
                    try
                    {
                        if (!routine.MoveNext())
                            break;
                    }
                    catch
                    {
                        isLoading = false;
                        throw;
                    }
                    yield return routine.Current;

                }
            }

            try
            {

                value = valueFactory();
            }
            catch
            {
                throw;
            }
            finally
            {
                //routine = null;
                valueFactory = null;

                isDone = true;
                isLoading = false;
            }
        }

        #endregion
    }
    public class LazyValue<T, TMetadata> : LazyValue<T>
    {
        private TMetadata metadata;
        public LazyValue(Func<T> valueFactory, TMetadata metadata)
            : this(valueFactory, null, metadata)
        {
        }
        public LazyValue(Func<T> valueFactory, IEnumerator routine, TMetadata metadata)
            : base(valueFactory, routine)
        {
            this.metadata = metadata;
        }

        public TMetadata Metadata
        {
            get { return metadata; }
        }

        protected void Rest(Func<T> valueFactory, IEnumerator routine, TMetadata metadata)
        {
            this.valueFactory = valueFactory;
            this.routine = routine;
            this.metadata = metadata;
        }
    }
}
