﻿using JetBrains.Annotations;

namespace SpaceWarp.API.Logging;

[PublicAPI]
public enum LogLevel
{
    None = 0,
    Fatal = 1,
    Error = 2,
    Warning = 4,
    Message = 8,
    Info = 16,
    Debug = 32,
    All = Debug | Info | Message | Warning | Error | Fatal
}