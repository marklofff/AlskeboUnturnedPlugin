﻿using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlskeboUnturnedPlugin
{
    public class AlskeboUnturnedPlugin : RocketPlugin
    {
        class PlayerData
        {
            public bool isDriving = false;
        }

        Dictionary<string, InteractableVehicle> playerVehicles = new Dictionary<string, InteractableVehicle>();
        Dictionary<ushort, UnturnedPlayer> vehiclePlayers = new Dictionary<ushort, UnturnedPlayer>();
        Dictionary<string, PlayerData> playerDataMap = new Dictionary<string, PlayerData>();

        public override void LoadPlugin()
        {
            base.LoadPlugin();
            Logger.LogWarning("\tAlskeboPlugin Loaded Sucessfully");
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerUpdateStance += onPlayerUpdateStance;
            Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerUpdateGesture += UnturnedPlayerEvents_OnPlayerUpdateGesture;
        }

        private void UnturnedPlayerEvents_OnPlayerUpdateGesture(UnturnedPlayer player, Rocket.Unturned.Events.UnturnedPlayerEvents.PlayerGesture gesture)
        {
            UnturnedChat.Say(player, "You gestured");
        }

        private void onPlayerUpdateStance(UnturnedPlayer player, byte stance)
        {
            if (!playerDataMap.ContainsKey(player.Id) || playerDataMap[player.Id] == null)
                playerDataMap.Add(player.Id, new PlayerData());

            PlayerData playerData = playerDataMap[player.Id];

            if (stance == 6 && !playerData.isDriving)
            {
                InteractableVehicle currentClaimedVehicle = getClaimedVehicle(player);
                if (player.CurrentVehicle != null && (currentClaimedVehicle == null || currentClaimedVehicle.id != player.CurrentVehicle.id))
                {
                    claimVehicle(player, player.CurrentVehicle);
                    UnturnedChat.Say(player, "You have claimed a new vehicle.");
                }
                else
                    UnturnedChat.Say(player, "You have entered your claimed vehicle.");
            }
            else if (stance != 6 && playerData.isDriving)
            {
                UnturnedChat.Say(player, "You have exited your claimed vehicle.");
            }
            else
                UnturnedChat.Say(player, "Something happened! " + stance.ToString());
        }

        private void claimVehicle(UnturnedPlayer player, InteractableVehicle vehicle)
        {
            if (vehiclePlayers.ContainsKey(vehicle.id))
                unclaimVehicle(vehiclePlayers[vehicle.id]);

            InteractableVehicle currentClaimedVehicle = getClaimedVehicle(player);
            if (currentClaimedVehicle == null || currentClaimedVehicle.id != vehicle.id)
            {
                unclaimVehicle(player);
                playerVehicles.Add(player.Id, vehicle);
                vehiclePlayers.Add(vehicle.id, player);
            }
        }

        private void unclaimVehicle(UnturnedPlayer player)
        {
            InteractableVehicle vehicle = getClaimedVehicle(player);
            if (!vehicle)
                return;

            playerVehicles.Remove(player.Id);
            vehiclePlayers.Remove(vehicle.id);

            if (vehicle.fuel <= 0)
                vehicle.tellExploded(); //TEST
        }

        private InteractableVehicle getClaimedVehicle(UnturnedPlayer player)
        {
            if (playerVehicles.ContainsKey(player.Id))
                return playerVehicles[player.Id];
            return null;
        }

    }
}