﻿using System;
using System.IO;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

// ReSharper disable ExplicitCallerInfoArgument - yes, we want to explicitly set it via overloads.

namespace Reusable.OmniLog.SemanticExtensions.v2
{
    using Reusable.OmniLog.v2;
    using v1 = Reusable.OmniLog.Abstractions;
    using v2 = Reusable.OmniLog.Abstractions.v2;

    [PublicAPI]
    public static class LoggerExtensions
    {
        // We use context as the name and not abstractionContext because it otherwise interferes with intellisense.
        // The name abstractionContext appears first on the list and you need to scroll to get the Abstraction.
        public static void Log
        (
            this v2.ILogger logger,
            IAbstractionContext context,
            Action<v1.ILog> transform = null,
            // These properties are for free so let's just log them too.
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string callerFilePath = null
        )
        {
            logger.Log(log =>
            {
                log.SetItem(nameof(SemanticExtensions.AbstractionContext), context);
                log.SetItem(LogPropertyNames.CallerMemberName, callerMemberName);
                log.SetItem(LogPropertyNames.CallerLineNumber, callerLineNumber);
                log.SetItem(LogPropertyNames.CallerFilePath, Path.GetFileName(callerFilePath));
                transform?.Invoke(log);
            });
        }

        public static void Log
        (
            this v2.ILogger logger,
            IAbstractionContext context,
            string message,
            Exception exception,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string callerFilePath = null)
        {
            logger.Log
            (
                context,
                log => log.SetItem(LogPropertyNames.Message, message).SetItem(LogPropertyNames.Exception, exception),
                callerMemberName,
                callerLineNumber,
                callerFilePath
            );
        }

        public static void Log
        (
            this v2.ILogger logger,
            IAbstractionContext context,
            string message,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string callerFilePath = null)
        {
            logger.Log
            (
                context,
                log => log.SetItem(LogPropertyNames.Message, message),
                callerMemberName,
                callerLineNumber,
                callerFilePath
            );
        }

        public static void Log
        (
            this v2.ILogger logger,
            IAbstractionContext context,
            Exception exception,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string callerFilePath = null)
        {
            logger.Log
            (
                context,
                log => log.SetItem(LogPropertyNames.Exception, exception),
                callerMemberName,
                callerLineNumber,
                callerFilePath
            );
        }
    }
}