using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static partial class GuildSystem
{
    public static void TerminateGuildAlly(string guildToSearch, string guildToRemove)
    {
        Player guildMember;
        // guild exists and member can terminate?
        if (guilds.TryGetValue(guildToSearch, out Guild guildTarget))
        {
            //Database.singleton.DeleteGuildAlly(guildName);
            // remove guild from database

            foreach (GuildMember member in guildTarget.members)
            {
                if (Player.onlinePlayers.TryGetValue(member.name, out guildMember))
                {
                    if (guildMember.playerAlliance.guildAlly.Contains(guildToRemove))
                    {
                        guildMember.playerAlliance.guildAlly.Remove(guildToRemove);
                    }
                }
            }
        }
    }
}
