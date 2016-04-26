using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Code.External.Engine.Sqlite
{
    public interface IDone
    {
        bool IsDone { get; }
    }

    public interface IRoutine : IDone
    {
        IEnumerator Routine { get; }

    }

    public interface IRoutine<TProgress> : IRoutine
    {
        TProgress Progress { get; }
    }
}
