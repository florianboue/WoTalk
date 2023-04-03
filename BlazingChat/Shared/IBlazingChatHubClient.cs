using WoTalk.Shared.DTOs;

namespace WoTalk.Shared
{
    public interface IWoTalkHubClient
    {
        Task UserConnected(UserDto user);
        Task OnlineUsersList(IEnumerable<UserDto> users);
        Task UserIsOnline(int userId);

        Task MessageRecieved(MessageDto messageDto);
    }
}
