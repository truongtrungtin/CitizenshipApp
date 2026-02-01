using Domain.Enums;

namespace Ui.Blazor.Services;

/// <summary>
/// Simple UI text provider for bilingual labels.
/// Reads current language from <see cref="UserSettingsState" />.
/// </summary>
public sealed class UiText
{
    private readonly UserSettingsState _settings;

    public UiText(UserSettingsState settings)
    {
        _settings = settings;
    }

    public string this[string key] => Get(key);

    public string Get(string key)
    {
        if (!Map.TryGetValue(key, out var entry))
        {
            return key; // fallback: show key to surface missing translation
        }

        return CurrentLanguage == LanguageCode.En ? entry.En : entry.Vi;
    }

    private LanguageCode CurrentLanguage
        => _settings.Current?.SystemLanguage ?? LanguageCode.Vi;

    private static readonly Dictionary<string, (string Vi, string En)> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        // Nav
        ["Nav.Home"] = ("Trang chủ", "Home"),
        ["Nav.Study"] = ("Học", "Study"),
        ["Nav.Settings"] = ("Cài đặt", "Settings"),
        ["Nav.Logout"] = ("Đăng xuất", "Logout"),
        ["Nav.Login"] = ("Đăng nhập", "Login"),
        ["Nav.Register"] = ("Tạo tài khoản", "Register"),

        // Home
        ["Home.Title"] = ("CitizenshipApp", "CitizenshipApp"),
        ["Home.Tagline"] = ("Ứng dụng ôn thi quốc tịch Mỹ đơn giản, thân thiện với người lớn tuổi.", "A simple, senior-friendly study companion for the U.S. citizenship test."),
        ["Home.Card.Elderly.Title"] = ("Thiết kế ưu tiên người lớn tuổi", "Elderly-first design"),
        ["Home.Card.Elderly.Desc"] = ("Chữ lớn, nút lớn, phản hồi rõ ràng.", "Large text, big buttons, clear feedback."),
        ["Home.Card.Bilingual.Title"] = ("Luyện song ngữ", "Bilingual practice"),
        ["Home.Card.Bilingual.Desc"] = ("Câu hỏi tiếng Anh và tiếng Việt.", "English and Vietnamese prompts."),
        ["Home.Card.Progress.Title"] = ("Theo dõi tiến độ mỗi ngày", "Daily progress"),
        ["Home.Card.Progress.Desc"] = ("Thống kê số câu đã làm và đúng.", "Track answered and correct counts."),
        ["Home.CTA.Start"] = ("Bắt đầu học", "Start studying"),
        ["Home.CTA.Settings"] = ("Cài đặt", "Settings"),
        ["Home.CTA.Login"] = ("Đăng nhập", "Login"),
        ["Home.CTA.Register"] = ("Tạo tài khoản", "Create account"),

        // Login/Register
        ["Auth.Login.Title"] = ("Đăng nhập", "Login"),
        ["Auth.Register.Title"] = ("Đăng ký", "Register"),
        ["Auth.Username.Label"] = ("Tên đăng nhập (email / số điện thoại)", "Username (email / phone)"),
        ["Auth.Password.Label"] = ("Mật khẩu", "Password"),
        ["Auth.Login.Button"] = ("Đăng nhập", "Login"),
        ["Auth.Register.Button"] = ("Tạo tài khoản", "Create account"),
        ["Auth.GoRegister"] = ("Đăng ký", "Register"),
        ["Auth.GoLogin"] = ("Quay lại đăng nhập", "Back to login"),

        // Onboarding
        ["Onboarding.Title"] = ("Thiết lập ban đầu", "Onboarding"),
        ["Onboarding.Language"] = ("Ngôn ngữ", "Language"),
        ["Onboarding.Font"] = ("Cỡ chữ", "Font size"),
        ["Onboarding.Audio"] = ("Tốc độ audio", "Audio speed"),
        ["Onboarding.DailyGoal"] = ("Mục tiêu mỗi ngày", "Daily goal"),
        ["Onboarding.Focus"] = ("Nội dung ưu tiên", "Focus"),
        ["Onboarding.Silent"] = ("Chế độ im lặng", "Silent mode"),
        ["Onboarding.Start"] = ("Bắt đầu học", "Start studying"),
        ["Onboarding.Loading"] = ("Đang tải…", "Loading…"),

        // Settings
        ["Settings.Title"] = ("Cài đặt", "Settings"),
        ["Settings.LoginRequired"] = ("Vui lòng đăng nhập để truy cập cài đặt.", "Please login to access settings."),
        ["Settings.GoLogin"] = ("Đi đến đăng nhập", "Go to login"),
        ["Settings.Loading"] = ("Đang tải cài đặt...", "Loading settings..."),
        ["Settings.Language"] = ("Ngôn ngữ câu hỏi", "Question language"),
        ["Settings.SystemLanguage"] = ("Ngôn ngữ hệ thống", "System language"),
        ["Lang.Vi"] = ("Tiếng Việt", "Vietnamese"),
        ["Lang.En"] = ("Tiếng Anh", "English"),
        ["Settings.Font"] = ("Cỡ chữ", "Font size"),
        ["Font.Small"] = ("Nhỏ", "Small"),
        ["Font.Medium"] = ("Trung bình", "Medium"),
        ["Font.Large"] = ("Lớn", "Large"),
        ["Settings.Audio"] = ("Tốc độ audio", "Audio speed"),
        ["Audio.Slow"] = ("Chậm", "Slow"),
        ["Audio.Medium"] = ("Trung bình", "Medium"),
        ["Audio.Fast"] = ("Nhanh", "Fast"),
        ["Settings.DailyGoal"] = ("Mục tiêu học mỗi ngày (phút)", "Daily study goal (minutes)"),
        ["Settings.Focus"] = ("Nội dung ưu tiên", "Study focus"),
        ["Focus.Civics"] = ("Công dân", "Civics"),
        ["Focus.Reading"] = ("Đọc", "Reading"),
        ["Focus.Writing"] = ("Viết", "Writing"),
        ["Settings.Silent"] = ("Chế độ im lặng", "Silent mode"),
        ["Settings.Save"] = ("Lưu", "Save"),
        ["Settings.Saved"] = ("Đã lưu.", "Saved."),

        // Study
        ["Study.Title"] = ("Học", "Study"),
        ["Study.Language"] = ("Ngôn ngữ", "Language"),
        ["Study.Deck"] = ("Bộ câu hỏi", "Deck"),
        ["Study.Suggested"] = ("Gợi ý theo nội dung", "Suggested by Focus"),
        ["Study.SuggestedDeck"] = ("Bộ gợi ý", "Suggested deck"),
        ["Study.None"] = ("(không có)", "(none)"),
        ["Study.NoDecks"] = ("— Không có bộ nào —", "— No decks —"),
        ["Study.Reload"] = ("Tải lại", "Reload"),
        ["Study.Next"] = ("Tiếp", "Next"),
        ["Study.Today"] = ("Hôm nay", "Today"),
        ["Study.Goal"] = ("Mục tiêu", "Goal"),
        ["Study.Answered"] = ("Đã làm", "Answered"),
        ["Study.Correct"] = ("Đúng", "Correct"),
        ["Study.UtcNote"] = ("(Theo mốc UTC)", "(UTC day boundary)"),
        ["Study.Loading"] = ("Đang tải…", "Loading…"),
        ["Study.QuestionType"] = ("Loại câu hỏi", "Question type"),
        ["Study.QuestionType.SingleChoice"] = ("Trắc nghiệm 1 đáp án", "Single choice"),
        ["Study.QuestionType.MultiChoice"] = ("Trắc nghiệm nhiều đáp án", "Multiple choice"),
        ["Study.QuestionType.Text"] = ("Tự luận", "Free text"),
        ["Study.QuestionType.Unknown"] = ("Không xác định", "Unknown"),
        ["Study.Listen"] = ("Nghe", "Listen"),
        ["Study.Stop"] = ("Dừng", "Stop"),
        ["Study.SilentModeHint"] = ("Silent Mode đang bật", "Silent Mode is on"),
        ["Study.Explanation"] = ("Giải thích", "Explanation"),
        ["Study.CorrectBadge"] = ("Đúng", "Correct"),
        ["Study.IncorrectBadge"] = ("Sai", "Incorrect"),
        ["Study.YourAnswer"] = ("Bạn chọn", "Your"),
        ["Study.CorrectAnswer"] = ("Đáp án", "Correct"),
        ["Study.Continue"] = ("Tiếp tục", "Continue"),
        ["Study.ClearResult"] = ("Xoá kết quả", "Clear result"),
        ["Study.NoOptions"] = ("Câu hỏi này chưa có lựa chọn (một phần tự do chưa hỗ trợ ở MVP).", "This question has no options yet (free-text not implemented in MVP)."),
        ["Study.Error"] = ("Lỗi", "Error"),

        // Logout
        ["Logout.SigningOut"] = ("Đang đăng xuất...", "Signing out..."),

        // NotFound
        ["Common.NotFound"] = ("Không tìm thấy trang.", "Page not found.")
    };
}
