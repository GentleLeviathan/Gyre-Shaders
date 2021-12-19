using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace GentleShaders.Gyre
{
    public static class GyreUpdateChecker
    {
        public static string currentVersion = "AR1";

        public static async Task<bool> CheckForUpdates()
        {
            UnityWebRequest www = UnityWebRequest.Get("https://raw.githubusercontent.com/GentleLeviathan/Gyre-Shaders/main/masterVersion");
            DownloadHandler handler = www.downloadHandler;
            UnityWebRequestAsyncOperation op = www.SendWebRequest();

            while (www.downloadProgress < 1.0f)
            {
                await Task.Delay(100);
            }
            if (www.isHttpError)
            {
                Debug.Log("Gyre Updater - There was an error checking for an update. - " + www.error);
                return false;
            }

            return !handler.text.Contains(currentVersion);
        }

        public static void OpenRepository()
        {
            Application.OpenURL("https://github.com/GentleLeviathan/Gyre-Shaders/releases");
        }
    }
}
