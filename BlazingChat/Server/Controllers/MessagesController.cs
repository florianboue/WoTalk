﻿using WoTalk.Server.Data;
using WoTalk.Server.Data.Entities;
using WoTalk.Server.Hubs;
using WoTalk.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace WoTalk.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : BaseController
    {
        private readonly ChatContext _chatContext;
        private readonly IHubContext<WoTalkHub, IWoTalkHubClient> _hubContext;

        public MessagesController(ChatContext chatContext, IHubContext<WoTalkHub, IWoTalkHubClient> hubContext)
        {
            _chatContext = chatContext;
            _hubContext = hubContext;
        }

        // /api/messages
        [HttpPost("")]
        public async Task<IActionResult> SendMessage(MessageSendDto messageDto, CancellationToken cancellationToken)
        {
            if (messageDto.ToUserId <= 0 || string.IsNullOrWhiteSpace(messageDto.Message))
                return BadRequest();

            var message = new Message
            {
                FromId = base.UserId,
                ToId = messageDto.ToUserId,
                Content = messageDto.Message,
                SentOn = DateTime.Now
            };
            await _chatContext.Messages.AddAsync(message, cancellationToken);
            if(await _chatContext.SaveChangesAsync(cancellationToken) > 0)
            {
                var responseMessageDto = new MessageDto(message.ToId, message.FromId, message.Content, message.SentOn);
                await _hubContext.Clients.User(messageDto.ToUserId.ToString())
                            .MessageRecieved(responseMessageDto);
                return Ok();
            }
            else
            {
                return StatusCode(500, "Unable to send message");
            }
        }

        [HttpGet("{otherUserId:int}")]
        public async Task<IEnumerable<MessageDto>> GetMessages(int otherUserId, CancellationToken cancellationToken)
        {
            var messages = await _chatContext.Messages
                            .AsNoTracking()
                            .Where(m =>
                                (m.FromId == otherUserId && m.ToId == UserId)
                                || (m.ToId == otherUserId && m.FromId == UserId)
                            )
                            .Select(m=> new MessageDto(m.ToId, m.FromId, m.Content, m.SentOn))
                            .ToListAsync(cancellationToken);

            return messages;
        }
    }
}
