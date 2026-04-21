// Scripts/BackEnd/AcceptAllCertificates.cs
using UnityEngine.Networking;

public class AcceptAllCertificates : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        // Luôn bypass khi là localhost (cả Editor lẫn Build)
        // Chỉ dùng cho dev local thôi, production thì sẽ đổi sang HTTPS thật
        return true;
    }
}