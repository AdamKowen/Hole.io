# Hole.io-style 2D Game — The Academic College of Tel-Aviv-Yaffo

---

## 🎮 Overview

A 2D Unity project inspired by Hole.io. The player controls a growing hole that moves through a recreated scene of The Academic College of Tel-Aviv-Yaffo, swallowing objects around the campus to increase size and score. The goal is to maximize points within a 3-minute session. The game includes a start menu, score tracking, growth system, and fully working object pooling for smooth performance.
**Gameplay Loop:**

> Move → Swallow smaller objects → Earn points → Grow → Swallow larger objects → Repeat until timer ends.

**Highlights:**

* Fully playable, stable, and optimized.
* Dynamic level-based growth and object swallowing.
* Floating score text for visual feedback.
* Modular and event-driven architecture.
* Clear separation between managers, systems, and gameplay logic.
* Optimized colliders for smoother runtime performance.

---

## 🧩 Architecture Overview

### **Singleton Framework (`Singleton<T>.cs`)**

Generic singleton base class ensures only one instance of each manager (e.g. `UIManager`, `ScoreManager`, `GameManager`). Automatically creates a new instance if none exist.

---

## 🕹️ Game Flow & Managers

### **GameManager.cs**

Controls game start, menu, and game over states. Links camera and hole references.

* Displays start menu.
* Starts or restarts the game.
* Freezes hole and shows Game Over panel.

### **UIManager.cs**

Manages UI elements: timer, score display, and local high scores. Uses PlayerPrefs to save top 5 scores.

### **ScoreManager.cs**

Handles all score logic. Triggers `OnScoreChanged` events for UI updates.

---

## 🕳️ Hole & Swallowing System

### **HoleController.cs**

Core gameplay class controlling movement, swallowing, growth, and floating text feedback.

* Swallows objects tagged with `Swallowable` based on `requiredLevel`.
* Animates swallowing via coroutine.
* Awards points using `LevelPointsTable`.
* Displays floating text with font scaling based on hole size.

### **Swallowable.cs**

Marks an object as consumable. Stores its level requirement and `Collider2D` reference for interaction.

### **LevelPointsTable.cs**

ScriptableObject mapping object level → score reward.

---

## 💬 Visual Feedback

### **FloatingText.cs**

Displays dynamic "+score" text using `ObjectPool`. Smooth cubic easing, fades out, and follows the hole transform.

---

## 🌍 World, Movement, and Paths

### **PathFollower.cs**

Moves objects back and forth along waypoints.

### **PathLapSwitcher.cs**

Loops over waypoints in laps, pauses at the start, and switches sprites between laps.

---

## 👥 Spawning & Pooling

### **ObjectPool.cs**

Implements efficient object reuse. Pre-instantiates or dynamically expands pool based on configuration.

### **NPCSpawner.cs**

Spawns pooled NPCs or props periodically using a linked `ObjectPool`.

---

## 🎥 Camera System

### **CameraFollowZoom2D.cs**

Smoothly follows the hole and adjusts orthographic zoom based on scale. Includes world-bound clamping and menu mode.

---

## 🗺️ Map & Tools

### **AssembleTiledMap.cs** *(Editor Tool)*

Automates tilemap creation from image tiles (named `tile_rX_cY`). Used for large map assembly during development. The game map was created using this system, assembling multiple high-resolution tiles into a single playable area.

### **MinimapSimpleURP.cs** *(Not used in final build)*

Functional minimap camera. Removed from the shipped game for simplicity but remains fully operational. To make the minimap visible, select the MinimapCamera object, set a negative Z position, and enable it in the scene.

---

## ⚙️ Level Progression & Scoring Flow

```
HoleController → ScoreManager.AddPoints() → UIManager.UpdateScore()
UIManager.Timer → GameManager.GameOver() → Freeze Hole
```

---

## 🧩 Editor & Utilities

* **AssembleTiledMap** — Automated map tile creation for development.
* **Collider Optimizer** — Integrated collider simplification tool for smoother runtime physics.

  * Based on: [PolygonColliderSimplification by j-bbr](https://github.com/j-bbr/PolygonColliderSimplification)

---

## 📈 Performance Notes

* Optimized physics and collision shapes using external simplification.
* Frame rate capped (`Application.targetFrameRate = 120`).
* Object pooling reduces instantiation overhead.
* The game world map was built from multiple high-resolution tiles using the *AssembleTiledMap* tool to optimize loading and rendering performance.

---

## 🧾 Credits

Developed by [**Adam Kowen**](https://github.com/AdamKowen) & [**Alon Bilman**](https://github.com/AlonBilman) — The Academic College of Tel Aviv-Yaffo.
Art, logic, and gameplay systems created in Unity 2022+.
Collider optimization by [PolygonColliderSimplification by j-bbr](https://github.com/j-bbr/PolygonColliderSimplification).
TextMeshPro © Unity Technologies.
