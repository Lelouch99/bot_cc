﻿using System;
using System.Threading.Tasks;

namespace Discord.WebSocket
{
    //TODO: Add event docstrings
    public partial class DiscordShardedClient
    {
        //Channels
        public event Func<SocketChannel, Task> ChannelCreated
        {
            add { _channelCreatedEvent.Add(value); }
            remove { _channelCreatedEvent.Remove(value); }
        }
        private readonly AsyncEvent<Func<SocketChannel, Task>> _channelCreatedEvent = new AsyncEvent<Func<SocketChannel, Task>>();
        public event Func<SocketChannel, Task> ChannelDestroyed
        {
            add { _channelDestroyedEvent.Add(value); }
            remove { _channelDestroyedEvent.Remove(value); }
        }
        private readonly AsyncEvent<Func<SocketChannel, Task>> _channelDestroyedEvent = new AsyncEvent<Func<SocketChannel, Task>>();
        public event Func<SocketChannel, SocketChannel, Task> ChannelUpdated
        {
            add { _channelUpdatedEvent.Add(value); }
            remove { _channelUpdatedEvent.Remove(value); }
        }
        private readonly AsyncEvent<Func<SocketChannel, SocketChannel, Task>> _channelUpdatedEvent = new AsyncEvent<Func<SocketChannel, SocketChannel, Task>>();

        //Messages
        public event Func<SocketMessage, Task> MessageReceived
        {
            add { _messageReceivedEvent.Add(value); }
            remove { _messageReceivedEvent.Remove(value); }
        }
        private readonly AsyncEvent<Func<SocketMessage, Task>> _messageReceivedEvent = new AsyncEvent<Func<SocketMessage, Task>>();
        public event Func<ulong, Optional<SocketMessage>, Task> MessageDeleted
        {
            add { _messageDeletedEvent.Add(value); }
            remove { _messageDeletedEvent.Remove(value); }
        }
        private readonly AsyncEvent<Func<ulong, Optional<SocketMessage>, Task>> _messageDeletedEvent = new AsyncEvent<Func<ulong, Optional<SocketMessage>, Task>>();
        public event Func<Optional<SocketMessage>, SocketMessage, Task> MessageUpdated
        {
            add { _messageUpdatedEvent.Add(value); }
            remove { _messageUpdatedEvent.Remove(value); }
        }
        private readonly AsyncEvent<Func<Optional<SocketMessage>, SocketMessage, Task>> _messageUpdatedEvent = new AsyncEvent<Func<Optional<SocketMessage>, SocketMessage, Task>>();
        public event Func<ulong, Optional<SocketUserMessage>, SocketReaction, Task> ReactionAdded
        {
            add { _reactionAddedEvent.Add(value); }
            remove { _reactionAddedEvent.Remove(value); }
        }
        private readonly AsyncEvent<Func<ulong, Optional<SocketUserMessage>, SocketReaction, Task>> _reactionAddedEvent = new AsyncEvent<Func<ulong, Optional<SocketUserMessage>, SocketReaction, Task>>();
        public event Func<ulong, Optional<SocketUserMessage>, SocketReaction, Task> ReactionRemoved
        {
            add { _reactionRemovedEvent.Add(value); }
            remove { _reactionRemovedEvent.Remove(value); }
        }
        private readonly AsyncEvent<Func<ulong, Optional<SocketUserMessage>, SocketReaction, Task>> _reactionRemovedEvent = new AsyncEvent<Func<ulong, Optional<SocketUserMessage>, SocketReaction, Task>>();
        public event Func<ulong, Optional<SocketUserMessage>, Task> ReactionsCleared
        {
            add { _reactionsClearedEvent.Add(value); }
            remove { _reactionsClearedEvent.Remove(value); }
        }
        private readonly AsyncEvent<Func<ulong, Optional<SocketUserMessage>, Task>> _reactionsClearedEvent = new AsyncEvent<Func<ulong, Optional<SocketUserMessage>, Task>>();

        //Roles
        public event Func<SocketRole, Task> RoleCreated
        {
            add { _roleCreatedEvent.Add(value); }
            remove { _roleCreatedEvent.Remove(value); }
        }
        private readonly AsyncEvent<Func<SocketRole, Task>> _roleCreatedEvent = new AsyncEvent<Func<SocketRole, Task>>();
        public event Func<SocketRole, Task> RoleDeleted
        {
            add { _roleDeletedEvent.Add(value); }
            remove { _roleDeletedEvent.Remove(value); }
        }
        private readonly AsyncEvent<Func<SocketRole, Task>> _roleDeletedEvent = new AsyncEvent<Func<SocketRole, Task>>();
        public event Func<SocketRole, SocketRole, Task> RoleUpdated
        {
            add { _roleUpdatedEvent.Add(value); }
            remove { _roleUpdatedEvent.Remove(value); }
        }
        private readonly AsyncEvent<Func<SocketRole, SocketRole, Task>> _roleUpdatedEvent = new AsyncEvent<Func<SocketRole, SocketRole, Task>>();

        //Guilds
        public event Func<SocketGuild, Task> JoinedGuild
        {
            add { _joinedGuildEvent.Add(value); }
            remove { _joinedGuildEvent.Remove(value); }
        }
        private AsyncEvent<Func<SocketGuild, Task>> _joinedGuildEvent = new AsyncEvent<Func<SocketGuild, Task>>();
        public event Func<SocketGuild, Task> LeftGuild
        {
            add { _leftGuildEvent.Add(value); }
            remove { _leftGuildEvent.Remove(value); }
        }
        private AsyncEvent<Func<SocketGuild, Task>> _leftGuildEvent = new AsyncEvent<Func<SocketGuild, Task>>();
        public event Func<SocketGuild, Task> GuildAvailable
        {
            add { _guildAvailableEvent.Add(value); }
            remove { _guildAvailableEvent.Remove(value); }
        }
        private AsyncEvent<Func<SocketGuild, Task>> _guildAvailableEvent = new AsyncEvent<Func<SocketGuild, Task>>();
        public event Func<SocketGuild, Task> GuildUnavailable
        {
            add { _guildUnavailableEvent.Add(value); }
            remove { _guildUnavailableEvent.Remove(value); }
        }
        private AsyncEvent<Func<SocketGuild, Task>> _guildUnavailableEvent = new AsyncEvent<Func<SocketGuild, Task>>();
        public event Func<SocketGuild, Task> GuildMembersDownloaded
        {
            add { _guildMembersDownloadedEvent.Add(value); }
            remove { _guildMembersDownloadedEvent.Remove(value); }
        }
        private AsyncEvent<Func<SocketGuild, Task>> _guildMembersDownloadedEvent = new AsyncEvent<Func<SocketGuild, Task>>();
        public event Func<SocketGuild, SocketGuild, Task> GuildUpdated
        {
            add { _guildUpdatedEvent.Add(value); }
            remove { _guildUpdatedEvent.Remove(value); }
        }
        private AsyncEvent<Func<SocketGuild, SocketGuild, Task>> _guildUpdatedEvent = new AsyncEvent<Func<SocketGuild, SocketGuild, Task>>();

        //Users
        public event Func<SocketGuildUser, Task> UserJoined
        {
            add { _userJoinedEvent.Add(value); }
            remove { _userJoinedEvent.Remove(value); }
        }
        private readonly AsyncEvent<Func<SocketGuildUser, Task>> _userJoinedEvent = new AsyncEvent<Func<SocketGuildUser, Task>>();
        public event Func<SocketGuildUser, Task> UserLeft
        {
            add { _userLeftEvent.Add(value); }
            remove { _userLeftEvent.Remove(value); }
        }
        private readonly AsyncEvent<Func<SocketGuildUser, Task>> _userLeftEvent = new AsyncEvent<Func<SocketGuildUser, Task>>();
        public event Func<SocketUser, SocketGuild, Task> UserBanned
        {
            add { _userBannedEvent.Add(value); }
            remove { _userBannedEvent.Remove(value); }
        }
        private readonly AsyncEvent<Func<SocketUser, SocketGuild, Task>> _userBannedEvent = new AsyncEvent<Func<SocketUser, SocketGuild, Task>>();
        public event Func<SocketUser, SocketGuild, Task> UserUnbanned
        {
            add { _userUnbannedEvent.Add(value); }
            remove { _userUnbannedEvent.Remove(value); }
        }
        private readonly AsyncEvent<Func<SocketUser, SocketGuild, Task>> _userUnbannedEvent = new AsyncEvent<Func<SocketUser, SocketGuild, Task>>();
        public event Func<SocketUser, SocketUser, Task> UserUpdated
        {
            add { _userUpdatedEvent.Add(value); }
            remove { _userUpdatedEvent.Remove(value); }
        }
        private readonly AsyncEvent<Func<SocketUser, SocketUser, Task>> _userUpdatedEvent = new AsyncEvent<Func<SocketUser, SocketUser, Task>>();
        public event Func<SocketGuildUser, SocketGuildUser, Task> GuildMemberUpdated
        {
            add { _guildMemberUpdatedEvent.Add(value); }
            remove { _guildMemberUpdatedEvent.Remove(value); }
        }
        private readonly AsyncEvent<Func<SocketGuildUser, SocketGuildUser, Task>> _guildMemberUpdatedEvent = new AsyncEvent<Func<SocketGuildUser, SocketGuildUser, Task>>();
        public event Func<Optional<SocketGuild>, SocketUser, SocketPresence, SocketPresence, Task> UserPresenceUpdated
        {
            add { _userPresenceUpdatedEvent.Add(value); }
            remove { _userPresenceUpdatedEvent.Remove(value); }
        }
        private readonly AsyncEvent<Func<Optional<SocketGuild>, SocketUser, SocketPresence, SocketPresence, Task>> _userPresenceUpdatedEvent = new AsyncEvent<Func<Optional<SocketGuild>, SocketUser, SocketPresence, SocketPresence, Task>>();
        public event Func<SocketUser, SocketVoiceState, SocketVoiceState, Task> UserVoiceStateUpdated
        {
            add { _userVoiceStateUpdatedEvent.Add(value); }
            remove { _userVoiceStateUpdatedEvent.Remove(value); }
        }
        private readonly AsyncEvent<Func<SocketUser, SocketVoiceState, SocketVoiceState, Task>> _userVoiceStateUpdatedEvent = new AsyncEvent<Func<SocketUser, SocketVoiceState, SocketVoiceState, Task>>();
        public event Func<SocketSelfUser, SocketSelfUser, Task> CurrentUserUpdated
        {
            add { _selfUpdatedEvent.Add(value); }
            remove { _selfUpdatedEvent.Remove(value); }
        }
        private readonly AsyncEvent<Func<SocketSelfUser, SocketSelfUser, Task>> _selfUpdatedEvent = new AsyncEvent<Func<SocketSelfUser, SocketSelfUser, Task>>();
        public event Func<SocketUser, ISocketMessageChannel, Task> UserIsTyping
        {
            add { _userIsTypingEvent.Add(value); }
            remove { _userIsTypingEvent.Remove(value); }
        }
        private readonly AsyncEvent<Func<SocketUser, ISocketMessageChannel, Task>> _userIsTypingEvent = new AsyncEvent<Func<SocketUser, ISocketMessageChannel, Task>>();
        public event Func<SocketGroupUser, Task> RecipientAdded
        {
            add { _recipientAddedEvent.Add(value); }
            remove { _recipientAddedEvent.Remove(value); }
        }
        private readonly AsyncEvent<Func<SocketGroupUser, Task>> _recipientAddedEvent = new AsyncEvent<Func<SocketGroupUser, Task>>();
        public event Func<SocketGroupUser, Task> RecipientRemoved
        {
            add { _recipientRemovedEvent.Add(value); }
            remove { _recipientRemovedEvent.Remove(value); }
        }
        private readonly AsyncEvent<Func<SocketGroupUser, Task>> _recipientRemovedEvent = new AsyncEvent<Func<SocketGroupUser, Task>>();
    }
}
