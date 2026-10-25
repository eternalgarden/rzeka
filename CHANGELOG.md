# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/).

Checklist:
- Move things from unreleased to the new release section.
- On GitHub after releasing:
  - Paste in the contents of the changelog for this release.
  - (if it's not there already) Add a link to the new full changelog between this tag's commit and one belonging to the last release.
## [Unreleased]
### Added
### Removed
### Fixed
### Changed

## [1.2.1] – 2026-10-25
### Fixed
- `WithCircumstances` no longer accepts `null` silently only to crash later: a `null` array threw straight away, and a `null` element was stored and blew up in `IsCircumstancedBy` or Eris. Nulls are now skipped.
- Fixes to WithCircumstances

### Changed
- `WithCircumstances` with nothing to stamp (no arguments, or only `null`s) returns the matter untouched instead of a clone with an empty list, so an absent optional cause can no longer wipe an existing stamp. Clearing circumstances this way is no longer possible; nothing in rzeka relied on it.

## [1.2.0] – 2026-10-02
### Added
- A Horror latch on shuttle emission of a reponse without a request reference set on it.
- Error boundary Horrors now carry the matter the spell was handling as circumstances (a Loom's latest inputs, a Shuttle's latest request, a Weave's delivered matter).
- Weave error boundary: a throwing Weave is whispered to Eris as a Horror and contained. `onUnhandledSourceError` runs for Weave failures too.
- EternalGarden.Rzeka.Reporting: opt-in crash reporting that sends the causal story of a failing spell to an HTTP dnepoint. Structure only, no matter payload values due to privacy reasons).
### Removed
### Fixed
- 🔥 A throwing Weave no longer silently stops its matter type from reaching every other subscriber for the rest of the session. This was a huge hidden bug discovered while working on rzeka-reporting.
### Changed
- Error boundary messages describe the owner by type name (plus `describeOwner`) instead of the owner's `ToString()`.

## [1.1.3] - 2026-08-16

### Fixed
- Ask extension method now completes after a single response emission.

### Changed
- Renamed .Reacting extension method to .Perform so that it fits the mental model better. Fortunately no one but me uses rzeka yet.

## [1.1.2] - 2026-08-12

### Added
- Made Pluck be automatically marshalled back to the main thread just as all other api methods.

### Fixed
- Debugger spam loop on unused input matter types in api lambdas.

## [1.1.1] - 2026-06-26

### Added
- Automatic Eris horror logging for unserializable matter passing into rzeka.

### Removed
- Unused Wispd spell occurence category.

### Fixed
- IsCircumstancedBy() extension potential infinite loop. Oof.

### Changed
- Little cute display of the connected river name in Eris.

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

[1.2.1]: https://github.com/eternalgarden/rzeka/compare/v1.2.0...v1.2.1
[1.1.3]: https://github.com/eternalgarden/rzeka/compare/v1.1.2...v1.1.3
[1.1.2]: https://github.com/eternalgarden/rzeka/compare/v1.1.1...v1.1.2
[1.1.1]: https://github.com/eternalgarden/rzeka/compare/v1.1.0...v1.1.1
[1.1.0]: https://github.com/eternalgarden/rzeka/compare/v1.0.2...v1.1.0
[1.0.2]: https://github.com/eternalgarden/rzeka/compare/v1.0.1...v1.0.2
[1.0.1]: https://github.com/eternalgarden/rzeka/compare/v1.0.0...v1.0.1
