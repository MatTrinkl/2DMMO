# 📚 2DMMO Documentation

**Welcome to the central documentation of the 2DMMO project!**

This documentation is modular and divided into thematic areas. Each area contains specific documents on its topic.

---

## 🗂️ Documentation Structure

### 01 - Overview & Vision
> **High-level project information, gameplay design and asset requirements**

| Document | Description |
|----------|-------------|
| [Game Design Document](../02-Game-Design/Systems/GAME_DESIGN_DOCUMENT.md) | Gameplay vision, races, classes, world design, progression |
| [Prototype Scope](../01-Planning/Roadmap/PROTOTYPE_SCOPE.md) | Definition of prototype scope and MVP features |
| [Assets & Resources](../02-Game-Design/Content/ASSETS.md) | Asset requirements, sources and specifications |

### 02 - Architecture
> **Technical system architecture, network design and infrastructure**

| Document | Description |
|----------|-------------|
| [Architecture Overview](../04-Tech/Architecture/Architecture-Overview.md) | Main architecture, tech stack, system overview |
| [Server Components](../04-Tech/Architecture/SERVER_COMPONENTS.md) | Gateway, Zone Server, communication |
| [Network Protocol](../04-Tech/Architecture/NETWORK_PROTOCOL.md) | Transport, message framing, connection flow |
| [Message Specification](../04-Tech/Architecture/MESSAGES.md) | Message types, DTOs, serialization |
| [Game Loop Design](../04-Tech/Architecture/GAME_LOOP.md) | Server game loop, tick timing |
| [Client-Server Sync](../04-Tech/Architecture/CLIENT_SERVER_SYNC.md) | Prediction, interpolation, reconciliation |
| [Chunk-Based Sync](../04-Tech/Architecture/CHUNK_BASED_SYNC.md) | **Phase 2** - AOI delta sync, chunk grid, bandwidth optimization |
| [ID System](../04-Tech/Architecture/ID_SYSTEM.md) | Entity identity, GlobalKey, ZoneId ranges |
| [Zone Data Architecture](../04-Tech/Architecture/ZONE_DATA_ARCHITECTURE.md) | ZoneBounds, CollisionData, data structures |
| [Redis Strategy](../04-Tech/Architecture/REDIS.md) | Key schema, caching, Pub/Sub |
| [Database Strategy](../04-Tech/Architecture/DATABASE.md) | PostgreSQL, write strategies, pooling |
| [Security](../04-Tech/Architecture/SECURITY.md) | Security layers, input validation |
| [Scaling](../04-Tech/Architecture/SCALING.md) | Zone sharding, metrics, auto-scaling |
| [Azure Deployment](../04-Tech/Architecture/AZURE_DEPLOYMENT.md) | Container Apps, networking, services |

### 03 - Messages & Technical Details
> **Message reference and detailed implementation decisions**

| Document | Description |
|----------|-------------|
| **[📨 Message Reference](../04-Tech/API/Message-Reference.md)** | **Complete documentation of all 1100+ network messages** |
| [Technical Design Document](../04-Tech/Architecture/TECHNICAL_DESIGN.md) | Detailed technical decisions, thread model, collision system |

### 04 - Project Management
> **Issue tracking, roadmaps and planning documents**

| Document | Description |
|----------|-------------|
| [Issue Hierarchy](../01-Planning/ISSUE_HIERARCHY.md) | Issue relationships and processing order |
| [Issue Updates Guide](../01-Planning/ISSUE_UPDATES_GUIDE.md) | Process guide for issue updates (templates, best practices) |
| [Sub-Issues](../01-Planning/SUB_ISSUES.md) | Detailed sub-issue suggestions for large issues |
| [Feature Roadmap](../01-Planning/Roadmap/FEATURE_ROADMAP.md) | New features through end of Phase 4 |

---

## 🎯 Quick Start

### For New Developers
1. Start with the [Game Design Document](../02-Game-Design/Systems/GAME_DESIGN_DOCUMENT.md) for the vision
2. Read the [Architecture Overview](../04-Tech/Architecture/Architecture-Overview.md) for technical understanding
3. Check the [Prototype Scope](../01-Planning/Roadmap/PROTOTYPE_SCOPE.md) for current development status

### For Existing Developers
- **Architecture Questions**: See [02-architecture/](02-architecture/)
- **Implementation Details**: See [Technical Design](../04-Tech/Architecture/TECHNICAL_DESIGN.md)
- **Issue Planning**: See [04-project-management/](04-project-management/)

---

## 📝 Documentation Conventions

### Structure
- All documents use **Markdown** (.md)
- Emojis in titles for better orientation (e.g. 🏗️, 🎮, 📡)
- Version number and update date in header
- Detailed table of contents with anchor links

### Versioning
- **Version**: Semantic Versioning (e.g. 1.2.0)
- **Status**: `In Development` | `Prototype Phase` | `Finalized`
- **Last Updated**: YYYY-MM-DD format

### Cross-References
- Use relative paths: `[Link](../04-Tech/Architecture/Architecture-Overview.md)`
- Anchor links for sections: `[Link](#section-name)`

---

## 🔄 Extending Documentation

### Adding New Documents

1. **Choose category**: Decide which folder the document belongs in
   - `01-overview`: High-level, non-technical information
   - `02-architecture`: System design, architecture decisions
   - `03-technical-details`: Implementation specifications
   - `04-project-management`: Issue tracking, planning

2. **Create document**: Follow the conventions (see above)

3. **Update index**: Add an entry in this README.md

4. **Update links**: Check and update all internal links

### Updating Existing Documents

1. Increment version number (for significant changes)
2. Update the date
3. Maintain changelog at end of document (optional)

---

## 📊 Documentation Dependencies

```
GAME_DESIGN_DOCUMENT (Vision)
    ↓
ARCHITECTURE (System Design)
    ↓
TECHNICAL_DESIGN (Implementation)
    ↓
PROTOTYPE_SCOPE (Current MVP)
    ↓
ISSUE_HIERARCHY (Task Breakdown)
```

---

## 🆘 Help & Feedback

- **Documentation questions**: Create an issue with label `documentation`
- **Missing documentation**: Create an issue describing the needed content
- **Improvement suggestions**: Create a pull request or issue

---

**Last Updated**: 2026-01-01  
**Structure Version**: 2.1.0

Source: docs/README.md
