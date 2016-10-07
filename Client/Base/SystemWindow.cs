﻿using LunaClient.Base.Interface;

namespace LunaClient.Base
{
    /// <summary>
    /// Base class for systems that also implement a window (chat for example)
    /// </summary>
    public abstract class SystemWindow<T, TS> : Window<T>
        where T : class, IWindow, new()
        where TS : class, ISystem
    {
        public abstract TS System { get; }
    }
}