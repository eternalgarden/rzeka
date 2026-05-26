# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/).

## [Unreleased]

## [1.1.0] - 2026-05-26

### Added
- Matter serialization tests.
- Eris: custom port setting.
- Eris: Filtering matter by who shaped/received it.
- Guards against user incorrect loom/shuttle spell registrations (breaking the required lambda structure by entirely ignoring the input matter)
- Eris: Log filtering badges.
- `Spring.Create` now requires a `mainThread` IScheduler. Conjuring spells (Strand, Loom, Shuttle) auto-`ObserveOn` it before publishing matter.
- `IRzeka` now exposes `MainThread` for post-await marshalling.

### Fixed
- Eris: Matter payload not serializing properly.
- Eris: Silent exceptions on matter types that contain unserializable properties.
- Eris: The HEAVY bug in occurences list that caused mind-boggling swaps of matter names, who shaped them, timestamp issues, all due to a recycle setting being on in FAST repeat element.

### Removed
- Broken github pages workflow.
- Forgotten leftovers from a currently suspended multi-river system implementation.
- `isOnMainThread` parameter from `Spring.Create` - thread ID is now auto-captured at .Create time.

### Changed
- Made the naming of some rzeka extensions more reasonable.
- Eris: she is now much prettier 💅🏻
- Eris: quality of life improvements (autoconnect, clear on new session id)

[Unreleased]: https://github.com/eternalgarden/rzeka/compare/v0.2.0...HEAD
[1.0.2]: https://github.com/eternalgarden/rzeka/compare/v1.0.1...v1.0.2
[1.0.1]: https://github.com/eternalgarden/rzeka/compare/v1.0.0...v1.0.1
