using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using MEC;
using System;
using System.Collections.Generic;

namespace AutoCommandSender
{
    public class Plugin : Plugin<Config>
    {
        public override string Name => "AutoCommandSender";
        public override string Author => "atombombasi_55908";
        public override Version Version => new Version(1, 0, 1);

        private const string TagWaiting = "AutoCommandSender_Waiting";
        private const string TagRoundStart = "AutoCommandSender_RoundStart";
        private const string TagRoundEnd = "AutoCommandSender_RoundEnd";
        private const string TagPlayerJoin = "AutoCommandSender_PlayerJoin";

        public override void OnEnabled()
        {
            if (!Config.IsEnabled)
            {
                Log.Warn("Plugin config'ten devre dışı!");
                return;
            }

            Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
            Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnded;
            Exiled.Events.Handlers.Player.Verified += OnPlayerVerified;

            Log.Info("AutoCommandSender AKTİF → Tüm eventlerde sınırsız komut + {player} desteği!");
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnded;
            Exiled.Events.Handlers.Player.Verified -= OnPlayerVerified;

            Timing.KillCoroutines(TagWaiting);
            Timing.KillCoroutines(TagRoundStart);
            Timing.KillCoroutines(TagRoundEnd);
            Timing.KillCoroutines(TagPlayerJoin);

            base.OnDisabled();
        }

        private void OnWaitingForPlayers() => ExecuteCommands(Config.CommandsOnWaiting, TagWaiting);
        private void OnRoundStarted() => ExecuteCommands(Config.CommandsOnRoundStart, TagRoundStart);
        private void OnRoundEnded(RoundEndedEventArgs ev) => ExecuteCommands(Config.CommandsOnRoundEnd, TagRoundEnd);

        private void OnPlayerVerified(VerifiedEventArgs ev)
        {
            if (ev.Player != null)
                ExecuteCommands(Config.CommandsOnPlayerJoin, TagPlayerJoin, ev.Player);
        }

        private void ExecuteCommands(List<string> list, string tag, Player player = null)
        {
            if (list == null || list.Count == 0) return;

            Timing.KillCoroutines(tag);
            Timing.RunCoroutine(RunCommands(list, player), tag);
        }

        private IEnumerator<float> RunCommands(List<string> commands, Player player)
        {
            foreach (string cmd in commands)
            {
                if (string.IsNullOrWhiteSpace(cmd)) continue;

                string final = cmd.Trim();

                if (final.IndexOf("{player}", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (player == null)
                    {
                        if (Config.Debug) Log.Warn($"[AutoCmd] {player} yok → Komut atlandı: {final}");
                        continue;
                    }
                    final = ReplaceIgnoreCase(final, "{player}", player.Nickname);
                }

                try
                {
                    Server.ExecuteCommand(final);
                    if (Config.Debug) Log.Info($"[AutoCmd] Gönderildi → {final}");
                }
                catch (Exception e)
                {
                    Log.Error($"[AutoCmd] HATA → {final} | Sebep: {e.Message}");
                }

                yield return Timing.WaitForSeconds(0.5f);
            }
        }
        private static string ReplaceIgnoreCase(string source, string oldValue, string newValue)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(oldValue))
                return source;

            int startIndex = 0;
            while (true)
            {
                int index = source.IndexOf(oldValue, startIndex, StringComparison.OrdinalIgnoreCase);
                if (index < 0)
                    break;

                source = source.Substring(0, index) + newValue + source.Substring(index + oldValue.Length);
                startIndex = index + newValue.Length;
            }

            return source;
        }
    }

    public class Config : Exiled.API.Interfaces.IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public List<string> CommandsOnWaiting { get; set; } = new List<string>
        {
            "mp load FullMap",
            "say <color=red>Sunucu hazır!</color>"
        };

        public List<string> CommandsOnRoundStart { get; set; } = new List<string>
        {
            "say <color=lime>ROUND BAŞLADI!</color>",
            "bc <color=yellow>İyi şanslar!</color> 10",
            "cassie Round has started"
        };

        public List<string> CommandsOnRoundEnd { get; set; } = new List<string>
        {
            "say <color=orange>ROUND BİTTİ!</color>",
            "bc <color=cyan>Sonraki round yakında!</color> 12"
        };

        public List<string> CommandsOnPlayerJoin { get; set; } = new List<string>
        {
            "pm {player} <color=green>Hoş geldin {player}!</color>",
            "bc <color=aqua>{player} sunucuya katıldı!</color> 6",
            "give {player} keycardjanitor",
            "effect {player} invigorated 20"
        };
    }
}
