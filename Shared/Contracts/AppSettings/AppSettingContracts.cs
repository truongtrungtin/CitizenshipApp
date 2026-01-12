namespace Shared.Contracts.AppSettings;

public sealed record AppSettingDto(int Id, string Key, string Value);

public sealed record CreateAppSettingRequest(string Key, string Value);

public sealed record UpdateAppSettingRequest(string Key, string Value);
