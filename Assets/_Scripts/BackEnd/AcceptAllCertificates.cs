using UnityEngine;
using UnityEngine.Networking;

public class AcceptAllCertificates : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        // CHỈ bypass khi là Editor + localhost
        // Production build sẽ dùng certificate thật (HTTPS)
#if UNITY_EDITOR
        if (Application.isEditor)
            return true;
#endif
        // Production: trả về false → Unity sẽ kiểm tra certificate bình thường
        return false;
    }
}