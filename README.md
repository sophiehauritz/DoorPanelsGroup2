# Collaborative Door Panel Assembly with Cobot Support

This project implements a collaborative human-robot assembly workflow for large door panels using a Universal Robots (UR) cobot. The cobot performs the physically demanding tasks of lifting, rotating, positioning, and stabilizing the door, while the human operator performs precision installation tasks such as hinge mounting and quality checks.

The system is implemented as a minimum viable product (MVP) with emphasis on ergonomics, operator control, and traceability rather than full automation.


## Features:

- **Collaborative assembly workflow** with human-in-the-loop control
- **GUI-based operator interface** built with Avalonia (.NET)
- **Robot communication** via TCP (Dashboard + URScript ports)
- **User authentication system** with account management
- **Local SQLite database**
  - User login
  - Order start/finish logging (timestamps + operator)
- **Door size selection**
  - Small / Medium / Large → maps to grip width + force
- **Operator-driven motion control**
  - Brake release / connect
  - Move to work table
  - Move to storage
  - Stop program
  - Finish order
  - Logout
- **Safety considerations**
  - Operator confirmation before robot motion
  - Emergency stop command via GUI
  - Login prevents unauthorized use

## System Architecture

The system consists of:

- **Universal Robots cobot** (UR series)
- **Gripper system** for flat-panel handling
- **Avalonia GUI** for operator interaction
- **SQLite database** for login + traceability
- **TCP communication layer**
  - Dashboard (29999)
  - URScript (30002)

The operator remains in charge of alignment and quality, while the cobot handles lifting, positioning, and stabilization.

## Demonstration Videos

- Automated cycle test: *https://youtu.be/CoLUI1Jd4c8*
- Collaborative assembly test: *https://youtu.be/NFPV_3pfhms*

## MVP Scope

This project focuses on:

- Ergonomics
- Human-robot collaboration
- Operator control
- Traceability

