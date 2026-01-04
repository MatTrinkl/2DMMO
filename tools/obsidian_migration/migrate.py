#!/usr/bin/env python3
"""
Build an Obsidian-compatible vault from docs/ into docs/Obsidian Vault/.

This script is intentionally minimal and focused on the current repository layout.
It copies markdown files into curated destinations, rewrites internal links,
adds source annotations, and produces migration artifacts.
"""

from __future__ import annotations

import csv
import os
import re
import shutil
from pathlib import Path, PurePosixPath

ROOT = Path(__file__).resolve().parents[2]
DOCS_ROOT = ROOT / "docs"
VAULT_ROOT = DOCS_ROOT / "Obsidian Vault"

BASE_DIRS = [
    "00-Home",
    "01-Planning",
    "01-Planning/Meetings",
    "01-Planning/Roadmap",
    "02-Game-Design",
    "02-Game-Design/Systems",
    "02-Game-Design/Content",
    "02-Game-Design/World",
    "04-Tech",
    "04-Tech/Architecture",
    "04-Tech/API",
    "04-Tech/Implementation-Notes",
    "04-Tech/Implementation-Notes/Testing",
    "05-Decisions",
    "99-Inbox",
    "Templates",
    "_assets",
]


def destination_for(rel_path: PurePosixPath) -> PurePosixPath:
    rel_str = rel_path.as_posix()
    name = rel_path.name

    if rel_str.startswith("01-overview/"):
        if name == "ASSETS.md":
            return PurePosixPath("02-Game-Design/Content") / name
        if name == "GAME_DESIGN_DOCUMENT.md":
            return PurePosixPath("02-Game-Design/Systems") / name
        if name == "PROTOTYPE_SCOPE.md":
            return PurePosixPath("01-Planning/Roadmap") / name
        if name == "README.md":
            return PurePosixPath("02-Game-Design/Systems/Overview-README.md")
        return PurePosixPath("02-Game-Design/Systems") / name

    if rel_str.startswith("02-architecture/"):
        if name == "README.md":
            return PurePosixPath("04-Tech/Architecture/Architecture-Overview.md")
        return PurePosixPath("04-Tech/Architecture") / name

    if rel_str.startswith("03-technical-details/"):
        if name == "README.md":
            return PurePosixPath("04-Tech/Architecture/Technical-Details-README.md")
        return PurePosixPath("04-Tech/Architecture") / name

    if rel_str.startswith("03-messages/"):
        if name == "README.md":
            return PurePosixPath("04-Tech/API/Message-Reference.md")
        return PurePosixPath("04-Tech/API") / name

    if rel_str.startswith("03-testing/"):
        return PurePosixPath("04-Tech/Implementation-Notes/Testing") / name

    if rel_str.startswith("04-project-management/"):
        if name == "FEATURE_ROADMAP.md":
            return PurePosixPath("01-Planning/Roadmap") / name
        if name == "README.md":
            return PurePosixPath("01-Planning/Project-Management-README.md")
        return PurePosixPath("01-Planning") / name

    if rel_str == "CONTENT_REVIEW_SUMMARY.md":
        return PurePosixPath("02-Game-Design/Content/CONTENT_REVIEW_SUMMARY.md")
    if rel_str == "MIGRATION_GUIDE.md":
        return PurePosixPath("04-Tech/Implementation-Notes/MIGRATION_GUIDE.md")
    if rel_str == "README.md":
        return PurePosixPath("00-Home/Docs-Overview.md")

    return PurePosixPath("99-Inbox") / name


LINK_PATTERN = re.compile(r"(!?\[[^\]]*?\])\(([^)]+)\)")


def remap_link(
    target: str,
    current_rel: PurePosixPath,
    dest_rel: PurePosixPath,
    mapping: dict[str, PurePosixPath],
    broken: list[dict],
) -> str:
    stripped = target.strip()
    if stripped.startswith(("http://", "https://", "mailto:")) or stripped.startswith("#"):
        return target

    anchor = ""
    if "#" in stripped:
        stripped, anchor = stripped.split("#", 1)

    if stripped.startswith("docs/"):
        candidate = PurePosixPath(stripped[5:])
    else:
        normalized = os.path.normpath((current_rel.parent / stripped).as_posix())
        candidate = PurePosixPath(normalized)

    candidate_str = candidate.as_posix().lstrip("./")

    if candidate.name.lower() == "architecture.md":
        target_new = PurePosixPath("04-Tech/Architecture/Architecture-Overview.md")
        rel_path = PurePosixPath(
            os.path.relpath(VAULT_ROOT / target_new, (VAULT_ROOT / dest_rel).parent)
        )
        new_target = rel_path.as_posix()
        if anchor:
            new_target += f"#{anchor}"
        return new_target
    if candidate_str in mapping:
        target_new = mapping[candidate_str]
        rel_path = PurePosixPath(
            os.path.relpath(VAULT_ROOT / target_new, (VAULT_ROOT / dest_rel).parent)
        )
        new_target = rel_path.as_posix()
        if anchor:
            new_target += f"#{anchor}"
        return new_target

    if any(candidate_str.endswith(suffix) for suffix in (".md", ".mdx")):
        broken.append(
            {
                "source": current_rel.as_posix(),
                "target": target,
            }
        )
    return target


def rewrite_content(
    content: str,
    current_rel: PurePosixPath,
    dest_rel: PurePosixPath,
    mapping: dict[str, PurePosixPath],
    broken: list[dict],
) -> str:
    lines = []
    for line in content.splitlines():
        def _replace(match: re.Match[str]) -> str:
            link_target = match.group(2)
            new_target = remap_link(link_target, current_rel, dest_rel, mapping, broken)
            return f"{match.group(1)}({new_target})"

        lines.append(LINK_PATTERN.sub(_replace, line))
    return "\n".join(lines)


def main() -> None:
    if VAULT_ROOT.exists():
        shutil.rmtree(VAULT_ROOT)
    VAULT_ROOT.mkdir(parents=True, exist_ok=True)
    for base in BASE_DIRS:
        (VAULT_ROOT / base).mkdir(parents=True, exist_ok=True)

    source_files = [
        PurePosixPath(p.relative_to(DOCS_ROOT).as_posix())
        for p in DOCS_ROOT.rglob("*.md")
        if "Obsidian Vault" not in p.parts
    ]

    mapping: dict[str, PurePosixPath] = {}
    for rel in source_files:
        mapping[rel.as_posix()] = destination_for(rel)

    broken_links: list[dict] = []
    migration_rows: list[tuple[str, str, str]] = []

    for rel in source_files:
        dest_rel = mapping[rel.as_posix()]
        src_path = DOCS_ROOT / rel
        dst_path = VAULT_ROOT / dest_rel
        dst_path.parent.mkdir(parents=True, exist_ok=True)

        content = src_path.read_text(encoding="utf-8")
        updated = rewrite_content(content, rel, dest_rel, mapping, broken_links)

        if not updated.endswith("\n"):
            updated += "\n"
        updated += "\nSource: docs/" + rel.as_posix() + "\n"

        dst_path.write_text(updated, encoding="utf-8")
        migration_rows.append((f"docs/{rel.as_posix()}", f"docs/Obsidian Vault/{dest_rel.as_posix()}", "copied"))

    create_templates()
    create_mocs(mapping)
    create_decisions_index()
    write_home()
    write_inbox()
    write_import_report(source_files, broken_links, migration_rows)
    write_migration_map(migration_rows)


def create_templates() -> None:
    templates = {
        "Templates/Feature-Spec.md": "\n".join(
            [
                "# Feature Spec",
                "- **Feature**:",
                "- **Owner**:",
                "- **Status**:",
                "",
                "## Goals",
                "- ",
                "",
                "## Non-Goals",
                "- ",
                "",
                "## User Stories",
                "- As a , I want , so that .",
                "",
                "## Dependencies",
                "- ",
                "",
                "## Acceptance Criteria",
                "- ",
                "",
                "## Risks / Open Questions",
                "- ",
                "",
            ]
        ),
        "Templates/System-Spec.md": "\n".join(
            [
                "# System Spec",
                "- **System**:",
                "- **Owner**:",
                "- **Status**:",
                "",
                "## Purpose",
                "",
                "## Rules / Constraints",
                "- ",
                "",
                "## Inputs",
                "- ",
                "",
                "## Outputs",
                "- ",
                "",
                "## Balancing / Tuning",
                "- ",
                "",
                "## Risks",
                "- ",
                "",
            ]
        ),
        "Templates/Meeting-Notes.md": "\n".join(
            [
                "# Meeting Notes",
                "- **Date**:",
                "- **Participants**:",
                "- **Topic**:",
                "",
                "## Agenda",
                "- ",
                "",
                "## Notes",
                "- ",
                "",
                "## Decisions",
                "- ",
                "",
                "## Action Items",
                "- [ ] Owner - Task",
                "",
            ]
        ),
        "Templates/ADR.md": "\n".join(
            [
                "# Architecture Decision Record",
                "- **Status**: draft | accepted | superseded",
                "- **Date**:",
                "",
                "## Context",
                "",
                "## Decision",
                "",
                "## Consequences",
                "",
                "## Alternatives Considered",
                "- ",
                "",
            ]
        ),
    }

    for rel, content in templates.items():
        path = VAULT_ROOT / rel
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(content + "\n", encoding="utf-8")


def collect_links_for_folder(
    mapping: dict[str, PurePosixPath], folder: str
) -> list[PurePosixPath]:
    result = []
    for dest in mapping.values():
        if PurePosixPath(folder) in dest.parents and dest.name.lower() != "home.md":
            result.append(dest)
    return sorted(result)


def create_mocs(mapping: dict[str, PurePosixPath]) -> None:
    planning_links = collect_links_for_folder(mapping, "01-Planning")
    tech_links = collect_links_for_folder(mapping, "04-Tech")
    design_links = collect_links_for_folder(mapping, "02-Game-Design")

    write_moc("01-Planning/Planning (MOC).md", "Planning (MOC)", planning_links)
    write_moc("04-Tech/Tech (MOC).md", "Tech (MOC)", tech_links)
    write_moc("02-Game-Design/Systems/Systems (MOC).md", "Systems (MOC)", design_links)


def write_moc(relative_path: str, title: str, links: list[PurePosixPath]) -> None:
    lines = [f"# {title}", ""]
    for link in links:
        display = link.name.removesuffix(".md")
        rel_path = PurePosixPath(os.path.relpath(VAULT_ROOT / link, (VAULT_ROOT / relative_path).parent))
        lines.append(f"- [{display}]({rel_path.as_posix()})")
    path = VAULT_ROOT / relative_path
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text("\n".join(lines) + "\n", encoding="utf-8")


def create_decisions_index() -> None:
    path = VAULT_ROOT / "05-Decisions/ADR Index.md"
    content = [
        "# ADR Index",
        "",
        "Collect architecture decisions here.",
    ]
    path.write_text("\n".join(content) + "\n", encoding="utf-8")


def write_home() -> None:
    home = [
        "# 2DMMO Knowledge Base",
        "",
        "Welcome to the Obsidian vault. Start with the maps of content and the inbox.",
        "",
        "## Quick Links",
        "- [Planning MOC](../01-Planning/Planning%20(MOC).md)",
        "- [Tech MOC](../04-Tech/Tech%20(MOC).md)",
        "- [Systems MOC](../02-Game-Design/Systems/Systems%20(MOC).md)",
        "- [Feature Roadmap](../01-Planning/Roadmap/FEATURE_ROADMAP.md)",
        "- [ADR Index](../05-Decisions/ADR%20Index.md)",
        "- [Import Report](../IMPORT-REPORT.md)",
        "- [Inbox](../99-Inbox/Inbox.md)",
        "",
        "## Usage",
        "- Prefer wikilinks or relative links within the vault.",
        "- Drop new ideas into the Inbox and triage regularly.",
    ]
    (VAULT_ROOT / "00-Home/Home.md").write_text("\n".join(home) + "\n", encoding="utf-8")


def write_inbox() -> None:
    inbox = [
        "# Inbox",
        "",
        "Use this space for quick notes before filing them into the proper MOC.",
    ]
    (VAULT_ROOT / "99-Inbox/Inbox.md").write_text("\n".join(inbox) + "\n", encoding="utf-8")


def write_import_report(
    source_files: list[PurePosixPath],
    broken_links: list[dict],
    migration_rows: list[tuple[str, str, str]],
) -> None:
    markdown_count = len(source_files)
    asset_count = 0

    structure = [
        "- 00-Home/",
        "- 01-Planning/ (Roadmap, issue processes)",
        "- 02-Game-Design/ (GDD, assets, content)",
        "- 04-Tech/ (Architecture, API, implementation notes)",
        "- 05-Decisions/",
        "- 99-Inbox/",
        "- Templates/",
        "- _assets/",
    ]

    report = [
        "# Import Report",
        "",
        "## Summary",
        f"- Markdown files imported: {markdown_count}",
        f"- Assets imported: {asset_count}",
        "",
        "## Structure",
        *structure,
        "",
        "## MOCs",
        "- Planning (MOC)",
        "- Tech (MOC)",
        "- Systems (MOC)",
        "",
        "## Broken or Unmapped Links",
    ]
    if broken_links:
        for item in broken_links:
            report.append(f"- {item['source']} -> {item['target']}")
    else:
        report.append("- None")

    report.extend(
        [
            "",
            "## Renames / Notes",
        ]
    )
    for old, new, note in migration_rows:
        if Path(old).name.lower() == "readme.md":
            report.append(f"- {old} => {new} ({note})")

    (VAULT_ROOT / "IMPORT-REPORT.md").write_text("\n".join(report) + "\n", encoding="utf-8")


def write_migration_map(rows: list[tuple[str, str, str]]) -> None:
    path = VAULT_ROOT / "migration-map.csv"
    with path.open("w", newline="", encoding="utf-8") as f:
        writer = csv.writer(f)
        writer.writerow(["old_path", "new_path", "notes"])
        writer.writerows(rows)


if __name__ == "__main__":
    main()
