#!/usr/bin/env python3
import argparse
import json
from pathlib import Path
import sys


def load(path: Path):
    return json.loads(path.read_text(encoding="utf-8"))


def main():
    p = argparse.ArgumentParser()
    p.add_argument("--audit", required=True)
    p.add_argument("--integrity", required=True)
    args = p.parse_args()
    audit = load(Path(args.audit))
    integrity = load(Path(args.integrity))
    observed = {item["id"]: item for item in audit["fixtures"]}
    expected = integrity["fixtures"]
    if set(expected) - set(observed):
        raise RuntimeError(f"missing audited fixtures: {sorted(set(expected) - set(observed))}")
    for fixture_id, hashes in expected.items():
        item = observed[fixture_id]
        if item["git_blob_sha1_observed"] != hashes["git_blob_sha1"]:
            raise RuntimeError(f"{fixture_id}: Git blob SHA1 mismatch")
        if item["sha256_observed"] != hashes["sha256"]:
            raise RuntimeError(f"{fixture_id}: SHA-256 mismatch")
    print(f"STAGE03_DUAL_HASH_PASS fixtures={len(expected)}")


if __name__ == "__main__":
    try:
        main()
    except Exception as exc:
        print(f"STAGE03_DUAL_HASH_FAIL: {exc}", file=sys.stderr)
        raise
