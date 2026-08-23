# AGENTS.md

Project: Poor Man's Football — a Unity football simulation/game.

Architecture:
- Assets/Scenes = playable Unity scenes
- Assets/Scripts = small reusable runtime C# components
- Assets/Tests/EditMode = deterministic editor tests
- Assets/Tests/PlayMode = runtime behavior tests when needed
- Packages = Unity package manifests and lock files
- ProjectSettings = Unity project configuration

Rules:
- This repository is intentionally skeletal. For the first implementation, create the Unity project structure and required files from scratch; missing starting assets are not a blocker.
- use only Unity primitives and project-generated code unless a ticket explicitly authorizes external assets
- keep gameplay behavior in small reusable components, not monolithic scene scripts
- keep the football as an independent Rigidbody; never permanently attach it to a player
- preserve Unity .meta files and never add Library/, Temp/, Logs/, Obj/, Builds/, or UserSettings/
- do not weaken project verification or modify poorman.yaml from a worker attempt
- do not commit; PMC owns Git state

Canonical verification:
- use commands in poorman.yaml when configured
- Unity compilation, EditMode tests, PlayMode tests, and visual/play-feel gates will be enabled when a Unity editor runner is registered
