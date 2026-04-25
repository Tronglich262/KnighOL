using UnityEngine;
using UnityEngine.Networking;

public class AcceptAllCertificates : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        // Bypass hoàn toàn cho localhost + ngrok (cả Editor lẫn Build)
        Debug.Log("[AcceptAllCertificates] Bypass certificate cho ngrok/localhost");
        return true;
    }
}