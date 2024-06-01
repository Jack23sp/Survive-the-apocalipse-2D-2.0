// We implemented a chat system that works directly with UNET. The chat supports
// different channels that can be used to communicate with other players:
//
// - **Local Chat:** by default, all messages that don't start with a **/** are
// addressed to the local chat. If one player writes a local message, then all
// players around him _(all observers)_ will be able to see the message.
// - **Whisper Chat:** a player can write a private message to another player by
// using the **/ name message** format.
// - **Guild Chat:** we implemented guild chat support with the **/g message**
// - **Info Chat:** the info chat can be used by the server to notify all
// players about important news. The clients won't be able to write any info
// messages.
//
// _Note: the channel names, colors and commands can be edited in the Inspector_
using System;
using UnityEngine;
using Mirror;
using System.Collections.Generic;

[Serializable]
public class ChannelInfo
{
    public string command; // /w etc.
    public string identifierOut; // for sending
    public string identifierIn; // for receiving
    public GameObject textPrefab;

    public ChannelInfo(string command, string identifierOut, string identifierIn, GameObject textPrefab)
    {
        this.command = command;
        this.identifierOut = identifierOut;
        this.identifierIn = identifierIn;
        this.textPrefab = textPrefab;
    }
}

[Serializable]
public struct ChatMessage
{
    public string sender;
    public string identifier;
    public string message;
    public string replyPrefix; // copied to input when clicking the message

    public ChatMessage(string sender, string identifier, string message, string replyPrefix)
    {
        this.sender = sender;
        this.identifier = identifier;
        this.message = message;
        this.replyPrefix = replyPrefix;
    }

    // construct the message
    public string Construct()
    {
        return "<b>" + sender + identifier + ":</b> " + message;
    }
}

[RequireComponent(typeof(PlayerGuild))]
[RequireComponent(typeof(PlayerParty))]
[DisallowMultipleComponent]
public class PlayerChat : NetworkBehaviour
{
    [Header("Components")] // to be assigned in inspector
    public PlayerGuild guild;
    public PlayerParty party;
    public Player player;

    [Header("Channels")]
    public ChannelInfo whisperChannel = new ChannelInfo("/w", "(TO)", "(FROM)", null);
    public ChannelInfo localChannel = new ChannelInfo("", "", "", null);
    public ChannelInfo partyChannel = new ChannelInfo("/p", "(Party)", "(Party)", null);
    public ChannelInfo guildChannel = new ChannelInfo("/g", "(Guild)", "(Guild)", null);
    public ChannelInfo allyChannel = new ChannelInfo("/a", "(Ally)", "(Ally)", null);
    public ChannelInfo infoChannel = new ChannelInfo("", "(Info)", "(Info)", null);

    public List<GameObject> textPrefab;

    public SyncList<ChatMessage> infoChat = new SyncList<ChatMessage>();
    public SyncList<ChatMessage> localChat = new SyncList<ChatMessage>();
    public SyncList<ChatMessage> whisperChat = new SyncList<ChatMessage>();
    public SyncList<ChatMessage> partyChat = new SyncList<ChatMessage>();
    public SyncList<ChatMessage> guildChat = new SyncList<ChatMessage>();
    public SyncList<ChatMessage> allyChat = new SyncList<ChatMessage>();

    public int infoToSee;
    public int localToSee;
    public int whisperToSee;
    public int partyToSee;
    public int guildToSee;
    public int allyToSee;

    [Header("Other")]
    public int maxLength = 70;

    [Header("Events")]
    public UnityEventString onSubmit;

    public override void OnStartServer()
    {
        // test messages
        infoChat.Add(new ChatMessage("", infoChannel.identifierIn, "Use /w NAME to whisper", ""));
        infoChat.Add(new ChatMessage("", infoChannel.identifierIn, "Use /p for party chat", ""));
        infoChat.Add(new ChatMessage("", infoChannel.identifierIn, "Use /g for guild chat", ""));
        infoChat.Add(new ChatMessage("", infoChannel.identifierIn, "Or click on a message to reply", ""));
        guildChat.Add(new ChatMessage("Someone", guildChannel.identifierIn, "Anyone here?", "/g "));
        partyChat.Add(new ChatMessage("Someone", partyChannel.identifierIn, "Let's hunt!", "/p "));
        whisperChat.Add(new ChatMessage("Someone", whisperChannel.identifierIn, "Are you there?", "/w Someone "));
        localChat.Add(new ChatMessage("Someone", localChannel.identifierIn, "Hello!", "/w Someone "));
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        infoChat.Callback += OnInfoMessageListChanged;
        localChat.Callback += OnLocalMessageListChanged;
        whisperChat.Callback += OnWhisperMessageListChanged;
        partyChat.Callback += OnPartyMessageListChanged;
        guildChat.Callback += OnGuildMessageListChanged;
        allyChat.Callback += OnAllianceMessageListChanged;
    }

    void OnInfoMessageListChanged(SyncList<ChatMessage>.Operation op, int index, ChatMessage oldSlot, ChatMessage newSlot)
    {
        if (UIChat.singleton.selectedChat == 0 && UIChat.singleton.isOpen)
        {
            UIChat.singleton.Display(0);
        }
        else
        {
            infoToSee++;
            UIChat.singleton.bubbleMessages[0].thisGameObject.SetActive(true);
            UIChat.singleton.bubbleMessages[0].amount.text = infoToSee.ToString();
        }
    }

    void OnLocalMessageListChanged(SyncList<ChatMessage>.Operation op, int index, ChatMessage oldSlot, ChatMessage newSlot)
    {
        if(UIChat.singleton.selectedChat == 1 && UIChat.singleton.isOpen)
        {
            UIChat.singleton.Display(1);
        }
        else
        {
            localToSee++;
            UIChat.singleton.bubbleMessages[1].thisGameObject.SetActive(true);
            UIChat.singleton.bubbleMessages[1].amount.text = localToSee.ToString();
        }
    }

    void OnWhisperMessageListChanged(SyncList<ChatMessage>.Operation op, int index, ChatMessage oldSlot, ChatMessage newSlot)
    {
        if (UIChat.singleton.selectedChat == 2 && UIChat.singleton.isOpen)
        {
            UIChat.singleton.Display(2);
        }
        else
        {
            whisperToSee++;
            UIChat.singleton.bubbleMessages[2].thisGameObject.SetActive(true);
            UIChat.singleton.bubbleMessages[2].amount.text = whisperToSee.ToString();
        }
    }

    void OnPartyMessageListChanged(SyncList<ChatMessage>.Operation op, int index, ChatMessage oldSlot, ChatMessage newSlot)
    {
        if (UIChat.singleton.selectedChat == 3 && UIChat.singleton.isOpen)
        {
            UIChat.singleton.Display(3);
        }
        else
        {
            partyToSee++;
            UIChat.singleton.bubbleMessages[3].thisGameObject.SetActive(true);
            UIChat.singleton.bubbleMessages[3].amount.text = partyToSee.ToString();
        }
    }

    void OnGuildMessageListChanged(SyncList<ChatMessage>.Operation op, int index, ChatMessage oldSlot, ChatMessage newSlot)
    {
        if (UIChat.singleton.selectedChat == 4 && UIChat.singleton.isOpen)
        {
            UIChat.singleton.Display(4);
        }
        else
        {
            guildToSee++;
            UIChat.singleton.bubbleMessages[4].thisGameObject.SetActive(true);
            UIChat.singleton.bubbleMessages[4].amount.text = guildToSee.ToString();
        }
    }

    void OnAllianceMessageListChanged(SyncList<ChatMessage>.Operation op, int index, ChatMessage oldSlot, ChatMessage newSlot)
    {
        if (UIChat.singleton.selectedChat == 5 && UIChat.singleton.isOpen)
        {
            UIChat.singleton.Display(5);
        }
        else
        {
            allyToSee++;
            UIChat.singleton.bubbleMessages[5].thisGameObject.SetActive(true);
            UIChat.singleton.bubbleMessages[5].amount.text = allyToSee.ToString();
        }
    }

    // submit tries to send the string and then returns the new input text
    [Client]
    public string OnSubmit(string text)
    {
        // not empty and not only spaces?
        if (!string.IsNullOrWhiteSpace(text))
        {
            // command in the commands list?
            // note: we don't do 'break' so that one message could potentially
            //       be sent to multiple channels (see mmorpg local chat)
            string lastCommand = "";
            if (UIChat.singleton.selectedChat == 2)
            {
                // whisper
                (string user, string message) = ParsePM(whisperChannel.command, text);
                if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(message))
                {
                    if (user != name)
                    {
                        //lastCommand = whisperChannel.command + " " + user + " ";
                        CmdMsgWhisper(user, message);
                    }
                    else Debug.Log("cant whisper to self");
                }
                else Debug.Log("invalid whisper format: " + user + "/" + message);
            }
            else if (UIChat.singleton.selectedChat == 3)
            {
                // party
                //string msg = ParseGeneral(partyChannel.command, text);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    //lastCommand = partyChannel.command + " ";
                    CmdMsgParty(text, player.name);
                }
            }
            else if (UIChat.singleton.selectedChat == 4)
            {
                // guild
                //string msg = ParseGeneral(guildChannel.command, text);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    //lastCommand = guildChannel.command + " ";
                    CmdMsgGuild(text,player.name);
                }
            }
            else if (UIChat.singleton.selectedChat == 5)
            {
                // guild
                //string msg = ParseGeneral(allyChannel.command, text);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    //lastCommand = guildChannel.command + " ";
                    CmdMsgAlly(text, player.name);
                }
            }
            else if (!text.StartsWith("/"))
            {
                // local chat is special: it has no command
                lastCommand = "";
                CmdMsgLocal(text, player.name);
            }


            // addon system hooks
            onSubmit.Invoke(text);

            // input text should be set to lastcommand
            return lastCommand;
        }

        // input text should be cleared
        return "";
    }

    // parse a message of form "/command message"
    internal static string ParseGeneral(string command, string msg)
    {
        // return message without command prefix (if any)
        return msg.StartsWith(command + " ") ? msg.Substring(command.Length + 1) : "";
    }

    // parse a private message
    internal static (string user, string message) ParsePM(string command, string pm)
    {
        // parse to /w content
        string content = ParseGeneral(command, pm);

        // now split the content in "user msg"
        if (content != "")
        {
            // find the first space that separates the name and the message
            int i = content.IndexOf(" ");
            if (i >= 0)
            {
                string user = content.Substring(0, i);
                string msg = content.Substring(i + 1);
                return (user, msg);
            }
        }
        return ("", "");
    }

    // networking //////////////////////////////////////////////////////////////
    [Command]
    void CmdMsgLocal(string message, string sender)
    {
        if (message.Length > maxLength) return;
        string identifier = sender != name ? localChannel.identifierIn : localChannel.identifierOut;
        string reply = whisperChannel.command + " " + sender + " "; // whisper

        foreach (Player player in Player.onlinePlayers.Values)
        {
            if(player.chat.localChat.Count >= 100)
            {
                player.chat.localChat.RemoveAt(0);
            }
            player.chat.localChat.Add(new ChatMessage(sender, identifier, message, reply));
        }
        // it's local chat, so let's send it to all observers via ClientRpc
        //RpcMsgLocal(name, message);
    }

    [Command]
    void CmdMsgParty(string message,string sender)
    {
        if (message.Length > maxLength) return;

        // send message to all online party members
        if (party.InParty())
        {
            foreach (string member in party.party.members)
            {
                if (Player.onlinePlayers.TryGetValue(member, out Player onlinePlayer))
                {
                    // call TargetRpc on that GameObject for that connection
                    //onlinePlayer.chat.TargetMsgParty(name, message);
                    string reply = whisperChannel.command + " " + sender + " "; // whisper
                    if (onlinePlayer.chat.partyChat.Count >= 100)
                    {
                        onlinePlayer.chat.partyChat.RemoveAt(0);
                    }
                    onlinePlayer.chat.partyChat.Add(new ChatMessage(sender, 
                        onlinePlayer.name == sender ? partyChannel.identifierIn : partyChannel.identifierOut, 
                        message, reply));
                }
            }
        }
    }

    [Command]
    void CmdMsgGuild(string message, string sender)
    {
        if (message.Length > maxLength) return;

        // send message to all online guild members
        if (guild.InGuild())
        {
            foreach (GuildMember member in guild.guild.members)
            {
                if (Player.onlinePlayers.TryGetValue(member.name, out Player onlinePlayer))
                {
                    // call TargetRpc on that GameObject for that connection
                    //onlinePlayer.chat.TargetMsgGuild(name, message);
                    string reply = whisperChannel.command + " " + sender + " "; // whisper
                    if (onlinePlayer.chat.guildChat.Count >= 100)
                    {
                        onlinePlayer.chat.guildChat.RemoveAt(0);
                    }
                    onlinePlayer.chat.guildChat.Add(new ChatMessage(sender,
                        onlinePlayer.name == sender ? guildChannel.identifierIn : guildChannel.identifierOut,
                        message, reply));
                }
            }
        }
    }

    [Command]
    void CmdMsgAlly(string message, string sender)
    {
        if (message.Length > maxLength) return;

        // send message to all online guild members
        if (guild.InGuild())
        {
            foreach (GuildMember member in guild.guild.members)
            {
                if (Player.onlinePlayers.TryGetValue(member.name, out Player onlinePlayer))
                {
                    // call TargetRpc on that GameObject for that connection
                    //onlinePlayer.chat.TargetMsgGuild(name, message);
                    if (onlinePlayer.chat.allyChat.Count >= 100)
                    {
                        allyChat.RemoveAt(0);
                    }
                    string reply = whisperChannel.command + " " + sender + " ";
                    onlinePlayer.chat.allyChat.Add(new ChatMessage(sender,
                        onlinePlayer.name == sender ? allyChannel.identifierIn : allyChannel.identifierOut, 
                        message, reply));
                }
            }
            for (int i = 0; i < player.playerAlliance.guildAlly.Count; i++)
            {
                if (GuildSystem.guilds.TryGetValue(player.playerAlliance.guildAlly[i], out Guild guildTarget))
                {
                    foreach (GuildMember member in guildTarget.members)
                    {
                        if (Player.onlinePlayers.TryGetValue(member.name, out Player onlinePlayer))
                        {
                            // call TargetRpc on that GameObject for that connection
                            //onlinePlayer.chat.TargetMsgGuild(name, message);
                            if (onlinePlayer.chat.allyChat.Count >= 100)
                            {
                                allyChat.RemoveAt(0);
                            }
                            string reply = whisperChannel.command + " " + sender + " ";
                            onlinePlayer.chat.allyChat.Add(new ChatMessage(sender,
                                onlinePlayer.name == sender ? allyChannel.identifierIn : allyChannel.identifierOut, 
                                message, reply));
                        }
                    }
                }
            }
        }
    }

    [Command]
    public void CmdMsgWhisper(string playerName, string message)
    {
        if (message.Length > maxLength) return;

        // find the player with that name
        if (Player.onlinePlayers.TryGetValue(playerName, out Player onlinePlayer))
        {
            // receiver gets a 'from' message, sender gets a 'to' message
            // (call TargetRpc on that GameObject for that connection)
            string reply = whisperChannel.command + " " + name + " ";
            onlinePlayer.chat.whisperChat.Add(new ChatMessage(name, whisperChannel.identifierIn, message, reply));
            whisperChat.Add(new ChatMessage(name, whisperChannel.identifierOut, message, reply));
            //TargetMsgWhisperTo(playerName, message);
        }
    }

    // send a global info message to everyone
    [Server]
    public void SendGlobalMessage(string message)
    {
        foreach (Player player in Player.onlinePlayers.Values)
            player.chat.TargetMsgInfo(message);
    }

    // message handlers ////////////////////////////////////////////////////////
    //[TargetRpc]
    //public void TargetMsgWhisperFrom(string sender, string message)
    //{
    //    // add message with identifierIn
    //    string identifier = whisperChannel.identifierIn;
    //    string reply = whisperChannel.command + " " + sender + " "; // whisper
    //    UIChat.singleton.AddMessage(new ChatMessage(sender, identifier, message, reply, whisperChannel.textPrefab));
    //}

    //[TargetRpc]
    //public void TargetMsgWhisperTo(string receiver, string message)
    //{
    //    // add message with identifierOut
    //    string identifier = whisperChannel.identifierOut;
    //    string reply = whisperChannel.command + " " + receiver + " "; // whisper
    //    UIChat.singleton.AddMessage(new ChatMessage(receiver, identifier, message, reply, whisperChannel.textPrefab));
    //}

    //[ClientRpc]
    //public void RpcMsgLocal(string sender, string message)
    //{
    //    // add message with identifierIn or Out depending on who sent it
    //    string identifier = sender != name ? localChannel.identifierIn : localChannel.identifierOut;
    //    string reply = whisperChannel.command + " " + sender + " "; // whisper
    //    UIChat.singleton.AddMessage(new ChatMessage(sender, identifier, message, reply, localChannel.textPrefab));
    //}

    //[TargetRpc]
    //public void TargetMsgGuild(string sender, string message)
    //{
    //    string reply = whisperChannel.command + " " + sender + " "; // whisper
    //    UIChat.singleton.AddMessage(new ChatMessage(sender, guildChannel.identifierIn, message, reply, guildChannel.textPrefab));
    //}

    //[TargetRpc]
    //public void TargetMsgParty(string sender, string message)
    //{
    //    string reply = whisperChannel.command + " " + sender + " "; // whisper
    //    UIChat.singleton.AddMessage(new ChatMessage(sender, partyChannel.identifierIn, message, reply, partyChannel.textPrefab));
    //}

    [TargetRpc]
    public void TargetMsgInfo(string message)
    {
        AddMsgInfo(message);
    }

    // info message can be added from client too
    public void AddMsgInfo(string message)
    {
        UIChat.singleton.AddMessage(new ChatMessage("", infoChannel.identifierIn, message, ""));
    }
}
