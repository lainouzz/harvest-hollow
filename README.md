# Harvest Hollow

> **Status: Complete (v1.3.1).** Development has finished; this is the final version.

A cozy, first-person farming game made solo in Unity. Till the soil, plant and water your crops, sell your harvest to a travelling merchant, restore an old greenhouse, keep chickens, and grow your farm one plot at a time.

> **Play it on itch.io:** https://lainouzz.itch.io/harvest-hollow

![Screenshot](docs/Harvest-Hollow-Farm-Logo.png)

---

## About

Harvest Hollow started as a small grid-placement experiment and grew into my first complete, released game. The goal was a short, polished, free game for my portfolio: something small enough to actually finish, but complete enough to ship with saving, menus, settings and audio. After the 1.0 release it got three content updates, and 1.3 is the final one.

### Core loop

```
Till soil → Water (daily) → Plant → Sleep (crops grow overnight, game saves)
     ↑                                                  ↓
Expand farm ← Buy seeds ← Sell harvest & eggs ← Harvest / collect eggs
```

### Features

- First-person farming with a hoe, watering can, seed bag and harvest tool
- 15 crops in three tiers; your harvest is also your seed
- Daily watering: soil dries out overnight, and wet soil makes crops grow faster and yield more
- A watering can with limited water, refilled at the well
- A restorable greenhouse with self-watering raised beds and greenhouse-only crops (strawberries and tulips)
- Chickens that wander their pen and lay an egg every morning
- A travelling merchant that arrives each morning, with daily stock and rare seeds
- Buyable farm plots, with a fence that automatically redraws around your farm
- Fence openings: knock out fence segments to make paths
- An end-of-day summary (harvested, earned, spent) before you sleep
- Sleep to advance the day, with a night-to-dawn transition screen, tips and autosave
- Save/load with **Continue** from the main menu; old saves keep working across updates
- Main menu, pause menu, settings (graphics quality, resolution, volume, VFX toggle) and an auto-scrolling credits screen
- First-time tutorial hints
- Baked lighting, colour grading and three quality levels (Low / Medium / High)
- ...and at least one secret somewhere on the farm

### Controls

| Action | Key |
|---|---|
| Move / Look | WASD / Mouse |
| Jump | Space |
| Switch tool | 1 – 5 |
| Use tool | Left click |
| Interact (bed, well, greenhouse, coop) | E |
| Inventory | Tab |
| Buy farm plot | B (look at it) |
| Open / close fence | X |
| Pause | Esc |

---

## Version history

| Version | What changed |
|---|---|
| **1.0** | First release |
| **1.1** | Morning routine: end-of-day summary, buy-plot prompt, merchant each morning, harvest popups |
| **1.2** | The water update: daily watering, a watering can with limited water, refilling at the well, a water bar |
| **1.3** | The final harvest: greenhouse, chickens and eggs, new tool models, new roads and paths, hills around the world, new sky and colour grading, credits screen |
| **1.3.1** | Fixed tools sometimes getting stuck mid-swing after switching |

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
├── Player/     movement, look, tools, planting, watering can, well, sounds, fence opener
├── Animals/    chickens, coop, egg collection
├── Economy/    wallet, day manager, daily stats
├── Merchant/   truck behaviour, SFX, VFX
├── UI/         HUD, inventory, merchant shop, menus, credits, tutorial, day transition, prompts
├── Save/       save data, save system (JSON), save manager, asset database
└── VFX/        tool particles, misting spray, VFX on/off setting
```

### How it fits together

- **`FarmGrid` is the single source of truth.** It holds every cell's state (locked/unlocked, tilled/watered, what's planted). Other systems only *react* to it through events (`OnGridChanged`, `OnCellStateChanged`, `OnFenceChanged`). The fence, grid lines, price icons and tile icons all redraw themselves when something changes.
- **Several grids, one set of rules.** The main farm and every greenhouse bed are separate `FarmGrid`s. The planting code works on whichever grid you're aiming at, and harvests are reported through one static `FarmGrid.AnyHarvested` event, so stats and popups don't need to know how many grids exist.
- **The night runs in a fixed order.** `FarmGrid` grows every crop (reading whether its soil is wet), *then* dries the soil, and the greenhouse re-waters its beds. If each crop listened for the new day itself, the order would be random.
- **Data lives in ScriptableObjects.** `CropData` (how it grows) and `InventoryItem` (what it's worth) are assets, so balancing never needs a code change.
- **Saving** writes a plain `SaveData` class to JSON in `Application.persistentDataPath`. ScriptableObjects can't go into a save file, so they're stored **by asset name** and looked up again through a `SaveDatabase` asset when loading. New fields always default to "nothing yet" (no chickens, broken greenhouse, water *used* rather than water *left*), so saves from older versions load cleanly.

---

## Problems I ran into (and what I learned)

The most useful part of this project was the bugs. These are the ones that taught me the most.

### Unity lifecycle & references
- **`Instance` was null in `OnEnable`.** Singletons set themselves up in `Awake`, but another object's `OnEnable` can run first. Fix: find references in `Awake` with `FindFirstObjectByType<T>(FindObjectsInactive.Include)` and null-check them.
- **`FindAnyObjectByType` skips inactive objects.** The merchant couldn't find the Buy section because the shop UI starts hidden, so it silently never restocked. Same fix as above.
- **`FindAnyObjectByType` gets risky once there are several of something.** With the greenhouse beds added there were suddenly many `FarmGrid`s, and "find any" could return a bed instead of the main farm. Systems now ask for the main grid explicitly or listen to a shared event.
- **ScriptableObjects aren't scene objects.** `FindAnyObjectByType<InventoryItem>()` will never find an asset. Pass the reference in or subscribe to an event instead.
- **`GetComponent<AudioSource>()` returns the first one.** With two AudioSources on the player (footsteps and tools), both scripts grabbed the same one. Assign references explicitly when there's more than one.

### Movement & space
- **Grid spacing was wrong.** The step between cells was multiplied instead of added (`cellSize * spacing` instead of `cellSize + spacing`).
- **Only half of a raised bed could be planted.** The bed's collider wasn't on the Ground layer, so the aim ray went straight through it and hit the floor below. Looking down at an angle, the floor hit lands *behind* where you aim, so the far half of the bed never counted. A missing layer produced a bug that looked like geometry.

### Systems
- **The fence surrounded the entire max-size grid.** It checked the grid's outer border instead of the farm's actual edge. The fix: for each unlocked cell, check each of its 4 neighbours and only build fence where the neighbour is locked. I also had to destroy the old fence before rebuilding, or fences piled up on every purchase.
- **Clicking an inventory item also planted a seed.** The sound and animation scripts checked "is a menu open?", but the planting script didn't. Every script that reacts to the same input needs the same guard.
- **The merchant never restocked.** It checked `state == Arriving` right *after* setting the state to `AtFarm`. The previous state has to be read before changing it.
- **Buy/Sell tabs stopped working.** The tab buttons were inside the sections they hid, so hiding the Sell section also hid the Sell button.
- **Saving only on sleep invites save-scumming.** For anything random, a player can quit and reload to undo a bad result. The fix was to save straight after that action, not to add a limit.

### Animation
- **Tools froze mid-swing after switching.** Switching was allowed before a swing ended, so the tool was switched off in a tilted pose. When an Animator's object is switched back on, it takes the *current* pose as its default, and the empty Idle state kept writing that frozen pose back. The fix: remember each tool's rest pose and restore it before switching the tool on.

### Audio
- **Volume sliders did nothing, or broke the audio.** I had exposed the mixer groups' **Pitch** instead of **Volume**, so the decibel maths was being written into pitch.
- **Saved volume didn't apply on startup.** `AudioMixer.SetFloat` gets overwritten in `Awake` when the mixer loads its default snapshot. It has to be applied a frame later. And a slider that's already at its saved value never fires `onValueChanged`, so the value has to be pushed to the mixer directly.

### Rendering, UI & builds
- **Day/night vs baked lighting.** Rotating the sun breaks baked lighting, because the bounce light is frozen at bake time. Rather than a real-time cycle, the day switches behind a "sleep" transition screen and keeps one good-looking bake.
- **"Non finite value" light-bake errors.** Some generated meshes had zero-area triangles (cone tips with a radius of 0). Their normals can't be calculated, the result is NaN, and it spreads through the baked lighting. Removing degenerate triangles fixed it.
- **The sky changed the whole scene's colour.** With ambient light taken from the skybox, a golden-hour sky tinted everything orange. Setting the ambient light to a hand-picked gradient kept the new sky but gave me control over the lighting.
- **UI only looked right at 1440p.** The Canvas Scaler was on *Constant Pixel Size*. Switching to *Scale With Screen Size* (and anchoring each element to its corner of the screen) fixed it across resolutions.
- **Editor-only code breaks builds.** Stray `using UnityEditor;` / `using System.Drawing;` lines compile in the editor but fail in a player build.
- **A low quality level silently disabled features.** The low-end URP asset used a different renderer without the Decal feature, and old light probes instead of Adaptive Probe Volumes. Every quality level needs the same renderer features.

### Data
- **Onion seeds grew tomatoes.** A wrong `produce` reference in one `CropData` asset. Data bugs are bugs too, and they're harder to spot than code bugs.
- **Unused fields cause confusion.** `CropData` had `seedCost`/`sellValue` that nothing read, which contradicted the real prices on the items. I removed them so there's one source of truth.

---

## Ideas that didn't make it

- Birds that go after your crops, plus a scarecrow to protect them
- Feeding chickens with wheat to give wheat a bigger role
- Plot prices based on distance from the farmhouse
- More ways to customize and decorate the farm

---

## Credits

- **Design, development & art direction:** linus lind aka lainouzz
- **Third-party assets (Unity Asset Store and others):** Pandazole – Farm Ranch Pack · CraftPix.net – Free Farming Crops · Cartoon Animal Pack 01 · Premisul – Mini Pickup Truck 2 · Low-Poly Simple Nature Pack · Low Poly Nature – FREE Vegetation · The_KnightShift – Skybox Sampler · Ouroboros – Stylized Textures Mini Set · Magicpot Inc. – Effect Textures and Prefabs · maanetorn – Farm Game UI – Starter 2D · Anonymous Side Projects – Pixel Art Farm Tools · Lilita One font (SIL OFL 1.1)
- **AI assistance:** Claude (Anthropic) helped with some parts of code and debugging

## License

All rights reserved for the game. Third-party assets keep their own licenses.
