// _Scripts/BackEnd/AcceptAllCertificates.cs
using UnityEngine;
using UnityEngine.Networking;

public class AcceptAllCertificates : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log("[AcceptAllCertificates] ✅ Bypass certificate cho Editor + Development Build + ngrok");
        return true;
#else
        Debug.LogWarning("[SECURITY] Production build không được bypass certificate!");
        return false;   // Production sẽ dùng certificate thật
#endif
    }
}