using UnityEngine.Networking;

public class AcceptAllCertificates : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
#if UNITY_EDITOR
        return true;        // Chỉ bypass trong Editor
#else
        return false;       // Production thì kiểm tra bình thường
#endif
    }
}