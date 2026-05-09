# Changelog

All notable changes to this project will be documented in this file.

## [1.2.0] - 2026-05-09
### Added
- **Keyboard Control:** Implemented `KeyboardController` using smooth polling (`DispatcherTimer`) for WASD and Arrow keys.
- **Input Mode Manager:** New `InputMode` enum to handle seamless switching between Keyboard, Gamepad, and None.
- **Deadzones:** Added configurable deadzones for Gamepad sticks to prevent hardware drift.
- **Right Stick Mapping:** Added support for Right Stick Y-axis for precise acceleration/braking on Xbox controllers.

### Changed
- **COM Port Discovery:** Migrated from `Win32_SerialPort` to `Win32_PnPEntity` WMI queries. This significantly reduces UI lag and prevents app freezing when Bluetooth COM ports are present.
- **Architecture Refactoring:** Optimized `MainController` and `SettingsController` using C# auto-properties and cleaner JSON serialization logic.

### Fixed
- **Bluetooth Connection Lag:** Implemented strict filtering for "BTHENUM" devices during serial port enumeration.
- **Settings Stability:** Added robust `try-catch` blocks and directory verification for JSON save/load operations.
- **Button Toggle Logic:** Fixed a bug where F1/F2 states would flip multiple times per single press.

## [1.1.0] - 2024-06-15
- **Initial Diploma Release:** Core WPF interface, Serial communication, and basic Gamepad support.