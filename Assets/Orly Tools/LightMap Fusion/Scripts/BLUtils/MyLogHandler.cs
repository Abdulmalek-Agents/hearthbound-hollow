
#if UNITY_EDITOR
using UnityEngine;

namespace LightMapFusion
{

    // Puedes crear un manejador de logs personalizado si solo quieres desactivar warnings
    class LogHandler : ILogHandler
    {
        private ILogHandler defaultLogHandler = Debug.unityLogger.logHandler;

        public void LogFormat(LogType logType, Object context, string format, params object[] args)
        {
            if (logType != LogType.Warning)
            {
                defaultLogHandler.LogFormat(logType, context, format, args);
            }
        }

        public void LogException(System.Exception exception, Object context)
        {
            defaultLogHandler.LogException(exception, context);
        }
    }
}
#endif