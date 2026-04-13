# Multiplayer Chat – Unity

Project corresponding to Practical Work 1 of the course  
**Multiplayer Video Game Programming with Unity**

---

## 📌 Description

Real-time messaging application developed in Unity, allowing communication between multiple clients through a server connection.

The system implements a client-server architecture where messages are sent to the server and redistributed to the rest of the connected clients.

---

## 🎯 Main Features

- Protocol support:
  - TCP
  - UDP
- Dynamic connection configuration
- User identification
- Message reply system
- Visual differentiation between own and other users' messages
- Chat-style interface with scrollable history
- Message sending via button or Enter key
- UI adaptation for Desktop and Mobile

---

## ⚙️ Requirements

- Unity **6000.0.36f1**
- Network connection (local or Internet)

---

## 🚀 Project Execution

1. Open the project from Unity Hub
2. Make sure to use version: 6000.0.36f1
3. Open the scene: ConnectionScene
4. Press Play or build the project

---

## 🌐 Connection Configuration

When starting the application, the following options are presented:

- **Execution Mode:**
- Client
- Client-Server

- **IP Address:**
- `127.0.0.1` → for local testing
- Public IP → for external connections

- **Port:**
- Example: `7777`

- **Network Protocol:**
- TCP
- UDP

---

## 💬 Chat Usage

Once the connection is established:

1. Type a message in the text field
2. Press:
- **Send** button, or
- **Enter** key
3. The message will be sent to the server and appear in the history

### Message System Features

- Each message displays the sender's name
- Messages are organized in a chat-style format
- Visual differentiation:
- Own messages → aligned to the right
- Other users' messages → aligned to the left
- Ability to reply to previous messages
- Display of the referenced message being replied to

---

## 📱 Compatibility

The system adapts the interface depending on the platform:

- **Desktop:** horizontal layout
- **Mobile:** vertical layout

---

## 🖥️ Server

The server can be executed in two ways:

- As a client-server within the same build
- As an independent server in headless mode

---

## 🌍 Connection Across Different Networks

The system supports connection via public IP.

For external testing:

- Use the host's public IP
- Configure port forwarding on the router
- Ensure the port is open

---

## 🧱 Project Structure

The project is organized as follows:

- Decoupled transport system (TCP / UDP)
- Centralized connection management
- Modular UI based on prefabs
- Message system with reply support
- Separation between network logic and presentation

---

## 👤 Author

Matias Pulido
