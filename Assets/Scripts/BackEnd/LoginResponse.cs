// Scripts/BackEnd/LoginResponse.cs
[System.Serializable]
public class LoginResponse
{
    public int accountId;           // ← phải là camelCase (giống server)
    public string name;
    public string role;
    public string accessToken;
    public string refreshToken;
    public string message;
}