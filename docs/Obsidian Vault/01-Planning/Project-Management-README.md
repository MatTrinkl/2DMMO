# 📋 Project Management

This section contains **issue tracking, roadmaps and planning documents**.

---

## 📚 Documents in This Section

### [Issue Hierarchy](ISSUE_HIERARCHY.md)
Shows the relationships between issues and the recommended processing order:
- Phase 1: Fundamentals & Project Setup
- Phase 2: Basic Network & Auth Skeleton
- Phase 3: Gameplay Loop
- Phase 4: Testing & Optimization
- Dependencies between issues

**Target Audience**: Developers, project management

---

### [Issue Updates Guide](ISSUE_UPDATES_GUIDE.md)
**Consolidated:** ISSUE_UPDATES.md + ZONE_CONCEPT_UPDATES.md

Process guide for issue updates:
- When issues need to be updated
- Templates for epic issues and sub-issues
- Best practices for issue management
- Example workflow for architecture changes

**Target Audience**: Project management, all developers

---

### [Sub-Issues](SUB_ISSUES.md)
**Split from:** ISSUES_ROADMAP.md (Part 1)

Detailed sub-issue suggestions for existing large issues:
- Issue #8: NetworkServer → 3 sub-issues
- Issue #10: Login Flow → 3 sub-issues
- Issue #11: MessagePack → 3 sub-issues
- Further splits for better handling

**Target Audience**: Developers, project management

---

### [Feature Roadmap](Roadmap/FEATURE_ROADMAP.md)
**Split from:** ISSUES_ROADMAP.md (Part 2)

New feature issues for functionality through end of Phase 4:
- Phase 2: Connection establishment, disconnect handling
- Phase 3: Movement, collision, chat
- Phase 4: Zone transfer, NPCs, performance
- All structured as epic issues

**Target Audience**: Project management, product owner

---

## 🔗 Related Documentation

- **Architecture (Zone System)**: See [ID System](../04-Tech/Architecture/ID_SYSTEM.md)
- **Prototype Scope**: See [Prototype Scope](Roadmap/PROTOTYPE_SCOPE.md)

---

## 📊 Workflow

```
1. Identify new features
   ↓
2. Create issue (see FEATURE_ROADMAP.md)
   ↓
3. Check hierarchy (see ISSUE_HIERARCHY.md)
   ↓
4. Split large issues into sub-issues (see SUB_ISSUES.md)
   ↓
5. For architecture changes: use ISSUE_UPDATES_GUIDE.md
   ↓
6. Work on issues (by priority)
```

---

## 📝 Changelog

### Version 2.1.0 (2026-01-01)
- ✅ **Translated**: All documents to English

### Version 2.0.0 (2025-12-09)
- ✅ **Consolidated**: ISSUE_UPDATES.md + ZONE_CONCEPT_UPDATES.md → ISSUE_UPDATES_GUIDE.md
- ✅ **Split**: ISSUES_ROADMAP.md → SUB_ISSUES.md + FEATURE_ROADMAP.md
- ✨ Improved structure for better navigability
- 📖 Clearer categorization of documents

---

**Navigation**: [← Back to Main Documentation](../00-Home/Docs-Overview.md)

Source: docs/04-project-management/README.md
