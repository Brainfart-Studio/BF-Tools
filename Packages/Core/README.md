# BFTools Core (`com.bftools.core`)

Foundational systems for BFTools: bootstrapping and event communication.

## Version
0.2.0

## Contents

### Bootstrapper
Global (app-lifetime) and Level (per-scene) system initialization. See [Documentation~/Bootstrapper.md](Documentation~/Bootstrapper.md).

### Event Bus
Generic static pub/sub system for struct-based events. See [Documentation~/EventBus.md](Documentation~/EventBus.md).

## Dependencies
None.

## Installation
Add via Unity Package Manager as a local/git package, or reference `com.bftools.core` from a dependent package's `package.json`.