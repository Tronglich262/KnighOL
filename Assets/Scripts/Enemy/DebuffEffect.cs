/// <summary>
/// Các hiệu ứng xấu (debuff) khi enemy bị trúng skill từ player.
/// </summary>
public enum DebuffEffect
{
    None = 0,
    Stun,       // Choáng: không di chuyển, không đánh
    Burn,       // Thiêu đốt: DoT damage theo thời gian
    Dizzy       // Choáng váng: giống stun, không hành động
}
