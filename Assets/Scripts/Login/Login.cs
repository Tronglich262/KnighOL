using UnityEngine;

public class Login : MonoBehaviour
{
    public GameObject Dangky;
    public GameObject Dangnhap;
    public GameObject dangkydangnhap;
   
    public void DangNhapButton()
    {
        Dangnhap.SetActive(true);
        dangkydangnhap.SetActive(false);
    }
    public void DangkyButton()
    {
        Dangky.SetActive(true);
        dangkydangnhap.SetActive(false) ;
    }
    public void BackDangKy()
    {
        Dangky.SetActive(false );
        dangkydangnhap .SetActive(true);
    }
    public void BackDangNhap()
    {
        Dangnhap.SetActive(false);
        dangkydangnhap.SetActive(true);
    }
}
