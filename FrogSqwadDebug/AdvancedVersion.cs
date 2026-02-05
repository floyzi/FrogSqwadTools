using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace FrogSqwadDebug
{
    internal class AdvancedVersion
    {
        internal enum Style
        {
            None,
            Default,
            InGame
        }

        Style CurrentStyle { get; set; }
        VersionNumberHUDManager VersionText { get; }
        string InGameFormatString { get; }
        string DefaultFormatString { get; }
        TextMeshProUGUI Text => VersionText._versionNumber;
        int FPS { get; set; }
        bool HasInit { get; }
        
        internal AdvancedVersion(VersionNumberHUDManager instance)
        {
            VersionText = instance;

            var sb = new StringBuilder();

            sb.AppendLine("Frog Sqwad V{0}");
            sb.AppendLine();
            sb.AppendLine("FPS: {1}");
            sb.AppendLine("RTT: {2} | Region: {3}");

            InGameFormatString = sb.ToString();

            sb.Clear();

            sb.AppendLine("Frog Sqwad V{0}");
            sb.AppendLine();
            sb.AppendLine("FPS: {1}");

            DefaultFormatString = sb.ToString();

            HasInit = true;
            UpdateStyleDisplay(Style.Default);
        }

        internal void UpdateStyleDisplay(Style newStyle)
        {
            if (!HasInit) return;
            if (CurrentStyle == newStyle) return;

            CurrentStyle = newStyle;
            UpdateDisplay();
            Text.GetComponent<RectTransform>().sizeDelta = Text.GetPreferredValues();
        }

        internal void UpdateDisplay()
        {
            switch (CurrentStyle)
            {
                case Style.Default:
                    Text.SetText(string.Format(DefaultFormatString, Application.version, FPS));
                    break;
                case Style.InGame:
                    var conn = NetworkManager.Instance.Runner;
                    Text.SetText(string.Format(InGameFormatString, Application.version, FPS, 
                        conn.IsServer ? 
                        "HOST" : 
                            NetworkManager.Instance.LocalPlayer != null ? 
                            Mathf.RoundToInt((float)conn.GetPlayerRtt(conn.LocalPlayer) * 1000.0f) : 
                            "?", 
                        string.IsNullOrEmpty(conn.SessionInfo.Region) ? 
                        "?" : 
                        conn.SessionInfo.Region));
                    break;
            }
        }

        internal void SetFPS() => FPS = (int)(1f / Time.deltaTime);
    }
}
