using UnityEngine;
using UnityEngine.Networking;

public class AcceptAllCertificates : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        // Trong Editor: bypass hết (localhost + Ngrok + bất kỳ HTTPS nào)
#if UNITY_EDITOR
        Debug.Log("[AcceptAllCertificates] Bypass certificate trong Editor (Ngrok + localhost)");
        return true;
#endif
        // Build production: kiểm tra certificate thật
        return false;
    }
}