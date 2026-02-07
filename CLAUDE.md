# MRAV2026 — Mixed Reality Audio Visual Performance System

## Project Location
- **Repo:** https://github.com/seanfunks/MRAV2026
- **Local:** `c:\repos\suibiweng\MRAV_Eagle`
- **Branch:** `main` (single-branch, fresh start Feb 2026)
- **Unity Version:** Check `ProjectSettings/ProjectVersion.txt`
- **Platform Target:** Oculus Quest (Android), Meta XR SDK 66.0

## What This Project Is
**MRDJ (Mixed Reality DJ)** — a VR audio-visual performance app built in Unity. A performer triggers visual effects and bird behaviors in real-time from external music software (Ableton Live) or hardware controllers via OSC (Open Sound Control). The audience/performer experiences it in mixed reality on the Quest headset.

## Architecture Overview

### Input Pipeline
```
External Controller (4x4 pad) / Ableton Live
    → OSC over UDP (port 6969)
    → OSC.cs (UnityOSC library)
    → BirdInputMapper.cs (action IDs 0-15)
    → BirdFlightController.cs (flock commands)
    → BirdFlightPhysics.cs (per-bird forces)

Keyboard (for dev/testing):
    1234 / QWER / ASDF / ZXCV → same 4x4 grid, same action IDs
```

### Key Systems

| System | Scripts | Purpose |
|--------|---------|---------|
| **Bird Flight (NEW)** | `BirdFlightPhysics.cs`, `BirdFlightAnimator.cs`, `BirdFlightController.cs`, `BirdInputMapper.cs` | Physics-based independent bird flight with gravity, lift, drag, steering, banking |
| **Bird Flight (OLD)** | `BirdController.cs`, `birdPath.cs`, `BirdSpinning.cs` | Legacy path-based bird movement (kept for rollback, toggle with F12) |
| **Particle VFX** | `MRAVctrl.cs`, `OneShotFly.cs` | OSC-triggered particle effects and background VFX |
| **Spline Path** | `SplineFly.cs` (SimpleFollowSpline) | Catmull-Rom spline following for player/camera path |
| **OSC** | `Assets/UnityOSC/OSC.cs` | Network communication library — `SetAddressHandler()` supports multiple handlers per address |

### Important File Paths
```
Assets/Scripts/BirdMovement/          — All bird scripts (old + new)
Assets/Scripts/MRAVctrl.cs            — Main OSC→VFX controller
Assets/Scripts/fromAlbeton.cs         — Ableton Live OSC receiver
Assets/UnityOSC/OSC.cs               — OSC library
Assets/SplineFly.cs                   — SimpleFollowSpline (Catmull-Rom)
Assets/Scenes/BirdFlightTest.unity    — Dev scene for bird flight work
Assets/Scenes/bobSandbox-*.unity      — Legacy sandbox scenes (old system)
Assets/ANIMALS FULL PACK/Birds Pack/Golden Eagle/  — Eagle model + animations
```

### Eagle Assets
- **Model:** `SK_Eagle.FBX`
- **Animations:** `Eagle@Fly.FBX`, `Eagle@Glide.FBX`, `Eagle@Falling.FBX`
- **Animator Controller:** `Eagle_Controller RG1.controller`
  - Parameters: `GlideABit`, `FlyABit`, `Dive` (triggers), `FlappySpeedAdjust` (float)
  - States: Fly (default), Glide, Falling, eagleWingTuck
  - Note: `eagleWingTuck` has NO transitions — must use `Animator.Play()` directly

### Design Decisions
- **No ECS Swarms for birds** — the ECS Swarms asset exists in the project but is deliberately not used for birds. All bird code is MonoBehaviour/Transform based.
- **OSC handlers are additive** — registering bird handlers on the same OSC addresses as MRAVctrl won't break particle effects. Both fire simultaneously.
- **Prefab spawning** — birds are spawned at runtime from a prefab via `BirdFlightController`. No manual scene placement needed.
- **System toggle** — F12 switches between old (BirdController) and new (BirdFlightPhysics) systems at runtime. Components are enabled/disabled, not destroyed.

---

## Bird Flight Physics — How It Works (Current Working Code)

### Core Principle: Separation of Concerns
The flight system is split into layers so that pathing/steering can change WITHOUT breaking physics:

```
BirdFlightController (WHAT to do)     — "go to this point" / "orbit here"
        ↓ calls SetSteeringTarget()
BirdFlightPhysics (HOW to fly there)  — gravity, lift, drag, steering forces
        ↓ reads physics state
BirdFlightAnimator (HOW it looks)     — Fly/Glide animation from velocity
```

**Key insight for pathing:** The physics layer only receives a target position via `SetSteeringTarget()`. It never teleports. It always flies there using real forces. So ANY pathing system (waypoints, splines, formations) just needs to call `SetSteeringTarget()` with the next destination — gravity, lift, and drag continue working automatically.

### FixedUpdate Execution Order (BirdFlightPhysics.cs)

```
Step 1: GRAVITY         velocity.y -= 9.81 * dt
                        (constant downward pull every frame)

Step 2: LIFT            if (isFlapping):
                          velocity.y += (gravity + 1.0) * dt     ← base lift: exactly counters gravity + slight climb
                          velocity.y += sin(flapAngle) * 3.0 * dt  ← periodic bobbing for natural motion
                        if (!isFlapping):
                          no lift → bird sinks gradually (gliding)

Step 3: THRUST          velocity += transform.forward * 6.0 * dt
                        (always pushes bird forward in its facing direction)

Step 4: DRAG            horizontal: quadratic (0.1 * hSpeed²) — only affects X/Z
                        vertical: light linear (0.02 * vy) — barely touches Y
                        CRITICAL: drag is SPLIT so horizontal speed doesn't kill lift

Step 5: SPEED CLAMP     horizontal only (2..15) — does NOT touch velocity.y
                        CRITICAL: clamping is horizontal-only so lift isn't squashed

Step 6: STEERING        if (hasTarget):
                          desired = toTarget.normalized * currentSpeed
                          steer = (desired - velocity).normalized * 4.0
                          velocity += steer * dt
                        (Craig Reynolds steering — produces smooth arcs, not sharp turns)

Step 7: INTEGRATE       position += velocity * dt

Step 8: ORIENT          rotation = Slerp toward LookRotation(velocity)
                        + bank/roll from lateral velocity component

Step 9: RECORD          verticalVelocity = velocity.y  (for animator)
```

### Why Drag and Speed Clamping Are Split Horizontal/Vertical
**This was the hardest bug to find.** Originally drag used total `velocity.sqrMagnitude` and speed clamping normalized the entire vector. This meant:
- A bird going fast horizontally (speed 15) generated drag of `0.3 * 225 = 67.5` — which crushed the Y component
- Speed clamping to maxSpeed would proportionally squash velocity.y when the bird was at max horizontal speed

The fix: horizontal drag only sees X/Z speed, vertical drag is tiny (0.02), and speed clamping only scales X/Z. This lets lift work independently of horizontal flight speed.

### Current Tuning Values
```
gravity = 9.81          — standard Earth gravity
liftForce = 3.0         — bobbing amplitude (NOT the main lift — that's gravity+1.0)
flapFrequency = 1.5     — flaps per second (base, desynchronized per bird)
thrustForce = 6.0       — forward propulsion
horizontalDrag = 0.1    — quadratic drag on X/Z only
verticalDrag = 0.02     — very light linear drag on Y
maxHorizontalSpeed = 15 — horizontal speed cap
minSpeed = 2.0          — minimum horizontal speed (birds never stop)
steeringForce = 4.0     — how hard birds turn toward targets
bankAngleMax = 45       — max roll degrees on turns
rotationSmoothing = 5.0 — how fast bird reorients
```

### Animation States (BirdFlightAnimator.cs)
Currently only two states are active:
- **Fly** — default, always active when `isFlapping == true`
- **Glide** — activates when `isFlapping == false` and speed > 8.0

Diving/Falling is disabled (code exists but won't auto-trigger). WingTuck available via `ForceWingTuck()` API only.

`FlappySpeedAdjust` float is continuously mapped from bird speed → animation playback speed (0.5x at minSpeed to 2.0x at maxSpeed).

### Flock Manager (BirdFlightController.cs)
- Spawns N birds from a prefab in a circle at height 8
- Default behavior: each bird steers toward a unique point spread around a flock center offset from the player
- Flock center slowly orbits (orbitSpeed = 0.3)
- Each bird has a unique spread angle so they don't clump

### How to Add Pathing Without Breaking Flight Physics

**The steering system is the bridge.** Any pathing approach works the same way:

```csharp
// Waypoint system example — just feed sequential targets
birdPhysics[i].SetSteeringTarget(waypoints[currentIndex]);
if (Vector3.Distance(bird.position, waypoints[currentIndex]) < arrivalRadius)
    currentIndex++;  // advance to next waypoint

// Spline system example — sample point ahead on spline
float lookAhead = 0.05f; // 5% ahead on the spline
Vector3 target = spline.EvaluatePosition(progress + lookAhead);
birdPhysics[i].SetSteeringTarget(target);
```

**What NOT to do:**
- Don't set `transform.position` directly — bypasses all physics
- Don't set `velocity` directly — breaks gravity/lift balance
- Don't disable `BirdFlightPhysics` and move the bird manually — loses all flight behavior

**What's safe:**
- `SetSteeringTarget(pos)` — the bird flies there naturally, gravity and lift keep working
- `SetFlapping(bool)` — toggle flap/glide (gliding = gradual altitude loss)
- `ApplyImpulse(vec)` — one-shot force push (for reactions to events)
- `ClearSteeringTarget()` — bird continues on current heading

**"Gliding around corners"** happens naturally — steering force is limited to 4.0, so when a bird approaches a waypoint at speed 15 it can't turn instantly. It arcs through the turn, banking as it goes. Tighter turns = increase steeringForce. Wider arcs = decrease it.

---

## 4x4 Input Grid
```
Row 0:  1  2  3  4    → Action IDs 0-3
Row 1:  Q  W  E  R    → Action IDs 4-7
Row 2:  A  S  D  F    → Action IDs 8-11
Row 3:  Z  X  C  V    → Action IDs 12-15

OSC addresses: /bird/R1C1 through /bird/R4C4
Legacy OSC:    /T1-T4, /TM1-TM4, /BM1-BM4, /B1-B4
```
Action-to-behavior bindings are NOT yet mapped — deferred until flight tuning is complete.

---

## Project Phases

### Phase 1: Bird Flight Physics — IN PROGRESS
- [x] Core physics script (gravity, lift, drag, thrust, steering, banking)
- [x] Animation driver (Fly/Glide from physics state)
- [x] Flock manager with prefab spawning
- [x] 4x4 input mapper infrastructure (keyboard + OSC)
- [x] Old/new system toggle (F12)
- [x] Dev scene (BirdFlightTest)
- [x] Push to new repo (seanfunks/MRAV2026)
- [x] Create eagle prefab with all components wired up
- [x] First flight test — fixed drag/lift interaction bug (split horizontal/vertical)
- [x] Disabled diving/falling (Fly + Glide only for now)
- [ ] Tune lift/gravity balance until vertical velocity oscillates near zero
- [ ] Verify flap animation syncs with physics bobbing
- [ ] Tune steering smoothness and banking feel
- [ ] Tune until flight looks/feels natural

### Phase 2: Input → Bird Behavior Mapping
- [ ] Define what each of the 16 buttons does to the birds
- [ ] Wire action IDs to specific flock commands
- [ ] Test with keyboard
- [ ] Test with external OSC controller

### Phase 3: Waypoint Navigation
- [ ] Design waypoint data structure (array of Vector3 per bird or shared path)
- [ ] Implement waypoint follower that calls `SetSteeringTarget()` sequentially
- [ ] Arrival detection (how close before advancing to next waypoint)
- [ ] Smooth "gliding around corners" — steering force produces natural arcs automatically
- [ ] Optional: integration with Unity Spline system for visual path editing
- [ ] Optional: per-waypoint speed/flap settings (e.g., "glide through this section")

### Phase 4: Integration & Performance
- [ ] Integrate bird system with existing MRAVctrl particle VFX
- [ ] Build to Oculus Quest — verify performance
- [ ] Mixed reality passthrough testing
- [ ] Live performance workflow testing with Ableton + controller

### Future Ideas
- [ ] Per-bird targeting (individual buttons control individual birds)
- [ ] Dynamic flock size (spawn/despawn birds via input)
- [ ] Bird-to-music reactivity (OSC audio data drives flight parameters)
- [ ] Predator-prey behaviors between bird groups
- [ ] Diving/Falling animations re-enabled with controlled trigger conditions
