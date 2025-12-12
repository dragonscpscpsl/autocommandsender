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
        public override Version Version => new Version(1, 0, 0);

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

            ExecuteCommands(Config.CommandsOnWaiting);
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnded;
            Exiled.Events.Handlers.Player.Verified -= OnPlayerVerified;

            base.OnDisabled();
        }

        private void OnWaitingForPlayers() => ExecuteCommands(Config.CommandsOnWaiting);
        private void OnRoundStarted() => ExecuteCommands(Config.CommandsOnRoundStart);
        private void OnRoundEnded(RoundEndedEventArgs ev) => ExecuteCommands(Config.CommandsOnRoundEnd);

        private void OnPlayerVerified(VerifiedEventArgs ev)
        {
            if (ev.Player != null)
                ExecuteCommands(Config.CommandsOnPlayerJoin, ev.Player);
        }

        private void ExecuteCommands(List<string> list, Player player = null)
        {
            if (list == null || list.Count == 0) return;

            Timing.RunCoroutine(RunCommands(list, player));
        }

        private IEnumerator<float> RunCommands(List<string> commands, Player player)
        {
            foreach (string cmd in commands)
            {
                if (string.IsNullOrWhiteSpace(cmd)) continue;

                string final = cmd.Trim();

                if (player != null && final.IndexOf("{player}", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    final = final.Replace("{player}", player.Nickname);
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