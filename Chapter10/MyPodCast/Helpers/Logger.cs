using System;

namespace MyPodCast
{
    public class Logger
    {
        private const string Tag = "HenryPodcast";
        public static void Verbose(string message)
        {
            Android.Util.Log.Verbose(Tag, message);
        }
        public static void Debug(string message)
        {
            Android.Util.Log.Debug(Tag, message);
        }
        public static void Warn(string message)
        {
            Android.Util.Log.Warn(Tag, message);
        }
        public static void Info(string message)
        {
            Android.Util.Log.Info(Tag, message);
        }
        public static void Error(string message)
        {
            Android.Util.Log.Error(Tag, message);
        }        
        public static void Error(Exception e, string message)
        {
            Android.Util.Log.Error(Tag, message + e.StackTrace);
        }
    }
}