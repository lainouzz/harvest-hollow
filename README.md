# Harvest Hollow

A cozy, first-person farming game made solo in Unity. Till the soil, plant and water your crops, sell your harvest to a travelling merchant, and grow your farm one plot at a time.

> **Play it on itch.io:** _link coming soon_

![Screenshot](docs/Harvest-Hollow-Farm-Logo.png)

---

## About

Harvest Hollow started as a small grid-placement experiment and grew into my first complete, released game. The goal was a short, polished, free game for my portfolio: something small enough to actually finish, but complete enough to ship with saving, menus, settings and audio.

### Core loop

```
Till soil → Water → Plant → Sleep (crops grow overnight, game saves)
     ↑                                         ↓
Expand farm ← Buy seeds ← Sell harvest ← Harvest
```

### Features

- First-person farming with a hoe, watering can, seed bag and harvest tool
- 13 crops in three tiers (starter, common, premium); your harvest is also your seed
- Watered soil makes crops grow faster and yield more, and soil dries out after a few days
- A travelling merchant truck that drives in and out, with daily stock and rare seeds
- Buyable farm plots, with a fence that automatically redraws around your farm
- Fence openings: knock out fence segments to make paths
- Sleep to advance the day, with a night-to-dawn transition screen, tips and autosave
- Save/load with **Continue** from the main menu
- Main menu, pause menu, and settings (graphics quality, resolution, volume, VFX toggle)
- First-time tutorial hints
- Baked lighting, post-processing and three quality levels (Low / Medium / High)

### Controls

| Action | Key |
|---|---|
| Move / Look | WASD / Mouse |
| Jump | Space |
| Switch tool | 1 – 5 |
| Use tool | Left click |
| Inventory | Tab |
| Buy farm plot | B (look at it) |
| Open / close fence | X |
| Pause | Esc |

---

## Tech

- **Engine:** Unity 6 (6000.0.24f1)
- **Rendering:** URP with baked lighting, Adaptive Probe Volumes and a post-processing volume
- **Input:** Unity Input System (generated `GameInput` class)
- **UI:** uGUI + TextMeshPro
- **Audio:** Audio Mixer with Master / Music / SFX groups, controlled from the settings menu

### Project structure

```
Assets/Scripts/
├── Grid/       FarmGrid (the farm's data), crops, tiles, fence, plot expansion
├── Player/     movement, look, tools, planting, sounds, fence opener
├── Economy/    wallet, day manager, income tracker
├── Merchant/   truck behaviour, SFX, VFX
├── UI/         HUD, inventory, merchant shop, menus, tutorial, day transition
├── Save/       save data, save system (JSON), save manager, asset database
├── VFX/        tool particles, VFX on/off setting
└── Dev/        temporary debug tools (not shipped)
```

### How it fits together

- **`FarmGrid` is the single source of truth.** It holds every cell's state (locked/unlocked, tilled/watered, what's planted). Other systems only *react* to it through events (`OnGridChanged`, `OnCellStateChanged`, `OnFenceChanged`). The fence, grid lines, price icons and tile icons all redraw themselves when something changes.
- **Crops grow by day, not by frame.** `DayManager.OnDayChanged` is a static event every crop listens to, so growing costs nothing per frame.
- **Data lives in ScriptableObjects.** `CropData` (how it grows) and `InventoryItem` (what it's worth) are assets, so balancing never needs a code change.
- **Saving** writes a plain `SaveData` class to JSON in `Application.persistentDataPath`. ScriptableObjects can't go into a save file, so they're stored **by asset name** and looked up again through a `SaveDatabase` asset when loading.

---

## Problems I ran into (and what I learned)

The most useful part of this project was the bugs. These are the ones that taught me the most.

### Unity lifecycle & references
- **`Instance` was null in `OnEnable`.** Singletons set themselves up in `Awake`, but another object's `OnEnable` can run first. Fix: find references in `Awake` with `FindFirstObjectByType<T>(FindObjectsInactive.Include)` and null-check them.
- **`FindAnyObjectByType` skips inactive objects.** The merchant couldn't find the Buy section because the shop UI starts hidden, so it silently never restocked. Same fix as above.
- **ScriptableObjects aren't scene objects.** `FindAnyObjectByType<InventoryItem>()` will never find an asset. Pass the reference in or subscribe to an event instead.
- **`GetComponent<AudioSource>()` returns the first one.** With two AudioSources on the player (footsteps and tools), both scripts grabbed the same one. Assign references explicitly when there's more than one.

### Movement & space
- **Player jitter when strafing.** `CharacterController` was moved in `FixedUpdate`. It has no interpolation, so it has to move in `Update` with `Time.deltaTime`.
- **The seed bag flew to (0,0,0).** It used world positions on an object parented to the player's hand. Held objects need `localPosition`.
- **Grid spacing was wrong.** The step between cells was multiplied instead of added (`cellSize * spacing` instead of `cellSize + spacing`).

### Systems
- **The fence surrounded the entire max-size grid.** It checked the grid's outer border instead of the farm's actual edge. The fix: for each unlocked cell, check each of its 4 neighbours and only build fence where the neighbour is locked. I also had to destroy the old fence before rebuilding, or fences piled up on every purchase.
- **Clicking an inventory item also planted a seed.** The sound and animation scripts checked "is a menu open?", but the planting script didn't. Every script that reacts to the same input needs the same guard.
- **The merchant never restocked.** It checked `state == Arriving` right *after* setting the state to `AtFarm`. The previous state has to be read before changing it.
- **Buy/Sell tabs stopped working.** The tab buttons were inside the sections they hid, so hiding the Sell section also hid the Sell button.

### Audio
- **Volume sliders did nothing, or broke the audio.** I had exposed the mixer groups' **Pitch** instead of **Volume**, so the decibel maths was being written into pitch.
- **Saved volume didn't apply on startup.** `AudioMixer.SetFloat` gets overwritten in `Awake` when the mixer loads its default snapshot. It has to be applied a frame later. And a slider that's already at its saved value never fires `onValueChanged`, so the value has to be pushed to the mixer directly.

### Rendering, UI & builds
- **Day/night vs baked lighting.** Rotating the sun breaks baked lighting, because the bounce light is frozen at bake time. Rather than a real-time cycle, the day switches behind a "sleep" transition screen and keeps one good-looking bake.
- **UI only looked right at 1440p.** The Canvas Scaler was on *Constant Pixel Size*. Switching to *Scale With Screen Size* (and anchoring each element to its corner of the screen) fixed it across resolutions.
- **Editor-only code breaks builds.** Stray `using UnityEditor;` / `using System.Drawing;` lines compile in the editor but fail in a player build.
- **A low quality level silently disabled features.** The low-end URP asset used a different renderer without the Decal feature, and old light probes instead of Adaptive Probe Volumes. Every quality level needs the same renderer features.

### Data
- **Onion seeds grew tomatoes.** A wrong `produce` reference in one `CropData` asset. Data bugs are bugs too, and they're harder to spot than code bugs.
- **Unused fields cause confusion.** `CropData` had `seedCost`/`sellValue` that nothing read, which contradicted the real prices on the items. I removed them so there's one source of truth.

---

## Roadmap / ideas

- Birds that go after your crops, plus a scarecrow (or something louder) to protect them
- Watering that matters every day, not just at planting time
- Watercan refill from a refill station
- In-depth UI for crops, player and merchant
- An end-of-day summary (earned, spent, harvested)
- The merchant arriving each morning instead of on a timer
- Plot prices based on distance from the farmhouse
- A way to customize the farm better.

---

## Credits

- **Design, development & art direction:** linus lind aka lainouzz
- **Third-party assets:** "Effect textures and prefabs Magicpot Inc." - "Mini Pickup Truck 2 Premisul" - "Pixel Art Farm Tools Anonymous Side Projects" - "Farm Game UI - Starter 2D maanetorn" - "Pandazole - Farm Ranch low poly Pack Pandazole", these assets are from Unity assets store
- **AI assistance:** Claude (Anthropic) helped with parts of the code/debug, some UI text and the sleep-screen background image to speed things up.

## License

All rights reserved for the game. Third-party assets keep their own licenses.
