# CLAUDE.md

**Keep this file current.** When a session changes something described here, update it in the same session, and add anything non-obvious learned the hard way. Be concise: the rule and why.

## What this project is

The test ground and showcase for **UI Animation Player** (`DOTweenAnimationPlayer`), which lives in the sibling repo `../UnityAnimationTools`. **Read that repo's `CLAUDE.md` before changing the tool**; this file is only about this project. It replaced the `GameCraftSep2026Jam` / `GameCraftSep2026P2Jam` test scenes in October 2026.

- Unity **6000.3.25f1**, URP 2D template, DOTween Pro 1.3.045 with Setup and Create ASMDEF done, TMP Essential Resources imported.
- `Packages/manifest.json` links the tool: `file:../../UnityAnimationTools/DOTweenAnimationPlayer`, so tests run against the working copy. Both repos must sit side by side. To test a release as users get it, switch that line to the `#release` git URL by hand, and back.
- **Input System only** (`activeInputHandler: 1`): EventSystems need `InputSystemUIInputModule` (with `AssignDefaultActions()`), and scripts read `UnityEngine.InputSystem`, never `UnityEngine.Input`.
- Only the animation player is covered. FlipbookAnimation and SelectableEvents aren't installed here.

## Layout

| Folder | Holds | Namespace |
| --- | --- | --- |
| `Assets/Shared` | `Station` + `StationBoard` (tiles and navigation), fixtures both sides use, generated sprites / sounds / shaders / materials, editor kit and the two checks | `UIAnimLab`, `UIAnimLab.EditorTools` |
| `Assets/UnitTests` | 11 hand-test scenes, their fixtures, sets and materials, and their builders | `UIAnimLab.UnitTests`, `.Builders` |
| `Assets/Showcase` | 21 scenes of capture frames, one per wiki page, `ShotDirector`, `SHOTLIST.md`, and their builders | `UIAnimLab.Showcase`, `.Builders` |

The author asked for UnitTests and Showcase to be separate "projects"; neither references the other, and anything both use goes in Shared.

## Scenes are generated

Every scene is built by a static method under `Scripts/Editor/Builders`, run from **Tools → UI Anim Lab → Build Unit Tests / Build Showcase** (one item per scene, plus Build All). The author chose to keep the builders after the scenes existed.

- A build makes a new additive scene, fills it, and saves it over the old file, so the scene's GUID survives; the scene you have open is never touched. **A target scene open with unsaved changes is refused.** If it was open and clean, it is reopened built.
- **A rebuild discards hand edits to that scene.** Fold any wanted edit into the builder.
- `LabAssets.Ensure()` generates the shared sprites, WAVs and materials once; an existing file is never rewritten, so its GUID (and every scene using it) survives. Change one by deleting it and rebuilding.
- `Anim.Asset(path, ...)` rewrites an existing Animation Set in place rather than recreating it, for the same reason.
- `SHOTLIST.md` is written from the showcase scenes by **Build Showcase → Write SHOTLIST.md** (Build All does it too). Don't edit it by hand.

### The kit (`Assets/Shared/Scripts/Editor/Kit`)

- `LabBoard.Begin(path, showcase, columns)` → `.Readme` / `.Test(code, title, width, stageHeight)` / `.Shot(page, title)` / `.Row()` → `.Save()` (lays the tiles on a grid from the canvas's top-left, builds the index bar).
- `Tile`: `.Summary .Watch .HowTo .Expect .Needs .Capture`, `.Player(...)` (on the tile root) / `.PlayerOn(go, ...)`. `Tile.Stage` is where objects go; positions are from its centre.
- `UI`: element factories (`Box`, `Square`, `Icon`, `Label`, `Caption`, `Button`, `PlayButton`, `Trigger`, `Group`...). `Box` is rounded; use `Square` where exact edges matter.
- `Anim.New(name, steps)` gives what `+` on an empty list gives (Interrupt Others on, COMPLETE, REPORT) - `new UIAnimation()` alone reads as old data (STOP). Options chain: `.Loop .Fps .SnapAll .SnapPerStep .NoFromImmediately .NoInterrupt .InterruptStop .InterruptComplete(silent) .Then(player, next) .OnDone(...) .Note`. `Anim.Set/Add/SetShared/Options/SharedOnComplete` write the player's private fields by reflection.
- `S`: one factory per step type (`Pos`, `Scale`, `Alpha`, `Tint`, `MatFloat`, `Prop`, `PunchScale`...), then `.From .FromBase .To .ToBase .ToCur .ToRest .With .Dur .Delay .Ease .Curve .Path .Snap .Target .Format`. Values are written exactly as the Inspector would.

### Conventions

- **A tile is named `CODE  Title`** (two spaces); showcase frames `NN  Title`. No `/` in any name (`Transform.Find`).
- A test tile's player usually sits on the tile root and reaches objects by Target Path `Stage/<name>` (`UT.Path`). Never aim a step at the tile root itself.
- **A tile states intended behaviour** - from the wiki and the tool's CLAUDE.md - not whatever the code does today. A tile that fails is a finding to report, not a tile to "fix".
- **Every label is SF Pro Display** (`Assets/Shared/Fonts`, the author's choice): `UI.Text` gives the Regular SDF asset, or the Bold one for `FontStyles.Bold` (the flag is cleared so TMP doesn't faux-bold it); legacy `Text` gets the Regular `.otf` (`UI.LegacyFont`). The Regular asset's Font Weights table maps 400 italic, 700 and 700 italic to the real faces, so `<b>` / `<i>` in tile text use them too (edited in the YAML, so no glyphs were saved with it). The SDF assets are dynamic, so building or playing writes glyphs into them: **`git checkout -- Assets/Shared/Fonts` after a build** rather than committing that churn.
- **No literal rich-text tags in tile text** (`<b>` meant as text): TMP and the Inspector both parse it.
- **Don't ship data that warns on every recompile.** An Animation Set with two animations of one name warned from `OnValidate` at every domain reload, so A8 has the tester rename one instead.
- Showcase frames are 960 x 540 with square corners and a `RectMask2D`, and **carry no instructions inside the frame** - only the caption above it, which a focused 16:9 Game view crops off. Directions go in the Station's fields (Inspector) and SHOTLIST.md.
- A play-mode fixture exposes `public void` methods wired to buttons with persistent listeners (visible in the Inspector), and shows what happened in white and what should have in yellow.

## Navigating boards

`StationBoard` (on each Canvas): right-drag pans, wheel zooms, Tab / Shift+Tab next / previous, 1-9 and 0 jump to tile N (the intro doesn't count), F focuses the tile under the cursor, Home fits all, H toggles the index bar. It moves the Board, never the tiles, so no resting value changes, and runs on unscaled time without DOTween. In the showcase, `ShotDirector` listens on the focused frame only: Space puts back and plays the cues, R puts back, L loops.

## Checking

- **Tools → UI Anim Lab → Smoke Check All Scenes** (`SceneSmokeCheck.CheckAll`): every active player in every scene is resolved and every animation played to its end in edit mode; prints warnings, errors and exceptions. Intentional ones: 06 G3 (path miss), 07 I1 and I8 (the type check), 11 X1-X3, showcase 14 (stencil Mask) and 21 (the warning frame). Anything else is news.
- **Tools → UI Anim Lab → Inspector Check All Scenes** (`InspectorSmokeCheck.CheckAll`): draws every Station's and player's Inspector fully unfolded in a hidden window and reports exceptions.
- Both open scenes additively, skip scenes already open, and close without saving.
- **Play-mode fixtures are checked by pressing their buttons from `eval`** (`button.onClick.Invoke()`), after `Application.runInBackground = true`. **Each `unity cmd eval` takes about 2 s**, so time a sequence inside Unity: press the first button and schedule the second with `DG.Tweening.DOVirtual.DelayedCall(seconds, ..., true)` in the same eval. **Match a tile by `"CODE  "` with both spaces**: `P1` alone also matches `P10`, whose buttons share names.

## Unity CLI notes for this project

- Call builders and checks by reflection, e.g. `System.Type.GetType("UIAnimLab.UnitTests.Builders.UT, Assembly-CSharp-Editor").GetMethod("BuildAll").Invoke(null, null)`; `eval` can't see the project's types directly. `--result-only` gives clean JSON.
- `capture_game_view` needs a Game view window open, shows Screen Space Overlay UI only in play mode, and its `save_path` must be inside the project: it lands under `Assets/`. Delete the captures (and their `.meta`) afterwards.
- After editing `Packages/manifest.json` with the Editor in the background, `unity cmd editor_focus` makes it re-resolve; `AssetDatabase.Refresh()` alone didn't.
- TMP Essentials were imported with `AssetDatabase.ImportPackage("Packages/com.unity.ugui/Package Resources/TMP Essential Resources.unitypackage", false)`.

## Open findings

- Steps whose target lacks the component they need (I8 steps 0 and 3, X1 step 0) warn **on every Play** ("A STEP WAS SKIPPED", from `UIAnimationStep.HasTarget`), while the wiki's Warnings and Targets pages say a broken step warns once when the player starts. I8 and X1 state the documented behaviour, so they fail until the tool or the wiki changes.
