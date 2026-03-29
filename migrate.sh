#!/usr/bin/env bash
# migrate.sh
#
# Usage:
#   ./migrate.sh                       — Blow away all migrations + SQL, recreate from scratch (InitDb)
#   ./migrate.sh "some migration name" — Add a new migration, generate incremental SQL file
#
# Spaces and hyphens in the migration name are converted to underscores.
# SQL files are written to ./db/sql/ with a 3-digit zero-padded prefix.

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DB_SQL_DIR="$SCRIPT_DIR/db/sql"
MIGRATIONS_DIR="$SCRIPT_DIR/src/Aethon.Data/Migrations"
DATA_PROJECT="$SCRIPT_DIR/src/Aethon.Data"
API_PROJECT="$SCRIPT_DIR/src/Aethon.Api"

# ── Colours ───────────────────────────────────────────────────────────────────
RED='\033[0;31m'; GREEN='\033[0;32m'; YELLOW='\033[1;33m'; CYAN='\033[0;36m'; NC='\033[0m'
info()    { echo -e "${CYAN}→${NC} $*"; }
success() { echo -e "${GREEN}✓${NC} $*"; }
warn()    { echo -e "${YELLOW}!${NC} $*"; }
err()     { echo -e "${RED}✗${NC} $*" >&2; exit 1; }

# ── Helpers ───────────────────────────────────────────────────────────────────

# Spaces and hyphens → underscores, lowercase
normalize_name() {
    echo "$1" | tr ' \t-' '_' | tr '[:upper:]' '[:lower:]' | tr -s '_'
}

# snake_case → PascalCase  (update_stripe_config → UpdateStripeConfig)
to_pascal_case() {
    echo "$1" | sed 's/_/ /g' \
              | awk '{for(i=1;i<=NF;i++) $i=toupper(substr($i,1,1)) substr($i,2)}1' \
              | tr -d ' '
}

# Return the next 3-digit zero-padded sequence number based on files in db/sql/
next_seq() {
    if [ ! -d "$DB_SQL_DIR" ] || [ -z "$(ls -A "$DB_SQL_DIR" 2>/dev/null)" ]; then
        echo "001"
        return
    fi
    local highest
    highest=$(ls "$DB_SQL_DIR"/*.sql 2>/dev/null \
        | xargs -I{} basename {} \
        | grep -oE '^[0-9]+' \
        | sort -n \
        | tail -1)
    if [ -z "$highest" ]; then
        echo "001"
    else
        printf "%03d" $((10#$highest + 1))
    fi
}

# Return the name of the latest EF migration currently in the Migrations directory.
# Excludes Designer files and the ModelSnapshot.
latest_migration_name() {
    ls "$MIGRATIONS_DIR"/*.cs 2>/dev/null \
        | grep -v '\.Designer\.cs$' \
        | grep -v 'Snapshot\.cs$' \
        | sort \
        | tail -1 \
        | xargs basename 2>/dev/null \
        | sed 's/^[0-9]*_//' \
        | sed 's/\.cs$//' \
    || true
}

# ── Init mode (no arguments) ──────────────────────────────────────────────────
if [ $# -eq 0 ]; then
    warn "Init mode — all existing migrations and SQL files will be deleted."
    read -r -p "Continue? [y/N] " confirm
    if [[ ! "$confirm" =~ ^[Yy]$ ]]; then
        echo "Aborted."
        exit 0
    fi

    info "Removing Migrations directory…"
    rm -rf "$MIGRATIONS_DIR"

    info "Removing db/sql directory…"
    rm -rf "$DB_SQL_DIR"

    info "Creating InitDb migration…"
    dotnet ef migrations add InitDb \
        --project "$DATA_PROJECT" \
        --startup-project "$API_PROJECT"

    info "Generating SQL → db/sql/001_init_db.sql"
    mkdir -p "$DB_SQL_DIR"
    dotnet ef migrations script \
        --idempotent \
        --project "$DATA_PROJECT" \
        --startup-project "$API_PROJECT" \
        -o "$DB_SQL_DIR/001_init_db.sql"

    success "Init complete → $DB_SQL_DIR/001_init_db.sql"
    exit 0
fi

# ── Add mode (migration name provided) ───────────────────────────────────────
RAW_NAME="$1"
SNAKE_NAME="$(normalize_name "$RAW_NAME")"
PASCAL_NAME="$(to_pascal_case "$SNAKE_NAME")"
SEQ="$(next_seq)"
SQL_FILENAME="${SEQ}_${SNAKE_NAME}_db.sql"
SQL_PATH="$DB_SQL_DIR/$SQL_FILENAME"

info "Migration name : $PASCAL_NAME"
info "SQL file       : $SQL_FILENAME"

# Capture the current latest migration BEFORE adding the new one
PREV_MIGRATION="$(latest_migration_name)"
if [ -n "$PREV_MIGRATION" ]; then
    info "Previous migration : $PREV_MIGRATION"
fi

info "Running: dotnet ef migrations add $PASCAL_NAME"
dotnet ef migrations add "$PASCAL_NAME" \
    --project "$DATA_PROJECT" \
    --startup-project "$API_PROJECT"

info "Generating incremental SQL…"
mkdir -p "$DB_SQL_DIR"

if [ -z "$PREV_MIGRATION" ]; then
    # No previous migration — generate full idempotent script
    dotnet ef migrations script \
        --idempotent \
        --project "$DATA_PROJECT" \
        --startup-project "$API_PROJECT" \
        -o "$SQL_PATH"
else
    # Generate only the SQL delta from previous migration to the new one
    dotnet ef migrations script "$PREV_MIGRATION" "$PASCAL_NAME" \
        --project "$DATA_PROJECT" \
        --startup-project "$API_PROJECT" \
        -o "$SQL_PATH"
fi

success "Migration created → $SQL_PATH"
