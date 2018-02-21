using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using NLog;

namespace VORBS.Utils
{
    public static class LoggerHelper
    {
        public static Type VOID_TYPE = typeof(void);

        public static NLog.LogMessageGenerator ExecutedFunctionMessage()
        {
            MethodBase method = new StackTrace().GetFrame(1).GetMethod();
            return () => $"Executed {method}";
        }


        public static NLog.LogMessageGenerator InitializeClassMessage(params string[] additionalMessages)
        {
            MethodBase method = new StackTrace().GetFrame(1).GetMethod();
            return delegate ()
            {
                try
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append($"Initialized {method.GetType().FullName} ");

                    foreach (string message in additionalMessages)
                    {
                        stringBuilder.Append($"| {message} ");
                    }

                    return stringBuilder.ToString();
                }
                catch (Exception ex)
                {
                    return $"Class: LoggerHelper, Method: {MethodBase.GetCurrentMethod().Name} An error occured generating log: Message:{ex.Message}, InnerMessage: {ex.InnerException?.Message}";
                }
            };
        }

        public static NLog.LogMessageGenerator ExecutedFunctionMessage(params object[] parameters)
        {
            MethodBase method = new StackTrace().GetFrame(1).GetMethod();
            return delegate ()
            {
                try
                {
                    StringBuilder stringBuilder = new StringBuilder();

                    if (parameters.Length > 0)
                        stringBuilder.Append($"with params: {GetInfoOnParams(parameters)}");

                    return stringBuilder.ToString();
                }
                catch (Exception ex)
                {
                    return $"Class: LoggerHelper, Method: {MethodBase.GetCurrentMethod().Name} An error occured generating log: Message:{ex.Message}, InnerMessage: {ex.InnerException?.Message}";
                }
            };
        }

        public static NLog.LogMessageGenerator ExecutedFunctionMessage(object result, params object[] parameters)
        {
            MethodBase method = new StackTrace().GetFrame(1).GetMethod();
            return delegate ()
            {
                try
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append($"Executed {method} ");
                    if (result != null)
                        stringBuilder.Append($"with result: {GetInfoOnObject(result)} ");
                    if (parameters.Length > 0)
                        stringBuilder.Append($"with params: {GetInfoOnParams(parameters)}");
                    return stringBuilder.ToString();
                }
                catch (Exception ex)
                {
                    return $"Class: LoggerHelper, Method: {MethodBase.GetCurrentMethod().Name} An error occured generating log: Message:{ex.Message}, InnerMessage: {ex.InnerException?.Message}";
                }
            };
        }

        private static string GetInfoOnParams(object[] parameters)
        {
            StringBuilder builder = new StringBuilder();

            if (parameters.Length > 0)
            {
                builder.Append("{{ ");

                int indexer = 0;
                foreach (object param in parameters)
                {
                    builder.Append($"{indexer}: ");
                    builder.Append(GetInfoOnObject(param));

                    if (indexer < parameters.Length - 1)
                        builder.Append($",");

                    indexer++;
                }

                builder.Append("}} ");
            }

            return builder.ToString();
        }

        private static string GetInfoOnObject(object o)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append($"{{ ");

            if (o != null)
            {
                Type objectType = o.GetType();
                builder.Append($"Type: {objectType.Name}, ");
            }
            else
            {
                builder.Append($"Type: null, ");
            }

            builder.Append($"IsNull: {o == null} ");
            if (o != null)
                builder.Append($", Value: {o} ");

            if (o != null)
            {
                Type objectType = o.GetType();
                if (typeof(IEnumerable).IsAssignableFrom(objectType))
                {
                    if (objectType != typeof(String))
                    {
                        builder.Append($",");
                        builder.Append($"IsEnumerable: True, ");
                        builder.Append($"Count: {((IEnumerable<object>)o).Count()} ");
                    }
                }
            }

            builder.Append($"}}");

            return builder.ToString();
        }
    }
}