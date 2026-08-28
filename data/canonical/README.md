# Canonical Football World Database

`football_world.sqlite` is the canonical structured database for Football World
Lab. Raw downloads never go directly into simulation: every imported value is
linked to a `sources` row and important derived values are recorded in
`provenance`.

The database is reproducible. Recreate the empty schema with:

```bash
python3 tools/ingest/create_canonical_db.py
```

Observed match results, derived calibration, and synthetic simulation values
must remain distinct. Bulk analytical exports should use Parquet; SQLite is the
authoritative identity/provenance store.
