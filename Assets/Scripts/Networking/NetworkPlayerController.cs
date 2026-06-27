using UnityEngine;
using Mirror;
using HereToSlay.Core;

namespace HereToSlay.Networking
{
    /// <summary>
    /// One instance per player. Owned by the client, commands are sent to server.
    /// Attach to the NetworkManager's player prefab.
    /// </summary>
    public class NetworkPlayerController : NetworkBehaviour
    {
        [SyncVar] public string playerName;
        [SyncVar] public string playerId;    // set on server = connectionId

        public static NetworkPlayerController LocalInstance { get; private set; }

        // ── Lifecycle ────────────────────────────────────────────────────────

        public override void OnStartLocalPlayer()
        {
            LocalInstance = this;
            CmdRegisterPlayer(PlayerPrefs.GetString("PlayerName", "Player"));
        }

        // ════════════════════════════════════════════════════════════════════
        //  CLIENT → SERVER COMMANDS
        // ════════════════════════════════════════════════════════════════════

        [Command]
        void CmdRegisterPlayer(string name)
        {
            playerName = name;
            playerId   = connectionToClient.connectionId.ToString();
            GameManager.Instance?.RegisterPlayer(connectionToClient, name);
        }

        [Command]
        public void CmdReady()
            => GameManager.Instance?.PlayerReady(playerId);

        [Command]
        public void CmdPlayCard(string cardInstanceId, string targetInstanceId, string targetPlayerId)
        {
            var action = new GameAction
            {
                actionType      = ActionType.PlayCard,
                actingPlayerId  = playerId,
                cardInstanceId  = cardInstanceId,
                targetInstanceId = targetInstanceId,
                targetPlayerId  = targetPlayerId
            };
            GameManager.Instance?.ProcessAction(action);
        }

        [Command]
        public void CmdUseHeroAbility(string heroInstanceId, string targetInstanceId, string targetPlayerId)
        {
            var action = new GameAction
            {
                actionType       = ActionType.UseHeroAbility,
                actingPlayerId   = playerId,
                cardInstanceId   = heroInstanceId,
                targetInstanceId = targetInstanceId,
                targetPlayerId   = targetPlayerId
            };
            GameManager.Instance?.ProcessAction(action);
        }

        [Command]
        public void CmdChallengeMonster(string monsterInstanceId)
        {
            var action = new GameAction
            {
                actionType       = ActionType.ChallengeMonster,
                actingPlayerId   = playerId,
                targetInstanceId = monsterInstanceId
            };
            GameManager.Instance?.ProcessAction(action);
        }

        [Command]
        public void CmdDrawCard()
        {
            GameManager.Instance?.ProcessAction(new GameAction
            {
                actionType     = ActionType.DrawCard,
                actingPlayerId = playerId
            });
        }

        [Command]
        public void CmdEndTurn()
        {
            GameManager.Instance?.ProcessAction(new GameAction
            {
                actionType     = ActionType.EndTurn,
                actingPlayerId = playerId
            });
        }

        [Command]
        public void CmdPlayModifier(string modifierInstanceId, bool targetsSelf)
        {
            GameManager.Instance?.ProcessAction(new GameAction
            {
                actionType     = ActionType.PlayModifier,
                actingPlayerId = playerId,
                cardInstanceId = modifierInstanceId,
                targetPlayerId = targetsSelf ? playerId : ""
            });
        }

        [Command]
        public void CmdPassReaction()
        {
            GameManager.Instance?.ProcessAction(new GameAction
            {
                actionType     = ActionType.PassReaction,
                actingPlayerId = playerId
            });
        }
    }
}
