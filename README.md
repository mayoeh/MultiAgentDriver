# MultiAgentDriver
Multi Agent Driver for World Building with AR/VR

## Dependencies:
- Unity Hub
- Unity Editor
- Logitech SDK
- Photon Fusion SDK
- Visual Studio Code (scripting)
- ParrelSync (optional, for testing of multiple players with one device)

## How to get the project:

### Option 1: Clone with Git
- Run:
 git clone https://github.com/mayoeh/MultiAgentDriver.git
- Open Unity Hub
- Click "Add Project" and select the cloned folder

### Option 2: Download ZIP
- Click "Code" → "Download ZIP" on GitHub or download the provided ZIP in the Canvas submission
- Extract the folder
- Open Unity Hub
- Click "Add Project" and select the extracted folder

## Setup (Photon Fusion):
- Ensure that the Photon App ID is set to: 11b5e4e8-031c-40c2-8464-33480b54a6c6
- To do so, access:
Tools → Photon Hub → Insert App ID

## Build instructions:
- Go to File → Build Settings
- Select the scene "no-ai-driving-environment"
- Choose your platform (PC recommended)
- Click "Build and Run"

## How to run:
- Open the project in Unity Hub
- Wait for all imports to be completed
- Open the scene:
 Assets/Scenes/no-ai-driving-environment.unity
- Press the Play button to start the simulation; the first player to start the scene will be the host with the VR headset and steering wheel

## Multiplayer testing:
- You can run one instance in the Unity Editor and another as a built application
- Make sure both use the same Photon App ID
- Alternatively, you may use ParrelSync to open two Unity Editors on one device

## Notes:
- Make sure the scene "no-ai-driving-environment" is the one being used for testing
- Ensure Photon App ID is set correctly or multiplayer will not work
