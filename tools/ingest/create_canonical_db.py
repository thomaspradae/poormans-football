#!/usr/bin/env python3
"""Create the versioned Football World Lab canonical SQLite database."""
from pathlib import Path
import sqlite3

ROOT = Path(__file__).resolve().parents[2]
DB = ROOT / "data" / "canonical" / "football_world.sqlite"

SCHEMA = """
PRAGMA foreign_keys = ON;
CREATE TABLE IF NOT EXISTS schema_meta (key TEXT PRIMARY KEY, value TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS sources (
  source_id TEXT PRIMARY KEY, name TEXT NOT NULL, url TEXT, license TEXT,
  retrieved_at TEXT NOT NULL, checksum TEXT, notes TEXT
);
CREATE TABLE IF NOT EXISTS countries (
  country_id TEXT PRIMARY KEY, name TEXT NOT NULL, iso2 TEXT, iso3 TEXT,
  source_id TEXT REFERENCES sources(source_id)
);
CREATE TABLE IF NOT EXISTS competitions (
  competition_id TEXT PRIMARY KEY, name TEXT NOT NULL, country_id TEXT REFERENCES countries(country_id),
  level INTEGER, gender TEXT NOT NULL DEFAULT 'men', source_id TEXT REFERENCES sources(source_id)
);
CREATE TABLE IF NOT EXISTS clubs (
  club_id TEXT PRIMARY KEY, canonical_name TEXT NOT NULL, country_id TEXT REFERENCES countries(country_id),
  city TEXT, founded_year INTEGER, wikidata_id TEXT, source_id TEXT REFERENCES sources(source_id)
);
CREATE TABLE IF NOT EXISTS club_aliases (
  club_id TEXT NOT NULL REFERENCES clubs(club_id), alias TEXT NOT NULL, source_id TEXT REFERENCES sources(source_id),
  PRIMARY KEY (club_id, alias)
);
CREATE TABLE IF NOT EXISTS matches (
  match_id TEXT PRIMARY KEY, competition_id TEXT REFERENCES competitions(competition_id),
  season TEXT, played_on TEXT NOT NULL, home_club_id TEXT NOT NULL REFERENCES clubs(club_id),
  away_club_id TEXT NOT NULL REFERENCES clubs(club_id), home_score INTEGER NOT NULL,
  away_score INTEGER NOT NULL, source_id TEXT NOT NULL REFERENCES sources(source_id), source_match_key TEXT,
  CHECK(home_score >= 0 AND away_score >= 0), CHECK(home_club_id <> away_club_id)
);
CREATE UNIQUE INDEX IF NOT EXISTS uq_match_source ON matches(source_id, source_match_key);
CREATE TABLE IF NOT EXISTS provenance (
  provenance_id INTEGER PRIMARY KEY, entity_type TEXT NOT NULL, entity_id TEXT NOT NULL,
  field_name TEXT, source_id TEXT REFERENCES sources(source_id), source_value TEXT,
  confidence REAL NOT NULL DEFAULT 1.0, observed_at TEXT, transformation TEXT
);
CREATE TABLE IF NOT EXISTS club_finances (
  club_id TEXT NOT NULL REFERENCES clubs(club_id), metric TEXT NOT NULL, value REAL NOT NULL,
  currency TEXT, fiscal_year INTEGER, source_type TEXT NOT NULL,
  source_id TEXT REFERENCES sources(source_id), confidence REAL NOT NULL DEFAULT 1.0,
  model_version TEXT, PRIMARY KEY (club_id, metric, fiscal_year, source_type)
);
CREATE TABLE IF NOT EXISTS database_runs (
  run_id TEXT PRIMARY KEY, started_at TEXT NOT NULL, tool_version TEXT NOT NULL,
  source_manifest TEXT, rows_imported INTEGER NOT NULL DEFAULT 0, notes TEXT
);
"""

def main() -> None:
    DB.parent.mkdir(parents=True, exist_ok=True)
    with sqlite3.connect(DB) as conn:
        conn.executescript(SCHEMA)
        conn.execute("INSERT OR REPLACE INTO schema_meta VALUES (?, ?)", ("schema_version", "1"))
        conn.execute("INSERT OR REPLACE INTO schema_meta VALUES (?, ?)", ("canonical_database", "football_world.sqlite"))
    print(DB)

if __name__ == "__main__":
    main()
