# MRAV2026 — Mixed Reality Audio Visual Performance System

## Project Location
- **Repo:** https://github.com/seanfunks/MRAV2026
- **Local:** `c:\repos\suibiweng\MRAV_Eagle`
- **Branch:** `main` (single-branch, fresh start Feb 2026)
- **Unity Version:** 6000.3.7f1 (Unity 6 LTS) — upgraded from 2022.3.35f1 on Feb 2026
- **Platform Target:** Oculus Quest (Android), Meta XR SDK 66.0 (upgrade to v74+ pending for OpenXR migration)

## What This Project Is
**MRDJ (Mixed Reality DJ)** — a VR audio-visual performance app built in Unity. A performer triggers visual effects and bird behaviors in real-time from external music software (Ableton Live) or hardware controllers via OSC (Open Sound Control). The audience/performer experiences it in mixed reality on the Quest headset.

## Architecture Overview

### Input Pipeline (Phase 2 — Current)
```
External Controller (4x4 pad) / Ableton Live
    → OSC over UDP (port 6969)
    → OSC.cs (UnityOSC library)
    → BirdInputMapper.cs (action IDs 0-15)
    → BirdOrchestrator.cs (routes to correct BirdGroup)
    → BirdGroup (button priority queue → active behavior)
    → Per-group behavior (direct positioning or physics steering)

Keyboard (for dev/testing):
    1234 / QWER / ASDF / ZXCV → same 4x4 grid, same action IDs
```

### Key Systems

| System | Scripts | Purpose |
|--------|---------|---------|
| **Bird Orchestrator (NEW)** | `BirdOrchestrator.cs`, `BirdGroup.cs` | Group-based bird management with per-group input routing and behaviors |
| **Bird Flight Physics** | `BirdFlightPhysics.cs`, `BirdFlightAnimator.cs` | Per-bird physics: gravity, lift, drag, steering, banking, auto flap/glide cycle |
| **Bird Flock Manager** | `BirdFlightController.cs` | Legacy flock spawner (still works, can be used alongside orchestrator) |
| **Input Mapper** | `BirdInputMapper.cs` | 4x4 keyboard + OSC → 16 action IDs |
| **Camera** | `CameraMouseLook.cs` | Right-click + mouse to look around (dev testing) |
| **Bird Flight (OLD)** | `BirdController.cs`, `birdPath.cs`, `BirdSpinning.cs` | Legacy path-based movement (kept for rollback, toggle with F12) |
| **Particle VFX** | `MRAVctrl.cs`, `OneShotFly.cs` | OSC-triggered particle effects and background VFX |
| **Spline Path** | `SplineFly.cs` (SimpleFollowSpline) | Catmull-Rom spline following for player/camera path |
| **OSC** | `Assets/UnityOSC/OSC.cs` | Network communication library — `SetAddressHandler()` supports multiple handlers per address |

### Important File Paths
```
Assets/Scripts/BirdMovement/              — All bird scripts (old + new + orchestrator)
Assets/Scripts/BirdMovement/BirdOrchestrator.cs  — Scene-level group manager
Assets/Scripts/BirdMovement/BirdGroup.cs         — Per-group logic + button priority
Assets/Scripts/BirdMovement/BirdFlightPhysics.cs — Per-bird physics
Assets/Scripts/BirdMovement/BirdFlightAnimator.cs — Per-bird animation driver
Assets/Scripts/BirdMovement/BirdFlightController.cs — Legacy flock spawner
Assets/Scripts/BirdMovement/BirdInputMapper.cs   — 4x4 input (keyboard + OSC)
Assets/Scripts/BirdMovement/CameraMouseLook.cs   — Dev camera control
Assets/Scripts/MRAVctrl.cs                — Main OSC→VFX controller
Assets/Scripts/fromAlbeton.cs             — Ableton Live OSC receiver
Assets/UnityOSC/OSC.cs                   — OSC library
Assets/SplineFly.cs                       — SimpleFollowSpline (Catmull-Rom)
Assets/Scenes/260206 - Phase 2 - Buttons.unity  — Current dev scene
Assets/Scenes/260206 - BirdsActingLikeBirds.unity — Phase 1 test scene
Assets/Scenes/BirdFlightTest.unity        — Original blank test scene
Assets/ANIMALS FULL PACK/Birds Pack/Golden Eagle/ — Eagle model + animations
```

### Eagle Assets
- **Model:** `SK_Eagle.FBX`
- **Prefab:** `Assets/SK_Eagle.prefab`
- **Animations:** `Eagle@Fly.FBX`, `Eagle@Glide.FBX`, `Eagle@Falling.FBX`
- **Animator Controller:** `Eagle_Controller RG1.controller`
  - Parameters: `GlideABit`, `FlyABit`, `Dive` (triggers), `FlappySpeedAdjust` (float)
  - States: Fly (default), Glide, Falling, eagleWingTuck
  - Note: `eagleWingTuck` has NO transitions — must use `Animator.Play()` directly

### Design Decisions
- **No ECS Swarms for birds** — the ECS Swarms asset exists in the project but is deliberately not used for birds. All bird code is MonoBehaviour/Transform based.
- **OSC handlers are additive** — registering bird handlers on the same OSC addresses as MRAVctrl won't break particle effects. Both fire simultaneously.
- **Prefab spawning** — birds are spawned at runtime from a prefab via orchestrator or BirdFlightController.
- **System toggle** — F12 switches between old (BirdController) and new (BirdFlightPhysics) systems at runtime.

---

## Bird Group Orchestrator — Phase 2 System

### 4x4 Input Grid → Group Mapping

The 16 buttons are organized into 4 groups (2x2 blocks):

```
 Group 1: FlapperBuddies  |  Group 2: CirclingFlappers
      1   2                |       3   4
      Q   W                |       E   R
 --------------------------+---------------------------
 Group 3: FlockPatterns    |  Group 4: VisualEffect
      A   S                |       D   F
      Z   X                |       C   V
```

**Action ID mapping:**
```
Group 1 (FlapperBuddies):    actionIDs 0,1,4,5   (keys 1,2,Q,W)  → localButtons 0,1,2,3
Group 2 (CirclingFlappers):   actionIDs 2,3,6,7   (keys 3,4,E,R)  → localButtons 0,1,2,3
Group 3 (FlockPatterns):      actionIDs 8,9,12,13 (keys A,S,Z,X)  → localButtons 0,1,2,3
Group 4 (VisualEffect):       actionIDs 10,11,14,15 (keys D,F,C,V) → localButtons 0,1,2,3
```

**OSC addresses:** `/bird/R1C1` through `/bird/R4C4`, plus legacy `/T1`-`/T4`, `/TM1`-`/TM4`, `/BM1`-`/BM4`, `/B1`-`/B4`

### Button Priority System (per group)

Each group has 4 buttons with this behavior:
- **Momentary**: behavior active only while button is held
- **First-pressed wins**: if button 0 is held and button 1 pressed, button 0's behavior stays active
- **Handoff on release**: if button 0 released while button 1 still held, transition to button 1's behavior
- **All released = default**: revert to group's default behavior

Implementation: `BirdGroup.cs` maintains an ordered list of held buttons (press order). First item = active behavior. Release removes from list; if it was first, next takes over.

### Groups Overview

| Group | Name | Birds | Default Behavior | Button Behaviors |
|-------|------|-------|------------------|------------------|
| 1 | FlapperBuddies | 2 (2x scale) | Hover in front of camera, facing away, gentle bob | TBD — placeholder stubs ready |
| 2 | CirclingFlappers | TBD | TBD | TBD |
| 3 | FlockPatterns | TBD | TBD | TBD |
| 4 | VisualEffect | N/A (no birds) | Deferred to later phase | Deferred |

### FlapperBuddies — Current Implementation

**Default behavior (no buttons pressed):**
- 2 eagles spawned at 2x scale
- **Directly positioned** relative to camera (physics disabled — `BirdFlightPhysics` and `BirdFlightAnimator` are disabled)
- Positioned in front of camera, spread left/right, always facing away from user
- Smooth lerp follow (8f * deltaTime) so they don't feel rigidly attached
- Each bird bobs out of sync (sine wave, 0.3 amplitude, 1.5 Hz)

**Inspector tuning (BirdOrchestrator):**
```
flapperBuddiesCount = 2
flapperBuddiesScale = 2.0    — double size
flapperDistance = 10.0        — units in front of camera
flapperSpread = 3.0           — left/right offset
flapperHeight = 1.5           — above camera height
```

**Why physics is disabled for FlapperBuddies:** The flight physics model (gravity, thrust, drag, steering) is designed for free-flying birds. FlapperBuddies need to be "locked" to the camera view — they move with your head. Direct positioning via `Vector3.Lerp` each frame is simpler and more reliable for this. When button behaviors need physics (e.g., "fly away and come back"), physics can be re-enabled per bird.

### Scene Setup (BirdOrchestrator)
1. Create empty GameObject → name `BirdOrchestrator`
2. Add Component: `BirdOrchestrator`
3. Add Component: `BirdInputMapper`
4. Inspector: drag self → Input Mapper, Main Camera → Camera Transform, SK_Eagle prefab → Bird Prefab

---

## Bird Flight Physics — How It Works

### Core Principle: Separation of Concerns
The flight system is split into layers:

```
BirdOrchestrator / BirdFlightController (WHAT to do)
        ↓ calls SetSteeringTarget() or directly positions
BirdFlightPhysics (HOW to fly there)
        ↓ reads physics state
BirdFlightAnimator (HOW it looks)
```

**Key insight for pathing:** The physics layer only receives a target position via `SetSteeringTarget()`. It never teleports. It always flies there using real forces. So ANY pathing system just needs to call `SetSteeringTarget()` with the next destination.

**Exception:** FlapperBuddies bypass physics entirely — they use direct positioning because they need to be camera-locked.

### FixedUpdate Execution Order (BirdFlightPhysics.cs)

```
Step 0: FLAP/GLIDE CYCLE  Auto-toggle isFlapping on timer
                           Flap: 3-7s random, Glide: 5-10s random
                           Each bird starts at random point in cycle

Step 1: GRAVITY            velocity.y -= 9.81 * dt

Step 2: LIFT (always on)   velocity.y += (gravity + 1.0) * dt     ← base: counters gravity + slight climb
                           velocity.y += sin(flapAngle) * 3.0 * dt  ← periodic bobbing
                           Lift is ALWAYS active (simulates wind/thermals during glide)
                           isFlapping only affects animation, not physics

Step 3: THRUST             velocity += transform.forward * 6.0 * dt

Step 4: DRAG               horizontal: quadratic (0.1 * hSpeed²) — X/Z only
                           vertical: light linear (0.02 * vy) — Y only
                           CRITICAL: split so horizontal speed doesn't kill lift

Step 5: SPEED CLAMP        horizontal only (2..15) — does NOT touch velocity.y

Step 6: STEERING           Craig Reynolds: steer = (desired - velocity).normalized * 4.0
                           Produces smooth arcs, not sharp turns

Step 7: INTEGRATE          position += velocity * dt

Step 8: ORIENT             Slerp toward LookRotation(velocity) + bank roll

Step 9: RECORD             verticalVelocity = velocity.y (for animator)
```

### Auto Flap/Glide Cycle
Birds automatically alternate between flapping and gliding:
- **Flap duration:** 3-7 seconds (random per cycle)
- **Glide duration:** 5-10 seconds (random per cycle)
- Each bird starts at a random point in its cycle (staggered)
- **Lift stays constant** during glide — `isFlapping` is purely visual (drives Fly↔Glide animation)
- No altitude loss during glide (simulates wind/thermals keeping bird aloft)

### Current Tuning Values
```
gravity = 9.81            — standard Earth gravity
liftForce = 3.0           — bobbing amplitude (base lift = gravity+1.0)
flapFrequency = 1.5       — flaps per second
flapDurationMin/Max = 3/7 — seconds of flapping before glide
glideDurationMin/Max = 5/10 — seconds of gliding before flap
thrustForce = 6.0         — forward propulsion
horizontalDrag = 0.1      — quadratic drag on X/Z only
verticalDrag = 0.02       — very light linear drag on Y
maxHorizontalSpeed = 15   — horizontal speed cap
minSpeed = 2.0            — minimum horizontal speed
steeringForce = 4.0       — turning toward targets
bankAngleMax = 45         — max roll degrees on turns
rotationSmoothing = 5.0   — how fast bird reorients
```

### Animation States (BirdFlightAnimator.cs)
- **Fly** — active when `isFlapping == true`
- **Glide** — active when `isFlapping == false` and speed > 8.0
- Diving/Falling disabled (code exists but won't auto-trigger)
- `FlappySpeedAdjust` mapped from speed → animation playback speed (0.5x-2.0x)

### How to Add Pathing Without Breaking Flight Physics

**Use `SetSteeringTarget()` — the bird flies there naturally:**
```csharp
birdPhysics[i].SetSteeringTarget(waypoints[currentIndex]);
```

**What NOT to do:**
- Don't set `transform.position` directly — bypasses physics
- Don't set `velocity` directly — breaks gravity/lift balance
- Don't disable `BirdFlightPhysics` unless the group intentionally bypasses physics (like FlapperBuddies)

**What's safe:**
- `SetSteeringTarget(pos)` — bird flies there naturally
- `SetFlapping(bool)` — toggle flap/glide animation (physics stays same)
- `ApplyImpulse(vec)` — one-shot force push
- `ClearSteeringTarget()` — bird continues on current heading

---

## Project Phases

### Phase 1: Bird Flight Physics — COMPLETE
- [x] Core physics (gravity, lift, drag, thrust, steering, banking)
- [x] Split horizontal/vertical drag (critical bug fix)
- [x] New lift model (base lift + bobbing, always active)
- [x] Auto flap/glide cycle (birds naturally alternate)
- [x] Lift constant during glide (simulates wind/thermals)
- [x] Animation driver (Fly/Glide only, diving disabled)
- [x] Flock manager with prefab spawning
- [x] 4x4 input mapper infrastructure (keyboard + OSC)
- [x] Old/new system toggle (F12)
- [x] Camera mouse look (right-click to look around)
- [x] Eagle prefab + dev scenes
- [x] Push to new repo (seanfunks/MRAV2026)
- [x] Birds acting like birds — flight looks/feels natural

### Phase 2: Group Orchestrator + Behaviors — IN PROGRESS
- [x] BirdGroup class with button priority queue (first-pressed wins, handoff)
- [x] BirdOrchestrator with group spawning and input routing
- [x] 4x4 grid → 4 groups (2x2 blocks) mapping
- [x] FlapperBuddies default behavior (2 birds, 2x scale, camera-locked hover)
- [ ] FlapperBuddies button behaviors (4 buttons, added one at a time)
- [ ] CirclingFlappers group (spawn count, default behavior, button behaviors)
- [ ] FlockPatterns group (spawn count, default behavior, button behaviors)
- [ ] Test with keyboard
- [ ] Test with external OSC controller

### Phase 3: Waypoint Navigation
- [ ] Waypoint data structure
- [ ] Sequential target follower using `SetSteeringTarget()`
- [ ] Arrival detection
- [ ] Smooth "gliding around corners" — steering force produces natural arcs automatically
- [ ] Optional: Unity Spline integration for visual path editing
- [ ] Optional: per-waypoint speed/flap settings

### Phase 4: Integration & Performance
- [ ] Integrate bird system with existing MRAVctrl particle VFX
- [ ] VisualEffect group (Group 4) — define behaviors
- [ ] Build to Oculus Quest — verify performance
- [ ] Mixed reality passthrough testing
- [ ] Live performance workflow testing with Ableton + controller

### Future Ideas
- [ ] Per-bird targeting (individual buttons control individual birds)
- [ ] Dynamic flock size (spawn/despawn birds via input)
- [ ] Bird-to-music reactivity (OSC audio data drives flight parameters)
- [ ] Predator-prey behaviors between bird groups
- [ ] Diving/Falling animations re-enabled with controlled trigger conditions
