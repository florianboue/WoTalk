using WoTalk.Shared.DTOs;

namespace WoTalk.Shared
{
    public interface IWoTalkHubServer
    {
        Task SetUserOnline(UserDto user);
    }
}
