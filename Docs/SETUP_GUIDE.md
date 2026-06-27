# Here to Slay — Unity Setup Guide
## Full Multiplayer Implementation (Mirror Networking)

---

## 1. PROJECT SETUP

### Unity Version
- **Unity 2022.3 LTS** (recommended)

### Required Packages (Window → Package Manager)
```
Mirror          → https://github.com/MirrorNetworking/Mirror  (add by git URL)
TextMeshPro     → built-in (Install TMP Essentials when prompted)
```

### Folder Structure
Create this layout under `Assets/`:
```
Assets/
├── Scripts/
│   ├── Core/          ← CardData.cs, PlayerState.cs, GameState.cs
│   ├── Cards/         ← CardCatalogue.cs
│   ├── Effects/       ← EffectResolver.cs
│   ├── Networking/    ← GameManager.cs, NetworkPlayerController.cs
│   └── UI/            ← UIManager.cs, CardUIItem.cs, OpponentPanel.cs, LobbyUI.cs
├── Resources/
│   ├── Cards/
│   │   ├── Heroes/    ← HeroCardData ScriptableObjects
│   │   ├── Monsters/  ← MonsterCardData ScriptableObjects
│   │   ├── Items/     ← MagicItemCardData ScriptableObjects
│   │   ├── Modifiers/ ← ModifierCardData ScriptableObjects
│   │   └── Leaders/   ← PartyLeaderCardData ScriptableObjects
│   └── CardArt/       ← Imported SVG sprites (name MUST match asset name in catalogue)
├── Prefabs/
│   ├── Cards/         ← HeroCard, MonsterCard, MagicItemCard, ModifierCard, PartyLeader prefabs
│   ├── UI/            ← OpponentPanel, MiniHero prefabs
│   └── Network/       ← NetworkPlayer prefab
└── Scenes/
    └── Main.unity
```

---

## 2. SCRIPTABLEOBJECT CREATION

For each entry in `CardCatalogue.cs`, create a matching ScriptableObject:

**Example — HeroFighter1:**
1. Right-click in `Resources/Cards/Heroes/` → Create → HereToSlay → HeroCard
2. Name it exactly `HeroFighter1`
3. Fill in: Card Name = "Ching Ching", Hero Class = Fighter, Roll Requirement = 4
4. Add Effect: Type = DestroyMagicItem, Trigger = OnAction, Description = "Roll 4+: Destroy a Magic Item"
5. Artwork slot: import `HeroFighter1.svg` as sprite, assign here

**Repeat for all 50+ cards.** The asset name MUST match `CardCatalogue.assetName` exactly.

---

## 3. SCENE SETUP

### NetworkManager GameObject
1. Create empty GameObject → name it `NetworkManager`
2. Add Component: `HereToSlayNetworkManager` (our subclass, found in LobbyUI.cs)
3. Settings:
   - Max Connections: 6
   - Player Prefab: assign NetworkPlayer prefab (see below)
   - Transport: KCP Transport (included with Mirror)

### NetworkPlayer Prefab
1. Create empty GameObject → name it `NetworkPlayer`
2. Add Components:
   - `NetworkIdentity` (check: Server Only = false, Local Player Authority = true)
   - `NetworkPlayerController` (our script)
3. Drag to `Prefabs/Network/` to make it a prefab
4. Assign to NetworkManager → Player Prefab

### GameManager GameObject
1. Create empty GameObject → name it `GameManager`
2. Add Components:
   - `NetworkIdentity` (check: Server Only = true)
   - `GameManager` (our script)
   - `EffectResolver` is created internally, no component needed
3. Inspector settings:
   - Use Expansions: ✓ (check to include expansion cards)
   - Starting Hand Size: 5

---

## 4. CARD PREFAB SETUP

### HeroCard Prefab (repeat for other types)
Create a UI Canvas child with this hierarchy:
```
HeroCard (GameObject)
├── Image - Background        [Image component - assign heroCardPrefab frame sprite]
├── Image - Artwork           [Image component - populated at runtime]
├── TMP - CardName            [TextMeshPro]
├── TMP - CardType            [TextMeshPro - e.g. "Fighter Hero"]
├── TMP - RollText            [TextMeshPro - e.g. "Roll 4+"]
├── TMP - Description         [TextMeshPro]
├── Image - HighlightBorder   [Image component - yellow glow, default inactive]
└── Button                    [Button component - transparent, full card size]
```
Add `CardUIItem` component to the root → wire all fields in Inspector.

---

## 5. UI CANVAS SETUP

Main Canvas (Screen Space - Camera or Overlay):
```
Canvas
├── LobbyPanel
│   ├── PlayerNameInput       [TMP_InputField]
│   ├── IPAddressInput        [TMP_InputField]
│   ├── HostButton            [Button]
│   ├── JoinButton            [Button]
│   ├── ReadyButton           [Button]
│   ├── StatusLabel           [TextMeshPro]
│   └── PlayerListArea        [Vertical Layout Group]
│
└── GamePanel
    ├── HUD
    │   ├── ActivePlayerLabel [TextMeshPro]
    │   ├── PhaseLabel        [TextMeshPro]
    │   ├── ActionsLabel      [TextMeshPro]
    │   └── DeckCountLabel    [TextMeshPro]
    │
    ├── MonsterRowArea        [Horizontal Layout Group - holds 3 monster cards]
    ├── PlayerHandArea        [Horizontal Layout Group - local player's hand]
    ├── PlayerPartyArea       [Horizontal Layout Group - local player's party]
    ├── OpponentListArea      [Vertical Layout Group - opponent panels]
    │
    ├── DicePopupPanel        [Panel - default inactive]
    │   ├── DiceRollText      [TextMeshPro]
    │   └── DiceResultText    [TextMeshPro]
    │
    ├── ReactionBar           [Panel - shown during ReactionWindow phase]
    │   └── PassReactionBtn   [Button]
    │
    ├── ActionButtons
    │   ├── DrawCardBtn       [Button]
    │   └── EndTurnBtn        [Button]
    │
    └── WinScreen             [Panel - default inactive]
        └── WinnerLabel       [TextMeshPro]
```

Add `UIManager` component to the GamePanel root → wire all fields.
Add `LobbyUI` component to Canvas root → wire all fields.

---

## 6. NETWORKING — HOW IT WORKS

```
HOST PLAYER                          CLIENT PLAYERS
───────────────────────────────────  ───────────────────────────────
GameManager (server authority)  ←──  NetworkPlayerController.Cmd*()
      │                                       ↑
      │  [ClientRpc] RpcReceiveState()        │
      └──────────────────────────────────────→│
                                     UIManager.HandleStateUpdate()
```

- **All game logic runs on the server** (the host)
- Clients send `Cmd*` commands (validated on server)
- Server broadcasts full `GameState` as JSON after every change
- Clients deserialize and redraw their UI

### Hosting a game:
1. Player 1 clicks **Host** → becomes server + client
2. Players 2–6 enter the host's IP and click **Join**
3. All click **Ready** → game starts automatically when all are ready

### For internet play (beyond LAN):
- Use a relay service: **Mirror's Edgegap** integration or **Fish-Networking's FishyUnityTransport**
- Or port-forward 7777 UDP on the host's router

---

## 7. IMPORTING SVG ARTWORK

Unity doesn't natively render SVGs at runtime. Two options:

### Option A — Convert to PNG (simplest)
1. Open each `.svg` in Inkscape or a browser
2. Export as PNG at 512×720 (card ratio)
3. Import to Unity: Texture Type = Sprite (2D and UI)
4. Name exactly as `CardCatalogue.assetName` (e.g. `HeroFighter1.png`)
5. Place in `Resources/CardArt/`

### Option B — Unity Vector Graphics package (advanced)
1. Package Manager → Add by name: `com.unity.vectorgraphics`
2. Import SVGs directly; set Importer to Vector
3. Generate sprites from SVG assets

---

## 8. CARD COUNTS (full deck)

| Category        | Cards | Copies | Total |
|----------------|-------|--------|-------|
| Base Heroes     | 18    | 1      | 18    |
| Expansion Heroes| 12    | 1      | 12    |
| Magic Items     | 12    | 2      | 24    |
| Modifiers       | 8     | 3      | 24    |
| **Main Deck**   |       |        | **78**|
| Base Monsters   | 12    | 1      | 12    |
| Exp. Monsters   | 5     | 1      | 5     |
| **Monster Deck**|       |        | **17**|
| Party Leaders   | 10    | 1      | 10    |

---

## 9. TESTING

1. **Single machine test:** File → Build Settings → Build, then run built exe + Play Mode simultaneously
2. **Two machines:** Both on same WiFi, client enters host's local IP (e.g. `192.168.1.x`)
3. **Debug:** Window → Mirror → NetworkManager HUD shows connection status

---

## 10. EXPANSION / MODDING

To add new cards:
1. Add entry to `CardCatalogue.cs` with unique `assetName`
2. Create matching ScriptableObject in `Resources/Cards/`
3. Add SVG/PNG artwork to `Resources/CardArt/`
4. Add effect handler to `EffectResolver.cs` if new `EffectType` needed

The system is data-driven — no code changes needed for new cards that use existing effect types.
