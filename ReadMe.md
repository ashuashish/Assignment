# **Unity Interaction Control & Lip-Sync System**

## **Architecture Overview**

The system is split into **independent components** with a central lock to prevent conflicts.

`UI Button`  
  `↓`  
`InteractionGate (global lock)`  
  `↓`  
`Reaction Sequence  OR  Dialogue / Lip-Sync`

This ensures **no overlapping animations or audio**.

---

## **InteractionGate (Global Lock)** 

* Prevents button spamming

* Only one interaction runs at a time

* Used by both Reaction and Dialogue systems

---

## **Reaction Sequence**

* Plays **Smile → Sad → Smile → Sad**

* Uses **Animator triggers** for body gestures

* Uses **index-based blendshapes** for facial expressions

* Controlled via coroutines for clean sequencing

---

## **Lip-Sync System**

* Split into **Controller \+ Engine**

* Generates a phoneme timeline from text

* Drives blendshapes in real time

* Syncs audio, mouth movement, and talking animation

---

## **Talking Animation**

* Animator bool: `Talking`

* `true` when dialogue starts

* `false` when dialogue ends

* Prevents animation jitter and missed transitions

---

### **Eye Blink**

* Auto-blinks at random intervals for realism

* Lightweight coroutine (no per-frame overhead)

* Index-based blendshapes for performance

* Can be triggered manually (events / timeline / reactions)

---

## **Performance Optimizations**

* Cached blendshape indices (no string lookup)

* Cursor-based phoneme playback (no full scans)

* Minimal per-frame updates

