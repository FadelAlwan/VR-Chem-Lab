# VirtuChem — A Virtual Chemistry Lab Trainer

A first-person 3D virtual chemistry lab built in Unity, where first-year chemistry students can safely practice lab procedures — safety gear, chemical mixing, and simple distillation — before entering a real lab.

> Built for **BER3023 Human-Computer Interaction** · Theme: **Smart Education**

---

## 🎥 Demo Video

[![Watch the demo](https://img.shields.io/badge/YouTube-Watch%20Demo-red?logo=youtube)](https://youtu.be/DBtwcW0dxFI)



---

## 📸 Screenshots

| Lab Overview | Safety Cabinet |
|---|---|
| ![Safety cabinet](screenshots/safety.png) |

| Chemical Mixing Station | Simple Distillation Station |
|---|---|
| ![Chemical Mixing Station](screenshots/Chemical_mixing.png) | ![Distillation Station](screenshots/distillation.png) |

---

## ✨ Features

- **First-person 3D lab room** — walk around a fully furnished chemistry lab using standard WASD + mouse controls.
- **Safety gear system** — lab coat, goggles, and gloves must be equipped before any risky action is allowed; missing items trigger a specific on-screen warning.
- **Chemical Mixing station** — pick up and pour matching chemical pairs (NaOH + HCl, FeCl3 + KSCN) into a beaker; incorrect pairs are rejected with real-time feedback.
- **Simple Distillation station** — light a spirit lamp, watch the temperature rise on a live thermometer, and see ethanol separate from water at 78°C.
- **Guided on-screen instructions** — step-by-step prompts that only advance once the correct action is completed.
- **Wall reference charts** — in-world posters showing valid reactions and the distillation process, so learners don't need to memorize procedures.
- **Seated interaction model** — each station's controls only respond while the learner is seated at that bench, and standing up resets the station for a retry.

---

## 🛠 Built With

- **Unity 6000.5.5f1** (Unity 6)
- **C#**

---

## 🚀 Getting Started

### Prerequisites

- [Unity Hub](https://unity.com/download)
- **Unity Editor 6000.5.5f1** (install via Unity Hub — this exact version is required for full compatibility)

### Setup

1. Clone this repository:
   ```bash
   git clone https://github.com/your-org/virtuchem.git
   ```
2. Open **Unity Hub** → **Add project from disk** → select the cloned folder.
3. Open the project (Unity Hub will prompt to install `6000.5.5f1` if not already installed).
4. In the **Project** window, open `Assets/Scenes/VirtuChem_MainScene`.
5. Press **Play** in the Unity Editor.

---

## 🎮 Controls

| Input | Action |
|---|---|
| `W` `A` `S` `D` | Move |
| Mouse | Look around |
| `E` | Interact (pick up / use / sit / stand from stations) |
| `Space` | Stand up (resets the current station) |
| `Esc` | Release the cursor |

---

## 📁 Project Structure

```
Assets/
├── Scenes/
│   └── VirtuChem_MainScene.unity
├── Scripts/
│   ├── SimpleFirstPersonController.cs   # Player movement & look
│   ├── PlayerInteraction.cs             # Raycast-based interaction system
│   ├── SafetyGearManager.cs             # Lab coat / goggles / gloves tracking
│   ├── SitInteractable.cs               # Seating, per-station reset, guided steps
│   ├── CupPourController.cs             # Pick-up-and-pour mixing interaction
│   ├── BeakerMixer.cs                   # Chemical reaction recipes & results
│   ├── SimpleDistillation.cs            # Heating & boiling-point separation logic
│   ├── HeatSource.cs                    # Spirit lamp ignition
│   ├── ProcedureManager.cs              # Guided on-screen instructions
│   └── ...
└── ...
```


---

## 📄 License

This project was created for educational purposes as part of a university course assignment.
