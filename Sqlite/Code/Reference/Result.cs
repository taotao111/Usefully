using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Code.External.Engine.Sqlite
{

    public class Result : IRoutine
    {
        private bool isSuccess;
        private string error;
        private IEnumerator routine;
        private bool isDone;
        private long code;

        public Result()
        {
            this.isDone = true;
        }

        public bool IsDone
        {
            get { return isDone; }
            protected set
            {
                isDone = value;
            }
        }

        public IEnumerator Routine
        {
            get { return routine; }
            set
            {
                routine = value;
                IsDone = (routine == null);
            }
        }

        public bool Success
        {
            get
            {
                CheckDone();
                return isSuccess;
            }
            set
            {
                isSuccess = value;
                IsDone = true;
            }

        }


        public string Error
        {
            get
            {
                CheckDone();
                return error;
            }
            set
            {
                error = value;
                IsDone = true;
            }
        }

        public long Code
        {
            get { return code; }
            set
            {
                code = value;
                IsDone = true;
            }
        }

        protected void CheckDone()
        {
            if (!IsDone)
                throw new Exception("result no done");
        }


        /// <summary>
        /// 有错误返回true
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <returns></returns>
        public bool CopyError(Result dst)
        {
            if (Success)
                return false;
            dst.Success = Success;
            dst.Code = Code;
            dst.Error = Error;
            return true;
        }



        #region From




        public static Result From(Func<Result, IEnumerator> func)
        {
            Result result = new Result();
            result.Routine = func(result);
            return result;
        }

        public static Result From<T1>(T1 t1, Func<T1, Result, IEnumerator> func)
        {
            Result result = new Result();
            result.Routine = func(t1, result);
            return result;
        }
        public static Result From<T1, T2>(T1 t1, T2 t2, Func<T1, T2, Result, IEnumerator> func)
        {
            Result result = new Result();
            result.Routine = func(t1, t2, result);
            return result;
        }


        public static Result From<T1, T2, T3>(T1 t1, T2 t2, T3 t3, Func<T1, T2, T3, Result, IEnumerator> func)
        {
            Result result = new Result();
            result.Routine = func(t1, t2, t3, result);
            return result;
        }

        public static Result From<T1, T2, T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4, Func<T1, T2, T3, T4, Result, IEnumerator> func)
        {
            Result result = new Result();
            result.Routine = func(t1, t2, t3, t4, result);
            return result;
        }

        public static Result From<T1, T2, T3, T4, T5>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, Func<T1, T2, T3, T4, T5, Result, IEnumerator> func)
        {
            Result result = new Result();
            result.Routine = func(t1, t2, t3, t4, t5, result);
            return result;
        }



        public static Result<TResult> From<TResult>(Func<Result<TResult>, IEnumerator> func)
        {
            Result<TResult> result = new Result<TResult>();
            result.Routine = func(result);
            return result;
        }

        public static Result<TResult> From<T1, TResult>(T1 args1, Func<T1, Result<TResult>, IEnumerator> func)
        {
            Result<TResult> result = new Result<TResult>();
            result.Routine = func(args1, result);
            return result;
        }

        public static Result<TResult> From<T1, T2, TResult>(T1 t1, T2 t2, Func<T1, T2, Result<TResult>, IEnumerator> func)
        {
            Result<TResult> result = new Result<TResult>();
            result.Routine = func(t1, t2, result);
            return result;
        }

        public static Result<TResult> From<T1, T2, T3, TResult>(T1 t1, T2 t2, T3 t3, Func<T1, T2, T3, Result<TResult>, IEnumerator> func)
        {
            Result<TResult> result = new Result<TResult>();
            result.Routine = func(t1, t2, t3, result);
            return result;
        }

        public static Result<TResult> From<T1, T2, T3, T4, TResult>(T1 t1, T2 t2, T3 t3, T4 t4, Func<T1, T2, T3, T4, Result<TResult>, IEnumerator> func)
        {
            Result<TResult> result = new Result<TResult>();
            result.Routine = func(t1, t2, t3, t4, result);
            return result;
        }

        public static Result<TResult> From<T1, T2, T3, T4, T5, TResult>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, Func<T1, T2, T3, T4, T5, Result<TResult>, IEnumerator> func)
        {
            Result<TResult> result = new Result<TResult>();
            result.Routine = func(t1, t2, t3, t4, t5, result);
            return result;
        }

        public delegate TResult Func<T1, T2, T3, T4, T5, TResult>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5);

        public delegate TResult Func<T1, T2, T3, T4, T5, T6, TResult>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6);

        #endregion


    }


    public class Result<T> : Result
    {
        private T data;


        public virtual int Total { get; set; }

        public virtual T Data
        {

            get
            {
                CheckDone();
                return data;
            }
            set
            {
                data = value;
                Success = true;
                IsDone = true;
            }
        }

        public LazyValue<T> ToLazyValue()
        {
            LazyValue<T> lazy;
            if (IsDone)
                lazy = new LazyValue<T>(() => Data);
            else
                lazy = new LazyValue<T>(() => Data, Routine);
            return lazy;
        }
    }



    public class ResultCancelable : Result
    {
        public bool Canceled { get; private set; }

        public void Cancel()
        {
            IsDone = true;
            Canceled = true;
        }
    }

    public class ResultCancelable<T> : Result<T>
    {
        public bool Canceled { get; private set; }

        public void Cancel()
        {
            IsDone = true;
            Canceled = true;
        }
    }



}
