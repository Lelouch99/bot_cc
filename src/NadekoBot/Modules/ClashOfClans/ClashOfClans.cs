﻿using Discord.Commands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using NadekoBot.Services;
using NadekoBot.Attributes;
using NadekoBot.Services.Database.Models;
using System.Linq;
using NadekoBot.Extensions;
using System.Threading;

namespace NadekoBot.Modules.ClashOfClans
{
    [NadekoModule("ClashOfClans", ",")]
    public class ClashOfClans : NadekoTopLevelModule
    {
        public static ConcurrentDictionary<ulong, List<ClashWar>> ClashWars { get; set; }

        private static Timer checkWarTimer { get; }

        static ClashOfClans()
        {
            using (var uow = DbHandler.UnitOfWork())
            {
                ClashWars = new ConcurrentDictionary<ulong, List<ClashWar>>(
                    uow.ClashOfClans
                        .GetAllWars()
                        .Select(cw =>
                        {
                            cw.Channel = NadekoBot.Client.GetGuild(cw.GuildId)?
                                                         .GetTextChannel(cw.ChannelId);
                            return cw;
                        })
                        .Where(cw => cw.Channel != null)
                        .GroupBy(cw => cw.GuildId)
                        .ToDictionary(g => g.Key, g => g.ToList()));
            }

            checkWarTimer = new Timer(async _ =>
            {
                foreach (var kvp in ClashWars)
                {
                    foreach (var war in kvp.Value)
                    {
                        try { await CheckWar(TimeSpan.FromHours(2), war).ConfigureAwait(false); } catch { }
                    }
                }
            }, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        }

        private static async Task CheckWar(TimeSpan callExpire, ClashWar war)
        {
            var Bases = war.Bases;
            for (var i = 0; i < Bases.Count; i++)
            {
                var callUser = Bases[i].CallUser;
                if (callUser == null) continue;
                if ((!Bases[i].BaseDestroyed) && DateTime.UtcNow - Bases[i].TimeAdded >= callExpire)
                {
                    if (Bases[i].Stars != 3)
                        Bases[i].BaseDestroyed = true;
                    else
                        Bases[i] = null;
                    try
                    {
                        SaveWar(war);
                        await war.Channel.SendErrorAsync(GetTextStatic("claim_expired", 
                                    NadekoBot.Localization.GetCultureInfo(war.Channel.GuildId), 
                                    typeof(ClashOfClans).Name.ToLowerInvariant(), 
                                    Format.Bold(Bases[i].CallUser), 
                                    war.ShortPrint()));
                    }
                    catch { }
                }
            }
        }

        [NadekoCommand, Usage, Description, Aliases]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task CreateWar(int size, [Remainder] string enemyClan = null)
        {
            if (string.IsNullOrWhiteSpace(enemyClan))
                return;

            if (size < 10 || size > 50 || size % 5 != 0)
            {
                await ReplyErrorLocalized("invalid_size").ConfigureAwait(false);
                return;
            }
            List<ClashWar> wars;
            if (!ClashWars.TryGetValue(Context.Guild.Id, out wars))
            {
                wars = new List<ClashWar>();
                if (!ClashWars.TryAdd(Context.Guild.Id, wars))
                    return;
            }


            var cw = await CreateWar(enemyClan, size, Context.Guild.Id, Context.Channel.Id);

            wars.Add(cw);
            await ReplyErrorLocalized("war_created", cw.ShortPrint()).ConfigureAwait(false);
        }

        [NadekoCommand, Usage, Description, Aliases]
        [RequireContext(ContextType.Guild)]
        public async Task StartWar([Remainder] string number = null)
        {
            int num = 0;
            int.TryParse(number, out num);

            var warsInfo = GetWarInfo(Context.Guild, num);
            if (warsInfo == null)
            {
                await ReplyErrorLocalized("war_not_exist").ConfigureAwait(false);
                return;
            }
            var war = warsInfo.Item1[warsInfo.Item2];
            try
            {
                war.Start();
                await ReplyConfirmLocalized("war_started", war.ShortPrint()).ConfigureAwait(false);
            }
            catch
            {
                await ReplyErrorLocalized("war_already_started", war.ShortPrint()).ConfigureAwait(false);
            }
            SaveWar(war);
        }

        [NadekoCommand, Usage, Description, Aliases]
        [RequireContext(ContextType.Guild)]
        public async Task ListWar([Remainder] string number = null)
        {

            // if number is null, print all wars in a short way
            if (string.IsNullOrWhiteSpace(number))
            {
                //check if there are any wars
                List<ClashWar> wars = null;
                ClashWars.TryGetValue(Context.Guild.Id, out wars);
                if (wars == null || wars.Count == 0)
                {
                    await ReplyErrorLocalized("no_active_wars").ConfigureAwait(false);
                    return;
                }

                var sb = new StringBuilder();
                sb.AppendLine("**-------------------------**");
                for (var i = 0; i < wars.Count; i++)
                {
                    sb.AppendLine($"**#{i + 1}.**  `{GetText("enemy")}:` **{wars[i].EnemyClan}**");
                    sb.AppendLine($"\t\t`{GetText("size")}:` **{wars[i].Size} v {wars[i].Size}**");
                    sb.AppendLine("**-------------------------**");
                }
                await Context.Channel.SendConfirmAsync(GetText("list_active_wars"), sb.ToString()).ConfigureAwait(false);
                return;
            }
            var num = 0;
            int.TryParse(number, out num);
            //if number is not null, print the war needed
            var warsInfo = GetWarInfo(Context.Guild, num);
            if (warsInfo == null)
            {
                await ReplyErrorLocalized("war_not_exist").ConfigureAwait(false);
                return;
            }
            var war = warsInfo.Item1[warsInfo.Item2];
            await Context.Channel.SendConfirmAsync(war.Localize("info_about_war", $"`{war.EnemyClan}` ({war.Size} v {war.Size})"), war.ToPrettyString()).ConfigureAwait(false);
        }

        [NadekoCommand, Usage, Description, Aliases]
        [RequireContext(ContextType.Guild)]
        public async Task Claim(int number, int baseNumber, [Remainder] string other_name = null)
        {
            var warsInfo = GetWarInfo(Context.Guild, number);
            if (warsInfo == null || warsInfo.Item1.Count == 0)
            {
                await ReplyErrorLocalized("war_not_exist").ConfigureAwait(false);
                return;
            }
            var usr =
                string.IsNullOrWhiteSpace(other_name) ?
                Context.User.Username :
                other_name;
            try
            {
                var war = warsInfo.Item1[warsInfo.Item2];
                war.Call(usr, baseNumber - 1);
                SaveWar(war);
                await ConfirmLocalized("claimed_base", Format.Bold(usr.ToString()), baseNumber, war.ShortPrint()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await Context.Channel.SendErrorAsync(ex.Message).ConfigureAwait(false);
            }
        }

        [NadekoCommand, Usage, Description, Aliases]
        [RequireContext(ContextType.Guild)]
        public async Task ClaimFinish1(int number, int baseNumber = 0)
        {
            await FinishClaim(number, baseNumber - 1, 1);
        }

        [NadekoCommand, Usage, Description, Aliases]
        [RequireContext(ContextType.Guild)]
        public async Task ClaimFinish2(int number, int baseNumber = 0)
        {
            await FinishClaim(number, baseNumber - 1, 2);
        }

        [NadekoCommand, Usage, Description, Aliases]
        [RequireContext(ContextType.Guild)]
        public async Task ClaimFinish(int number, int baseNumber = 0)
        {
            await FinishClaim(number, baseNumber - 1);
        }

        [NadekoCommand, Usage, Description, Aliases]
        [RequireContext(ContextType.Guild)]
        public async Task EndWar(int number)
        {
            var warsInfo = GetWarInfo(Context.Guild, number);
            if (warsInfo == null)
            {
                await ReplyErrorLocalized("war_not_exist").ConfigureAwait(false);
                return;
            }
            var war = warsInfo.Item1[warsInfo.Item2];
            war.End();
            SaveWar(war);
            await ReplyConfirmLocalized("war_ended", warsInfo.Item1[warsInfo.Item2].ShortPrint()).ConfigureAwait(false);

            warsInfo.Item1.RemoveAt(warsInfo.Item2);
        }

        [NadekoCommand, Usage, Description, Aliases]
        [RequireContext(ContextType.Guild)]
        public async Task Unclaim(int number, [Remainder] string otherName = null)
        {
            var warsInfo = GetWarInfo(Context.Guild, number);
            if (warsInfo == null || warsInfo.Item1.Count == 0)
            {
                await ReplyErrorLocalized("war_not_exist").ConfigureAwait(false);
                return;
            }
            var usr =
                string.IsNullOrWhiteSpace(otherName) ?
                Context.User.Username :
                otherName;
            try
            {
                var war = warsInfo.Item1[warsInfo.Item2];
                var baseNumber = war.Uncall(usr);
                SaveWar(war);
                await ReplyConfirmLocalized("base_unclaimed", usr, baseNumber + 1, war.ShortPrint()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await Context.Channel.SendErrorAsync(ex.Message).ConfigureAwait(false);
            }
        }

        private async Task FinishClaim(int number, int baseNumber, int stars = 3)
        {
            var warInfo = GetWarInfo(Context.Guild, number);
            if (warInfo == null || warInfo.Item1.Count == 0)
            {
                await ReplyErrorLocalized("war_not_exist").ConfigureAwait(false);
                return;
            }
            var war = warInfo.Item1[warInfo.Item2];
            try
            {
                if (baseNumber == -1)
                {
                    baseNumber = war.FinishClaim(Context.User.Username, stars);
                    SaveWar(war);
                }
                else
                {
                    war.FinishClaim(baseNumber, stars);
                }
                await ReplyConfirmLocalized("base_destroyed", baseNumber +1, war.ShortPrint()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await Context.Channel.SendErrorAsync($"🔰 {ex.Message}").ConfigureAwait(false);
            }
        }

        private static Tuple<List<ClashWar>, int> GetWarInfo(IGuild guild, int num)
        {
            List<ClashWar> wars = null;
            ClashWars.TryGetValue(guild.Id, out wars);
            if (wars == null || wars.Count == 0)
            {
                return null;
            }
            // get the number of the war
            else if (num < 1 || num > wars.Count)
            {
                return null;
            }
            num -= 1;
            //get the actual war
            return new Tuple<List<ClashWar>, int>(wars, num);
        }

        public static async Task<ClashWar> CreateWar(string enemyClan, int size, ulong serverId, ulong channelId)
        {
            var channel = NadekoBot.Client.GetGuild(serverId)?.GetTextChannel(channelId);
            using (var uow = DbHandler.UnitOfWork())
            {
                var cw = new ClashWar
                {
                    EnemyClan = enemyClan,
                    Size = size,
                    Bases = new List<ClashCaller>(size),
                    GuildId = serverId,
                    ChannelId = channelId,
                    Channel = channel,
                };
                cw.Bases.Capacity = size;
                for (int i = 0; i < size; i++)
                {
                    cw.Bases.Add(new ClashCaller()
                    {
                        CallUser = null,
                        SequenceNumber = i,
                    });
                }
                Console.WriteLine(cw.Bases.Capacity);
                uow.ClashOfClans.Add(cw);
                await uow.CompleteAsync();
                return cw;
            }
        }

        public static void SaveWar(ClashWar cw)
        {
            if (cw.WarState == ClashWar.StateOfWar.Ended)
            {
                using (var uow = DbHandler.UnitOfWork())
                {
                    uow.ClashOfClans.Remove(cw);
                    uow.CompleteAsync();
                }
                return;
            }


            using (var uow = DbHandler.UnitOfWork())
            {
                uow.ClashOfClans.Update(cw);
                uow.CompleteAsync();
            }
        }
    }
}